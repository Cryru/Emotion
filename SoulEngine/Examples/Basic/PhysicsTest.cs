// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using Raya.Graphics;
using Raya.Graphics.Primitives;
using Raya.Input;
using Soul.Engine;
using Soul.Engine.Enums;
using Soul.Engine.Modules;
using Soul.Engine.Objects;
using Soul.Physics;
using Soul.Physics.Dynamics;
using Settings = Raya.System.Settings;
using ShapeType = Soul.Engine.Enums.ShapeType;

#endregion

namespace Soul.Examples.Basic
{
    public class PhysicsTest : Scene
    {
        public static void Main()
        {
            Core.Start(new PhysicsTest(), "physicsTest");
        }

        #region Declarations

        private ShapeType _currentShape = ShapeType.Rectangle;
        private int _currentSize = 10;
#endregion

        public override void Initialize()
        {
            GameObject ceiling = new GameObject();
            ceiling.Position = new Vector2(0, 0);
            ceiling.Size = new Vector2(Settings.Width, 5);
            ceiling.AddChild(new PhysicsBody(this, ShapeType.Rectangle));
            ceiling.AddChild(new BasicShape(ShapeType.Rectangle));
            ceiling.GetChild<BasicShape>().Color = Color.Black;
            ceiling.GetChild<PhysicsBody>().SimulationType = BodyType.Static;

            AddChild("ceiling", ceiling);

            GameObject leftWall = new GameObject();
            leftWall.Position = new Vector2(0, 0);
            leftWall.Size = new Vector2(5, Settings.Height);
            leftWall.AddChild(new PhysicsBody(this, ShapeType.Rectangle));
            leftWall.AddChild(new BasicShape(ShapeType.Rectangle));
            leftWall.GetChild<BasicShape>().Color = Color.Black;
            leftWall.GetChild<PhysicsBody>().SimulationType = BodyType.Static;

            AddChild("leftWall", leftWall);

            GameObject rightWall = new GameObject();
            rightWall.Position = new Vector2(Settings.Width - 5, 0);
            rightWall.Size = new Vector2(5, Settings.Height);
            rightWall.AddChild(new PhysicsBody(this, ShapeType.Rectangle));
            rightWall.AddChild(new BasicShape(ShapeType.Rectangle));
            rightWall.GetChild<BasicShape>().Color = Color.Black;
            rightWall.GetChild<PhysicsBody>().SimulationType = BodyType.Static;

            AddChild("rightWall", rightWall);

            GameObject floor = new GameObject();
            floor.Position = new Vector2(0, Settings.Height - 15);
            floor.Size = new Vector2(Settings.Width, 15);
            floor.AddChild(new PhysicsBody(this, ShapeType.Rectangle));
            floor.AddChild(new BasicShape(ShapeType.Rectangle));
            floor.GetChild<BasicShape>().Color = Color.Black;
            floor.GetChild<PhysicsBody>().SimulationType = BodyType.Static;

            AddChild("floor", floor);

            Vector2[] vert = { new Vector2(16, 43), new Vector2(12, -15), new Vector2(-10, -2) };

            GameObject polygon = new GameObject();
            polygon.Position = new Vector2(50, 50);
            polygon.Size = new Vector2(50, 50);
            polygon.AddChild(new PhysicsBody(this, ShapeType.Polygon, vert));
            polygon.AddChild(new BasicShape(ShapeType.Polygon, vert));
            polygon.GetChild<PhysicsBody>().SimulationType = BodyType.Dynamic;
            polygon.GetChild<BasicShape>().Color = new Raya.Graphics.Color(255, 0, 0);

            AddChild("polygon", polygon);

            GameObject mouseIndicator = new GameObject();
            mouseIndicator.AddChild(new BasicShape(ShapeType.Rectangle));
            mouseIndicator.GetChild<BasicShape>().Color = new Color(255, 255, 255, 100);
            mouseIndicator.GetChild<BasicShape>().OutlineColor = new Raya.Graphics.Color(255, 0, 0, 200);
            mouseIndicator.GetChild<BasicShape>().OutlineThickness = 2;

            AddChild("mouseIndicator", mouseIndicator);
        }

        public override void Update()
        {
            // Decide shape.
            bool add = true;
            if (Input.MouseButtonHeld(Mouse.Button.Left))
            {
                _currentShape = ShapeType.Rectangle;
            }
            else if (Input.MouseButtonHeld(Mouse.Button.Right))
            {
                _currentShape = ShapeType.Circle;
            }
            else
            {
                add = false;
            }

            // Check if size changed.
            _currentSize += Input.MouseWheelScroll();

            // Clamp.
            if (_currentSize > 100) _currentSize = 100;
            else if (_currentSize < 10) _currentSize = 10;

            // Update mouse indicator.
            GetChild<GameObject>("mouseIndicator").Center = Input.MousePosition;
            GetChild<GameObject>("mouseIndicator").Size = new Vector2(_currentSize, _currentSize);
            GetChild<GameObject>("mouseIndicator").GetChild<BasicShape>().Type = _currentShape;

            // If clicked produce shapes.
            if (add)
            {
                GameObject temp = new GameObject();
                temp.Position = GetChild<GameObject>("mouseIndicator").Position;
                temp.Size = new Vector2(_currentSize, _currentSize);
                temp.AddChild(new PhysicsBody(this, _currentShape));
                temp.AddChild(new BasicShape(_currentShape));
                temp.GetChild<PhysicsBody>().SimulationType = BodyType.Dynamic;
                temp.GetChild<BasicShape>().OutlineColor = Color.Black;
                temp.GetChild<BasicShape>().OutlineThickness = 1;

                AddChild(temp);
            }
        }
    }
}