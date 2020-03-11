#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Graphics;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Shading;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.Primitives;
using Emotion.Standard.Logging;
using ImGuiNET;
using OpenGL;

#endregion

namespace Emotion.Plugins.ImGuiNet
{
    /// <summary>
    /// The plugin for ImGuiNet integration.
    /// </summary>
    public class ImGuiNetPlugin : IPlugin
    {
        #region Properties

        /// <summary>
        /// Whether the plugin is initialized.
        /// </summary>
        public static bool Initialized { get; private set; }

        /// <summary>
        /// Whether any widget is focused and requires input.
        /// </summary>
        public static bool Focused { get; private set; }

        #endregion

        public static float ImGuiScale
        {
            get => (int) MathF.Max(1, Engine.Renderer.Scale * 0.5f);
        }

        #region Privates

        private static ConcurrentQueue<Action> _beforeStartOperations = new ConcurrentQueue<Action>();
        private static IntPtr _imguiContext;
        private static Dictionary<string, ImFontPtr> _loadedFonts = new Dictionary<string, ImFontPtr>();
        private static bool _usingDefaultFont = true;
        private static List<char> _textInput = new List<char>();

        /// <summary>
        /// The state in which ImGui is drawn.
        /// </summary>
        private static RenderState _imGuiState;

        #endregion

        #region Objects

        public static VertexBuffer VBO;
        public static IndexBuffer IBO;
        public static VertexArrayObject VAO;

        /// <summary>
        /// The texture of the imgui font.
        /// </summary>
        public static Texture ImGuiFontTexture;

        #endregion

        public unsafe void Initialize()
        {
            if (Initialized)
            {
                Engine.Log.Warning("The ImGuiNet plugin is already initialized.", "ImGuiNet");
                return;
            }

            if (!GLThread.IsGLThread())
            {
                Engine.Log.Warning("The ImGuiNet plugin must be initialized in the graphics thread.", "ImGuiNet");
                return;
            }

            _imGuiState = new RenderState
            {
                DepthTest = false,
                ViewMatrix = false,
                Shader = ShaderFactory.DefaultProgram
            };

            // Create the ImGui context.
            _imguiContext = ImGui.CreateContext();
            ImGui.SetCurrentContext(_imguiContext);

            ImGuiIOPtr io = ImGui.GetIO();

            // Setup the font and display parameters.
            io.Fonts.AddFontDefault();
            io.DisplaySize = Engine.Renderer.DrawBuffer.Size / ImGuiScale;
            io.DisplayFramebufferScale = new Vector2(1, 1);
            io.NativePtr->IniFilename = null;
            io.NativePtr->LogFilename = null;

            // Configure the keymap from ImGuiKeys to Adfectus keys.
            io.KeyMap[(int) ImGuiKey.Tab] = (int) Key.Tab;
            io.KeyMap[(int) ImGuiKey.LeftArrow] = (int) Key.Left;
            io.KeyMap[(int) ImGuiKey.RightArrow] = (int) Key.Right;
            io.KeyMap[(int) ImGuiKey.UpArrow] = (int) Key.Up;
            io.KeyMap[(int) ImGuiKey.DownArrow] = (int) Key.Down;
            io.KeyMap[(int) ImGuiKey.PageUp] = (int) Key.PageUp;
            io.KeyMap[(int) ImGuiKey.PageDown] = (int) Key.PageDown;
            io.KeyMap[(int) ImGuiKey.Home] = (int) Key.Home;
            io.KeyMap[(int) ImGuiKey.End] = (int) Key.End;
            io.KeyMap[(int) ImGuiKey.Delete] = (int) Key.Delete;
            io.KeyMap[(int) ImGuiKey.Backspace] = (int) Key.Backspace;
            io.KeyMap[(int) ImGuiKey.Enter] = (int) Key.Enter;
            io.KeyMap[(int) ImGuiKey.Escape] = (int) Key.Escape;
            io.KeyMap[(int) ImGuiKey.A] = (int) Key.A;
            io.KeyMap[(int) ImGuiKey.C] = (int) Key.C;
            io.KeyMap[(int) ImGuiKey.V] = (int) Key.V;
            io.KeyMap[(int) ImGuiKey.X] = (int) Key.X;
            io.KeyMap[(int) ImGuiKey.Y] = (int) Key.Y;
            io.KeyMap[(int) ImGuiKey.Z] = (int) Key.Z;

            // Get font texture from ImGui
            io.Fonts.GetTexDataAsRGBA32(out byte* pixelData, out int width, out int height, out int bytesPerPixel);

            // Copy the data to a managed array
            var pixels = new byte[width * height * bytesPerPixel];
            Marshal.Copy(new IntPtr(pixelData), pixels, 0, pixels.Length);

            // Create the font texture.
            ImGuiFontTexture = new Texture(new Vector2(width, height), pixels);

            // Let ImGui know where to find the texture.
            io.Fonts.SetTexID(new IntPtr(ImGuiFontTexture.Pointer));
            io.Fonts.ClearTexData();

            // Setup the stream buffer which will render the gui.
            IBO = new IndexBuffer(RenderComposer.MAX_INDICES * sizeof(ushort), BufferUsage.DynamicDraw);
            VBO = new VertexBuffer((uint) (RenderComposer.MAX_INDICES * 4 * sizeof(ImDrawVert)));
            VAO = new VertexArrayObject<EmImGuiVertex>(VBO, IBO);


            Engine.Host.OnTextInput.AddListener(c =>
            {
                _textInput.Add(c);
                return true;
            });
            Engine.Host.OnKey.AddListener((_, __) => !Focused);
            Engine.Host.OnMouseKey.AddListener((_, __) => !Focused);

            Initialized = true;
        }

