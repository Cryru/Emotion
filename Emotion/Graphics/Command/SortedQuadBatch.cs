#region Using

using System;

#endregion

namespace Emotion.Graphics.Command
{
    public class SortedQuadBatch : QuadBatch
    {
        public override void Process(RenderComposer composer)
        {
            BatchedTexturables.Sort((x, y) => MathF.Sign(x.Position.Z - y.Position.Z));
            base.Process(composer);
        }
    }
}