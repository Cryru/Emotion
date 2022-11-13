#region Using

using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Emotion.Standard.Audio;
using Emotion.Utility;

#endregion

namespace Emotion.Audio
{
	public abstract partial class AudioLayer
	{
		private float _baseVolumeLastApplied;

		protected int GetProcessedFramesFromTrack(AudioFormat format, AudioTrack track, int frames, float[] memory, ref int playhead, bool applyFading = true)
		{
			// Record where these frames are gotten from.
			int channels = format.Channels;
			int startingSample = playhead;

			// Get data.
			int framesOutput = track.File.AudioConverter.GetResamplesFrames(format, playhead, frames, memory);
			playhead += framesOutput * channels;
			Debug.Assert(framesOutput <= frames);

			// Force mono post process.
			// Dont apply force mono on tracks while the resampler applied mono to.
			bool mergeChannels = Engine.Configuration.ForceMono && channels != 1 && track.File.Format.Channels != 1;
			if (mergeChannels) PostProcessConvertToMono(framesOutput, memory, channels);

			// Apply fading, if needed. For instance we don't want additional fading while crossfading.
			if (!applyFading) return framesOutput;

			float baseVolume = VolumeModifier * Engine.Configuration.MasterVolume;
			float lastApplied = ApplyVolumeToFrames(startingSample, memory, framesOutput, baseVolume, _baseVolumeLastApplied);
			_baseVolumeLastApplied = lastApplied; // Record the base volume applied, as it is possible for the new one to not have applied in a single tick.

			//// If cross fading check if there is another track afterward.
			//var modulatedUpToFrame = 0;

			//if (_currentCrossFade != null)
			//{
			//    modulatedUpToFrame = PostProcessCrossFade(format, _currentCrossFade, startingFrame, framesOutput, memory);
			//    if (modulatedUpToFrame == 0) _currentCrossFade = null;
			//}

			//// Apply fading. If the current track doesn't have a crossfade active.
			//if (modulatedUpToFrame == 0)
			//    modulatedUpToFrame = PostProcessApplyFading(format, track, startingFrame, framesOutput, channels, memory);

			//// Apply base volume modulation to the rest of the samples.
			//float baseVolume = VolumeModifier * Engine.Configuration.MasterVolume;
			//baseVolume = VolumeToMultiplier(baseVolume);
			//for (int i = modulatedUpToFrame * format.Channels; i < samples; i++)
			//{
			//    memory[i] *= baseVolume;
			//}

			return framesOutput;
		}

		/// <summary>
		/// Convert multi-channel samples to mono.
		/// </summary>
		private static void PostProcessConvertToMono(int framesOutput, float[] soundData, int srcChannels)
		{
			// Fast path for stereo to mono.
			if (srcChannels == 2)
			{
				for (var i = 0; i < framesOutput; i++)
				{
					int sampleLeft = i * srcChannels;
					int sampleRight = i * srcChannels + 1;

					float leftData = soundData[sampleLeft];
					float rightData = soundData[sampleRight];
					float combined = (leftData + rightData);
					soundData[sampleLeft] = combined;
					soundData[sampleRight] = combined;
				}

				return;
			}

			for (var i = 0; i < framesOutput; i++)
			{
				int sampleIdx = i * srcChannels;
				float sampleAccum = 0;
				for (var c = 0; c < srcChannels; c++)
				{
					float sample = soundData[sampleIdx + c];
					sampleAccum += sample;
				}

				for (var c = 0; c < srcChannels; c++)
				{
					soundData[sampleIdx + c] = sampleAccum;
				}
			}
		}

		/// <summary>
		/// Apply cross fading between two tracks.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int PostProcessCrossFade(AudioFormat format, FadeEffectData crossFadeData, int startingFrame, int frameCount, float[] soundData)
		{
			// Assuming we crossfade from the start of the new track, the duration is all we need.
			var crossFadeDurationFrames = (int) MathF.Floor(crossFadeData.DurationSeconds * format.SampleRate);
			if (crossFadeDurationFrames <= startingFrame) return 0;

			// Clamp to wherever the crossfade will finish.
			if (startingFrame + frameCount > crossFadeDurationFrames) frameCount = crossFadeDurationFrames - startingFrame;
			Debug.Assert(frameCount > 0);

			// Get data from the track we were fading from.
			frameCount = GetProcessedFramesFromTrack(format, crossFadeData.Track, frameCount, _internalBufferCrossFade, ref crossFadeData.PlayHead, false);

			// Apply fade in increments.
			int channels = format.Channels;
			float baseVolume = VolumeModifier * Engine.Configuration.MasterVolume;
			var localFrame = 0;
			while (localFrame < frameCount)
			{
				int frames = Math.Min(VOLUME_MODULATION_FRAME_GRANULARITY, frameCount - localFrame);
				int frameIdx = startingFrame + localFrame;
				float frameT = (float) frameIdx / crossFadeDurationFrames;
				frameT = Maths.Clamp01(frameT);
				frameT = frameT * frameT; // Cubic

				float volumeFadingInto = frameT;
				float volumeFadingFrom = 1.0f - frameT;

				volumeFadingInto = AudioUtil.VolumeToMultiplier(baseVolume * volumeFadingInto);
				volumeFadingFrom = AudioUtil.VolumeToMultiplier(baseVolume * volumeFadingFrom);

				for (var i = 0; i < frames; i++)
				{
					int frameInDataIdx = (localFrame + i) * channels;
					for (var c = 0; c < channels; c++)
					{
						int sampleIdx = frameInDataIdx + c;
						float sampleCurrentTrack = soundData[sampleIdx] * volumeFadingInto;
						float sampleNextTrack = _internalBufferCrossFade[sampleIdx] * volumeFadingFrom;
						soundData[sampleIdx] = sampleCurrentTrack + sampleNextTrack;
					}
				}

				localFrame += frames;
			}

			return localFrame;
		}
	}
}