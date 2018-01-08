using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Soul.Engine;
using Soul.Engine.Modules;
using Soul.Engine.Scenography;

namespace Examples.Basic
{
    public class InputTest : Scene
    {
        public static void Main()
        {
            Core.Setup(new InputTest());
        }

        protected override void Setup()
        {
            
        }

        protected override void Update()
        {
            if (Input.KeyPressed(OpenTK.Input.Key.A)) Console.WriteLine("Pressed!");
            if (Input.KeyHeld(OpenTK.Input.Key.A)) Console.WriteLine("HELD");
            if (Input.MouseButtonPressed(OpenTK.Input.MouseButton.Left)) Console.WriteLine("MOUS PRES");
            if (Input.MouseButtonHeld(OpenTK.Input.MouseButton.Left)) Console.WriteLine("MAUS HELD");
            if (Input.MouseWheelScroll() != 0) Console.WriteLine(Input.MouseWheelScroll());
            Console.WriteLine(Input.MouseLocation());
        }
    }
}
