#region Using

using Emotion.Game.Animation2D;
using Emotion.Game.QuadTree;
using Emotion.Game.World;
using Emotion.Graphics;

#endregion

#nullable enable

namespace Emotion.Game.World2D
{
    public partial class GameObject2D : BaseGameObject
    {
        public GameObject2D(string name) : base(name)
        {
        }

        // Serialization constructor.
        protected GameObject2D()
        {
        }

        /// <inheritdoc />
        protected override void RenderInternal(RenderComposer c)
        {
            c.RenderSprite(Bounds.PositionZ(Z), Size, Tint);
        }
    }
}