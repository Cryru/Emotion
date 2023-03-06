#region Using

using System.Runtime.InteropServices;
using System.Text;
using Android.Opengl;
using Emotion.Platform;
using Java.Nio;
using OpenGL;
using Buffer = Java.Nio.Buffer;

#endregion

namespace Emotion.Droid;

public unsafe class AndroidGraphicsContext : GraphicsContext
{
	public OpenGLSurface Surface; // GL surface to render on.
	public AndroidGLRenderer Renderer; // Android API GL context.

	private Dictionary<string, Delegate> _funcDictionary = new();
	private Dictionary<int, uint> _boundBuffers = new(); // <Target, BufferId>
	private Dictionary<uint, int> _bufferToLength = new(); // <BufferId, Size>

	public AndroidGraphicsContext(OpenGLSurface surface, AndroidGLRenderer renderer)
	{
		Native = false;

		Surface = surface;
		Renderer = renderer;

		_funcDictionary.Add("glGetError", (Gl.Delegates.glGetError)GLES20.GlGetError);
		_funcDictionary.Add("glGetString", (Gl.Delegates.glGetString)GetString);
		_funcDictionary.Add("glGetIntegerv", (Gl.Delegates.glGetIntegerv)GetIntegerV);
		_funcDictionary.Add("glGetFloatv", (Gl.Delegates.glGetFloatv)GetFloatV);

		_funcDictionary.Add("glGenBuffers", (Gl.Delegates.glGenBuffers)GenBuffers);
		_funcDictionary.Add("glBindBuffer", (Gl.Delegates.glBindBuffer)BindBuffer);
		_funcDictionary.Add("glBufferData", (Gl.Delegates.glBufferData)BufferData);
		_funcDictionary.Add("glBufferSubData", (Gl.Delegates.glBufferSubData)BufferSubData);

		_funcDictionary.Add("glMapBuffer", (Gl.Delegates.glMapBuffer)MapBuffer);
		_funcDictionary.Add("glMapBufferRange", (Gl.Delegates.glMapBufferRange)MapBufferRange);
		_funcDictionary.Add("glUnmapBuffer", (Gl.Delegates.glUnmapBuffer)GLES30.GlUnmapBuffer);
		_funcDictionary.Add("glFlushMappedBufferRange", (Gl.Delegates.glFlushMappedBufferRange)FlushMappedRange);

		_funcDictionary.Add("glClear", (Gl.Delegates.glClear)Clear);
		_funcDictionary.Add("glClearColor", (Gl.Delegates.glClearColor)GLES20.GlClearColor);
		_funcDictionary.Add("glColorMask", (Gl.Delegates.glColorMask)GLES20.GlColorMask);
		_funcDictionary.Add("glEnable", (Gl.Delegates.glEnable)GLES20.GlEnable);
		_funcDictionary.Add("glDisable", (Gl.Delegates.glDisable)GLES20.GlDisable);
		_funcDictionary.Add("glDepthFunc", (Gl.Delegates.glDepthFunc)GLES20.GlDepthFunc);
		_funcDictionary.Add("glStencilMask", (Gl.Delegates.glStencilMask)StencilMask);
		_funcDictionary.Add("glStencilFunc", (Gl.Delegates.glStencilFunc)StencilFunc);
		_funcDictionary.Add("glStencilOp", (Gl.Delegates.glStencilOp)GLES20.GlStencilOp);
		_funcDictionary.Add("glBlendFuncSeparate", (Gl.Delegates.glBlendFuncSeparate)GLES20.GlBlendFuncSeparate);
		_funcDictionary.Add("glViewport", (Gl.Delegates.glViewport)GLES20.GlViewport);
		_funcDictionary.Add("glCullFace", (Gl.Delegates.glCullFace)GLES20.GlCullFace);
		_funcDictionary.Add("glFrontFace", (Gl.Delegates.glFrontFace)GLES20.GlFrontFace);

		_funcDictionary.Add("glCreateShader", (Gl.Delegates.glCreateShader)CreateShader);
		_funcDictionary.Add("glShaderSource", (Gl.Delegates.glShaderSource)ShaderSource);
		_funcDictionary.Add("glCompileShader", (Gl.Delegates.glCompileShader)CompileShader);
		_funcDictionary.Add("glGetShaderiv", (Gl.Delegates.glGetShaderiv)ShaderGetParam);
		_funcDictionary.Add("glGetShaderInfoLog", (Gl.Delegates.glGetShaderInfoLog)ShaderInfoLog);

		_funcDictionary.Add("glCreateProgram", (Gl.Delegates.glCreateProgram)CreateProgram);
		_funcDictionary.Add("glDeleteShader", (Gl.Delegates.glDeleteShader)DeleteShader);
		_funcDictionary.Add("glUseProgram", (Gl.Delegates.glUseProgram)UseProgram);
		_funcDictionary.Add("glAttachShader", (Gl.Delegates.glAttachShader)AttachShader);
		_funcDictionary.Add("glBindAttribLocation", (Gl.Delegates.glBindAttribLocation)BindAttributeLocation);
		_funcDictionary.Add("glLinkProgram", (Gl.Delegates.glLinkProgram)LinkProgram);
		_funcDictionary.Add("glValidateProgram", (Gl.Delegates.glValidateProgram)ValidateProgram);
		_funcDictionary.Add("glGetProgramInfoLog", (Gl.Delegates.glGetProgramInfoLog)ProgramInfoLog);
		_funcDictionary.Add("glGetProgramiv", (Gl.Delegates.glGetProgramiv)ProgramGetParam);
		_funcDictionary.Add("glGetUniformLocation", (Gl.Delegates.glGetUniformLocation)GetUniformLocation);
		_funcDictionary.Add("glUniform1iv", (Gl.Delegates.glUniform1iv)UploadUniform);
		_funcDictionary.Add("glUniform1f", (Gl.Delegates.glUniform1f)GLES20.GlUniform1f);
		_funcDictionary.Add("glUniform2f", (Gl.Delegates.glUniform2f)GLES20.GlUniform2f);
		_funcDictionary.Add("glUniform1i", (Gl.Delegates.glUniform1i)GLES20.GlUniform1i);
		_funcDictionary.Add("glUniform3f", (Gl.Delegates.glUniform3f)GLES20.GlUniform3f);
		_funcDictionary.Add("glUniform4f", (Gl.Delegates.glUniform4f)GLES20.GlUniform4f);
		_funcDictionary.Add("glUniform1fv", (Gl.Delegates.glUniform1fv)UploadUniform);
		_funcDictionary.Add("glUniform2fv", (Gl.Delegates.glUniform2fv)UploadUniformFloatArrayMultiComponent2);
		_funcDictionary.Add("glUniform3fv", (Gl.Delegates.glUniform3fv)UploadUniformFloatArrayMultiComponent3);
		_funcDictionary.Add("glUniform4fv", (Gl.Delegates.glUniform4fv)UploadUniformFloatArrayMultiComponent4);
		_funcDictionary.Add("glUniformMatrix4fv", (Gl.Delegates.glUniformMatrix4fv)UploadUniformMat4);

		_funcDictionary.Add("glGenFramebuffers", (Gl.Delegates.glGenFramebuffers)CreateFramebuffer);
		_funcDictionary.Add("glBindFramebuffer", (Gl.Delegates.glBindFramebuffer)BindFramebuffer);
		_funcDictionary.Add("glFramebufferTexture2D", (Gl.Delegates.glFramebufferTexture2D)FramebufferUploadTexture2D);
		_funcDictionary.Add("glCheckFramebufferStatus", (Gl.Delegates.glCheckFramebufferStatus)GLES20.GlCheckFramebufferStatus);
		_funcDictionary.Add("glDrawBuffers", (Gl.Delegates.glDrawBuffers)DrawBuffers);

		_funcDictionary.Add("glGenVertexArrays", (Gl.Delegates.glGenVertexArrays)GenVertexArrays);
		_funcDictionary.Add("glBindVertexArray", (Gl.Delegates.glBindVertexArray)BindVertexArray);
		_funcDictionary.Add("glEnableVertexAttribArray", (Gl.Delegates.glEnableVertexAttribArray)EnableVertexAttribArray);
		_funcDictionary.Add("glVertexAttribPointer", (Gl.Delegates.glVertexAttribPointer)VertexAttribPointer);

		_funcDictionary.Add("glDrawElements", (Gl.Delegates.glDrawElements)DrawElements);
		_funcDictionary.Add("glDrawArrays", (Gl.Delegates.glDrawArrays)GLES20.GlDrawArrays);

		_funcDictionary.Add("glFenceSync", (Gl.Delegates.glFenceSync)FenceSync);
		_funcDictionary.Add("glClientWaitSync", (Gl.Delegates.glClientWaitSync)ClientWaitSync);
		_funcDictionary.Add("glDeleteSync", (Gl.Delegates.glDeleteSync)DeleteSync);

		_funcDictionary.Add("glGenTextures", (Gl.Delegates.glGenTextures)CreateTexture);
		_funcDictionary.Add("glDeleteTextures", (Gl.Delegates.glDeleteTextures)DeleteTextures);
		_funcDictionary.Add("glBindTexture", (Gl.Delegates.glBindTexture)BindTexture);
		_funcDictionary.Add("glActiveTexture", (Gl.Delegates.glActiveTexture)GLES20.GlActiveTexture);
		_funcDictionary.Add("glTexImage2D", (Gl.Delegates.glTexImage2D)UploadTexture);
		_funcDictionary.Add("glTexParameteri", (Gl.Delegates.glTexParameteri)GLES20.GlTexParameteri);

		_funcDictionary.Add("glGenRenderbuffers", (Gl.Delegates.glGenRenderbuffers)CreateRenderbuffer);
		_funcDictionary.Add("glBindRenderbuffer", (Gl.Delegates.glBindRenderbuffer)BindRenderbuffer);
		_funcDictionary.Add("glRenderbufferStorage", (Gl.Delegates.glRenderbufferStorage)GLES20.GlRenderbufferStorage);
		_funcDictionary.Add("glFramebufferRenderbuffer", (Gl.Delegates.glFramebufferRenderbuffer)FramebufferRenderbuffer);
	}

