#region Using

using System;
using System.Numerics;
using Emotion.Graphics.Objects;
using Emotion.Primitives;

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

            Vector2 uvOne = new Vector2(
                reqUv.X / t.Size.X,
                reqUv.Y / t.Size.Y * -1
            );
            Vector2 uvTwo = new Vector2(
                (reqUv.X + reqUv.Size.X) / t.Size.X,
                (reqUv.Y + reqUv.Size.Y) / t.Size.Y * -1
            );

            return new Tuple<Vector2, Vector2>(uvOne, uvTwo);
        }
    }
}