#region Using

using System.Runtime.CompilerServices;
using Emotion.Standard.Audio;
using Emotion.Utility;

#endregion

#nullable enable

namespace Emotion.Audio
{
	public abstract partial class AudioLayer
	{
		// The interval in seconds to apply volume modulation in.
		// Calculating it for every frame is expensive, so we chunk it.
		// If this is too small it's a huge performance hit,
		// If it is too big though, it will cause discontinuity.
		public const float VOLUME_MODULATION_INTERVAL = 0.05f;

		public enum EffectPosition
		{
			TrackRelative,
			Absolute
		}

		// Track modulations include fade ins and fade outs.
		// They are applied at the volume modulation interval.
		private class VolumeModulationEffect
		{
			public EffectPosition Pos;

			public float _startVolume;
			public float _endVolume;

			public int _startSample;
			public int _endSample;

			public VolumeModulationEffect(float startVol, float endVol, int startSample, int endSample, EffectPosition pos)
			{
				_startVolume = startVol;
				_endVolume = endVol;

				_startSample = startSample;
				_endSample = endSample;

				Pos = pos;
			}

			public float GetVolumeAt(int sample)
			{
				if (sample < _startSample) return _startVolume;
				if (sample > _endSample) return _endVolume;

				float duration = _endSample - _startSample;
				float progress = (sample - _startSample) / duration;

				// Apply cubic in a way that is always in the out direction.
				if (_startVolume > _endVolume)
				{
					progress = 1.0f - progress;
					progress *= progress;
					progress = 1.0f - progress;
				}
				else
				{
					progress *= progress;
				}

				return Maths.FastLerp(_startVolume, _endVolume, progress);
			}

			public void FormatChanged(AudioTrack currentTrack, AudioFormat old, AudioFormat neu)
			{
				float totalSamplesWere = currentTrack.File.AudioConverter.GetSampleCountInFormat(old);
				float totalSamplesNow = currentTrack.File.AudioConverter.GetSampleCountInFormat(neu);

				float progressStartSample = _startSample / totalSamplesWere;
				_startSample = (int) MathF.Floor(progressStartSample * totalSamplesNow);

				float progressEndSample = _endSample / totalSamplesWere;
				_endSample = (int) MathF.Floor(progressEndSample * totalSamplesNow);
			}
		}

		private VolumeModulationEffect? _fadeInVol;
		private VolumeModulationEffect? _fadeOutVol;
		private VolumeModulationEffect? _userModifier;

		/// <summary>
		/// Get the volume applied by SetVolume. This will update over time.
		/// </summary>
		public float AppliedVolume
		{
			get
			{
				if (_userModifier == null) return 1f;
				return _userModifier.GetVolumeAt(_playHead);
			}
		}

