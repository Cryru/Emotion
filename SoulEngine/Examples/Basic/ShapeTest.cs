using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raya.Graphics.Primitives;
using Soul.Engine;
using Soul.Engine.Enums;
using Soul.Engine.Objects;

namespace Soul.Examples.Basic
{
    public class ShapeTest : Scene
    {

        public static void Main(string[] args)
        {
            // Start the engine.
            Core.Start(new ShapeTest());
        }

        public override void Initialize()
        {
            GameObject line = new GameObject();
            line.AddChild("line", new BasicShape());
            line.GetChild<BasicShape>("line").Type = ShapeType.Line;
            line.Size = new Vector2(50, 50);
            line.Position = new Vector2(100, 50);

            AddChild("line", line);

        }

        public override void Update()
        {

        }
    }
}
