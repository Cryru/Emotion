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

        public override void Initialize()
        {
            //GameObject ceiling = new GameObject();
            //ceiling.Position = new Vector2(0, 0);
            //ceiling.Size = new Vector2(Settings.Width, 5);
            //ceiling.AddChild(new PhysicsBody(this, PhysicsShape.Rectangle));
            //ceiling.AddChild(new BasicShape(ShapeType.Rectangle));
            //ceiling.GetChild<BasicShape>().Color = Color.Black;
            //ceiling.GetChild<PhysicsBody>().SimulationType = BodyType.Static;

            //AddChild("ceiling", ceiling);

            //GameObject leftWall = new GameObject();
            //leftWall.Position = new Vector2(0, 0);
            //leftWall.Size = new Vector2(5, Settings.Height);
            //leftWall.AddChild(new PhysicsBody(this, PhysicsShape.Rectangle));
            //leftWall.AddChild(new BasicShape(ShapeType.Rectangle));
            //leftWall.GetChild<BasicShape>().Color = Color.Black;
            //leftWall.GetChild<PhysicsBody>().SimulationType = BodyType.Static;

            //AddChild("leftWall", leftWall);

            //GameObject rightWall = new GameObject();
            //rightWall.Position = new Vector2(Settings.Width - 5, 0);
            //rightWall.Size = new Vector2(5, Settings.Height);
            //rightWall.AddChild(new PhysicsBody(this, PhysicsShape.Rectangle));
            //rightWall.AddChild(new BasicShape(ShapeType.Rectangle));
            //rightWall.GetChild<BasicShape>().Color = Color.Black;
            //rightWall.GetChild<PhysicsBody>().SimulationType = BodyType.Static;

            //AddChild("rightWall", rightWall);

            GameObject floor = new GameObject();
            floor.Position = new Vector2(0, Settings.Height - 15);
            floor.Size = new Vector2(Settings.Width, 15);
            floor.AddChild(new PhysicsBody(this, PhysicsShape.Rectangle));
            floor.AddChild(new BasicShape(ShapeType.Rectangle));
            floor.GetChild<BasicShape>().Color = Color.Black;
            floor.GetChild<PhysicsBody>().SimulationType = BodyType.Static;

            AddChild("floor", floor);

            GameObject rect1 = new GameObject();
            rect1.Position = new Vector2(50, 50);
            rect1.Size = new Vector2(15, 15);
            rect1.AddChild(new PhysicsBody(this, PhysicsShape.Rectangle));
            rect1.AddChild(new BasicShape(ShapeType.Rectangle));
            rect1.GetChild<BasicShape>().Color = Color.Black;
            rect1.GetChild<PhysicsBody>().SimulationType = BodyType.Dynamic;

            AddChild("rect1", rect1);

            GameObject rect2 = new GameObject();
            rect2.Position = new Vector2(50, 100);
            rect2.Size = new Vector2(15, 15);
            rect2.AddChild(new PhysicsBody(this, PhysicsShape.Rectangle));
            rect2.AddChild(new BasicShape(ShapeType.Rectangle));
            rect2.GetChild<BasicShape>().Color = Color.Black;
            rect2.GetChild<PhysicsBody>().SimulationType = BodyType.Dynamic;

            AddChild("rect2", rect2);

            //Vector2[] vert = {new Vector2(27, -32), new Vector2(-27, -35), new Vector2(3, 22)};

            //GameObject physicsObject = new GameObject();
            //physicsObject.Position = new Vector2(50, 50);
            //physicsObject.Size = new Vector2(50, 50);
            //physicsObject.AddChild(new PhysicsBody(this, PhysicsShape.Polygon, vert));
            //physicsObject.AddChild(new BasicShape(ShapeType.Polygon, vert));
            //physicsObject.GetChild<PhysicsBody>().SimulationType = BodyType.Dynamic;

            //AddChild("physicsTest", physicsObject);
        }

        public override void Update()
        {
            if (Input.MouseButtonHeld(Mouse.Button.Left))
            {
                GameObject temp = new GameObject();
                temp.Position = Input.MousePosition;
                temp.Size = new Vector2(10, 10);
                temp.AddChild(new PhysicsBody(this, PhysicsShape.Circle));
                temp.AddChild(new BasicShape(ShapeType.Circle));
                temp.GetChild<PhysicsBody>().SimulationType = BodyType.Dynamic;

                AddChild(temp);
            }
        }
    }
}