		/// <summary>
		/// Set the volume for the layer over a period, or instant (if 0)
		/// This doesn't change the layer's volume modifier, or global volume, but is multiplicative to it.
		/// </summary>
		/// <param name="volumeGoal"></param>
		/// <param name="ms"></param>
		public void SetVolume(float volumeGoal, int ms)
		{
			float startVol = AppliedVolume;
			int volumeChangeSamples = _streamingFormat.GetFrameCount(ms / 1000f) * _streamingFormat.Channels;
			_userModifier = new VolumeModulationEffect(startVol, volumeGoal, _playHead, _playHead + volumeChangeSamples, EffectPosition.Absolute);

			//float startVol = GetVolume();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float GetVolume(int atSample)
		{
			var mod = 1f;
			if (_fadeInVol != null) mod *= _fadeInVol.GetVolumeAt(atSample);
			if (_fadeOutVol != null) mod *= _fadeOutVol.GetVolumeAt(atSample);
			if (_userModifier != null) mod *= _userModifier.GetVolumeAt(atSample);
			return mod;
		}

		private void FormatChangedRecalculateFx(AudioTrack track, AudioFormat oldFormat, AudioFormat newFormat)
		{
			// Track based
			_fadeInVol?.FormatChanged(track, oldFormat, newFormat);
			_fadeOutVol?.FormatChanged(track, oldFormat, newFormat);
			_userModifier?.FormatChanged(track, oldFormat, newFormat);
		}

		private void TrackChangedFx(AudioTrack? currentTrack, int prevTrackPlayHead)
		{
			_fadeInVol = null;
			_fadeOutVol = null;

			if (currentTrack != null && currentTrack.FadeIn.HasValue && (!currentTrack.FadeInOnlyFirstLoop || _loopCount == 0))
			{
				AudioFormat format = _streamingFormat;

				// Calculate fade in frames.
				float val = currentTrack.FadeIn.Value;
				if (val < 0) val = currentTrack.File.Duration * -val;
				int fadeInSamples = format.GetFrameCount(val) * format.Channels;

				_fadeInVol = new VolumeModulationEffect(0f, 1f, 0, fadeInSamples, EffectPosition.TrackRelative);
			}

			if (currentTrack != null && currentTrack.FadeOut.HasValue)
			{
				AudioFormat format = _streamingFormat;

				// Calculate fade in frames.
				float val = currentTrack.FadeOut.Value;
				if (val < 0) val = currentTrack.File.Duration * -val;
				val = currentTrack.File.Duration - val;
				int fadeOutSampleStart = format.GetFrameCount(val) * format.Channels;

				_fadeOutVol = new VolumeModulationEffect(1f, 0f, fadeOutSampleStart, _totalSamplesConv, EffectPosition.TrackRelative);
			}

			// Transition "absolute" positions fx.
			if (_userModifier != null)
			{
				int samplesLeft = _userModifier._endSample - prevTrackPlayHead;
				if (samplesLeft > 0)
				{
					_userModifier._startVolume = _userModifier.GetVolumeAt(prevTrackPlayHead);
					_userModifier._startSample = 0;
					_userModifier._endSample = samplesLeft;
				}
				else
				{
					_userModifier._startSample = 0;
					_userModifier._endSample = _totalSamplesConv;
					_userModifier._startVolume = _userModifier._endVolume;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private float ApplyVolumeToFrames(int sampleStart, Span<float> frames, int framesInBuffer, float baseMod, float lastAppliedBaseMod)
		{
			int channels = _streamingFormat.Channels;
			int samplesInBuffer = framesInBuffer * channels;

			int finalSample = sampleStart + samplesInBuffer;
			int intervalInSamples = _streamingFormat.GetFrameCount(VOLUME_MODULATION_INTERVAL) * channels;

			var startingInterval = (int) MathF.Floor((float) sampleStart / intervalInSamples);
			var endingInterval = (int) MathF.Ceiling((float) finalSample / intervalInSamples);

			int firstZeroCrossIdx = -1;

			// If changing base mod, find a zero crossing on all channels at once,
			// to ensure no click is heard.
			if (baseMod != lastAppliedBaseMod)
			{
				Span<float> lastSampleValues = stackalloc float[channels];

				var crossZeroSampleIdx = 0;
				for (int i = startingInterval; i < endingInterval; i++)
				{
					int sampleAmount = Math.Min(intervalInSamples, samplesInBuffer - crossZeroSampleIdx);
					int sampleAmountStart = crossZeroSampleIdx;
					int sampleEnd = sampleAmountStart + sampleAmount;

					// Initialize last sample values.
					if (i == startingInterval)
						for (var c = 0; c < channels; c++)
						{
							lastSampleValues[c] = frames[sampleAmountStart + c];
						}

					for (int s = sampleAmountStart + channels; s < sampleEnd; s += channels)
					{
						var isZeroCrossing = true;
						for (var c = 0; c < channels; c++)
						{
							float thisSample = frames[s + c];
							float lastSample = lastSampleValues[c];

							// Where the sound sine wave crosses 0 (-1 to 1 or vice versa) you
							// can safely apply gain without causing clicks.
							bool zeroCrossingOnThisChannel = thisSample * lastSample <= 0;
							isZeroCrossing = isZeroCrossing && zeroCrossingOnThisChannel;
							lastSampleValues[c] = thisSample;
						}

						if (isZeroCrossing)
						{
							firstZeroCrossIdx = s;
							break;
						}
					}

					if (firstZeroCrossIdx != -1) break;
					crossZeroSampleIdx += sampleAmount;
				}
			}

			var sampleIdx = 0;
			for (int i = startingInterval; i < endingInterval; i++)
			{
				int sampleAmount = Math.Min(intervalInSamples, samplesInBuffer - sampleIdx);
				int sampleAmountStart = sampleIdx;
				int sampleEnd = sampleAmountStart + sampleAmount;

				int sampleIdxOfInterval = i * intervalInSamples;
				float intervalMod = GetVolume(sampleIdxOfInterval);
				float combinedMod = AudioUtil.VolumeToMultiplier(lastAppliedBaseMod * intervalMod);

				// Apply old gain up to crossing and then switch.
				if (firstZeroCrossIdx != -1 && firstZeroCrossIdx >= sampleAmountStart && firstZeroCrossIdx < sampleEnd)
				{
					for (int s = sampleAmountStart; s < firstZeroCrossIdx; s++)
					{
						frames[s] *= combinedMod;
					}

					lastAppliedBaseMod = baseMod;
					combinedMod = AudioUtil.VolumeToMultiplier(lastAppliedBaseMod * intervalMod);
					sampleAmountStart = firstZeroCrossIdx;
				}

				// Apply volume to samples of interval.
				for (int s = sampleAmountStart; s < sampleEnd; s++)
				{
					frames[s] *= combinedMod;
				}

				_debugLastAppliedSampleMod = combinedMod;
				sampleIdx += sampleAmount;
			}

			return lastAppliedBaseMod;
		}

		private float _debugLastAppliedSampleMod;

		/// <summary>
		/// Get the volume at the current playhead position.
		/// Use for debugging and visualization.
		/// </summary>
		public float DebugGetCurrentVolume()
		{
			return MathF.Round(_debugLastAppliedSampleMod, 2);
		}

		public void DebugSetPlaybackAtSecond(float second)
		{
			_playHead = _streamingFormat.GetFrameCount(second) * _streamingFormat.Channels;
		}

		#region API

		private void CrossFadeIntoNext()
		{
		}

		private void StartFadeOut()
		{
		}

		private void StartFadeIn(bool setVolumeZero, float seconds)
		{
			//if (setVolumeZero) ;
		}

		#endregion
	}
}