#region Using

using System;
using System.Collections.Generic;
using Emotion.Common;
using Emotion.Platform.Implementation.EglAngle.Native;
using Emotion.Platform.RenderDoc;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Platform.Implementation.EglAngle
{
    public class EglGraphicsContext : RenderDocGraphicsContext
    {
        private IntPtr _display;
        private IntPtr _surface;
        private IntPtr _context;
        private IntPtr _eglLib;
        private PlatformBase _platform;

        public void Init(IntPtr nativeDeviceHandle, IntPtr nativeWindowHandle, PlatformBase platform)
        {
            _platform = platform;
            _eglLib = platform.LoadLibrary("libEGL");
            if (_eglLib == IntPtr.Zero)
            {
                Engine.Log.Error("Couldn't load EGL.", MessageSource.Egl);
                return;
            }

            IntPtr gles = platform.LoadLibrary("libGLESv2");
            if (gles == IntPtr.Zero)
            {
                Engine.Log.Error("Couldn't load GLES.", MessageSource.Egl);
                return;
            }

            string extensions = Egl.QueryString(IntPtr.Zero, Egl.Query.EGL_EXTENSIONS);
            Egl.Error err = Egl.GetError();
            if (extensions == null || err != Egl.Error.Success)
            {
                Engine.Log.Error($"Couldn't load extensions. {err}", MessageSource.Egl);
                extensions = "";
            }

            _display = Egl.GetDisplay(nativeDeviceHandle);
            if (_display == IntPtr.Zero)
            {
                Engine.Log.Error($"Couldn't initialize display. {Egl.GetError()}", MessageSource.Egl);
                return;
            }

            var majorVer = 3;
            var minorVer = 0;
            if (RenderDoc.Loaded) minorVer = 1;
            if (!Egl.Init(_display, ref majorVer, ref minorVer))
            {
                Engine.Log.Error($"Couldn't initialize Egl. {Egl.GetError()}", MessageSource.Egl);
                return;
            }

            // Pick config
            var totalConfigs = 0;
            Egl.GetConfigs(_display, null, 0, ref totalConfigs);
            if (totalConfigs == 0)
            {
                Engine.Log.Error($"No configs for current display. {Egl.GetError()}", MessageSource.Egl);
                return;
            }

            var configs = new IntPtr[totalConfigs];
            Egl.GetConfigs(_display, configs, totalConfigs, ref totalConfigs);
            if (totalConfigs == 0)
            {
                Engine.Log.Error($"No configs for current display. {Egl.GetError()}", MessageSource.Egl);
                return;
            }

            int configHandle = SupportedPixelFormat(configs);
            if (configHandle == 0)
            {
                Engine.Log.Error("No valid config found.", MessageSource.Egl);
                return;
            }

            IntPtr config = configs[configHandle - 1];

            if (!Egl.BindAPI(Egl.API.EGL_OPENGL_ES_API))
            {
                Engine.Log.Error($"Couldn't bind EGL API. {Egl.GetError()}", MessageSource.Egl);
                return;
            }

            int[] attributes =
            {
                (int) Egl.ContextAttribute.EGL_CONTEXT_CLIENT_VERSION,
                3,
                0x3038 // EGL_NONE
            };

            _context = Egl.CreateContext(_display, config, IntPtr.Zero, attributes);
            if (_context == IntPtr.Zero)
            {
                Engine.Log.Error($"Context creation failed. {Egl.GetError()}", MessageSource.Egl);
                return;
            }

            _surface = Egl.CreateWindowSurface(_display, config, nativeWindowHandle, null);
            if (_surface == IntPtr.Zero)
            {
                Engine.Log.Error($"Surface creation failed. {Egl.GetError()}", MessageSource.Egl);
                return;
            }

            Valid = true;
        }

        /// <summary>
        /// Finds the index of the supported pixel format closest to the requested pixel format.
        /// </summary>
        /// <returns>The index of the pixel format to use.</returns>
        private int SupportedPixelFormat(IntPtr[] configs)
        {
            var usableConfigs = new List<FramebufferConfig>();
            var attributeValue = 0;

            for (var i = 0; i < configs.Length; i++)
            {
                IntPtr currentConfig = configs[i];

                // We only want RGB buffers.
                if (!Egl.GetConfigAttribute(_display, currentConfig, Egl.ConfigAttribute.EGL_COLOR_BUFFER_TYPE, ref attributeValue)) continue;
                if (attributeValue != (int) Egl.ConfigValue.EGL_RGB_BUFFER) continue;

                // We only want window type configs.
                // EGL_WINDOW_BIT 0x0004
                Egl.GetConfigAttribute(_display, currentConfig, Egl.ConfigAttribute.EGL_SURFACE_TYPE, ref attributeValue);
                if ((attributeValue & 0x0004) == 0) continue;

                // We want configs that support ES2 (We actually want ES3, but there's no such bit)
                // EGL_OPENGL_ES2_BIT 0x0004
                Egl.GetConfigAttribute(_display, currentConfig, Egl.ConfigAttribute.EGL_RENDERABLE_TYPE, ref attributeValue);
                if ((attributeValue & 0x0004) == 0) continue;

                var fbConfig = new FramebufferConfig();

                Egl.GetConfigAttribute(_display, currentConfig, Egl.ConfigAttribute.EGL_RED_SIZE, ref attributeValue);
                fbConfig.RedBits = attributeValue;

                Egl.GetConfigAttribute(_display, currentConfig, Egl.ConfigAttribute.EGL_GREEN_SIZE, ref attributeValue);
                fbConfig.GreenBits = attributeValue;

                Egl.GetConfigAttribute(_display, currentConfig, Egl.ConfigAttribute.EGL_BLUE_SIZE, ref attributeValue);
                fbConfig.BlueBits = attributeValue;

                Egl.GetConfigAttribute(_display, currentConfig, Egl.ConfigAttribute.EGL_ALPHA_SIZE, ref attributeValue);
                fbConfig.AlphaBits = attributeValue;

                Egl.GetConfigAttribute(_display, currentConfig, Egl.ConfigAttribute.EGL_DEPTH_SIZE, ref attributeValue);
                fbConfig.DepthBits = attributeValue;

                Egl.GetConfigAttribute(_display, currentConfig, Egl.ConfigAttribute.EGL_STENCIL_SIZE, ref attributeValue);
                fbConfig.StencilBits = attributeValue;

                Egl.GetConfigAttribute(_display, currentConfig, Egl.ConfigAttribute.EGL_SAMPLES, ref attributeValue);
                fbConfig.Samples = attributeValue;
                fbConfig.Doublebuffer = true;

                fbConfig.Handle = i + 1;
                usableConfigs.Add(fbConfig);
            }

            if (usableConfigs.Count == 0)
            {
                Engine.Log.Error("The driver doesn't seem to support OpenGL.", MessageSource.WGallium);
                return 0;
            }

            FramebufferConfig closestConfig = ChoosePixelFormat(usableConfigs);
            if (closestConfig != null) return closestConfig.Handle;

            Engine.Log.Error("Couldn't find suitable pixel format.", MessageSource.WGallium);
            return 0;
        }

        protected override void SetSwapIntervalPlatform(int interval)
        {
            Egl.SwapInterval(_display, interval);
        }

        public override void MakeCurrent()
        {
            Egl.MakeCurrent(_display, _surface, _surface, _context);
        }

        public override void SwapBuffers()
        {
            Egl.SwapBuffers(_display, _surface);
        }

        public override IntPtr GetProcAddress(string func)
        {
            IntPtr eglFunc = _platform.GetLibrarySymbolPtr(_eglLib, func);
            if (eglFunc != IntPtr.Zero) return eglFunc;
            return Egl.GetProcAddress(func);
        }
    }
}