// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using Breath.Graphics;
using Examples.Systems;
using OpenTK;
using OpenTK.Input;
using Soul.Engine;
using Soul.Engine.ECS;
using Soul.Engine.ECS.Components;
using Soul.Engine.ECS.Systems;
using Soul.Engine.Enums;
using Soul.Engine.Graphics.Components;
using Soul.Engine.Modules;
using Soul.Engine.Scenography;
using Soul.Physics.Dynamics;

#endregion

namespace Examples.Basic
{
    public class PhysicsTest : Scene
    {
        public static void Main()
        {
            Core.Setup(new PhysicsTest());
        }

        #region Declarations

        private PhysicsShapeType _currentShape = PhysicsShapeType.Rectangle;
        private int _currentSize = 10;
        private int _entityCounter;

        #endregion

        protected override void Setup()
        {
            Entity ceiling = Entity.CreateBasicDrawable("ceiling");
            ceiling.GetComponent<Transform>().Position = new Vector2(0, 0);
            ceiling.GetComponent<Transform>().Size = new Vector2(Settings.Width, 5);
            ceiling.GetComponent<RenderData>().Color = Color.Black;
            ceiling.AttachComponent<PhysicsObject>();
            ceiling.GetComponent<PhysicsObject>().SimulationType = BodyType.Static;

            AddEntity(ceiling);

            Entity leftWall = Entity.CreateBasicDrawable("leftWall");
            leftWall.GetComponent<Transform>().Position = new Vector2(0, 0);
            leftWall.GetComponent<Transform>().Size = new Vector2(5, Settings.Height);
            leftWall.GetComponent<RenderData>().Color = Color.Black;
            leftWall.AttachComponent<PhysicsObject>();
            leftWall.GetComponent<PhysicsObject>().SimulationType = BodyType.Static;

            AddEntity(leftWall);

            Entity rightWall = Entity.CreateBasicDrawable("rightWall");
            rightWall.GetComponent<Transform>().Position = new Vector2(Settings.Width - 5, 0);
            rightWall.GetComponent<Transform>().Size = new Vector2(5, Settings.Height);
            rightWall.GetComponent<RenderData>().Color = Color.Black;
            rightWall.AttachComponent<PhysicsObject>();
            rightWall.GetComponent<PhysicsObject>().SimulationType = BodyType.Static;

            AddEntity(rightWall);

            Entity floor = Entity.CreateBasicDrawable("floor");
            floor.GetComponent<Transform>().Position = new Vector2(0, Settings.Height - 15);
            floor.GetComponent<Transform>().Size = new Vector2(Settings.Width, 15);
            floor.GetComponent<RenderData>().Color = Color.Black;
            floor.AttachComponent<PhysicsObject>();
            floor.GetComponent<PhysicsObject>().SimulationType = BodyType.Static;

            AddEntity(floor);

            Vector2[] customPoly =
            {
                new Vector2(9, -9),
                new Vector2(15, 7),
                new Vector2(2, 16),
                new Vector2(-6, 19),
                new Vector2(-19, 5),
                new Vector2(-20, -4),
                new Vector2(-11, -14),
                new Vector2(-4, -15),
                new Vector2(4, -15)
            };

            Entity poly = new Entity("poly");
            poly.AttachComponent<Transform>();
            poly.GetComponent<Transform>().Position = new Vector2(450, 50);
            poly.GetComponent<Transform>().Size = new Vector2(1, 1);
            poly.AttachComponent<RenderData>();
            poly.GetComponent<RenderData>().UpdateVertices();
            poly.GetComponent<RenderData>().SetVertices(customPoly);
            poly.GetComponent<RenderData>().Color = new Color(255, 0, 0);
            poly.AttachComponent<PhysicsObject>();
            poly.GetComponent<PhysicsObject>().Shape = PhysicsShapeType.Polygon;
            poly.GetComponent<PhysicsObject>().PolygonVertices = customPoly;

            AddEntity(poly);

            Entity mouseIndicator = Entity.CreateBasicDrawable("mouseIndicator");
            mouseIndicator.GetComponent<Transform>().Position = new Vector2(0, Settings.Height - 15);
            mouseIndicator.GetComponent<Transform>().Size = new Vector2(Settings.Width, 15);
            mouseIndicator.GetComponent<RenderData>().Color = new Color(255, 0, 0, 100);
            mouseIndicator.GetComponent<RenderData>().Priority = 100;

            AddEntity(mouseIndicator);

            AddSystem(new PhysicsEngine(PhysicsEngine.DefaultGravity));
            AddSystem(new TransformCleanup());
        }

        protected override void Update()
        {
            // Decide shape based on input.
            bool add = true;
            PhysicsShapeType newShape = _currentShape;

            if (Input.MouseButtonHeld(MouseButton.Left))
                newShape = PhysicsShapeType.Rectangle;
            else if (Input.MouseButtonHeld(MouseButton.Right))
                newShape = PhysicsShapeType.Circle;
            else
                add = false;

            // Check if size changed.
            _currentSize += Input.MouseWheelScroll();

            // Clamp.
            if (_currentSize > 100) _currentSize = 100;
            else if (_currentSize < 10)
                _currentSize = 10;

            // Update mouse indicator.
            GetEntity("mouseIndicator").GetComponent<Transform>().Center = Input.MouseLocation();
            GetEntity("mouseIndicator").GetComponent<Transform>().Size = new Vector2(_currentSize, _currentSize);

            if (newShape != _currentShape)
            {
                switch (newShape)
                {
                    case PhysicsShapeType.Rectangle:
                        GetEntity("mouseIndicator").GetComponent<RenderData>().ApplyTemplate_Rectangle();
                        break;
                    case PhysicsShapeType.Circle:
                        GetEntity("mouseIndicator").GetComponent<RenderData>().ApplyTemplate_Circle();
                        break;
                }

                _currentShape = newShape;
            }

            // If clicked produce shapes.
            if (!add) return;
            Entity spawnedEntity = new Entity("spawnedEntity" + _entityCounter);
            spawnedEntity.AttachComponent<Transform>();
            spawnedEntity.GetComponent<Transform>().Size = new Vector2(_currentSize, _currentSize);
            spawnedEntity.GetComponent<Transform>().Center =
                GetEntity("mouseIndicator").GetComponent<Transform>().Center;
            spawnedEntity.AttachComponent<RenderData>();
            switch (_currentShape)
            {
                case PhysicsShapeType.Rectangle:
                    spawnedEntity.GetComponent<RenderData>().ApplyTemplate_Rectangle();
                    break;
                case PhysicsShapeType.Circle:
                    spawnedEntity.GetComponent<RenderData>().ApplyTemplate_Circle();
                    break;
            }

            spawnedEntity.GetComponent<RenderData>().Color = Color.White;
            spawnedEntity.AttachComponent<PhysicsObject>();
            spawnedEntity.GetComponent<PhysicsObject>().SimulationType = BodyType.Dynamic;
            spawnedEntity.GetComponent<PhysicsObject>().Shape = _currentShape;

            AddEntity(spawnedEntity);

            _entityCounter++;
        }
    }
}