#region Using

using System;
using System.IO;
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
            for (int i = 0; i < layers.Length; i++)
            {
                AudioLayer layer = Engine.Host.Audio.GetLayer(layers[i]);

                ImGui.Text($"Layer {layers[i]}");

                ImGui.PushID(i);
                ImGui.Text($"Status: {layer.Status}");
                ImGui.Text($"Volume - {layer.Volume}");

                if(ImGui.Button("Add To Queue"))
                    ExecuteOnFile(layer.AddToQueue);
                ImGui.SameLine();
                if (ImGui.Button("Play Next"))
                    ExecuteOnFile(layer.PlayNext);

                if (ImGui.Button("Play"))
                    layer.Resume();
                ImGui.SameLine();
                if (ImGui.Button("Pause"))
                    layer.Pause();
                ImGui.SameLine();
                if (ImGui.Button("Stop"))
                    layer.Clear();
                ImGui.PopID();
            }

            ImGui.NewLine();

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