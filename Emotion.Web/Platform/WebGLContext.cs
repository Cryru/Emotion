#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
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

        // State
        private Dictionary<int, uint> _boundBuffers = new Dictionary<int, uint>();

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

            _webGlFuncDictionary.Add("glGenBuffers", (Gl.Delegates.glGenBuffers) GenBuffers);
            _webGlFuncDictionary.Add("glBindBuffer", (Gl.Delegates.glBindBuffer) BindBuffer);
            _webGlFuncDictionary.Add("glBufferData", (Gl.Delegates.glBufferData) BufferData);

            _webGlFuncDictionary.Add("glMapBuffer", (Gl.Delegates.glMapBuffer) MapBuffer);
            _webGlFuncDictionary.Add("glMapBufferRange", (Gl.Delegates.glMapBufferRange) MapBufferRange);
            _webGlFuncDictionary.Add("glUnmapBuffer", (Gl.Delegates.glUnmapBuffer) UnmapBuffer);

            _webGlFuncDictionary.Add("glClearColor", (Gl.Delegates.glClearColor) ClearColor);

            _webGlFuncDictionary.Add("glCreateShader", (Gl.Delegates.glCreateShader) CreateShader);
            _webGlFuncDictionary.Add("glShaderSource", (Gl.Delegates.glShaderSource) ShaderSource);
            _webGlFuncDictionary.Add("glCompileShader", (Gl.Delegates.glCompileShader) CompileShader);
            _webGlFuncDictionary.Add("glGetShaderiv", (Gl.Delegates.glGetShaderiv) ShaderGet);
            _webGlFuncDictionary.Add("glGetShaderInfoLog", (Gl.Delegates.glGetShaderInfoLog) ShaderInfoLog);
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
            if (_webGlFuncDictionary.ContainsKey(func))
            {
                Engine.Log.Trace($"Returning func {func}", "");
                return _webGlFuncDictionary[func];
            }

            Engine.Log.Trace($"Missing WebGL function {func}", "WebGL");
            return base.GetProcAddressNonNative(func);
        }

        private int GetError()
        {
            return _gl.InvokeUnmarshalled<int>("glGetError");
        }

        private IntPtr GetString(int paramId)
        {
            var stringGetMemoryName = $"glGetString{paramId}";

            // Check if already gotten and allocated memory for it.
            IntPtr ptr = UnmanagedMemoryAllocator.GetNamedMemory(stringGetMemoryName, out int _);
            if (ptr != IntPtr.Zero) return ptr;

            // Extensions are gotten from another function.
            string value;
            if (paramId == (int) StringName.Extensions)
                value = _gl.InvokeUnmarshalled<string>("GetGLExtensions");
            else
                value = _gl.InvokeUnmarshalled<int, string>("glGet", paramId);

            //Engine.Log.Trace($"String query {(StringName) paramId} got {value}", "WebGLInternal");
            ptr = Marshal.StringToHGlobalAuto(value);
            UnmanagedMemoryAllocator.RegisterUnownedNamedMemory(ptr, stringGetMemoryName, value.Length * sizeof(char));
            return ptr;
        }

        private void GetInteger(int paramId, int* data)
        {
            int[] value = _gl.InvokeUnmarshalled<int, int[]>("glGet", paramId);
            //Engine.Log.Trace($"Integer query {(GetPName) paramId} got {string.Join(", ", value)}", "WebGLInternal");
            Marshal.Copy(value, 0, (IntPtr) data, value.Length);
        }

        private void GetFloat(int paramId, float* data)
        {
            float[] value = _gl.InvokeUnmarshalled<int, float[]>("glGet", paramId);
            //Engine.Log.Trace($"Float query {(GetPName) paramId} got {string.Join(", ", value)}", "WebGLInternal");
            Marshal.Copy(value, 0, (IntPtr) data, value.Length);
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

            if (_boundBuffers.ContainsKey(target))
                _boundBuffers[target] = bufferId;
            else
                _boundBuffers.Add(target, bufferId);
        }

        private void BufferData(int target, uint size, IntPtr ptr, int usage)
        {
            _boundBuffers.TryGetValue(target, out uint boundBuffer);
            var memoryName = $"DataBuffer{target}{boundBuffer}";
            if (ptr == IntPtr.Zero)
                ptr = UnmanagedMemoryAllocator.MemAllocOrReAllocNamed((int) size, memoryName);
            else
                UnmanagedMemoryAllocator.RegisterUnownedNamedMemory(ptr, memoryName, (int) size);
            _gl.InvokeUnmarshalled<int, uint, IntPtr, object>("glBufferData", target, size, ptr);
        }

        private IntPtr MapBuffer(int target, int access)
        {
            _boundBuffers.TryGetValue(target, out uint boundBuffer);
            var memoryName = $"DataBuffer{target}{boundBuffer}";
            IntPtr memory = UnmanagedMemoryAllocator.GetNamedMemory(memoryName, out int _);
            return memory;
        }

        private IntPtr MapBufferRange(int target, IntPtr offset, uint length, uint access)
        {
            _boundBuffers.TryGetValue(target, out uint boundBuffer);
            var memoryName = $"DataBuffer{target}{boundBuffer}";
            IntPtr memory = UnmanagedMemoryAllocator.GetNamedMemory(memoryName, out int _);
            return memory + (int) offset;
        }

        private bool UnmapBuffer(int target)
        {
            _boundBuffers.TryGetValue(target, out uint boundBuffer);
            var memoryName = $"DataBuffer{target}{boundBuffer}";
            IntPtr memory = UnmanagedMemoryAllocator.GetNamedMemory(memoryName, out int size);

            // Upload memory.
            BufferData(target, (uint) size, memory, -1);
            return true;
        }

        private void ClearColor(float r, float g, float b, float a)
        {
            _gl.InvokeUnmarshalled<Vector4, object>("glClearColor", new Vector4(0.5f, g, 0.32f, a));
        }

        private uint CreateShader(int type)
        {
            return _gl.InvokeUnmarshalled<int, uint>("glCreateShader", type);
            //Engine.Log.Trace($"Creating shader of type {type}", "WebGLInternal");
            //WebGLShader shader = _gl.CreateShaderAsync((ShaderType) type).Result;
            //return (uint) shader.Id;
        }

        private void ShaderSource(uint shader, int count, string[] data, int* length)
        {
            string shaderSource = data.Length > 1 ? string.Join('\n', data) : data[0];
            _gl.InvokeUnmarshalled<uint, string, object>("glShaderSource", shader, shaderSource);
        }

        private void CompileShader(uint shader)
        {
            _gl.InvokeUnmarshalled<uint, object>("glCompileShader", shader);
        }

        private void ShaderGet(uint shader, int paramId, int* data)
        {
            int[] value;

            // Unsupported in WebGL. Invent a number if the shader compiled unsuccessfully, otherwise return 0 (for no log).
            if (paramId == (int) ShaderParameterName.InfoLogLength)
            {
                value = _gl.InvokeUnmarshalled<uint, int, int[]>("glGetShader", shader, (int) ShaderParameterName.CompileStatus);
                if (value == null || value[0] == 1)
                {
                    *data = 0;
                    return;
                }

                // Dummy size.
                *data = 1024 * 4;
                return;
            }

            value = _gl.InvokeUnmarshalled<uint, int, int[]>("glGetShader", shader, paramId);
            //Engine.Log.Trace($"Shader query {(ShaderParameterName) paramId} got {string.Join(", ", value)}", "WebGLInternal");
            Marshal.Copy(value, 0, (IntPtr) data, value.Length);
        }

        private void ShaderInfoLog(uint shaderId, int bufSize, int* length, StringBuilder logData)
        {
            string log = _gl.InvokeUnmarshalled<uint, string>("glGetShaderInfo", shaderId);
            Engine.Log.Trace($"Shader log {log}", "WebGLInternal");
            *length = log.Length;
            if (log.Length == 0) return;
            logData.Append(log, 0, Math.Min(log.Length, bufSize));
        }
    }
}