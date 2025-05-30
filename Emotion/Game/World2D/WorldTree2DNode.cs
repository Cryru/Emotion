﻿#region Using

using Emotion.Game.World;
using Emotion.Graphics;

#endregion

#nullable enable

namespace Emotion.Game.World2D
{
    public class WorldTree2DNode
    {
        public Rectangle Bounds;
        public int Capacity;
        public int MaxDepth;

        public WorldTree2DNode? Parent;
        public WorldTree2DNode[]? ChildNodes;

        // Node objects if undivided, and objects which span multiple nodes if divided.
        public List<BaseGameObject>? Objects;

        public WorldTree2DNode(WorldTree2DNode? parent, Rectangle bounds, int capacity = 3, int maxDepth = 5)
        {
            Parent = parent;
            Bounds = bounds;
            Capacity = capacity;
            MaxDepth = maxDepth;
        }

        public WorldTree2DNode GetNodeForBounds(Rectangle bounds)
        {
            if (ChildNodes == null) return this;

            for (var i = 0; i < ChildNodes.Length; i++)
            {
                WorldTree2DNode node = ChildNodes[i];
                if (node.Bounds.ContainsInclusive(bounds)) return node.GetNodeForBounds(bounds);
            }

            return this;
        }

        public WorldTree2DNode AddObject(Rectangle bounds, BaseGameObject obj)
        {
            Objects ??= new List<BaseGameObject>();
            if (Objects.Count + 1 > Capacity && ChildNodes == null && MaxDepth > 0)
            {
                float halfWidth = Bounds.Width / 2;
                float halfHeight = Bounds.Height / 2;

                ChildNodes = new WorldTree2DNode[4];
                ChildNodes[0] = new WorldTree2DNode(this, new Rectangle(Bounds.X, Bounds.Y, halfWidth, halfHeight), Capacity, MaxDepth - 1);
                ChildNodes[1] = new WorldTree2DNode(this, new Rectangle(Bounds.X + halfWidth, Bounds.Y, halfWidth, halfHeight), Capacity, MaxDepth - 1);
                ChildNodes[2] = new WorldTree2DNode(this, new Rectangle(Bounds.X, Bounds.Y + halfHeight, halfWidth, halfHeight), Capacity, MaxDepth - 1);
                ChildNodes[3] = new WorldTree2DNode(this, new Rectangle(Bounds.X + halfWidth, Bounds.Y + halfHeight, halfWidth, halfHeight), Capacity, MaxDepth - 1);

                WorldTree2DNode subNode = GetNodeForBounds(bounds);
                return subNode.AddObject(bounds, obj);
            }

            Assert(Objects.IndexOf(obj) == -1);
            Objects.Add(obj);
            return this;
        }

        public void RemoveObject(BaseGameObject obj)
        {
            Objects?.Remove(obj);
        }

        public void RenderDebug(RenderComposer c)
        {
            c.RenderRectOutline(Bounds, Color.Blue);
            if (ChildNodes == null) return;
            for (var i = 0; i < ChildNodes.Length; i++)
            {
                WorldTree2DNode node = ChildNodes[i];
                node.RenderDebug(c);
            }
        }
    }
}