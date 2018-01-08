// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using Breath.Graphics;
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
using Soul.Physics.Collision.Shapes;
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

        //private ShapeType _currentShape = ShapeType.Rectangle;
        //private int _currentSize = 10;

        #endregion

        protected override void Setup()
        {
            Entity ceiling = new Entity("ceiling");
            ceiling.AttachComponent<Transform>();
            ceiling.GetComponent<Transform>().Position = new Vector2(0, 0);
            ceiling.GetComponent<Transform>().Size = new Vector2(Settings.Width, 5);
            ceiling.AttachComponent<RenderData>();
            ceiling.GetComponent<RenderData>().ApplyTemplate_Rectangle();
            ceiling.GetComponent<RenderData>().Color = Color.Black;
            ceiling.AttachComponent<PhysicsObject>();
            ceiling.GetComponent<PhysicsObject>().SimulationType = BodyType.Static;

            AddEntity(ceiling);

            Entity leftWall = new Entity("leftWall");
            leftWall.AttachComponent<Transform>();
            leftWall.GetComponent<Transform>().Position = new Vector2(0, 0);
            leftWall.GetComponent<Transform>().Size = new Vector2(5, Settings.Height);
            leftWall.AttachComponent<RenderData>();
            leftWall.GetComponent<RenderData>().ApplyTemplate_Rectangle();
            leftWall.GetComponent<RenderData>().Color = Color.Black;
            leftWall.AttachComponent<PhysicsObject>();
            leftWall.GetComponent<PhysicsObject>().SimulationType = BodyType.Static;

            AddEntity(leftWall);

            Entity rightWall = new Entity("rightWall");
            rightWall.AttachComponent<Transform>();
            rightWall.GetComponent<Transform>().Position = new Vector2(Settings.Width - 5, 0);
            rightWall.GetComponent<Transform>().Size = new Vector2(5, Settings.Height);
            rightWall.AttachComponent<RenderData>();
            rightWall.GetComponent<RenderData>().ApplyTemplate_Rectangle();
            rightWall.GetComponent<RenderData>().Color = Color.Black;
            rightWall.AttachComponent<PhysicsObject>();
            rightWall.GetComponent<PhysicsObject>().SimulationType = BodyType.Static;

            AddEntity(rightWall);

            Entity floor = new Entity("floor");
            floor.AttachComponent<Transform>();
            floor.GetComponent<Transform>().Position = new Vector2(0, Settings.Height - 15);
            floor.GetComponent<Transform>().Size = new Vector2(Settings.Width, 15);
            floor.AttachComponent<RenderData>();
            floor.GetComponent<RenderData>().ApplyTemplate_Rectangle();
            floor.GetComponent<RenderData>().Color = Color.Black;
            floor.AttachComponent<PhysicsObject>();
            floor.GetComponent<PhysicsObject>().SimulationType = BodyType.Static;

            AddEntity(floor);

            Vector2[] vert = { new Vector2(16, 43), new Vector2(12, -15), new Vector2(-10, -2) };

            Entity poly = new Entity("poly");
            poly.AttachComponent<Transform>();
            poly.GetComponent<Transform>().Position = new Vector2(450, 50);
            poly.GetComponent<Transform>().Size = new Vector2(1, 1);
            poly.AttachComponent<RenderData>();
            poly.GetComponent<RenderData>().SetVertices(vert);
            poly.GetComponent<RenderData>().Color = new Color(255, 0, 0);
            poly.AttachComponent<PhysicsObject>();
            poly.GetComponent<PhysicsObject>().Shape = PhysicsShapeType.Polygon;
            poly.GetComponent<PhysicsObject>().PolygonVertices = vert;

            AddEntity(poly);

            AddSystem(new PhysicsEngine(PhysicsEngine.DefaultGravity));

            //GameObject mouseIndicator = new GameObject();
            //mouseIndicator.AddChild(new BasicShape(ShapeType.Rectangle));
            //mouseIndicator.GetChild<BasicShape>().Color = new Color(255, 255, 255, 100);
            //mouseIndicator.GetChild<BasicShape>().OutlineColor = new Color(255, 0, 0, 200);
            //mouseIndicator.GetChild<BasicShape>().OutlineThickness = 2;
            //mouseIndicator.Priority = 1;

            //AddChild("mouseIndicator", mouseIndicator);
        }

        protected override void Update()
        {
            //// Decide shape.
            //bool add = true;
            //if (Input.MouseButtonHeld(Mouse.Button.Left))
            //    _currentShape = ShapeType.Rectangle;
            //else if (Input.MouseButtonHeld(Mouse.Button.Right))
            //    _currentShape = ShapeType.Circle;
            //else
            //    add = false;

            //// Check if size changed.
            //_currentSize += Input.MouseWheelScroll();

            //// Clamp.
            //if (_currentSize > 100) _currentSize = 100;
            //else if (_currentSize < 10) _currentSize = 10;

            //// Update mouse indicator.
            //GetChild<GameObject>("mouseIndicator").Center = Input.MousePosition;
            //GetChild<GameObject>("mouseIndicator").Size = new Vector2(_currentSize, _currentSize);
            //GetChild<GameObject>("mouseIndicator").GetChild<BasicShape>().Type = _currentShape;

            //// If clicked produce shapes.
            //if (add)
            //{
            //    GameObject temp = new GameObject();
            //    temp.Position = GetChild<GameObject>("mouseIndicator").Position;
            //    temp.Size = new Vector2(_currentSize, _currentSize);
            //    temp.AddChild(new PhysicsBody(this, _currentShape));
            //    temp.AddChild(new BasicShape(_currentShape));
            //    temp.GetChild<PhysicsBody>().SimulationType = BodyType.Dynamic;
            //    temp.GetChild<BasicShape>().OutlineColor = Color.Black;
            //    temp.GetChild<BasicShape>().OutlineThickness = 1;

            //    AddChild(temp);
            //}
        }
    }
}