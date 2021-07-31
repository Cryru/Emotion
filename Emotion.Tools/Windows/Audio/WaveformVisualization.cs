#region Using

using System;
using System.Numerics;
using Emotion.Audio;
using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.Standard.Audio;

#endregion

namespace Emotion.Tools.Windows.Audio
{
    public class WaveformVisualization : IRenderable
    {
        public AudioLayer Layer;
        public AudioTrack Track;

        private Vector2[] _cache;
        private float _cacheWidth;
        private float _cacheHeight;

        public WaveformVisualization(AudioLayer layer)
        {
            Layer = layer;
        }

        public void Create(AudioTrack track, float width, float height)
        {
            Track = track;
            _cacheWidth = width;
            _cacheHeight = height;
            Recreate();
        }

        public void Recreate()
        {
            if (Track == null) return;

            int frames = Track.File.SoundData.Length / Track.File.Format.FrameSize;
            int frameInterval = (int) (frames / _cacheWidth);
            int numFrames = frames / frameInterval;
            int frameIntervalVisually = (int) MathF.Round(_cacheWidth / numFrames);
            _cache = new Vector2[numFrames];

            for (int i = 0; i < numFrames; i++)
            {
                int sampleIndex = i * frameInterval * Track.File.Format.Channels;
                float firstChannelSample = AudioConverter.GetSampleAsFloat(sampleIndex, Track.File.SoundData.Span, Track.File.Format);
                _cache[i] = new Vector2(frameIntervalVisually * i, _cacheHeight * ((1.0f + firstChannelSample) / 2f));
            }
        }

        public void Render(RenderComposer c)
        {
            if (Track == null || _cache == null) return;

            c.RenderSprite(new Rectangle(0, 0, _cacheWidth, _cacheHeight), new Color(74, 74, 96));

            for (var i = 1; i < _cache.Length; i++)
            {
                c.RenderLine(_cache[i - 1], _cache[i], Color.Red);
            }

            float progressLine = _cacheWidth * Layer.Progress;
            c.RenderLine(new Vector2(progressLine, 0), new Vector2(progressLine, _cacheHeight), Color.Yellow);
        }

        public void Clear()
        {
            Track = null;
        }
    }
}