	protected override void SetSwapIntervalPlatform(int interval)
	{
		// todo
	}

	public override void MakeCurrent()
	{
		// always current
	}

	public override void SwapBuffers()
	{
		Surface.RequestRender();
	}

	// unused
	public override IntPtr GetProcAddress(string func)
	{
		return IntPtr.Zero;
	}

	public override Delegate GetProcAddressManaged(string func)
	{
		if (_funcDictionary.ContainsKey(func))
			//Engine.Log.Trace($"Returning func {func}", "GL");
			return _funcDictionary[func];

		//Engine.Log.Trace($"Missing GL function {func}", "GL");
		return base.GetProcAddressManaged(func);
	}

	private IntPtr GetString(int param)
	{
		string? str = GLES20.GlGetString(param);
		if (str == null) return IntPtr.Zero;
		return Marshal.StringToHGlobalAuto(str);
	}

	private void GetIntegerV(int param, int[] data)
	{
		GLES20.GlGetIntegerv(param, data, 0);
	}

	private void GetFloatV(int param, float[] data)
	{
		GLES20.GlGetFloatv(param, data, 0);
	}

	private void GenBuffers(int count, uint* data)
	{
		var value = new int[count];
		GLES20.GlGenBuffers(count, value, 0);
		for (var i = 0; i < value.Length; i++)
		{
			data[i] = (uint)value[i];
		}
	}

