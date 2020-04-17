#region Using

using System;
using System.Numerics;
using Emotion.Audio;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.Primitives;

#endregion

namespace Emotion.Tools.Windows.Audio
{
    public class WaveformCache : IRenderable
    {
        public AudioLayer Layer;
        public AudioTrack Track;

        private Vector2[] _cache;
        private float _cacheWidth;
        private float _cacheHeight;

        public WaveformCache(AudioLayer layer)
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
            if(Track == null) return;
            Track.GetNextVolumeModulatedFrames(Layer.Volume * Engine.Configuration.MasterVolume, 0, Span<byte>.Empty);

            float interval = Track.File.Duration / _cacheWidth;
            var sampleCount = (int) (1f / interval);
            _cache = new Vector2[sampleCount];
            for (var i = 0; i < sampleCount; i++)
            {
                float location = i == sampleCount - 1 ? 1 : i * interval;
                float sample = Track.GetSampleAsFloat((int) ((Track.SourceSamples - 1) * location));
                _cache[i] = new Vector2(_cacheWidth * location, _cacheHeight * ((1.0f + sample) / 2f));
            }
        }

        public void Render(RenderComposer c)
        {
            if (Track == null || _cache == null) return;

            for (var i = 1; i < _cache.Length; i++)
            {
                c.RenderLine(_cache[i - 1], _cache[i], Color.Red);
            }

            float progressLine = _cacheWidth * Track.Progress;
            c.RenderLine(new Vector2(progressLine, 0), new Vector2(progressLine, _cacheHeight), Color.Yellow);
        }

        public void Clear()
        {
            Track = null;
        }
    }
}