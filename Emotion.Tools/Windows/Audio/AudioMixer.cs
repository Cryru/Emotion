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
        private int _waveFormHeight = 100;

        private Dictionary<AudioLayer, WaveformVisualization> _waveFormCache = new Dictionary<AudioLayer, WaveformVisualization>();

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
                foreach (KeyValuePair<AudioLayer, WaveformVisualization> cache in _waveFormCache)
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
            composer.PushModelMatrix(Matrix4x4.CreateTranslation(new Vector3(0, 50, 0)));

            // Render ImGui section of layers.
            string[] layers = Engine.Audio.GetLayers();
            for (var i = 0; i < layers.Length; i++)
            {
                AudioLayer layer = Engine.Audio.GetLayer(layers[i]);
                _waveFormCache.TryGetValue(layer, out WaveformVisualization cache);

                ImGui.PushID(i);
                ImGui.Text($"Layer: {layers[i]} [{layer.Status}] {(layer.CurrentTrack != null ? $" {MathF.Truncate(layer.Playback * 100f) / 100f:0}/{layer.CurrentTrack.File.Duration:0.0}s" : "")}");

                float volume = layer.Volume;
                if (ImGui.DragFloat("Volume", ref volume, 0.01f, 0f, 1f))
                {
                    cache?.Recreate();
                    layer.Volume = volume;
                }

                if (ImGui.Button("Add To Queue"))
                    ExecuteOnFile(layer.AddToQueue);
                ImGui.SameLine();
                if (ImGui.Button("Add To Play Next"))
                    ExecuteOnFile(layer.PlayNext);
                ImGui.SameLine();
                if (ImGui.Button("Quick Play"))
                    ExecuteOnFile(layer.QuickPlay);

                if(layer.Status == PlaybackStatus.Paused)
                {
                    if (ImGui.Button("Resume"))
                        layer.Resume();
                }
                else
                {
                    if (ImGui.Button("Pause"))
                        layer.Pause();
                }
                ImGui.SameLine();
                
                ImGui.SameLine();
                if (ImGui.Button("Stop"))
                    layer.Stop();

                ImGui.SameLine();
                if (ImGui.Button("Toggle Loop"))
                    layer.LoopingCurrent = !layer.LoopingCurrent;
                ImGui.SameLine();
                ImGui.Text($"Looping: {layer.LoopingCurrent}");

                string[] items = layer.Playlist.Select(x => x.Name).ToArray();
                if (ImGui.TreeNode($"Playlist, Currently Playing: {(items.Length > 0 ? items[0] : "None")}"))
                {
                    var r = 0;
                    ImGui.ListBox("", ref r, items, items.Length);
                    ImGui.TreePop();
                }

                ImGui.PopID();
                ImGui.NewLine();

                composer.PushModelMatrix(Matrix4x4.CreateTranslation(new Vector3(10, i * (_waveFormHeight + 10), 0)));
                cache?.Render(composer);
                composer.PopModelMatrix();
            }

            composer.PopModelMatrix();

            ImGui.InputText("", ref _newLayerName, 50);
            ImGui.SameLine();
            if (ImGui.Button("Create Layer") && !string.IsNullOrEmpty(_newLayerName))
            {
                Engine.Audio.CreateLayer(_newLayerName);
                _newLayerName = "New Layer" + Engine.Audio.GetLayers().Length;
            }
        }

        public override void Update()
        {
            string[] layers = Engine.Audio.GetLayers();
            for (var i = 0; i < layers.Length; i++)
            {
                AudioLayer layer = Engine.Audio.GetLayer(layers[i]);

                _waveFormCache.TryGetValue(layer, out WaveformVisualization cache);
                if (cache == null)
                {
                    cache = new WaveformVisualization(layer);
                    _waveFormCache[layer] = cache;
                }

                if (layer.Status == PlaybackStatus.NotPlaying && layer.Status != PlaybackStatus.Paused)
                {
                    cache.Clear();
                    continue;
                }

                // Update waveform cache.
                if (layer.CurrentTrack != cache.Track && layer.CurrentTrack != null) cache.Create(layer.CurrentTrack, Math.Min(25 * layer.CurrentTrack.File.Duration / 1.0f, 600), _waveFormHeight);
            }
        }
    }
}