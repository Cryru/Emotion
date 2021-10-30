#region Using

using System;
using System.Collections;
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
using Emotion.Platform.Implementation.CommonDesktop;
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

            // Resolve native library through the Emotion platform.
            var libName = "cimgui";
            if (Engine.Host is DesktopPlatform desktop) libName = desktop.AppendPlatformIdentifierAndExtension("Cimgui", "cimgui");
            NativeLibrary.SetDllImportResolver(typeof(ImGui).Assembly, (_, _, _) => Engine.Host == null ? IntPtr.Zero : Engine.Host.LoadLibrary(libName));

            // Create render state for ImGui drawing.
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
            io.KeyMap[(int) ImGuiKey.LeftArrow] = (int) Key.LeftArrow;
            io.KeyMap[(int) ImGuiKey.RightArrow] = (int) Key.RightArrow;
            io.KeyMap[(int) ImGuiKey.UpArrow] = (int) Key.UpArrow;
            io.KeyMap[(int) ImGuiKey.DownArrow] = (int) Key.DownArrow;
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
            ImGuiFontTexture = new Texture(new Vector2(width, height), pixels, PixelFormat.Rgba);

            // Let ImGui know where to find the texture.
            io.Fonts.SetTexID(new IntPtr(ImGuiFontTexture.Pointer));
            io.Fonts.ClearTexData();
            if (!Gl.CurrentVersion.GLES) io.BackendFlags = ImGuiBackendFlags.RendererHasVtxOffset;

            // Setup the stream buffer which will render the gui.
            IBO = new IndexBuffer(RenderComposer.MAX_INDICES * sizeof(ushort), BufferUsage.DynamicDraw);
            VBO = new VertexBuffer((uint) (RenderComposer.MAX_INDICES * 4 * sizeof(ImDrawVert)));
            VAO = new VertexArrayObject<EmImGuiVertex>(VBO, IBO);

            Engine.Host.OnTextInputAll += c =>
            {
                if (c == '\t') return;
                _textInput.Add(c);
            };
            Engine.Host.OnKey.AddListener((_, __) => !Focused); // Block input while imgui is focused.
            Engine.CoroutineManager.StartCoroutine(UpdateRoutine());

            Initialized = true;
        }

        private IEnumerator UpdateRoutine()
        {
            while (Engine.Status != EngineStatus.Stopped)
            {
                Update();
                yield return null;
            }
        }

        public void Update()
        {
            // Invoke before start operations actions.
            while (!_beforeStartOperations.IsEmpty)
            {
                if (_beforeStartOperations.TryDequeue(out Action act))
                    act.Invoke();
            }

            ImGuiIOPtr io = ImGui.GetIO();
            io.DeltaTime = Engine.DeltaTime / 1000;

            // Set scale.
            io.DisplaySize = Engine.Renderer.DrawBuffer.Size / ImGuiScale;

            // Check focus.
            if (io.WantCaptureKeyboard || io.WantCaptureMouse)
                Focused = true;
            else
                Focused = false;

            // Update input.
            io.MousePos = Engine.Host.MousePosition / ImGuiScale;
            io.MouseDown[0] = Engine.Host.KeyState(Key.MouseKeyLeft);
            io.MouseDown[1] = Engine.Host.KeyState(Key.MouseKeyRight);
            io.MouseDown[2] = Engine.Host.KeyState(Key.MouseKeyMiddle);
            io.MouseWheel = -Engine.Host.GetMouseScrollRelative();

            io.KeyCtrl = Engine.Host.KeyState(Key.LeftControl);
            io.KeyAlt = Engine.Host.KeyState(Key.LeftAlt);
            io.KeyShift = Engine.Host.KeyState(Key.LeftShift);
            io.KeySuper = Engine.Host.KeyState(Key.LeftSuper);

            if (_textInput.Count > 0)
            {
                foreach (char c in _textInput)
                {
                    io.AddInputCharacter(c);
                }

                _textInput.Clear();
            }

            // Apply all key inputs.
            for (var i = 0; i < (int) Key.KeyboardLast; i++)
            {
                io.KeysDown[i] = Engine.Host.IsKeyHeld((Key) i);
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
            VertexArrayObject.EnsureBound(VAO);
            drawPointer.ScaleClipRects(io.DisplayFramebufferScale);

            // Go through command lists and render.
            for (var i = 0; i < drawPointer.CmdListsCount; i++)
            {
                // Get the current draw list.
                ImDrawListPtr drawList = drawPointer.CmdListsRange[i];

                // Copy vertex and index buffers to the stream buffer.
                var vtxSize = (uint) (drawList.VtxBuffer.Size * sizeof(ImDrawVert));
                uint idxSize = (uint) drawList.IdxBuffer.Size * sizeof(ushort);

                // Upload.
                VBO.UploadPartial(drawList.VtxBuffer.Data, vtxSize);
                IBO.UploadPartial(drawList.IdxBuffer.Data, idxSize);

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
                    if (Gl.CurrentVersion.GLES)
                        Gl.DrawElements(PrimitiveType.Triangles, (int) currentCommandList.ElemCount, DrawElementsType.UnsignedShort,
                            (IntPtr) (currentCommandList.IdxOffset * sizeof(ushort)));
                    else
                        Gl.DrawElementsBaseVertex(
                            PrimitiveType.Triangles,
                            (int) currentCommandList.ElemCount,
                            DrawElementsType.UnsignedShort,
                            (IntPtr) (currentCommandList.IdxOffset * sizeof(ushort)),
                            (int) currentCommandList.VtxOffset
                        );
                }
            }

            composer.PopModelMatrix();
            composer.SetState(RenderState.Default);
        }

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
                    var font = Engine.AssetLoader.Get<OtherAsset>(ttfFontPath);
                    if (font == null) return;

                    ImGuiIOPtr io = ImGui.GetIO();
                    fixed (void* fontData = &font.Content.Span[0])
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
                    var newFontTexture = new Texture(new Vector2(width, height), pixels, PixelFormat.Rgba);

                    // Let ImGui know where to find the texture.
                    io.Fonts.SetTexID(new IntPtr(newFontTexture.Pointer));
                    io.Fonts.ClearTexData();
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

        public void Dispose()
        {
        }
    }
}