        public void Update()
        {
            // Invoke before start operations actions.
            while (!_beforeStartOperations.IsEmpty)
            {
                _beforeStartOperations.TryDequeue(out Action act);
                act.Invoke();
            }

            ImGuiIOPtr io = ImGui.GetIO();
            io.DeltaTime = Engine.DeltaTime / 1000;

            // Set scale.
            io.DisplaySize = Engine.Renderer.DrawBuffer.Size;

            // Check focus.
            if (io.WantCaptureKeyboard || io.WantCaptureMouse)
                Focused = true;
            else
                Focused = false;

            // Update input.
            io.MousePos = Engine.Host.MousePosition / ImGuiScale;
            io.MouseDown[0] = Engine.InputManager.IsMouseKeyDown(MouseKey.Left) || Engine.InputManager.IsMouseKeyHeld(MouseKey.Left);
            io.MouseDown[1] = Engine.InputManager.IsMouseKeyDown(MouseKey.Right) || Engine.InputManager.IsMouseKeyHeld(MouseKey.Right);
            io.MouseDown[2] = Engine.InputManager.IsMouseKeyDown(MouseKey.Middle) || Engine.InputManager.IsMouseKeyHeld(MouseKey.Middle);
            io.MouseWheel = -Engine.InputManager.GetMouseScrollRelative();

            io.KeyCtrl = KeyDownOrHeld(Key.LeftControl) || KeyDownOrHeld(Key.RightControl);
            io.KeyAlt = KeyDownOrHeld(Key.LeftAlt) || KeyDownOrHeld(Key.RightAlt);
            io.KeyShift = KeyDownOrHeld(Key.LeftShift) || KeyDownOrHeld(Key.RightShift);
            io.KeySuper = KeyDownOrHeld(Key.LeftSuper) || KeyDownOrHeld(Key.RightSuper);

            if (_textInput.Count > 0)
            {
                foreach (char c in _textInput)
                {
                    io.AddInputCharacter(c);
                }

                _textInput.Clear();
            }

            // Apply all key inputs.
            for (var i = 0; i < (int) Key.Last; i++)
            {
                io.KeysDown[i] = Engine.InputManager.IsKeyHeld((Key) i);
            }
        }

