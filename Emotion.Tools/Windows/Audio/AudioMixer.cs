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
        private string _newLayerName = "";

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

            void ExecuteOnFile(Action<AudioAsset> func)
            {
                _explorer = new FileExplorer<AudioAsset>(func);
                Parent.AddWindow(_explorer);
            }

            // Push waveforms down.
            composer.PushModelMatrix(Matrix4x4.CreateTranslation(new Vector3(0, 200, 0)));

            // Render ImGui section of layers.
            string[] layers = Engine.Host.Audio.GetLayers();
            for (var i = 0; i < layers.Length; i++)
            {
                AudioLayer layer = Engine.Host.Audio.GetLayer(layers[i]);

                ImGui.Text($"Layer {layers[i]}");

                ImGui.PushID(i);
                ImGui.Text($"Status: {layer.Status}" +
                           (layer.CurrentTrack != null ? $" {(MathF.Truncate(layer.CurrentTrack.Playback * 100f) / 100f).ToString("0")}/{layer.CurrentTrack.File.Duration}" : ""));
                float volume = layer.Volume;
                ImGui.InputFloat("Volume", ref volume);
                layer.Volume = volume;

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

                if (ImGui.Button("Loop"))
                    layer.LoopingCurrent = !layer.LoopingCurrent;

                var r = 0;
                string[] items = layer.Playlist.Select(x => x.Name).ToArray();
                ImGui.ListBox("Playlist", ref r, items, items.Length);

                ImGui.PopID();
                ImGui.NewLine();

                _waveFormCache.TryGetValue(layer, out WaveformCache cache);
                cache?.Render(composer);
            }

            composer.PopModelMatrix();

            if (ImGui.Button("Create Layer") && !string.IsNullOrEmpty(_newLayerName))
            {
                Engine.Host.Audio.CreateLayer(_newLayerName);
                _newLayerName = "";
            }

            ImGui.InputText("", ref _newLayerName, 50);
        }

        public override void Update()
        {
            string[] layers = Engine.Host.Audio.GetLayers();
            for (var i = 0; i < layers.Length; i++)
            {
                AudioLayer layer = Engine.Host.Audio.GetLayer(layers[i]);

                _waveFormCache.TryGetValue(layer, out WaveformCache cache);
                if (cache == null)
                {
                    cache = new WaveformCache(layer);
                    _waveFormCache[layer] = cache;
                }

                if (layer.Status != PlaybackStatus.Playing)
                {
                    cache.Clear();
                    continue;
                }

                // Update waveform cache.
                if (layer.CurrentTrack != cache.Track) cache.Create(layer.CurrentTrack, Engine.Renderer.DrawBuffer.Size.X, 200);
            }
        }
    }
}