#region Using

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.Platform;
using Microsoft.JSInterop;
using OpenGL;

#endregion

namespace Emotion.Web.Platform
{
    public unsafe class WebGL : GraphicsContext
    {
        private IJSInProcessRuntime _gl;
        private Dictionary<string, Delegate> _webGlFuncDictionary = new Dictionary<string, Delegate>();

        public WebGL(IJSInProcessRuntime glContext)
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

        // Unused
        public override IntPtr GetProcAddress(string func)
        {
            return IntPtr.Zero;
        }

        public override Delegate GetProcAddressNonNative(string func)
        {
            if (_webGlFuncDictionary.ContainsKey(func)) return _webGlFuncDictionary[func];

            Engine.Log.Trace($"Missing WebGL function {func}", "WebGL");
            return base.GetProcAddressNonNative(func);
        }

        private int GetError()
        {
            return _gl.Invoke<int>("glGetError");
        }

        private IntPtr GetString(int paramId)
        {
            string value;
            if (paramId == (int) StringName.Extensions)
                value = _gl.Invoke<string>("GetGLExtensions");
            else
                value = _gl.Invoke<string>("glGet", paramId);

            Engine.Log.Trace($"String query {(StringName) paramId} got {value}", "WebGLInternal");
            IntPtr ptr = Marshal.StringToHGlobalAuto(value);
            UnmanagedMemoryAllocator.MarkAlloc(ptr);
            return ptr;
        }

        private void GetInteger(int paramId, int* data)
        {
            int[] value = _gl.Invoke<int[]>("glGet", paramId);
            Engine.Log.Trace($"Integer query {(GetPName) paramId} got {string.Join(", ", value)}", "WebGLInternal");
            IntPtr ptr = UnmanagedMemoryAllocator.MemAlloc(value.Length * 4);
            Marshal.Copy(value, 0, ptr, value.Length);
            data = (int*) ptr;
        }

        private void GetFloat(int paramId, float* data)
        {
            float[] value = _gl.Invoke<float[]>("glGet", paramId);
            Engine.Log.Trace($"Float query {(GetPName) paramId} got {string.Join(", ", value)}", "WebGLInternal");
            IntPtr ptr = UnmanagedMemoryAllocator.MemAlloc(value.Length * 4);
            Marshal.Copy(value, 0, ptr, value.Length);
            data = (float*) ptr;
        }

        private uint CreateShader(int type)
        {
            return 0;
            //Engine.Log.Trace($"Creating shader of type {type}", "WebGLInternal");
            //WebGLShader shader = _gl.CreateShaderAsync((ShaderType) type).Result;
            //return (uint) shader.Id;
        }
    }
}