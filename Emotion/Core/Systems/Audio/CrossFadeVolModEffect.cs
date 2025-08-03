#nullable enable

using Emotion;

#nullable enable

using Emotion.Core.Systems.Audio;

namespace Emotion.Core.Systems.Audio;

public class CrossFadeVolModEffect : VolumeModulationEffect
{
    public AudioTrack? Track;
    public int PlayHead;
    public bool Looping;

    public void SetCrossFadeProps(AudioTrack track, int playHead, bool looping = false)
    {
        Track = track;
        PlayHead = playHead;
        EndSample = EndSample - StartSample;
        StartSample = 0;
        StartVolume = 0;
        EndVolume = 1f;
        Looping = looping;
    }

    public CrossFadeVolModEffect(float startVol, float endVol, int startSample, int endSample, EffectPosition pos) : base(startVol, endVol, startSample, endSample, pos)
    {
    }

    public override void FormatChanged(AudioTrack currentTrack, AudioFormat old, AudioFormat neu)
    {
        base.FormatChanged(currentTrack, old, neu);

        float totalSamplesWere = Track.File.AudioConverter.GetSampleCountInFormat(old);
        float totalSamplesNow = Track.File.AudioConverter.GetSampleCountInFormat(neu);

        float progressFadeTrackPlayhead = PlayHead / totalSamplesWere;
        PlayHead = (int) MathF.Floor(progressFadeTrackPlayhead * totalSamplesNow);
    }
}