	private void BindBuffer(int target, uint bufferId)
	{
		GLES20.GlBindBuffer(target, (int)bufferId);
		_boundBuffers[target] = bufferId;
	}

	private void BufferData(int target, uint size, IntPtr ptr, int usage)
	{
		uint boundBufferId = _boundBuffers[target];
		_bufferToLength[boundBufferId] = (int)size;

		// We have to copy the data in order to marshal it to a Java object and upload it.
		// This kinda sucks. Todo: Can we pass a pointer directly in an actual native way?
		var arr = new byte[size];
		if (ptr != IntPtr.Zero) Marshal.Copy(ptr, arr, 0, (int)size);
		ByteBuffer? buffer = ByteBuffer.Wrap(arr);
		GLES20.GlBufferData(target, (int)size, buffer, usage);
		buffer?.Dispose();
	}

	private void BufferSubData(int target, IntPtr offset, uint size, IntPtr ptr)
	{
		var arr = new byte[size];
		Marshal.Copy(ptr, arr, 0, (int)size);
		ByteBuffer? buffer = ByteBuffer.Wrap(arr);
		GLES20.GlBufferSubData(target, (int)offset, (int)size, buffer);
		buffer?.Dispose();
	}

	private IntPtr MapBuffer(int target, int access)
	{
		uint boundBufferId = _boundBuffers[target];
		int length = _bufferToLength[boundBufferId];

		Buffer? buffer = GLES30.GlMapBufferRange(target, 0, length, access);
		return buffer?.GetDirectBufferAddress() ?? IntPtr.Zero;
	}

