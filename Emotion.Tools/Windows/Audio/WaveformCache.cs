#region Using

using System.Numerics;
using Emotion.Audio;
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

            float interval = track.File.Duration / width;
            var sampleCount = (int) (1f / interval);
            _cache = new Vector2[sampleCount];
            for (var i = 0; i < sampleCount; i++)
            {
                float location = i == sampleCount - 1 ? 1 : i * interval;
                float sample = track.GetSampleAsFloat((int) (track.SourceSamples * location));
                _cache[i] = new Vector2(width * location, height * ((1.0f + sample) / 2f));
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