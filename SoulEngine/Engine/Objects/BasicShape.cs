// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using Raya.Graphics;
using Raya.Graphics.Primitives;
using Soul.Engine.ECS;
using Soul.Engine.Enums;

#endregion

namespace Soul.Engine.Objects
{
    public class BasicShape : Actor
    {
        #region Properties

        /// <summary>
        /// The type of shape.
        /// </summary>
        public ShapeType Type
        {
            get { return _type; }
            set
            {
                _type = value;
                UpdateShapeType();
            }
        }

        private ShapeType _type = ShapeType.Rectangle;

        /// <summary>
        /// The shape's color.
        /// </summary>
        public Color Color
        {
            get { return _nativeObject.FillColor; }
            set { _nativeObject.FillColor = value; }
        }

        /// <summary>
        /// The color of the shape's outline.
        /// </summary>
        public Color OutlineColor
        {
            get { return _nativeObject.OutlineColor; }
            set { _nativeObject.OutlineColor = value; }
        }

        /// <summary>
        /// The thickness of the shape's outline.
        /// </summary>
        public float OutlineThickness
        {
            get { return _nativeObject.OutlineThickness; }
            set { _nativeObject.OutlineThickness = value; }
        }

        /// <summary>
        /// The detail to display a circle at if circle is the shape type.
        /// </summary>
        public int CircleDetail
        {
            get { return _circleDetail; }
            set
            {
                // Limit detail to 1000.
                if (value > 1000) value = 1000;

                _circleDetail = value;
                UpdateShapeType();
            }
        }

        private int _circleDetail = 30;

        /// <summary>
        /// The vertex array to use if the shape type is Polygon.
        /// </summary>
        public Vector2[] PolygonVertices
        {
            get { return _polygonVertices; }
            set
            {
                _polygonVertices = value;
                UpdateShapeType();
            }
        }

        private Vector2[] _polygonVertices;

        #endregion

        #region Raya API
        /// <summary>
        /// The shape object inside the Raya API.
        /// </summary>
        private Shape _nativeObject;

        #endregion

        public BasicShape()
        {
            _nativeObject = new Shape();
        }

        /// <summary>
        /// Create a new basic shape.
        /// </summary>
        /// <param name="type">The shape to display.</param>
        /// <param name="polygonVertices">The vertices that make up the polygon, if the shape is polygon.</param>
        public BasicShape(ShapeType type, Vector2[] polygonVertices = null) : this()
        {
            _type = type;
            _polygonVertices = polygonVertices;
        }

        public override void Initialize()
        {
            // Hook up to events modifying the parent.
            ((GameObject) Parent).onSizeChanged += UpdateShapeType;
            ((GameObject) Parent).onPositionChanged += UpdateShapeData;
            ((GameObject) Parent).onRotationChanged += UpdateShapeRotation;

            // Generate shape data.
            UpdateShapeType();
        }

        public void Destroy()
        {
            // Unhook events.
            ((GameObject)Parent).onSizeChanged -= UpdateShapeType;
            ((GameObject)Parent).onPositionChanged -= UpdateShapeData;
            ((GameObject)Parent).onRotationChanged -= UpdateShapeRotation;

            // Unhook from parent.
            int index = Parent.Children.IndexOf(this);
            if (index != -1)
            {
                Parent.RemoveChild(index);
            }      

            // Dispose the native object.
            _nativeObject.Dispose();
        }

        public override void Update()
        {
            // Draw the Raya shape.
            Core.Draw(_nativeObject);
        }

        #region Internal

        private void UpdateShapeData()
        {
            // Set the native object's position to the SE object's position minus the origin so that the object will rotate from the center but position from the top left point.
            _nativeObject.Position = (Vector2f)((GameObject)Parent).Position;
            // Except for lines and polygons, because they shouldn't have a custom origin.
            _nativeObject.Origin = _type != ShapeType.Line && _type != ShapeType.Polygon ? new Vector2f(((GameObject)Parent).Width / 2, ((GameObject)Parent).Height / 2) : new Vector2f(0, 0);
            _nativeObject.Position = _nativeObject.Position + _nativeObject.Origin;
        }

