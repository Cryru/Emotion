#region Using

using System.Collections.Concurrent;
using System.Threading;
using Emotion.Common.Threading;
using Emotion.Platform;

#endregion

namespace Emotion.Audio
{
	public abstract class AudioContext
	{
		protected List<AudioLayer> _layerMapping = new();
		protected ConcurrentBag<AudioLayer> _toRemove = new();
		protected ConcurrentBag<AudioLayer> _toAdd = new();

		protected volatile bool _running;
		protected Thread _audioThread;

		/// <summary>
		/// The rate at which audio layers are updated. This is also the amount of time
		/// worth in audio samples that will be buffered on each tick (roughly).
		/// Buffered date will be roughly this much times 2 ahead (depending on how much the buffer consumes),
		/// which is essentially the audio lag.
		/// </summary>
		public static int AudioUpdateRate = 25;

		/// <summary>
		/// How many ms the backend buffer is expected to be. This number should be 100ms+ to prevent audio flickering.
		/// </summary>
		public static int BackendBufferExpectedAhead = AudioUpdateRate * 4;

		private PlatformBase _host;

		protected AudioContext(PlatformBase platform)
		{
			_host = platform;
			_running = true;

			AudioConverter.SetResamplerQuality(Engine.Configuration.AudioQuality);
			if (Environment.ProcessorCount == 1) BackendBufferExpectedAhead *= 2;

#if !WEB
			_audioThread = new Thread(AudioLayerProc)
			{
				IsBackground = true,
			};
			_audioThread.Start();
#endif
		}

		public virtual void AudioLayerProc()
		{
			if (_host?.NamedThreads ?? false) Thread.CurrentThread.Name ??= "Audio Thread";

			var audioTimeTracker = Stopwatch.StartNew();
			var audioProcessEvent = new AutoResetEvent(false);

			var layers = new List<AudioLayer>();
			while (_running)
			{
				int timeToSleep = AudioUpdateRate - (int) audioTimeTracker.ElapsedMilliseconds;
				if (timeToSleep > 0) audioProcessEvent.WaitOne(timeToSleep);

				// Pause sound if host is paused.
				if (Engine.Host != null && Engine.Host.HostPaused)
				{
					if (GLThread.IsGLThread()) return; // Don't stop main thread.
					Engine.Host.HostPausedWaiter.WaitOne();
				}

				if (!_running) break;

				var timePassed = (int) audioTimeTracker.ElapsedMilliseconds;
				audioTimeTracker.Restart();

				// Handle changes
				while (!_toAdd.IsEmpty && _toAdd.TryTake(out AudioLayer layer))
					layers.Add(layer);

				while (!_toRemove.IsEmpty && _toRemove.TryTake(out AudioLayer layer))
				{
					layers.Remove(layer);
					layer.Stop();
					layer.Dispose();
				}

				for (var i = 0; i < layers.Count; i++)
				{
					layers[i].Update(timePassed);
				}
			}
		}

		public AudioLayer CreateLayer(string layerName, float layerVolume = 1)
		{
			layerName = layerName.ToLower();
			AudioLayer newLayer = CreatePlatformAudioLayer(layerName);
			newLayer.VolumeModifier = layerVolume;

			_toAdd.Add(newLayer);
			_layerMapping.Add(newLayer);

			Engine.Log.Info($"Created audio layer {newLayer.Name}", MessageSource.Audio);
			return newLayer;
		}

		public abstract AudioLayer CreatePlatformAudioLayer(string layerName);

		public void RemoveLayer(string layerName)
		{
			layerName = layerName.ToLower();
			AudioLayer layer = GetLayer(layerName);
			if (layer == null) return;

			_toRemove.Add(layer);
			_layerMapping.Remove(layer);

			Engine.Log.Info($"Removed audio layer {layer.Name}", MessageSource.Audio);
		}

		public AudioLayer GetLayer(string layerName)
		{
			layerName = layerName.ToLower();
			for (var i = 0; i < _layerMapping.Count; i++)
			{
				AudioLayer layer = _layerMapping[i];
				if (layer.Name == layerName) return layer;
			}

			return null;
		}

		public string[] GetLayers()
		{
			int entries = _layerMapping.Count;
			var names = new string[entries];
			for (var i = 0; i < entries; i++)
			{
				names[i] = _layerMapping[i].Name;
			}

			return names;
		}

		public virtual void Dispose()
		{
			_running = false;
		}
	}
}