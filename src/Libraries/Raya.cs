using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Libraries
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    // Using Raya: https://github.com/Cryru/Raya                                //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// A C# wrapper for Raya.
    /// </summary>
    public static class Raya
    {
        #region Raya Imports
        [DllImport("Raya.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Initialize(bool Debugging = false, int OpenGLMajor = 3, int OpenGLMinor = 0);

        [DllImport("Raya.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Quit(int ExitCode = 0);

        [DllImport("Raya.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateWindow(string Name, int WindowWidth, int WindowHeight);

        [DllImport("Raya.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern Event ProcessNextEvent(); // Managed by a wrapper - Tick()
        #endregion

        #region Callback Wrappers
        /// <summary>
        /// The callback for errors.
        /// </summary>
        /// <param name="Error">The error string.</param>
        private static void ErrorCallBack(string Description, string Error)
        {
            Console.WriteLine(Description + " > " + Error);
        }
        #endregion
        #region Function Wrappers
        /// <summary>
        /// Runs Raya processes like events, etc.
        /// </summary>
        public static void Tick()
        {
            Event a = ProcessNextEvent();

            //switch (a)
            //{
            //    case "Quit":
            //        Quit(0);
            //        break;
            //}

            Console.WriteLine(a);
        }
        #endregion
        #region Structures
        [StructLayout(LayoutKind.Sequential)]
        public struct Event
        {
            public EventType Type;
            public uint TimeStamp;
            public uint WindowID;
            public EventId EventID;
            private byte padding1;
            private byte padding2;
            private byte padding3;
            public int Data1;
            public int Data2;
        }
        public enum EventId : byte
        {
            None,
            Shown,
            Hidden,
            Exposed,
            Moved,
            Resized,
            SizeChanged,
            Minimized,
            Maximized,
            Restored,
            Enter,
            Leave,
            FocusGained,
            FocusLost,
            Close,
        }
        public enum EventType : uint
        {
            First = 0,

            Quit = 0x100,

            WindowEvent = 0x200,
            SysWM = 0x201,

            KeyDown = 0x300,
            KeyUp = 0x301,
            TextEditing = 0x302,
            TextInput = 0x303,

            MouseMotion = 0x400,
            MouseButtonDown = 0x401,
            MouseButtonup = 0x402,
            MouseWheel = 0x403,

            JoyAxisMotion = 0x600,
            JoyBallMotion = 0x601,
            JoyHatMotion = 0x602,
            JoyButtonDown = 0x603,
            JoyButtonUp = 0x604,
            JoyDeviceAdded = 0x605,
            JoyDeviceRemoved = 0x606,

            ControllerAxisMotion = 0x650,
            ControllerButtonDown = 0x651,
            ControllerButtonUp = 0x652,
            ControllerDeviceAdded = 0x653,
            ControllerDeviceRemoved = 0x654,
            ControllerDeviceRemapped = 0x654,

            FingerDown = 0x700,
            FingerUp = 0x701,
            FingerMotion = 0x702,

            DollarGesture = 0x800,
            DollarRecord = 0x801,
            MultiGesture = 0x802,

            ClipboardUpdate = 0x900,

            DropFile = 0x1000,

            AudioDeviceAdded = 0x1100,
            AudioDeviceRemoved = 0x1101,

            RenderTargetsReset = 0x2000,
            RenderDeviceReset = 0x2001,

            UserEvent = 0x8000,

            Last = 0xFFFF
        }
        #endregion
        #region Events

        #endregion
    }
}
