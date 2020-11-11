#pragma warning disable 649, 1572, 1573

// ReSharper disable RedundantUsingDirective

#region Using

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Khronos;

#endregion

// ReSharper disable StringLiteralTypo
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InvalidXmlDocComment
// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
namespace OpenGL
{
    public partial class Gl
    {
        /// <summary>
        /// [GL] Value of GL_CONTEXT_LOST symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_KHR_robustness", Api = "gl|glcore|gles2")]
        public const int CONTEXT_LOST = 0x0507;

        /// <summary>
        /// [GL] Value of GL_NEGATIVE_ONE_TO_ONE symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_clip_control", Api = "gl|glcore")]
        public const int NEGATIVE_ONE_TO_ONE = 0x935E;

        /// <summary>
        /// [GL] Value of GL_ZERO_TO_ONE symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_clip_control", Api = "gl|glcore")]
        public const int ZERO_TO_ONE = 0x935F;

        /// <summary>
        /// [GL] Value of GL_CLIP_ORIGIN symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_clip_control", Api = "gl|glcore")]
        public const int CLIP_ORIGIN = 0x935C;

        /// <summary>
        /// [GL] Value of GL_CLIP_DEPTH_MODE symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_clip_control", Api = "gl|glcore")]
        public const int CLIP_DEPTH_MODE = 0x935D;

        /// <summary>
        /// [GL] Value of GL_QUERY_WAIT_INVERTED symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_conditional_render_inverted", Api = "gl|glcore")]
        public const int QUERY_WAIT_INVERTED = 0x8E17;

        /// <summary>
        /// [GL] Value of GL_QUERY_NO_WAIT_INVERTED symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_conditional_render_inverted", Api = "gl|glcore")]
        public const int QUERY_NO_WAIT_INVERTED = 0x8E18;

        /// <summary>
        /// [GL] Value of GL_QUERY_BY_REGION_WAIT_INVERTED symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_conditional_render_inverted", Api = "gl|glcore")]
        public const int QUERY_BY_REGION_WAIT_INVERTED = 0x8E19;

        /// <summary>
        /// [GL] Value of GL_QUERY_BY_REGION_NO_WAIT_INVERTED symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_conditional_render_inverted", Api = "gl|glcore")]
        public const int QUERY_BY_REGION_NO_WAIT_INVERTED = 0x8E1A;

        /// <summary>
        /// [GL] Value of GL_MAX_CULL_DISTANCES symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_cull_distance", Api = "gl|glcore")]
        public const int MAX_CULL_DISTANCES = 0x82F9;

        /// <summary>
        /// [GL] Value of GL_MAX_COMBINED_CLIP_AND_CULL_DISTANCES symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_cull_distance", Api = "gl|glcore")]
        public const int MAX_COMBINED_CLIP_AND_CULL_DISTANCES = 0x82FA;

        /// <summary>
        /// [GL4] Gl.GetTexParameter: Returns the effective target of the texture object. For glGetTex*Parameter functions, this is
        /// the target parameter. For glGetTextureParameter*, it is the target to which the texture was initially bound when it was
        /// created, or the value of the target parameter to the call to glCreateTextures which created the texture.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public const int TEXTURE_TARGET = 0x1006;

        /// <summary>
        /// [GL] Value of GL_QUERY_TARGET symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public const int QUERY_TARGET = 0x82EA;

        /// <summary>
        /// [GL4|GLES3.2] Gl.GetGraphicsResetStatus: Indicates that a reset has been detected that is attributable to the current
        /// GL
        /// context.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_KHR_robustness", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_ARB_robustness", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_robustness", Api = "gles1|gles2")]
        public const int GUILTY_CONTEXT_RESET = 0x8253;

        /// <summary>
        /// [GL4|GLES3.2] Gl.GetGraphicsResetStatus: Indicates a reset has been detected that is not attributable to the current GL
        /// context.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_KHR_robustness", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_ARB_robustness", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_robustness", Api = "gles1|gles2")]
        public const int INNOCENT_CONTEXT_RESET = 0x8254;

        /// <summary>
        /// [GL4|GLES3.2] Gl.GetGraphicsResetStatus: Indicates a detected graphics reset whose cause is unknown.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_KHR_robustness", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_ARB_robustness", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_robustness", Api = "gles1|gles2")]
        public const int UNKNOWN_CONTEXT_RESET = 0x8255;

        /// <summary>
        /// [GLES3.2] Gl.Get: data returns a single value, the behaviour of reset notification. Valid values are
        /// Gl.NO_RESET_NOTIFICATION, indicating that no reset notification events will be provided by the implementation, or
        /// Gl.LOSE_CONTEXT_ON_RESET, indicating that a reset will result in the loss of graphics context. This loss can be found
        /// by
        /// querying Gl.GetGraphicsResetStatus.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_KHR_robustness", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_ARB_robustness", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_robustness", Api = "gles1|gles2")]
        public const int RESET_NOTIFICATION_STRATEGY = 0x8256;

        /// <summary>
        /// [GL] Value of GL_LOSE_CONTEXT_ON_RESET symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_KHR_robustness", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_ARB_robustness", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_robustness", Api = "gles1|gles2")]
        public const int LOSE_CONTEXT_ON_RESET = 0x8252;

        /// <summary>
        /// [GL] Value of GL_NO_RESET_NOTIFICATION symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_robustness", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_ARB_robustness", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_robustness", Api = "gles1|gles2")]
        public const int NO_RESET_NOTIFICATION = 0x8261;

        /// <summary>
        /// [GL] Value of GL_CONTEXT_FLAG_ROBUST_ACCESS_BIT symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")] [RequiredByFeature("GL_ARB_robustness", Api = "gl|glcore")]
        public const uint CONTEXT_FLAG_ROBUST_ACCESS_BIT = 0x00000004;

        /// <summary>
        /// [GL] Value of GL_CONTEXT_RELEASE_BEHAVIOR symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_VERSION_4_6")] [RequiredByFeature("GL_KHR_context_flush_control", Api = "gl|glcore|gles2")]
        public const int CONTEXT_RELEASE_BEHAVIOR = 0x82FB;

        /// <summary>
        /// [GL] Value of GL_CONTEXT_RELEASE_BEHAVIOR_FLUSH symbol.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_VERSION_4_6")] [RequiredByFeature("GL_KHR_context_flush_control", Api = "gl|glcore|gles2")]
        public const int CONTEXT_RELEASE_BEHAVIOR_FLUSH = 0x82FC;

