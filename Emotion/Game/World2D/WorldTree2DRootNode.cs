#region Using

using System.Collections.Generic;
using Emotion.Primitives;

#endregion

#nullable enable

namespace Emotion.Game.World2D
{
    public class WorldTree2DRootNode : WorldTree2DNode
    {
        protected Dictionary<GameObject2D, WorldTree2DNode> _objToNode = new();

        public WorldTree2DRootNode(Rectangle bounds) : base(bounds)
        {
        }

        public void UpdateObject(GameObject2D obj)
        {
        }
    }
}