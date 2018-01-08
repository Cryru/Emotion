using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Breath.Systems;
using OpenTK;
using Soul.Engine.ECS.Components;
using Soul.Engine.Modules;
using Soul.Physics.Collision.Shapes;
using Soul.Physics.Common;
using Soul.Physics.Dynamics;
using Soul.Physics.Factories;
using Transform = Soul.Engine.ECS.Components.Transform;
using Vector2 = OpenTK.Vector2;

namespace Soul.Engine.ECS.Systems
{
    public class PhysicsEngine : SystemBase
    {

        #region Declarations

        /// <summary>
        /// The physics world for the current scene.
        /// </summary>
        public World CurrentWorld;

        /// <summary>
        /// The default gravity.
        /// </summary>
        public static Vector2 DefaultGravity = new Vector2(0, 9.8f);

        #endregion

        #region Static Properties

        /// <summary>
        /// The scale at which to simulate physics.
        /// </summary>
        public static float Scale = 5;

        /// <summary>
        /// The scale at which to simulate physics in reverse.
        /// </summary>
        private static float ScaleReverse
        {
            get { return 1 / Scale; }
        }

        #endregion

        public PhysicsEngine(Vector2 gravity)
        {
            CurrentWorld = new World(new Physics.Common.Vector2(gravity.X, gravity.Y));
        }

        protected internal override Type[] GetRequirements()
        {
            return new[] { typeof(PhysicsObject), typeof(Transform) };
        }

        protected internal override void Setup()
        {

        }

        protected override void Update(Entity link)
        {
            // Get components.
            PhysicsObject physics = link.GetComponent<PhysicsObject>();
            Transform transform = link.GetComponent<Transform>();

            // Recreate body if updated.
            if (physics.HasUpdated)
            {
                if (physics.Body != null)
                {
                    if (physics.Body?.FixtureList != null) physics.Body.DestroyFixture(physics.Body.FixtureList[0]);
                    CurrentWorld.RemoveBody(physics.Body);

#if DEBUG
                    Debugging.DebugMessage(Enums.DebugMessageType.Warning,
                        "Destroyed a body at " + physics.Body.Position);
#endif
                    physics.Body = null;
                }
            }

            // Create physics body if one is missing.
            if (physics.Body == null)
            {
                // Create a body from the defined shape.
                switch (physics.Shape)
                {
                    case Enums.PhysicsShapeType.Polygon:

                        Vertices vertices = new Vertices(physics.PolygonVertices.Length);

                        // Convert vertices from pixel coordinates to physics coordinates.
                        foreach (Vector2 vec in physics.PolygonVertices)
                        {
                            vertices.Add(PixelToPhysics(vec));
                        }
                        // Create a polygon body from the vertices.
                        physics.Body = BodyFactory.CreatePolygon(
                            CurrentWorld,
                            vertices,
                            1,
                            link);
                        // Set the transform size to the bounds of the vertices.
                        Vector2 offset;
                        Vector2 scale = new Vector2(1, 1);

                        physics.PolygonSizeOffset = Helpers.CalculateSizeFromVertices(physics.PolygonVertices, scale, out offset) + offset;

                        break;
                    case Enums.PhysicsShapeType.Rectangle:
                        // Create a body from a rectangle template.
                        physics.Body = BodyFactory.CreateRectangle(
                            CurrentWorld,
                            PixelToPhysics(transform.Width),
                            PixelToPhysics(transform.Height),
                            1,
                            link);
                        break;
                    case Enums.PhysicsShapeType.Circle:
                        // Create a body from a circle template.
                        physics.Body = BodyFactory.CreateCircle(
                            CurrentWorld,
                            PixelToPhysics(Math.Max(transform.Width / 2, transform.Height / 2)),
                            1, link);
                        break;
                    default:
                        ErrorHandling.Raise(Enums.ErrorOrigin.Physics, "Unknown physics shape type: " + physics.Shape);
                        break;
                }

                // Error check.
                if (physics.Body == null) return;

                // Assign the body type.
                physics.Body.BodyType = physics.SimulationType;

                // Set the physics body's position according to the transform's center.
                physics.Body.Position = PixelToPhysics(transform.Center);

#if DEBUG
                Debugging.DebugMessage(Enums.DebugMessageType.InfoGreen,
                    "Created physics body of type " + physics.SimulationType + " and shape " + physics.Shape);
#endif
            }

            // Update the entity's position and rotation according to what's happening in the physics world.
            if (physics.Shape != Enums.PhysicsShapeType.Polygon)
            {
                transform.Center = PhysicsToPixel(physics.Body.Position.X, physics.Body.Position.Y);
            }
            else
            {
                Vector2 physicsSize = PhysicsToPixel(physics.Body.Position.X, physics.Body.Position.Y);

                transform.X = physicsSize.X - physics.PolygonSizeOffset.X / 2;
                transform.Y = physicsSize.Y - physics.PolygonSizeOffset.Y / 2;
            }

            transform.Rotation = physics.Body.Rotation;
        }

        protected internal override void Run()
        {
            CurrentWorld.Step(0.016f);

            base.Run();
        }

        #region Helpers

        /// <summary>
        /// Converts the physics measurements to pixel measurements.
        /// </summary>
        public static Vector2 PhysicsToPixel(Physics.Common.Vector2 vec)
        {
            return new Vector2(vec.X, vec.Y) * Scale;
        }

        /// <summary>
        /// Converts the physics measurements to pixel measurements.
        /// </summary>
        public static Vector2 PhysicsToPixel(float x, float y)
        {
            return new Vector2((int)(x * Scale), (int)(y * Scale));
        }

        /// <summary>
        /// Converts the pixel measurements to physics measurements.
        /// </summary>
        public static float PixelToPhysics(float num)
        {
            return num * ScaleReverse;
        }

        /// <summary>
        /// Converts the pixel measurements to physics measurements.
        /// </summary>
        public static Physics.Common.Vector2 PixelToPhysics(Vector2 vec)
        {
            return new Physics.Common.Vector2(vec.X, vec.Y) * ScaleReverse;
        }

        #endregion
    }
}
