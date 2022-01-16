#region Using

using System.Collections.Generic;
using System.Numerics;
using Emotion.DebugTools;
using Emotion.Graphics;
using Emotion.Primitives;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;

#endregion

namespace Emotion.Tools.OtherPhysics.Velcro
{
    public class VelcroPhysicsAdapter : PhysicsTesterAdapter
    {
        public World World;
        private Dictionary<Body, Vector2> _bodyToSize = new Dictionary<Body, Vector2>();
        private List<Body> _bodies = new List<Body>();

        public VelcroPhysicsAdapter(Vector2 gravity)
        {
            World = new World(new Microsoft.Xna.Framework.Vector2(gravity.X, gravity.Y));
        }

        public override int AddBody(Vector2 position, Vector2 size, float rotation, int type)
        {
            BodyType bodyType = type == 0 ? BodyType.Static : BodyType.Dynamic;
            Body newBody = BodyFactory.CreateRectangle(World, size.X, size.Y, 1, new Microsoft.Xna.Framework.Vector2(position.X, position.Y), 0, bodyType);
            _bodyToSize.Add(newBody, size);
            _bodies.Add(newBody);
            return _bodies.Count;
        }

        public override void Simulate(float time)
        {
            World.Step(time);
        }

        public override void GetBodyData(int bodyIndex, out Vector2 position, out float rotation)
        {
            Body body = _bodies[bodyIndex];
            position = new Vector2(body.Position.X, body.Position.Y);
            rotation = body.Rotation;
        }

        public override void Render(RenderComposer c, Color color)
        {
            for (var i = 0; i < _bodies.Count; i++)
            {
                Body body = _bodies[i];
                Vector2 size = _bodyToSize[body];
                Rectangle v = new Rectangle(0, 0, size);
                v.Center = new Vector2(body.Position.X, body.Position.Y);

                c.PushModelMatrix(Matrix4x4.CreateRotationZ(body.Rotation, v.Center.ToVec3()) * c.ModelMatrix, false);
                c.RenderSprite(v.Position, v.Size, color);
                c.PopModelMatrix();
            }
        }
    }
}