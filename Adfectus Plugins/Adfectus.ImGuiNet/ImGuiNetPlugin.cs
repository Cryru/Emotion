#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Adfectus.Common;
using Adfectus.Graphics;
using Adfectus.Input;
using Adfectus.IO;
using ImGuiNET;

#endregion

namespace Adfectus.ImGuiNet
{
    /// <summary>
    /// The plugin for ImGui Net integration.
    /// </summary>
    public sealed class ImGuiNetPlugin : Plugin
    {
        private IntPtr _imguiContext;
        private string _fontPath;
        private int _fontSize;
        private float _pixelSize;

        /// <summary>
        /// Create the ImGuiNetPlugin to interface with Adfectus.
        /// </summary>
        /// <param name="ttfFontPath">The path which will be provided to the Assetloader to load the ttf font.</param>
        /// <param name="fontSize">The size of font to use.</param>
        /// <param name="pixelSize">The size in pixels to use.</param>
        public ImGuiNetPlugin(string ttfFontPath = null, int fontSize = 10, float pixelSize = 10)
        {
            _fontPath = ttfFontPath;
            _fontSize = fontSize;
            _pixelSize = pixelSize;
        }

        public override unsafe void Initialize()
        {
            // Load the imgui native library.
            //Bootstrapper.LoadAdditionalLibrary("cimgui", "cimgui");

            // Create the imgui context.
            _imguiContext = ImGui.CreateContext();
            ImGui.SetCurrentContext(_imguiContext);

            ImGuiIOPtr io = ImGui.GetIO();

            // Setup the font.
            if (_fontPath == null)
            {
                io.Fonts.AddFontDefault();
            }
            else
            {
                OtherAsset font = Engine.AssetLoader.Get<OtherAsset>(_fontPath);
                fixed (void* fontData = &font.Content[0])
                {
                    io.Fonts.AddFontFromMemoryTTF((IntPtr) fontData, _fontSize, _pixelSize);
                }

                // Clear the file.
                Engine.AssetLoader.Destroy(_fontPath);
            }

            io.DisplaySize = Engine.GraphicsManager.RenderSize;
            io.DisplayFramebufferScale = new Vector2(1f, 1f);
            io.NativePtr->IniFilename = null;
            io.NativePtr->LogFilename = null;

            // Configure the keymap from ImGuiKeys to Adfectus keys.
            io.KeyMap[(int) ImGuiKey.Tab] = (int) KeyCode.Tab;
            io.KeyMap[(int) ImGuiKey.LeftArrow] = (int) KeyCode.Left;
            io.KeyMap[(int) ImGuiKey.RightArrow] = (int) KeyCode.Right;
            io.KeyMap[(int) ImGuiKey.UpArrow] = (int) KeyCode.Up;
            io.KeyMap[(int) ImGuiKey.DownArrow] = (int) KeyCode.Down;
            io.KeyMap[(int) ImGuiKey.PageUp] = (int) KeyCode.PageUp;
            io.KeyMap[(int) ImGuiKey.PageDown] = (int) KeyCode.PageDown;
            io.KeyMap[(int) ImGuiKey.Home] = (int) KeyCode.Home;
            io.KeyMap[(int) ImGuiKey.End] = (int) KeyCode.End;
            io.KeyMap[(int) ImGuiKey.Delete] = (int) KeyCode.Delete;
            io.KeyMap[(int) ImGuiKey.Backspace] = (int) KeyCode.Backspace;
            io.KeyMap[(int) ImGuiKey.Enter] = (int) KeyCode.Enter;
            io.KeyMap[(int) ImGuiKey.Escape] = (int) KeyCode.Escape;
            io.KeyMap[(int) ImGuiKey.A] = (int) KeyCode.A;
            io.KeyMap[(int) ImGuiKey.C] = (int) KeyCode.C;
            io.KeyMap[(int) ImGuiKey.V] = (int) KeyCode.V;
            io.KeyMap[(int) ImGuiKey.X] = (int) KeyCode.X;
            io.KeyMap[(int) ImGuiKey.Y] = (int) KeyCode.Y;
            io.KeyMap[(int) ImGuiKey.Z] = (int) KeyCode.Z;

            // Upload the font.
            ImGuiNetController.ImGuiFontTexture = Engine.GraphicsManager.CreateTexture();

            // Get font texture from ImGui
            io.Fonts.GetTexDataAsRGBA32(out byte* pixelData, out int width, out int height, out int bytesPerPixel);

            // Copy the data to a managed array
            byte[] pixels = new byte[width * height * bytesPerPixel];
            Marshal.Copy(new IntPtr(pixelData), pixels, 0, pixels.Length);

            // Create and register the texture.
            Engine.GraphicsManager.BindTexture(ImGuiNetController.ImGuiFontTexture);
            Engine.GraphicsManager.UploadToTexture(pixels, new Vector2(width, height), TextureInternalFormat.Rgba, TexturePixelFormat.Rgba);

            // Let ImGui know where to find the texture.
            io.Fonts.SetTexID(new IntPtr(ImGuiNetController.ImGuiFontTexture));
            io.Fonts.ClearTexData();

            // Setup the stream buffer which will render the gui.
            uint ibo = Engine.GraphicsManager.CreateDataBuffer();
            Engine.GraphicsManager.BindIndexBuffer(ibo);
            Engine.GraphicsManager.UploadToIndexBuffer(IntPtr.Zero, Engine.Flags.RenderFlags.MaxRenderable * 6 * sizeof(ushort));

            uint vbo = Engine.GraphicsManager.CreateDataBuffer();
            Engine.GraphicsManager.BindDataBuffer(vbo);
            Engine.GraphicsManager.UploadToDataBuffer(IntPtr.Zero, (uint) (Engine.Flags.RenderFlags.MaxRenderable * 4 * sizeof(ImDrawVert)));

            uint vao = Engine.GraphicsManager.CreateVertexArrayBuffer();
            Engine.GraphicsManager.BindVertexArrayBuffer(vao);
            Engine.GraphicsManager.AttachDataBufferToVertexArray(vbo, vao, 0, 2, DataType.Float, false, (uint) sizeof(ImDrawVert), Marshal.OffsetOf(typeof(ImDrawVert), "pos"));
            Engine.GraphicsManager.AttachDataBufferToVertexArray(vbo, vao, 1, 2, DataType.Float, false, (uint) sizeof(ImDrawVert), Marshal.OffsetOf(typeof(ImDrawVert), "uv"));
            Engine.GraphicsManager.AttachDataBufferToVertexArray(vbo, vao, 3, 4, DataType.UnsignedByte, true, (uint) sizeof(ImDrawVert),
                Marshal.OffsetOf(typeof(ImDrawVert), "col"));

            ImGuiNetController.GuiBuffer = Engine.GraphicsManager.CreateStreamBuffer(vbo, vao, ibo, 1, Engine.Flags.RenderFlags.MaxRenderable, 1);
        }

