#region Using

using System;
using System.Diagnostics;
using System.Numerics;
using Emotion.Audio;
using Emotion.Common;
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

        private Vector2[] _cacheVolume;

        private Stopwatch _volumeCacheUpdate;

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
            RecreateVolume();
            _volumeCacheUpdate = Stopwatch.StartNew();
        }

        public void Recreate()
        {
            if (Track == null) return;

            float scale = 2f;
            float w = _cacheWidth / scale;

            int frames = Track.File.SoundData.Length / Track.File.Format.FrameSize;
            int frameInterval = (int) (frames / w);
            int numFrames = frames / frameInterval;
            int frameIntervalVisually = (int) MathF.Round(w / numFrames);
            _cache = new Vector2[numFrames];

            for (var i = 0; i < numFrames; i++)
            {
                int sampleIndex = i * frameInterval / Track.File.Format.Channels;
                float firstChannelSample = Track.File.SoundData[sampleIndex];
                _cache[i] = new Vector2(frameIntervalVisually * i * scale, _cacheHeight * ((1.0f - firstChannelSample) / 2f));
            }
        }

        public void RecreateVolume()
        {
            if (Track == null) return;

            AudioFormat layerFormat = Layer.CurrentStreamingFormat;
            int intervalInFrames = layerFormat.GetFrameCount(AudioLayer.VOLUME_MODULATION_INTERVAL);
            int frames = Track.File.AudioConverter.GetSampleCountInFormat(layerFormat) / layerFormat.Channels;
            int frameIntervals = frames / intervalInFrames;
            _cacheVolume = new Vector2[frameIntervals];

            float baseModifier = Layer.VolumeModifier * Engine.Configuration.MasterVolume;

            float frameIntervalVisually = _cacheWidth / frameIntervals;
            for (var i = 0; i < frameIntervals; i++)
            {
                int frameIndex = i * intervalInFrames;
                float intervalVolume = Layer.GetVolume(frameIndex * layerFormat.Channels);
                intervalVolume *= baseModifier;
                intervalVolume = AudioLayer.VolumeToMultiplier(intervalVolume);
                _cacheVolume[i] = new Vector2(frameIntervalVisually * i, _cacheHeight * ((2.0f - intervalVolume) / 2f));
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

            // Check if recaching volume.
            if (_volumeCacheUpdate.ElapsedMilliseconds > 1000)
            {
                RecreateVolume();
                _volumeCacheUpdate.Restart();
            }

            for (var i = 1; i < _cacheVolume.Length; i++)
            {
                c.RenderLine(_cacheVolume[i - 1], _cacheVolume[i], Color.White * 0.55f);
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