#region Using

using System;
using System.Numerics;
using Emotion.Graphics;
using Emotion.Graphics.Objects;
using Emotion.Primitives;
using ImGuiNET;

#endregion

namespace Emotion.Plugins.ImGuiNet
{
    public static class ImGuiExtensions
    {
        public static Tuple<Vector2, Vector2> GetImGuiUV(this Texture t, Rectangle? uv = null)
        {
            Rectangle reqUv;
            if (uv == null)
                reqUv = new Rectangle(0, 0, t.Size);
            else
                reqUv = (Rectangle) uv;

            var uvOne = new Vector2(
                reqUv.X / t.Size.X,
                reqUv.Y / t.Size.Y
            );
            var uvTwo = new Vector2(
                (reqUv.X + reqUv.Size.X) / t.Size.X,
                (reqUv.Y + reqUv.Size.Y) / t.Size.Y
            );

            return new Tuple<Vector2, Vector2>(uvOne, uvTwo);
        }

        public static void RenderUI(this RenderComposer composer)
        {
            ImGuiNetPlugin.RenderUI(composer);
        }
    }
}