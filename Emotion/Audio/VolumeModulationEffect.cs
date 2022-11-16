#region Using

using Emotion.Standard.Audio;
using Emotion.Utility;

#endregion

namespace Emotion.Audio
{
	public enum EffectPosition
	{
		TrackRelative,
		Absolute
	}

	// Track modulations include fade ins and fade outs.
	// They are applied at the volume modulation interval.
	public class VolumeModulationEffect
	{
		public EffectPosition Pos;

		public float StartVolume;
		public float EndVolume;

		public int StartSample;
		public int EndSample;

		public VolumeModulationEffect(float startVol, float endVol, int startSample, int endSample, EffectPosition pos)
		{
			StartVolume = startVol;
			EndVolume = endVol;

			StartSample = startSample;
			EndSample = endSample;

			Pos = pos;
		}

		public float GetVolumeAt(int sample)
		{
			if (sample < StartSample) return StartVolume;
			if (sample > EndSample) return EndVolume;

			float duration = EndSample - StartSample;
			float progress = (sample - StartSample) / duration;

			// Apply cubic in a way that is always in the out direction.
			if (StartVolume > EndVolume)
			{
				progress = 1.0f - progress;
				progress *= progress;
				progress = 1.0f - progress;
			}
			else
			{
				progress *= progress;
			}

			return Maths.FastLerp(StartVolume, EndVolume, progress);
		}

		public virtual void FormatChanged(AudioTrack currentTrack, AudioFormat old, AudioFormat neu)
		{
			float totalSamplesWere = currentTrack.File.AudioConverter.GetSampleCountInFormat(old);
			float totalSamplesNow = currentTrack.File.AudioConverter.GetSampleCountInFormat(neu);

			float progressStartSample = StartSample / totalSamplesWere;
			StartSample = (int) MathF.Floor(progressStartSample * totalSamplesNow);

			float progressEndSample = EndSample / totalSamplesWere;
			EndSample = (int) MathF.Floor(progressEndSample * totalSamplesNow);
		}
	}
}