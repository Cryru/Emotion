#region Using

using System;
using System.Numerics;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Platform.Implementation;
using Emotion.Platform.Input;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Primitives;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows
{
    public class AudioEditor : ImGuiWindow
    {
        private WaveSoundAsset _file;
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

        public AudioEditor() : base("Audio Editor")
        {
        }

        protected override void RenderContent(RenderComposer composer)
        {
            if (ImGui.Button("Choose File"))
            {
                var explorer = new FileExplorer<WaveSoundAsset>(LoadFile);
                Parent.AddWindow(explorer);
            }

            ImGui.Text($"Current File: {_file?.Name ?? "None"}");
            if (_file == null) return;
            ImGui.Text($"Channels: {_file.Channels}");
            ImGui.Text($"Sample Rate: {_file.SampleRate}");
            ImGui.Text($"Bits Per Sample: {_file.BitsPerSample}");
            ImGui.SameLine();
            ImGui.Text(_file.IsFloat ? "Float" : "Int");
            ImGui.Text($"Data Size: {_file.SoundData.Length}");

            if (ImGui.Button("Play"))
            {
                Engine.Host.Audio.PlayAudioTest(_file);
            }

            ImGui.DragFloat("Zoom", ref _zoom, 0.1f, 0.1f, 1);

            ImGui.Text("Convert Input");
            ImGui.InputInt("Channels", ref _inputChan, 1);
            ImGui.InputInt("Sample Rate", ref _inputSampleRate, 100);
            ImGui.InputInt("Bits Per Sample", ref _inputBitsPerSample, 2);
            ImGui.Checkbox("isFloat", ref _inputFloat);
            if (ImGui.Button("Convert"))
            {
                _converted = new byte[_file.SoundData.Length];
                Array.Copy(_file.SoundData, 0, _converted, 0, _file.SoundData.Length);

                if (_file.Channels != _inputChan || _file.BitsPerSample != _inputBitsPerSample || _file.IsFloat != _inputFloat || _file.SampleRate != _inputSampleRate)
                    AudioContext.ConvertFormat(_file.BitsPerSample, _file.IsFloat, _file.SampleRate, _file.Channels,
                        _inputBitsPerSample, _inputFloat, _inputSampleRate,  _inputChan, ref _converted);

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
            }

            composer.SetUseViewMatrix(true);
            RenderWaveFormImage(composer, _file.SoundData, _file.Channels, _file.BitsPerSample, _file.IsFloat, Color.Red);
            if (_converted != null) RenderWaveFormImage(composer, _converted, _convChan * (_convSampleRate / _file.SampleRate), _convBitsPerSample, _convFloat, Color.Yellow, _scale);
        }

        private void LoadFile(WaveSoundAsset f)
        {
            _file = f;
            _converted = null;
            _inputChan = _file.Channels;
            _inputSampleRate = _file.SampleRate;
            _inputBitsPerSample = _file.BitsPerSample;
            _inputFloat = _file.IsFloat;
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

        public void RenderWaveFormImage(RenderComposer composer, byte[] data, int channels, int bitsPerSample, bool isFloat, Color color, float yOffset = 0)
        {
            var i = 0;
            float x = (Engine.Renderer.Camera.Position.X - Engine.Renderer.CurrentTarget.Size.X / 2) * _zoom;
            var prev = new Vector2(x, yOffset + _scale / 2);
            bool RenderFloat(float val)
            {
                // Simple clip.
                if(x > Engine.Renderer.CurrentTarget.Size.X / 2 + Engine.Renderer.Camera.Position.X) return false;

                var cur = new Vector2(x, yOffset + (val * (_scale / 2) + _scale / 2));
                composer.RenderLine(prev, cur, color);
                prev = cur;
                x += 1 * _zoom;
                return true;
            }

            switch (bitsPerSample)
            {
                case 16:
                    unsafe
                    {
                        fixed (void* dataPtr = &data[0])
                        {
                            var dataShort = new Span<short>(dataPtr, data.Length / 2);
                            for (; i < dataShort.Length; i += channels)
                            {
                                float v;
                                if (dataShort[i] < 0)
                                {
                                    v = (float) -dataShort[i] / short.MinValue;
                                } 
                                else
                                {
                                    v = (float) dataShort[i] / short.MaxValue;
                                }

                                if(!RenderFloat(v)) break;
                            }
                        }
                    }

                    break;
                case 32 when isFloat:
                    unsafe
                    {
                        fixed (void* dataPtr = &data[0])
                        {
                            var dataFloat = new Span<float>(dataPtr, data.Length / 4);

                            for (; i < dataFloat.Length; i += channels)
                            {
                                if(!RenderFloat(dataFloat[i])) break;
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
                            for (; i < dataInt.Length; i += channels)
                            {
                                float v;
                                if (dataInt[i] < 0)
                                {
                                    v = (float) -dataInt[i] / int.MinValue;
                                } 
                                else
                                {
                                    v = (float) dataInt[i] / int.MaxValue;
                                }

                                if(!RenderFloat(v)) break;
                            }
                        }
                    }

                    break;
            }
        }
    }
}