// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using OpenTK.Input;
using Soul.Engine;
using Soul.Engine.Modules;
using Soul.Engine.Scenography;

#endregion

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
            if (Input.KeyPressed(Key.A)) Console.WriteLine("Pressed!");
            if (Input.KeyHeld(Key.A)) Console.WriteLine("HELD");
            if (Input.MouseButtonPressed(MouseButton.Left)) Console.WriteLine("MOUS PRES");
            if (Input.MouseButtonHeld(MouseButton.Left)) Console.WriteLine("MAUS HELD");
            if (Input.MouseWheelScroll() != 0) Console.WriteLine(Input.MouseWheelScroll());
            Console.WriteLine(Input.MouseLocation());
        }
    }
}