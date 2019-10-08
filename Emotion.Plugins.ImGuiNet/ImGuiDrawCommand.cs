#region Using

using System;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.Graphics.Command;
using Emotion.Graphics.Objects;
using Emotion.Primitives;
using ImGuiNET;
using OpenGL;

#endregion

namespace Emotion.Plugins.ImGuiNet
{
    public unsafe class ImGuiDrawCommand : RenderCommand
    {
        public VertexBuffer VBO;
        public IndexBuffer IBO;
        public VertexArrayObject VAO;

        public override void Execute(RenderComposer composer)
        {
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
                    Engine.Renderer.SetClip(new Rectangle(
                        currentCommandList.ClipRect.X,
                        currentCommandList.ClipRect.Y,
                        currentCommandList.ClipRect.Z - currentCommandList.ClipRect.X,
                        currentCommandList.ClipRect.W - currentCommandList.ClipRect.Y
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
        }

        public override void Process()
        {
        }
    }
}