        /// <summary>
        /// Updates the native object shape's rotation according to the parent game object's rotation.
        /// </summary>
        private void UpdateShapeRotation()
        {
            _nativeObject.Rotation = ((GameObject) Parent).RotationDegree * -1;
        }

        /// <summary>
        /// Reconstructs the shape.
        /// </summary>
        private void UpdateShapeType()
        {
            // Update the shape's data.
            UpdateShapeData();

            // Generate the object based on the type.
            switch (_type)
            {
                case ShapeType.Line:
                    GenerateLine();
                    break;
                case ShapeType.Rectangle:
                    GenerateRectangle();
                    break;
                case ShapeType.Circle:
                    GenerateCircle();
                    break;
                case ShapeType.Triangle:
                    GenerateTriangle();
                    break;
                case ShapeType.Polygon:
                    GeneratePolygon();
                    break;
                default:
                    Error.Raise(3, "Unknown shape type.");
                    _nativeObject.SetPointCount(0);
                    break;
            }
        }

        #endregion

        #region Shape Templates

        /// <summary>
        /// Generates a line shape.
        /// </summary>
        private void GenerateLine()
        {
            if (_nativeObject.PointCount != 2) _nativeObject.SetPointCount(2);

            Vector2 size = ((GameObject)Parent).Size;
            Vector2 position = ((GameObject)Parent).Position;

            // Construct line points. The size will be the lines destination.
            _nativeObject.SetPoint(0, new Vector2f(0, 0));
            _nativeObject.SetPoint(1, new Vector2f(size.X - position.X, size.Y - position.Y));
        }

        /// <summary>
        /// Generates a rectangle shape.
        /// </summary>
        private void GenerateRectangle()
        {
            if (_nativeObject.PointCount != 4) _nativeObject.SetPointCount(4);

            Vector2 size = ((GameObject)Parent).Size;

            // Construct rectangle points from parent size.
            _nativeObject.SetPoint(0, new Vector2f(0, 0));
            _nativeObject.SetPoint(1, new Vector2f(size.X, 0));
            _nativeObject.SetPoint(2, new Vector2f(size.X, size.Y));
            _nativeObject.SetPoint(3, new Vector2f(0, size.Y));
        }

        /// <summary>
        /// Generates a triangle shape.
        /// </summary>
        private void GenerateTriangle()
        {
            if (_nativeObject.PointCount != 3) _nativeObject.SetPointCount(3);

            Vector2 size = ((GameObject)Parent).Size;

            // Construct triangle points from parent size.
            _nativeObject.SetPoint(0, new Vector2f(0, 0));
            _nativeObject.SetPoint(1, new Vector2f(size.X / 2f, size.Y));
            _nativeObject.SetPoint(2, new Vector2f(size.X, 0));    
        }

        /// <summary>
        /// Generates a circle shape.
        /// </summary>
        private void GenerateCircle()
        {
            if (_nativeObject.PointCount != CircleDetail) _nativeObject.SetPointCount((uint) CircleDetail);

            Vector2 size = ((GameObject)Parent).Size;

            // Get the radius of the circle from the size. Whichever size is bigger.
            float radius = Math.Max(size.X / 2, size.Y / 2);

            // Generate points.
            for (uint i = 0; i < CircleDetail; i++)
            {
                float angle = (float) (i * 2 * Math.PI / CircleDetail - Math.PI / 2);
                float x = (float) Math.Cos(angle) * radius;
                float y = (float) Math.Sin(angle) * radius;

                _nativeObject.SetPoint(i, new Vector2f(radius + x, radius + y));
            }
        }

        /// <summary>
        /// Generates a polygon shape.
        /// </summary>
        private void GeneratePolygon()
        {
            if (PolygonVertices == null) return;
            if (_nativeObject.PointCount != PolygonVertices.Length) _nativeObject.SetPointCount((uint)PolygonVertices.Length);

            for (uint i = 0; i < PolygonVertices.Length; i++)
            {
                _nativeObject.SetPoint(i, (Vector2f)PolygonVertices[i] );
            }
        }
        #endregion
    }
}