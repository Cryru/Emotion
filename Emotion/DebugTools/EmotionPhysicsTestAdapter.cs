#region Using

using System.Collections.Generic;
using System.Numerics;
using Emotion.Game.Physics2D;
using Emotion.Game.Physics2D.Actors;
using Emotion.Graphics;
using Emotion.Primitives;

#endregion

namespace Emotion.DebugTools
{
    public class EmotionPhysicsTestAdapter : PhysicsTesterAdapter
    {
        public PhysicsWorld World;
        private Dictionary<PhysicsBody, Vector2> _bodyToSize = new Dictionary<PhysicsBody, Vector2>();

        public EmotionPhysicsTestAdapter(Rectangle worldBound, Vector2 gravity)
        {
            World = new PhysicsWorld(gravity);
        }

        public override int AddBody(Vector2 position, Vector2 size, float rotation, int type)
        {
            BodyType bodyType = type == 0 ? BodyType.Static : BodyType.Dynamic;
            var newBody = PhysicsBody.CreateRectangle(position, size, bodyType, rotation);
            World.AddBody(newBody);
            _bodyToSize.Add(newBody, size);
            return (int) newBody.WorldId;
        }

        public override void Simulate(float time)
        {
            World.Step(time);
        }

        public override void GetBodyData(int bodyIndex, out Vector2 position, out float rotation)
        {
            position = Vector2.Zero;
            rotation = 0;
            for (var i = 0; i < World.Bodies.Count; i++)
            {
                PhysicsBody body = World.Bodies[i];
                if (body.WorldId == bodyIndex)
                {
                    position = body.Position;
                    rotation = body.Rotation;
                    return;
                }
            }
        }

        public override void Render(RenderComposer c, Color color)
        {
            List<PhysicsBody> allBodies = World.Bodies;
            for (var i = 0; i < allBodies.Count; i++)
            {
                PhysicsBody body = allBodies[i];
                Vector2 size = _bodyToSize[body];
                var v = new Rectangle(0, 0, size)
                {
                    Center = body.Position
                };

                c.PushModelMatrix(Matrix4x4.CreateRotationZ(body.Rotation, body.Position.ToVec3()) * c.ModelMatrix, false);
                c.RenderSprite(v.Position, v.Size, color);
                c.PopModelMatrix();
            }
        }
    }
}