        public static unsafe void RenderUI(RenderComposer composer)
        {
            composer.PushModelMatrix(Matrix4x4.CreateScale(ImGuiScale));
            composer.SetState(_imGuiState);

            // Get render data from imgui.
            ImGui.Render();
            ImDrawDataPtr drawPointer = ImGui.GetDrawData();
            ImGuiIOPtr io = ImGui.GetIO();

            // Copy vertices and indices.
            uint vtxOffset = 0;
            uint idxOffset = 0;
            VertexArrayObject.EnsureBound(VAO);

            for (var i = 0; i < drawPointer.CmdListsCount; i++)
            {
                ImDrawListPtr drawList = drawPointer.CmdListsRange[i];

                // Check if any command lists.
                if (drawList.CmdBuffer.Size == 0) continue;

                // Copy vertex and index buffers to the stream buffer.
                var vtxSize = (uint) (drawList.VtxBuffer.Size * sizeof(ImDrawVert));
                uint idxSize = (uint) drawList.IdxBuffer.Size * sizeof(ushort);

                // Upload.
                VBO.UploadPartial(drawList.VtxBuffer.Data, vtxSize, vtxOffset);
                IBO.UploadPartial(drawList.IdxBuffer.Data, idxSize, idxOffset);

                // Increment the offset trackers.
                vtxOffset += vtxSize;
                idxOffset += idxSize;
            }

            drawPointer.ScaleClipRects(io.DisplayFramebufferScale);

            // Go through command lists and render.
            uint offset = 0;
            uint indicesOffset = 0;
            for (var i = 0; i < drawPointer.CmdListsCount; i++)
            {
                // Get the current draw list.
                ImDrawListPtr drawList = drawPointer.CmdListsRange[i];

                for (var cmdList = 0; cmdList < drawList.CmdBuffer.Size; cmdList++)
                {
                    ImDrawCmdPtr currentCommandList = drawList.CmdBuffer[cmdList];

                    Texture.EnsureBound((uint) currentCommandList.TextureId);

                    // Set the clip rect.
                    composer.SetClipRect(new Rectangle(
                        currentCommandList.ClipRect.X * ImGuiScale,
                        currentCommandList.ClipRect.Y * ImGuiScale,
                        (currentCommandList.ClipRect.Z - currentCommandList.ClipRect.X) * ImGuiScale,
                        (currentCommandList.ClipRect.W - currentCommandList.ClipRect.Y) * ImGuiScale
                    ));

                    // Set the draw range of this specific command, and draw it.
                    Gl.DrawElementsBaseVertex(
                        PrimitiveType.Triangles,
                        (int) currentCommandList.ElemCount,
                        DrawElementsType.UnsignedShort,
                        (IntPtr) (offset * sizeof(ushort)),
                        (int) indicesOffset
                    );

                    // Set drawing offset.
                    offset += currentCommandList.ElemCount;
                }

                indicesOffset += (uint) drawList.VtxBuffer.Size;
            }

            composer.PopModelMatrix();

            // No need to restore state, as this should be rendered last and is also used in developer mode only.
        }

        #region Helpers

        /// <summary>
        /// Whether the key is down or held.
        /// </summary>
        /// <param name="key">The keyboard key to check.</param>
        /// <returns>Whether the key is down or held.</returns>
        private static bool KeyDownOrHeld(Key key)
        {
            return Engine.InputManager.IsKeyDown(key) || Engine.InputManager.IsKeyHeld(key);
        }

        #endregion

        #region Font Helpers

        /// <summary>
        /// Load a font.
        /// </summary>
        /// <param name="ttfFontPath">An Adfectus asset path to the font to load. Only ttf format is supported.</param>
        /// <param name="fontSize">The font size to load.</param>
        /// <param name="pixelSize">The font's pixel size to load.</param>
        public static unsafe void LoadFont(string ttfFontPath, int fontSize, int pixelSize)
        {
            _beforeStartOperations.Enqueue(() =>
            {
                GLThread.ExecuteGLThread(() =>
                {
                    ImGuiIOPtr io = ImGui.GetIO();
                    var font = Engine.AssetLoader.Get<OtherAsset>(ttfFontPath);
                    fixed (void* fontData = &font.Content[0])
                    {
                        _loadedFonts.Add(ttfFontPath, io.Fonts.AddFontFromMemoryTTF((IntPtr) fontData, fontSize, pixelSize));
                    }

                    // Clear the file.
                    Engine.AssetLoader.Destroy(ttfFontPath);

                    // Get font texture from ImGui
                    io.Fonts.GetTexDataAsRGBA32(out byte* pixelData, out int width, out int height, out int bytesPerPixel);

                    // Copy the data to a managed array
                    var pixels = new byte[width * height * bytesPerPixel];
                    Marshal.Copy(new IntPtr(pixelData), pixels, 0, pixels.Length);

                    // Upload the font.
                    var newFontTexture = new Texture(new Vector2(width, height), pixels);

                    // Let ImGui know where to find the texture.
                    io.Fonts.SetTexID(new IntPtr(newFontTexture.Pointer));
                    io.Fonts.ClearTexData();

                    Engine.AssetLoader.Destroy(ttfFontPath);
                });
            });
        }

        /// <summary>
        /// Use a font, or return to the default one.
        /// Fonts must be loaded using the "LoadFont" function first.
        /// </summary>
        /// <param name="fontPath">An Adfectus asset path to the loaded font to use, or null to use the default one.</param>
        public static void UseFont(string fontPath)
        {
            // Check if resetting to default font.
            if (fontPath == null)
            {
                if (!_usingDefaultFont) ImGui.PopFont();
                _usingDefaultFont = true;
                return;
            }

            // Try to get the loaded font.
            bool gotten = _loadedFonts.TryGetValue(fontPath, out ImFontPtr font);
            if (!gotten)
            {
                Engine.Log.Info($"ImGuiPlugin: Tried to use font {fontPath} which isn't loaded.", MessageSource.Other);
                return;
            }

            ImGui.PushFont(font);
            _usingDefaultFont = false;
        }

        #endregion
    }
}