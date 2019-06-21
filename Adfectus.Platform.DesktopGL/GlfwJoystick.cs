#region Using

using System;
using Adfectus.Common;
using Adfectus.Input;

#endregion

namespace Adfectus.Platform.DesktopGL
{
    /// <summary>
    /// A joystick loaded by glfw.
    /// </summary>
    public sealed class GlfwJoystick : Joystick
    {
        /// <summary>
        /// A joystick loaded by glfw.
        /// </summary>
        /// <param name="id">The id of the joystick.</param>
        /// <param name="name">The name of the joystick.</param>
        public GlfwJoystick(int id, string name) : base(id, name)
        {
        }

        public override float GetAxis(JoystickAxis axis)
        {
            float[] a = Glfw.GetJoystickAxes(Id, out int count);
            if (count == 0) return 0;
            return a[(int) axis];
        }

        public override bool GetKeyDown(JoystickButton b)
        {
            byte[] c = Glfw.GetJoystickButtons(Id, out int count);
            if (count == 0) return false;
            return c[(int) b] == 1;
        }
    }
}