	private IntPtr MapBufferRange(int target, IntPtr offset, uint length, uint access)
	{
		Buffer? buffer = GLES30.GlMapBufferRange(target, (int)offset, (int)length, (int)access);
		return buffer?.GetDirectBufferAddress() ?? IntPtr.Zero;
	}

	private void FlushMappedRange(int target, IntPtr offset, uint length)
	{
		GLES30.GlFlushMappedBufferRange(target, (int)offset, (int)length);
	}

	private void Clear(uint mask)
	{
		GLES20.GlClear((int)mask);
	}

	private void StencilMask(uint maskType)
	{
		GLES20.GlStencilMask((int)maskType);
	}

	private void StencilFunc(int funcId, int refV, uint mask)
	{
		GLES20.GlStencilFunc(funcId, refV, (int)mask);
	}

	private uint CreateShader(int type)
	{
		return (uint)GLES20.GlCreateShader(type);
	}

	private void ShaderSource(uint shader, int count, string[] data, int* length)
	{
		string shaderSource = data.Length > 1 ? string.Join(' ', data) : data[0];
		GLES20.GlShaderSource((int)shader, shaderSource);
	}

	private void CompileShader(uint shader)
	{
		GLES20.GlCompileShader((int)shader);
	}

	private void ShaderGetParam(uint shader, int paramId, int* data)
	{
		var value = new int[1];
		GLES20.GlGetShaderiv((int)shader, paramId, value, 0);
		*data = value[0];
	}

	private void ShaderInfoLog(uint shaderId, int bufSize, int* length, StringBuilder logData)
	{
		string? log = GLES20.GlGetShaderInfoLog((int)shaderId);
		if (log == null)
		{
			*length = 0;
			return;
		}

		*length = log.Length;
		if (log.Length == 0) return;
		logData.Append(log, 0, Math.Min(log.Length, bufSize));
	}

	private uint CreateProgram()
	{
		return (uint)GLES20.GlCreateProgram();
	}

	private void DeleteShader(uint programId)
	{
		GLES20.GlDeleteShader((int)programId);
	}

	private void UseProgram(uint programId)
	{
		GLES20.GlUseProgram((int)programId);
	}

	private void AttachShader(uint program, uint shader)
	{
		GLES20.GlAttachShader((int)program, (int)shader);
	}

	private void BindAttributeLocation(uint program, uint index, string name)
	{
		GLES20.GlBindAttribLocation((int)program, (int)index, name);
	}

	private void LinkProgram(uint program)
	{
		GLES20.GlLinkProgram((int)program);
	}

	private void ValidateProgram(uint program)
	{
		GLES20.GlValidateProgram((int)program);
	}

	private void ProgramInfoLog(uint programId, int bufSize, int* length, StringBuilder logData)
	{
		string? log = GLES20.GlGetProgramInfoLog((int)programId);
		if (log == null) *length = 0;
	}

	private void ProgramGetParam(uint program, int paramId, int* data)
	{
		var value = new int[1];
		GLES20.GlGetProgramiv((int)program, paramId, value, 0);
		*data = value[0];
	}

	private int GetUniformLocation(uint program, string name)
	{
		return GLES20.GlGetUniformLocation((int)program, name);
	}

	private void UploadUniform(int location, int size, int* value)
	{
		var arr = new int[size];
		Marshal.Copy((IntPtr)value, arr, 0, size);
		GLES20.GlUniform1iv(location, size, arr, 0);
	}

	private void UploadUniform(int location, int size, float* value)
	{
		var arr = new float[size];
		Marshal.Copy((IntPtr)value, arr, 0, size);
		GLES20.GlUniform1fv(location, size, arr, 0);
	}

	private void UploadUniformFloatArrayMultiComponent2(int location, int count, float* value)
	{
		var arr = new float[count * 2];
		Marshal.Copy((IntPtr)value, arr, 0, arr.Length);
		GLES20.GlUniform2fv(location, count, arr, 0);
	}

	private void UploadUniformFloatArrayMultiComponent3(int location, int count, float* value)
	{
		var arr = new float[count * 3];
		Marshal.Copy((IntPtr)value, arr, 0, arr.Length);
		GLES20.GlUniform3fv(location, count, arr, 0);
	}

	private void UploadUniformFloatArrayMultiComponent4(int location, int count, float* value)
	{
		var arr = new float[count * 4];
		Marshal.Copy((IntPtr)value, arr, 0, arr.Length);
		GLES20.GlUniform4fv(location, count, arr, 0);
	}

