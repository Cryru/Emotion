#region Using

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.Platform;
using Emotion.Web.Helpers;
using Microsoft.JSInterop;
using OpenGL;

#endregion

namespace Emotion.Web.Platform
{
    public unsafe class WebGLContext : GraphicsContext
    {
        private IJSUnmarshalledRuntime _gl;
        private Dictionary<string, Delegate> _webGlFuncDictionary = new Dictionary<string, Delegate>();
        private IntPtr _objectGenPtrHolder;

        public WebGLContext(IJSUnmarshalledRuntime glContext)
        {
            Native = false;
            _gl = glContext;

            const int maxGenAtOnce = 5;
            _objectGenPtrHolder = UnmanagedMemoryAllocator.MemAlloc(sizeof(uint) * maxGenAtOnce);

            _webGlFuncDictionary.Add("glGetError", (Gl.Delegates.glGetError) GetError);
            _webGlFuncDictionary.Add("glGetString", (Gl.Delegates.glGetString) GetString);
            _webGlFuncDictionary.Add("glGetIntegerv", (Gl.Delegates.glGetIntegerv) GetInteger);
            _webGlFuncDictionary.Add("glGetFloatv", (Gl.Delegates.glGetFloatv) GetFloat);
            _webGlFuncDictionary.Add("glCreateShader", (Gl.Delegates.glCreateShader) CreateShader);

            _webGlFuncDictionary.Add("glGenBuffers", (Gl.Delegates.glGenBuffers) GenBuffers);
            _webGlFuncDictionary.Add("glBindBuffer", (Gl.Delegates.glBindBuffer) BindBuffer);
            _webGlFuncDictionary.Add("glBufferData", (Gl.Delegates.glBufferData) BufferData);
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
            return _gl.InvokeUnmarshalled<int>("glGetError");
        }

        private IntPtr GetString(int paramId)
        {
            string value;
            if (paramId == (int) StringName.Extensions)
                value = _gl.InvokeUnmarshalled<string>("GetGLExtensions");
            else
                value = _gl.InvokeUnmarshalled<int, string>("glGet", paramId);

            //Engine.Log.Trace($"String query {(StringName) paramId} got {value}", "WebGLInternal");
            IntPtr ptr = Marshal.StringToHGlobalAuto(value);
            UnmanagedMemoryAllocator.MarkAlloc(ptr);
            return ptr;
        }

        private void GetInteger(int paramId, int* data)
        {
            int[] value = _gl.InvokeUnmarshalled<int, int[]>("glGet", paramId);
            //Engine.Log.Trace($"Integer query {(GetPName) paramId} got {string.Join(", ", value)}", "WebGLInternal");
            IntPtr ptr = UnmanagedMemoryAllocator.MemAlloc(value.Length * sizeof(int));
            Marshal.Copy(value, 0, ptr, value.Length);
            data = (int*) ptr;
        }

        private void GetFloat(int paramId, float* data)
        {
            float[] value = _gl.InvokeUnmarshalled<int, float[]>("glGet", paramId);
            //Engine.Log.Trace($"Float query {(GetPName) paramId} got {string.Join(", ", value)}", "WebGLInternal");
            IntPtr ptr = UnmanagedMemoryAllocator.MemAlloc(value.Length * sizeof(float));
            Marshal.Copy(value, 0, ptr, value.Length);
            data = (float*) ptr;
        }

        private void GenBuffers(int count, uint* resp)
        {
            uint[] value = _gl.InvokeUnmarshalled<int, uint[]>("glGenBuffers", count);
            for (var i = 0; i < value.Length; i++)
            {
                Marshal.WriteInt64((IntPtr) (resp + i * sizeof(uint)), value[i]);
            }
        }

        private void BindBuffer(int target, uint bufferId)
        {
            _gl.InvokeUnmarshalled<int, uint, object>("glBindBuffer", target, bufferId);
        }

        private void BufferData(int target, uint size, IntPtr ptr, int usage)
        {
            if (ptr == IntPtr.Zero) ptr = UnmanagedMemoryAllocator.MemAlloc((int) size);
            _gl.InvokeUnmarshalled<int, uint, IntPtr, object>("glBufferData", target, size, ptr);
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