        public override void Update()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.DeltaTime = Engine.FrameTime / 1000;

            // Update input.
            io.MousePos = Engine.InputManager.GetMousePosition();
            io.MouseDown[0] = Engine.InputManager.IsMouseKeyHeld(MouseKey.Left);
            io.MouseDown[1] = Engine.InputManager.IsMouseKeyHeld(MouseKey.Right);
            io.MouseDown[2] = Engine.InputManager.IsMouseKeyHeld(MouseKey.Middle);
            io.MouseWheel = Engine.InputManager.GetMouseScrollRelative();

            io.KeyCtrl = Engine.InputManager.IsKeyHeld(KeyCode.LeftControl) || Engine.InputManager.IsKeyHeld(KeyCode.RightControl);
            io.KeyAlt = Engine.InputManager.IsKeyHeld(KeyCode.LeftAlt) || Engine.InputManager.IsKeyHeld(KeyCode.RightAlt);
            io.KeyShift = Engine.InputManager.IsKeyHeld(KeyCode.LeftShift) || Engine.InputManager.IsKeyHeld(KeyCode.RightShift);
            io.KeySuper = Engine.InputManager.IsKeyHeld(KeyCode.LeftSuper) || Engine.InputManager.IsKeyHeld(KeyCode.RightSuper);

            char nextTInput = Engine.InputManager.GetNextTextInput();

            while (nextTInput != 0)
            {
                io.AddInputCharacter(nextTInput);
                nextTInput = Engine.InputManager.GetNextTextInput();
            }

            // Apply all key inputs.
            IEnumerable<KeyCode> downKeys = Engine.InputManager.GetAllKeysHeld();
            KeyCode[] allKeys = (KeyCode[]) Enum.GetValues(typeof(KeyCode));

            foreach (KeyCode key in allKeys)
            {
                io.KeysDown[(int) key] = false;
            }

