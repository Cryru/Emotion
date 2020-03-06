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
using Emotion.Standard.Audio.WAV;
using Emotion.Tools.Windows.HelpWindows;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows
{
    public class AudioEditor : ImGuiWindow
    {
        private AudioAsset _file;
        private byte[] _converted;
        private int _convChan;
        private int _convSampleRate;
        private int _convBitsPerSample;
        private bool _convFloat;

        private static int _scale = 100;
        private static float _zoom = 0.25f;

        private int _inputChan;
        private int _inputSampleRate;
        private int _inputBitsPerSample;
        private bool _inputFloat;
        private int _convertQuality = 10;

        public AudioEditor() : base("Audio Editor")
        {
        }

        protected override void RenderContent(RenderComposer composer)
        {
            if (ImGui.Button("Choose File"))
            {
                var explorer = new FileExplorer<AudioAsset>(LoadFile);
                Parent.AddWindow(explorer);
            }

            ImGui.Text($"Current File: {_file?.Name ?? "None"}");
            if (_file == null) return;
            ImGui.Text($"Duration: {_file.Duration} seconds");
            ImGui.Text($"Channels: {_file.Format.Channels}");
            ImGui.Text($"Sample Rate: {_file.Format.SampleRate}");
            ImGui.Text($"Bits Per Sample: {_file.Format.BitsPerSample}");
            ImGui.SameLine();
            ImGui.Text(_file.Format.IsFloat ? "Float" : "Int");
            ImGui.Text($"Data Size: {_file.SoundData.Length}");

            if (ImGui.Button("Play"))
            {
                AudioLayer layer = Engine.Host.Audio.GetLayer("test");
                if (layer == null) layer = Engine.Host.Audio.CreateLayer("test");
                layer.PlayNext(_file);
            }

            ImGui.DragFloat("Zoom", ref _zoom, 0.1f, 0.1f, 1);

            ImGui.Text("Convert Input");
            ImGui.InputInt("Channels", ref _inputChan, 1);
            ImGui.InputInt("Sample Rate", ref _inputSampleRate, 100);
            ImGui.InputInt("Bits Per Sample", ref _inputBitsPerSample, 2);
            ImGui.Checkbox("isFloat", ref _inputFloat);
            ImGui.InputInt("Convert Quality", ref _convertQuality);
            if (ImGui.Button("Convert"))
            {
                _converted = new byte[_file.SoundData.Length];
                _file.SoundData.CopyTo(_converted);

                var dstFormat = new AudioFormat(_inputBitsPerSample, _inputFloat, _inputChan, _inputSampleRate);
                if (!_file.Format.Equals(dstFormat))
                    AudioUtil.ConvertFormat(_file.Format, dstFormat, ref _converted, _convertQuality);

                _convChan = _inputChan;
                _convSampleRate = _inputSampleRate;
                _convBitsPerSample = _inputBitsPerSample;
                _convFloat = _inputFloat;
            }

            if (_converted != null)
            {
                ImGui.Text("Converted");
                ImGui.Text($"Channels: {_convChan}");
                ImGui.Text($"Sample Rate: {_convSampleRate}");
                ImGui.Text($"Bits Per Sample: {_convBitsPerSample}");
                ImGui.SameLine();
                ImGui.Text(_convFloat ? "Float" : "Int");
                ImGui.Text($"Converted Byte Size: {_converted.Length}");

                if (ImGui.Button("Save To File"))
                {
                    byte[] wav = WavFormat.Encode(_converted, new AudioFormat(_inputBitsPerSample, _inputFloat, _inputChan, _inputSampleRate));
                    File.WriteAllBytes($"{Path.GetFileNameWithoutExtension(_file.Name) + "_converted"}.wav", wav);
                }
            }

            composer.SetUseViewMatrix(true);
            RenderWaveFormImage(composer, _file.SoundData.Span, _file.Format, Color.Red);
            if (_converted != null) RenderWaveFormImage(composer, _converted, _file.Format, Color.Yellow, _scale);
        }

        private void LoadFile(AudioAsset f)
        {
            _file = f;
            _converted = null;
            _inputChan = _file.Format.Channels;
            _inputSampleRate = _file.Format.SampleRate;
            _inputBitsPerSample = _file.Format.BitsPerSample;
            _inputFloat = _file.Format.IsFloat;
        }

        public override void Update()
        {
            var speed = 0.5f;
            Vector2 dir = Vector2.Zero;
            if (Engine.InputManager.IsKeyHeld(Key.A)) dir.X -= 1;
            if (Engine.InputManager.IsKeyHeld(Key.D)) dir.X += 1;
            if (Engine.InputManager.IsKeyHeld(Key.LeftControl)) speed *= 2;

            dir *= new Vector2(speed * Engine.DeltaTime, speed * Engine.DeltaTime);
            Engine.Renderer.Camera.Position += new Vector3(dir, 0);
        }

        public void RenderWaveFormImage(RenderComposer composer, Span<byte> data, AudioFormat f, Color color, float yOffset = 0)
        {
            var i = 0;
            float x = (Engine.Renderer.Camera.Position.X - Engine.Renderer.CurrentTarget.Size.X / 2) * _zoom;
            var prev = new Vector2(x, yOffset + _scale / 2);

            bool RenderFloat(float val)
            {
                // Simple clip.
                if (x > Engine.Renderer.CurrentTarget.Size.X / 2 + Engine.Renderer.Camera.Position.X) return false;

                var cur = new Vector2(x, yOffset + (val * (_scale / 2) + _scale / 2));
                composer.RenderLine(prev, cur, color);
                prev = cur;
                x += 1 * _zoom;
                return true;
            }

            switch (f.BitsPerSample)
            {
                case 16:
                    unsafe
                    {
                        fixed (void* dataPtr = &data[0])
                        {
                            var dataShort = new Span<short>(dataPtr, data.Length / 2);
                            for (; i < dataShort.Length; i += f.Channels)
                            {
                                float v;
                                if (dataShort[i] < 0)
                                    v = (float) -dataShort[i] / short.MinValue;
                                else
                                    v = (float) dataShort[i] / short.MaxValue;

                                if (!RenderFloat(v)) break;
                            }
                        }
                    }

                    break;
                case 32 when f.IsFloat:
                    unsafe
                    {
                        fixed (void* dataPtr = &data[0])
                        {
                            var dataFloat = new Span<float>(dataPtr, data.Length / 4);

                            for (; i < dataFloat.Length; i += f.Channels)
                            {
                                if (!RenderFloat(dataFloat[i])) break;
                            }
                        }
                    }

                    break;
                case 32:
                    unsafe
                    {
                        fixed (void* dataPtr = &data[0])
                        {
                            var dataInt = new Span<int>(dataPtr, data.Length / 2);
                            for (; i < dataInt.Length; i += f.Channels)
                            {
                                float v;
                                if (dataInt[i] < 0)
                                    v = (float) -dataInt[i] / int.MinValue;
                                else
                                    v = (float) dataInt[i] / int.MaxValue;

                                if (!RenderFloat(v)) break;
                            }
                        }
                    }

                    break;
            }
        }
    }
}