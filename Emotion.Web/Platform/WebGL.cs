#region Using

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Blazor.Extensions.Canvas.WebGL;
using Emotion.Common;
using Emotion.Platform;
using OpenGL;
using ShaderType = Blazor.Extensions.Canvas.WebGL.ShaderType;

#endregion

namespace Emotion.Web.Platform
{
    public static class WebGLCache
    {
        private static Parameter[] _cacheStrings =
        {
            Parameter.VERSION,
            Parameter.VENDOR,
            Parameter.RENDERER,
        };

        public static Dictionary<Parameter, string> GLGetDictStrings = new Dictionary<Parameter, string>();

        private static Parameter[] _cachedNumbers =
        {
            Parameter.MAX_TEXTURE_SIZE,
        };

        public static Dictionary<Parameter, float> GLGetDictNumbers = new Dictionary<Parameter, float>();

        public static IntPtr AllocatedEmptyString;
        public static IntPtr Allocated4ByteReturnMemory;

        public static async Task PopulateCache(WebGLContext gl)
        {
            AllocatedEmptyString = Marshal.StringToHGlobalAuto("");
            Allocated4ByteReturnMemory = Marshal.AllocHGlobal(4);
            await CacheGets(gl);
        }

        public static async Task CacheGets(WebGLContext gl)
        {
            foreach (Parameter param in _cacheStrings)
            {
                Console.WriteLine($"Caching string {param}");
                var resp = await gl.GetParameterAsync<string>(param);
                GLGetDictStrings.Add(param, resp);
            }

            foreach (Parameter param in _cachedNumbers)
            {
                Console.WriteLine($"Caching number {param}");
                var resp = await gl.GetParameterAsync<float>(param);
                GLGetDictNumbers.Add(param, resp);
            }
        }
    }

    public unsafe class WebGL : GraphicsContext
    {
        private WebGLContext _gl;
        private Dictionary<string, Delegate> _webGlFuncDictionary = new Dictionary<string, Delegate>();

        public WebGL(WebGLContext glContext)
        {
            Native = false;
            _gl = glContext;

            _webGlFuncDictionary.Add("glGetError", (Gl.Delegates.glGetError) GetError);
            _webGlFuncDictionary.Add("glGetString", (Gl.Delegates.glGetString) GetString);
            _webGlFuncDictionary.Add("glGetIntegerv", (Gl.Delegates.glGetIntegerv) GetInteger);
            _webGlFuncDictionary.Add("glGetFloatv", (Gl.Delegates.glGetFloatv) GetFloat);
            _webGlFuncDictionary.Add("glCreateShader", (Gl.Delegates.glCreateShader) CreateShader);
        }

        protected override void SetSwapIntervalPlatform(int interval)
        {
            // Have no control over
        }

        public override void MakeCurrent()
        {
            // Always current
        }

        public override void SwapBuffers()
        {
            // Have no control over.
        }

        public override Delegate GetProcAddressNonNative(string func)
        {
            if (_webGlFuncDictionary.ContainsKey(func))
            {
                Engine.Log.Trace($"{func}", "WebGL");
                return _webGlFuncDictionary[func];
            }

            Engine.Log.Trace($"Missing WebGL function {func}", "WebGL");
            return base.GetProcAddressNonNative(func);
        }

        public override IntPtr GetProcAddress(string func)
        {
            Engine.Log.Trace($"Tried to get Native WebGL function - {func}!", "WebGL");
            return IntPtr.Zero;
        }

        private int GetError()
        {
            Error error = Error.NO_ERROR; //_gl.GetErrorAsync().Result;
            return (int) error;
        }

        private IntPtr GetString(int paramId)
        {
            var paramName = (Parameter) paramId;
            bool found = WebGLCache.GLGetDictStrings.ContainsKey(paramName);
            if (!found)
            {
                Engine.Log.Trace($"String query {paramId} {paramName} not found.", "WebGLInternal");
                return WebGLCache.AllocatedEmptyString;
            }

            string value = WebGLCache.GLGetDictStrings[paramName];
            Engine.Log.Trace($"String query {paramId} {(Parameter) paramId} got {value}", "WebGLInternal");
            return Marshal.StringToHGlobalAuto(value);
        }

        private void GetInteger(int paramId, int* data)
        {
            var paramName = (Parameter) paramId;
            bool found = WebGLCache.GLGetDictNumbers.ContainsKey(paramName);
            if (!found)
            {
                data = (int*) IntPtr.Zero;
                Engine.Log.Trace($"Integer query {paramId} {paramName} not found.", "WebGLInternal");
                return;
            }

            var value = (int) WebGLCache.GLGetDictNumbers[paramName];
            Engine.Log.Trace($"Integer query {paramId} {(Parameter) paramId} got {value}", "WebGLInternal");
            Marshal.Copy(new[] {value}, 0, WebGLCache.Allocated4ByteReturnMemory, 4);
            data = (int*) WebGLCache.Allocated4ByteReturnMemory;
        }

        private void GetFloat(int paramId, float* data)
        {
            var paramName = (Parameter) paramId;
            bool found = WebGLCache.GLGetDictNumbers.ContainsKey(paramName);
            if (!found)
            {
                data = (float*) IntPtr.Zero;
                Engine.Log.Trace($"Float query {paramId} {paramName} not found.", "WebGLInternal");
                return;
            }

            float value = WebGLCache.GLGetDictNumbers[paramName];
            Engine.Log.Trace($"Float query {paramId} {(Parameter) paramId} got {value}", "WebGLInternal");
            Marshal.Copy(new[] {value}, 0, WebGLCache.Allocated4ByteReturnMemory, 4);
            data = (float*) WebGLCache.Allocated4ByteReturnMemory;
        }

        private uint CreateShader(int type)
        {
            Engine.Log.Trace($"Creating shader of type {type}", "WebGLInternal");
            WebGLShader shader = _gl.CreateShaderAsync((ShaderType) type).Result;
            return (uint) shader.Id;
        }
    }
}