            foreach (KeyCode key in downKeys)
            {
                io.KeysDown[(int) key] = true;
            }
        }

        public override void Dispose()
        {
            ImGui.DestroyContext(_imguiContext);
        }
    }

    /// <summary>
    /// Plugin extensions for other classes within Adfectus.
    /// </summary>
    public static unsafe class ImGuiNetController
    {
        /// <summary>
        /// The texture of the imgui font.
        /// </summary>
        public static uint ImGuiFontTexture;

        /// <summary>
        /// The gui buffer which will be used to render imgui.
        /// </summary>
        public static StreamBuffer GuiBuffer;

        /// <summary>
        /// Render the imgui calls.
        /// </summary>
        /// <param name="r">The Engine.Renderer module.</param>
        public static void RenderGui(this Renderer r)
        {
            // Submit the renderer up to here so there's no interference with its buffer.
            r.Submit();

            // Get render data from imgui.
            ImGui.Render();
            ImDrawDataPtr drawPointer = ImGui.GetDrawData();
            ImGuiIOPtr io = ImGui.GetIO();

            // Copy vertices and indices.
            uint vtxOffset = 0;
            uint idxOffset = 0;
            Engine.GraphicsManager.BindDataBuffer(GuiBuffer.Vbo);
            Engine.GraphicsManager.BindIndexBuffer(GuiBuffer.Ibo);

            for (int i = 0; i < drawPointer.CmdListsCount; i++)
            {
                ImDrawListPtr drawList = drawPointer.CmdListsRange[i];

                // Check if any command lists.
                if (drawList.CmdBuffer.Size == 0) continue;

                // Copy vertex and index buffers to the stream buffer.
                uint vtxSize = (uint) (drawList.VtxBuffer.Size * sizeof(ImDrawVert));
                uint idxSize = (uint) drawList.IdxBuffer.Size * sizeof(ushort);

                // Upload.
                Engine.GraphicsManager.MapDataBuffer(drawList.VtxBuffer.Data, vtxSize, vtxOffset);
                Engine.GraphicsManager.MapIndexBuffer(drawList.IdxBuffer.Data, idxSize, idxOffset);

                // Increment the offset trackers.
                vtxOffset += vtxSize;
                idxOffset += idxSize;
            }

            // Prepare for drawing. Set the needed state, bind the imgui texture.
            Engine.GraphicsManager.StateDepthTest(false);

            // Check if the view matrix is enabled.
            bool viewMatrixEnabled = Engine.GraphicsManager.ViewMatrixEnabled;
            if (viewMatrixEnabled) Engine.GraphicsManager.ViewMatrixEnabled = false;

            GuiBuffer.Reset();
            GuiBuffer.UnsafeSetMappedVertices(idxOffset / sizeof(ushort)); // The only reason for this is so the MappedVertices doesn't error.
            drawPointer.ScaleClipRects(io.DisplayFramebufferScale);
            Engine.GraphicsManager.BindTexture(ImGuiFontTexture);

            // Go through command lists and render.
            uint offset = 0;
            uint indicesOffset = 0;
            for (int i = 0; i < drawPointer.CmdListsCount; i++)
            {
                // Get the current draw list.
                ImDrawListPtr drawList = drawPointer.CmdListsRange[i];

                for (int cmdList = 0; cmdList < drawList.CmdBuffer.Size; cmdList++)
                {
                    ImDrawCmdPtr currentCommandList = drawList.CmdBuffer[cmdList];

                    // Set the clip rect.
                    Engine.GraphicsManager.SetClipRect(
                        (int) currentCommandList.ClipRect.X,
                        (int) currentCommandList.ClipRect.Y,
                        (int) (currentCommandList.ClipRect.Z - currentCommandList.ClipRect.X),
                        (int) (currentCommandList.ClipRect.W - currentCommandList.ClipRect.Y));

                    // Set the draw range of this specific command, and draw it.
                    GuiBuffer.SetRenderRangeIndices(offset, offset + currentCommandList.ElemCount);
                    GuiBuffer.SetBaseVertex(indicesOffset);
                    r.Render(GuiBuffer);

                    // Set drawing offset.
                    offset += currentCommandList.ElemCount;
                }

                indicesOffset += (uint) drawList.VtxBuffer.Size;
            }

            // Reset state to default.
            Engine.GraphicsManager.DefaultGLState();
            Engine.GraphicsManager.ViewMatrixEnabled = viewMatrixEnabled;
        }
    }
}