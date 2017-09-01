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
                _circleDetail = value;
                UpdateShapeType();
            }
        }

        private int _circleDetail = 30;

        #endregion

        #region Raya API

        private Shape _nativeObject;

        #endregion

        public BasicShape()
        {
            _nativeObject = new Shape();
        }

        public BasicShape(ShapeType type) : this()
        {
            _type = type;
        }

        public override void Initialize()
        {
            // If the parent is a game object we want to hook up to the size changed event.
            if (Parent is GameObject convParent)
            {
                convParent.onSizeChanged += UpdateShapeType;
                convParent.onPositionChanged += UpdateShapePosition;
                convParent.onRotationChanged += UpdateShapeRotation;
            }

            // Generate shape data.
            UpdateShapeType();
            ;
        }

        ~BasicShape()
        {
            Console.WriteLine("DESTROYED");

            // If the parent is a game object free the event.
            if (Parent is GameObject convParent)
            {
                convParent.onSizeChanged -= UpdateShapeType;
                convParent.onPositionChanged -= UpdateShapePosition;
                convParent.onRotationChanged += UpdateShapeRotation;
            }
        }

        public override void Update()
        {
            // Draw the Raya shape.
            Core.Draw(_nativeObject);
        }

        #region Internal

        /// <summary>
        /// Updates the native object shape's rotation according to the parent game object's rotation.
        /// </summary>
        private void UpdateShapeRotation()
        {
            _nativeObject.Rotation = ((GameObject) Parent).Rotation;
        }

        /// <summary>
        /// Updates the native object shape's position according to the parent game object's position.
        /// </summary>
        private void UpdateShapePosition()
        {
            _nativeObject.Position = (Vector2f) ((GameObject) Parent).Position;
        }

        /// <summary>
        /// Reconstructs the shape.
        /// </summary>
        private void UpdateShapeType()
        {
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

            Vector2 size = new Vector2(10, 10);
            if (Parent is GameObject convParent) size = convParent.Size;

            // Construct line points. The size will be the lines destination.
            _nativeObject.SetPoint(0, new Vector2f(0, 0));
            _nativeObject.SetPoint(1, new Vector2f(size.X, size.Y));
        }

        /// <summary>
        /// Generates a rectangle shape.
        /// </summary>
        private void GenerateRectangle()
        {
            if (_nativeObject.PointCount != 4) _nativeObject.SetPointCount(4);

            Vector2 size = new Vector2(10, 10);
            if (Parent is GameObject convParent) size = convParent.Size;

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

            Vector2 size = new Vector2(10, 10);
            if (Parent is GameObject convParent) size = convParent.Size;

            // Construct triangle points from parent size.
            _nativeObject.SetPoint(0, new Vector2f(size.X / 2f * -1, 0));
            _nativeObject.SetPoint(1, new Vector2f(0, size.Y));
            _nativeObject.SetPoint(2, new Vector2f(size.X / 2f, 0));
        }

        /// <summary>
        /// Generates a circle shape.
        /// </summary>
        private void GenerateCircle()
        {
            if (_nativeObject.PointCount != CircleDetail) _nativeObject.SetPointCount((uint) CircleDetail);

            Vector2 size = new Vector2(10, 10);
            if (Parent is GameObject convParent) size = convParent.Size;

            // Get the radius of the circle from the size. Whichever size is bigger.
            float radius = Math.Max(size.X, size.Y);

            // Generate points.
            for (uint i = 0; i < CircleDetail; i++)
            {
                float angle = (float) (i * 2 * Math.PI / CircleDetail - Math.PI / 2);
                float x = (float) Math.Cos(angle) * radius;
                float y = (float) Math.Sin(angle) * radius;

                _nativeObject.SetPoint(i, new Vector2f(radius + x, radius + y));
            }
        }

        #endregion
    }
}