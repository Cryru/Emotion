// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
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
            if (Input.KeyPressed(OpenTK.Input.Key.A)) Console.WriteLine("Pressed A!");
            if (Input.KeyHeld(OpenTK.Input.Key.A)) Console.WriteLine("Holding A...");
            if (Input.MouseButtonPressed(OpenTK.Input.MouseButton.Left)) Console.WriteLine("Mouse Pressed");
            if (Input.MouseButtonHeld(OpenTK.Input.MouseButton.Left)) Console.WriteLine("Mouse Held");
            if (Input.MouseWheelScroll() != 0) Console.WriteLine("Scroll Value: " + Input.MouseWheelScroll());
            if (Input.KeyPressed(OpenTK.Input.Key.Space)) Console.WriteLine("Mouse is at: " + Input.MouseLocation());
        }
    }
}