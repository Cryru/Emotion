#region Using

using System.Collections.Generic;
using Emotion.Primitives;

#endregion

#nullable enable

namespace Emotion.Game.World2D
{
    public class WorldTree2DNode
    {
        public Rectangle Bounds;

        public WorldTree2DNode TopLeft
        {
            get => ChildNodes[0];
        }

        public WorldTree2DNode TopRight
        {
            get => ChildNodes[1];
        }

        public WorldTree2DNode BottomLeft
        {
            get => ChildNodes[2];
        }

        public WorldTree2DNode BottomRight
        {
            get => ChildNodes[3];
        }

        public WorldTree2DNode[] ChildNodes;

        // Contains all objects in the node and it's children nodes.
        protected List<GameObject2D>? _objects;

        public WorldTree2DNode(Rectangle bounds)
        {
            Bounds = bounds;
            ChildNodes = new WorldTree2DNode[4];
        }

        public void AddObject(GameObject2D obj)
        {
        }

        public void RemoveObject(GameObject2D obj)
        {
        }
    }
}