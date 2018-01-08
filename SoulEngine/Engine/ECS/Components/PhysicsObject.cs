// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using OpenTK;
using Soul.Engine.Enums;
using Soul.Physics.Dynamics;

#endregion

namespace Soul.Engine.ECS.Components
{
    public class PhysicsObject : ComponentBase
    {
        /// <summary>
        /// The simulation type of the object.
        /// </summary>
        public BodyType SimulationType
        {
            get { return _simulationType; }
            set
            {
                _simulationType = value;

                // If the body is initialized change the value.
                if (Body != null) Body.BodyType = value;
            }
        }

        private BodyType _simulationType = BodyType.Dynamic;

        /// <summary>
        /// The object's physics body.
        /// </summary>
        public Body Body;

        /// <summary>
        /// The body's shape template.
        /// </summary>
        public PhysicsShapeType Shape
        {
            get { return _shape; }
            set
            {
                HasUpdated = true;
                _shape = value;
            }
        }

        private PhysicsShapeType _shape = PhysicsShapeType.Rectangle;

        /// <summary>
        /// The vertices to be used if the shape type is polygon.
        /// </summary>
        public Vector2[] PolygonVertices;

        /// <summary>
        /// The offset size of the polygon vertices.
        /// </summary>
        public Vector2 PolygonSizeOffset;
    }
}