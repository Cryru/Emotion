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

        public override void GetAxis()
        {
      
        }

        public override void GetKeyDown()
        {
            byte[] c = Glfw.GetJoystickButtons(Id, out int buttons);
            //Engine.Log.Info(string.Join("|", c), Logging.MessageSource.Other);
            for (int i = 0; i < buttons; i++)
            {
                
            }
        }
    }
}