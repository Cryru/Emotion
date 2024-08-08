#region Using

using System;
using Emotion.Audio;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Plugins.ImGuiNet.Windowing;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows.Audio
{
    public class AudioTrackModifyModal : ImGuiModal
    {
        private AudioAsset _asset;
        private bool _fadeIn;
        private float _fadeInTimestamp;
        private bool _fadeOut;
        private float _fadeOutTimestamp;
        private bool _crossFade;
        private float _crossFadeTimestamp;

        private Action<AudioTrack> _trackCallback;

        public AudioTrackModifyModal(AudioAsset asset, Action<AudioTrack> trackCallback) : base("Audio Track Settings")
        {
            _asset = asset;
            _trackCallback = trackCallback;
        }

        public override void Update()
        {
        }

        protected override void RenderContent(RenderComposer composer)
        {
            ImGui.Checkbox("FadeIn", ref _fadeIn);
            if (_fadeIn)
            {
                ImGui.SameLine();
                ImGui.DragFloat("FadeIn Duration", ref _fadeInTimestamp, 0.5f, 0.0f, _asset.Duration);
            }

            ImGui.Checkbox("FadeOut", ref _fadeOut);
            if (_fadeOut)
            {
                ImGui.SameLine();
                ImGui.DragFloat("FadeOut Duration", ref _fadeOutTimestamp, 0.5f, 0.0f, _asset.Duration);
            }

            ImGui.Checkbox("CrossFade", ref _crossFade);
            if (_crossFade)
            {
                ImGui.SameLine();
                ImGui.DragFloat("CrossFade Duration", ref _crossFadeTimestamp, 0.5f, 0.0f, _asset.Duration);
            }

            if (ImGui.Button("Ok"))
            {
                _trackCallback(GetTrack());
                Open = false;
            }
        }

        private AudioTrack GetTrack()
        {
            var track = new AudioTrack(_asset);

            if (_fadeIn) track.FadeIn = _fadeInTimestamp;
            if (_fadeOut) track.FadeOut = _fadeOutTimestamp;
            if (_crossFade) track.CrossFade = _crossFadeTimestamp;

            return track;
        }
    }
}