        /// <summary>
        /// [GL4] glClipControl: control clip coordinate to window coordinate behavior
        /// </summary>
        /// <param name="origin">
        /// Specifies the clip control origin. Must be one of Gl.LOWER_LEFT or Gl.UPPER_LEFT.
        /// </param>
        /// <param name="depth">
        /// Specifies the clip control depth mode. Must be one of Gl.NEGATIVE_ONE_TO_ONE or Gl.ZERO_TO_ONE.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_clip_control", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_clip_control", Api = "gles2")]
        public static void ClipControl(ClipControlOrigin origin, ClipControlDepth depth)
        {
            Debug.Assert(Delegates.pglClipControl != null, "pglClipControl not implemented");
            Delegates.pglClipControl((int) origin, (int) depth);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glCreateTransformFeedbacks: create transform feedback objects
        /// </summary>
        /// <param name="ids">
        /// Specifies an array in which names of the new transform feedback objects are stored.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void CreateTransformFeedbacks(uint[] ids)
        {
            unsafe
            {
                fixed (uint* p_ids = ids)
                {
                    Debug.Assert(Delegates.pglCreateTransformFeedbacks != null, "pglCreateTransformFeedbacks not implemented");
                    Delegates.pglCreateTransformFeedbacks(ids.Length, p_ids);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glCreateTransformFeedbacks: create transform feedback objects
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static uint CreateTransformFeedback()
        {
            uint retValue;
            unsafe
            {
                Delegates.pglCreateTransformFeedbacks(1, &retValue);
            }

            DebugCheckErrors(null);
            return retValue;
        }

        /// <summary>
        /// [GL4] glTransformFeedbackBufferBase: bind a buffer object to a transform feedback buffer object
        /// </summary>
        /// <param name="xfb">
        /// Name of the transform feedback buffer object.
        /// </param>
        /// <param name="index">
        /// Index of the binding point within <paramref name="xfb" />.
        /// </param>
        /// <param name="buffer">
        /// Name of the buffer object to bind to the specified binding point.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void TransformFeedbackBufferBase(uint xfb, uint index, uint buffer)
        {
            Debug.Assert(Delegates.pglTransformFeedbackBufferBase != null, "pglTransformFeedbackBufferBase not implemented");
            Delegates.pglTransformFeedbackBufferBase(xfb, index, buffer);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glTransformFeedbackBufferRange: bind a range within a buffer object to a transform feedback buffer object
        /// </summary>
        /// <param name="xfb">
        /// Name of the transform feedback buffer object.
        /// </param>
        /// <param name="index">
        /// Index of the binding point within <paramref name="xfb" />.
        /// </param>
        /// <param name="buffer">
        /// Name of the buffer object to bind to the specified binding point.
        /// </param>
        /// <param name="offset">
        /// The starting offset in basic machine units into the buffer object.
        /// </param>
        /// <param name="size">
        /// The amount of data in basic machine units that can be read from or written to the buffer object while used as an
        /// indexed
        /// target.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void TransformFeedbackBufferRange(uint xfb, uint index, uint buffer, IntPtr offset, uint size)
        {
            Debug.Assert(Delegates.pglTransformFeedbackBufferRange != null, "pglTransformFeedbackBufferRange not implemented");
            Delegates.pglTransformFeedbackBufferRange(xfb, index, buffer, offset, size);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetTransformFeedbackiv: query the state of a transform feedback object.
        /// </summary>
        /// <param name="xfb">
        /// The name of an existing transform feedback object, or zero for the default transform feedback object.
        /// </param>
        /// <param name="pname">
        /// Property to use for the query. Must be one of the values: Gl.TRANSFORM_FEEDBACK_BUFFER_BINDING,
        /// Gl.TRANSFORM_FEEDBACK_BUFFER_START, Gl.TRANSFORM_FEEDBACK_BUFFER_SIZE, Gl.TRANSFORM_FEEDBACK_PAUSED,
        /// Gl.TRANSFORM_FEEDBACK_ACTIVE.
        /// </param>
        /// <param name="param">
        /// The address of a buffer into which will be written the requested state information.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetTransformFeedback(uint xfb, TransformFeedbackPName pname, [Out] int[] param)
        {
            unsafe
            {
                fixed (int* p_param = param)
                {
                    Debug.Assert(Delegates.pglGetTransformFeedbackiv != null, "pglGetTransformFeedbackiv not implemented");
                    Delegates.pglGetTransformFeedbackiv(xfb, (int) pname, p_param);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetTransformFeedbacki_v: query the state of a transform feedback object.
        /// </summary>
        /// <param name="xfb">
        /// The name of an existing transform feedback object, or zero for the default transform feedback object.
        /// </param>
        /// <param name="pname">
        /// Property to use for the query. Must be one of the values: Gl.TRANSFORM_FEEDBACK_BUFFER_BINDING,
        /// Gl.TRANSFORM_FEEDBACK_BUFFER_START, Gl.TRANSFORM_FEEDBACK_BUFFER_SIZE, Gl.TRANSFORM_FEEDBACK_PAUSED,
        /// Gl.TRANSFORM_FEEDBACK_ACTIVE.
        /// </param>
        /// <param name="index">
        /// Index of the transform feedback stream (for indexed state).
        /// </param>
        /// <param name="param">
        /// The address of a buffer into which will be written the requested state information.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetTransformFeedback(uint xfb, TransformFeedbackPName pname, uint index, [Out] int[] param)
        {
            unsafe
            {
                fixed (int* p_param = param)
                {
                    Debug.Assert(Delegates.pglGetTransformFeedbacki_v != null, "pglGetTransformFeedbacki_v not implemented");
                    Delegates.pglGetTransformFeedbacki_v(xfb, (int) pname, index, p_param);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetTransformFeedbacki64_v: query the state of a transform feedback object.
        /// </summary>
        /// <param name="xfb">
        /// The name of an existing transform feedback object, or zero for the default transform feedback object.
        /// </param>
        /// <param name="pname">
        /// Property to use for the query. Must be one of the values: Gl.TRANSFORM_FEEDBACK_BUFFER_BINDING,
        /// Gl.TRANSFORM_FEEDBACK_BUFFER_START, Gl.TRANSFORM_FEEDBACK_BUFFER_SIZE, Gl.TRANSFORM_FEEDBACK_PAUSED,
        /// Gl.TRANSFORM_FEEDBACK_ACTIVE.
        /// </param>
        /// <param name="index">
        /// Index of the transform feedback stream (for indexed state).
        /// </param>
        /// <param name="param">
        /// The address of a buffer into which will be written the requested state information.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetTransformFeedback(uint xfb, TransformFeedbackPName pname, uint index, [Out] long[] param)
        {
            unsafe
            {
                fixed (long* p_param = param)
                {
                    Debug.Assert(Delegates.pglGetTransformFeedbacki64_v != null, "pglGetTransformFeedbacki64_v not implemented");
                    Delegates.pglGetTransformFeedbacki64_v(xfb, (int) pname, index, p_param);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glCreateBuffers: create buffer objects
        /// </summary>
        /// <param name="buffers">
        /// Specifies an array in which names of the new buffer objects are stored.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void CreateBuffers(uint[] buffers)
        {
            unsafe
            {
                fixed (uint* p_buffers = buffers)
                {
                    Debug.Assert(Delegates.pglCreateBuffers != null, "pglCreateBuffers not implemented");
                    Delegates.pglCreateBuffers(buffers.Length, p_buffers);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glCreateBuffers: create buffer objects
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static uint CreateBuffer()
        {
            uint retValue;
            unsafe
            {
                Delegates.pglCreateBuffers(1, &retValue);
            }

            DebugCheckErrors(null);
            return retValue;
        }

        /// <summary>
        /// [GL4] glNamedBufferStorage: creates and initializes a buffer object's immutable data store
        /// </summary>
        /// <param name="buffer">
        /// Specifies the name of the buffer object for Gl.NamedBufferStorage function.
        /// </param>
        /// <param name="size">
        /// Specifies the size in bytes of the buffer object's new data store.
        /// </param>
        /// <param name="data">
        /// Specifies a pointer to data that will be copied into the data store for initialization, or Gl.L if no data is to be
        /// copied.
        /// </param>
        /// <param name="flags">
        /// Specifies the intended usage of the buffer's data store. Must be a bitwise combination of the following flags.
        /// Gl.DYNAMIC_STORAGE_BIT, Gl.MAP_READ_BITGl.MAP_WRITE_BIT, Gl.MAP_PERSISTENT_BIT, Gl.MAP_COHERENT_BIT, and
        /// Gl.CLIENT_STORAGE_BIT.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_direct_state_access", Api = "gl|glcore")]
        public static void NamedBufferStorage(uint buffer, uint size, IntPtr data, MapBufferUsageMask flags)
        {
            Debug.Assert(Delegates.pglNamedBufferStorage != null, "pglNamedBufferStorage not implemented");
            Delegates.pglNamedBufferStorage(buffer, size, data, (uint) flags);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glNamedBufferStorage: creates and initializes a buffer object's immutable data store
        /// </summary>
        /// <param name="buffer">
        /// Specifies the name of the buffer object for Gl.NamedBufferStorage function.
        /// </param>
        /// <param name="size">
        /// Specifies the size in bytes of the buffer object's new data store.
        /// </param>
        /// <param name="data">
        /// Specifies a pointer to data that will be copied into the data store for initialization, or Gl.L if no data is to be
        /// copied.
        /// </param>
        /// <param name="flags">
        /// Specifies the intended usage of the buffer's data store. Must be a bitwise combination of the following flags.
        /// Gl.DYNAMIC_STORAGE_BIT, Gl.MAP_READ_BITGl.MAP_WRITE_BIT, Gl.MAP_PERSISTENT_BIT, Gl.MAP_COHERENT_BIT, and
        /// Gl.CLIENT_STORAGE_BIT.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_direct_state_access", Api = "gl|glcore")]
        public static void NamedBufferStorage(uint buffer, uint size, object data, MapBufferUsageMask flags)
        {
            GCHandle pin_data = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                NamedBufferStorage(buffer, size, pin_data.AddrOfPinnedObject(), flags);
            }
            finally
            {
                pin_data.Free();
            }
        }

        /// <summary>
        /// [GL4] glNamedBufferData: creates and initializes a buffer object's data store
        /// </summary>
        /// <param name="buffer">
        /// Specifies the name of the buffer object for Gl.NamedBufferData function.
        /// </param>
        /// <param name="size">
        /// Specifies the size in bytes of the buffer object's new data store.
        /// </param>
        /// <param name="data">
        /// Specifies a pointer to data that will be copied into the data store for initialization, or Gl.L if no data is to be
        /// copied.
        /// </param>
        /// <param name="usage">
        /// Specifies the expected usage pattern of the data store. The symbolic constant must be Gl.STREAM_DRAW, Gl.STREAM_READ,
        /// Gl.STREAM_COPY, Gl.STATIC_DRAW, Gl.STATIC_READ, Gl.STATIC_COPY, Gl.DYNAMIC_DRAW, Gl.DYNAMIC_READ, or Gl.DYNAMIC_COPY.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void NamedBufferData(uint buffer, uint size, IntPtr data, BufferUsage usage)
        {
            Debug.Assert(Delegates.pglNamedBufferData != null, "pglNamedBufferData not implemented");
            Delegates.pglNamedBufferData(buffer, size, data, (int) usage);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glNamedBufferData: creates and initializes a buffer object's data store
        /// </summary>
        /// <param name="buffer">
        /// Specifies the name of the buffer object for Gl.NamedBufferData function.
        /// </param>
        /// <param name="size">
        /// Specifies the size in bytes of the buffer object's new data store.
        /// </param>
        /// <param name="data">
        /// Specifies a pointer to data that will be copied into the data store for initialization, or Gl.L if no data is to be
        /// copied.
        /// </param>
        /// <param name="usage">
        /// Specifies the expected usage pattern of the data store. The symbolic constant must be Gl.STREAM_DRAW, Gl.STREAM_READ,
        /// Gl.STREAM_COPY, Gl.STATIC_DRAW, Gl.STATIC_READ, Gl.STATIC_COPY, Gl.DYNAMIC_DRAW, Gl.DYNAMIC_READ, or Gl.DYNAMIC_COPY.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void NamedBufferData(uint buffer, uint size, object data, BufferUsage usage)
        {
            GCHandle pin_data = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                NamedBufferData(buffer, size, pin_data.AddrOfPinnedObject(), usage);
            }
            finally
            {
                pin_data.Free();
            }
        }

        /// <summary>
        /// [GL4] glNamedBufferSubData: updates a subset of a buffer object's data store
        /// </summary>
        /// <param name="buffer">
        /// Specifies the name of the buffer object for Gl.NamedBufferSubData.
        /// </param>
        /// <param name="offset">
        /// Specifies the offset into the buffer object's data store where data replacement will begin, measured in bytes.
        /// </param>
        /// <param name="size">
        /// Specifies the size in bytes of the data store region being replaced.
        /// </param>
        /// <param name="data">
        /// Specifies a pointer to the new data that will be copied into the data store.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_direct_state_access", Api = "gl|glcore")]
        public static void NamedBufferSubData(uint buffer, IntPtr offset, uint size, IntPtr data)
        {
            Debug.Assert(Delegates.pglNamedBufferSubData != null, "pglNamedBufferSubData not implemented");
            Delegates.pglNamedBufferSubData(buffer, offset, size, data);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glNamedBufferSubData: updates a subset of a buffer object's data store
        /// </summary>
        /// <param name="buffer">
        /// Specifies the name of the buffer object for Gl.NamedBufferSubData.
        /// </param>
        /// <param name="offset">
        /// Specifies the offset into the buffer object's data store where data replacement will begin, measured in bytes.
        /// </param>
        /// <param name="size">
        /// Specifies the size in bytes of the data store region being replaced.
        /// </param>
        /// <param name="data">
        /// Specifies a pointer to the new data that will be copied into the data store.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_direct_state_access", Api = "gl|glcore")]
        public static void NamedBufferSubData(uint buffer, IntPtr offset, uint size, object data)
        {
            GCHandle pin_data = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                NamedBufferSubData(buffer, offset, size, pin_data.AddrOfPinnedObject());
            }
            finally
            {
                pin_data.Free();
            }
        }

        /// <summary>
        /// [GL4] glCopyNamedBufferSubData: copy all or part of the data store of a buffer object to the data store of another
        /// buffer object
        /// </summary>
        /// <param name="readBuffer">
        /// Specifies the name of the source buffer object for Gl.CopyNamedBufferSubData.
        /// </param>
        /// <param name="writeBuffer">
        /// Specifies the name of the destination buffer object for Gl.CopyNamedBufferSubData.
        /// </param>
        /// <param name="readOffset">
        /// Specifies the offset, in basic machine units, within the data store of the source buffer object at which data will be
        /// read.
        /// </param>
        /// <param name="writeOffset">
        /// Specifies the offset, in basic machine units, within the data store of the destination buffer object at which data will
        /// be written.
        /// </param>
        /// <param name="size">
        /// Specifies the size, in basic machine units, of the data to be copied from the source buffer object to the destination
        /// buffer object.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void CopyNamedBufferSubData(uint readBuffer, uint writeBuffer, IntPtr readOffset, IntPtr writeOffset, uint size)
        {
            Debug.Assert(Delegates.pglCopyNamedBufferSubData != null, "pglCopyNamedBufferSubData not implemented");
            Delegates.pglCopyNamedBufferSubData(readBuffer, writeBuffer, readOffset, writeOffset, size);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glClearNamedBufferData: fill a buffer object's data store with a fixed value
        /// </summary>
        /// <param name="buffer">
        /// Specifies the name of the buffer object for Gl.ClearNamedBufferData.
        /// </param>
        /// <param name="internalformat">
        /// The internal format with which the data will be stored in the buffer object.
        /// </param>
        /// <param name="format">
        /// The format of the data in memory addressed by <paramref name="data" />.
        /// </param>
        /// <param name="type">
        /// The type of the data in memory addressed by <paramref name="data" />.
        /// </param>
        /// <param name="data">
        /// The address of a memory location storing the data to be replicated into the buffer's data store.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void ClearNamedBufferData(uint buffer, InternalFormat internalformat, PixelFormat format, PixelType type, IntPtr data)
        {
            Debug.Assert(Delegates.pglClearNamedBufferData != null, "pglClearNamedBufferData not implemented");
            Delegates.pglClearNamedBufferData(buffer, (int) internalformat, (int) format, (int) type, data);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glClearNamedBufferData: fill a buffer object's data store with a fixed value
        /// </summary>
        /// <param name="buffer">
        /// Specifies the name of the buffer object for Gl.ClearNamedBufferData.
        /// </param>
        /// <param name="internalformat">
        /// The internal format with which the data will be stored in the buffer object.
        /// </param>
        /// <param name="format">
        /// The format of the data in memory addressed by <paramref name="data" />.
        /// </param>
        /// <param name="type">
        /// The type of the data in memory addressed by <paramref name="data" />.
        /// </param>
        /// <param name="data">
        /// The address of a memory location storing the data to be replicated into the buffer's data store.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void ClearNamedBufferData(uint buffer, InternalFormat internalformat, PixelFormat format, PixelType type, object data)
        {
            GCHandle pin_data = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                ClearNamedBufferData(buffer, internalformat, format, type, pin_data.AddrOfPinnedObject());
            }
            finally
            {
                pin_data.Free();
            }
        }

        /// <summary>
        /// [GL4] glClearNamedBufferSubData: fill all or part of buffer object's data store with a fixed value
        /// </summary>
        /// <param name="buffer">
        /// Specifies the name of the buffer object for Gl.ClearNamedBufferSubData.
        /// </param>
        /// <param name="internalformat">
        /// The internal format with which the data will be stored in the buffer object.
        /// </param>
        /// <param name="offset">
        /// The offset in basic machine units into the buffer object's data store at which to start filling.
        /// </param>
        /// <param name="size">
        /// The size in basic machine units of the range of the data store to fill.
        /// </param>
        /// <param name="format">
        /// The format of the data in memory addressed by <paramref name="data" />.
        /// </param>
        /// <param name="type">
        /// The type of the data in memory addressed by <paramref name="data" />.
        /// </param>
        /// <param name="data">
        /// The address of a memory location storing the data to be replicated into the buffer's data store.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void ClearNamedBufferSubData(uint buffer, InternalFormat internalformat, IntPtr offset, uint size, PixelFormat format, PixelType type, IntPtr data)
        {
            Debug.Assert(Delegates.pglClearNamedBufferSubData != null, "pglClearNamedBufferSubData not implemented");
            Delegates.pglClearNamedBufferSubData(buffer, (int) internalformat, offset, size, (int) format, (int) type, data);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glClearNamedBufferSubData: fill all or part of buffer object's data store with a fixed value
        /// </summary>
        /// <param name="buffer">
        /// Specifies the name of the buffer object for Gl.ClearNamedBufferSubData.
        /// </param>
        /// <param name="internalformat">
        /// The internal format with which the data will be stored in the buffer object.
        /// </param>
        /// <param name="offset">
        /// The offset in basic machine units into the buffer object's data store at which to start filling.
        /// </param>
        /// <param name="size">
        /// The size in basic machine units of the range of the data store to fill.
        /// </param>
        /// <param name="format">
        /// The format of the data in memory addressed by <paramref name="data" />.
        /// </param>
        /// <param name="type">
        /// The type of the data in memory addressed by <paramref name="data" />.
        /// </param>
        /// <param name="data">
        /// The address of a memory location storing the data to be replicated into the buffer's data store.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void ClearNamedBufferSubData(uint buffer, InternalFormat internalformat, IntPtr offset, uint size, PixelFormat format, PixelType type, object data)
        {
            GCHandle pin_data = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                ClearNamedBufferSubData(buffer, internalformat, offset, size, format, type, pin_data.AddrOfPinnedObject());
            }
            finally
            {
                pin_data.Free();
            }
        }

        /// <summary>
        /// [GL4] glMapNamedBuffer: map all of a buffer object's data store into the client's address space
        /// </summary>
        /// <param name="buffer">
        /// Specifies the name of the buffer object for Gl.MapNamedBuffer.
        /// </param>
        /// <param name="access">
        /// Specifies the access policy for Gl.MapBuffer and Gl.MapNamedBuffer, indicating whether it will be possible to read
        /// from,
        /// write to, or both read from and write to the buffer object's mapped data store. The symbolic constant must be
        /// Gl.READ_ONLY, Gl.WRITE_ONLY, or Gl.READ_WRITE.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static IntPtr MapNamedBuffer(uint buffer, BufferAccess access)
        {
            IntPtr retValue;

            Debug.Assert(Delegates.pglMapNamedBuffer != null, "pglMapNamedBuffer not implemented");
            retValue = Delegates.pglMapNamedBuffer(buffer, (int) access);
            DebugCheckErrors(retValue);

            return retValue;
        }

        /// <summary>
        /// [GL4] glMapNamedBufferRange: map all or part of a buffer object's data store into the client's address space
        /// </summary>
        /// <param name="buffer">
        /// Specifies the name of the buffer object for Gl.MapNamedBufferRange.
        /// </param>
        /// <param name="offset">
        /// Specifies the starting offset within the buffer of the range to be mapped.
        /// </param>
        /// <param name="length">
        /// Specifies the length of the range to be mapped.
        /// </param>
        /// <param name="access">
        /// Specifies a combination of access flags indicating the desired access to the mapped range.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static IntPtr MapNamedBufferRange(uint buffer, IntPtr offset, uint length, BufferAccessMask access)
        {
            IntPtr retValue;

            Debug.Assert(Delegates.pglMapNamedBufferRange != null, "pglMapNamedBufferRange not implemented");
            retValue = Delegates.pglMapNamedBufferRange(buffer, offset, length, (uint) access);
            DebugCheckErrors(retValue);

            return retValue;
        }

        /// <summary>
        /// [GL4] glUnmapNamedBuffer: release the mapping of a buffer object's data store into the client's address space
        /// </summary>
        /// <param name="buffer">
        /// Specifies the name of the buffer object for Gl.UnmapNamedBuffer.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static bool UnmapNamedBuffer(uint buffer)
        {
            bool retValue;

            Debug.Assert(Delegates.pglUnmapNamedBuffer != null, "pglUnmapNamedBuffer not implemented");
            retValue = Delegates.pglUnmapNamedBuffer(buffer);
            DebugCheckErrors(retValue);

            return retValue;
        }

        /// <summary>
        /// [GL4] glFlushMappedNamedBufferRange: indicate modifications to a range of a mapped buffer
        /// </summary>
        /// <param name="buffer">
        /// Specifies the name of the buffer object for Gl.FlushMappedNamedBufferRange.
        /// </param>
        /// <param name="offset">
        /// Specifies the start of the buffer subrange, in basic machine units.
        /// </param>
        /// <param name="length">
        /// Specifies the length of the buffer subrange, in basic machine units.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void FlushMappedNamedBufferRange(uint buffer, IntPtr offset, uint length)
        {
            Debug.Assert(Delegates.pglFlushMappedNamedBufferRange != null, "pglFlushMappedNamedBufferRange not implemented");
            Delegates.pglFlushMappedNamedBufferRange(buffer, offset, length);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetNamedBufferParameteriv: return parameters of a buffer object
        /// </summary>
        /// <param name="buffer">
        /// Specifies the name of the buffer object for Gl.GetNamedBufferParameteriv and Gl.GetNamedBufferParameteri64v.
        /// </param>
        /// <param name="value">
        /// Specifies the name of the buffer object parameter to query.
        /// </param>
        /// <param name="data">
        /// Returns the requested parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetNamedBufferParameter(uint buffer, VertexBufferObjectParameter value, [Out] int[] data)
        {
            unsafe
            {
                fixed (int* p_params = data)
                {
                    Debug.Assert(Delegates.pglGetNamedBufferParameteriv != null, "pglGetNamedBufferParameteriv not implemented");
                    Delegates.pglGetNamedBufferParameteriv(buffer, (int) value, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetNamedBufferParameteriv: return parameters of a buffer object
        /// </summary>
        /// <param name="buffer">
        /// Specifies the name of the buffer object for Gl.GetNamedBufferParameteriv and Gl.GetNamedBufferParameteri64v.
        /// </param>
        /// <param name="value">
        /// Specifies the name of the buffer object parameter to query.
        /// </param>
        /// <param name="data">
        /// Returns the requested parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetNamedBufferParameter(uint buffer, VertexBufferObjectParameter value, out int data)
        {
            unsafe
            {
                fixed (int* p_params = &data)
                {
                    Debug.Assert(Delegates.pglGetNamedBufferParameteriv != null, "pglGetNamedBufferParameteriv not implemented");
                    Delegates.pglGetNamedBufferParameteriv(buffer, (int) value, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetNamedBufferParameteriv: return parameters of a buffer object
        /// </summary>
        /// <param name="buffer">
        /// Specifies the name of the buffer object for Gl.GetNamedBufferParameteriv and Gl.GetNamedBufferParameteri64v.
        /// </param>
        /// <param name="value">
        /// Specifies the name of the buffer object parameter to query.
        /// </param>
        /// <param name="data">
        /// Returns the requested parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static unsafe void GetNamedBufferParameter(uint buffer, VertexBufferObjectParameter value, [Out] int* data)
        {
            Debug.Assert(Delegates.pglGetNamedBufferParameteriv != null, "pglGetNamedBufferParameteriv not implemented");
            Delegates.pglGetNamedBufferParameteriv(buffer, (int) value, data);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetNamedBufferParameteriv: return parameters of a buffer object
        /// </summary>
        /// <param name="buffer">
        /// Specifies the name of the buffer object for Gl.GetNamedBufferParameteriv and Gl.GetNamedBufferParameteri64v.
        /// </param>
        /// <param name="value">
        /// Specifies the name of the buffer object parameter to query.
        /// </param>
        /// <param name="data">
        /// Returns the requested parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetNamedBufferParameteri<T>(uint buffer, VertexBufferObjectParameter value, out T data) where T : struct
        {
            Debug.Assert(Delegates.pglGetNamedBufferParameteriv != null, "pglGetNamedBufferParameteriv not implemented");
            data = default;
            unsafe
            {
                TypedReference refParams = __makeref(data);
                IntPtr refParamsPtr = *(IntPtr*) (&refParams);

                Delegates.pglGetNamedBufferParameteriv(buffer, (int) value, (int*) refParamsPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetNamedBufferParameteri64v: return parameters of a buffer object
        /// </summary>
        /// <param name="buffer">
        /// Specifies the name of the buffer object for Gl.GetNamedBufferParameteriv and Gl.GetNamedBufferParameteri64v.
        /// </param>
        /// <param name="value">
        /// Specifies the name of the buffer object parameter to query.
        /// </param>
        /// <param name="data">
        /// Returns the requested parameter.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetNamedBufferParameter(uint buffer, VertexBufferObjectParameter value, [Out] long[] data)
        {
            unsafe
            {
                fixed (long* p_params = data)
                {
                    Debug.Assert(Delegates.pglGetNamedBufferParameteri64v != null, "pglGetNamedBufferParameteri64v not implemented");
                    Delegates.pglGetNamedBufferParameteri64v(buffer, (int) value, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetNamedBufferPointerv: return the pointer to a mapped buffer object's data store
        /// </summary>
        /// <param name="buffer">
        /// Specifies the name of the buffer object for Gl.GetNamedBufferPointerv.
        /// </param>
        /// <param name="pname">
        /// Specifies the name of the pointer to be returned. Must be Gl.BUFFER_MAP_POINTER.
        /// </param>
        /// <param name="params">
        /// Returns the pointer value specified by <paramref name="pname" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetNamedBufferPointer(uint buffer, VertexBufferObjectParameter pname, [Out] IntPtr[] @params)
        {
            unsafe
            {
                fixed (IntPtr* p_params = @params)
                {
                    Debug.Assert(Delegates.pglGetNamedBufferPointerv != null, "pglGetNamedBufferPointerv not implemented");
                    Delegates.pglGetNamedBufferPointerv(buffer, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetNamedBufferSubData: returns a subset of a buffer object's data store
        /// </summary>
        /// <param name="buffer">
        /// Specifies the name of the buffer object for Gl.GetNamedBufferSubData.
        /// </param>
        /// <param name="offset">
        /// Specifies the offset into the buffer object's data store from which data will be returned, measured in bytes.
        /// </param>
        /// <param name="size">
        /// Specifies the size in bytes of the data store region being returned.
        /// </param>
        /// <param name="data">
        /// Specifies a pointer to the location where buffer object data is returned.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetNamedBufferSubData(uint buffer, IntPtr offset, uint size, IntPtr data)
        {
            Debug.Assert(Delegates.pglGetNamedBufferSubData != null, "pglGetNamedBufferSubData not implemented");
            Delegates.pglGetNamedBufferSubData(buffer, offset, size, data);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetNamedBufferSubData: returns a subset of a buffer object's data store
        /// </summary>
        /// <param name="buffer">
        /// Specifies the name of the buffer object for Gl.GetNamedBufferSubData.
        /// </param>
        /// <param name="offset">
        /// Specifies the offset into the buffer object's data store from which data will be returned, measured in bytes.
        /// </param>
        /// <param name="size">
        /// Specifies the size in bytes of the data store region being returned.
        /// </param>
        /// <param name="data">
        /// Specifies a pointer to the location where buffer object data is returned.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetNamedBufferSubData(uint buffer, IntPtr offset, uint size, object data)
        {
            GCHandle pin_data = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                GetNamedBufferSubData(buffer, offset, size, pin_data.AddrOfPinnedObject());
            }
            finally
            {
                pin_data.Free();
            }
        }

        /// <summary>
        /// [GL4] glCreateFramebuffers: create framebuffer objects
        /// </summary>
        /// <param name="framebuffers">
        /// Specifies an array in which names of the new framebuffer objects are stored.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void CreateFramebuffers(uint[] framebuffers)
        {
            unsafe
            {
                fixed (uint* p_framebuffers = framebuffers)
                {
                    Debug.Assert(Delegates.pglCreateFramebuffers != null, "pglCreateFramebuffers not implemented");
                    Delegates.pglCreateFramebuffers(framebuffers.Length, p_framebuffers);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glCreateFramebuffers: create framebuffer objects
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static uint CreateFramebuffer()
        {
            uint retValue;
            unsafe
            {
                Delegates.pglCreateFramebuffers(1, &retValue);
            }

            DebugCheckErrors(null);
            return retValue;
        }

        /// <summary>
        /// [GL4] glNamedFramebufferRenderbuffer: attach a renderbuffer as a logical buffer of a framebuffer object
        /// </summary>
        /// <param name="framebuffer">
        /// Specifies the name of the framebuffer object for Gl.NamedFramebufferRenderbuffer.
        /// </param>
        /// <param name="attachment">
        /// Specifies the attachment point of the framebuffer.
        /// </param>
        /// <param name="renderbuffertarget">
        /// Specifies the renderbuffer target. Must be Gl.RENDERBUFFER.
        /// </param>
        /// <param name="renderbuffer">
        /// Specifies the name of an existing renderbuffer object of type <paramref name="renderbuffertarget" /> to attach.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void NamedFramebufferRenderbuffer(uint framebuffer, FramebufferAttachment attachment, RenderbufferTarget renderbuffertarget, uint renderbuffer)
        {
            Debug.Assert(Delegates.pglNamedFramebufferRenderbuffer != null, "pglNamedFramebufferRenderbuffer not implemented");
            Delegates.pglNamedFramebufferRenderbuffer(framebuffer, (int) attachment, (int) renderbuffertarget, renderbuffer);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glNamedFramebufferParameteri: set a named parameter of a framebuffer object
        /// </summary>
        /// <param name="framebuffer">
        /// Specifies the name of the framebuffer object for Gl.NamedFramebufferParameteri.
        /// </param>
        /// <param name="pname">
        /// Specifies the framebuffer parameter to be modified.
        /// </param>
        /// <param name="param">
        /// The new value for the parameter named <paramref name="pname" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void NamedFramebufferParameter(uint framebuffer, FramebufferParameterName pname, int param)
        {
            Debug.Assert(Delegates.pglNamedFramebufferParameteri != null, "pglNamedFramebufferParameteri not implemented");
            Delegates.pglNamedFramebufferParameteri(framebuffer, (int) pname, param);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glNamedFramebufferTexture: attach a level of a texture object as a logical buffer of a framebuffer object
        /// </summary>
        /// <param name="framebuffer">
        /// Specifies the name of the framebuffer object for Gl.NamedFramebufferTexture.
        /// </param>
        /// <param name="attachment">
        /// Specifies the attachment point of the framebuffer.
        /// </param>
        /// <param name="texture">
        /// Specifies the name of an existing texture object to attach.
        /// </param>
        /// <param name="level">
        /// Specifies the mipmap level of the texture object to attach.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void NamedFramebufferTexture(uint framebuffer, FramebufferAttachment attachment, uint texture, int level)
        {
            Debug.Assert(Delegates.pglNamedFramebufferTexture != null, "pglNamedFramebufferTexture not implemented");
            Delegates.pglNamedFramebufferTexture(framebuffer, (int) attachment, texture, level);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glNamedFramebufferTextureLayer: attach a single layer of a texture object as a logical buffer of a framebuffer
        /// object
        /// </summary>
        /// <param name="framebuffer">
        /// Specifies the name of the framebuffer object for Gl.NamedFramebufferTextureLayer.
        /// </param>
        /// <param name="attachment">
        /// Specifies the attachment point of the framebuffer.
        /// </param>
        /// <param name="texture">
        /// Specifies the name of an existing texture object to attach.
        /// </param>
        /// <param name="level">
        /// Specifies the mipmap level of the texture object to attach.
        /// </param>
        /// <param name="layer">
        /// Specifies the layer of the texture object to attach.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void NamedFramebufferTextureLayer(uint framebuffer, FramebufferAttachment attachment, uint texture, int level, int layer)
        {
            Debug.Assert(Delegates.pglNamedFramebufferTextureLayer != null, "pglNamedFramebufferTextureLayer not implemented");
            Delegates.pglNamedFramebufferTextureLayer(framebuffer, (int) attachment, texture, level, layer);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glNamedFramebufferDrawBuffer: specify which color buffers are to be drawn into
        /// </summary>
        /// <param name="framebuffer">
        /// Specifies the name of the framebuffer object for Gl.NamedFramebufferDrawBuffer function. Must be zero or the name of a
        /// framebuffer object.
        /// </param>
        /// <param name="buf">
        /// For default framebuffer, the argument specifies up to four color buffers to be drawn into. Symbolic constants Gl.NONE,
        /// Gl.FRONT_LEFT, Gl.FRONT_RIGHT, Gl.BACK_LEFT, Gl.BACK_RIGHT, Gl.FRONT, Gl.BACK, Gl.LEFT, Gl.RIGHT, and Gl.FRONT_AND_BACK
        /// are accepted. The initial value is Gl.FRONT for single-buffered contexts, and Gl.BACK for double-buffered contexts. For
        /// framebuffer objects, Gl.COLOR_ATTACHMENT$m$ and Gl.NONE enums are accepted, where Gl. is a value between 0 and
        /// Gl.MAX_COLOR_ATTACHMENTS.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void NamedFramebufferDrawBuffer(uint framebuffer, ColorBuffer buf)
        {
            Debug.Assert(Delegates.pglNamedFramebufferDrawBuffer != null, "pglNamedFramebufferDrawBuffer not implemented");
            Delegates.pglNamedFramebufferDrawBuffer(framebuffer, (int) buf);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glNamedFramebufferDrawBuffers: Specifies a list of color buffers to be drawn into
        /// </summary>
        /// <param name="framebuffer">
        /// Specifies the name of the framebuffer object for Gl.NamedFramebufferDrawBuffers.
        /// </param>
        /// <param name="n">
        /// Specifies the number of buffers in <paramref name="bufs" />.
        /// </param>
        /// <param name="bufs">
        /// Points to an array of symbolic constants specifying the buffers into which fragment colors or data values will be
        /// written.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void NamedFramebufferDrawBuffers(uint framebuffer, int n, int[] bufs)
        {
            unsafe
            {
                fixed (int* p_bufs = bufs)
                {
                    Debug.Assert(Delegates.pglNamedFramebufferDrawBuffers != null, "pglNamedFramebufferDrawBuffers not implemented");
                    Delegates.pglNamedFramebufferDrawBuffers(framebuffer, n, p_bufs);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glNamedFramebufferReadBuffer: select a color buffer source for pixels
        /// </summary>
        /// <param name="framebuffer">
        /// Specifies the name of the framebuffer object for Gl.NamedFramebufferReadBuffer function.
        /// </param>
        /// <param name="mode">
        /// Specifies a color buffer. Accepted values are Gl.FRONT_LEFT, Gl.FRONT_RIGHT, Gl.BACK_LEFT, Gl.BACK_RIGHT, Gl.FRONT,
        /// Gl.BACK, Gl.LEFT, Gl.RIGHT, and the constants Gl.COLOR_ATTACHMENTi.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void NamedFramebufferReadBuffer(uint framebuffer, ColorBuffer mode)
        {
            Debug.Assert(Delegates.pglNamedFramebufferReadBuffer != null, "pglNamedFramebufferReadBuffer not implemented");
            Delegates.pglNamedFramebufferReadBuffer(framebuffer, (int) mode);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glInvalidateNamedFramebufferData: invalidate the content of some or all of a framebuffer's attachments
        /// </summary>
        /// <param name="framebuffer">
        /// Specifies the name of the framebuffer object for Gl.InvalidateNamedFramebufferData.
        /// </param>
        /// <param name="numAttachments">
        /// Specifies the number of entries in the <paramref name="attachments" /> array.
        /// </param>
        /// <param name="attachments">
        /// Specifies a pointer to an array identifying the attachments to be invalidated.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void InvalidateNamedFramebufferData(uint framebuffer, int numAttachments, int[] attachments)
        {
            unsafe
            {
                fixed (int* p_attachments = attachments)
                {
                    Debug.Assert(Delegates.pglInvalidateNamedFramebufferData != null, "pglInvalidateNamedFramebufferData not implemented");
                    Delegates.pglInvalidateNamedFramebufferData(framebuffer, numAttachments, p_attachments);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glInvalidateNamedFramebufferSubData: invalidate the content of a region of some or all of a framebuffer's
        /// attachments
        /// </summary>
        /// <param name="framebuffer">
        /// Specifies the name of the framebuffer object for Gl.InvalidateNamedFramebufferSubData.
        /// </param>
        /// <param name="numAttachments">
        /// Specifies the number of entries in the <paramref name="attachments" /> array.
        /// </param>
        /// <param name="attachments">
        /// Specifies a pointer to an array identifying the attachments to be invalidated.
        /// </param>
        /// <param name="x">
        /// Specifies the X offset of the region to be invalidated.
        /// </param>
        /// <param name="y">
        /// Specifies the Y offset of the region to be invalidated.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the region to be invalidated.
        /// </param>
        /// <param name="height">
        /// Specifies the height of the region to be invalidated.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void InvalidateNamedFramebufferSubData(uint framebuffer, int numAttachments, int[] attachments, int x, int y, int width, int height)
        {
            unsafe
            {
                fixed (int* p_attachments = attachments)
                {
                    Debug.Assert(Delegates.pglInvalidateNamedFramebufferSubData != null, "pglInvalidateNamedFramebufferSubData not implemented");
                    Delegates.pglInvalidateNamedFramebufferSubData(framebuffer, numAttachments, p_attachments, x, y, width, height);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glClearNamedFramebufferiv: clear individual buffers of a framebuffer
        /// </summary>
        /// <param name="framebuffer">
        /// Specifies the name of the framebuffer object for Gl.ClearNamedFramebuffer*.
        /// </param>
        /// <param name="buffer">
        /// Specify the buffer to clear.
        /// </param>
        /// <param name="drawbuffer">
        /// Specify a particular draw buffer to clear.
        /// </param>
        /// <param name="value">
        /// A pointer to the value or values to clear the buffer to.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void ClearNamedFramebuffer(uint framebuffer, Buffer buffer, int drawbuffer, int[] value)
        {
            unsafe
            {
                fixed (int* p_value = value)
                {
                    Debug.Assert(Delegates.pglClearNamedFramebufferiv != null, "pglClearNamedFramebufferiv not implemented");
                    Delegates.pglClearNamedFramebufferiv(framebuffer, (int) buffer, drawbuffer, p_value);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glClearNamedFramebufferuiv: clear individual buffers of a framebuffer
        /// </summary>
        /// <param name="framebuffer">
        /// Specifies the name of the framebuffer object for Gl.ClearNamedFramebuffer*.
        /// </param>
        /// <param name="buffer">
        /// Specify the buffer to clear.
        /// </param>
        /// <param name="drawbuffer">
        /// Specify a particular draw buffer to clear.
        /// </param>
        /// <param name="value">
        /// A pointer to the value or values to clear the buffer to.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void ClearNamedFramebuffer(uint framebuffer, Buffer buffer, int drawbuffer, uint[] value)
        {
            unsafe
            {
                fixed (uint* p_value = value)
                {
                    Debug.Assert(Delegates.pglClearNamedFramebufferuiv != null, "pglClearNamedFramebufferuiv not implemented");
                    Delegates.pglClearNamedFramebufferuiv(framebuffer, (int) buffer, drawbuffer, p_value);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glClearNamedFramebufferfv: clear individual buffers of a framebuffer
        /// </summary>
        /// <param name="framebuffer">
        /// Specifies the name of the framebuffer object for Gl.ClearNamedFramebuffer*.
        /// </param>
        /// <param name="buffer">
        /// Specify the buffer to clear.
        /// </param>
        /// <param name="drawbuffer">
        /// Specify a particular draw buffer to clear.
        /// </param>
        /// <param name="value">
        /// A pointer to the value or values to clear the buffer to.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void ClearNamedFramebuffer(uint framebuffer, Buffer buffer, int drawbuffer, float[] value)
        {
            unsafe
            {
                fixed (float* p_value = value)
                {
                    Debug.Assert(Delegates.pglClearNamedFramebufferfv != null, "pglClearNamedFramebufferfv not implemented");
                    Delegates.pglClearNamedFramebufferfv(framebuffer, (int) buffer, drawbuffer, p_value);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glClearNamedFramebufferfi: clear individual buffers of a framebuffer
        /// </summary>
        /// <param name="framebuffer">
        /// Specifies the name of the framebuffer object for Gl.ClearNamedFramebuffer*.
        /// </param>
        /// <param name="buffer">
        /// Specify the buffer to clear.
        /// </param>
        /// <param name="drawbuffer">
        /// Specify a particular draw buffer to clear.
        /// </param>
        /// <param name="depth">
        /// The value to clear the depth buffer to.
        /// </param>
        /// <param name="stencil">
        /// The value to clear the stencil buffer to.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void ClearNamedFramebuffer(uint framebuffer, Buffer buffer, int drawbuffer, float depth, int stencil)
        {
            Debug.Assert(Delegates.pglClearNamedFramebufferfi != null, "pglClearNamedFramebufferfi not implemented");
            Delegates.pglClearNamedFramebufferfi(framebuffer, (int) buffer, drawbuffer, depth, stencil);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glBlitNamedFramebuffer: copy a block of pixels from one framebuffer object to another
        /// </summary>
        /// <param name="readFramebuffer">
        /// Specifies the name of the source framebuffer object for Gl.BlitNamedFramebuffer.
        /// </param>
        /// <param name="drawFramebuffer">
        /// Specifies the name of the destination framebuffer object for Gl.BlitNamedFramebuffer.
        /// </param>
        /// <param name="srcX0">
        /// Specify the bounds of the source rectangle within the read buffer of the read framebuffer.
        /// </param>
        /// <param name="srcY0">
        /// Specify the bounds of the source rectangle within the read buffer of the read framebuffer.
        /// </param>
        /// <param name="srcX1">
        /// Specify the bounds of the source rectangle within the read buffer of the read framebuffer.
        /// </param>
        /// <param name="srcY1">
        /// Specify the bounds of the source rectangle within the read buffer of the read framebuffer.
        /// </param>
        /// <param name="dstX0">
        /// Specify the bounds of the destination rectangle within the write buffer of the write framebuffer.
        /// </param>
        /// <param name="dstY0">
        /// Specify the bounds of the destination rectangle within the write buffer of the write framebuffer.
        /// </param>
        /// <param name="dstX1">
        /// Specify the bounds of the destination rectangle within the write buffer of the write framebuffer.
        /// </param>
        /// <param name="dstY1">
        /// Specify the bounds of the destination rectangle within the write buffer of the write framebuffer.
        /// </param>
        /// <param name="mask">
        /// The bitwise OR of the flags indicating which buffers are to be copied. The allowed flags are Gl.COLOR_BUFFER_BIT,
        /// Gl.DEPTH_BUFFER_BIT and Gl.STENCIL_BUFFER_BIT.
        /// </param>
        /// <param name="filter">
        /// Specifies the interpolation to be applied if the image is stretched. Must be Gl.NEAREST or Gl.LINEAR.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void BlitNamedFramebuffer(uint readFramebuffer, uint drawFramebuffer, int srcX0, int srcY0, int srcX1, int srcY1, int dstX0, int dstY0, int dstX1, int dstY1,
            ClearBufferMask mask, BlitFramebufferFilter filter)
        {
            Debug.Assert(Delegates.pglBlitNamedFramebuffer != null, "pglBlitNamedFramebuffer not implemented");
            Delegates.pglBlitNamedFramebuffer(readFramebuffer, drawFramebuffer, srcX0, srcY0, srcX1, srcY1, dstX0, dstY0, dstX1, dstY1, (uint) mask, (int) filter);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glCheckNamedFramebufferStatus: check the completeness status of a framebuffer
        /// </summary>
        /// <param name="framebuffer">
        /// Specifies the name of the framebuffer object for Gl.CheckNamedFramebufferStatus
        /// </param>
        /// <param name="target">
        /// Specify the target to which the framebuffer is bound for Gl.CheckFramebufferStatus, and the target against which
        /// framebuffer completeness of <paramref name="framebuffer" /> is checked for Gl.CheckNamedFramebufferStatus.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static FramebufferStatus CheckNamedFramebufferStatus(uint framebuffer, FramebufferTarget target)
        {
            int retValue;

            Debug.Assert(Delegates.pglCheckNamedFramebufferStatus != null, "pglCheckNamedFramebufferStatus not implemented");
            retValue = Delegates.pglCheckNamedFramebufferStatus(framebuffer, (int) target);
            DebugCheckErrors(retValue);

            return (FramebufferStatus) retValue;
        }

        /// <summary>
        /// [GL4] glGetNamedFramebufferParameteriv: query a named parameter of a framebuffer object
        /// </summary>
        /// <param name="framebuffer">
        /// Specifies the name of the framebuffer object for Gl.GetNamedFramebufferParameteriv.
        /// </param>
        /// <param name="pname">
        /// Specifies the parameter of the framebuffer object to query.
        /// </param>
        /// <param name="param">
        /// A <see cref="T:int[]" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetNamedFramebufferParameter(uint framebuffer, GetFramebufferParameter pname, [Out] int[] param)
        {
            unsafe
            {
                fixed (int* p_param = param)
                {
                    Debug.Assert(Delegates.pglGetNamedFramebufferParameteriv != null, "pglGetNamedFramebufferParameteriv not implemented");
                    Delegates.pglGetNamedFramebufferParameteriv(framebuffer, (int) pname, p_param);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetNamedFramebufferParameteriv: query a named parameter of a framebuffer object
        /// </summary>
        /// <param name="framebuffer">
        /// Specifies the name of the framebuffer object for Gl.GetNamedFramebufferParameteriv.
        /// </param>
        /// <param name="pname">
        /// Specifies the parameter of the framebuffer object to query.
        /// </param>
        /// <param name="param">
        /// A <see cref="T:int" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetNamedFramebufferParameter(uint framebuffer, GetFramebufferParameter pname, out int param)
        {
            unsafe
            {
                fixed (int* p_param = &param)
                {
                    Debug.Assert(Delegates.pglGetNamedFramebufferParameteriv != null, "pglGetNamedFramebufferParameteriv not implemented");
                    Delegates.pglGetNamedFramebufferParameteriv(framebuffer, (int) pname, p_param);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetNamedFramebufferParameteriv: query a named parameter of a framebuffer object
        /// </summary>
        /// <param name="framebuffer">
        /// Specifies the name of the framebuffer object for Gl.GetNamedFramebufferParameteriv.
        /// </param>
        /// <param name="pname">
        /// Specifies the parameter of the framebuffer object to query.
        /// </param>
        /// <param name="param">
        /// A <see cref="T:int*" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static unsafe void GetNamedFramebufferParameter(uint framebuffer, GetFramebufferParameter pname, [Out] int* param)
        {
            Debug.Assert(Delegates.pglGetNamedFramebufferParameteriv != null, "pglGetNamedFramebufferParameteriv not implemented");
            Delegates.pglGetNamedFramebufferParameteriv(framebuffer, (int) pname, param);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetNamedFramebufferParameteriv: query a named parameter of a framebuffer object
        /// </summary>
        /// <param name="framebuffer">
        /// Specifies the name of the framebuffer object for Gl.GetNamedFramebufferParameteriv.
        /// </param>
        /// <param name="pname">
        /// Specifies the parameter of the framebuffer object to query.
        /// </param>
        /// <param name="param">
        /// A <see cref="T:T" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetNamedFramebufferParameteri<T>(uint framebuffer, GetFramebufferParameter pname, out T param) where T : struct
        {
            Debug.Assert(Delegates.pglGetNamedFramebufferParameteriv != null, "pglGetNamedFramebufferParameteriv not implemented");
            param = default;
            unsafe
            {
                TypedReference refParam = __makeref(param);
                IntPtr refParamPtr = *(IntPtr*) (&refParam);

                Delegates.pglGetNamedFramebufferParameteriv(framebuffer, (int) pname, (int*) refParamPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetNamedFramebufferAttachmentParameteriv: retrieve information about attachments of a framebuffer object
        /// </summary>
        /// <param name="framebuffer">
        /// Specifies the name of the framebuffer object for Gl.GetNamedFramebufferAttachmentParameteriv.
        /// </param>
        /// <param name="attachment">
        /// Specifies the attachment of the framebuffer object to query.
        /// </param>
        /// <param name="pname">
        /// Specifies the parameter of <paramref name="attachment" /> to query.
        /// </param>
        /// <param name="params">
        /// Returns the value of parameter <paramref name="pname" /> for <paramref name="attachment" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetNamedFramebufferAttachmentParameter(uint framebuffer, FramebufferAttachment attachment, FramebufferAttachmentParameterName pname, [Out] int[] @params)
        {
            unsafe
            {
                fixed (int* p_params = @params)
                {
                    Debug.Assert(Delegates.pglGetNamedFramebufferAttachmentParameteriv != null, "pglGetNamedFramebufferAttachmentParameteriv not implemented");
                    Delegates.pglGetNamedFramebufferAttachmentParameteriv(framebuffer, (int) attachment, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetNamedFramebufferAttachmentParameteriv: retrieve information about attachments of a framebuffer object
        /// </summary>
        /// <param name="framebuffer">
        /// Specifies the name of the framebuffer object for Gl.GetNamedFramebufferAttachmentParameteriv.
        /// </param>
        /// <param name="attachment">
        /// Specifies the attachment of the framebuffer object to query.
        /// </param>
        /// <param name="pname">
        /// Specifies the parameter of <paramref name="attachment" /> to query.
        /// </param>
        /// <param name="params">
        /// Returns the value of parameter <paramref name="pname" /> for <paramref name="attachment" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetNamedFramebufferAttachmentParameter(uint framebuffer, FramebufferAttachment attachment, FramebufferAttachmentParameterName pname, out int @params)
        {
            unsafe
            {
                fixed (int* p_params = &@params)
                {
                    Debug.Assert(Delegates.pglGetNamedFramebufferAttachmentParameteriv != null, "pglGetNamedFramebufferAttachmentParameteriv not implemented");
                    Delegates.pglGetNamedFramebufferAttachmentParameteriv(framebuffer, (int) attachment, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetNamedFramebufferAttachmentParameteriv: retrieve information about attachments of a framebuffer object
        /// </summary>
        /// <param name="framebuffer">
        /// Specifies the name of the framebuffer object for Gl.GetNamedFramebufferAttachmentParameteriv.
        /// </param>
        /// <param name="attachment">
        /// Specifies the attachment of the framebuffer object to query.
        /// </param>
        /// <param name="pname">
        /// Specifies the parameter of <paramref name="attachment" /> to query.
        /// </param>
        /// <param name="params">
        /// Returns the value of parameter <paramref name="pname" /> for <paramref name="attachment" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static unsafe void GetNamedFramebufferAttachmentParameter(uint framebuffer, FramebufferAttachment attachment, FramebufferAttachmentParameterName pname, [Out] int* @params)
        {
            Debug.Assert(Delegates.pglGetNamedFramebufferAttachmentParameteriv != null, "pglGetNamedFramebufferAttachmentParameteriv not implemented");
            Delegates.pglGetNamedFramebufferAttachmentParameteriv(framebuffer, (int) attachment, (int) pname, @params);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetNamedFramebufferAttachmentParameteriv: retrieve information about attachments of a framebuffer object
        /// </summary>
        /// <param name="framebuffer">
        /// Specifies the name of the framebuffer object for Gl.GetNamedFramebufferAttachmentParameteriv.
        /// </param>
        /// <param name="attachment">
        /// Specifies the attachment of the framebuffer object to query.
        /// </param>
        /// <param name="pname">
        /// Specifies the parameter of <paramref name="attachment" /> to query.
        /// </param>
        /// <param name="params">
        /// Returns the value of parameter <paramref name="pname" /> for <paramref name="attachment" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetNamedFramebufferAttachmentParameteri<T>(uint framebuffer, FramebufferAttachment attachment, FramebufferAttachmentParameterName pname, out T @params) where T : struct
        {
            Debug.Assert(Delegates.pglGetNamedFramebufferAttachmentParameteriv != null, "pglGetNamedFramebufferAttachmentParameteriv not implemented");
            @params = default;
            unsafe
            {
                TypedReference refParams = __makeref(@params);
                IntPtr refParamsPtr = *(IntPtr*) (&refParams);

                Delegates.pglGetNamedFramebufferAttachmentParameteriv(framebuffer, (int) attachment, (int) pname, (int*) refParamsPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glCreateRenderbuffers: create renderbuffer objects
        /// </summary>
        /// <param name="renderbuffers">
        /// Specifies an array in which names of the new renderbuffer objects are stored.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void CreateRenderbuffers(uint[] renderbuffers)
        {
            unsafe
            {
                fixed (uint* p_renderbuffers = renderbuffers)
                {
                    Debug.Assert(Delegates.pglCreateRenderbuffers != null, "pglCreateRenderbuffers not implemented");
                    Delegates.pglCreateRenderbuffers(renderbuffers.Length, p_renderbuffers);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glCreateRenderbuffers: create renderbuffer objects
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static uint CreateRenderbuffer()
        {
            uint retValue;
            unsafe
            {
                Delegates.pglCreateRenderbuffers(1, &retValue);
            }

            DebugCheckErrors(null);
            return retValue;
        }

        /// <summary>
        /// [GL4] glNamedRenderbufferStorage: establish data storage, format and dimensions of a renderbuffer object's image
        /// </summary>
        /// <param name="renderbuffer">
        /// Specifies the name of the renderbuffer object for Gl.NamedRenderbufferStorage function.
        /// </param>
        /// <param name="internalformat">
        /// Specifies the internal format to use for the renderbuffer object's image.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the renderbuffer, in pixels.
        /// </param>
        /// <param name="height">
        /// Specifies the height of the renderbuffer, in pixels.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void NamedRenderbufferStorage(uint renderbuffer, InternalFormat internalformat, int width, int height)
        {
            Debug.Assert(Delegates.pglNamedRenderbufferStorage != null, "pglNamedRenderbufferStorage not implemented");
            Delegates.pglNamedRenderbufferStorage(renderbuffer, (int) internalformat, width, height);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glNamedRenderbufferStorageMultisample: establish data storage, format, dimensions and sample count of a
        /// renderbuffer object's image
        /// </summary>
        /// <param name="renderbuffer">
        /// Specifies the name of the renderbuffer object for Gl.NamedRenderbufferStorageMultisample function.
        /// </param>
        /// <param name="samples">
        /// Specifies the number of samples to be used for the renderbuffer object's storage.
        /// </param>
        /// <param name="internalformat">
        /// Specifies the internal format to use for the renderbuffer object's image.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the renderbuffer, in pixels.
        /// </param>
        /// <param name="height">
        /// Specifies the height of the renderbuffer, in pixels.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void NamedRenderbufferStorageMultisample(uint renderbuffer, int samples, InternalFormat internalformat, int width, int height)
        {
            Debug.Assert(Delegates.pglNamedRenderbufferStorageMultisample != null, "pglNamedRenderbufferStorageMultisample not implemented");
            Delegates.pglNamedRenderbufferStorageMultisample(renderbuffer, samples, (int) internalformat, width, height);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetNamedRenderbufferParameteriv: query a named parameter of a renderbuffer object
        /// </summary>
        /// <param name="renderbuffer">
        /// Specifies the name of the renderbuffer object for Gl.GetNamedRenderbufferParameteriv.
        /// </param>
        /// <param name="pname">
        /// Specifies the parameter of the renderbuffer object to query.
        /// </param>
        /// <param name="params">
        /// Returns the value of parameter <paramref name="pname" /> for the renderbuffer object.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetNamedRenderbufferParameter(uint renderbuffer, RenderbufferParameterName pname, [Out] int[] @params)
        {
            unsafe
            {
                fixed (int* p_params = @params)
                {
                    Debug.Assert(Delegates.pglGetNamedRenderbufferParameteriv != null, "pglGetNamedRenderbufferParameteriv not implemented");
                    Delegates.pglGetNamedRenderbufferParameteriv(renderbuffer, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetNamedRenderbufferParameteriv: query a named parameter of a renderbuffer object
        /// </summary>
        /// <param name="renderbuffer">
        /// Specifies the name of the renderbuffer object for Gl.GetNamedRenderbufferParameteriv.
        /// </param>
        /// <param name="pname">
        /// Specifies the parameter of the renderbuffer object to query.
        /// </param>
        /// <param name="params">
        /// Returns the value of parameter <paramref name="pname" /> for the renderbuffer object.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetNamedRenderbufferParameter(uint renderbuffer, RenderbufferParameterName pname, out int @params)
        {
            unsafe
            {
                fixed (int* p_params = &@params)
                {
                    Debug.Assert(Delegates.pglGetNamedRenderbufferParameteriv != null, "pglGetNamedRenderbufferParameteriv not implemented");
                    Delegates.pglGetNamedRenderbufferParameteriv(renderbuffer, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetNamedRenderbufferParameteriv: query a named parameter of a renderbuffer object
        /// </summary>
        /// <param name="renderbuffer">
        /// Specifies the name of the renderbuffer object for Gl.GetNamedRenderbufferParameteriv.
        /// </param>
        /// <param name="pname">
        /// Specifies the parameter of the renderbuffer object to query.
        /// </param>
        /// <param name="params">
        /// Returns the value of parameter <paramref name="pname" /> for the renderbuffer object.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static unsafe void GetNamedRenderbufferParameter(uint renderbuffer, RenderbufferParameterName pname, [Out] int* @params)
        {
            Debug.Assert(Delegates.pglGetNamedRenderbufferParameteriv != null, "pglGetNamedRenderbufferParameteriv not implemented");
            Delegates.pglGetNamedRenderbufferParameteriv(renderbuffer, (int) pname, @params);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetNamedRenderbufferParameteriv: query a named parameter of a renderbuffer object
        /// </summary>
        /// <param name="renderbuffer">
        /// Specifies the name of the renderbuffer object for Gl.GetNamedRenderbufferParameteriv.
        /// </param>
        /// <param name="pname">
        /// Specifies the parameter of the renderbuffer object to query.
        /// </param>
        /// <param name="params">
        /// Returns the value of parameter <paramref name="pname" /> for the renderbuffer object.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetNamedRenderbufferParameteri<T>(uint renderbuffer, RenderbufferParameterName pname, out T @params) where T : struct
        {
            Debug.Assert(Delegates.pglGetNamedRenderbufferParameteriv != null, "pglGetNamedRenderbufferParameteriv not implemented");
            @params = default;
            unsafe
            {
                TypedReference refParams = __makeref(@params);
                IntPtr refParamsPtr = *(IntPtr*) (&refParams);

                Delegates.pglGetNamedRenderbufferParameteriv(renderbuffer, (int) pname, (int*) refParamsPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glCreateTextures: create texture objects
        /// </summary>
        /// <param name="target">
        /// Specifies the effective texture target of each created texture.
        /// </param>
        /// <param name="n">
        /// Number of texture objects to create.
        /// </param>
        /// <param name="textures">
        /// Specifies an array in which names of the new texture objects are stored.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void CreateTextures(TextureTarget target, int n, uint[] textures)
        {
            unsafe
            {
                fixed (uint* p_textures = textures)
                {
                    Debug.Assert(Delegates.pglCreateTextures != null, "pglCreateTextures not implemented");
                    Delegates.pglCreateTextures((int) target, n, p_textures);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glCreateTextures: create texture objects
        /// </summary>
        /// <param name="target">
        /// Specifies the effective texture target of each created texture.
        /// </param>
        /// <param name="textures">
        /// Specifies an array in which names of the new texture objects are stored.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void CreateTextures(TextureTarget target, uint[] textures)
        {
            unsafe
            {
                fixed (uint* p_textures = textures)
                {
                    Debug.Assert(Delegates.pglCreateTextures != null, "pglCreateTextures not implemented");
                    Delegates.pglCreateTextures((int) target, textures.Length, p_textures);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glCreateTextures: create texture objects
        /// </summary>
        /// <param name="target">
        /// Specifies the effective texture target of each created texture.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static uint CreateTexture(TextureTarget target)
        {
            uint retValue;
            unsafe
            {
                Delegates.pglCreateTextures((int) target, 1, &retValue);
            }

            DebugCheckErrors(null);
            return retValue;
        }

        /// <summary>
        /// [GL4] glTextureBuffer: attach a buffer object's data store to a buffer texture object
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.TextureBuffer.
        /// </param>
        /// <param name="internalformat">
        /// A <see cref="T:InternalFormat" />.
        /// </param>
        /// <param name="buffer">
        /// Specifies the name of the buffer object whose storage to attach to the active buffer texture.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void TextureBuffer(uint texture, InternalFormat internalformat, uint buffer)
        {
            Debug.Assert(Delegates.pglTextureBuffer != null, "pglTextureBuffer not implemented");
            Delegates.pglTextureBuffer(texture, (int) internalformat, buffer);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glTextureBufferRange: attach a range of a buffer object's data store to a buffer texture object
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.TextureBufferRange.
        /// </param>
        /// <param name="internalformat">
        /// A <see cref="T:InternalFormat" />.
        /// </param>
        /// <param name="buffer">
        /// Specifies the name of the buffer object whose storage to attach to the active buffer texture.
        /// </param>
        /// <param name="offset">
        /// Specifies the offset of the start of the range of the buffer's data store to attach.
        /// </param>
        /// <param name="size">
        /// Specifies the size of the range of the buffer's data store to attach.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void TextureBufferRange(uint texture, InternalFormat internalformat, uint buffer, IntPtr offset, uint size)
        {
            Debug.Assert(Delegates.pglTextureBufferRange != null, "pglTextureBufferRange not implemented");
            Delegates.pglTextureBufferRange(texture, (int) internalformat, buffer, offset, size);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL] glTextureStorage1D: Binding for glTextureStorage1D.
        /// </summary>
        /// <param name="texture">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="levels">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="internalformat">
        /// A <see cref="T:InternalFormat" />.
        /// </param>
        /// <param name="width">
        /// A <see cref="T:int" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void TextureStorage1D(uint texture, int levels, InternalFormat internalformat, int width)
        {
            Debug.Assert(Delegates.pglTextureStorage1D != null, "pglTextureStorage1D not implemented");
            Delegates.pglTextureStorage1D(texture, levels, (int) internalformat, width);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL] glTextureStorage2D: Binding for glTextureStorage2D.
        /// </summary>
        /// <param name="texture">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="levels">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="internalformat">
        /// A <see cref="T:InternalFormat" />.
        /// </param>
        /// <param name="width">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="height">
        /// A <see cref="T:int" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void TextureStorage2D(uint texture, int levels, InternalFormat internalformat, int width, int height)
        {
            Debug.Assert(Delegates.pglTextureStorage2D != null, "pglTextureStorage2D not implemented");
            Delegates.pglTextureStorage2D(texture, levels, (int) internalformat, width, height);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL] glTextureStorage3D: Binding for glTextureStorage3D.
        /// </summary>
        /// <param name="texture">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="levels">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="internalformat">
        /// A <see cref="T:InternalFormat" />.
        /// </param>
        /// <param name="width">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="height">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="depth">
        /// A <see cref="T:int" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void TextureStorage3D(uint texture, int levels, InternalFormat internalformat, int width, int height, int depth)
        {
            Debug.Assert(Delegates.pglTextureStorage3D != null, "pglTextureStorage3D not implemented");
            Delegates.pglTextureStorage3D(texture, levels, (int) internalformat, width, height, depth);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glTextureStorage2DMultisample: specify storage for a two-dimensional multisample texture
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.TextureStorage2DMultisample. The effective target of
        /// <paramref name="texture" />
        /// must be one of the valid non-proxy target values above.
        /// </param>
        /// <param name="samples">
        /// Specify the number of samples in the texture.
        /// </param>
        /// <param name="internalformat">
        /// Specifies the sized internal format to be used to store texture image data.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture, in texels.
        /// </param>
        /// <param name="height">
        /// Specifies the height of the texture, in texels.
        /// </param>
        /// <param name="fixedsamplelocations">
        /// Specifies whether the image will use identical sample locations and the same number of samples for all texels in the
        /// image, and the sample locations will not depend on the internal format or size of the image.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void TextureStorage2DMultisample(uint texture, int samples, InternalFormat internalformat, int width, int height, bool fixedsamplelocations)
        {
            Debug.Assert(Delegates.pglTextureStorage2DMultisample != null, "pglTextureStorage2DMultisample not implemented");
            Delegates.pglTextureStorage2DMultisample(texture, samples, (int) internalformat, width, height, fixedsamplelocations);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glTextureStorage3DMultisample: specify storage for a two-dimensional multisample array texture
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.TextureStorage3DMultisample. The effective target of
        /// <paramref name="texture" />
        /// must be one of the valid non-proxy target values above.
        /// </param>
        /// <param name="samples">
        /// Specify the number of samples in the texture.
        /// </param>
        /// <param name="internalformat">
        /// Specifies the sized internal format to be used to store texture image data.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture, in texels.
        /// </param>
        /// <param name="height">
        /// Specifies the height of the texture, in texels.
        /// </param>
        /// <param name="depth">
        /// Specifies the depth of the texture, in layers.
        /// </param>
        /// <param name="fixedsamplelocations">
        /// Specifies whether the image will use identical sample locations and the same number of samples for all texels in the
        /// image, and the sample locations will not depend on the internal format or size of the image.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void TextureStorage3DMultisample(uint texture, int samples, InternalFormat internalformat, int width, int height, int depth, bool fixedsamplelocations)
        {
            Debug.Assert(Delegates.pglTextureStorage3DMultisample != null, "pglTextureStorage3DMultisample not implemented");
            Delegates.pglTextureStorage3DMultisample(texture, samples, (int) internalformat, width, height, depth, fixedsamplelocations);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glTextureSubImage1D: specify a one-dimensional texture subimage
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.TextureSubImage1D. The effective target of <paramref name="texture" /> must be
        /// one of the valid target values above.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number. Level 0 is the base image level. Level n is the nth mipmap reduction image.
        /// </param>
        /// <param name="xoffset">
        /// Specifies a texel offset in the x direction within the texture array.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture subimage.
        /// </param>
        /// <param name="format">
        /// Specifies the format of the pixel data. The following symbolic values are accepted: Gl.RED, Gl.RG, Gl.RGB, Gl.BGR,
        /// Gl.RGBA, Gl.DEPTH_COMPONENT, and Gl.STENCIL_INDEX.
        /// </param>
        /// <param name="type">
        /// Specifies the data type of the pixel data. The following symbolic values are accepted: Gl.UNSIGNED_BYTE, Gl.BYTE,
        /// Gl.UNSIGNED_SHORT, Gl.SHORT, Gl.UNSIGNED_INT, Gl.INT, Gl.FLOAT, Gl.UNSIGNED_BYTE_3_3_2, Gl.UNSIGNED_BYTE_2_3_3_REV,
        /// Gl.UNSIGNED_SHORT_5_6_5, Gl.UNSIGNED_SHORT_5_6_5_REV, Gl.UNSIGNED_SHORT_4_4_4_4, Gl.UNSIGNED_SHORT_4_4_4_4_REV,
        /// Gl.UNSIGNED_SHORT_5_5_5_1, Gl.UNSIGNED_SHORT_1_5_5_5_REV, Gl.UNSIGNED_INT_8_8_8_8, Gl.UNSIGNED_INT_8_8_8_8_REV,
        /// Gl.UNSIGNED_INT_10_10_10_2, and Gl.UNSIGNED_INT_2_10_10_10_REV.
        /// </param>
        /// <param name="pixels">
        /// Specifies a pointer to the image data in memory.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void TextureSubImage1D(uint texture, int level, int xoffset, int width, PixelFormat format, PixelType type, IntPtr pixels)
        {
            Debug.Assert(Delegates.pglTextureSubImage1D != null, "pglTextureSubImage1D not implemented");
            Delegates.pglTextureSubImage1D(texture, level, xoffset, width, (int) format, (int) type, pixels);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glTextureSubImage1D: specify a one-dimensional texture subimage
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.TextureSubImage1D. The effective target of <paramref name="texture" /> must be
        /// one of the valid target values above.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number. Level 0 is the base image level. Level n is the nth mipmap reduction image.
        /// </param>
        /// <param name="xoffset">
        /// Specifies a texel offset in the x direction within the texture array.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture subimage.
        /// </param>
        /// <param name="format">
        /// Specifies the format of the pixel data. The following symbolic values are accepted: Gl.RED, Gl.RG, Gl.RGB, Gl.BGR,
        /// Gl.RGBA, Gl.DEPTH_COMPONENT, and Gl.STENCIL_INDEX.
        /// </param>
        /// <param name="type">
        /// Specifies the data type of the pixel data. The following symbolic values are accepted: Gl.UNSIGNED_BYTE, Gl.BYTE,
        /// Gl.UNSIGNED_SHORT, Gl.SHORT, Gl.UNSIGNED_INT, Gl.INT, Gl.FLOAT, Gl.UNSIGNED_BYTE_3_3_2, Gl.UNSIGNED_BYTE_2_3_3_REV,
        /// Gl.UNSIGNED_SHORT_5_6_5, Gl.UNSIGNED_SHORT_5_6_5_REV, Gl.UNSIGNED_SHORT_4_4_4_4, Gl.UNSIGNED_SHORT_4_4_4_4_REV,
        /// Gl.UNSIGNED_SHORT_5_5_5_1, Gl.UNSIGNED_SHORT_1_5_5_5_REV, Gl.UNSIGNED_INT_8_8_8_8, Gl.UNSIGNED_INT_8_8_8_8_REV,
        /// Gl.UNSIGNED_INT_10_10_10_2, and Gl.UNSIGNED_INT_2_10_10_10_REV.
        /// </param>
        /// <param name="pixels">
        /// Specifies a pointer to the image data in memory.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void TextureSubImage1D(uint texture, int level, int xoffset, int width, PixelFormat format, PixelType type, object pixels)
        {
            GCHandle pin_pixels = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            try
            {
                TextureSubImage1D(texture, level, xoffset, width, format, type, pin_pixels.AddrOfPinnedObject());
            }
            finally
            {
                pin_pixels.Free();
            }
        }

        /// <summary>
        /// [GL4] glTextureSubImage2D: specify a two-dimensional texture subimage
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.TextureSubImage2D. The effective target of <paramref name="texture" /> must be
        /// one of the valid target values above.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number. Level 0 is the base image level. Level n is the nth mipmap reduction image.
        /// </param>
        /// <param name="xoffset">
        /// Specifies a texel offset in the x direction within the texture array.
        /// </param>
        /// <param name="yoffset">
        /// Specifies a texel offset in the y direction within the texture array.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture subimage.
        /// </param>
        /// <param name="height">
        /// Specifies the height of the texture subimage.
        /// </param>
        /// <param name="format">
        /// Specifies the format of the pixel data. The following symbolic values are accepted: Gl.RED, Gl.RG, Gl.RGB, Gl.BGR,
        /// Gl.RGBA, Gl.BGRA, Gl.DEPTH_COMPONENT, and Gl.STENCIL_INDEX.
        /// </param>
        /// <param name="type">
        /// Specifies the data type of the pixel data. The following symbolic values are accepted: Gl.UNSIGNED_BYTE, Gl.BYTE,
        /// Gl.UNSIGNED_SHORT, Gl.SHORT, Gl.UNSIGNED_INT, Gl.INT, Gl.FLOAT, Gl.UNSIGNED_BYTE_3_3_2, Gl.UNSIGNED_BYTE_2_3_3_REV,
        /// Gl.UNSIGNED_SHORT_5_6_5, Gl.UNSIGNED_SHORT_5_6_5_REV, Gl.UNSIGNED_SHORT_4_4_4_4, Gl.UNSIGNED_SHORT_4_4_4_4_REV,
        /// Gl.UNSIGNED_SHORT_5_5_5_1, Gl.UNSIGNED_SHORT_1_5_5_5_REV, Gl.UNSIGNED_INT_8_8_8_8, Gl.UNSIGNED_INT_8_8_8_8_REV,
        /// Gl.UNSIGNED_INT_10_10_10_2, and Gl.UNSIGNED_INT_2_10_10_10_REV.
        /// </param>
        /// <param name="pixels">
        /// Specifies a pointer to the image data in memory.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void TextureSubImage2D(uint texture, int level, int xoffset, int yoffset, int width, int height, PixelFormat format, PixelType type, IntPtr pixels)
        {
            Debug.Assert(Delegates.pglTextureSubImage2D != null, "pglTextureSubImage2D not implemented");
            Delegates.pglTextureSubImage2D(texture, level, xoffset, yoffset, width, height, (int) format, (int) type, pixels);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glTextureSubImage2D: specify a two-dimensional texture subimage
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.TextureSubImage2D. The effective target of <paramref name="texture" /> must be
        /// one of the valid target values above.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number. Level 0 is the base image level. Level n is the nth mipmap reduction image.
        /// </param>
        /// <param name="xoffset">
        /// Specifies a texel offset in the x direction within the texture array.
        /// </param>
        /// <param name="yoffset">
        /// Specifies a texel offset in the y direction within the texture array.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture subimage.
        /// </param>
        /// <param name="height">
        /// Specifies the height of the texture subimage.
        /// </param>
        /// <param name="format">
        /// Specifies the format of the pixel data. The following symbolic values are accepted: Gl.RED, Gl.RG, Gl.RGB, Gl.BGR,
        /// Gl.RGBA, Gl.BGRA, Gl.DEPTH_COMPONENT, and Gl.STENCIL_INDEX.
        /// </param>
        /// <param name="type">
        /// Specifies the data type of the pixel data. The following symbolic values are accepted: Gl.UNSIGNED_BYTE, Gl.BYTE,
        /// Gl.UNSIGNED_SHORT, Gl.SHORT, Gl.UNSIGNED_INT, Gl.INT, Gl.FLOAT, Gl.UNSIGNED_BYTE_3_3_2, Gl.UNSIGNED_BYTE_2_3_3_REV,
        /// Gl.UNSIGNED_SHORT_5_6_5, Gl.UNSIGNED_SHORT_5_6_5_REV, Gl.UNSIGNED_SHORT_4_4_4_4, Gl.UNSIGNED_SHORT_4_4_4_4_REV,
        /// Gl.UNSIGNED_SHORT_5_5_5_1, Gl.UNSIGNED_SHORT_1_5_5_5_REV, Gl.UNSIGNED_INT_8_8_8_8, Gl.UNSIGNED_INT_8_8_8_8_REV,
        /// Gl.UNSIGNED_INT_10_10_10_2, and Gl.UNSIGNED_INT_2_10_10_10_REV.
        /// </param>
        /// <param name="pixels">
        /// Specifies a pointer to the image data in memory.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void TextureSubImage2D(uint texture, int level, int xoffset, int yoffset, int width, int height, PixelFormat format, PixelType type, object pixels)
        {
            GCHandle pin_pixels = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            try
            {
                TextureSubImage2D(texture, level, xoffset, yoffset, width, height, format, type, pin_pixels.AddrOfPinnedObject());
            }
            finally
            {
                pin_pixels.Free();
            }
        }

        /// <summary>
        /// [GL4] glTextureSubImage3D: specify a three-dimensional texture subimage
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.TextureSubImage3D. The effective target of <paramref name="texture" /> must be
        /// one of the valid target values above.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number. Level 0 is the base image level. Level n is the nth mipmap reduction image.
        /// </param>
        /// <param name="xoffset">
        /// Specifies a texel offset in the x direction within the texture array.
        /// </param>
        /// <param name="yoffset">
        /// Specifies a texel offset in the y direction within the texture array.
        /// </param>
        /// <param name="zoffset">
        /// Specifies a texel offset in the z direction within the texture array.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture subimage.
        /// </param>
        /// <param name="height">
        /// Specifies the height of the texture subimage.
        /// </param>
        /// <param name="depth">
        /// Specifies the depth of the texture subimage.
        /// </param>
        /// <param name="format">
        /// Specifies the format of the pixel data. The following symbolic values are accepted: Gl.RED, Gl.RG, Gl.RGB, Gl.BGR,
        /// Gl.RGBA, Gl.DEPTH_COMPONENT, and Gl.STENCIL_INDEX.
        /// </param>
        /// <param name="type">
        /// Specifies the data type of the pixel data. The following symbolic values are accepted: Gl.UNSIGNED_BYTE, Gl.BYTE,
        /// Gl.UNSIGNED_SHORT, Gl.SHORT, Gl.UNSIGNED_INT, Gl.INT, Gl.FLOAT, Gl.UNSIGNED_BYTE_3_3_2, Gl.UNSIGNED_BYTE_2_3_3_REV,
        /// Gl.UNSIGNED_SHORT_5_6_5, Gl.UNSIGNED_SHORT_5_6_5_REV, Gl.UNSIGNED_SHORT_4_4_4_4, Gl.UNSIGNED_SHORT_4_4_4_4_REV,
        /// Gl.UNSIGNED_SHORT_5_5_5_1, Gl.UNSIGNED_SHORT_1_5_5_5_REV, Gl.UNSIGNED_INT_8_8_8_8, Gl.UNSIGNED_INT_8_8_8_8_REV,
        /// Gl.UNSIGNED_INT_10_10_10_2, and Gl.UNSIGNED_INT_2_10_10_10_REV.
        /// </param>
        /// <param name="pixels">
        /// Specifies a pointer to the image data in memory.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void TextureSubImage3D(uint texture, int level, int xoffset, int yoffset, int zoffset, int width, int height, int depth, PixelFormat format, PixelType type, IntPtr pixels)
        {
            Debug.Assert(Delegates.pglTextureSubImage3D != null, "pglTextureSubImage3D not implemented");
            Delegates.pglTextureSubImage3D(texture, level, xoffset, yoffset, zoffset, width, height, depth, (int) format, (int) type, pixels);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glTextureSubImage3D: specify a three-dimensional texture subimage
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.TextureSubImage3D. The effective target of <paramref name="texture" /> must be
        /// one of the valid target values.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number. Level 0 is the base image level. Level n is the nth mipmap reduction image.
        /// </param>
        /// <param name="xoffset">
        /// Specifies a texel offset in the x direction within the texture array.
        /// </param>
        /// <param name="yoffset">
        /// Specifies a texel offset in the y direction within the texture array.
        /// </param>
        /// <param name="zoffset">
        /// Specifies a texel offset in the z direction within the texture array.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture subimage.
        /// </param>
        /// <param name="height">
        /// Specifies the height of the texture subimage.
        /// </param>
        /// <param name="depth">
        /// Specifies the depth of the texture subimage.
        /// </param>
        /// <param name="format">
        /// Specifies the format of the pixel data. The following symbolic values are accepted: Gl.RED, Gl.RG, Gl.RGB, Gl.BGR,
        /// Gl.RGBA, Gl.DEPTH_COMPONENT, and Gl.STENCIL_INDEX.
        /// </param>
        /// <param name="type">
        /// Specifies the data type of the pixel data. The following symbolic values are accepted: Gl.UNSIGNED_BYTE, Gl.BYTE,
        /// Gl.UNSIGNED_SHORT, Gl.SHORT, Gl.UNSIGNED_INT, Gl.INT, Gl.FLOAT, Gl.UNSIGNED_BYTE_3_3_2, Gl.UNSIGNED_BYTE_2_3_3_REV,
        /// Gl.UNSIGNED_SHORT_5_6_5, Gl.UNSIGNED_SHORT_5_6_5_REV, Gl.UNSIGNED_SHORT_4_4_4_4, Gl.UNSIGNED_SHORT_4_4_4_4_REV,
        /// Gl.UNSIGNED_SHORT_5_5_5_1, Gl.UNSIGNED_SHORT_1_5_5_5_REV, Gl.UNSIGNED_INT_8_8_8_8, Gl.UNSIGNED_INT_8_8_8_8_REV,
        /// Gl.UNSIGNED_INT_10_10_10_2, and Gl.UNSIGNED_INT_2_10_10_10_REV.
        /// </param>
        /// <param name="pixels">
        /// Specifies a pointer to the image data in memory.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void TextureSubImage3D(uint texture, int level, int xoffset, int yoffset, int zoffset, int width, int height, int depth, PixelFormat format, PixelType type, object pixels)
        {
            GCHandle pin_pixels = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            try
            {
                TextureSubImage3D(texture, level, xoffset, yoffset, zoffset, width, height, depth, format, type, pin_pixels.AddrOfPinnedObject());
            }
            finally
            {
                pin_pixels.Free();
            }
        }

        /// <summary>
        /// [GL4] glCompressedTextureSubImage1D: specify a one-dimensional texture subimage in a compressed format
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.CompressedTextureSubImage1D function.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number. Level 0 is the base image level. Level n is the nth mipmap reduction image.
        /// </param>
        /// <param name="xoffset">
        /// Specifies a texel offset in the x direction within the texture array.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture subimage.
        /// </param>
        /// <param name="format">
        /// Specifies the format of the compressed image data stored at address <paramref name="data" />.
        /// </param>
        /// <param name="imageSize">
        /// Specifies the number of unsigned bytes of image data starting at the address specified by <paramref name="data" />.
        /// </param>
        /// <param name="data">
        /// Specifies a pointer to the compressed image data in memory.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void CompressedTextureSubImage1D(uint texture, int level, int xoffset, int width, PixelFormat format, int imageSize, IntPtr data)
        {
            Debug.Assert(Delegates.pglCompressedTextureSubImage1D != null, "pglCompressedTextureSubImage1D not implemented");
            Delegates.pglCompressedTextureSubImage1D(texture, level, xoffset, width, (int) format, imageSize, data);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glCompressedTextureSubImage1D: specify a one-dimensional texture subimage in a compressed format
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.CompressedTextureSubImage1D function.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number. Level 0 is the base image level. Level n is the nth mipmap reduction image.
        /// </param>
        /// <param name="xoffset">
        /// Specifies a texel offset in the x direction within the texture array.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture subimage.
        /// </param>
        /// <param name="format">
        /// Specifies the format of the compressed image data stored at address <paramref name="data" />.
        /// </param>
        /// <param name="imageSize">
        /// Specifies the number of unsigned bytes of image data starting at the address specified by <paramref name="data" />.
        /// </param>
        /// <param name="data">
        /// Specifies a pointer to the compressed image data in memory.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void CompressedTextureSubImage1D(uint texture, int level, int xoffset, int width, PixelFormat format, int imageSize, object data)
        {
            GCHandle pin_data = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                CompressedTextureSubImage1D(texture, level, xoffset, width, format, imageSize, pin_data.AddrOfPinnedObject());
            }
            finally
            {
                pin_data.Free();
            }
        }

        /// <summary>
        /// [GL4] glCompressedTextureSubImage2D: specify a two-dimensional texture subimage in a compressed format
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.CompressedTextureSubImage2D function.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number. Level 0 is the base image level. Level n is the nth mipmap reduction image.
        /// </param>
        /// <param name="xoffset">
        /// Specifies a texel offset in the x direction within the texture array.
        /// </param>
        /// <param name="yoffset">
        /// Specifies a texel offset in the y direction within the texture array.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture subimage.
        /// </param>
        /// <param name="height">
        /// Specifies the height of the texture subimage.
        /// </param>
        /// <param name="format">
        /// Specifies the format of the compressed image data stored at address <paramref name="data" />.
        /// </param>
        /// <param name="imageSize">
        /// Specifies the number of unsigned bytes of image data starting at the address specified by <paramref name="data" />.
        /// </param>
        /// <param name="data">
        /// Specifies a pointer to the compressed image data in memory.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void CompressedTextureSubImage2D(uint texture, int level, int xoffset, int yoffset, int width, int height, PixelFormat format, int imageSize, IntPtr data)
        {
            Debug.Assert(Delegates.pglCompressedTextureSubImage2D != null, "pglCompressedTextureSubImage2D not implemented");
            Delegates.pglCompressedTextureSubImage2D(texture, level, xoffset, yoffset, width, height, (int) format, imageSize, data);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glCompressedTextureSubImage2D: specify a two-dimensional texture subimage in a compressed format
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.CompressedTextureSubImage2D function.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number. Level 0 is the base image level. Level n is the nth mipmap reduction image.
        /// </param>
        /// <param name="xoffset">
        /// Specifies a texel offset in the x direction within the texture array.
        /// </param>
        /// <param name="yoffset">
        /// Specifies a texel offset in the y direction within the texture array.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture subimage.
        /// </param>
        /// <param name="height">
        /// Specifies the height of the texture subimage.
        /// </param>
        /// <param name="format">
        /// Specifies the format of the compressed image data stored at address <paramref name="data" />.
        /// </param>
        /// <param name="imageSize">
        /// Specifies the number of unsigned bytes of image data starting at the address specified by <paramref name="data" />.
        /// </param>
        /// <param name="data">
        /// Specifies a pointer to the compressed image data in memory.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void CompressedTextureSubImage2D(uint texture, int level, int xoffset, int yoffset, int width, int height, PixelFormat format, int imageSize, object data)
        {
            GCHandle pin_data = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                CompressedTextureSubImage2D(texture, level, xoffset, yoffset, width, height, format, imageSize, pin_data.AddrOfPinnedObject());
            }
            finally
            {
                pin_data.Free();
            }
        }

        /// <summary>
        /// [GL4] glCompressedTextureSubImage3D: specify a three-dimensional texture subimage in a compressed format
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.CompressedTextureSubImage3D function.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number. Level 0 is the base image level. Level n is the nth mipmap reduction image.
        /// </param>
        /// <param name="xoffset">
        /// Specifies a texel offset in the x direction within the texture array.
        /// </param>
        /// <param name="yoffset">
        /// Specifies a texel offset in the y direction within the texture array.
        /// </param>
        /// <param name="zoffset">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture subimage.
        /// </param>
        /// <param name="height">
        /// Specifies the height of the texture subimage.
        /// </param>
        /// <param name="depth">
        /// Specifies the depth of the texture subimage.
        /// </param>
        /// <param name="format">
        /// Specifies the format of the compressed image data stored at address <paramref name="data" />.
        /// </param>
        /// <param name="imageSize">
        /// Specifies the number of unsigned bytes of image data starting at the address specified by <paramref name="data" />.
        /// </param>
        /// <param name="data">
        /// Specifies a pointer to the compressed image data in memory.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void CompressedTextureSubImage3D(uint texture, int level, int xoffset, int yoffset, int zoffset, int width, int height, int depth, PixelFormat format, int imageSize, IntPtr data)
        {
            Debug.Assert(Delegates.pglCompressedTextureSubImage3D != null, "pglCompressedTextureSubImage3D not implemented");
            Delegates.pglCompressedTextureSubImage3D(texture, level, xoffset, yoffset, zoffset, width, height, depth, (int) format, imageSize, data);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glCompressedTextureSubImage3D: specify a three-dimensional texture subimage in a compressed format
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.CompressedTextureSubImage3D function.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number. Level 0 is the base image level. Level n is the nth mipmap reduction image.
        /// </param>
        /// <param name="xoffset">
        /// Specifies a texel offset in the x direction within the texture array.
        /// </param>
        /// <param name="yoffset">
        /// Specifies a texel offset in the y direction within the texture array.
        /// </param>
        /// <param name="zoffset">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture subimage.
        /// </param>
        /// <param name="height">
        /// Specifies the height of the texture subimage.
        /// </param>
        /// <param name="depth">
        /// Specifies the depth of the texture subimage.
        /// </param>
        /// <param name="format">
        /// Specifies the format of the compressed image data stored at address <paramref name="data" />.
        /// </param>
        /// <param name="imageSize">
        /// Specifies the number of unsigned bytes of image data starting at the address specified by <paramref name="data" />.
        /// </param>
        /// <param name="data">
        /// Specifies a pointer to the compressed image data in memory.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void CompressedTextureSubImage3D(uint texture, int level, int xoffset, int yoffset, int zoffset, int width, int height, int depth, PixelFormat format, int imageSize, object data)
        {
            GCHandle pin_data = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                CompressedTextureSubImage3D(texture, level, xoffset, yoffset, zoffset, width, height, depth, format, imageSize, pin_data.AddrOfPinnedObject());
            }
            finally
            {
                pin_data.Free();
            }
        }

        /// <summary>
        /// [GL] glCopyTextureSubImage1D: Binding for glCopyTextureSubImage1D.
        /// </summary>
        /// <param name="texture">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="level">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="xoffset">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="x">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="y">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="width">
        /// A <see cref="T:int" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void CopyTextureSubImage1D(uint texture, int level, int xoffset, int x, int y, int width)
        {
            Debug.Assert(Delegates.pglCopyTextureSubImage1D != null, "pglCopyTextureSubImage1D not implemented");
            Delegates.pglCopyTextureSubImage1D(texture, level, xoffset, x, y, width);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL] glCopyTextureSubImage2D: Binding for glCopyTextureSubImage2D.
        /// </summary>
        /// <param name="texture">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="level">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="xoffset">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="yoffset">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="x">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="y">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="width">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="height">
        /// A <see cref="T:int" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void CopyTextureSubImage2D(uint texture, int level, int xoffset, int yoffset, int x, int y, int width, int height)
        {
            Debug.Assert(Delegates.pglCopyTextureSubImage2D != null, "pglCopyTextureSubImage2D not implemented");
            Delegates.pglCopyTextureSubImage2D(texture, level, xoffset, yoffset, x, y, width, height);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL] glCopyTextureSubImage3D: Binding for glCopyTextureSubImage3D.
        /// </summary>
        /// <param name="texture">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="level">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="xoffset">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="yoffset">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="zoffset">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="x">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="y">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="width">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="height">
        /// A <see cref="T:int" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void CopyTextureSubImage3D(uint texture, int level, int xoffset, int yoffset, int zoffset, int x, int y, int width, int height)
        {
            Debug.Assert(Delegates.pglCopyTextureSubImage3D != null, "pglCopyTextureSubImage3D not implemented");
            Delegates.pglCopyTextureSubImage3D(texture, level, xoffset, yoffset, zoffset, x, y, width, height);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL] glTextureParameterf: Binding for glTextureParameterf.
        /// </summary>
        /// <param name="texture">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="pname">
        /// A <see cref="T:TextureParameterName" />.
        /// </param>
        /// <param name="param">
        /// A <see cref="T:float" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void TextureParameter(uint texture, TextureParameterName pname, float param)
        {
            Debug.Assert(Delegates.pglTextureParameterf != null, "pglTextureParameterf not implemented");
            Delegates.pglTextureParameterf(texture, (int) pname, param);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL] glTextureParameterfv: Binding for glTextureParameterfv.
        /// </summary>
        /// <param name="texture">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="pname">
        /// A <see cref="T:TextureParameterName" />.
        /// </param>
        /// <param name="param">
        /// A <see cref="T:float[]" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void TextureParameter(uint texture, TextureParameterName pname, float[] param)
        {
            unsafe
            {
                fixed (float* p_param = param)
                {
                    Debug.Assert(Delegates.pglTextureParameterfv != null, "pglTextureParameterfv not implemented");
                    Delegates.pglTextureParameterfv(texture, (int) pname, p_param);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL] glTextureParameterfv: Binding for glTextureParameterfv.
        /// </summary>
        /// <param name="texture">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="pname">
        /// A <see cref="T:TextureParameterName" />.
        /// </param>
        /// <param name="param">
        /// A <see cref="T:float*" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static unsafe void TextureParameter(uint texture, TextureParameterName pname, float* param)
        {
            Debug.Assert(Delegates.pglTextureParameterfv != null, "pglTextureParameterfv not implemented");
            Delegates.pglTextureParameterfv(texture, (int) pname, param);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL] glTextureParameterfv: Binding for glTextureParameterfv.
        /// </summary>
        /// <param name="texture">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="pname">
        /// A <see cref="T:TextureParameterName" />.
        /// </param>
        /// <param name="param">
        /// A <see cref="T:T" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void TextureParameterf<T>(uint texture, TextureParameterName pname, T param) where T : struct
        {
            Debug.Assert(Delegates.pglTextureParameterfv != null, "pglTextureParameterfv not implemented");
            unsafe
            {
                TypedReference refParam = __makeref(param);
                IntPtr refParamPtr = *(IntPtr*) (&refParam);

                Delegates.pglTextureParameterfv(texture, (int) pname, (float*) refParamPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL] glTextureParameteri: Binding for glTextureParameteri.
        /// </summary>
        /// <param name="texture">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="pname">
        /// A <see cref="T:TextureParameterName" />.
        /// </param>
        /// <param name="param">
        /// A <see cref="T:int" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void TextureParameter(uint texture, TextureParameterName pname, int param)
        {
            Debug.Assert(Delegates.pglTextureParameteri != null, "pglTextureParameteri not implemented");
            Delegates.pglTextureParameteri(texture, (int) pname, param);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL] glTextureParameterIiv: Binding for glTextureParameterIiv.
        /// </summary>
        /// <param name="texture">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="pname">
        /// A <see cref="T:TextureParameterName" />.
        /// </param>
        /// <param name="params">
        /// A <see cref="T:int[]" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void TextureParameterI(uint texture, TextureParameterName pname, int[] @params)
        {
            unsafe
            {
                fixed (int* p_params = @params)
                {
                    Debug.Assert(Delegates.pglTextureParameterIiv != null, "pglTextureParameterIiv not implemented");
                    Delegates.pglTextureParameterIiv(texture, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL] glTextureParameterIiv: Binding for glTextureParameterIiv.
        /// </summary>
        /// <param name="texture">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="pname">
        /// A <see cref="T:TextureParameterName" />.
        /// </param>
        /// <param name="params">
        /// A <see cref="T:int*" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static unsafe void TextureParameterI(uint texture, TextureParameterName pname, int* @params)
        {
            Debug.Assert(Delegates.pglTextureParameterIiv != null, "pglTextureParameterIiv not implemented");
            Delegates.pglTextureParameterIiv(texture, (int) pname, @params);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL] glTextureParameterIiv: Binding for glTextureParameterIiv.
        /// </summary>
        /// <param name="texture">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="pname">
        /// A <see cref="T:TextureParameterName" />.
        /// </param>
        /// <param name="params">
        /// A <see cref="T:T" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void TextureParameterIi<T>(uint texture, TextureParameterName pname, T @params) where T : struct
        {
            Debug.Assert(Delegates.pglTextureParameterIiv != null, "pglTextureParameterIiv not implemented");
            unsafe
            {
                TypedReference refParams = __makeref(@params);
                IntPtr refParamsPtr = *(IntPtr*) (&refParams);

                Delegates.pglTextureParameterIiv(texture, (int) pname, (int*) refParamsPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL] glTextureParameterIuiv: Binding for glTextureParameterIuiv.
        /// </summary>
        /// <param name="texture">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="pname">
        /// A <see cref="T:TextureParameterName" />.
        /// </param>
        /// <param name="params">
        /// A <see cref="T:uint[]" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void TextureParameterI(uint texture, TextureParameterName pname, uint[] @params)
        {
            unsafe
            {
                fixed (uint* p_params = @params)
                {
                    Debug.Assert(Delegates.pglTextureParameterIuiv != null, "pglTextureParameterIuiv not implemented");
                    Delegates.pglTextureParameterIuiv(texture, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL] glTextureParameterIuiv: Binding for glTextureParameterIuiv.
        /// </summary>
        /// <param name="texture">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="pname">
        /// A <see cref="T:TextureParameterName" />.
        /// </param>
        /// <param name="params">
        /// A <see cref="T:uint*" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static unsafe void TextureParameterI(uint texture, TextureParameterName pname, uint* @params)
        {
            Debug.Assert(Delegates.pglTextureParameterIuiv != null, "pglTextureParameterIuiv not implemented");
            Delegates.pglTextureParameterIuiv(texture, (int) pname, @params);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL] glTextureParameterIuiv: Binding for glTextureParameterIuiv.
        /// </summary>
        /// <param name="texture">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="pname">
        /// A <see cref="T:TextureParameterName" />.
        /// </param>
        /// <param name="params">
        /// A <see cref="T:T" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void TextureParameterIui<T>(uint texture, TextureParameterName pname, T @params) where T : struct
        {
            Debug.Assert(Delegates.pglTextureParameterIuiv != null, "pglTextureParameterIuiv not implemented");
            unsafe
            {
                TypedReference refParams = __makeref(@params);
                IntPtr refParamsPtr = *(IntPtr*) (&refParams);

                Delegates.pglTextureParameterIuiv(texture, (int) pname, (uint*) refParamsPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL] glTextureParameteriv: Binding for glTextureParameteriv.
        /// </summary>
        /// <param name="texture">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="pname">
        /// A <see cref="T:TextureParameterName" />.
        /// </param>
        /// <param name="param">
        /// A <see cref="T:int[]" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void TextureParameter(uint texture, TextureParameterName pname, int[] param)
        {
            unsafe
            {
                fixed (int* p_param = param)
                {
                    Debug.Assert(Delegates.pglTextureParameteriv != null, "pglTextureParameteriv not implemented");
                    Delegates.pglTextureParameteriv(texture, (int) pname, p_param);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL] glTextureParameteriv: Binding for glTextureParameteriv.
        /// </summary>
        /// <param name="texture">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="pname">
        /// A <see cref="T:TextureParameterName" />.
        /// </param>
        /// <param name="param">
        /// A <see cref="T:int*" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static unsafe void TextureParameter(uint texture, TextureParameterName pname, int* param)
        {
            Debug.Assert(Delegates.pglTextureParameteriv != null, "pglTextureParameteriv not implemented");
            Delegates.pglTextureParameteriv(texture, (int) pname, param);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL] glTextureParameteriv: Binding for glTextureParameteriv.
        /// </summary>
        /// <param name="texture">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="pname">
        /// A <see cref="T:TextureParameterName" />.
        /// </param>
        /// <param name="param">
        /// A <see cref="T:T" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void TextureParameteri<T>(uint texture, TextureParameterName pname, T param) where T : struct
        {
            Debug.Assert(Delegates.pglTextureParameteriv != null, "pglTextureParameteriv not implemented");
            unsafe
            {
                TypedReference refParam = __makeref(param);
                IntPtr refParamPtr = *(IntPtr*) (&refParam);

                Delegates.pglTextureParameteriv(texture, (int) pname, (int*) refParamPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGenerateTextureMipmap: generate mipmaps for a specified texture object
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.GenerateTextureMipmap.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GenerateTextureMipmap(uint texture)
        {
            Debug.Assert(Delegates.pglGenerateTextureMipmap != null, "pglGenerateTextureMipmap not implemented");
            Delegates.pglGenerateTextureMipmap(texture);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glBindTextureUnit: bind an existing texture object to the specified texture unit
        /// </summary>
        /// <param name="unit">
        /// Specifies the texture unit, to which the texture object should be bound to.
        /// </param>
        /// <param name="texture">
        /// Specifies the name of a texture.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void BindTextureUnit(uint unit, uint texture)
        {
            Debug.Assert(Delegates.pglBindTextureUnit != null, "pglBindTextureUnit not implemented");
            Delegates.pglBindTextureUnit(unit, texture);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL] glGetTextureImage: Binding for glGetTextureImage.
        /// </summary>
        /// <param name="texture">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="level">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="format">
        /// A <see cref="T:PixelFormat" />.
        /// </param>
        /// <param name="type">
        /// A <see cref="T:PixelType" />.
        /// </param>
        /// <param name="bufSize">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="pixels">
        /// A <see cref="T:IntPtr" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetTextureImage(uint texture, int level, PixelFormat format, PixelType type, int bufSize, IntPtr pixels)
        {
            Debug.Assert(Delegates.pglGetTextureImage != null, "pglGetTextureImage not implemented");
            Delegates.pglGetTextureImage(texture, level, (int) format, (int) type, bufSize, pixels);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL] glGetTextureImage: Binding for glGetTextureImage.
        /// </summary>
        /// <param name="texture">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="level">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="format">
        /// A <see cref="T:PixelFormat" />.
        /// </param>
        /// <param name="type">
        /// A <see cref="T:PixelType" />.
        /// </param>
        /// <param name="bufSize">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="pixels">
        /// A <see cref="T:object" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetTextureImage(uint texture, int level, PixelFormat format, PixelType type, int bufSize, object pixels)
        {
            GCHandle pin_pixels = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            try
            {
                GetTextureImage(texture, level, format, type, bufSize, pin_pixels.AddrOfPinnedObject());
            }
            finally
            {
                pin_pixels.Free();
            }
        }

        /// <summary>
        /// [GL4] glGetCompressedTextureImage: return a compressed texture image
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.GetCompressedTextureImage function.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number of the desired image. Level 0 is the base image level. Level $n$ is the $n$-th
        /// mipmap reduction image.
        /// </param>
        /// <param name="bufSize">
        /// Specifies the size of the buffer <paramref name="pixels" /> for Gl.GetCompressedTextureImage and
        /// Gl.GetnCompressedTexImage functions.
        /// </param>
        /// <param name="pixels">
        /// Returns the compressed texture image.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetCompressedTextureImage(uint texture, int level, int bufSize, IntPtr pixels)
        {
            Debug.Assert(Delegates.pglGetCompressedTextureImage != null, "pglGetCompressedTextureImage not implemented");
            Delegates.pglGetCompressedTextureImage(texture, level, bufSize, pixels);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetCompressedTextureImage: return a compressed texture image
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.GetCompressedTextureImage function.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number of the desired image. Level 0 is the base image level. Level $n$ is the $n$-th
        /// mipmap reduction image.
        /// </param>
        /// <param name="bufSize">
        /// Specifies the size of the buffer <paramref name="pixels" /> for Gl.GetCompressedTextureImage and
        /// Gl.GetnCompressedTexImage functions.
        /// </param>
        /// <param name="pixels">
        /// Returns the compressed texture image.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetCompressedTextureImage(uint texture, int level, int bufSize, object pixels)
        {
            GCHandle pin_pixels = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            try
            {
                GetCompressedTextureImage(texture, level, bufSize, pin_pixels.AddrOfPinnedObject());
            }
            finally
            {
                pin_pixels.Free();
            }
        }

        /// <summary>
        /// [GL4] glGetTextureLevelParameterfv: return texture parameter values for a specific level of detail
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.GetTextureLevelParameterfv and Gl.GetTextureLevelParameteriv functions.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number of the desired image. Level 0 is the base image level. Level n is the nth mipmap
        /// reduction image.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.TEXTURE_WIDTH, Gl.TEXTURE_HEIGHT, Gl.TEXTURE_DEPTH,
        /// Gl.TEXTURE_INTERNAL_FORMAT, Gl.TEXTURE_RED_SIZE, Gl.TEXTURE_GREEN_SIZE, Gl.TEXTURE_BLUE_SIZE, Gl.TEXTURE_ALPHA_SIZE,
        /// Gl.TEXTURE_DEPTH_SIZE, Gl.TEXTURE_COMPRESSED, Gl.TEXTURE_COMPRESSED_IMAGE_SIZE, and Gl.TEXTURE_BUFFER_OFFSET are
        /// accepted.
        /// </param>
        /// <param name="params">
        /// Returns the requested data.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetTextureLevelParameter(uint texture, int level, GetTextureParameter pname, [Out] float[] @params)
        {
            unsafe
            {
                fixed (float* p_params = @params)
                {
                    Debug.Assert(Delegates.pglGetTextureLevelParameterfv != null, "pglGetTextureLevelParameterfv not implemented");
                    Delegates.pglGetTextureLevelParameterfv(texture, level, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetTextureLevelParameterfv: return texture parameter values for a specific level of detail
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.GetTextureLevelParameterfv and Gl.GetTextureLevelParameteriv functions.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number of the desired image. Level 0 is the base image level. Level n is the nth mipmap
        /// reduction image.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.TEXTURE_WIDTH, Gl.TEXTURE_HEIGHT, Gl.TEXTURE_DEPTH,
        /// Gl.TEXTURE_INTERNAL_FORMAT, Gl.TEXTURE_RED_SIZE, Gl.TEXTURE_GREEN_SIZE, Gl.TEXTURE_BLUE_SIZE, Gl.TEXTURE_ALPHA_SIZE,
        /// Gl.TEXTURE_DEPTH_SIZE, Gl.TEXTURE_COMPRESSED, Gl.TEXTURE_COMPRESSED_IMAGE_SIZE, and Gl.TEXTURE_BUFFER_OFFSET are
        /// accepted.
        /// </param>
        /// <param name="params">
        /// Returns the requested data.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetTextureLevelParameter(uint texture, int level, GetTextureParameter pname, out float @params)
        {
            unsafe
            {
                fixed (float* p_params = &@params)
                {
                    Debug.Assert(Delegates.pglGetTextureLevelParameterfv != null, "pglGetTextureLevelParameterfv not implemented");
                    Delegates.pglGetTextureLevelParameterfv(texture, level, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetTextureLevelParameterfv: return texture parameter values for a specific level of detail
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.GetTextureLevelParameterfv and Gl.GetTextureLevelParameteriv functions.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number of the desired image. Level 0 is the base image level. Level n is the nth mipmap
        /// reduction image.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.TEXTURE_WIDTH, Gl.TEXTURE_HEIGHT, Gl.TEXTURE_DEPTH,
        /// Gl.TEXTURE_INTERNAL_FORMAT, Gl.TEXTURE_RED_SIZE, Gl.TEXTURE_GREEN_SIZE, Gl.TEXTURE_BLUE_SIZE, Gl.TEXTURE_ALPHA_SIZE,
        /// Gl.TEXTURE_DEPTH_SIZE, Gl.TEXTURE_COMPRESSED, Gl.TEXTURE_COMPRESSED_IMAGE_SIZE, and Gl.TEXTURE_BUFFER_OFFSET are
        /// accepted.
        /// </param>
        /// <param name="params">
        /// Returns the requested data.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static unsafe void GetTextureLevelParameter(uint texture, int level, GetTextureParameter pname, [Out] float* @params)
        {
            Debug.Assert(Delegates.pglGetTextureLevelParameterfv != null, "pglGetTextureLevelParameterfv not implemented");
            Delegates.pglGetTextureLevelParameterfv(texture, level, (int) pname, @params);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetTextureLevelParameterfv: return texture parameter values for a specific level of detail
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.GetTextureLevelParameterfv and Gl.GetTextureLevelParameteriv functions.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number of the desired image. Level 0 is the base image level. Level n is the nth mipmap
        /// reduction image.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.TEXTURE_WIDTH, Gl.TEXTURE_HEIGHT, Gl.TEXTURE_DEPTH,
        /// Gl.TEXTURE_INTERNAL_FORMAT, Gl.TEXTURE_RED_SIZE, Gl.TEXTURE_GREEN_SIZE, Gl.TEXTURE_BLUE_SIZE, Gl.TEXTURE_ALPHA_SIZE,
        /// Gl.TEXTURE_DEPTH_SIZE, Gl.TEXTURE_COMPRESSED, Gl.TEXTURE_COMPRESSED_IMAGE_SIZE, and Gl.TEXTURE_BUFFER_OFFSET are
        /// accepted.
        /// </param>
        /// <param name="params">
        /// Returns the requested data.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetTextureLevelParameterf<T>(uint texture, int level, GetTextureParameter pname, out T @params) where T : struct
        {
            Debug.Assert(Delegates.pglGetTextureLevelParameterfv != null, "pglGetTextureLevelParameterfv not implemented");
            @params = default;
#if NETCOREAPP1_1
			GCHandle valueHandle = GCHandle.Alloc(@params);
			try {
				unsafe {
					Delegates.pglGetTextureLevelParameterfv(texture, level, (int)pname, (float*)valueHandle.AddrOfPinnedObject().ToPointer());
				}
			} finally {
				valueHandle.Free();
			}
#else
            unsafe
            {
                TypedReference refParams = __makeref(@params);
                IntPtr refParamsPtr = *(IntPtr*) (&refParams);

                Delegates.pglGetTextureLevelParameterfv(texture, level, (int) pname, (float*) refParamsPtr.ToPointer());
            }
#endif
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetTextureLevelParameteriv: return texture parameter values for a specific level of detail
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.GetTextureLevelParameterfv and Gl.GetTextureLevelParameteriv functions.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number of the desired image. Level 0 is the base image level. Level n is the nth mipmap
        /// reduction image.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.TEXTURE_WIDTH, Gl.TEXTURE_HEIGHT, Gl.TEXTURE_DEPTH,
        /// Gl.TEXTURE_INTERNAL_FORMAT, Gl.TEXTURE_RED_SIZE, Gl.TEXTURE_GREEN_SIZE, Gl.TEXTURE_BLUE_SIZE, Gl.TEXTURE_ALPHA_SIZE,
        /// Gl.TEXTURE_DEPTH_SIZE, Gl.TEXTURE_COMPRESSED, Gl.TEXTURE_COMPRESSED_IMAGE_SIZE, and Gl.TEXTURE_BUFFER_OFFSET are
        /// accepted.
        /// </param>
        /// <param name="params">
        /// Returns the requested data.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetTextureLevelParameter(uint texture, int level, GetTextureParameter pname, [Out] int[] @params)
        {
            unsafe
            {
                fixed (int* p_params = @params)
                {
                    Debug.Assert(Delegates.pglGetTextureLevelParameteriv != null, "pglGetTextureLevelParameteriv not implemented");
                    Delegates.pglGetTextureLevelParameteriv(texture, level, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetTextureLevelParameteriv: return texture parameter values for a specific level of detail
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.GetTextureLevelParameterfv and Gl.GetTextureLevelParameteriv functions.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number of the desired image. Level 0 is the base image level. Level n is the nth mipmap
        /// reduction image.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.TEXTURE_WIDTH, Gl.TEXTURE_HEIGHT, Gl.TEXTURE_DEPTH,
        /// Gl.TEXTURE_INTERNAL_FORMAT, Gl.TEXTURE_RED_SIZE, Gl.TEXTURE_GREEN_SIZE, Gl.TEXTURE_BLUE_SIZE, Gl.TEXTURE_ALPHA_SIZE,
        /// Gl.TEXTURE_DEPTH_SIZE, Gl.TEXTURE_COMPRESSED, Gl.TEXTURE_COMPRESSED_IMAGE_SIZE, and Gl.TEXTURE_BUFFER_OFFSET are
        /// accepted.
        /// </param>
        /// <param name="params">
        /// Returns the requested data.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetTextureLevelParameter(uint texture, int level, GetTextureParameter pname, out int @params)
        {
            unsafe
            {
                fixed (int* p_params = &@params)
                {
                    Debug.Assert(Delegates.pglGetTextureLevelParameteriv != null, "pglGetTextureLevelParameteriv not implemented");
                    Delegates.pglGetTextureLevelParameteriv(texture, level, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetTextureLevelParameteriv: return texture parameter values for a specific level of detail
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.GetTextureLevelParameterfv and Gl.GetTextureLevelParameteriv functions.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number of the desired image. Level 0 is the base image level. Level n is the nth mipmap
        /// reduction image.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.TEXTURE_WIDTH, Gl.TEXTURE_HEIGHT, Gl.TEXTURE_DEPTH,
        /// Gl.TEXTURE_INTERNAL_FORMAT, Gl.TEXTURE_RED_SIZE, Gl.TEXTURE_GREEN_SIZE, Gl.TEXTURE_BLUE_SIZE, Gl.TEXTURE_ALPHA_SIZE,
        /// Gl.TEXTURE_DEPTH_SIZE, Gl.TEXTURE_COMPRESSED, Gl.TEXTURE_COMPRESSED_IMAGE_SIZE, and Gl.TEXTURE_BUFFER_OFFSET are
        /// accepted.
        /// </param>
        /// <param name="params">
        /// Returns the requested data.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static unsafe void GetTextureLevelParameter(uint texture, int level, GetTextureParameter pname, [Out] int* @params)
        {
            Debug.Assert(Delegates.pglGetTextureLevelParameteriv != null, "pglGetTextureLevelParameteriv not implemented");
            Delegates.pglGetTextureLevelParameteriv(texture, level, (int) pname, @params);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetTextureLevelParameteriv: return texture parameter values for a specific level of detail
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.GetTextureLevelParameterfv and Gl.GetTextureLevelParameteriv functions.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number of the desired image. Level 0 is the base image level. Level n is the nth mipmap
        /// reduction image.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.TEXTURE_WIDTH, Gl.TEXTURE_HEIGHT, Gl.TEXTURE_DEPTH,
        /// Gl.TEXTURE_INTERNAL_FORMAT, Gl.TEXTURE_RED_SIZE, Gl.TEXTURE_GREEN_SIZE, Gl.TEXTURE_BLUE_SIZE, Gl.TEXTURE_ALPHA_SIZE,
        /// Gl.TEXTURE_DEPTH_SIZE, Gl.TEXTURE_COMPRESSED, Gl.TEXTURE_COMPRESSED_IMAGE_SIZE, and Gl.TEXTURE_BUFFER_OFFSET are
        /// accepted.
        /// </param>
        /// <param name="params">
        /// Returns the requested data.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetTextureLevelParameteri<T>(uint texture, int level, GetTextureParameter pname, out T @params) where T : struct
        {
            Debug.Assert(Delegates.pglGetTextureLevelParameteriv != null, "pglGetTextureLevelParameteriv not implemented");
            @params = default;
            unsafe
            {
                TypedReference refParams = __makeref(@params);
                IntPtr refParamsPtr = *(IntPtr*) (&refParams);

                Delegates.pglGetTextureLevelParameteriv(texture, level, (int) pname, (int*) refParamsPtr.ToPointer());
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetTextureParameterfv: return texture parameter values
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.GetTextureParameterfv, Gl.GetTextureParameteriv, Gl.GetTextureParameterIiv,
        /// and
        /// Gl.GetTextureParameterIuiv functions.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.DEPTH_STENCIL_TEXTURE_MODE, Gl.IMAGE_FORMAT_COMPATIBILITY_TYPE,
        /// Gl.TEXTURE_BASE_LEVEL, Gl.TEXTURE_BORDER_COLOR, Gl.TEXTURE_COMPARE_MODE, Gl.TEXTURE_COMPARE_FUNC,
        /// Gl.TEXTURE_IMMUTABLE_FORMAT, Gl.TEXTURE_IMMUTABLE_LEVELS, Gl.TEXTURE_LOD_BIAS, Gl.TEXTURE_MAG_FILTER,
        /// Gl.TEXTURE_MAX_LEVEL, Gl.TEXTURE_MAX_LOD, Gl.TEXTURE_MIN_FILTER, Gl.TEXTURE_MIN_LOD, Gl.TEXTURE_SWIZZLE_R,
        /// Gl.TEXTURE_SWIZZLE_G, Gl.TEXTURE_SWIZZLE_B, Gl.TEXTURE_SWIZZLE_A, Gl.TEXTURE_SWIZZLE_RGBA, Gl.TEXTURE_TARGET,
        /// Gl.TEXTURE_VIEW_MIN_LAYER, Gl.TEXTURE_VIEW_MIN_LEVEL, Gl.TEXTURE_VIEW_NUM_LAYERS, Gl.TEXTURE_VIEW_NUM_LEVELS,
        /// Gl.TEXTURE_WRAP_S, Gl.TEXTURE_WRAP_T, and Gl.TEXTURE_WRAP_R are accepted.
        /// </param>
        /// <param name="params">
        /// Returns the texture parameters.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetTextureParameter(uint texture, GetTextureParameter pname, [Out] float[] @params)
        {
            unsafe
            {
                fixed (float* p_params = @params)
                {
                    Debug.Assert(Delegates.pglGetTextureParameterfv != null, "pglGetTextureParameterfv not implemented");
                    Delegates.pglGetTextureParameterfv(texture, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetTextureParameterfv: return texture parameter values
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.GetTextureParameterfv, Gl.GetTextureParameteriv, Gl.GetTextureParameterIiv,
        /// and
        /// Gl.GetTextureParameterIuiv functions.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.DEPTH_STENCIL_TEXTURE_MODE, Gl.IMAGE_FORMAT_COMPATIBILITY_TYPE,
        /// Gl.TEXTURE_BASE_LEVEL, Gl.TEXTURE_BORDER_COLOR, Gl.TEXTURE_COMPARE_MODE, Gl.TEXTURE_COMPARE_FUNC,
        /// Gl.TEXTURE_IMMUTABLE_FORMAT, Gl.TEXTURE_IMMUTABLE_LEVELS, Gl.TEXTURE_LOD_BIAS, Gl.TEXTURE_MAG_FILTER,
        /// Gl.TEXTURE_MAX_LEVEL, Gl.TEXTURE_MAX_LOD, Gl.TEXTURE_MIN_FILTER, Gl.TEXTURE_MIN_LOD, Gl.TEXTURE_SWIZZLE_R,
        /// Gl.TEXTURE_SWIZZLE_G, Gl.TEXTURE_SWIZZLE_B, Gl.TEXTURE_SWIZZLE_A, Gl.TEXTURE_SWIZZLE_RGBA, Gl.TEXTURE_TARGET,
        /// Gl.TEXTURE_VIEW_MIN_LAYER, Gl.TEXTURE_VIEW_MIN_LEVEL, Gl.TEXTURE_VIEW_NUM_LAYERS, Gl.TEXTURE_VIEW_NUM_LEVELS,
        /// Gl.TEXTURE_WRAP_S, Gl.TEXTURE_WRAP_T, and Gl.TEXTURE_WRAP_R are accepted.
        /// </param>
        /// <param name="params">
        /// Returns the texture parameters.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetTextureParameter(uint texture, GetTextureParameter pname, out float @params)
        {
            unsafe
            {
                fixed (float* p_params = &@params)
                {
                    Debug.Assert(Delegates.pglGetTextureParameterfv != null, "pglGetTextureParameterfv not implemented");
                    Delegates.pglGetTextureParameterfv(texture, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetTextureParameterfv: return texture parameter values
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.GetTextureParameterfv, Gl.GetTextureParameteriv, Gl.GetTextureParameterIiv,
        /// and
        /// Gl.GetTextureParameterIuiv functions.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.DEPTH_STENCIL_TEXTURE_MODE, Gl.IMAGE_FORMAT_COMPATIBILITY_TYPE,
        /// Gl.TEXTURE_BASE_LEVEL, Gl.TEXTURE_BORDER_COLOR, Gl.TEXTURE_COMPARE_MODE, Gl.TEXTURE_COMPARE_FUNC,
        /// Gl.TEXTURE_IMMUTABLE_FORMAT, Gl.TEXTURE_IMMUTABLE_LEVELS, Gl.TEXTURE_LOD_BIAS, Gl.TEXTURE_MAG_FILTER,
        /// Gl.TEXTURE_MAX_LEVEL, Gl.TEXTURE_MAX_LOD, Gl.TEXTURE_MIN_FILTER, Gl.TEXTURE_MIN_LOD, Gl.TEXTURE_SWIZZLE_R,
        /// Gl.TEXTURE_SWIZZLE_G, Gl.TEXTURE_SWIZZLE_B, Gl.TEXTURE_SWIZZLE_A, Gl.TEXTURE_SWIZZLE_RGBA, Gl.TEXTURE_TARGET,
        /// Gl.TEXTURE_VIEW_MIN_LAYER, Gl.TEXTURE_VIEW_MIN_LEVEL, Gl.TEXTURE_VIEW_NUM_LAYERS, Gl.TEXTURE_VIEW_NUM_LEVELS,
        /// Gl.TEXTURE_WRAP_S, Gl.TEXTURE_WRAP_T, and Gl.TEXTURE_WRAP_R are accepted.
        /// </param>
        /// <param name="params">
        /// Returns the texture parameters.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static unsafe void GetTextureParameter(uint texture, GetTextureParameter pname, [Out] float* @params)
        {
            Debug.Assert(Delegates.pglGetTextureParameterfv != null, "pglGetTextureParameterfv not implemented");
            Delegates.pglGetTextureParameterfv(texture, (int) pname, @params);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetTextureParameterfv: return texture parameter values
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.GetTextureParameterfv, Gl.GetTextureParameteriv, Gl.GetTextureParameterIiv,
        /// and
        /// Gl.GetTextureParameterIuiv functions.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.DEPTH_STENCIL_TEXTURE_MODE, Gl.IMAGE_FORMAT_COMPATIBILITY_TYPE,
        /// Gl.TEXTURE_BASE_LEVEL, Gl.TEXTURE_BORDER_COLOR, Gl.TEXTURE_COMPARE_MODE, Gl.TEXTURE_COMPARE_FUNC,
        /// Gl.TEXTURE_IMMUTABLE_FORMAT, Gl.TEXTURE_IMMUTABLE_LEVELS, Gl.TEXTURE_LOD_BIAS, Gl.TEXTURE_MAG_FILTER,
        /// Gl.TEXTURE_MAX_LEVEL, Gl.TEXTURE_MAX_LOD, Gl.TEXTURE_MIN_FILTER, Gl.TEXTURE_MIN_LOD, Gl.TEXTURE_SWIZZLE_R,
        /// Gl.TEXTURE_SWIZZLE_G, Gl.TEXTURE_SWIZZLE_B, Gl.TEXTURE_SWIZZLE_A, Gl.TEXTURE_SWIZZLE_RGBA, Gl.TEXTURE_TARGET,
        /// Gl.TEXTURE_VIEW_MIN_LAYER, Gl.TEXTURE_VIEW_MIN_LEVEL, Gl.TEXTURE_VIEW_NUM_LAYERS, Gl.TEXTURE_VIEW_NUM_LEVELS,
        /// Gl.TEXTURE_WRAP_S, Gl.TEXTURE_WRAP_T, and Gl.TEXTURE_WRAP_R are accepted.
        /// </param>
        /// <param name="params">
        /// Returns the texture parameters.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetTextureParameterf<T>(uint texture, GetTextureParameter pname, out T @params) where T : struct
        {
            Debug.Assert(Delegates.pglGetTextureParameterfv != null, "pglGetTextureParameterfv not implemented");
            @params = default;
#if NETCOREAPP1_1
			GCHandle valueHandle = GCHandle.Alloc(@params);
			try {
				unsafe {
					Delegates.pglGetTextureParameterfv(texture, (int)pname, (float*)valueHandle.AddrOfPinnedObject().ToPointer());
				}
			} finally {
				valueHandle.Free();
			}
#else
            unsafe
            {
                TypedReference refParams = __makeref(@params);
                IntPtr refParamsPtr = *(IntPtr*) (&refParams);

                Delegates.pglGetTextureParameterfv(texture, (int) pname, (float*) refParamsPtr.ToPointer());
            }
#endif
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetTextureParameterIiv: return texture parameter values
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.GetTextureParameterfv, Gl.GetTextureParameteriv, Gl.GetTextureParameterIiv,
        /// and
        /// Gl.GetTextureParameterIuiv functions.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.DEPTH_STENCIL_TEXTURE_MODE, Gl.IMAGE_FORMAT_COMPATIBILITY_TYPE,
        /// Gl.TEXTURE_BASE_LEVEL, Gl.TEXTURE_BORDER_COLOR, Gl.TEXTURE_COMPARE_MODE, Gl.TEXTURE_COMPARE_FUNC,
        /// Gl.TEXTURE_IMMUTABLE_FORMAT, Gl.TEXTURE_IMMUTABLE_LEVELS, Gl.TEXTURE_LOD_BIAS, Gl.TEXTURE_MAG_FILTER,
        /// Gl.TEXTURE_MAX_LEVEL, Gl.TEXTURE_MAX_LOD, Gl.TEXTURE_MIN_FILTER, Gl.TEXTURE_MIN_LOD, Gl.TEXTURE_SWIZZLE_R,
        /// Gl.TEXTURE_SWIZZLE_G, Gl.TEXTURE_SWIZZLE_B, Gl.TEXTURE_SWIZZLE_A, Gl.TEXTURE_SWIZZLE_RGBA, Gl.TEXTURE_TARGET,
        /// Gl.TEXTURE_VIEW_MIN_LAYER, Gl.TEXTURE_VIEW_MIN_LEVEL, Gl.TEXTURE_VIEW_NUM_LAYERS, Gl.TEXTURE_VIEW_NUM_LEVELS,
        /// Gl.TEXTURE_WRAP_S, Gl.TEXTURE_WRAP_T, and Gl.TEXTURE_WRAP_R are accepted.
        /// </param>
        /// <param name="params">
        /// Returns the texture parameters.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetTextureParameterI(uint texture, GetTextureParameter pname, [Out] int[] @params)
        {
            unsafe
            {
                fixed (int* p_params = @params)
                {
                    Debug.Assert(Delegates.pglGetTextureParameterIiv != null, "pglGetTextureParameterIiv not implemented");
                    Delegates.pglGetTextureParameterIiv(texture, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetTextureParameterIiv: return texture parameter values
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.GetTextureParameterfv, Gl.GetTextureParameteriv, Gl.GetTextureParameterIiv,
        /// and
        /// Gl.GetTextureParameterIuiv functions.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.DEPTH_STENCIL_TEXTURE_MODE, Gl.IMAGE_FORMAT_COMPATIBILITY_TYPE,
        /// Gl.TEXTURE_BASE_LEVEL, Gl.TEXTURE_BORDER_COLOR, Gl.TEXTURE_COMPARE_MODE, Gl.TEXTURE_COMPARE_FUNC,
        /// Gl.TEXTURE_IMMUTABLE_FORMAT, Gl.TEXTURE_IMMUTABLE_LEVELS, Gl.TEXTURE_LOD_BIAS, Gl.TEXTURE_MAG_FILTER,
        /// Gl.TEXTURE_MAX_LEVEL, Gl.TEXTURE_MAX_LOD, Gl.TEXTURE_MIN_FILTER, Gl.TEXTURE_MIN_LOD, Gl.TEXTURE_SWIZZLE_R,
        /// Gl.TEXTURE_SWIZZLE_G, Gl.TEXTURE_SWIZZLE_B, Gl.TEXTURE_SWIZZLE_A, Gl.TEXTURE_SWIZZLE_RGBA, Gl.TEXTURE_TARGET,
        /// Gl.TEXTURE_VIEW_MIN_LAYER, Gl.TEXTURE_VIEW_MIN_LEVEL, Gl.TEXTURE_VIEW_NUM_LAYERS, Gl.TEXTURE_VIEW_NUM_LEVELS,
        /// Gl.TEXTURE_WRAP_S, Gl.TEXTURE_WRAP_T, and Gl.TEXTURE_WRAP_R are accepted.
        /// </param>
        /// <param name="params">
        /// Returns the texture parameters.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetTextureParameterI(uint texture, GetTextureParameter pname, out int @params)
        {
            unsafe
            {
                fixed (int* p_params = &@params)
                {
                    Debug.Assert(Delegates.pglGetTextureParameterIiv != null, "pglGetTextureParameterIiv not implemented");
                    Delegates.pglGetTextureParameterIiv(texture, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetTextureParameterIiv: return texture parameter values
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.GetTextureParameterfv, Gl.GetTextureParameteriv, Gl.GetTextureParameterIiv,
        /// and
        /// Gl.GetTextureParameterIuiv functions.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.DEPTH_STENCIL_TEXTURE_MODE, Gl.IMAGE_FORMAT_COMPATIBILITY_TYPE,
        /// Gl.TEXTURE_BASE_LEVEL, Gl.TEXTURE_BORDER_COLOR, Gl.TEXTURE_COMPARE_MODE, Gl.TEXTURE_COMPARE_FUNC,
        /// Gl.TEXTURE_IMMUTABLE_FORMAT, Gl.TEXTURE_IMMUTABLE_LEVELS, Gl.TEXTURE_LOD_BIAS, Gl.TEXTURE_MAG_FILTER,
        /// Gl.TEXTURE_MAX_LEVEL, Gl.TEXTURE_MAX_LOD, Gl.TEXTURE_MIN_FILTER, Gl.TEXTURE_MIN_LOD, Gl.TEXTURE_SWIZZLE_R,
        /// Gl.TEXTURE_SWIZZLE_G, Gl.TEXTURE_SWIZZLE_B, Gl.TEXTURE_SWIZZLE_A, Gl.TEXTURE_SWIZZLE_RGBA, Gl.TEXTURE_TARGET,
        /// Gl.TEXTURE_VIEW_MIN_LAYER, Gl.TEXTURE_VIEW_MIN_LEVEL, Gl.TEXTURE_VIEW_NUM_LAYERS, Gl.TEXTURE_VIEW_NUM_LEVELS,
        /// Gl.TEXTURE_WRAP_S, Gl.TEXTURE_WRAP_T, and Gl.TEXTURE_WRAP_R are accepted.
        /// </param>
        /// <param name="params">
        /// Returns the texture parameters.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static unsafe void GetTextureParameterI(uint texture, GetTextureParameter pname, [Out] int* @params)
        {
            Debug.Assert(Delegates.pglGetTextureParameterIiv != null, "pglGetTextureParameterIiv not implemented");
            Delegates.pglGetTextureParameterIiv(texture, (int) pname, @params);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetTextureParameterIiv: return texture parameter values
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.GetTextureParameterfv, Gl.GetTextureParameteriv, Gl.GetTextureParameterIiv,
        /// and
        /// Gl.GetTextureParameterIuiv functions.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.DEPTH_STENCIL_TEXTURE_MODE, Gl.IMAGE_FORMAT_COMPATIBILITY_TYPE,
        /// Gl.TEXTURE_BASE_LEVEL, Gl.TEXTURE_BORDER_COLOR, Gl.TEXTURE_COMPARE_MODE, Gl.TEXTURE_COMPARE_FUNC,
        /// Gl.TEXTURE_IMMUTABLE_FORMAT, Gl.TEXTURE_IMMUTABLE_LEVELS, Gl.TEXTURE_LOD_BIAS, Gl.TEXTURE_MAG_FILTER,
        /// Gl.TEXTURE_MAX_LEVEL, Gl.TEXTURE_MAX_LOD, Gl.TEXTURE_MIN_FILTER, Gl.TEXTURE_MIN_LOD, Gl.TEXTURE_SWIZZLE_R,
        /// Gl.TEXTURE_SWIZZLE_G, Gl.TEXTURE_SWIZZLE_B, Gl.TEXTURE_SWIZZLE_A, Gl.TEXTURE_SWIZZLE_RGBA, Gl.TEXTURE_TARGET,
        /// Gl.TEXTURE_VIEW_MIN_LAYER, Gl.TEXTURE_VIEW_MIN_LEVEL, Gl.TEXTURE_VIEW_NUM_LAYERS, Gl.TEXTURE_VIEW_NUM_LEVELS,
        /// Gl.TEXTURE_WRAP_S, Gl.TEXTURE_WRAP_T, and Gl.TEXTURE_WRAP_R are accepted.
        /// </param>
        /// <param name="params">
        /// Returns the texture parameters.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetTextureParameterIi<T>(uint texture, GetTextureParameter pname, out T @params) where T : struct
        {
            Debug.Assert(Delegates.pglGetTextureParameterIiv != null, "pglGetTextureParameterIiv not implemented");
            @params = default;
#if NETCOREAPP1_1
			GCHandle valueHandle = GCHandle.Alloc(@params);
			try {
				unsafe {
					Delegates.pglGetTextureParameterIiv(texture, (int)pname, (int*)valueHandle.AddrOfPinnedObject().ToPointer());
				}
			} finally {
				valueHandle.Free();
			}
#else
            unsafe
            {
                TypedReference refParams = __makeref(@params);
                IntPtr refParamsPtr = *(IntPtr*) (&refParams);

                Delegates.pglGetTextureParameterIiv(texture, (int) pname, (int*) refParamsPtr.ToPointer());
            }
#endif
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetTextureParameterIuiv: return texture parameter values
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.GetTextureParameterfv, Gl.GetTextureParameteriv, Gl.GetTextureParameterIiv,
        /// and
        /// Gl.GetTextureParameterIuiv functions.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.DEPTH_STENCIL_TEXTURE_MODE, Gl.IMAGE_FORMAT_COMPATIBILITY_TYPE,
        /// Gl.TEXTURE_BASE_LEVEL, Gl.TEXTURE_BORDER_COLOR, Gl.TEXTURE_COMPARE_MODE, Gl.TEXTURE_COMPARE_FUNC,
        /// Gl.TEXTURE_IMMUTABLE_FORMAT, Gl.TEXTURE_IMMUTABLE_LEVELS, Gl.TEXTURE_LOD_BIAS, Gl.TEXTURE_MAG_FILTER,
        /// Gl.TEXTURE_MAX_LEVEL, Gl.TEXTURE_MAX_LOD, Gl.TEXTURE_MIN_FILTER, Gl.TEXTURE_MIN_LOD, Gl.TEXTURE_SWIZZLE_R,
        /// Gl.TEXTURE_SWIZZLE_G, Gl.TEXTURE_SWIZZLE_B, Gl.TEXTURE_SWIZZLE_A, Gl.TEXTURE_SWIZZLE_RGBA, Gl.TEXTURE_TARGET,
        /// Gl.TEXTURE_VIEW_MIN_LAYER, Gl.TEXTURE_VIEW_MIN_LEVEL, Gl.TEXTURE_VIEW_NUM_LAYERS, Gl.TEXTURE_VIEW_NUM_LEVELS,
        /// Gl.TEXTURE_WRAP_S, Gl.TEXTURE_WRAP_T, and Gl.TEXTURE_WRAP_R are accepted.
        /// </param>
        /// <param name="params">
        /// Returns the texture parameters.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetTextureParameterI(uint texture, GetTextureParameter pname, [Out] uint[] @params)
        {
            unsafe
            {
                fixed (uint* p_params = @params)
                {
                    Debug.Assert(Delegates.pglGetTextureParameterIuiv != null, "pglGetTextureParameterIuiv not implemented");
                    Delegates.pglGetTextureParameterIuiv(texture, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetTextureParameterIuiv: return texture parameter values
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.GetTextureParameterfv, Gl.GetTextureParameteriv, Gl.GetTextureParameterIiv,
        /// and
        /// Gl.GetTextureParameterIuiv functions.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.DEPTH_STENCIL_TEXTURE_MODE, Gl.IMAGE_FORMAT_COMPATIBILITY_TYPE,
        /// Gl.TEXTURE_BASE_LEVEL, Gl.TEXTURE_BORDER_COLOR, Gl.TEXTURE_COMPARE_MODE, Gl.TEXTURE_COMPARE_FUNC,
        /// Gl.TEXTURE_IMMUTABLE_FORMAT, Gl.TEXTURE_IMMUTABLE_LEVELS, Gl.TEXTURE_LOD_BIAS, Gl.TEXTURE_MAG_FILTER,
        /// Gl.TEXTURE_MAX_LEVEL, Gl.TEXTURE_MAX_LOD, Gl.TEXTURE_MIN_FILTER, Gl.TEXTURE_MIN_LOD, Gl.TEXTURE_SWIZZLE_R,
        /// Gl.TEXTURE_SWIZZLE_G, Gl.TEXTURE_SWIZZLE_B, Gl.TEXTURE_SWIZZLE_A, Gl.TEXTURE_SWIZZLE_RGBA, Gl.TEXTURE_TARGET,
        /// Gl.TEXTURE_VIEW_MIN_LAYER, Gl.TEXTURE_VIEW_MIN_LEVEL, Gl.TEXTURE_VIEW_NUM_LAYERS, Gl.TEXTURE_VIEW_NUM_LEVELS,
        /// Gl.TEXTURE_WRAP_S, Gl.TEXTURE_WRAP_T, and Gl.TEXTURE_WRAP_R are accepted.
        /// </param>
        /// <param name="params">
        /// Returns the texture parameters.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetTextureParameterI(uint texture, GetTextureParameter pname, out uint @params)
        {
            unsafe
            {
                fixed (uint* p_params = &@params)
                {
                    Debug.Assert(Delegates.pglGetTextureParameterIuiv != null, "pglGetTextureParameterIuiv not implemented");
                    Delegates.pglGetTextureParameterIuiv(texture, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetTextureParameterIuiv: return texture parameter values
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.GetTextureParameterfv, Gl.GetTextureParameteriv, Gl.GetTextureParameterIiv,
        /// and
        /// Gl.GetTextureParameterIuiv functions.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.DEPTH_STENCIL_TEXTURE_MODE, Gl.IMAGE_FORMAT_COMPATIBILITY_TYPE,
        /// Gl.TEXTURE_BASE_LEVEL, Gl.TEXTURE_BORDER_COLOR, Gl.TEXTURE_COMPARE_MODE, Gl.TEXTURE_COMPARE_FUNC,
        /// Gl.TEXTURE_IMMUTABLE_FORMAT, Gl.TEXTURE_IMMUTABLE_LEVELS, Gl.TEXTURE_LOD_BIAS, Gl.TEXTURE_MAG_FILTER,
        /// Gl.TEXTURE_MAX_LEVEL, Gl.TEXTURE_MAX_LOD, Gl.TEXTURE_MIN_FILTER, Gl.TEXTURE_MIN_LOD, Gl.TEXTURE_SWIZZLE_R,
        /// Gl.TEXTURE_SWIZZLE_G, Gl.TEXTURE_SWIZZLE_B, Gl.TEXTURE_SWIZZLE_A, Gl.TEXTURE_SWIZZLE_RGBA, Gl.TEXTURE_TARGET,
        /// Gl.TEXTURE_VIEW_MIN_LAYER, Gl.TEXTURE_VIEW_MIN_LEVEL, Gl.TEXTURE_VIEW_NUM_LAYERS, Gl.TEXTURE_VIEW_NUM_LEVELS,
        /// Gl.TEXTURE_WRAP_S, Gl.TEXTURE_WRAP_T, and Gl.TEXTURE_WRAP_R are accepted.
        /// </param>
        /// <param name="params">
        /// Returns the texture parameters.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static unsafe void GetTextureParameterI(uint texture, GetTextureParameter pname, [Out] uint* @params)
        {
            Debug.Assert(Delegates.pglGetTextureParameterIuiv != null, "pglGetTextureParameterIuiv not implemented");
            Delegates.pglGetTextureParameterIuiv(texture, (int) pname, @params);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetTextureParameterIuiv: return texture parameter values
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.GetTextureParameterfv, Gl.GetTextureParameteriv, Gl.GetTextureParameterIiv,
        /// and
        /// Gl.GetTextureParameterIuiv functions.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.DEPTH_STENCIL_TEXTURE_MODE, Gl.IMAGE_FORMAT_COMPATIBILITY_TYPE,
        /// Gl.TEXTURE_BASE_LEVEL, Gl.TEXTURE_BORDER_COLOR, Gl.TEXTURE_COMPARE_MODE, Gl.TEXTURE_COMPARE_FUNC,
        /// Gl.TEXTURE_IMMUTABLE_FORMAT, Gl.TEXTURE_IMMUTABLE_LEVELS, Gl.TEXTURE_LOD_BIAS, Gl.TEXTURE_MAG_FILTER,
        /// Gl.TEXTURE_MAX_LEVEL, Gl.TEXTURE_MAX_LOD, Gl.TEXTURE_MIN_FILTER, Gl.TEXTURE_MIN_LOD, Gl.TEXTURE_SWIZZLE_R,
        /// Gl.TEXTURE_SWIZZLE_G, Gl.TEXTURE_SWIZZLE_B, Gl.TEXTURE_SWIZZLE_A, Gl.TEXTURE_SWIZZLE_RGBA, Gl.TEXTURE_TARGET,
        /// Gl.TEXTURE_VIEW_MIN_LAYER, Gl.TEXTURE_VIEW_MIN_LEVEL, Gl.TEXTURE_VIEW_NUM_LAYERS, Gl.TEXTURE_VIEW_NUM_LEVELS,
        /// Gl.TEXTURE_WRAP_S, Gl.TEXTURE_WRAP_T, and Gl.TEXTURE_WRAP_R are accepted.
        /// </param>
        /// <param name="params">
        /// Returns the texture parameters.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetTextureParameterIui<T>(uint texture, GetTextureParameter pname, out T @params) where T : struct
        {
            Debug.Assert(Delegates.pglGetTextureParameterIuiv != null, "pglGetTextureParameterIuiv not implemented");
            @params = default;
#if NETCOREAPP1_1
			GCHandle valueHandle = GCHandle.Alloc(@params);
			try {
				unsafe {
					Delegates.pglGetTextureParameterIuiv(texture, (int)pname, (uint*)valueHandle.AddrOfPinnedObject().ToPointer());
				}
			} finally {
				valueHandle.Free();
			}
#else
            unsafe
            {
                TypedReference refParams = __makeref(@params);
                IntPtr refParamsPtr = *(IntPtr*) (&refParams);

                Delegates.pglGetTextureParameterIuiv(texture, (int) pname, (uint*) refParamsPtr.ToPointer());
            }
#endif
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetTextureParameteriv: return texture parameter values
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.GetTextureParameterfv, Gl.GetTextureParameteriv, Gl.GetTextureParameterIiv,
        /// and
        /// Gl.GetTextureParameterIuiv functions.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.DEPTH_STENCIL_TEXTURE_MODE, Gl.IMAGE_FORMAT_COMPATIBILITY_TYPE,
        /// Gl.TEXTURE_BASE_LEVEL, Gl.TEXTURE_BORDER_COLOR, Gl.TEXTURE_COMPARE_MODE, Gl.TEXTURE_COMPARE_FUNC,
        /// Gl.TEXTURE_IMMUTABLE_FORMAT, Gl.TEXTURE_IMMUTABLE_LEVELS, Gl.TEXTURE_LOD_BIAS, Gl.TEXTURE_MAG_FILTER,
        /// Gl.TEXTURE_MAX_LEVEL, Gl.TEXTURE_MAX_LOD, Gl.TEXTURE_MIN_FILTER, Gl.TEXTURE_MIN_LOD, Gl.TEXTURE_SWIZZLE_R,
        /// Gl.TEXTURE_SWIZZLE_G, Gl.TEXTURE_SWIZZLE_B, Gl.TEXTURE_SWIZZLE_A, Gl.TEXTURE_SWIZZLE_RGBA, Gl.TEXTURE_TARGET,
        /// Gl.TEXTURE_VIEW_MIN_LAYER, Gl.TEXTURE_VIEW_MIN_LEVEL, Gl.TEXTURE_VIEW_NUM_LAYERS, Gl.TEXTURE_VIEW_NUM_LEVELS,
        /// Gl.TEXTURE_WRAP_S, Gl.TEXTURE_WRAP_T, and Gl.TEXTURE_WRAP_R are accepted.
        /// </param>
        /// <param name="params">
        /// Returns the texture parameters.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetTextureParameter(uint texture, GetTextureParameter pname, [Out] int[] @params)
        {
            unsafe
            {
                fixed (int* p_params = @params)
                {
                    Debug.Assert(Delegates.pglGetTextureParameteriv != null, "pglGetTextureParameteriv not implemented");
                    Delegates.pglGetTextureParameteriv(texture, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetTextureParameteriv: return texture parameter values
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.GetTextureParameterfv, Gl.GetTextureParameteriv, Gl.GetTextureParameterIiv,
        /// and
        /// Gl.GetTextureParameterIuiv functions.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.DEPTH_STENCIL_TEXTURE_MODE, Gl.IMAGE_FORMAT_COMPATIBILITY_TYPE,
        /// Gl.TEXTURE_BASE_LEVEL, Gl.TEXTURE_BORDER_COLOR, Gl.TEXTURE_COMPARE_MODE, Gl.TEXTURE_COMPARE_FUNC,
        /// Gl.TEXTURE_IMMUTABLE_FORMAT, Gl.TEXTURE_IMMUTABLE_LEVELS, Gl.TEXTURE_LOD_BIAS, Gl.TEXTURE_MAG_FILTER,
        /// Gl.TEXTURE_MAX_LEVEL, Gl.TEXTURE_MAX_LOD, Gl.TEXTURE_MIN_FILTER, Gl.TEXTURE_MIN_LOD, Gl.TEXTURE_SWIZZLE_R,
        /// Gl.TEXTURE_SWIZZLE_G, Gl.TEXTURE_SWIZZLE_B, Gl.TEXTURE_SWIZZLE_A, Gl.TEXTURE_SWIZZLE_RGBA, Gl.TEXTURE_TARGET,
        /// Gl.TEXTURE_VIEW_MIN_LAYER, Gl.TEXTURE_VIEW_MIN_LEVEL, Gl.TEXTURE_VIEW_NUM_LAYERS, Gl.TEXTURE_VIEW_NUM_LEVELS,
        /// Gl.TEXTURE_WRAP_S, Gl.TEXTURE_WRAP_T, and Gl.TEXTURE_WRAP_R are accepted.
        /// </param>
        /// <param name="params">
        /// Returns the texture parameters.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetTextureParameter(uint texture, GetTextureParameter pname, out int @params)
        {
            unsafe
            {
                fixed (int* p_params = &@params)
                {
                    Debug.Assert(Delegates.pglGetTextureParameteriv != null, "pglGetTextureParameteriv not implemented");
                    Delegates.pglGetTextureParameteriv(texture, (int) pname, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetTextureParameteriv: return texture parameter values
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.GetTextureParameterfv, Gl.GetTextureParameteriv, Gl.GetTextureParameterIiv,
        /// and
        /// Gl.GetTextureParameterIuiv functions.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.DEPTH_STENCIL_TEXTURE_MODE, Gl.IMAGE_FORMAT_COMPATIBILITY_TYPE,
        /// Gl.TEXTURE_BASE_LEVEL, Gl.TEXTURE_BORDER_COLOR, Gl.TEXTURE_COMPARE_MODE, Gl.TEXTURE_COMPARE_FUNC,
        /// Gl.TEXTURE_IMMUTABLE_FORMAT, Gl.TEXTURE_IMMUTABLE_LEVELS, Gl.TEXTURE_LOD_BIAS, Gl.TEXTURE_MAG_FILTER,
        /// Gl.TEXTURE_MAX_LEVEL, Gl.TEXTURE_MAX_LOD, Gl.TEXTURE_MIN_FILTER, Gl.TEXTURE_MIN_LOD, Gl.TEXTURE_SWIZZLE_R,
        /// Gl.TEXTURE_SWIZZLE_G, Gl.TEXTURE_SWIZZLE_B, Gl.TEXTURE_SWIZZLE_A, Gl.TEXTURE_SWIZZLE_RGBA, Gl.TEXTURE_TARGET,
        /// Gl.TEXTURE_VIEW_MIN_LAYER, Gl.TEXTURE_VIEW_MIN_LEVEL, Gl.TEXTURE_VIEW_NUM_LAYERS, Gl.TEXTURE_VIEW_NUM_LEVELS,
        /// Gl.TEXTURE_WRAP_S, Gl.TEXTURE_WRAP_T, and Gl.TEXTURE_WRAP_R are accepted.
        /// </param>
        /// <param name="params">
        /// Returns the texture parameters.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static unsafe void GetTextureParameter(uint texture, GetTextureParameter pname, [Out] int* @params)
        {
            Debug.Assert(Delegates.pglGetTextureParameteriv != null, "pglGetTextureParameteriv not implemented");
            Delegates.pglGetTextureParameteriv(texture, (int) pname, @params);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetTextureParameteriv: return texture parameter values
        /// </summary>
        /// <param name="texture">
        /// Specifies the texture object name for Gl.GetTextureParameterfv, Gl.GetTextureParameteriv, Gl.GetTextureParameterIiv,
        /// and
        /// Gl.GetTextureParameterIuiv functions.
        /// </param>
        /// <param name="pname">
        /// Specifies the symbolic name of a texture parameter. Gl.DEPTH_STENCIL_TEXTURE_MODE, Gl.IMAGE_FORMAT_COMPATIBILITY_TYPE,
        /// Gl.TEXTURE_BASE_LEVEL, Gl.TEXTURE_BORDER_COLOR, Gl.TEXTURE_COMPARE_MODE, Gl.TEXTURE_COMPARE_FUNC,
        /// Gl.TEXTURE_IMMUTABLE_FORMAT, Gl.TEXTURE_IMMUTABLE_LEVELS, Gl.TEXTURE_LOD_BIAS, Gl.TEXTURE_MAG_FILTER,
        /// Gl.TEXTURE_MAX_LEVEL, Gl.TEXTURE_MAX_LOD, Gl.TEXTURE_MIN_FILTER, Gl.TEXTURE_MIN_LOD, Gl.TEXTURE_SWIZZLE_R,
        /// Gl.TEXTURE_SWIZZLE_G, Gl.TEXTURE_SWIZZLE_B, Gl.TEXTURE_SWIZZLE_A, Gl.TEXTURE_SWIZZLE_RGBA, Gl.TEXTURE_TARGET,
        /// Gl.TEXTURE_VIEW_MIN_LAYER, Gl.TEXTURE_VIEW_MIN_LEVEL, Gl.TEXTURE_VIEW_NUM_LAYERS, Gl.TEXTURE_VIEW_NUM_LEVELS,
        /// Gl.TEXTURE_WRAP_S, Gl.TEXTURE_WRAP_T, and Gl.TEXTURE_WRAP_R are accepted.
        /// </param>
        /// <param name="params">
        /// Returns the texture parameters.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetTextureParameteri<T>(uint texture, GetTextureParameter pname, out T @params) where T : struct
        {
            Debug.Assert(Delegates.pglGetTextureParameteriv != null, "pglGetTextureParameteriv not implemented");
            @params = default;
#if NETCOREAPP1_1
			GCHandle valueHandle = GCHandle.Alloc(@params);
			try {
				unsafe {
					Delegates.pglGetTextureParameteriv(texture, (int)pname, (int*)valueHandle.AddrOfPinnedObject().ToPointer());
				}
			} finally {
				valueHandle.Free();
			}
#else
            unsafe
            {
                TypedReference refParams = __makeref(@params);
                IntPtr refParamsPtr = *(IntPtr*) (&refParams);

                Delegates.pglGetTextureParameteriv(texture, (int) pname, (int*) refParamsPtr.ToPointer());
            }
#endif
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glCreateVertexArrays: create vertex array objects
        /// </summary>
        /// <param name="arrays">
        /// Specifies an array in which names of the new vertex array objects are stored.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void CreateVertexArrays(uint[] arrays)
        {
            unsafe
            {
                fixed (uint* p_arrays = arrays)
                {
                    Debug.Assert(Delegates.pglCreateVertexArrays != null, "pglCreateVertexArrays not implemented");
                    Delegates.pglCreateVertexArrays(arrays.Length, p_arrays);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glCreateVertexArrays: create vertex array objects
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static uint CreateVertexArray()
        {
            uint retValue;
            unsafe
            {
                Delegates.pglCreateVertexArrays(1, &retValue);
            }

            DebugCheckErrors(null);
            return retValue;
        }

        /// <summary>
        /// [GL4] glDisableVertexArrayAttrib: Enable or disable a generic vertex attribute array
        /// </summary>
        /// <param name="vaobj">
        /// Specifies the name of the vertex array object for Gl.DisableVertexArrayAttrib and Gl.EnableVertexArrayAttrib functions.
        /// </param>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be enabled or disabled.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void DisableVertexArrayAttrib(uint vaobj, uint index)
        {
            Debug.Assert(Delegates.pglDisableVertexArrayAttrib != null, "pglDisableVertexArrayAttrib not implemented");
            Delegates.pglDisableVertexArrayAttrib(vaobj, index);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glEnableVertexArrayAttrib: Enable or disable a generic vertex attribute array
        /// </summary>
        /// <param name="vaobj">
        /// Specifies the name of the vertex array object for Gl.DisableVertexArrayAttrib and Gl.EnableVertexArrayAttrib functions.
        /// </param>
        /// <param name="index">
        /// Specifies the index of the generic vertex attribute to be enabled or disabled.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void EnableVertexArrayAttrib(uint vaobj, uint index)
        {
            Debug.Assert(Delegates.pglEnableVertexArrayAttrib != null, "pglEnableVertexArrayAttrib not implemented");
            Delegates.pglEnableVertexArrayAttrib(vaobj, index);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexArrayElementBuffer: configures element array buffer binding of a vertex array object
        /// </summary>
        /// <param name="vaobj">
        /// Specifies the name of the vertex array object.
        /// </param>
        /// <param name="buffer">
        /// Specifies the name of the buffer object to use for the element array buffer binding.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void VertexArrayElementBuffer(uint vaobj, uint buffer)
        {
            Debug.Assert(Delegates.pglVertexArrayElementBuffer != null, "pglVertexArrayElementBuffer not implemented");
            Delegates.pglVertexArrayElementBuffer(vaobj, buffer);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexArrayVertexBuffer: bind a buffer to a vertex buffer bind point
        /// </summary>
        /// <param name="vaobj">
        /// Specifies the name of the vertex array object to be used by Gl.VertexArrayVertexBuffer function.
        /// </param>
        /// <param name="bindingindex">
        /// The index of the vertex buffer binding point to which to bind the buffer.
        /// </param>
        /// <param name="buffer">
        /// The name of a buffer to bind to the vertex buffer binding point.
        /// </param>
        /// <param name="offset">
        /// The offset of the first element of the buffer.
        /// </param>
        /// <param name="stride">
        /// The distance between elements within the buffer.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void VertexArrayVertexBuffer(uint vaobj, uint bindingindex, uint buffer, IntPtr offset, int stride)
        {
            Debug.Assert(Delegates.pglVertexArrayVertexBuffer != null, "pglVertexArrayVertexBuffer not implemented");
            Delegates.pglVertexArrayVertexBuffer(vaobj, bindingindex, buffer, offset, stride);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexArrayVertexBuffers: attach multiple buffer objects to a vertex array object
        /// </summary>
        /// <param name="vaobj">
        /// Specifies the name of the vertex array object for Gl.VertexArrayVertexBuffers.
        /// </param>
        /// <param name="first">
        /// Specifies the first vertex buffer binding point to which a buffer object is to be bound.
        /// </param>
        /// <param name="count">
        /// Specifies the number of buffers to bind.
        /// </param>
        /// <param name="buffers">
        /// Specifies the address of an array of names of existing buffer objects.
        /// </param>
        /// <param name="offsets">
        /// Specifies the address of an array of offsets to associate with the binding points.
        /// </param>
        /// <param name="strides">
        /// A <see cref="T:int[]" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void VertexArrayVertexBuffers(uint vaobj, uint first, int count, uint[] buffers, IntPtr[] offsets, int[] strides)
        {
            unsafe
            {
                fixed (uint* p_buffers = buffers)
                fixed (IntPtr* p_offsets = offsets)
                fixed (int* p_strides = strides)
                {
                    Debug.Assert(Delegates.pglVertexArrayVertexBuffers != null, "pglVertexArrayVertexBuffers not implemented");
                    Delegates.pglVertexArrayVertexBuffers(vaobj, first, count, p_buffers, p_offsets, p_strides);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexArrayAttribBinding: associate a vertex attribute and a vertex buffer binding for a vertex array object
        /// </summary>
        /// <param name="vaobj">
        /// Specifies the name of the vertex array object for Gl.VertexArrayAttribBinding.
        /// </param>
        /// <param name="attribindex">
        /// The index of the attribute to associate with a vertex buffer binding.
        /// </param>
        /// <param name="bindingindex">
        /// The index of the vertex buffer binding with which to associate the generic vertex attribute.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void VertexArrayAttribBinding(uint vaobj, uint attribindex, uint bindingindex)
        {
            Debug.Assert(Delegates.pglVertexArrayAttribBinding != null, "pglVertexArrayAttribBinding not implemented");
            Delegates.pglVertexArrayAttribBinding(vaobj, attribindex, bindingindex);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexArrayAttribFormat: specify the organization of vertex arrays
        /// </summary>
        /// <param name="vaobj">
        /// Specifies the name of the vertex array object for Gl.VertexArrayAttrib{I, L}Format functions.
        /// </param>
        /// <param name="attribindex">
        /// The generic vertex attribute array being described.
        /// </param>
        /// <param name="size">
        /// The number of values per vertex that are stored in the array.
        /// </param>
        /// <param name="type">
        /// The type of the data stored in the array.
        /// </param>
        /// <param name="normalized">
        /// Specifies whether fixed-point data values should be normalized (Gl.TRUE) or converted directly as fixed-point values
        /// (Gl.FALSE) when they are accessed. This parameter is ignored if <paramref name="type" /> is Gl.FIXED.
        /// </param>
        /// <param name="relativeoffset">
        /// The distance between elements within the buffer.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void VertexArrayAttribFormat(uint vaobj, uint attribindex, int size, VertexAttribType type, bool normalized, uint relativeoffset)
        {
            Debug.Assert(Delegates.pglVertexArrayAttribFormat != null, "pglVertexArrayAttribFormat not implemented");
            Delegates.pglVertexArrayAttribFormat(vaobj, attribindex, size, (int) type, normalized, relativeoffset);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexArrayAttribIFormat: specify the organization of vertex arrays
        /// </summary>
        /// <param name="vaobj">
        /// Specifies the name of the vertex array object for Gl.VertexArrayAttrib{I, L}Format functions.
        /// </param>
        /// <param name="attribindex">
        /// The generic vertex attribute array being described.
        /// </param>
        /// <param name="size">
        /// The number of values per vertex that are stored in the array.
        /// </param>
        /// <param name="type">
        /// The type of the data stored in the array.
        /// </param>
        /// <param name="relativeoffset">
        /// The distance between elements within the buffer.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void VertexArrayAttribIFormat(uint vaobj, uint attribindex, int size, VertexAttribType type, uint relativeoffset)
        {
            Debug.Assert(Delegates.pglVertexArrayAttribIFormat != null, "pglVertexArrayAttribIFormat not implemented");
            Delegates.pglVertexArrayAttribIFormat(vaobj, attribindex, size, (int) type, relativeoffset);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexArrayAttribLFormat: specify the organization of vertex arrays
        /// </summary>
        /// <param name="vaobj">
        /// Specifies the name of the vertex array object for Gl.VertexArrayAttrib{I, L}Format functions.
        /// </param>
        /// <param name="attribindex">
        /// The generic vertex attribute array being described.
        /// </param>
        /// <param name="size">
        /// The number of values per vertex that are stored in the array.
        /// </param>
        /// <param name="type">
        /// The type of the data stored in the array.
        /// </param>
        /// <param name="relativeoffset">
        /// The distance between elements within the buffer.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void VertexArrayAttribLFormat(uint vaobj, uint attribindex, int size, VertexAttribType type, uint relativeoffset)
        {
            Debug.Assert(Delegates.pglVertexArrayAttribLFormat != null, "pglVertexArrayAttribLFormat not implemented");
            Delegates.pglVertexArrayAttribLFormat(vaobj, attribindex, size, (int) type, relativeoffset);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glVertexArrayBindingDivisor: modify the rate at which generic vertex attributes advance
        /// </summary>
        /// <param name="vaobj">
        /// Specifies the name of the vertex array object for Gl.VertexArrayBindingDivisor function.
        /// </param>
        /// <param name="bindingindex">
        /// The index of the binding whose divisor to modify.
        /// </param>
        /// <param name="divisor">
        /// The new value for the instance step rate to apply.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void VertexArrayBindingDivisor(uint vaobj, uint bindingindex, uint divisor)
        {
            Debug.Assert(Delegates.pglVertexArrayBindingDivisor != null, "pglVertexArrayBindingDivisor not implemented");
            Delegates.pglVertexArrayBindingDivisor(vaobj, bindingindex, divisor);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL] glGetVertexArrayiv: Binding for glGetVertexArrayiv.
        /// </summary>
        /// <param name="vaobj">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="pname">
        /// A <see cref="T:VertexArrayPName" />.
        /// </param>
        /// <param name="param">
        /// A <see cref="T:int[]" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetVertexArray(uint vaobj, VertexArrayPName pname, [Out] int[] param)
        {
            unsafe
            {
                fixed (int* p_param = param)
                {
                    Debug.Assert(Delegates.pglGetVertexArrayiv != null, "pglGetVertexArrayiv not implemented");
                    Delegates.pglGetVertexArrayiv(vaobj, (int) pname, p_param);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetVertexArrayIndexediv: retrieve parameters of an attribute of a vertex array object
        /// </summary>
        /// <param name="vaobj">
        /// Specifies the name of a vertex array object.
        /// </param>
        /// <param name="index">
        /// Specifies the index of the vertex array object attribute. Must be a number between 0 and (Gl.MAX_VERTEX_ATTRIBS - 1).
        /// </param>
        /// <param name="pname">
        /// Specifies the property to be used for the query. For Gl.GetVertexArrayIndexediv, it must be one of the following
        /// values:
        /// Gl.VERTEX_ATTRIB_ARRAY_ENABLED, Gl.VERTEX_ATTRIB_ARRAY_SIZE, Gl.VERTEX_ATTRIB_ARRAY_STRIDE,
        /// Gl.VERTEX_ATTRIB_ARRAY_TYPE,
        /// Gl.VERTEX_ATTRIB_ARRAY_NORMALIZED, Gl.VERTEX_ATTRIB_ARRAY_INTEGER, Gl.VERTEX_ATTRIB_ARRAY_LONG,
        /// Gl.VERTEX_ATTRIB_ARRAY_DIVISOR, or Gl.VERTEX_ATTRIB_RELATIVE_OFFSET. For Gl.GetVertexArrayIndexed64v, it must be equal
        /// to Gl.VERTEX_BINDING_OFFSET.
        /// </param>
        /// <param name="param">
        /// Returns the requested value.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetVertexArrayIndexed(uint vaobj, uint index, VertexArrayPName pname, [Out] int[] param)
        {
            unsafe
            {
                fixed (int* p_param = param)
                {
                    Debug.Assert(Delegates.pglGetVertexArrayIndexediv != null, "pglGetVertexArrayIndexediv not implemented");
                    Delegates.pglGetVertexArrayIndexediv(vaobj, index, (int) pname, p_param);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetVertexArrayIndexed64iv: retrieve parameters of an attribute of a vertex array object
        /// </summary>
        /// <param name="vaobj">
        /// Specifies the name of a vertex array object.
        /// </param>
        /// <param name="index">
        /// Specifies the index of the vertex array object attribute. Must be a number between 0 and (Gl.MAX_VERTEX_ATTRIBS - 1).
        /// </param>
        /// <param name="pname">
        /// Specifies the property to be used for the query. For Gl.GetVertexArrayIndexediv, it must be one of the following
        /// values:
        /// Gl.VERTEX_ATTRIB_ARRAY_ENABLED, Gl.VERTEX_ATTRIB_ARRAY_SIZE, Gl.VERTEX_ATTRIB_ARRAY_STRIDE,
        /// Gl.VERTEX_ATTRIB_ARRAY_TYPE,
        /// Gl.VERTEX_ATTRIB_ARRAY_NORMALIZED, Gl.VERTEX_ATTRIB_ARRAY_INTEGER, Gl.VERTEX_ATTRIB_ARRAY_LONG,
        /// Gl.VERTEX_ATTRIB_ARRAY_DIVISOR, or Gl.VERTEX_ATTRIB_RELATIVE_OFFSET. For Gl.GetVertexArrayIndexed64v, it must be equal
        /// to Gl.VERTEX_BINDING_OFFSET.
        /// </param>
        /// <param name="param">
        /// Returns the requested value.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetVertexArrayIndexed(uint vaobj, uint index, VertexArrayPName pname, [Out] long[] param)
        {
            unsafe
            {
                fixed (long* p_param = param)
                {
                    Debug.Assert(Delegates.pglGetVertexArrayIndexed64iv != null, "pglGetVertexArrayIndexed64iv not implemented");
                    Delegates.pglGetVertexArrayIndexed64iv(vaobj, index, (int) pname, p_param);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glCreateSamplers: create sampler objects
        /// </summary>
        /// <param name="samplers">
        /// Specifies an array in which names of the new sampler objects are stored.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void CreateSamplers(uint[] samplers)
        {
            unsafe
            {
                fixed (uint* p_samplers = samplers)
                {
                    Debug.Assert(Delegates.pglCreateSamplers != null, "pglCreateSamplers not implemented");
                    Delegates.pglCreateSamplers(samplers.Length, p_samplers);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glCreateSamplers: create sampler objects
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static uint CreateSampler()
        {
            uint retValue;
            unsafe
            {
                Delegates.pglCreateSamplers(1, &retValue);
            }

            DebugCheckErrors(null);
            return retValue;
        }

        /// <summary>
        /// [GL4] glCreateProgramPipelines: create program pipeline objects
        /// </summary>
        /// <param name="pipelines">
        /// Specifies an array in which names of the new program pipeline objects are stored.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void CreateProgramPipelines(uint[] pipelines)
        {
            unsafe
            {
                fixed (uint* p_pipelines = pipelines)
                {
                    Debug.Assert(Delegates.pglCreateProgramPipelines != null, "pglCreateProgramPipelines not implemented");
                    Delegates.pglCreateProgramPipelines(pipelines.Length, p_pipelines);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glCreateProgramPipelines: create program pipeline objects
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static uint CreateProgramPipeline()
        {
            uint retValue;
            unsafe
            {
                Delegates.pglCreateProgramPipelines(1, &retValue);
            }

            DebugCheckErrors(null);
            return retValue;
        }

        /// <summary>
        /// [GL4] glCreateQueries: create query objects
        /// </summary>
        /// <param name="target">
        /// Specifies the target of each created query object.
        /// </param>
        /// <param name="n">
        /// Number of query objects to create.
        /// </param>
        /// <param name="ids">
        /// Specifies an array in which names of the new query objects are stored.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void CreateQueries(QueryTarget target, int n, uint[] ids)
        {
            unsafe
            {
                fixed (uint* p_ids = ids)
                {
                    Debug.Assert(Delegates.pglCreateQueries != null, "pglCreateQueries not implemented");
                    Delegates.pglCreateQueries((int) target, n, p_ids);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glCreateQueries: create query objects
        /// </summary>
        /// <param name="target">
        /// Specifies the target of each created query object.
        /// </param>
        /// <param name="ids">
        /// Specifies an array in which names of the new query objects are stored.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void CreateQueries(QueryTarget target, uint[] ids)
        {
            unsafe
            {
                fixed (uint* p_ids = ids)
                {
                    Debug.Assert(Delegates.pglCreateQueries != null, "pglCreateQueries not implemented");
                    Delegates.pglCreateQueries((int) target, ids.Length, p_ids);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glCreateQueries: create query objects
        /// </summary>
        /// <param name="target">
        /// Specifies the target of each created query object.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static uint CreateQuery(QueryTarget target)
        {
            uint retValue;
            unsafe
            {
                Delegates.pglCreateQueries((int) target, 1, &retValue);
            }

            DebugCheckErrors(null);
            return retValue;
        }

        /// <summary>
        /// [GL] glGetQueryBufferObjecti64v: Binding for glGetQueryBufferObjecti64v.
        /// </summary>
        /// <param name="id">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="buffer">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="pname">
        /// A <see cref="T:QueryObjectParameterName" />.
        /// </param>
        /// <param name="offset">
        /// A <see cref="T:IntPtr" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetQueryBufferObject64i(uint id, uint buffer, QueryObjectParameterName pname, IntPtr offset)
        {
            Debug.Assert(Delegates.pglGetQueryBufferObjecti64v != null, "pglGetQueryBufferObjecti64v not implemented");
            Delegates.pglGetQueryBufferObjecti64v(id, buffer, (int) pname, offset);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL] glGetQueryBufferObjectiv: Binding for glGetQueryBufferObjectiv.
        /// </summary>
        /// <param name="id">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="buffer">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="pname">
        /// A <see cref="T:QueryObjectParameterName" />.
        /// </param>
        /// <param name="offset">
        /// A <see cref="T:IntPtr" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetQueryBufferObject32i(uint id, uint buffer, QueryObjectParameterName pname, IntPtr offset)
        {
            Debug.Assert(Delegates.pglGetQueryBufferObjectiv != null, "pglGetQueryBufferObjectiv not implemented");
            Delegates.pglGetQueryBufferObjectiv(id, buffer, (int) pname, offset);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL] glGetQueryBufferObjectui64v: Binding for glGetQueryBufferObjectui64v.
        /// </summary>
        /// <param name="id">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="buffer">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="pname">
        /// A <see cref="T:QueryObjectParameterName" />.
        /// </param>
        /// <param name="offset">
        /// A <see cref="T:IntPtr" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetQueryBufferObject64ui(uint id, uint buffer, QueryObjectParameterName pname, IntPtr offset)
        {
            Debug.Assert(Delegates.pglGetQueryBufferObjectui64v != null, "pglGetQueryBufferObjectui64v not implemented");
            Delegates.pglGetQueryBufferObjectui64v(id, buffer, (int) pname, offset);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL] glGetQueryBufferObjectuiv: Binding for glGetQueryBufferObjectuiv.
        /// </summary>
        /// <param name="id">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="buffer">
        /// A <see cref="T:uint" />.
        /// </param>
        /// <param name="pname">
        /// A <see cref="T:QueryObjectParameterName" />.
        /// </param>
        /// <param name="offset">
        /// A <see cref="T:IntPtr" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        public static void GetQueryBufferObject32ui(uint id, uint buffer, QueryObjectParameterName pname, IntPtr offset)
        {
            Debug.Assert(Delegates.pglGetQueryBufferObjectuiv != null, "pglGetQueryBufferObjectuiv not implemented");
            Delegates.pglGetQueryBufferObjectuiv(id, buffer, (int) pname, offset);
            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glMemoryBarrierByRegion: defines a barrier ordering memory transactions
        ///     </para>
        /// </summary>
        /// <param name="barriers">
        /// Specifies the barriers to insert.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_ARB_ES3_1_compatibility", Api = "gl|glcore")]
        public static void MemoryBarrierByRegion(MemoryBarrierMask barriers)
        {
            Debug.Assert(Delegates.pglMemoryBarrierByRegion != null, "pglMemoryBarrierByRegion not implemented");
            Delegates.pglMemoryBarrierByRegion((uint) barriers);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetTextureSubImage: retrieve a sub-region of a texture image from a texture object
        /// </summary>
        /// <param name="texture">
        /// Specifies the name of the source texture object. Must be Gl.TEXTURE_1D, Gl.TEXTURE_1D_ARRAY, Gl.TEXTURE_2D,
        /// Gl.TEXTURE_2D_ARRAY, Gl.TEXTURE_3D, Gl.TEXTURE_CUBE_MAP, Gl.TEXTURE_CUBE_MAP_ARRAY or Gl.TEXTURE_RECTANGLE. In
        /// specific,
        /// buffer and multisample textures are not permitted.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number. Level 0 is the base image level. Level $n$ is the $n$th mipmap reduction image.
        /// </param>
        /// <param name="xoffset">
        /// Specifies a texel offset in the x direction within the texture array.
        /// </param>
        /// <param name="yoffset">
        /// Specifies a texel offset in the y direction within the texture array.
        /// </param>
        /// <param name="zoffset">
        /// Specifies a texel offset in the z direction within the texture array.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture subimage.
        /// </param>
        /// <param name="height">
        /// Specifies the height of the texture subimage.
        /// </param>
        /// <param name="depth">
        /// Specifies the depth of the texture subimage.
        /// </param>
        /// <param name="format">
        /// Specifies the format of the pixel data. The following symbolic values are accepted: Gl.RED, Gl.RG, Gl.RGB, Gl.BGR,
        /// Gl.RGBA, Gl.BGRA, Gl.DEPTH_COMPONENT and Gl.STENCIL_INDEX.
        /// </param>
        /// <param name="type">
        /// Specifies the data type of the pixel data. The following symbolic values are accepted: Gl.UNSIGNED_BYTE, Gl.BYTE,
        /// Gl.UNSIGNED_SHORT, Gl.SHORT, Gl.UNSIGNED_INT, Gl.INT, Gl.FLOAT, Gl.UNSIGNED_BYTE_3_3_2, Gl.UNSIGNED_BYTE_2_3_3_REV,
        /// Gl.UNSIGNED_SHORT_5_6_5, Gl.UNSIGNED_SHORT_5_6_5_REV, Gl.UNSIGNED_SHORT_4_4_4_4, Gl.UNSIGNED_SHORT_4_4_4_4_REV,
        /// Gl.UNSIGNED_SHORT_5_5_5_1, Gl.UNSIGNED_SHORT_1_5_5_5_REV, Gl.UNSIGNED_INT_8_8_8_8, Gl.UNSIGNED_INT_8_8_8_8_REV,
        /// Gl.UNSIGNED_INT_10_10_10_2, and Gl.UNSIGNED_INT_2_10_10_10_REV.
        /// </param>
        /// <param name="bufSize">
        /// Specifies the size of the buffer to receive the retrieved pixel data.
        /// </param>
        /// <param name="pixels">
        /// Returns the texture subimage. Should be a pointer to an array of the type specified by <paramref name="type" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_get_texture_sub_image", Api = "gl|glcore")]
        public static void GetTextureSubImage(uint texture, int level, int xoffset, int yoffset, int zoffset, int width, int height, int depth, PixelFormat format, PixelType type, int bufSize,
            IntPtr pixels)
        {
            Debug.Assert(Delegates.pglGetTextureSubImage != null, "pglGetTextureSubImage not implemented");
            Delegates.pglGetTextureSubImage(texture, level, xoffset, yoffset, zoffset, width, height, depth, (int) format, (int) type, bufSize, pixels);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetTextureSubImage: retrieve a sub-region of a texture image from a texture object
        /// </summary>
        /// <param name="texture">
        /// Specifies the name of the source texture object. Must be Gl.TEXTURE_1D, Gl.TEXTURE_1D_ARRAY, Gl.TEXTURE_2D,
        /// Gl.TEXTURE_2D_ARRAY, Gl.TEXTURE_3D, Gl.TEXTURE_CUBE_MAP, Gl.TEXTURE_CUBE_MAP_ARRAY or Gl.TEXTURE_RECTANGLE. In
        /// specific,
        /// buffer and multisample textures are not permitted.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number. Level 0 is the base image level. Level $n$ is the $n$th mipmap reduction image.
        /// </param>
        /// <param name="xoffset">
        /// Specifies a texel offset in the x direction within the texture array.
        /// </param>
        /// <param name="yoffset">
        /// Specifies a texel offset in the y direction within the texture array.
        /// </param>
        /// <param name="zoffset">
        /// Specifies a texel offset in the z direction within the texture array.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture subimage.
        /// </param>
        /// <param name="height">
        /// Specifies the height of the texture subimage.
        /// </param>
        /// <param name="depth">
        /// Specifies the depth of the texture subimage.
        /// </param>
        /// <param name="format">
        /// Specifies the format of the pixel data. The following symbolic values are accepted: Gl.RED, Gl.RG, Gl.RGB, Gl.BGR,
        /// Gl.RGBA, Gl.BGRA, Gl.DEPTH_COMPONENT and Gl.STENCIL_INDEX.
        /// </param>
        /// <param name="type">
        /// Specifies the data type of the pixel data. The following symbolic values are accepted: Gl.UNSIGNED_BYTE, Gl.BYTE,
        /// Gl.UNSIGNED_SHORT, Gl.SHORT, Gl.UNSIGNED_INT, Gl.INT, Gl.FLOAT, Gl.UNSIGNED_BYTE_3_3_2, Gl.UNSIGNED_BYTE_2_3_3_REV,
        /// Gl.UNSIGNED_SHORT_5_6_5, Gl.UNSIGNED_SHORT_5_6_5_REV, Gl.UNSIGNED_SHORT_4_4_4_4, Gl.UNSIGNED_SHORT_4_4_4_4_REV,
        /// Gl.UNSIGNED_SHORT_5_5_5_1, Gl.UNSIGNED_SHORT_1_5_5_5_REV, Gl.UNSIGNED_INT_8_8_8_8, Gl.UNSIGNED_INT_8_8_8_8_REV,
        /// Gl.UNSIGNED_INT_10_10_10_2, and Gl.UNSIGNED_INT_2_10_10_10_REV.
        /// </param>
        /// <param name="bufSize">
        /// Specifies the size of the buffer to receive the retrieved pixel data.
        /// </param>
        /// <param name="pixels">
        /// Returns the texture subimage. Should be a pointer to an array of the type specified by <paramref name="type" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_get_texture_sub_image", Api = "gl|glcore")]
        public static void GetTextureSubImage(uint texture, int level, int xoffset, int yoffset, int zoffset, int width, int height, int depth, PixelFormat format, PixelType type, int bufSize,
            object pixels)
        {
            GCHandle pin_pixels = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            try
            {
                GetTextureSubImage(texture, level, xoffset, yoffset, zoffset, width, height, depth, format, type, bufSize, pin_pixels.AddrOfPinnedObject());
            }
            finally
            {
                pin_pixels.Free();
            }
        }

        /// <summary>
        /// [GL4] glGetCompressedTextureSubImage: retrieve a sub-region of a compressed texture image from a compressed texture
        /// object
        /// </summary>
        /// <param name="texture">
        /// Specifies the name of the source texture object. Must be Gl.TEXTURE_1D, Gl.TEXTURE_1D_ARRAY, Gl.TEXTURE_2D,
        /// Gl.TEXTURE_2D_ARRAY, Gl.TEXTURE_3D, Gl.TEXTURE_CUBE_MAP, Gl.TEXTURE_CUBE_MAP_ARRAY or Gl.TEXTURE_RECTANGLE. In
        /// specific,
        /// buffer and multisample textures are not permitted.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number. Level 0 is the base image level. Level $n$ is the $n$th mipmap reduction image.
        /// </param>
        /// <param name="xoffset">
        /// Specifies a texel offset in the x direction within the texture array.
        /// </param>
        /// <param name="yoffset">
        /// Specifies a texel offset in the y direction within the texture array.
        /// </param>
        /// <param name="zoffset">
        /// Specifies a texel offset in the z direction within the texture array.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture subimage. Must be a multiple of the compressed block's width, unless any of the
        /// offsets is zero and the size equals the texture image size.
        /// </param>
        /// <param name="height">
        /// Specifies the height of the texture subimage. Must be a multiple of the compressed block's height, unless any of the
        /// offsets is zero and the size equals the texture image size.
        /// </param>
        /// <param name="depth">
        /// Specifies the depth of the texture subimage. Must be a multiple of the compressed block's depth, unless any of the
        /// offsets is zero and the size equals the texture image size.
        /// </param>
        /// <param name="bufSize">
        /// Specifies the size of the buffer to receive the retrieved pixel data.
        /// </param>
        /// <param name="pixels">
        /// Returns the texture subimage. Should be a pointer to an array of the type specified by type.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_get_texture_sub_image", Api = "gl|glcore")]
        public static void GetCompressedTextureSubImage(uint texture, int level, int xoffset, int yoffset, int zoffset, int width, int height, int depth, int bufSize, IntPtr pixels)
        {
            Debug.Assert(Delegates.pglGetCompressedTextureSubImage != null, "pglGetCompressedTextureSubImage not implemented");
            Delegates.pglGetCompressedTextureSubImage(texture, level, xoffset, yoffset, zoffset, width, height, depth, bufSize, pixels);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetCompressedTextureSubImage: retrieve a sub-region of a compressed texture image from a compressed texture
        /// object
        /// </summary>
        /// <param name="texture">
        /// Specifies the name of the source texture object. Must be Gl.TEXTURE_1D, Gl.TEXTURE_1D_ARRAY, Gl.TEXTURE_2D,
        /// Gl.TEXTURE_2D_ARRAY, Gl.TEXTURE_3D, Gl.TEXTURE_CUBE_MAP, Gl.TEXTURE_CUBE_MAP_ARRAY or Gl.TEXTURE_RECTANGLE. In
        /// specific,
        /// buffer and multisample textures are not permitted.
        /// </param>
        /// <param name="level">
        /// Specifies the level-of-detail number. Level 0 is the base image level. Level $n$ is the $n$th mipmap reduction image.
        /// </param>
        /// <param name="xoffset">
        /// Specifies a texel offset in the x direction within the texture array.
        /// </param>
        /// <param name="yoffset">
        /// Specifies a texel offset in the y direction within the texture array.
        /// </param>
        /// <param name="zoffset">
        /// Specifies a texel offset in the z direction within the texture array.
        /// </param>
        /// <param name="width">
        /// Specifies the width of the texture subimage. Must be a multiple of the compressed block's width, unless any of the
        /// offsets is zero and the size equals the texture image size.
        /// </param>
        /// <param name="height">
        /// Specifies the height of the texture subimage. Must be a multiple of the compressed block's height, unless any of the
        /// offsets is zeroo and the size equals the texture image size.
        /// </param>
        /// <param name="depth">
        /// Specifies the depth of the texture subimage. Must be a multiple of the compressed block's depth, unless any of the
        /// offsets is zero and the size equals the texture image size.
        /// </param>
        /// <param name="bufSize">
        /// Specifies the size of the buffer to receive the retrieved pixel data.
        /// </param>
        /// <param name="pixels">
        /// Returns the texture subimage. Should be a pointer to an array of the type specified by type.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_get_texture_sub_image", Api = "gl|glcore")]
        public static void GetCompressedTextureSubImage(uint texture, int level, int xoffset, int yoffset, int zoffset, int width, int height, int depth, int bufSize, object pixels)
        {
            GCHandle pin_pixels = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            try
            {
                GetCompressedTextureSubImage(texture, level, xoffset, yoffset, zoffset, width, height, depth, bufSize, pin_pixels.AddrOfPinnedObject());
            }
            finally
            {
                pin_pixels.Free();
            }
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetGraphicsResetStatus: check if the rendering context has not been lost due to software or
        ///     hardware
        ///     issues
        ///     </para>
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_robustness", Api = "gles1|gles2")]
        [RequiredByFeature("GL_KHR_robustness")]
        [RequiredByFeature("GL_KHR_robustness", Api = "gles2")]
        public static GraphicsResetStatus GetGraphicsResetStatus()
        {
            int retValue;

            Debug.Assert(Delegates.pglGetGraphicsResetStatus != null, "pglGetGraphicsResetStatus not implemented");
            retValue = Delegates.pglGetGraphicsResetStatus();
            DebugCheckErrors(retValue);

            return (GraphicsResetStatus) retValue;
        }

        /// <summary>
        /// [GL4] glGetnCompressedTexImage: return a compressed texture image
        /// </summary>
        /// <param name="target">
        /// Specifies the target to which the texture is bound for Gl.GetCompressedTexImage and Gl.GetnCompressedTexImage
        /// functions.
        /// Gl.TEXTURE_1D, Gl.TEXTURE_1D_ARRAY, Gl.TEXTURE_2D, Gl.TEXTURE_2D_ARRAY, Gl.TEXTURE_3D, Gl.TEXTURE_CUBE_MAP_ARRAY,
        /// Gl.TEXTURE_CUBE_MAP_POSITIVE_X, Gl.TEXTURE_CUBE_MAP_NEGATIVE_X, Gl.TEXTURE_CUBE_MAP_POSITIVE_Y,
        /// Gl.TEXTURE_CUBE_MAP_NEGATIVE_Y, Gl.TEXTURE_CUBE_MAP_POSITIVE_Z, and Gl.TEXTURE_CUBE_MAP_NEGATIVE_Z,
        /// Gl.TEXTURE_RECTANGLE
        /// are accepted.
        /// </param>
        /// <param name="lod">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="bufSize">
        /// Specifies the size of the buffer <paramref name="pixels" /> for Gl.GetCompressedTextureImage and
        /// Gl.GetnCompressedTexImage functions.
        /// </param>
        /// <param name="pixels">
        /// Returns the compressed texture image.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        public static void GetnCompressedTexImage(TextureTarget target, int lod, int bufSize, IntPtr pixels)
        {
            Debug.Assert(Delegates.pglGetnCompressedTexImage != null, "pglGetnCompressedTexImage not implemented");
            Delegates.pglGetnCompressedTexImage((int) target, lod, bufSize, pixels);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glGetnCompressedTexImage: return a compressed texture image
        /// </summary>
        /// <param name="target">
        /// Specifies the target to which the texture is bound for Gl.GetCompressedTexImage and Gl.GetnCompressedTexImage
        /// functions.
        /// Gl.TEXTURE_1D, Gl.TEXTURE_1D_ARRAY, Gl.TEXTURE_2D, Gl.TEXTURE_2D_ARRAY, Gl.TEXTURE_3D, Gl.TEXTURE_CUBE_MAP_ARRAY,
        /// Gl.TEXTURE_CUBE_MAP_POSITIVE_X, Gl.TEXTURE_CUBE_MAP_NEGATIVE_X, Gl.TEXTURE_CUBE_MAP_POSITIVE_Y,
        /// Gl.TEXTURE_CUBE_MAP_NEGATIVE_Y, Gl.TEXTURE_CUBE_MAP_POSITIVE_Z, and Gl.TEXTURE_CUBE_MAP_NEGATIVE_Z,
        /// Gl.TEXTURE_RECTANGLE
        /// are accepted.
        /// </param>
        /// <param name="lod">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="bufSize">
        /// Specifies the size of the buffer <paramref name="pixels" /> for Gl.GetCompressedTextureImage and
        /// Gl.GetnCompressedTexImage functions.
        /// </param>
        /// <param name="pixels">
        /// Returns the compressed texture image.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        public static void GetnCompressedTexImage(TextureTarget target, int lod, int bufSize, object pixels)
        {
            GCHandle pin_pixels = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            try
            {
                GetnCompressedTexImage(target, lod, bufSize, pin_pixels.AddrOfPinnedObject());
            }
            finally
            {
                pin_pixels.Free();
            }
        }

        /// <summary>
        /// [GL] glGetnTexImage: Binding for glGetnTexImage.
        /// </summary>
        /// <param name="target">
        /// A <see cref="T:TextureTarget" />.
        /// </param>
        /// <param name="level">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="format">
        /// A <see cref="T:PixelFormat" />.
        /// </param>
        /// <param name="type">
        /// A <see cref="T:PixelType" />.
        /// </param>
        /// <param name="bufSize">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="pixels">
        /// A <see cref="T:IntPtr" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        public static void GetnTexImage(TextureTarget target, int level, PixelFormat format, PixelType type, int bufSize, IntPtr pixels)
        {
            Debug.Assert(Delegates.pglGetnTexImage != null, "pglGetnTexImage not implemented");
            Delegates.pglGetnTexImage((int) target, level, (int) format, (int) type, bufSize, pixels);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL] glGetnTexImage: Binding for glGetnTexImage.
        /// </summary>
        /// <param name="target">
        /// A <see cref="T:TextureTarget" />.
        /// </param>
        /// <param name="level">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="format">
        /// A <see cref="T:PixelFormat" />.
        /// </param>
        /// <param name="type">
        /// A <see cref="T:PixelType" />.
        /// </param>
        /// <param name="bufSize">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="pixels">
        /// A <see cref="T:object" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        public static void GetnTexImage(TextureTarget target, int level, PixelFormat format, PixelType type, int bufSize, object pixels)
        {
            GCHandle pin_pixels = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            try
            {
                GetnTexImage(target, level, format, type, bufSize, pin_pixels.AddrOfPinnedObject());
            }
            finally
            {
                pin_pixels.Free();
            }
        }

        /// <summary>
        /// [GL4] glGetnUniformdv: Returns the value of a uniform variable
        /// </summary>
        /// <param name="program">
        /// Specifies the program object to be queried.
        /// </param>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be queried.
        /// </param>
        /// <param name="params">
        /// Returns the value of the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        public static void GetnUniform(uint program, int location, [Out] double[] @params)
        {
            unsafe
            {
                fixed (double* p_params = @params)
                {
                    Debug.Assert(Delegates.pglGetnUniformdv != null, "pglGetnUniformdv not implemented");
                    Delegates.pglGetnUniformdv(program, location, @params.Length, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetnUniformfv: Returns the value of a uniform variable
        ///     </para>
        /// </summary>
        /// <param name="program">
        /// Specifies the program object to be queried.
        /// </param>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be queried.
        /// </param>
        /// <param name="params">
        /// Returns the value of the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_robustness", Api = "gles1|gles2")]
        [RequiredByFeature("GL_KHR_robustness")]
        [RequiredByFeature("GL_KHR_robustness", Api = "gles2")]
        public static void GetnUniform(uint program, int location, [Out] float[] @params)
        {
            unsafe
            {
                fixed (float* p_params = @params)
                {
                    Debug.Assert(Delegates.pglGetnUniformfv != null, "pglGetnUniformfv not implemented");
                    Delegates.pglGetnUniformfv(program, location, @params.Length, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetnUniformiv: Returns the value of a uniform variable
        ///     </para>
        /// </summary>
        /// <param name="program">
        /// Specifies the program object to be queried.
        /// </param>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be queried.
        /// </param>
        /// <param name="params">
        /// Returns the value of the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_robustness", Api = "gles1|gles2")]
        [RequiredByFeature("GL_KHR_robustness")]
        [RequiredByFeature("GL_KHR_robustness", Api = "gles2")]
        public static void GetnUniform(uint program, int location, [Out] int[] @params)
        {
            unsafe
            {
                fixed (int* p_params = @params)
                {
                    Debug.Assert(Delegates.pglGetnUniformiv != null, "pglGetnUniformiv not implemented");
                    Delegates.pglGetnUniformiv(program, location, @params.Length, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        ///     <para>
        ///     [GL4|GLES3.2] glGetnUniformuiv: Returns the value of a uniform variable
        ///     </para>
        /// </summary>
        /// <param name="program">
        /// Specifies the program object to be queried.
        /// </param>
        /// <param name="location">
        /// Specifies the location of the uniform variable to be queried.
        /// </param>
        /// <param name="params">
        /// Returns the value of the specified uniform variable.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_robustness")]
        [RequiredByFeature("GL_KHR_robustness", Api = "gles2")]
        public static void GetnUniform(uint program, int location, [Out] uint[] @params)
        {
            unsafe
            {
                fixed (uint* p_params = @params)
                {
                    Debug.Assert(Delegates.pglGetnUniformuiv != null, "pglGetnUniformuiv not implemented");
                    Delegates.pglGetnUniformuiv(program, location, @params.Length, p_params);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL] glReadnPixels: Binding for glReadnPixels.
        /// </summary>
        /// <param name="x">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="y">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="width">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="height">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="format">
        /// A <see cref="T:PixelFormat" />.
        /// </param>
        /// <param name="type">
        /// A <see cref="T:PixelType" />.
        /// </param>
        /// <param name="bufSize">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="data">
        /// A <see cref="T:IntPtr" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_robustness", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_robustness", Api = "gles1|gles2")]
        [RequiredByFeature("GL_KHR_robustness")]
        [RequiredByFeature("GL_KHR_robustness", Api = "gles2")]
        public static void ReadnPixels(int x, int y, int width, int height, PixelFormat format, PixelType type, int bufSize, IntPtr data)
        {
            Debug.Assert(Delegates.pglReadnPixels != null, "pglReadnPixels not implemented");
            Delegates.pglReadnPixels(x, y, width, height, (int) format, (int) type, bufSize, data);
            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL] glGetnPolygonStipple: Binding for glGetnPolygonStipple.
        /// </summary>
        /// <param name="bufSize">
        /// A <see cref="T:int" />.
        /// </param>
        /// <param name="pattern">
        /// A <see cref="T:byte[]" />.
        /// </param>
        [RequiredByFeature("GL_VERSION_4_5", Profile = "compatibility")]
        public static void GetnPolygonStipple(int bufSize, [Out] byte[] pattern)
        {
            unsafe
            {
                fixed (byte* p_pattern = pattern)
                {
                    Debug.Assert(Delegates.pglGetnPolygonStipple != null, "pglGetnPolygonStipple not implemented");
                    Delegates.pglGetnPolygonStipple(bufSize, p_pattern);
                }
            }

            DebugCheckErrors(null);
        }

        /// <summary>
        /// [GL4] glTextureBarrier: controls the ordering of reads and writes to rendered fragments across drawing commands
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_texture_barrier", Api = "gl|glcore")]
        public static void TextureBarrier()
        {
            Debug.Assert(Delegates.pglTextureBarrier != null, "pglTextureBarrier not implemented");
            Delegates.pglTextureBarrier();
            DebugCheckErrors(null);
        }

        public static unsafe partial class Delegates
        {
            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_clip_control", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_clip_control", Api = "gles2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glClipControl(int origin, int depth);

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_clip_control", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_clip_control", Api = "gles2", EntryPoint = "glClipControlEXT")]
            [ThreadStatic]
            public static glClipControl pglClipControl;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glCreateTransformFeedbacks(int n, uint* ids);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glCreateTransformFeedbacks pglCreateTransformFeedbacks;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glTransformFeedbackBufferBase(uint xfb, uint index, uint buffer);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glTransformFeedbackBufferBase pglTransformFeedbackBufferBase;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glTransformFeedbackBufferRange(uint xfb, uint index, uint buffer, IntPtr offset, uint size);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glTransformFeedbackBufferRange pglTransformFeedbackBufferRange;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetTransformFeedbackiv(uint xfb, int pname, int* param);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glGetTransformFeedbackiv pglGetTransformFeedbackiv;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetTransformFeedbacki_v(uint xfb, int pname, uint index, int* param);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glGetTransformFeedbacki_v pglGetTransformFeedbacki_v;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetTransformFeedbacki64_v(uint xfb, int pname, uint index, long* param);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glGetTransformFeedbacki64_v pglGetTransformFeedbacki64_v;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glCreateBuffers(int n, uint* buffers);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glCreateBuffers pglCreateBuffers;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glNamedBufferStorage(uint buffer, uint size, IntPtr data, uint flags);

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_direct_state_access", Api = "gl|glcore", EntryPoint = "glNamedBufferStorageEXT")]
            [ThreadStatic]
            public static glNamedBufferStorage pglNamedBufferStorage;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glNamedBufferData(uint buffer, uint size, IntPtr data, int usage);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glNamedBufferData pglNamedBufferData;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glNamedBufferSubData(uint buffer, IntPtr offset, uint size, IntPtr data);

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_direct_state_access", Api = "gl|glcore", EntryPoint = "glNamedBufferSubDataEXT")]
            [ThreadStatic]
            public static glNamedBufferSubData pglNamedBufferSubData;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glCopyNamedBufferSubData(uint readBuffer, uint writeBuffer, IntPtr readOffset, IntPtr writeOffset, uint size);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glCopyNamedBufferSubData pglCopyNamedBufferSubData;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glClearNamedBufferData(uint buffer, int internalformat, int format, int type, IntPtr data);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glClearNamedBufferData pglClearNamedBufferData;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glClearNamedBufferSubData(uint buffer, int internalformat, IntPtr offset, uint size, int format, int type, IntPtr data);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glClearNamedBufferSubData pglClearNamedBufferSubData;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate IntPtr glMapNamedBuffer(uint buffer, int access);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glMapNamedBuffer pglMapNamedBuffer;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate IntPtr glMapNamedBufferRange(uint buffer, IntPtr offset, uint length, uint access);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glMapNamedBufferRange pglMapNamedBufferRange;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            [return: MarshalAs(UnmanagedType.I1)]
            public delegate bool glUnmapNamedBuffer(uint buffer);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glUnmapNamedBuffer pglUnmapNamedBuffer;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glFlushMappedNamedBufferRange(uint buffer, IntPtr offset, uint length);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glFlushMappedNamedBufferRange pglFlushMappedNamedBufferRange;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetNamedBufferParameteriv(uint buffer, int pname, int* @params);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glGetNamedBufferParameteriv pglGetNamedBufferParameteriv;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetNamedBufferParameteri64v(uint buffer, int pname, long* @params);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glGetNamedBufferParameteri64v pglGetNamedBufferParameteri64v;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetNamedBufferPointerv(uint buffer, int pname, IntPtr* @params);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glGetNamedBufferPointerv pglGetNamedBufferPointerv;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetNamedBufferSubData(uint buffer, IntPtr offset, uint size, IntPtr data);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glGetNamedBufferSubData pglGetNamedBufferSubData;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glCreateFramebuffers(int n, uint* framebuffers);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glCreateFramebuffers pglCreateFramebuffers;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glNamedFramebufferRenderbuffer(uint framebuffer, int attachment, int renderbuffertarget, uint renderbuffer);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glNamedFramebufferRenderbuffer pglNamedFramebufferRenderbuffer;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glNamedFramebufferParameteri(uint framebuffer, int pname, int param);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glNamedFramebufferParameteri pglNamedFramebufferParameteri;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glNamedFramebufferTexture(uint framebuffer, int attachment, uint texture, int level);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glNamedFramebufferTexture pglNamedFramebufferTexture;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glNamedFramebufferTextureLayer(uint framebuffer, int attachment, uint texture, int level, int layer);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glNamedFramebufferTextureLayer pglNamedFramebufferTextureLayer;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glNamedFramebufferDrawBuffer(uint framebuffer, int buf);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glNamedFramebufferDrawBuffer pglNamedFramebufferDrawBuffer;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glNamedFramebufferDrawBuffers(uint framebuffer, int n, int* bufs);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glNamedFramebufferDrawBuffers pglNamedFramebufferDrawBuffers;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glNamedFramebufferReadBuffer(uint framebuffer, int src);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glNamedFramebufferReadBuffer pglNamedFramebufferReadBuffer;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glInvalidateNamedFramebufferData(uint framebuffer, int numAttachments, int* attachments);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glInvalidateNamedFramebufferData pglInvalidateNamedFramebufferData;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glInvalidateNamedFramebufferSubData(uint framebuffer, int numAttachments, int* attachments, int x, int y, int width, int height);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glInvalidateNamedFramebufferSubData pglInvalidateNamedFramebufferSubData;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glClearNamedFramebufferiv(uint framebuffer, int buffer, int drawbuffer, int* value);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glClearNamedFramebufferiv pglClearNamedFramebufferiv;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glClearNamedFramebufferuiv(uint framebuffer, int buffer, int drawbuffer, uint* value);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glClearNamedFramebufferuiv pglClearNamedFramebufferuiv;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glClearNamedFramebufferfv(uint framebuffer, int buffer, int drawbuffer, float* value);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glClearNamedFramebufferfv pglClearNamedFramebufferfv;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glClearNamedFramebufferfi(uint framebuffer, int buffer, int drawbuffer, float depth, int stencil);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glClearNamedFramebufferfi pglClearNamedFramebufferfi;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glBlitNamedFramebuffer(uint readFramebuffer, uint drawFramebuffer, int srcX0, int srcY0, int srcX1, int srcY1, int dstX0, int dstY0, int dstX1, int dstY1, uint mask,
                int filter);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glBlitNamedFramebuffer pglBlitNamedFramebuffer;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate int glCheckNamedFramebufferStatus(uint framebuffer, int target);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glCheckNamedFramebufferStatus pglCheckNamedFramebufferStatus;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetNamedFramebufferParameteriv(uint framebuffer, int pname, int* param);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glGetNamedFramebufferParameteriv pglGetNamedFramebufferParameteriv;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetNamedFramebufferAttachmentParameteriv(uint framebuffer, int attachment, int pname, int* @params);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glGetNamedFramebufferAttachmentParameteriv pglGetNamedFramebufferAttachmentParameteriv;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glCreateRenderbuffers(int n, uint* renderbuffers);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glCreateRenderbuffers pglCreateRenderbuffers;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glNamedRenderbufferStorage(uint renderbuffer, int internalformat, int width, int height);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glNamedRenderbufferStorage pglNamedRenderbufferStorage;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glNamedRenderbufferStorageMultisample(uint renderbuffer, int samples, int internalformat, int width, int height);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glNamedRenderbufferStorageMultisample pglNamedRenderbufferStorageMultisample;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetNamedRenderbufferParameteriv(uint renderbuffer, int pname, int* @params);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glGetNamedRenderbufferParameteriv pglGetNamedRenderbufferParameteriv;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glCreateTextures(int target, int n, uint* textures);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glCreateTextures pglCreateTextures;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glTextureBuffer(uint texture, int internalformat, uint buffer);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glTextureBuffer pglTextureBuffer;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glTextureBufferRange(uint texture, int internalformat, uint buffer, IntPtr offset, uint size);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glTextureBufferRange pglTextureBufferRange;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glTextureStorage1D(uint texture, int levels, int internalformat, int width);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glTextureStorage1D pglTextureStorage1D;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glTextureStorage2D(uint texture, int levels, int internalformat, int width, int height);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glTextureStorage2D pglTextureStorage2D;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glTextureStorage3D(uint texture, int levels, int internalformat, int width, int height, int depth);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glTextureStorage3D pglTextureStorage3D;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glTextureStorage2DMultisample(uint texture, int samples, int internalformat, int width, int height, [MarshalAs(UnmanagedType.I1)] bool fixedsamplelocations);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glTextureStorage2DMultisample pglTextureStorage2DMultisample;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glTextureStorage3DMultisample(uint texture, int samples, int internalformat, int width, int height, int depth,
                [MarshalAs(UnmanagedType.I1)] bool fixedsamplelocations);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glTextureStorage3DMultisample pglTextureStorage3DMultisample;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glTextureSubImage1D(uint texture, int level, int xoffset, int width, int format, int type, IntPtr pixels);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glTextureSubImage1D pglTextureSubImage1D;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glTextureSubImage2D(uint texture, int level, int xoffset, int yoffset, int width, int height, int format, int type, IntPtr pixels);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glTextureSubImage2D pglTextureSubImage2D;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glTextureSubImage3D(uint texture, int level, int xoffset, int yoffset, int zoffset, int width, int height, int depth, int format, int type, IntPtr pixels);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glTextureSubImage3D pglTextureSubImage3D;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glCompressedTextureSubImage1D(uint texture, int level, int xoffset, int width, int format, int imageSize, IntPtr data);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glCompressedTextureSubImage1D pglCompressedTextureSubImage1D;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glCompressedTextureSubImage2D(uint texture, int level, int xoffset, int yoffset, int width, int height, int format, int imageSize, IntPtr data);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glCompressedTextureSubImage2D pglCompressedTextureSubImage2D;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glCompressedTextureSubImage3D(uint texture, int level, int xoffset, int yoffset, int zoffset, int width, int height, int depth, int format, int imageSize,
                IntPtr data);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glCompressedTextureSubImage3D pglCompressedTextureSubImage3D;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glCopyTextureSubImage1D(uint texture, int level, int xoffset, int x, int y, int width);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glCopyTextureSubImage1D pglCopyTextureSubImage1D;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glCopyTextureSubImage2D(uint texture, int level, int xoffset, int yoffset, int x, int y, int width, int height);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glCopyTextureSubImage2D pglCopyTextureSubImage2D;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glCopyTextureSubImage3D(uint texture, int level, int xoffset, int yoffset, int zoffset, int x, int y, int width, int height);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glCopyTextureSubImage3D pglCopyTextureSubImage3D;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glTextureParameterf(uint texture, int pname, float param);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glTextureParameterf pglTextureParameterf;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glTextureParameterfv(uint texture, int pname, float* param);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glTextureParameterfv pglTextureParameterfv;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glTextureParameteri(uint texture, int pname, int param);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glTextureParameteri pglTextureParameteri;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glTextureParameterIiv(uint texture, int pname, int* @params);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glTextureParameterIiv pglTextureParameterIiv;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glTextureParameterIuiv(uint texture, int pname, uint* @params);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glTextureParameterIuiv pglTextureParameterIuiv;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glTextureParameteriv(uint texture, int pname, int* param);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glTextureParameteriv pglTextureParameteriv;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGenerateTextureMipmap(uint texture);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glGenerateTextureMipmap pglGenerateTextureMipmap;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glBindTextureUnit(uint unit, uint texture);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glBindTextureUnit pglBindTextureUnit;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetTextureImage(uint texture, int level, int format, int type, int bufSize, IntPtr pixels);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glGetTextureImage pglGetTextureImage;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetCompressedTextureImage(uint texture, int level, int bufSize, IntPtr pixels);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glGetCompressedTextureImage pglGetCompressedTextureImage;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetTextureLevelParameterfv(uint texture, int level, int pname, float* @params);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glGetTextureLevelParameterfv pglGetTextureLevelParameterfv;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetTextureLevelParameteriv(uint texture, int level, int pname, int* @params);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glGetTextureLevelParameteriv pglGetTextureLevelParameteriv;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetTextureParameterfv(uint texture, int pname, float* @params);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glGetTextureParameterfv pglGetTextureParameterfv;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetTextureParameterIiv(uint texture, int pname, int* @params);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glGetTextureParameterIiv pglGetTextureParameterIiv;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetTextureParameterIuiv(uint texture, int pname, uint* @params);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glGetTextureParameterIuiv pglGetTextureParameterIuiv;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetTextureParameteriv(uint texture, int pname, int* @params);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glGetTextureParameteriv pglGetTextureParameteriv;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glCreateVertexArrays(int n, uint* arrays);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glCreateVertexArrays pglCreateVertexArrays;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glDisableVertexArrayAttrib(uint vaobj, uint index);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glDisableVertexArrayAttrib pglDisableVertexArrayAttrib;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glEnableVertexArrayAttrib(uint vaobj, uint index);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glEnableVertexArrayAttrib pglEnableVertexArrayAttrib;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexArrayElementBuffer(uint vaobj, uint buffer);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glVertexArrayElementBuffer pglVertexArrayElementBuffer;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexArrayVertexBuffer(uint vaobj, uint bindingindex, uint buffer, IntPtr offset, int stride);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glVertexArrayVertexBuffer pglVertexArrayVertexBuffer;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexArrayVertexBuffers(uint vaobj, uint first, int count, uint* buffers, IntPtr* offsets, int* strides);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glVertexArrayVertexBuffers pglVertexArrayVertexBuffers;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexArrayAttribBinding(uint vaobj, uint attribindex, uint bindingindex);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glVertexArrayAttribBinding pglVertexArrayAttribBinding;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexArrayAttribFormat(uint vaobj, uint attribindex, int size, int type, [MarshalAs(UnmanagedType.I1)] bool normalized, uint relativeoffset);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glVertexArrayAttribFormat pglVertexArrayAttribFormat;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexArrayAttribIFormat(uint vaobj, uint attribindex, int size, int type, uint relativeoffset);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glVertexArrayAttribIFormat pglVertexArrayAttribIFormat;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexArrayAttribLFormat(uint vaobj, uint attribindex, int size, int type, uint relativeoffset);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glVertexArrayAttribLFormat pglVertexArrayAttribLFormat;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glVertexArrayBindingDivisor(uint vaobj, uint bindingindex, uint divisor);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glVertexArrayBindingDivisor pglVertexArrayBindingDivisor;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetVertexArrayiv(uint vaobj, int pname, int* param);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glGetVertexArrayiv pglGetVertexArrayiv;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetVertexArrayIndexediv(uint vaobj, uint index, int pname, int* param);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glGetVertexArrayIndexediv pglGetVertexArrayIndexediv;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetVertexArrayIndexed64iv(uint vaobj, uint index, int pname, long* param);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glGetVertexArrayIndexed64iv pglGetVertexArrayIndexed64iv;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glCreateSamplers(int n, uint* samplers);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glCreateSamplers pglCreateSamplers;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glCreateProgramPipelines(int n, uint* pipelines);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glCreateProgramPipelines pglCreateProgramPipelines;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glCreateQueries(int target, int n, uint* ids);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glCreateQueries pglCreateQueries;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetQueryBufferObjecti64v(uint id, uint buffer, int pname, IntPtr offset);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glGetQueryBufferObjecti64v pglGetQueryBufferObjecti64v;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetQueryBufferObjectiv(uint id, uint buffer, int pname, IntPtr offset);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glGetQueryBufferObjectiv pglGetQueryBufferObjectiv;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetQueryBufferObjectui64v(uint id, uint buffer, int pname, IntPtr offset);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glGetQueryBufferObjectui64v pglGetQueryBufferObjectui64v;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetQueryBufferObjectuiv(uint id, uint buffer, int pname, IntPtr offset);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [ThreadStatic]
            public static glGetQueryBufferObjectuiv pglGetQueryBufferObjectuiv;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_ES3_1_compatibility", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glMemoryBarrierByRegion(uint barriers);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_ES3_1_compatibility", Api = "gl|glcore")] [ThreadStatic]
            public static glMemoryBarrierByRegion pglMemoryBarrierByRegion;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_get_texture_sub_image", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetTextureSubImage(uint texture, int level, int xoffset, int yoffset, int zoffset, int width, int height, int depth, int format, int type, int bufSize,
                IntPtr pixels);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_get_texture_sub_image", Api = "gl|glcore")] [ThreadStatic]
            public static glGetTextureSubImage pglGetTextureSubImage;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_get_texture_sub_image", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetCompressedTextureSubImage(uint texture, int level, int xoffset, int yoffset, int zoffset, int width, int height, int depth, int bufSize, IntPtr pixels);

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_get_texture_sub_image", Api = "gl|glcore")] [ThreadStatic]
            public static glGetCompressedTextureSubImage pglGetCompressedTextureSubImage;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_EXT_robustness", Api = "gles1|gles2")]
            [RequiredByFeature("GL_KHR_robustness")]
            [RequiredByFeature("GL_KHR_robustness", Api = "gles2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate int glGetGraphicsResetStatus();

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_EXT_robustness", Api = "gles1|gles2", EntryPoint = "glGetGraphicsResetStatusEXT")]
            [RequiredByFeature("GL_KHR_robustness")]
            [RequiredByFeature("GL_KHR_robustness", Api = "gles2", EntryPoint = "glGetGraphicsResetStatusKHR")]
            [ThreadStatic]
            public static glGetGraphicsResetStatus pglGetGraphicsResetStatus;

            [RequiredByFeature("GL_VERSION_4_5")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetnCompressedTexImage(int target, int lod, int bufSize, IntPtr pixels);

            [RequiredByFeature("GL_VERSION_4_5")] [ThreadStatic]
            public static glGetnCompressedTexImage pglGetnCompressedTexImage;

            [RequiredByFeature("GL_VERSION_4_5")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetnTexImage(int target, int level, int format, int type, int bufSize, IntPtr pixels);

            [RequiredByFeature("GL_VERSION_4_5")] [ThreadStatic]
            public static glGetnTexImage pglGetnTexImage;

            [RequiredByFeature("GL_VERSION_4_5")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetnUniformdv(uint program, int location, int bufSize, double* @params);

            [RequiredByFeature("GL_VERSION_4_5")] [ThreadStatic]
            public static glGetnUniformdv pglGetnUniformdv;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_EXT_robustness", Api = "gles1|gles2")]
            [RequiredByFeature("GL_KHR_robustness")]
            [RequiredByFeature("GL_KHR_robustness", Api = "gles2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetnUniformfv(uint program, int location, int bufSize, float* @params);

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_EXT_robustness", Api = "gles1|gles2", EntryPoint = "glGetnUniformfvEXT")]
            [RequiredByFeature("GL_KHR_robustness")]
            [RequiredByFeature("GL_KHR_robustness", Api = "gles2", EntryPoint = "glGetnUniformfvKHR")]
            [ThreadStatic]
            public static glGetnUniformfv pglGetnUniformfv;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_EXT_robustness", Api = "gles1|gles2")]
            [RequiredByFeature("GL_KHR_robustness")]
            [RequiredByFeature("GL_KHR_robustness", Api = "gles2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetnUniformiv(uint program, int location, int bufSize, int* @params);

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_EXT_robustness", Api = "gles1|gles2", EntryPoint = "glGetnUniformivEXT")]
            [RequiredByFeature("GL_KHR_robustness")]
            [RequiredByFeature("GL_KHR_robustness", Api = "gles2", EntryPoint = "glGetnUniformivKHR")]
            [ThreadStatic]
            public static glGetnUniformiv pglGetnUniformiv;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_KHR_robustness")]
            [RequiredByFeature("GL_KHR_robustness", Api = "gles2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetnUniformuiv(uint program, int location, int bufSize, uint* @params);

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_KHR_robustness")]
            [RequiredByFeature("GL_KHR_robustness", Api = "gles2", EntryPoint = "glGetnUniformuivKHR")]
            [ThreadStatic]
            public static glGetnUniformuiv pglGetnUniformuiv;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_robustness", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_robustness", Api = "gles1|gles2")]
            [RequiredByFeature("GL_KHR_robustness")]
            [RequiredByFeature("GL_KHR_robustness", Api = "gles2")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glReadnPixels(int x, int y, int width, int height, int format, int type, int bufSize, IntPtr data);

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_robustness", Api = "gl|glcore", EntryPoint = "glReadnPixelsARB")]
            [RequiredByFeature("GL_EXT_robustness", Api = "gles1|gles2", EntryPoint = "glReadnPixelsEXT")]
            [RequiredByFeature("GL_KHR_robustness")]
            [RequiredByFeature("GL_KHR_robustness", Api = "gles2", EntryPoint = "glReadnPixelsKHR")]
            [ThreadStatic]
            public static glReadnPixels pglReadnPixels;

            [RequiredByFeature("GL_VERSION_4_5", Profile = "compatibility")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetnMapdv(int target, int query, int bufSize, double* v);

            [RequiredByFeature("GL_VERSION_4_5", Profile = "compatibility")] [ThreadStatic]
            public static glGetnMapdv pglGetnMapdv;

            [RequiredByFeature("GL_VERSION_4_5", Profile = "compatibility")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetnMapfv(int target, int query, int bufSize, float* v);

            [RequiredByFeature("GL_VERSION_4_5", Profile = "compatibility")] [ThreadStatic]
            public static glGetnMapfv pglGetnMapfv;

            [RequiredByFeature("GL_VERSION_4_5", Profile = "compatibility")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetnMapiv(int target, int query, int bufSize, int* v);

            [RequiredByFeature("GL_VERSION_4_5", Profile = "compatibility")] [ThreadStatic]
            public static glGetnMapiv pglGetnMapiv;

            [RequiredByFeature("GL_VERSION_4_5", Profile = "compatibility")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetnPixelMapfv(int map, int bufSize, float* values);

            [RequiredByFeature("GL_VERSION_4_5", Profile = "compatibility")] [ThreadStatic]
            public static glGetnPixelMapfv pglGetnPixelMapfv;

            [RequiredByFeature("GL_VERSION_4_5", Profile = "compatibility")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetnPixelMapuiv(int map, int bufSize, uint* values);

            [RequiredByFeature("GL_VERSION_4_5", Profile = "compatibility")] [ThreadStatic]
            public static glGetnPixelMapuiv pglGetnPixelMapuiv;

            [RequiredByFeature("GL_VERSION_4_5", Profile = "compatibility")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetnPixelMapusv(int map, int bufSize, ushort* values);

            [RequiredByFeature("GL_VERSION_4_5", Profile = "compatibility")] [ThreadStatic]
            public static glGetnPixelMapusv pglGetnPixelMapusv;

            [RequiredByFeature("GL_VERSION_4_5", Profile = "compatibility")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetnPolygonStipple(int bufSize, byte* pattern);

            [RequiredByFeature("GL_VERSION_4_5", Profile = "compatibility")] [ThreadStatic]
            public static glGetnPolygonStipple pglGetnPolygonStipple;

            [RequiredByFeature("GL_VERSION_4_5", Profile = "compatibility")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetnColorTable(int target, int format, int type, int bufSize, IntPtr table);

            [RequiredByFeature("GL_VERSION_4_5", Profile = "compatibility")] [ThreadStatic]
            public static glGetnColorTable pglGetnColorTable;

            [RequiredByFeature("GL_VERSION_4_5", Profile = "compatibility")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetnConvolutionFilter(int target, int format, int type, int bufSize, IntPtr image);

            [RequiredByFeature("GL_VERSION_4_5", Profile = "compatibility")] [ThreadStatic]
            public static glGetnConvolutionFilter pglGetnConvolutionFilter;

            [RequiredByFeature("GL_VERSION_4_5", Profile = "compatibility")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetnSeparableFilter(int target, int format, int type, int rowBufSize, IntPtr row, int columnBufSize, IntPtr column, IntPtr span);

            [RequiredByFeature("GL_VERSION_4_5", Profile = "compatibility")] [ThreadStatic]
            public static glGetnSeparableFilter pglGetnSeparableFilter;

            [RequiredByFeature("GL_VERSION_4_5", Profile = "compatibility")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetnHistogram(int target, [MarshalAs(UnmanagedType.I1)] bool reset, int format, int type, int bufSize, IntPtr values);

            [RequiredByFeature("GL_VERSION_4_5", Profile = "compatibility")] [ThreadStatic]
            public static glGetnHistogram pglGetnHistogram;

            [RequiredByFeature("GL_VERSION_4_5", Profile = "compatibility")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glGetnMinmax(int target, [MarshalAs(UnmanagedType.I1)] bool reset, int format, int type, int bufSize, IntPtr values);

            [RequiredByFeature("GL_VERSION_4_5", Profile = "compatibility")] [ThreadStatic]
            public static glGetnMinmax pglGetnMinmax;

            [RequiredByFeature("GL_VERSION_4_5")]
            [RequiredByFeature("GL_ARB_texture_barrier", Api = "gl|glcore")]
            [SuppressUnmanagedCodeSecurity]
            public delegate void glTextureBarrier();

            [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_texture_barrier", Api = "gl|glcore")] [ThreadStatic]
            public static glTextureBarrier pglTextureBarrier;
        }
    }
}