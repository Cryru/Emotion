using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raya.System;

namespace SoulEngine
{
    /// <summary>
    /// Engine settings file.
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// Setup SE defaults.
        /// </summary>
        static Settings()
        {
            Title = "SoulEngine";
        }

        #region Window Settings
        public static string Title
        {
            get => RayaSettings.WTitle;
            set => RayaSettings.WTitle = value;
        }
        public static int WWidth
        {
            get => RayaSettings.WWidth;
            set => RayaSettings.WWidth = value;
        }
        public static int WHeight
        {
            get => RayaSettings.WHeight;
            set => RayaSettings.WHeight = value;
        }
        #endregion
        #region Render Settings
        public static int Width
        {
            get => RayaSettings.Width;
            set => RayaSettings.Width = value;
        }
        public static int Height
        {
            get => RayaSettings.Height;
            set => RayaSettings.Height = value;
        }
        public static bool VSync
        {
            get => RayaSettings.VSync;
            set => RayaSettings.VSync = value;
        }
        public static int FPSCap
        {
            get => RayaSettings.FPSCap;
            set => RayaSettings.FPSCap = value;
        }
        #endregion

    }
}