	private void UploadUniformMat4(int location, int size, bool transpose, float* value)
	{
		var arr = new float[4 * 4 * size];
		Marshal.Copy((IntPtr)value, arr, 0, arr.Length);
		GLES20.GlUniformMatrix4fv(location, size, transpose, arr, 0);
	}

	private void CreateFramebuffer(int count, uint* resp)
	{
		var arr = new int[count];
		GLES20.GlGenFramebuffers(count, arr, 0);
		Marshal.Copy(arr, 0, (IntPtr)resp, count);
	}

	private void BindFramebuffer(int target, uint bufferId)
	{
		GLES20.GlBindFramebuffer(target, (int)bufferId);
	}

	private void FramebufferUploadTexture2D(int target, int attachment, int textarget, uint texture, int level)
	{
		GLES20.GlFramebufferTexture2D(target, attachment, textarget, (int)texture, level);
	}

	private void DrawBuffers(int count, int* modes)
	{
		var arr = new int[count];
		GLES30.GlDrawBuffers(count, arr, 0);
		Marshal.Copy((IntPtr)modes, arr, 0, count);
	}

	private void GenVertexArrays(int count, uint* resp)
	{
		var arr = new int[count];
		GLES30.GlGenVertexArrays(count, arr, 0);
		Marshal.Copy(arr, 0, (IntPtr)resp, count);
	}

	private void BindVertexArray(uint bufferId)
	{
		GLES30.GlBindVertexArray((int)bufferId);
	}

	private void EnableVertexAttribArray(uint attribute)
	{
		GLES20.GlEnableVertexAttribArray((int)attribute);
	}

	private void VertexAttribPointer(uint index, int size, int type, bool normalized, int stride, IntPtr offset)
	{
		GLES20.GlVertexAttribPointer((int)index, size, type, normalized, stride, (int)offset);
	}

	private void DrawElements(int mode, int count, int type, IntPtr offset)
	{
		GLES20.GlDrawElements(mode, count, type, (int)offset);
	}

	private IntPtr FenceSync(int condition, uint flags)
	{
		return (IntPtr)GLES30.GlFenceSync(condition, (int)flags);
	}

	private int ClientWaitSync(IntPtr condition, uint flags, ulong timeout)
	{
		return GLES30.GlClientWaitSync(condition, (int)flags, (long)timeout);
	}

	private void DeleteSync(IntPtr sync)
	{
		GLES30.GlDeleteSync(sync);
	}

	private void CreateTexture(int count, uint* resp)
	{
		var arr = new int[count];
		GLES20.GlGenTextures(count, arr, 0);
		Marshal.Copy(arr, 0, (IntPtr)resp, count);
	}

	private void DeleteTextures(int count, uint* val)
	{
		var arr = new int[count];
		Marshal.Copy((IntPtr)val, arr, 0, count);
		GLES20.GlDeleteTextures(count, arr, 0);
	}

	private void BindTexture(int slot, uint pointer)
	{
		GLES20.GlBindTexture(slot, (int)pointer);
	}

	private void UploadTexture(int target, int level, int internalFormat, int width, int height, int border, int format, int type, IntPtr pixels)
	{
		int bytes = width * height * Gl.PixelFormatToComponentCount((PixelFormat)format) * Gl.PixelTypeToByteCount((PixelType)type);
		var arr = new byte[bytes];
		if (pixels != IntPtr.Zero) Marshal.Copy(pixels, arr, 0, bytes);

		ByteBuffer? buffer = ByteBuffer.Wrap(arr);
		GLES20.GlTexImage2D(target, level, internalFormat, width, height, border, format, type, buffer);
		buffer?.Dispose();
	}

	private void CreateRenderbuffer(int count, uint* resp)
	{
		var arr = new int[count];
		GLES20.GlGenRenderbuffers(count, arr, 0);
		Marshal.Copy(arr, 0, (IntPtr)resp, count);
	}

	private void BindRenderbuffer(int target, uint bufferId)
	{
		GLES20.GlBindRenderbuffer(target, (int)bufferId);
	}

	private void FramebufferRenderbuffer(int target, int attachment, int renderbufferTarget, uint renderbuffer)
	{
		GLES20.GlFramebufferRenderbuffer(target, attachment, renderbufferTarget, (int)renderbuffer);
	}
}