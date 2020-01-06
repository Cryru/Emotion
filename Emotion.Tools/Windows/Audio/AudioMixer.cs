#region Using

using System;
using System.IO;
using System.Linq;
using System.Numerics;
using Emotion.Audio;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Primitives;
using Emotion.Standard.Audio;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows
{
    public class AudioMixer : ImGuiWindow
    {
        private FileExplorer<AudioAsset> _explorer;
        private string _newLayerName = "";

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

            string[] layers = Engine.Host.Audio.GetLayers();
            for (var i = 0; i < layers.Length; i++)
            {
                AudioLayer layer = Engine.Host.Audio.GetLayer(layers[i]);

                ImGui.Text($"Layer {layers[i]}");

                ImGui.PushID(i);
                ImGui.Text($"Status: {layer.Status}" + (layer.CurrentTrack != null ? $" {(MathF.Truncate(layer.CurrentTrack.Playback * 100f) / 100f).ToString("0")}/{layer.CurrentTrack.File.Duration}" : ""));
                //ImGui.SameLine();
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
            }

            if(ImGui.Button("Create Layer") && !string.IsNullOrEmpty(_newLayerName))
            {
                Engine.Host.Audio.CreateLayer(_newLayerName);
                _newLayerName = "";
            }
            ImGui.InputText("", ref _newLayerName, 50);
        }

        public override void Update()
        {

        }
    }
}