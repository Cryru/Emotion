#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Emotion.Audio;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Tools.Windows.HelpWindows;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows.Audio
{
    public class AudioMixer : ImGuiWindow
    {
        private FileExplorer<AudioAsset> _explorer;
        private string _newLayerName = "New Layer";
        private int _waveFormHeight = 200;

        private Dictionary<AudioLayer, WaveformCache> _waveFormCache = new Dictionary<AudioLayer, WaveformCache>();

        public AudioMixer() : base("Audio Mixer")
        {
        }

        protected override void RenderContent(RenderComposer composer)
        {
            if (_explorer != null && _explorer.Open)
            {
                ImGui.Text("Waiting for file...");
                return;
            }

            void ExecuteOnFile(Action<AudioTrack> func)
            {
                _explorer = new FileExplorer<AudioAsset>(asset =>
                {
                    var modifyModal = new AudioTrackModifyModal(asset, func);
                    Parent.AddWindow(modifyModal);
                }, true);
                Parent.AddWindow(_explorer);
            }

            float masterVol = Engine.Configuration.MasterVolume;
            if (ImGui.DragFloat("Global Volume", ref masterVol, 0.01f, 0f, 1f))
            {
                Engine.Configuration.MasterVolume = masterVol;
                foreach (KeyValuePair<AudioLayer, WaveformCache> cache in _waveFormCache)
                {
                    cache.Value.Recreate();
                }
            }

            bool mono = Engine.Configuration.ForceMono;
            if (ImGui.Checkbox("Force Mono", ref mono))
            {
                Engine.Configuration.ForceMono = mono;
            }

            // Push waveforms down.
            composer.PushModelMatrix(Matrix4x4.CreateTranslation(new Vector3(0, _waveFormHeight, 0)));

            // Render ImGui section of layers.
            string[] layers = Engine.Audio.GetLayers();
            for (var i = 0; i < layers.Length; i++)
            {
                AudioLayer layer = Engine.Audio.GetLayer(layers[i]);
                _waveFormCache.TryGetValue(layer, out WaveformCache cache);

                ImGui.Text($"Layer {layers[i]}");

                ImGui.PushID(i);
                ImGui.Text($"Status: {layer.Status}" +
                           (layer.CurrentTrack != null ? $" {MathF.Truncate(layer.CurrentTrack.Playback * 100f) / 100f:0}/{layer.CurrentTrack.File.Duration}" : ""));
                float volume = layer.Volume;
                if (ImGui.DragFloat("Volume", ref volume, 0.01f, 0f, 1f))
                {
                    cache?.Recreate();
                    layer.Volume = volume;
                }

                if (ImGui.Button("Add To Queue"))
                    ExecuteOnFile(layer.AddToQueue);
                ImGui.SameLine();
                if (ImGui.Button("Play Next"))
                    ExecuteOnFile(layer.PlayNext);
                ImGui.SameLine();
                if (ImGui.Button("Quick Play"))
                    ExecuteOnFile(layer.QuickPlay);

                if (ImGui.Button("Resume"))
                    layer.Resume();
                ImGui.SameLine();
                if (ImGui.Button("Pause"))
                    layer.Pause();
                ImGui.SameLine();
                if (ImGui.Button("Stop"))
                    layer.Stop();

                if (ImGui.Button("Loop Current"))
                    layer.LoopingCurrent = !layer.LoopingCurrent;
                ImGui.SameLine();
                ImGui.Text(layer.LoopingCurrent.ToString());

                var r = 0;
                string[] items = layer.Playlist.Select(x => x.Name).ToArray();
                ImGui.ListBox("Playlist", ref r, items, items.Length);

                ImGui.PopID();
                ImGui.NewLine();

                composer.PushModelMatrix(Matrix4x4.CreateTranslation(new Vector3(0, i * _waveFormHeight, 0)));
                cache?.Render(composer);
                composer.PopModelMatrix();
            }

            composer.PopModelMatrix();

            if (ImGui.Button("Create Layer") && !string.IsNullOrEmpty(_newLayerName))
            {
                Engine.Audio.CreateLayer(_newLayerName);
                _newLayerName = "New Layer" + Engine.Audio.GetLayers().Length;
            }

            ImGui.InputText("", ref _newLayerName, 50);
        }

        public override void Update()
        {
            string[] layers = Engine.Audio.GetLayers();
            for (var i = 0; i < layers.Length; i++)
            {
                AudioLayer layer = Engine.Audio.GetLayer(layers[i]);

                _waveFormCache.TryGetValue(layer, out WaveformCache cache);
                if (cache == null)
                {
                    cache = new WaveformCache(layer);
                    _waveFormCache[layer] = cache;
                }

                if (layer.Status == PlaybackStatus.NotPlaying && layer.Status != PlaybackStatus.Paused)
                {
                    cache.Clear();
                    continue;
                }

                // Update waveform cache.
                if (layer.CurrentTrack != cache.Track) cache.Create(layer.CurrentTrack, Engine.Renderer.DrawBuffer.Size.X, _waveFormHeight);
            }
        }
    }
}