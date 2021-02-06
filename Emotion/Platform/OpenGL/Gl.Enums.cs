#pragma warning disable 618

#region Using

using System;
using Khronos;

#endregion

namespace OpenGL
{
    /// <summary>
    /// Strongly typed enumeration AlphaFunction.
    /// </summary>
    public enum AlphaFunction
    {
        /// <summary>
        /// Strongly typed for value GL_ALWAYS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Always = Gl.ALWAYS,

        /// <summary>
        /// Strongly typed for value GL_EQUAL.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
        Equal = Gl.EQUAL,

        /// <summary>
        /// Strongly typed for value GL_GEQUAL.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Gequal = Gl.GEQUAL,

        /// <summary>
        /// Strongly typed for value GL_GREATER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Greater = Gl.GREATER,

        /// <summary>
        /// Strongly typed for value GL_LEQUAL.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Lequal = Gl.LEQUAL,

        /// <summary>
        /// Strongly typed for value GL_LESS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Less = Gl.LESS,

        /// <summary>
        /// Strongly typed for value GL_NEVER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Never = Gl.NEVER,

        /// <summary>
        /// Strongly typed for value GL_NOTEQUAL.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Notequal = Gl.NOTEQUAL
    }

    /// <summary>
    /// Strongly typed enumeration AtomicCounterBufferPName.
    /// </summary>
    public enum AtomicCounterBufferPName
    {
        /// <summary>
        /// Strongly typed for value GL_ATOMIC_COUNTER_BUFFER_BINDING.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
        AtomicCounterBufferBinding = Gl.ATOMIC_COUNTER_BUFFER_BINDING,

        /// <summary>
        /// Strongly typed for value GL_ATOMIC_COUNTER_BUFFER_DATA_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")] [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
        AtomicCounterBufferDataSize = Gl.ATOMIC_COUNTER_BUFFER_DATA_SIZE,

        /// <summary>
        /// Strongly typed for value GL_ATOMIC_COUNTER_BUFFER_ACTIVE_ATOMIC_COUNTERS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")] [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
        AtomicCounterBufferActiveAtomicCounters = Gl.ATOMIC_COUNTER_BUFFER_ACTIVE_ATOMIC_COUNTERS,

        /// <summary>
        /// Strongly typed for value GL_ATOMIC_COUNTER_BUFFER_ACTIVE_ATOMIC_COUNTER_INDICES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")] [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
        AtomicCounterBufferActiveAtomicCounterIndices = Gl.ATOMIC_COUNTER_BUFFER_ACTIVE_ATOMIC_COUNTER_INDICES,

        /// <summary>
        /// Strongly typed for value GL_ATOMIC_COUNTER_BUFFER_REFERENCED_BY_VERTEX_SHADER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")] [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
        AtomicCounterBufferReferencedByVertexShader = Gl.ATOMIC_COUNTER_BUFFER_REFERENCED_BY_VERTEX_SHADER,

        /// <summary>
        /// Strongly typed for value GL_ATOMIC_COUNTER_BUFFER_REFERENCED_BY_TESS_CONTROL_SHADER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")] [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
        AtomicCounterBufferReferencedByTessControlShader = Gl.ATOMIC_COUNTER_BUFFER_REFERENCED_BY_TESS_CONTROL_SHADER,

        /// <summary>
        /// Strongly typed for value GL_ATOMIC_COUNTER_BUFFER_REFERENCED_BY_TESS_EVALUATION_SHADER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")] [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
        AtomicCounterBufferReferencedByTessEvaluationShader = Gl.ATOMIC_COUNTER_BUFFER_REFERENCED_BY_TESS_EVALUATION_SHADER,

        /// <summary>
        /// Strongly typed for value GL_ATOMIC_COUNTER_BUFFER_REFERENCED_BY_GEOMETRY_SHADER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")] [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
        AtomicCounterBufferReferencedByGeometryShader = Gl.ATOMIC_COUNTER_BUFFER_REFERENCED_BY_GEOMETRY_SHADER,

        /// <summary>
        /// Strongly typed for value GL_ATOMIC_COUNTER_BUFFER_REFERENCED_BY_FRAGMENT_SHADER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")] [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
        AtomicCounterBufferReferencedByFragmentShader = Gl.ATOMIC_COUNTER_BUFFER_REFERENCED_BY_FRAGMENT_SHADER,

        /// <summary>
        /// Strongly typed for value GL_ATOMIC_COUNTER_BUFFER_REFERENCED_BY_COMPUTE_SHADER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_compute_shader", Api = "gl|glcore")]
        AtomicCounterBufferReferencedByComputeShader = Gl.ATOMIC_COUNTER_BUFFER_REFERENCED_BY_COMPUTE_SHADER
    }

    /// <summary>
    /// Strongly typed enumeration AttributeType.
    /// </summary>
    public enum AttributeType
    {
        /// <summary>
        /// Strongly typed for value GL_FLOAT_VEC2, GL_FLOAT_VEC2_ARB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects")] [RequiredByFeature("GL_ARB_vertex_shader")]
        FloatVec2 = Gl.FLOAT_VEC2,

        /// <summary>
        /// Strongly typed for value GL_FLOAT_VEC3, GL_FLOAT_VEC3_ARB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects")] [RequiredByFeature("GL_ARB_vertex_shader")]
        FloatVec3 = Gl.FLOAT_VEC3,

        /// <summary>
        /// Strongly typed for value GL_FLOAT_VEC4, GL_FLOAT_VEC4_ARB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects")] [RequiredByFeature("GL_ARB_vertex_shader")]
        FloatVec4 = Gl.FLOAT_VEC4,

        /// <summary>
        /// Strongly typed for value GL_INT_VEC2, GL_INT_VEC2_ARB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects")]
        IntVec2 = Gl.INT_VEC2,

        /// <summary>
        /// Strongly typed for value GL_INT_VEC3, GL_INT_VEC3_ARB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects")]
        IntVec3 = Gl.INT_VEC3,

        /// <summary>
        /// Strongly typed for value GL_INT_VEC4, GL_INT_VEC4_ARB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects")]
        IntVec4 = Gl.INT_VEC4,

        /// <summary>
        /// Strongly typed for value GL_BOOL, GL_BOOL_ARB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects")]
        Bool = Gl.BOOL,

        /// <summary>
        /// Strongly typed for value GL_BOOL_VEC2, GL_BOOL_VEC2_ARB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects")]
        BoolVec2 = Gl.BOOL_VEC2,

        /// <summary>
        /// Strongly typed for value GL_BOOL_VEC3, GL_BOOL_VEC3_ARB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects")]
        BoolVec3 = Gl.BOOL_VEC3,

        /// <summary>
        /// Strongly typed for value GL_BOOL_VEC4, GL_BOOL_VEC4_ARB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects")]
        BoolVec4 = Gl.BOOL_VEC4,

        /// <summary>
        /// Strongly typed for value GL_FLOAT_MAT2, GL_FLOAT_MAT2_ARB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects")] [RequiredByFeature("GL_ARB_vertex_shader")]
        FloatMat2 = Gl.FLOAT_MAT2,

        /// <summary>
        /// Strongly typed for value GL_FLOAT_MAT3, GL_FLOAT_MAT3_ARB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects")] [RequiredByFeature("GL_ARB_vertex_shader")]
        FloatMat3 = Gl.FLOAT_MAT3,

        /// <summary>
        /// Strongly typed for value GL_FLOAT_MAT4, GL_FLOAT_MAT4_ARB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects")] [RequiredByFeature("GL_ARB_vertex_shader")]
        FloatMat4 = Gl.FLOAT_MAT4,

        /// <summary>
        /// Strongly typed for value GL_SAMPLER_1D, GL_SAMPLER_1D_ARB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ARB_shader_objects")]
        Sampler1d = Gl.SAMPLER_1D,

        /// <summary>
        /// Strongly typed for value GL_SAMPLER_2D, GL_SAMPLER_2D_ARB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        Sampler2d = Gl.SAMPLER_2D,

        /// <summary>
        /// Strongly typed for value GL_SAMPLER_3D, GL_SAMPLER_3D_ARB, GL_SAMPLER_3D_OES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        [RequiredByFeature("GL_OES_texture_3D", Api = "gles2")]
        Sampler3d = Gl.SAMPLER_3D,

        /// <summary>
        /// Strongly typed for value GL_SAMPLER_CUBE, GL_SAMPLER_CUBE_ARB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_objects")]
        SamplerCube = Gl.SAMPLER_CUBE,

        /// <summary>
        /// Strongly typed for value GL_SAMPLER_1D_SHADOW, GL_SAMPLER_1D_SHADOW_ARB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ARB_shader_objects")]
        Sampler1dShadow = Gl.SAMPLER_1D_SHADOW,

        /// <summary>
        /// Strongly typed for value GL_SAMPLER_2D_SHADOW, GL_SAMPLER_2D_SHADOW_ARB, GL_SAMPLER_2D_SHADOW_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_objects")]
        [RequiredByFeature("GL_EXT_shadow_samplers", Api = "gles2")]
        Sampler2dShadow = Gl.SAMPLER_2D_SHADOW,

        /// <summary>
        /// Strongly typed for value GL_SAMPLER_2D_RECT, GL_SAMPLER_2D_RECT_ARB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ARB_shader_objects")]
        Sampler2dRect = Gl.SAMPLER_2D_RECT,

        /// <summary>
        /// Strongly typed for value GL_SAMPLER_2D_RECT_SHADOW, GL_SAMPLER_2D_RECT_SHADOW_ARB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ARB_shader_objects")]
        Sampler2dRectShadow = Gl.SAMPLER_2D_RECT_SHADOW,

        /// <summary>
        /// Strongly typed for value GL_FLOAT_MAT2x3, GL_FLOAT_MAT2x3_NV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_NV_non_square_matrices", Api = "gles2")]
        FloatMat2X3 = Gl.FLOAT_MAT2x3,

        /// <summary>
        /// Strongly typed for value GL_FLOAT_MAT2x4, GL_FLOAT_MAT2x4_NV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_NV_non_square_matrices", Api = "gles2")]
        FloatMat2X4 = Gl.FLOAT_MAT2x4,

        /// <summary>
        /// Strongly typed for value GL_FLOAT_MAT3x2, GL_FLOAT_MAT3x2_NV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_NV_non_square_matrices", Api = "gles2")]
        FloatMat3X2 = Gl.FLOAT_MAT3x2,

        /// <summary>
        /// Strongly typed for value GL_FLOAT_MAT3x4, GL_FLOAT_MAT3x4_NV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_NV_non_square_matrices", Api = "gles2")]
        FloatMat3X4 = Gl.FLOAT_MAT3x4,

        /// <summary>
        /// Strongly typed for value GL_FLOAT_MAT4x2, GL_FLOAT_MAT4x2_NV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_NV_non_square_matrices", Api = "gles2")]
        FloatMat4X2 = Gl.FLOAT_MAT4x2,

        /// <summary>
        /// Strongly typed for value GL_FLOAT_MAT4x3, GL_FLOAT_MAT4x3_NV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_NV_non_square_matrices", Api = "gles2")]
        FloatMat4X3 = Gl.FLOAT_MAT4x3
    }

    /// <summary>
    /// Strongly typed enumeration BindTransformFeedbackTarget.
    /// </summary>
    public enum BindTransformFeedbackTarget
    {
        /// <summary>
        /// Strongly typed for value GL_TRANSFORM_FEEDBACK.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_transform_feedback2", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_debug_label", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_NV_transform_feedback2")]
        TransformFeedback = Gl.TRANSFORM_FEEDBACK
    }

    /// <summary>
    /// Strongly typed enumeration BlendEquationMode.
    /// </summary>
    public enum BlendEquationMode
    {
        /// <summary>
        /// Strongly typed for value GL_FUNC_ADD, GL_FUNC_ADD_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_imaging", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_EXT_blend_minmax", Api = "gl|gles1|gles2")]
        [RequiredByFeature("GL_OES_blend_subtract", Api = "gles1")]
        FuncAdd = Gl.FUNC_ADD,

        /// <summary>
        /// Strongly typed for value GL_FUNC_REVERSE_SUBTRACT, GL_FUNC_REVERSE_SUBTRACT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_imaging", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_EXT_blend_subtract")]
        [RequiredByFeature("GL_OES_blend_subtract", Api = "gles1")]
        FuncReverseSubtract = Gl.FUNC_REVERSE_SUBTRACT,

        /// <summary>
        /// Strongly typed for value GL_FUNC_SUBTRACT, GL_FUNC_SUBTRACT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_imaging", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_EXT_blend_subtract")]
        [RequiredByFeature("GL_OES_blend_subtract", Api = "gles1")]
        FuncSubtract = Gl.FUNC_SUBTRACT,

        /// <summary>
        /// Strongly typed for value GL_MAX, GL_MAX_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_imaging", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_EXT_blend_minmax", Api = "gl|gles1|gles2")]
        Max = Gl.MAX,

        /// <summary>
        /// Strongly typed for value GL_MIN, GL_MIN_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_imaging", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_EXT_blend_minmax", Api = "gl|gles1|gles2")]
        Min = Gl.MIN
    }

    /// <summary>
    /// Strongly typed enumeration BlendingFactor.
    /// </summary>
    public enum BlendingFactor
    {
        /// <summary>
        /// Strongly typed for value GL_ZERO.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_NV_register_combiners")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        Zero = Gl.ZERO,

        /// <summary>
        /// Strongly typed for value GL_ONE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        One = Gl.ONE,

        /// <summary>
        /// Strongly typed for value GL_SRC_COLOR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        SrcColor = Gl.SRC_COLOR,

        /// <summary>
        /// Strongly typed for value GL_ONE_MINUS_SRC_COLOR, GL_ONE_MINUS_SRC_COLOR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        OneMinusSrcColor = Gl.ONE_MINUS_SRC_COLOR,

        /// <summary>
        /// Strongly typed for value GL_DST_COLOR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        DstColor = Gl.DST_COLOR,

        /// <summary>
        /// Strongly typed for value GL_ONE_MINUS_DST_COLOR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        OneMinusDstColor = Gl.ONE_MINUS_DST_COLOR,

        /// <summary>
        /// Strongly typed for value GL_SRC_ALPHA.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        SrcAlpha = Gl.SRC_ALPHA,

        /// <summary>
        /// Strongly typed for value GL_ONE_MINUS_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        OneMinusSrcAlpha = Gl.ONE_MINUS_SRC_ALPHA,

        /// <summary>
        /// Strongly typed for value GL_DST_ALPHA.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        DstAlpha = Gl.DST_ALPHA,

        /// <summary>
        /// Strongly typed for value GL_ONE_MINUS_DST_ALPHA.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        OneMinusDstAlpha = Gl.ONE_MINUS_DST_ALPHA,

        /// <summary>
        /// Strongly typed for value GL_CONSTANT_COLOR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_imaging", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_EXT_blend_color")]
        ConstantColor = Gl.CONSTANT_COLOR,

        /// <summary>
        /// Strongly typed for value GL_ONE_MINUS_CONSTANT_COLOR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_imaging", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_EXT_blend_color")]
        OneMinusConstantColor = Gl.ONE_MINUS_CONSTANT_COLOR,

        /// <summary>
        /// Strongly typed for value GL_CONSTANT_ALPHA.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_imaging", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_EXT_blend_color")]
        ConstantAlpha = Gl.CONSTANT_ALPHA,

        /// <summary>
        /// Strongly typed for value GL_ONE_MINUS_CONSTANT_ALPHA.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_imaging", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_EXT_blend_color")]
        OneMinusConstantAlpha = Gl.ONE_MINUS_CONSTANT_ALPHA,

        /// <summary>
        /// Strongly typed for value GL_SRC_ALPHA_SATURATE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_EXT_blend_func_extended", Api = "gles2")]
        SrcAlphaSaturate = Gl.SRC_ALPHA_SATURATE,

        /// <summary>
        /// Strongly typed for value GL_SRC1_COLOR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_3")] [RequiredByFeature("GL_ARB_blend_func_extended", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_blend_func_extended", Api = "gles2")]
        Src1Color = Gl.SRC1_COLOR
    }

    /// <summary>
    /// Strongly typed enumeration BlitFramebufferFilter.
    /// </summary>
    public enum BlitFramebufferFilter
    {
        /// <summary>
        /// Strongly typed for value GL_NEAREST.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Nearest = Gl.NEAREST,

        /// <summary>
        /// Strongly typed for value GL_LINEAR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Linear = Gl.LINEAR
    }

    /// <summary>
    /// Strongly typed enumeration Boolean.
    /// </summary>
    public enum Boolean
    {
        /// <summary>
        /// Strongly typed for value GL_FALSE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        False = Gl.FALSE,

        /// <summary>
        /// Strongly typed for value GL_TRUE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        True = Gl.TRUE
    }

    /// <summary>
    /// Strongly typed enumeration Buffer.
    /// </summary>
    public enum Buffer
    {
        /// <summary>
        /// Strongly typed for value GL_COLOR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_discard_framebuffer", Api = "gles1|gles2")]
        Color = Gl.COLOR,

        /// <summary>
        /// Strongly typed for value GL_DEPTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_discard_framebuffer", Api = "gles1|gles2")]
        Depth = Gl.DEPTH,

        /// <summary>
        /// Strongly typed for value GL_STENCIL.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_discard_framebuffer", Api = "gles1|gles2")]
        Stencil = Gl.STENCIL
    }

    /// <summary>
    /// Strongly typed enumeration BufferAccess.
    /// </summary>
    public enum BufferAccess
    {
        /// <summary>
        /// Strongly typed for value GL_READ_ONLY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_5")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_vertex_buffer_object")]
        ReadOnly = Gl.READ_ONLY,

        /// <summary>
        /// Strongly typed for value GL_WRITE_ONLY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_5")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_NV_shader_buffer_store", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_vertex_buffer_object")]
        [RequiredByFeature("GL_OES_mapbuffer", Api = "gles1|gles2")]
        WriteOnly = Gl.WRITE_ONLY,

        /// <summary>
        /// Strongly typed for value GL_READ_WRITE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_5")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_NV_shader_buffer_store", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_vertex_buffer_object")]
        ReadWrite = Gl.READ_WRITE
    }

    /// <summary>
    /// Strongly typed enumeration BufferAccessMask.
    /// </summary>
    [Flags]
    public enum BufferAccessMask : uint
    {
        /// <summary>
        /// Strongly typed for value GL_MAP_COHERENT_BIT, GL_MAP_COHERENT_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_4")] [RequiredByFeature("GL_ARB_buffer_storage", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_buffer_storage", Api = "gles2")]
        MapCoherentBit = Gl.MAP_COHERENT_BIT,

        /// <summary>
        /// Strongly typed for value GL_MAP_FLUSH_EXPLICIT_BIT, GL_MAP_FLUSH_EXPLICIT_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_map_buffer_range", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_map_buffer_range", Api = "gles1|gles2")]
        MapFlushExplicitBit = Gl.MAP_FLUSH_EXPLICIT_BIT,

        /// <summary>
        /// Strongly typed for value GL_MAP_INVALIDATE_BUFFER_BIT, GL_MAP_INVALIDATE_BUFFER_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_map_buffer_range", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_map_buffer_range", Api = "gles1|gles2")]
        MapInvalidateBufferBit = Gl.MAP_INVALIDATE_BUFFER_BIT,

        /// <summary>
        /// Strongly typed for value GL_MAP_INVALIDATE_RANGE_BIT, GL_MAP_INVALIDATE_RANGE_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_map_buffer_range", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_map_buffer_range", Api = "gles1|gles2")]
        MapInvalidateRangeBit = Gl.MAP_INVALIDATE_RANGE_BIT,

        /// <summary>
        /// Strongly typed for value GL_MAP_PERSISTENT_BIT, GL_MAP_PERSISTENT_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_4")] [RequiredByFeature("GL_ARB_buffer_storage", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_buffer_storage", Api = "gles2")]
        MapPersistentBit = Gl.MAP_PERSISTENT_BIT,

        /// <summary>
        /// Strongly typed for value GL_MAP_READ_BIT, GL_MAP_READ_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_VERSION_4_4")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_buffer_storage", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_map_buffer_range", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_buffer_storage", Api = "gles2")]
        [RequiredByFeature("GL_EXT_map_buffer_range", Api = "gles1|gles2")]
        MapReadBit = Gl.MAP_READ_BIT,

        /// <summary>
        /// Strongly typed for value GL_MAP_UNSYNCHRONIZED_BIT, GL_MAP_UNSYNCHRONIZED_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_map_buffer_range", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_map_buffer_range", Api = "gles1|gles2")]
        MapUnsynchronizedBit = Gl.MAP_UNSYNCHRONIZED_BIT,

        /// <summary>
        /// Strongly typed for value GL_MAP_WRITE_BIT, GL_MAP_WRITE_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_VERSION_4_4")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_buffer_storage", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_map_buffer_range", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_buffer_storage", Api = "gles2")]
        [RequiredByFeature("GL_EXT_map_buffer_range", Api = "gles1|gles2")]
        MapWriteBit = Gl.MAP_WRITE_BIT
    }

    /// <summary>
    /// Strongly typed enumeration BufferTarget.
    /// </summary>
    public enum BufferTarget
    {
        /// <summary>
        /// Strongly typed for value GL_ARRAY_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_5")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_buffer_object")]
        ArrayBuffer = Gl.ARRAY_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_ATOMIC_COUNTER_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_ARB_program_interface_query", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
        AtomicCounterBuffer = Gl.ATOMIC_COUNTER_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_COPY_READ_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_copy_buffer", Api = "gl|glcore")]
        [RequiredByFeature("GL_NV_copy_buffer", Api = "gles2")]
        CopyReadBuffer = Gl.COPY_READ_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_COPY_WRITE_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_copy_buffer", Api = "gl|glcore")]
        [RequiredByFeature("GL_NV_copy_buffer", Api = "gles2")]
        CopyWriteBuffer = Gl.COPY_WRITE_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_DISPATCH_INDIRECT_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_compute_shader", Api = "gl|glcore")]
        DispatchIndirectBuffer = Gl.DISPATCH_INDIRECT_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_DRAW_INDIRECT_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_0")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_draw_indirect", Api = "gl|glcore")]
        DrawIndirectBuffer = Gl.DRAW_INDIRECT_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_ELEMENT_ARRAY_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_5")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_buffer_object")]
        ElementArrayBuffer = Gl.ELEMENT_ARRAY_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_PIXEL_PACK_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_1")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_pixel_buffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_pixel_buffer_object")]
        [RequiredByFeature("GL_NV_pixel_buffer_object", Api = "gles2")]
        PixelPackBuffer = Gl.PIXEL_PACK_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_PIXEL_UNPACK_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_1")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_pixel_buffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_pixel_buffer_object")]
        [RequiredByFeature("GL_NV_pixel_buffer_object", Api = "gles2")]
        PixelUnpackBuffer = Gl.PIXEL_UNPACK_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_QUERY_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_4")] [RequiredByFeature("GL_ARB_query_buffer_object", Api = "gl|glcore")] [RequiredByFeature("GL_AMD_query_buffer_object")]
        QueryBuffer = Gl.QUERY_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_SHADER_STORAGE_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_storage_buffer_object", Api = "gl|glcore")]
        ShaderStorageBuffer = Gl.SHADER_STORAGE_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_texture_buffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_texture_buffer", Api = "gles2")]
        [RequiredByFeature("GL_EXT_texture_buffer_object")]
        [RequiredByFeature("GL_OES_texture_buffer", Api = "gles2")]
        TextureBuffer = Gl.TEXTURE_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_TRANSFORM_FEEDBACK_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_VERSION_4_4")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_enhanced_layouts", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_transform_feedback")]
        [RequiredByFeature("GL_NV_transform_feedback")]
        TransformFeedbackBuffer = Gl.TRANSFORM_FEEDBACK_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_UNIFORM_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        UniformBuffer = Gl.UNIFORM_BUFFER
    }

    /// <summary>
    /// Strongly typed enumeration BufferTargetARB.
    /// </summary>
    public enum BufferTargetArb
    {
        /// <summary>
        /// Strongly typed for value GL_ARRAY_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_5")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_buffer_object")]
        ArrayBuffer = Gl.ARRAY_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_ATOMIC_COUNTER_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_ARB_program_interface_query", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
        AtomicCounterBuffer = Gl.ATOMIC_COUNTER_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_COPY_READ_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_copy_buffer", Api = "gl|glcore")]
        [RequiredByFeature("GL_NV_copy_buffer", Api = "gles2")]
        CopyReadBuffer = Gl.COPY_READ_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_COPY_WRITE_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_copy_buffer", Api = "gl|glcore")]
        [RequiredByFeature("GL_NV_copy_buffer", Api = "gles2")]
        CopyWriteBuffer = Gl.COPY_WRITE_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_DISPATCH_INDIRECT_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_compute_shader", Api = "gl|glcore")]
        DispatchIndirectBuffer = Gl.DISPATCH_INDIRECT_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_DRAW_INDIRECT_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_0")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_draw_indirect", Api = "gl|glcore")]
        DrawIndirectBuffer = Gl.DRAW_INDIRECT_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_ELEMENT_ARRAY_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_5")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_buffer_object")]
        ElementArrayBuffer = Gl.ELEMENT_ARRAY_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_PIXEL_PACK_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_1")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_pixel_buffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_pixel_buffer_object")]
        [RequiredByFeature("GL_NV_pixel_buffer_object", Api = "gles2")]
        PixelPackBuffer = Gl.PIXEL_PACK_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_PIXEL_UNPACK_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_1")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_pixel_buffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_pixel_buffer_object")]
        [RequiredByFeature("GL_NV_pixel_buffer_object", Api = "gles2")]
        PixelUnpackBuffer = Gl.PIXEL_UNPACK_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_QUERY_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_4")] [RequiredByFeature("GL_ARB_query_buffer_object", Api = "gl|glcore")] [RequiredByFeature("GL_AMD_query_buffer_object")]
        QueryBuffer = Gl.QUERY_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_SHADER_STORAGE_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_storage_buffer_object", Api = "gl|glcore")]
        ShaderStorageBuffer = Gl.SHADER_STORAGE_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_texture_buffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_texture_buffer", Api = "gles2")]
        [RequiredByFeature("GL_EXT_texture_buffer_object")]
        [RequiredByFeature("GL_OES_texture_buffer", Api = "gles2")]
        TextureBuffer = Gl.TEXTURE_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_TRANSFORM_FEEDBACK_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_VERSION_4_4")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_enhanced_layouts", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_transform_feedback")]
        [RequiredByFeature("GL_NV_transform_feedback")]
        TransformFeedbackBuffer = Gl.TRANSFORM_FEEDBACK_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_UNIFORM_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        UniformBuffer = Gl.UNIFORM_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_PARAMETER_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_6")] ParameterBuffer = Gl.PARAMETER_BUFFER
    }

    /// <summary>
    /// Strongly typed enumeration BufferUsage.
    /// </summary>
    public enum BufferUsage
    {
        /// <summary>
        /// Strongly typed for value GL_STREAM_DRAW.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_5")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_buffer_object")]
        StreamDraw = Gl.STREAM_DRAW,

        /// <summary>
        /// Strongly typed for value GL_STREAM_READ.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_5")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_vertex_buffer_object")]
        StreamRead = Gl.STREAM_READ,

        /// <summary>
        /// Strongly typed for value GL_STREAM_COPY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_5")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_vertex_buffer_object")]
        StreamCopy = Gl.STREAM_COPY,

        /// <summary>
        /// Strongly typed for value GL_STATIC_DRAW.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_5")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_buffer_object")]
        StaticDraw = Gl.STATIC_DRAW,

        /// <summary>
        /// Strongly typed for value GL_STATIC_READ.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_5")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_vertex_buffer_object")]
        StaticRead = Gl.STATIC_READ,

        /// <summary>
        /// Strongly typed for value GL_STATIC_COPY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_5")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_vertex_buffer_object")]
        StaticCopy = Gl.STATIC_COPY,

        /// <summary>
        /// Strongly typed for value GL_DYNAMIC_DRAW.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_5")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_buffer_object")]
        DynamicDraw = Gl.DYNAMIC_DRAW,

        /// <summary>
        /// Strongly typed for value GL_DYNAMIC_READ.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_5")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_vertex_buffer_object")]
        DynamicRead = Gl.DYNAMIC_READ,

        /// <summary>
        /// Strongly typed for value GL_DYNAMIC_COPY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_5")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_vertex_buffer_object")]
        DynamicCopy = Gl.DYNAMIC_COPY
    }

    /// <summary>
    /// Strongly typed enumeration CheckFramebufferStatusTarget.
    /// </summary>
    public enum CheckFramebufferStatusTarget
    {
        /// <summary>
        /// Strongly typed for value GL_DRAW_FRAMEBUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_ANGLE_framebuffer_blit", Api = "gles2")]
        [RequiredByFeature("GL_APPLE_framebuffer_multisample", Api = "gles1|gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_blit")]
        [RequiredByFeature("GL_NV_framebuffer_blit", Api = "gles2")]
        DrawFramebuffer = Gl.DRAW_FRAMEBUFFER,

        /// <summary>
        /// Strongly typed for value GL_READ_FRAMEBUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_ANGLE_framebuffer_blit", Api = "gles2")]
        [RequiredByFeature("GL_APPLE_framebuffer_multisample", Api = "gles1|gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_blit")]
        [RequiredByFeature("GL_NV_framebuffer_blit", Api = "gles2")]
        ReadFramebuffer = Gl.READ_FRAMEBUFFER,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        Framebuffer = Gl.FRAMEBUFFER
    }

    /// <summary>
    /// Strongly typed enumeration ClearBufferMask.
    /// </summary>
    [Flags]
    public enum ClearBufferMask : uint
    {
        /// <summary>
        /// Strongly typed for value GL_COLOR_BUFFER_BIT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        ColorBufferBit = Gl.COLOR_BUFFER_BIT,

        /// <summary>
        /// Strongly typed for value GL_DEPTH_BUFFER_BIT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        DepthBufferBit = Gl.DEPTH_BUFFER_BIT,

        /// <summary>
        /// Strongly typed for value GL_STENCIL_BUFFER_BIT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        StencilBufferBit = Gl.STENCIL_BUFFER_BIT
    }

    /// <summary>
    /// Strongly typed enumeration ClipControlDepth.
    /// </summary>
    public enum ClipControlDepth
    {
        /// <summary>
        /// Strongly typed for value GL_NEGATIVE_ONE_TO_ONE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_clip_control", Api = "gl|glcore")]
        NegativeOneToOne = Gl.NEGATIVE_ONE_TO_ONE,

        /// <summary>
        /// Strongly typed for value GL_ZERO_TO_ONE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_clip_control", Api = "gl|glcore")]
        ZeroToOne = Gl.ZERO_TO_ONE
    }

    /// <summary>
    /// Strongly typed enumeration ClipControlOrigin.
    /// </summary>
    public enum ClipControlOrigin
    {
        /// <summary>
        /// Strongly typed for value GL_LOWER_LEFT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_clip_control", Api = "gl|glcore")]
        LowerLeft = Gl.LOWER_LEFT,

        /// <summary>
        /// Strongly typed for value GL_UPPER_LEFT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_clip_control", Api = "gl|glcore")]
        UpperLeft = Gl.UPPER_LEFT
    }

    /// <summary>
    /// Strongly typed enumeration ClipPlaneName.
    /// </summary>
    public enum ClipPlaneName
    {
        /// <summary>
        /// Strongly typed for value GL_CLIP_DISTANCE0, GL_CLIP_PLANE0.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_APPLE_clip_distance", Api = "gles2")]
        ClipDistance0 = Gl.CLIP_DISTANCE0,

        /// <summary>
        /// Strongly typed for value GL_CLIP_DISTANCE1, GL_CLIP_PLANE1.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_APPLE_clip_distance", Api = "gles2")]
        ClipDistance1 = Gl.CLIP_DISTANCE1,

        /// <summary>
        /// Strongly typed for value GL_CLIP_DISTANCE2, GL_CLIP_PLANE2.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_APPLE_clip_distance", Api = "gles2")]
        ClipDistance2 = Gl.CLIP_DISTANCE2,

        /// <summary>
        /// Strongly typed for value GL_CLIP_DISTANCE3, GL_CLIP_PLANE3.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_APPLE_clip_distance", Api = "gles2")]
        ClipDistance3 = Gl.CLIP_DISTANCE3,

        /// <summary>
        /// Strongly typed for value GL_CLIP_DISTANCE4, GL_CLIP_PLANE4.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_APPLE_clip_distance", Api = "gles2")]
        ClipDistance4 = Gl.CLIP_DISTANCE4,

        /// <summary>
        /// Strongly typed for value GL_CLIP_DISTANCE5, GL_CLIP_PLANE5.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_APPLE_clip_distance", Api = "gles2")]
        ClipDistance5 = Gl.CLIP_DISTANCE5,

        /// <summary>
        /// Strongly typed for value GL_CLIP_DISTANCE6.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_APPLE_clip_distance", Api = "gles2")]
        ClipDistance6 = Gl.CLIP_DISTANCE6,

        /// <summary>
        /// Strongly typed for value GL_CLIP_DISTANCE7.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_APPLE_clip_distance", Api = "gles2")]
        ClipDistance7 = Gl.CLIP_DISTANCE7
    }

    /// <summary>
    /// Strongly typed enumeration ColorBuffer.
    /// </summary>
    public enum ColorBuffer
    {
        /// <summary>
        /// Strongly typed for value GL_NONE, GL_NONE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_VERSION_4_6")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_KHR_context_flush_control", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_NV_register_combiners")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        None = Gl.NONE,

        /// <summary>
        /// Strongly typed for value GL_FRONT_LEFT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] FrontLeft = Gl.FRONT_LEFT,

        /// <summary>
        /// Strongly typed for value GL_FRONT_RIGHT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] FrontRight = Gl.FRONT_RIGHT,

        /// <summary>
        /// Strongly typed for value GL_BACK_LEFT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] BackLeft = Gl.BACK_LEFT,

        /// <summary>
        /// Strongly typed for value GL_BACK_RIGHT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] BackRight = Gl.BACK_RIGHT,

        /// <summary>
        /// Strongly typed for value GL_FRONT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Front = Gl.FRONT,

        /// <summary>
        /// Strongly typed for value GL_BACK.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_ES3_1_compatibility", Api = "gl|glcore")]
        Back = Gl.BACK,

        /// <summary>
        /// Strongly typed for value GL_LEFT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] Left = Gl.LEFT,

        /// <summary>
        /// Strongly typed for value GL_RIGHT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] Right = Gl.RIGHT,

        /// <summary>
        /// Strongly typed for value GL_FRONT_AND_BACK.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        FrontAndBack = Gl.FRONT_AND_BACK,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT0.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        ColorAttachment0 = Gl.COLOR_ATTACHMENT0,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT1.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment1 = Gl.COLOR_ATTACHMENT1,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT2.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment2 = Gl.COLOR_ATTACHMENT2,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT3.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment3 = Gl.COLOR_ATTACHMENT3,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT4.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment4 = Gl.COLOR_ATTACHMENT4,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT5.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment5 = Gl.COLOR_ATTACHMENT5,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT6.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment6 = Gl.COLOR_ATTACHMENT6,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT7.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment7 = Gl.COLOR_ATTACHMENT7,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT8.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment8 = Gl.COLOR_ATTACHMENT8,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT9.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment9 = Gl.COLOR_ATTACHMENT9,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT10.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment10 = Gl.COLOR_ATTACHMENT10,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT11.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment11 = Gl.COLOR_ATTACHMENT11,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT12.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment12 = Gl.COLOR_ATTACHMENT12,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT13.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment13 = Gl.COLOR_ATTACHMENT13,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT14.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment14 = Gl.COLOR_ATTACHMENT14,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT15.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment15 = Gl.COLOR_ATTACHMENT15,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT16.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment16 = Gl.COLOR_ATTACHMENT16,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT17.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment17 = Gl.COLOR_ATTACHMENT17,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT18.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment18 = Gl.COLOR_ATTACHMENT18,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT19.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment19 = Gl.COLOR_ATTACHMENT19,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT20.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment20 = Gl.COLOR_ATTACHMENT20,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT21.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment21 = Gl.COLOR_ATTACHMENT21,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT22.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment22 = Gl.COLOR_ATTACHMENT22,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT23.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment23 = Gl.COLOR_ATTACHMENT23,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT24.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment24 = Gl.COLOR_ATTACHMENT24,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT25.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment25 = Gl.COLOR_ATTACHMENT25,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT26.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment26 = Gl.COLOR_ATTACHMENT26,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT27.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment27 = Gl.COLOR_ATTACHMENT27,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT28.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment28 = Gl.COLOR_ATTACHMENT28,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT29.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment29 = Gl.COLOR_ATTACHMENT29,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT30.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment30 = Gl.COLOR_ATTACHMENT30,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT31.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment31 = Gl.COLOR_ATTACHMENT31
    }

    /// <summary>
    /// Strongly typed enumeration ColorMaterialFace.
    /// </summary>
    public enum ColorMaterialFace
    {
        /// <summary>
        /// Strongly typed for value GL_BACK.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_ES3_1_compatibility", Api = "gl|glcore")]
        Back = Gl.BACK,

        /// <summary>
        /// Strongly typed for value GL_FRONT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Front = Gl.FRONT,

        /// <summary>
        /// Strongly typed for value GL_FRONT_AND_BACK.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        FrontAndBack = Gl.FRONT_AND_BACK
    }

    /// <summary>
    /// Strongly typed enumeration ColorPointerType.
    /// </summary>
    public enum ColorPointerType
    {
        /// <summary>
        /// Strongly typed for value GL_BYTE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_render_snorm", Api = "gles2")]
        [RequiredByFeature("GL_OES_byte_coordinates", Api = "gl|gles1")]
        Byte = Gl.BYTE,

        /// <summary>
        /// Strongly typed for value GL_DOUBLE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_ARB_gpu_shader_fp64", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_vertex_attrib_64bit")]
        Double = Gl.DOUBLE,

        /// <summary>
        /// Strongly typed for value GL_FLOAT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_OES_texture_float", Api = "gles2")]
        Float = Gl.FLOAT,

        /// <summary>
        /// Strongly typed for value GL_INT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Int = Gl.INT,

        /// <summary>
        /// Strongly typed for value GL_SHORT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_render_snorm", Api = "gles2")]
        Short = Gl.SHORT,

        /// <summary>
        /// Strongly typed for value GL_UNSIGNED_BYTE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        UnsignedByte = Gl.UNSIGNED_BYTE,

        /// <summary>
        /// Strongly typed for value GL_UNSIGNED_INT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ANGLE_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_element_index_uint", Api = "gles1|gles2")]
        UnsignedInt = Gl.UNSIGNED_INT,

        /// <summary>
        /// Strongly typed for value GL_UNSIGNED_SHORT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ANGLE_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_depth_texture", Api = "gles2")]
        UnsignedShort = Gl.UNSIGNED_SHORT
    }

    /// <summary>
    /// Strongly typed enumeration ConditionalQueryMode.
    /// </summary>
    public enum ConditionalQueryMode
    {
        /// <summary>
        /// Strongly typed for value GL_QUERY_WAIT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_NV_conditional_render", Api = "gl|glcore|gles2")]
        QueryWait = Gl.QUERY_WAIT,

        /// <summary>
        /// Strongly typed for value GL_QUERY_NO_WAIT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_NV_conditional_render", Api = "gl|glcore|gles2")]
        QueryNoWait = Gl.QUERY_NO_WAIT,

        /// <summary>
        /// Strongly typed for value GL_QUERY_BY_REGION_WAIT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_NV_conditional_render", Api = "gl|glcore|gles2")]
        QueryByRegionWait = Gl.QUERY_BY_REGION_WAIT,

        /// <summary>
        /// Strongly typed for value GL_QUERY_BY_REGION_NO_WAIT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_NV_conditional_render", Api = "gl|glcore|gles2")]
        QueryByRegionNoWait = Gl.QUERY_BY_REGION_NO_WAIT
    }

    /// <summary>
    /// Strongly typed enumeration ContextFlagMask.
    /// </summary>
    [Flags]
    public enum ContextFlagMask : uint
    {
        /// <summary>
        /// Strongly typed for value GL_CONTEXT_FLAG_DEBUG_BIT, GL_CONTEXT_FLAG_DEBUG_BIT_KHR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        ContextFlagDebugBit = Gl.CONTEXT_FLAG_DEBUG_BIT,

        /// <summary>
        /// Strongly typed for value GL_CONTEXT_FLAG_FORWARD_COMPATIBLE_BIT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] ContextFlagForwardCompatibleBit = Gl.CONTEXT_FLAG_FORWARD_COMPATIBLE_BIT,

        /// <summary>
        /// Strongly typed for value GL_CONTEXT_FLAG_ROBUST_ACCESS_BIT, GL_CONTEXT_FLAG_ROBUST_ACCESS_BIT_ARB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")] [RequiredByFeature("GL_ARB_robustness", Api = "gl|glcore")]
        ContextFlagRobustAccessBit = Gl.CONTEXT_FLAG_ROBUST_ACCESS_BIT,

        /// <summary>
        /// Strongly typed for value GL_CONTEXT_FLAG_NO_ERROR_BIT, GL_CONTEXT_FLAG_NO_ERROR_BIT_KHR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_6")] ContextFlagNoErrorBit = Gl.CONTEXT_FLAG_NO_ERROR_BIT
    }

    /// <summary>
    /// Strongly typed enumeration ContextProfileMask.
    /// </summary>
    [Flags]
    public enum ContextProfileMask : uint
    {
        /// <summary>
        /// Strongly typed for value GL_CONTEXT_COMPATIBILITY_PROFILE_BIT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")] ContextCompatibilityProfileBit = Gl.CONTEXT_COMPATIBILITY_PROFILE_BIT,

        /// <summary>
        /// Strongly typed for value GL_CONTEXT_CORE_PROFILE_BIT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")] ContextCoreProfileBit = Gl.CONTEXT_CORE_PROFILE_BIT
    }

    /// <summary>
    /// Strongly typed enumeration CullFaceMode.
    /// </summary>
    public enum CullFaceMode
    {
        /// <summary>
        /// Strongly typed for value GL_BACK.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_ES3_1_compatibility", Api = "gl|glcore")]
        Back = Gl.BACK,

        /// <summary>
        /// Strongly typed for value GL_FRONT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Front = Gl.FRONT,

        /// <summary>
        /// Strongly typed for value GL_FRONT_AND_BACK.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        FrontAndBack = Gl.FRONT_AND_BACK
    }

    /// <summary>
    /// Strongly typed enumeration DataType.
    /// </summary>
    [Flags]
    public enum DataType : uint
    {
    }

    /// <summary>
    /// Strongly typed enumeration DebugSeverity.
    /// </summary>
    public enum DebugSeverity
    {
        /// <summary>
        /// Strongly typed for value GL_DEBUG_SEVERITY_LOW.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_AMD_debug_output")]
        [RequiredByFeature("GL_ARB_debug_output", Api = "gl|glcore")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        DebugSeverityLow = Gl.DEBUG_SEVERITY_LOW,

        /// <summary>
        /// Strongly typed for value GL_DEBUG_SEVERITY_MEDIUM.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_AMD_debug_output")]
        [RequiredByFeature("GL_ARB_debug_output", Api = "gl|glcore")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        DebugSeverityMedium = Gl.DEBUG_SEVERITY_MEDIUM,

        /// <summary>
        /// Strongly typed for value GL_DEBUG_SEVERITY_HIGH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_AMD_debug_output")]
        [RequiredByFeature("GL_ARB_debug_output", Api = "gl|glcore")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        DebugSeverityHigh = Gl.DEBUG_SEVERITY_HIGH,

        /// <summary>
        /// Strongly typed for value GL_DEBUG_SEVERITY_NOTIFICATION.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        DebugSeverityNotification = Gl.DEBUG_SEVERITY_NOTIFICATION,

        /// <summary>
        /// Strongly typed for value GL_DONT_CARE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        DontCare = Gl.DONT_CARE
    }

    /// <summary>
    /// Strongly typed enumeration DebugSource.
    /// </summary>
    public enum DebugSource
    {
        /// <summary>
        /// Strongly typed for value GL_DEBUG_SOURCE_API.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_ARB_debug_output", Api = "gl|glcore")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        DebugSourceApi = Gl.DEBUG_SOURCE_API,

        /// <summary>
        /// Strongly typed for value GL_DEBUG_SOURCE_WINDOW_SYSTEM.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_ARB_debug_output", Api = "gl|glcore")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        DebugSourceWindowSystem = Gl.DEBUG_SOURCE_WINDOW_SYSTEM,

        /// <summary>
        /// Strongly typed for value GL_DEBUG_SOURCE_SHADER_COMPILER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_ARB_debug_output", Api = "gl|glcore")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        DebugSourceShaderCompiler = Gl.DEBUG_SOURCE_SHADER_COMPILER,

        /// <summary>
        /// Strongly typed for value GL_DEBUG_SOURCE_THIRD_PARTY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_ARB_debug_output", Api = "gl|glcore")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        DebugSourceThirdParty = Gl.DEBUG_SOURCE_THIRD_PARTY,

        /// <summary>
        /// Strongly typed for value GL_DEBUG_SOURCE_APPLICATION.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_ARB_debug_output", Api = "gl|glcore")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        DebugSourceApplication = Gl.DEBUG_SOURCE_APPLICATION,

        /// <summary>
        /// Strongly typed for value GL_DEBUG_SOURCE_OTHER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_ARB_debug_output", Api = "gl|glcore")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        DebugSourceOther = Gl.DEBUG_SOURCE_OTHER,

        /// <summary>
        /// Strongly typed for value GL_DONT_CARE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        DontCare = Gl.DONT_CARE
    }

    /// <summary>
    /// Strongly typed enumeration DebugType.
    /// </summary>
    public enum DebugType
    {
        /// <summary>
        /// Strongly typed for value GL_DEBUG_TYPE_ERROR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_ARB_debug_output", Api = "gl|glcore")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        DebugTypeError = Gl.DEBUG_TYPE_ERROR,

        /// <summary>
        /// Strongly typed for value GL_DEBUG_TYPE_DEPRECATED_BEHAVIOR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_ARB_debug_output", Api = "gl|glcore")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        DebugTypeDeprecatedBehavior = Gl.DEBUG_TYPE_DEPRECATED_BEHAVIOR,

        /// <summary>
        /// Strongly typed for value GL_DEBUG_TYPE_UNDEFINED_BEHAVIOR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_ARB_debug_output", Api = "gl|glcore")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        DebugTypeUndefinedBehavior = Gl.DEBUG_TYPE_UNDEFINED_BEHAVIOR,

        /// <summary>
        /// Strongly typed for value GL_DEBUG_TYPE_PORTABILITY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_ARB_debug_output", Api = "gl|glcore")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        DebugTypePortability = Gl.DEBUG_TYPE_PORTABILITY,

        /// <summary>
        /// Strongly typed for value GL_DEBUG_TYPE_PERFORMANCE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_ARB_debug_output", Api = "gl|glcore")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        DebugTypePerformance = Gl.DEBUG_TYPE_PERFORMANCE,

        /// <summary>
        /// Strongly typed for value GL_DEBUG_TYPE_MARKER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        DebugTypeMarker = Gl.DEBUG_TYPE_MARKER,

        /// <summary>
        /// Strongly typed for value GL_DEBUG_TYPE_PUSH_GROUP.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        DebugTypePushGroup = Gl.DEBUG_TYPE_PUSH_GROUP,

        /// <summary>
        /// Strongly typed for value GL_DEBUG_TYPE_POP_GROUP.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        DebugTypePopGroup = Gl.DEBUG_TYPE_POP_GROUP,

        /// <summary>
        /// Strongly typed for value GL_DEBUG_TYPE_OTHER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_ARB_debug_output", Api = "gl|glcore")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        DebugTypeOther = Gl.DEBUG_TYPE_OTHER,

        /// <summary>
        /// Strongly typed for value GL_DONT_CARE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        DontCare = Gl.DONT_CARE
    }

    /// <summary>
    /// Strongly typed enumeration DepthFunction.
    /// </summary>
    public enum DepthFunction
    {
        /// <summary>
        /// Strongly typed for value GL_ALWAYS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Always = Gl.ALWAYS,

        /// <summary>
        /// Strongly typed for value GL_EQUAL.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
        Equal = Gl.EQUAL,

        /// <summary>
        /// Strongly typed for value GL_GEQUAL.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Gequal = Gl.GEQUAL,

        /// <summary>
        /// Strongly typed for value GL_GREATER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Greater = Gl.GREATER,

        /// <summary>
        /// Strongly typed for value GL_LEQUAL.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Lequal = Gl.LEQUAL,

        /// <summary>
        /// Strongly typed for value GL_LESS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Less = Gl.LESS,

        /// <summary>
        /// Strongly typed for value GL_NEVER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Never = Gl.NEVER,

        /// <summary>
        /// Strongly typed for value GL_NOTEQUAL.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Notequal = Gl.NOTEQUAL
    }

    /// <summary>
    /// Strongly typed enumeration DrawBufferMode.
    /// </summary>
    public enum DrawBufferMode
    {
        /// <summary>
        /// Strongly typed for value GL_BACK.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_ES3_1_compatibility", Api = "gl|glcore")]
        Back = Gl.BACK,

        /// <summary>
        /// Strongly typed for value GL_BACK_LEFT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] BackLeft = Gl.BACK_LEFT,

        /// <summary>
        /// Strongly typed for value GL_BACK_RIGHT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] BackRight = Gl.BACK_RIGHT,

        /// <summary>
        /// Strongly typed for value GL_FRONT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Front = Gl.FRONT,

        /// <summary>
        /// Strongly typed for value GL_FRONT_AND_BACK.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        FrontAndBack = Gl.FRONT_AND_BACK,

        /// <summary>
        /// Strongly typed for value GL_FRONT_LEFT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] FrontLeft = Gl.FRONT_LEFT,

        /// <summary>
        /// Strongly typed for value GL_FRONT_RIGHT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] FrontRight = Gl.FRONT_RIGHT,

        /// <summary>
        /// Strongly typed for value GL_LEFT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] Left = Gl.LEFT,

        /// <summary>
        /// Strongly typed for value GL_NONE, GL_NONE_OES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_VERSION_4_6")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_KHR_context_flush_control", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_NV_register_combiners")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        None = Gl.NONE,

        /// <summary>
        /// Strongly typed for value GL_RIGHT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] Right = Gl.RIGHT
    }

    /// <summary>
    /// Strongly typed enumeration DrawElementsType.
    /// </summary>
    public enum DrawElementsType
    {
        /// <summary>
        /// Strongly typed for value GL_UNSIGNED_BYTE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        UnsignedByte = Gl.UNSIGNED_BYTE,

        /// <summary>
        /// Strongly typed for value GL_UNSIGNED_SHORT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ANGLE_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_depth_texture", Api = "gles2")]
        UnsignedShort = Gl.UNSIGNED_SHORT,

        /// <summary>
        /// Strongly typed for value GL_UNSIGNED_INT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ANGLE_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_element_index_uint", Api = "gles1|gles2")]
        UnsignedInt = Gl.UNSIGNED_INT
    }

    /// <summary>
    /// Strongly typed enumeration EnableCap.
    /// </summary>
    public enum EnableCap
    {
        /// <summary>
        /// Strongly typed for value GL_BLEND.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        Blend = Gl.BLEND,

        /// <summary>
        /// Strongly typed for value GL_CLIP_PLANE0, GL_CLIP_DISTANCE0.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_APPLE_clip_distance", Api = "gles2")]
        ClipDistance0 = Gl.CLIP_DISTANCE0,

        /// <summary>
        /// Strongly typed for value GL_CLIP_PLANE1, GL_CLIP_DISTANCE1.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_APPLE_clip_distance", Api = "gles2")]
        ClipDistance1 = Gl.CLIP_DISTANCE1,

        /// <summary>
        /// Strongly typed for value GL_CLIP_PLANE2, GL_CLIP_DISTANCE2.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_APPLE_clip_distance", Api = "gles2")]
        ClipDistance2 = Gl.CLIP_DISTANCE2,

        /// <summary>
        /// Strongly typed for value GL_CLIP_PLANE3, GL_CLIP_DISTANCE3.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_APPLE_clip_distance", Api = "gles2")]
        ClipDistance3 = Gl.CLIP_DISTANCE3,

        /// <summary>
        /// Strongly typed for value GL_CLIP_PLANE4, GL_CLIP_DISTANCE4.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_APPLE_clip_distance", Api = "gles2")]
        ClipDistance4 = Gl.CLIP_DISTANCE4,

        /// <summary>
        /// Strongly typed for value GL_CLIP_PLANE5, GL_CLIP_DISTANCE5.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_APPLE_clip_distance", Api = "gles2")]
        ClipDistance5 = Gl.CLIP_DISTANCE5,

        /// <summary>
        /// Strongly typed for value GL_CLIP_DISTANCE6.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_APPLE_clip_distance", Api = "gles2")]
        ClipDistance6 = Gl.CLIP_DISTANCE6,

        /// <summary>
        /// Strongly typed for value GL_CLIP_DISTANCE7.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_APPLE_clip_distance", Api = "gles2")]
        ClipDistance7 = Gl.CLIP_DISTANCE7,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ARRAY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        ColorArray = Gl.COLOR_ARRAY,

        /// <summary>
        /// Strongly typed for value GL_COLOR_LOGIC_OP.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        ColorLogicOp = Gl.COLOR_LOGIC_OP,

        /// <summary>
        /// Strongly typed for value GL_CULL_FACE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        CullFace = Gl.CULL_FACE,

        /// <summary>
        /// Strongly typed for value GL_DEPTH_TEST.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        DepthTest = Gl.DEPTH_TEST,

        /// <summary>
        /// Strongly typed for value GL_DITHER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Dither = Gl.DITHER,

        /// <summary>
        /// Strongly typed for value GL_EDGE_FLAG_ARRAY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        EdgeFlagArray = Gl.EDGE_FLAG_ARRAY,

        /// <summary>
        /// Strongly typed for value GL_INDEX_ARRAY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        IndexArray = Gl.INDEX_ARRAY,

        /// <summary>
        /// Strongly typed for value GL_INDEX_LOGIC_OP.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RemovedByFeature("GL_VERSION_3_2")]
        IndexLogicOp = Gl.INDEX_LOGIC_OP,

        /// <summary>
        /// Strongly typed for value GL_LINE_SMOOTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        LineSmooth = Gl.LINE_SMOOTH,

        /// <summary>
        /// Strongly typed for value GL_MULTISAMPLE_SGIS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ARB_multisample")]
        [RequiredByFeature("GL_EXT_multisample")]
        [RequiredByFeature("GL_EXT_multisampled_compatibility", Api = "gles2")]
        [RequiredByFeature("GL_SGIS_multisample")]
        Multisample = Gl.MULTISAMPLE,

        /// <summary>
        /// Strongly typed for value GL_NORMAL_ARRAY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        NormalArray = Gl.NORMAL_ARRAY,

        /// <summary>
        /// Strongly typed for value GL_POLYGON_OFFSET_FILL.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        PolygonOffsetFill = Gl.POLYGON_OFFSET_FILL,

        /// <summary>
        /// Strongly typed for value GL_POLYGON_OFFSET_LINE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_NV_polygon_mode", Api = "gles2")]
        PolygonOffsetLine = Gl.POLYGON_OFFSET_LINE,

        /// <summary>
        /// Strongly typed for value GL_POLYGON_OFFSET_POINT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_NV_polygon_mode", Api = "gles2")]
        PolygonOffsetPoint = Gl.POLYGON_OFFSET_POINT,

        /// <summary>
        /// Strongly typed for value GL_POLYGON_SMOOTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] PolygonSmooth = Gl.POLYGON_SMOOTH,

        /// <summary>
        /// Strongly typed for value GL_RESCALE_NORMAL_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_rescale_normal")] [RemovedByFeature("GL_VERSION_3_2")]
        RescaleNormal = Gl.RESCALE_NORMAL,

        /// <summary>
        /// Strongly typed for value GL_SAMPLE_ALPHA_TO_ONE_SGIS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ARB_multisample")]
        [RequiredByFeature("GL_EXT_multisample")]
        [RequiredByFeature("GL_EXT_multisampled_compatibility", Api = "gles2")]
        [RequiredByFeature("GL_SGIS_multisample")]
        SampleAlphaToOne = Gl.SAMPLE_ALPHA_TO_ONE,

        /// <summary>
        /// Strongly typed for value GL_SCISSOR_TEST.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_viewport_array", Api = "gl|glcore")]
        [RequiredByFeature("GL_NV_viewport_array", Api = "gles2")]
        [RequiredByFeature("GL_OES_viewport_array", Api = "gles2")]
        ScissorTest = Gl.SCISSOR_TEST,

        /// <summary>
        /// Strongly typed for value GL_STENCIL_TEST.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        StencilTest = Gl.STENCIL_TEST,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_1D.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        Texture1d = Gl.TEXTURE_1D,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_2D.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_sparse_texture", Api = "gles2")]
        Texture2d = Gl.TEXTURE_2D,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_3D_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_sparse_texture", Api = "gles2")]
        [RequiredByFeature("GL_EXT_texture3D")]
        [RequiredByFeature("GL_OES_texture_3D", Api = "gles2")]
        Texture3d = Gl.TEXTURE_3D,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_COORD_ARRAY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        TextureCoordArray = Gl.TEXTURE_COORD_ARRAY,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_ARRAY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_EXT_vertex_array")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RemovedByFeature("GL_VERSION_3_2")]
        VertexArray = Gl.VERTEX_ARRAY,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_SRGB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ARB_framebuffer_sRGB", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_framebuffer_sRGB")]
        [RequiredByFeature("GL_EXT_sRGB_write_control", Api = "gles2")]
        FramebufferSrgb = Gl.FRAMEBUFFER_SRGB,

        /// <summary>
        /// Strongly typed for value GL_PRIMITIVE_RESTART.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] PrimitiveRestart = Gl.PRIMITIVE_RESTART,

        /// <summary>
        /// Strongly typed for value GL_PRIMITIVE_RESTART_FIXED_INDEX.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_ES3_compatibility", Api = "gl|glcore")]
        PrimitiveRestartFixedIndex = Gl.PRIMITIVE_RESTART_FIXED_INDEX,

        /// <summary>
        /// Strongly typed for value GL_RASTERIZER_DISCARD.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_transform_feedback")] [RequiredByFeature("GL_NV_transform_feedback")]
        RasterizerDiscard = Gl.RASTERIZER_DISCARD,

        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_debug_output", Api = "gl|glcore")]
        [RequiredByFeature("GL_KHR_debug")]
        [RequiredByFeature("GL_KHR_debug", Api = "gles2")]
        DebugOuput = 0x92E0
    }

    /// <summary>
    /// Strongly typed enumeration ErrorCode.
    /// </summary>
    public enum ErrorCode
    {
        /// <summary>
        /// Strongly typed for value GL_INVALID_ENUM.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        InvalidEnum = Gl.INVALID_ENUM,

        /// <summary>
        /// Strongly typed for value GL_INVALID_FRAMEBUFFER_OPERATION, GL_INVALID_FRAMEBUFFER_OPERATION_EXT,
        /// GL_INVALID_FRAMEBUFFER_OPERATION_OES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        InvalidFramebufferOperation = Gl.INVALID_FRAMEBUFFER_OPERATION,

        /// <summary>
        /// Strongly typed for value GL_INVALID_OPERATION.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        InvalidOperation = Gl.INVALID_OPERATION,

        /// <summary>
        /// Strongly typed for value GL_INVALID_VALUE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        InvalidValue = Gl.INVALID_VALUE,

        /// <summary>
        /// Strongly typed for value GL_NO_ERROR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_robustness", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_robustness", Api = "gles1|gles2")]
        [RequiredByFeature("GL_KHR_robustness", Api = "gl|glcore|gles2")]
        NoError = Gl.NO_ERROR,

        /// <summary>
        /// Strongly typed for value GL_OUT_OF_MEMORY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        OutOfMemory = Gl.OUT_OF_MEMORY,

        /// <summary>
        /// Strongly typed for value GL_STACK_OVERFLOW.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RemovedByFeature("GL_VERSION_3_2")]
        StackOverflow = Gl.STACK_OVERFLOW,

        /// <summary>
        /// Strongly typed for value GL_STACK_UNDERFLOW.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RemovedByFeature("GL_VERSION_3_2")]
        StackUnderflow = Gl.STACK_UNDERFLOW
    }

    /// <summary>
    /// Strongly typed enumeration FogCoordinatePointerType.
    /// </summary>
    public enum FogCoordinatePointerType
    {
        /// <summary>
        /// Strongly typed for value GL_FLOAT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_OES_texture_float", Api = "gles2")]
        Float = Gl.FLOAT,

        /// <summary>
        /// Strongly typed for value GL_DOUBLE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_ARB_gpu_shader_fp64", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_vertex_attrib_64bit")]
        Double = Gl.DOUBLE
    }

    /// <summary>
    /// Strongly typed enumeration FogMode.
    /// </summary>
    public enum FogMode
    {
        /// <summary>
        /// Strongly typed for value GL_LINEAR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Linear = Gl.LINEAR
    }

    /// <summary>
    /// Strongly typed enumeration FogPointerTypeIBM.
    /// </summary>
    public enum FogPointerTypeIbm
    {
        /// <summary>
        /// Strongly typed for value GL_FLOAT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_OES_texture_float", Api = "gles2")]
        Float = Gl.FLOAT,

        /// <summary>
        /// Strongly typed for value GL_DOUBLE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_ARB_gpu_shader_fp64", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_vertex_attrib_64bit")]
        Double = Gl.DOUBLE
    }

    /// <summary>
    /// Strongly typed enumeration FramebufferAttachment.
    /// </summary>
    public enum FramebufferAttachment
    {
        /// <summary>
        /// Strongly typed for value GL_MAX_COLOR_ATTACHMENTS, GL_MAX_COLOR_ATTACHMENTS_EXT, GL_MAX_COLOR_ATTACHMENTS_NV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        MaxColorAttachments = Gl.MAX_COLOR_ATTACHMENTS,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT0, GL_COLOR_ATTACHMENT0_EXT, GL_COLOR_ATTACHMENT0_NV,
        /// GL_COLOR_ATTACHMENT0_OES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        ColorAttachment0 = Gl.COLOR_ATTACHMENT0,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT1, GL_COLOR_ATTACHMENT1_EXT, GL_COLOR_ATTACHMENT1_NV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment1 = Gl.COLOR_ATTACHMENT1,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT2, GL_COLOR_ATTACHMENT2_EXT, GL_COLOR_ATTACHMENT2_NV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment2 = Gl.COLOR_ATTACHMENT2,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT3, GL_COLOR_ATTACHMENT3_EXT, GL_COLOR_ATTACHMENT3_NV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment3 = Gl.COLOR_ATTACHMENT3,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT4, GL_COLOR_ATTACHMENT4_EXT, GL_COLOR_ATTACHMENT4_NV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment4 = Gl.COLOR_ATTACHMENT4,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT5, GL_COLOR_ATTACHMENT5_EXT, GL_COLOR_ATTACHMENT5_NV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment5 = Gl.COLOR_ATTACHMENT5,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT6, GL_COLOR_ATTACHMENT6_EXT, GL_COLOR_ATTACHMENT6_NV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment6 = Gl.COLOR_ATTACHMENT6,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT7, GL_COLOR_ATTACHMENT7_EXT, GL_COLOR_ATTACHMENT7_NV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment7 = Gl.COLOR_ATTACHMENT7,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT8, GL_COLOR_ATTACHMENT8_EXT, GL_COLOR_ATTACHMENT8_NV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment8 = Gl.COLOR_ATTACHMENT8,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT9, GL_COLOR_ATTACHMENT9_EXT, GL_COLOR_ATTACHMENT9_NV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment9 = Gl.COLOR_ATTACHMENT9,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT10, GL_COLOR_ATTACHMENT10_EXT, GL_COLOR_ATTACHMENT10_NV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment10 = Gl.COLOR_ATTACHMENT10,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT11, GL_COLOR_ATTACHMENT11_EXT, GL_COLOR_ATTACHMENT11_NV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment11 = Gl.COLOR_ATTACHMENT11,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT12, GL_COLOR_ATTACHMENT12_EXT, GL_COLOR_ATTACHMENT12_NV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment12 = Gl.COLOR_ATTACHMENT12,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT13, GL_COLOR_ATTACHMENT13_EXT, GL_COLOR_ATTACHMENT13_NV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment13 = Gl.COLOR_ATTACHMENT13,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT14, GL_COLOR_ATTACHMENT14_EXT, GL_COLOR_ATTACHMENT14_NV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment14 = Gl.COLOR_ATTACHMENT14,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT15, GL_COLOR_ATTACHMENT15_EXT, GL_COLOR_ATTACHMENT15_NV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment15 = Gl.COLOR_ATTACHMENT15,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT16.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment16 = Gl.COLOR_ATTACHMENT16,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT17.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment17 = Gl.COLOR_ATTACHMENT17,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT18.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment18 = Gl.COLOR_ATTACHMENT18,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT19.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment19 = Gl.COLOR_ATTACHMENT19,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT20.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment20 = Gl.COLOR_ATTACHMENT20,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT21.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment21 = Gl.COLOR_ATTACHMENT21,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT22.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment22 = Gl.COLOR_ATTACHMENT22,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT23.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment23 = Gl.COLOR_ATTACHMENT23,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT24.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment24 = Gl.COLOR_ATTACHMENT24,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT25.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment25 = Gl.COLOR_ATTACHMENT25,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT26.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment26 = Gl.COLOR_ATTACHMENT26,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT27.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment27 = Gl.COLOR_ATTACHMENT27,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT28.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment28 = Gl.COLOR_ATTACHMENT28,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT29.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment29 = Gl.COLOR_ATTACHMENT29,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT30.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment30 = Gl.COLOR_ATTACHMENT30,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT31.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment31 = Gl.COLOR_ATTACHMENT31,

        /// <summary>
        /// Strongly typed for value GL_DEPTH_ATTACHMENT, GL_DEPTH_ATTACHMENT_EXT, GL_DEPTH_ATTACHMENT_OES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        DepthAttachment = Gl.DEPTH_ATTACHMENT,

        /// <summary>
        /// Strongly typed for value GL_DEPTH_STENCIL_ATTACHMENT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        DepthStencilAttachment = Gl.DEPTH_STENCIL_ATTACHMENT
    }

    /// <summary>
    /// Strongly typed enumeration FramebufferAttachmentParameterName.
    /// </summary>
    public enum FramebufferAttachmentParameterName
    {
        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_ATTACHMENT_RED_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        FramebufferAttachmentRedSize = Gl.FRAMEBUFFER_ATTACHMENT_RED_SIZE,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_ATTACHMENT_GREEN_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        FramebufferAttachmentGreenSize = Gl.FRAMEBUFFER_ATTACHMENT_GREEN_SIZE,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_ATTACHMENT_BLUE_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        FramebufferAttachmentBlueSize = Gl.FRAMEBUFFER_ATTACHMENT_BLUE_SIZE,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_ATTACHMENT_ALPHA_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        FramebufferAttachmentAlphaSize = Gl.FRAMEBUFFER_ATTACHMENT_ALPHA_SIZE,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_ATTACHMENT_DEPTH_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        FramebufferAttachmentDepthSize = Gl.FRAMEBUFFER_ATTACHMENT_DEPTH_SIZE,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_ATTACHMENT_STENCIL_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        FramebufferAttachmentStencilSize = Gl.FRAMEBUFFER_ATTACHMENT_STENCIL_SIZE,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_ATTACHMENT_COMPONENT_TYPE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_color_buffer_half_float", Api = "gles2")]
        FramebufferAttachmentComponentType = Gl.FRAMEBUFFER_ATTACHMENT_COMPONENT_TYPE,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_ATTACHMENT_COLOR_ENCODING.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_sRGB", Api = "gles1|gles2")]
        FramebufferAttachmentColorEncoding = Gl.FRAMEBUFFER_ATTACHMENT_COLOR_ENCODING,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_ATTACHMENT_OBJECT_NAME, GL_FRAMEBUFFER_ATTACHMENT_OBJECT_NAME.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        FramebufferAttachmentObjectName = Gl.FRAMEBUFFER_ATTACHMENT_OBJECT_NAME,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_ATTACHMENT_TEXTURE_LEVEL.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        FramebufferAttachmentTextureLevel = Gl.FRAMEBUFFER_ATTACHMENT_TEXTURE_LEVEL,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_ATTACHMENT_TEXTURE_CUBE_MAP_FACE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        FramebufferAttachmentTextureCubeMapFace = Gl.FRAMEBUFFER_ATTACHMENT_TEXTURE_CUBE_MAP_FACE,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_ATTACHMENT_LAYERED.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_geometry_shader4", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
        [RequiredByFeature("GL_EXT_geometry_shader4")]
        [RequiredByFeature("GL_NV_geometry_program4")]
        [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
        FramebufferAttachmentLayered = Gl.FRAMEBUFFER_ATTACHMENT_LAYERED,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_ATTACHMENT_TEXTURE_LAYER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_geometry_shader4", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_geometry_shader4")]
        [RequiredByFeature("GL_EXT_texture_array")]
        [RequiredByFeature("GL_NV_geometry_program4")]
        FramebufferAttachmentTextureLayer = Gl.FRAMEBUFFER_ATTACHMENT_TEXTURE_LAYER
    }

    /// <summary>
    /// Strongly typed enumeration FramebufferParameterName.
    /// </summary>
    public enum FramebufferParameterName
    {
        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_DEFAULT_WIDTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_framebuffer_no_attachments", Api = "gl|glcore")]
        FramebufferDefaultWidth = Gl.FRAMEBUFFER_DEFAULT_WIDTH,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_DEFAULT_HEIGHT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_framebuffer_no_attachments", Api = "gl|glcore")]
        FramebufferDefaultHeight = Gl.FRAMEBUFFER_DEFAULT_HEIGHT,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_DEFAULT_LAYERS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_no_attachments", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
        FramebufferDefaultLayers = Gl.FRAMEBUFFER_DEFAULT_LAYERS,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_DEFAULT_SAMPLES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_framebuffer_no_attachments", Api = "gl|glcore")]
        FramebufferDefaultSamples = Gl.FRAMEBUFFER_DEFAULT_SAMPLES,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_DEFAULT_FIXED_SAMPLE_LOCATIONS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_framebuffer_no_attachments", Api = "gl|glcore")]
        FramebufferDefaultFixedSampleLocations = Gl.FRAMEBUFFER_DEFAULT_FIXED_SAMPLE_LOCATIONS
    }

    /// <summary>
    /// Strongly typed enumeration FramebufferStatus.
    /// </summary>
    public enum FramebufferStatus
    {
        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_COMPLETE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        FramebufferComplete = Gl.FRAMEBUFFER_COMPLETE,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_UNDEFINED.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_OES_surfaceless_context", Api = "gles1|gles2")]
        FramebufferUndefined = Gl.FRAMEBUFFER_UNDEFINED,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_INCOMPLETE_ATTACHMENT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        FramebufferIncompleteAttachment = Gl.FRAMEBUFFER_INCOMPLETE_ATTACHMENT,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        FramebufferIncompleteMissingAttachment = Gl.FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_INCOMPLETE_DRAW_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_framebuffer_object")]
        FramebufferIncompleteDrawBuffer = Gl.FRAMEBUFFER_INCOMPLETE_DRAW_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_INCOMPLETE_READ_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_framebuffer_object")]
        FramebufferIncompleteReadBuffer = Gl.FRAMEBUFFER_INCOMPLETE_READ_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_UNSUPPORTED.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        FramebufferUnsupported = Gl.FRAMEBUFFER_UNSUPPORTED,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_INCOMPLETE_MULTISAMPLE, GL_FRAMEBUFFER_INCOMPLETE_MULTISAMPLE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_ANGLE_framebuffer_multisample", Api = "gles2")]
        [RequiredByFeature("GL_APPLE_framebuffer_multisample", Api = "gles1|gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_multisample")]
        [RequiredByFeature("GL_EXT_multisampled_render_to_texture", Api = "gles1|gles2")]
        [RequiredByFeature("GL_NV_framebuffer_multisample", Api = "gles2")]
        FramebufferIncompleteMultisample = Gl.FRAMEBUFFER_INCOMPLETE_MULTISAMPLE,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_INCOMPLETE_LAYER_TARGETS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_geometry_shader4", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
        [RequiredByFeature("GL_EXT_geometry_shader4")]
        [RequiredByFeature("GL_NV_geometry_program4")]
        [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
        FramebufferIncompleteLayerTargets = Gl.FRAMEBUFFER_INCOMPLETE_LAYER_TARGETS
    }

    /// <summary>
    /// Strongly typed enumeration FramebufferTarget.
    /// </summary>
    public enum FramebufferTarget
    {
        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        Framebuffer = Gl.FRAMEBUFFER,

        /// <summary>
        /// Strongly typed for value GL_DRAW_FRAMEBUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_ANGLE_framebuffer_blit", Api = "gles2")]
        [RequiredByFeature("GL_APPLE_framebuffer_multisample", Api = "gles1|gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_blit")]
        [RequiredByFeature("GL_NV_framebuffer_blit", Api = "gles2")]
        DrawFramebuffer = Gl.DRAW_FRAMEBUFFER,

        /// <summary>
        /// Strongly typed for value GL_READ_FRAMEBUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_ANGLE_framebuffer_blit", Api = "gles2")]
        [RequiredByFeature("GL_APPLE_framebuffer_multisample", Api = "gles1|gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_blit")]
        [RequiredByFeature("GL_NV_framebuffer_blit", Api = "gles2")]
        ReadFramebuffer = Gl.READ_FRAMEBUFFER
    }

    /// <summary>
    /// Strongly typed enumeration FrontFaceDirection.
    /// </summary>
    public enum FrontFaceDirection
    {
        /// <summary>
        /// Strongly typed for value GL_CCW.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
        Ccw = Gl.CCW,

        /// <summary>
        /// Strongly typed for value GL_CW.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
        Cw = Gl.CW
    }

    /// <summary>
    /// Strongly typed enumeration GetFramebufferParameter.
    /// </summary>
    public enum GetFramebufferParameter
    {
        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_DEFAULT_WIDTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_framebuffer_no_attachments", Api = "gl|glcore")]
        FramebufferDefaultWidth = Gl.FRAMEBUFFER_DEFAULT_WIDTH,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_DEFAULT_HEIGHT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_framebuffer_no_attachments", Api = "gl|glcore")]
        FramebufferDefaultHeight = Gl.FRAMEBUFFER_DEFAULT_HEIGHT,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_DEFAULT_LAYERS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_no_attachments", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
        FramebufferDefaultLayers = Gl.FRAMEBUFFER_DEFAULT_LAYERS,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_DEFAULT_SAMPLES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_framebuffer_no_attachments", Api = "gl|glcore")]
        FramebufferDefaultSamples = Gl.FRAMEBUFFER_DEFAULT_SAMPLES,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_DEFAULT_FIXED_SAMPLE_LOCATIONS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_framebuffer_no_attachments", Api = "gl|glcore")]
        FramebufferDefaultFixedSampleLocations = Gl.FRAMEBUFFER_DEFAULT_FIXED_SAMPLE_LOCATIONS,

        /// <summary>
        /// Strongly typed for value GL_DOUBLEBUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] Doublebuffer = Gl.DOUBLEBUFFER,

        /// <summary>
        /// Strongly typed for value GL_IMPLEMENTATION_COLOR_READ_FORMAT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_ES2_compatibility", Api = "gl|glcore")]
        [RequiredByFeature("GL_OES_read_format", Api = "gl|gles1")]
        ImplementationColorReadFormat = Gl.IMPLEMENTATION_COLOR_READ_FORMAT,

        /// <summary>
        /// Strongly typed for value GL_IMPLEMENTATION_COLOR_READ_TYPE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_ES2_compatibility", Api = "gl|glcore")]
        [RequiredByFeature("GL_OES_read_format", Api = "gl|gles1")]
        ImplementationColorReadType = Gl.IMPLEMENTATION_COLOR_READ_TYPE,

        /// <summary>
        /// Strongly typed for value GL_SAMPLES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_multisample")]
        [RequiredByFeature("GL_NV_multisample_coverage")]
        [RequiredByFeature("GL_EXT_multisample")]
        [RequiredByFeature("GL_SGIS_multisample")]
        Samples = Gl.SAMPLES,

        /// <summary>
        /// Strongly typed for value GL_SAMPLE_BUFFERS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multisample")]
        [RequiredByFeature("GL_EXT_multisample")]
        [RequiredByFeature("GL_SGIS_multisample")]
        SampleBuffers = Gl.SAMPLE_BUFFERS,

        /// <summary>
        /// Strongly typed for value GL_STEREO.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] Stereo = Gl.STEREO
    }

    /// <summary>
    /// Strongly typed enumeration GetPName.
    /// </summary>
    public enum GetPName
    {
        /// <summary>
        /// Strongly typed for value GL_ACTIVE_TEXTURE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        ActiveTexture = Gl.ACTIVE_TEXTURE,

        /// <summary>
        /// Strongly typed for value GL_ALIASED_LINE_WIDTH_RANGE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        AliasedLineWidthRange = Gl.ALIASED_LINE_WIDTH_RANGE,

        /// <summary>
        /// Strongly typed for value GL_ALIASED_POINT_SIZE_RANGE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RemovedByFeature("GL_VERSION_3_2")]
        AliasedPointSizeRange = Gl.ALIASED_POINT_SIZE_RANGE,

        /// <summary>
        /// Strongly typed for value GL_ARRAY_BUFFER_BINDING.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_5")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_buffer_object")]
        ArrayBufferBinding = Gl.ARRAY_BUFFER_BINDING,

        /// <summary>
        /// Strongly typed for value GL_BLEND.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        Blend = Gl.BLEND,

        /// <summary>
        /// Strongly typed for value GL_BLEND_COLOR, GL_BLEND_COLOR_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_imaging", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_blend_color")]
        BlendColor = Gl.BLEND_COLOR,

        /// <summary>
        /// Strongly typed for value GL_BLEND_DST.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        BlendDst = Gl.BLEND_DST,

        /// <summary>
        /// Strongly typed for value GL_BLEND_DST_ALPHA.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_EXT_blend_func_separate")]
        [RequiredByFeature("GL_OES_blend_func_separate", Api = "gles1")]
        BlendDstAlpha = Gl.BLEND_DST_ALPHA,

        /// <summary>
        /// Strongly typed for value GL_BLEND_DST_RGB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_EXT_blend_func_separate")]
        [RequiredByFeature("GL_OES_blend_func_separate", Api = "gles1")]
        BlendDstRgb = Gl.BLEND_DST_RGB,

        /// <summary>
        /// Strongly typed for value GL_BLEND_EQUATION_ALPHA.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_EXT_blend_equation_separate")]
        [RequiredByFeature("GL_OES_blend_equation_separate", Api = "gles1")]
        BlendEquationAlpha = Gl.BLEND_EQUATION_ALPHA,

        /// <summary>
        /// Strongly typed for value GL_BLEND_EQUATION_EXT, GL_BLEND_EQUATION_RGB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_EXT_blend_equation_separate")]
        [RequiredByFeature("GL_OES_blend_equation_separate", Api = "gles1")]
        BlendEquationRgb = Gl.BLEND_EQUATION_RGB,

        /// <summary>
        /// Strongly typed for value GL_BLEND_SRC.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        BlendSrc = Gl.BLEND_SRC,

        /// <summary>
        /// Strongly typed for value GL_BLEND_SRC_ALPHA.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_EXT_blend_func_separate")]
        [RequiredByFeature("GL_OES_blend_func_separate", Api = "gles1")]
        BlendSrcAlpha = Gl.BLEND_SRC_ALPHA,

        /// <summary>
        /// Strongly typed for value GL_BLEND_SRC_RGB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_EXT_blend_func_separate")]
        [RequiredByFeature("GL_OES_blend_func_separate", Api = "gles1")]
        BlendSrcRgb = Gl.BLEND_SRC_RGB,

        /// <summary>
        /// Strongly typed for value GL_CLIENT_ATTRIB_STACK_DEPTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RemovedByFeature("GL_VERSION_3_2")]
        ClientAttribStackDepth = Gl.CLIENT_ATTRIB_STACK_DEPTH,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ARRAY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        ColorArray = Gl.COLOR_ARRAY,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ARRAY_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        ColorArraySize = Gl.COLOR_ARRAY_SIZE,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ARRAY_STRIDE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        ColorArrayStride = Gl.COLOR_ARRAY_STRIDE,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ARRAY_TYPE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        ColorArrayType = Gl.COLOR_ARRAY_TYPE,

        /// <summary>
        /// Strongly typed for value GL_COLOR_CLEAR_VALUE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        ColorClearValue = Gl.COLOR_CLEAR_VALUE,

        /// <summary>
        /// Strongly typed for value GL_COLOR_LOGIC_OP.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        ColorLogicOp = Gl.COLOR_LOGIC_OP,

        /// <summary>
        /// Strongly typed for value GL_COLOR_WRITEMASK.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        ColorWritemask = Gl.COLOR_WRITEMASK,

        /// <summary>
        /// Strongly typed for value GL_COMPRESSED_TEXTURE_FORMATS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_texture_compression")]
        CompressedTextureFormats = Gl.COMPRESSED_TEXTURE_FORMATS,

        /// <summary>
        /// Strongly typed for value GL_CONTEXT_FLAGS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        ContextFlags = Gl.CONTEXT_FLAGS,

        /// <summary>
        /// Strongly typed for value GL_CULL_FACE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        CullFace = Gl.CULL_FACE,

        /// <summary>
        /// Strongly typed for value GL_CULL_FACE_MODE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        CullFaceMode = Gl.CULL_FACE_MODE,

        /// <summary>
        /// Strongly typed for value GL_CURRENT_PROGRAM.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        CurrentProgram = Gl.CURRENT_PROGRAM,

        /// <summary>
        /// Strongly typed for value GL_DEBUG_GROUP_STACK_DEPTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        DebugGroupStackDepth = Gl.DEBUG_GROUP_STACK_DEPTH,

        /// <summary>
        /// Strongly typed for value GL_DEPTH_CLEAR_VALUE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        DepthClearValue = Gl.DEPTH_CLEAR_VALUE,

        /// <summary>
        /// Strongly typed for value GL_DEPTH_FUNC.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        DepthFunc = Gl.DEPTH_FUNC,

        /// <summary>
        /// Strongly typed for value GL_DEPTH_RANGE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_viewport_array", Api = "gl|glcore")]
        [RequiredByFeature("GL_NV_viewport_array", Api = "gles2")]
        [RequiredByFeature("GL_OES_viewport_array", Api = "gles2")]
        DepthRange = Gl.DEPTH_RANGE,

        /// <summary>
        /// Strongly typed for value GL_DEPTH_TEST.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        DepthTest = Gl.DEPTH_TEST,

        /// <summary>
        /// Strongly typed for value GL_DEPTH_WRITEMASK.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        DepthWritemask = Gl.DEPTH_WRITEMASK,

        /// <summary>
        /// Strongly typed for value GL_DISPATCH_INDIRECT_BUFFER_BINDING.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_compute_shader", Api = "gl|glcore")]
        DispatchIndirectBufferBinding = Gl.DISPATCH_INDIRECT_BUFFER_BINDING,

        /// <summary>
        /// Strongly typed for value GL_DITHER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Dither = Gl.DITHER,

        /// <summary>
        /// Strongly typed for value GL_DOUBLEBUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] Doublebuffer = Gl.DOUBLEBUFFER,

        /// <summary>
        /// Strongly typed for value GL_DRAW_BUFFER, GL_DRAW_BUFFER_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_EXT_multiview_draw_buffers", Api = "gles2")]
        DrawBuffer = Gl.DRAW_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_DRAW_FRAMEBUFFER_BINDING.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_ANGLE_framebuffer_blit", Api = "gles2")]
        [RequiredByFeature("GL_APPLE_framebuffer_multisample", Api = "gles1|gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_blit")]
        [RequiredByFeature("GL_NV_framebuffer_blit", Api = "gles2")]
        DrawFramebufferBinding = Gl.DRAW_FRAMEBUFFER_BINDING,

        /// <summary>
        /// Strongly typed for value GL_EDGE_FLAG_ARRAY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        EdgeFlagArray = Gl.EDGE_FLAG_ARRAY,

        /// <summary>
        /// Strongly typed for value GL_EDGE_FLAG_ARRAY_STRIDE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        EdgeFlagArrayStride = Gl.EDGE_FLAG_ARRAY_STRIDE,

        /// <summary>
        /// Strongly typed for value GL_ELEMENT_ARRAY_BUFFER_BINDING.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_5")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_buffer_object")]
        ElementArrayBufferBinding = Gl.ELEMENT_ARRAY_BUFFER_BINDING,

        /// <summary>
        /// Strongly typed for value GL_FEEDBACK_BUFFER_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RemovedByFeature("GL_VERSION_3_2")]
        FeedbackBufferSize = Gl.FEEDBACK_BUFFER_SIZE,

        /// <summary>
        /// Strongly typed for value GL_FEEDBACK_BUFFER_TYPE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RemovedByFeature("GL_VERSION_3_2")]
        FeedbackBufferType = Gl.FEEDBACK_BUFFER_TYPE,

        /// <summary>
        /// Strongly typed for value GL_FRAGMENT_SHADER_DERIVATIVE_HINT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_fragment_shader")]
        [RequiredByFeature("GL_OES_standard_derivatives", Api = "gles2|glsc2")]
        FragmentShaderDerivativeHint = Gl.FRAGMENT_SHADER_DERIVATIVE_HINT,

        /// <summary>
        /// Strongly typed for value GL_FRONT_FACE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        FrontFace = Gl.FRONT_FACE,

        /// <summary>
        /// Strongly typed for value GL_GENERATE_MIPMAP_HINT_SGIS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_SGIS_generate_mipmap")]
        [RemovedByFeature("GL_VERSION_3_2")]
        GenerateMipmapHint = Gl.GENERATE_MIPMAP_HINT,

        /// <summary>
        /// Strongly typed for value GL_IMPLEMENTATION_COLOR_READ_FORMAT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_ES2_compatibility", Api = "gl|glcore")]
        [RequiredByFeature("GL_OES_read_format", Api = "gl|gles1")]
        ImplementationColorReadFormat = Gl.IMPLEMENTATION_COLOR_READ_FORMAT,

        /// <summary>
        /// Strongly typed for value GL_IMPLEMENTATION_COLOR_READ_TYPE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_ES2_compatibility", Api = "gl|glcore")]
        [RequiredByFeature("GL_OES_read_format", Api = "gl|gles1")]
        ImplementationColorReadType = Gl.IMPLEMENTATION_COLOR_READ_TYPE,

        /// <summary>
        /// Strongly typed for value GL_INDEX_ARRAY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        IndexArray = Gl.INDEX_ARRAY,

        /// <summary>
        /// Strongly typed for value GL_INDEX_ARRAY_STRIDE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        IndexArrayStride = Gl.INDEX_ARRAY_STRIDE,

        /// <summary>
        /// Strongly typed for value GL_INDEX_ARRAY_TYPE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        IndexArrayType = Gl.INDEX_ARRAY_TYPE,

        /// <summary>
        /// Strongly typed for value GL_INDEX_LOGIC_OP, GL_LOGIC_OP.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RemovedByFeature("GL_VERSION_3_2")]
        IndexLogicOp = Gl.INDEX_LOGIC_OP,

        /// <summary>
        /// Strongly typed for value GL_LAYER_PROVOKING_VERTEX.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_viewport_array", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
        LayerProvokingVertex = Gl.LAYER_PROVOKING_VERTEX,

        /// <summary>
        /// Strongly typed for value GL_LIGHT_MODEL_COLOR_CONTROL.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_EXT_separate_specular_color")] [RemovedByFeature("GL_VERSION_3_2")]
        LightModelColorControl = Gl.LIGHT_MODEL_COLOR_CONTROL,

        /// <summary>
        /// Strongly typed for value GL_LINE_SMOOTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        LineSmooth = Gl.LINE_SMOOTH,

        /// <summary>
        /// Strongly typed for value GL_LINE_SMOOTH_HINT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        LineSmoothHint = Gl.LINE_SMOOTH_HINT,

        /// <summary>
        /// Strongly typed for value GL_LINE_WIDTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        LineWidth = Gl.LINE_WIDTH,

        /// <summary>
        /// Strongly typed for value GL_LINE_WIDTH_GRANULARITY, GL_SMOOTH_LINE_WIDTH_GRANULARITY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] LineWidthGranularity = Gl.LINE_WIDTH_GRANULARITY,

        /// <summary>
        /// Strongly typed for value GL_LINE_WIDTH_RANGE, GL_SMOOTH_LINE_WIDTH_RANGE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] LineWidthRange = Gl.LINE_WIDTH_RANGE,

        /// <summary>
        /// Strongly typed for value GL_LOGIC_OP_MODE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        LogicOpMode = Gl.LOGIC_OP_MODE,

        /// <summary>
        /// Strongly typed for value GL_MAJOR_VERSION.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        MajorVersion = Gl.MAJOR_VERSION,

        /// <summary>
        /// Strongly typed for value GL_MAX_3D_TEXTURE_SIZE, GL_MAX_3D_TEXTURE_SIZE_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_texture3D")] [RequiredByFeature("GL_OES_texture_3D", Api = "gles2")]
        Max3dTextureSize = Gl.MAX_3D_TEXTURE_SIZE,

        /// <summary>
        /// Strongly typed for value GL_MAX_ARRAY_TEXTURE_LAYERS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_texture_array")]
        MaxArrayTextureLayers = Gl.MAX_ARRAY_TEXTURE_LAYERS,

        /// <summary>
        /// Strongly typed for value GL_MAX_CLIP_DISTANCES, GL_MAX_CLIP_PLANES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_APPLE_clip_distance", Api = "gles2")]
        MaxClipDistances = Gl.MAX_CLIP_DISTANCES,

        /// <summary>
        /// Strongly typed for value GL_MAX_COLOR_TEXTURE_SAMPLES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_texture_multisample", Api = "gl|glcore")]
        MaxColorTextureSamples = Gl.MAX_COLOR_TEXTURE_SAMPLES,

        /// <summary>
        /// Strongly typed for value GL_MAX_COMBINED_ATOMIC_COUNTERS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
        MaxCombinedAtomicCounters = Gl.MAX_COMBINED_ATOMIC_COUNTERS,

        /// <summary>
        /// Strongly typed for value GL_MAX_COMBINED_COMPUTE_UNIFORM_COMPONENTS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_compute_shader", Api = "gl|glcore")]
        MaxCombinedComputeUniformComponents = Gl.MAX_COMBINED_COMPUTE_UNIFORM_COMPONENTS,

        /// <summary>
        /// Strongly typed for value GL_MAX_COMBINED_FRAGMENT_UNIFORM_COMPONENTS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        MaxCombinedFragmentUniformComponents = Gl.MAX_COMBINED_FRAGMENT_UNIFORM_COMPONENTS,

        /// <summary>
        /// Strongly typed for value GL_MAX_COMBINED_GEOMETRY_UNIFORM_COMPONENTS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
        MaxCombinedGeometryUniformComponents = Gl.MAX_COMBINED_GEOMETRY_UNIFORM_COMPONENTS,

        /// <summary>
        /// Strongly typed for value GL_MAX_COMBINED_SHADER_STORAGE_BLOCKS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_storage_buffer_object", Api = "gl|glcore")]
        MaxCombinedShaderStorageBlocks = Gl.MAX_COMBINED_SHADER_STORAGE_BLOCKS,

        /// <summary>
        /// Strongly typed for value GL_MAX_COMBINED_TEXTURE_IMAGE_UNITS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        MaxCombinedTextureImageUnits = Gl.MAX_COMBINED_TEXTURE_IMAGE_UNITS,

        /// <summary>
        /// Strongly typed for value GL_MAX_COMBINED_UNIFORM_BLOCKS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        MaxCombinedUniformBlocks = Gl.MAX_COMBINED_UNIFORM_BLOCKS,

        /// <summary>
        /// Strongly typed for value GL_MAX_COMBINED_VERTEX_UNIFORM_COMPONENTS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        MaxCombinedVertexUniformComponents = Gl.MAX_COMBINED_VERTEX_UNIFORM_COMPONENTS,

        /// <summary>
        /// Strongly typed for value GL_MAX_COMPUTE_ATOMIC_COUNTERS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_compute_shader", Api = "gl|glcore")]
        MaxComputeAtomicCounters = Gl.MAX_COMPUTE_ATOMIC_COUNTERS,

        /// <summary>
        /// Strongly typed for value GL_MAX_COMPUTE_ATOMIC_COUNTER_BUFFERS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_compute_shader", Api = "gl|glcore")]
        MaxComputeAtomicCounterBuffers = Gl.MAX_COMPUTE_ATOMIC_COUNTER_BUFFERS,

        /// <summary>
        /// Strongly typed for value GL_MAX_COMPUTE_SHADER_STORAGE_BLOCKS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_storage_buffer_object", Api = "gl|glcore")]
        MaxComputeShaderStorageBlocks = Gl.MAX_COMPUTE_SHADER_STORAGE_BLOCKS,

        /// <summary>
        /// Strongly typed for value GL_MAX_COMPUTE_TEXTURE_IMAGE_UNITS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_compute_shader", Api = "gl|glcore")]
        MaxComputeTextureImageUnits = Gl.MAX_COMPUTE_TEXTURE_IMAGE_UNITS,

        /// <summary>
        /// Strongly typed for value GL_MAX_COMPUTE_UNIFORM_BLOCKS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_compute_shader", Api = "gl|glcore")]
        MaxComputeUniformBlocks = Gl.MAX_COMPUTE_UNIFORM_BLOCKS,

        /// <summary>
        /// Strongly typed for value GL_MAX_COMPUTE_UNIFORM_COMPONENTS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_compute_shader", Api = "gl|glcore")]
        MaxComputeUniformComponents = Gl.MAX_COMPUTE_UNIFORM_COMPONENTS,

        /// <summary>
        /// Strongly typed for value GL_MAX_COMPUTE_WORK_GROUP_COUNT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_compute_shader", Api = "gl|glcore")]
        MaxComputeWorkGroupCount = Gl.MAX_COMPUTE_WORK_GROUP_COUNT,

        /// <summary>
        /// Strongly typed for value GL_MAX_COMPUTE_WORK_GROUP_INVOCATIONS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_compute_shader", Api = "gl|glcore")]
        MaxComputeWorkGroupInvocations = Gl.MAX_COMPUTE_WORK_GROUP_INVOCATIONS,

        /// <summary>
        /// Strongly typed for value GL_MAX_COMPUTE_WORK_GROUP_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_compute_shader", Api = "gl|glcore")]
        MaxComputeWorkGroupSize = Gl.MAX_COMPUTE_WORK_GROUP_SIZE,

        /// <summary>
        /// Strongly typed for value GL_MAX_CUBE_MAP_TEXTURE_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_texture_cube_map")]
        [RequiredByFeature("GL_EXT_texture_cube_map")]
        [RequiredByFeature("GL_OES_texture_cube_map", Api = "gles1")]
        MaxCubeMapTextureSize = Gl.MAX_CUBE_MAP_TEXTURE_SIZE,

        /// <summary>
        /// Strongly typed for value GL_MAX_DEBUG_GROUP_STACK_DEPTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        MaxDebugGroupStackDepth = Gl.MAX_DEBUG_GROUP_STACK_DEPTH,

        /// <summary>
        /// Strongly typed for value GL_MAX_DEPTH_TEXTURE_SAMPLES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_texture_multisample", Api = "gl|glcore")]
        MaxDepthTextureSamples = Gl.MAX_DEPTH_TEXTURE_SAMPLES,

        /// <summary>
        /// Strongly typed for value GL_MAX_DRAW_BUFFERS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_draw_buffers")]
        [RequiredByFeature("GL_ATI_draw_buffers")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        MaxDrawBuffers = Gl.MAX_DRAW_BUFFERS,

        /// <summary>
        /// Strongly typed for value GL_MAX_DUAL_SOURCE_DRAW_BUFFERS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_3")] [RequiredByFeature("GL_ARB_blend_func_extended", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_blend_func_extended", Api = "gles2")]
        MaxDualSourceDrawBuffers = Gl.MAX_DUAL_SOURCE_DRAW_BUFFERS,

        /// <summary>
        /// Strongly typed for value GL_MAX_ELEMENTS_INDICES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_draw_range_elements")]
        MaxElementsIndices = Gl.MAX_ELEMENTS_INDICES,

        /// <summary>
        /// Strongly typed for value GL_MAX_ELEMENTS_VERTICES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_draw_range_elements")]
        MaxElementsVertices = Gl.MAX_ELEMENTS_VERTICES,

        /// <summary>
        /// Strongly typed for value GL_MAX_ELEMENT_INDEX.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_ES3_compatibility", Api = "gl|glcore")]
        MaxElementIndex = Gl.MAX_ELEMENT_INDEX,

        /// <summary>
        /// Strongly typed for value GL_MAX_FRAGMENT_ATOMIC_COUNTERS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
        MaxFragmentAtomicCounters = Gl.MAX_FRAGMENT_ATOMIC_COUNTERS,

        /// <summary>
        /// Strongly typed for value GL_MAX_FRAGMENT_INPUT_COMPONENTS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        MaxFragmentInputComponents = Gl.MAX_FRAGMENT_INPUT_COMPONENTS,

        /// <summary>
        /// Strongly typed for value GL_MAX_FRAGMENT_SHADER_STORAGE_BLOCKS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_storage_buffer_object", Api = "gl|glcore")]
        MaxFragmentShaderStorageBlocks = Gl.MAX_FRAGMENT_SHADER_STORAGE_BLOCKS,

        /// <summary>
        /// Strongly typed for value GL_MAX_FRAGMENT_UNIFORM_BLOCKS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        MaxFragmentUniformBlocks = Gl.MAX_FRAGMENT_UNIFORM_BLOCKS,

        /// <summary>
        /// Strongly typed for value GL_MAX_FRAGMENT_UNIFORM_COMPONENTS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_fragment_shader")]
        MaxFragmentUniformComponents = Gl.MAX_FRAGMENT_UNIFORM_COMPONENTS,

        /// <summary>
        /// Strongly typed for value GL_MAX_FRAGMENT_UNIFORM_VECTORS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_ES2_compatibility", Api = "gl|glcore")]
        MaxFragmentUniformVectors = Gl.MAX_FRAGMENT_UNIFORM_VECTORS,

        /// <summary>
        /// Strongly typed for value GL_MAX_FRAMEBUFFER_HEIGHT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_framebuffer_no_attachments", Api = "gl|glcore")]
        MaxFramebufferHeight = Gl.MAX_FRAMEBUFFER_HEIGHT,

        /// <summary>
        /// Strongly typed for value GL_MAX_FRAMEBUFFER_LAYERS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_no_attachments", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
        MaxFramebufferLayers = Gl.MAX_FRAMEBUFFER_LAYERS,

        /// <summary>
        /// Strongly typed for value GL_MAX_FRAMEBUFFER_SAMPLES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_framebuffer_no_attachments", Api = "gl|glcore")]
        MaxFramebufferSamples = Gl.MAX_FRAMEBUFFER_SAMPLES,

        /// <summary>
        /// Strongly typed for value GL_MAX_FRAMEBUFFER_WIDTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_framebuffer_no_attachments", Api = "gl|glcore")]
        MaxFramebufferWidth = Gl.MAX_FRAMEBUFFER_WIDTH,

        /// <summary>
        /// Strongly typed for value GL_MAX_GEOMETRY_ATOMIC_COUNTERS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
        MaxGeometryAtomicCounters = Gl.MAX_GEOMETRY_ATOMIC_COUNTERS,

        /// <summary>
        /// Strongly typed for value GL_MAX_GEOMETRY_INPUT_COMPONENTS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
        MaxGeometryInputComponents = Gl.MAX_GEOMETRY_INPUT_COMPONENTS,

        /// <summary>
        /// Strongly typed for value GL_MAX_GEOMETRY_OUTPUT_COMPONENTS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
        MaxGeometryOutputComponents = Gl.MAX_GEOMETRY_OUTPUT_COMPONENTS,

        /// <summary>
        /// Strongly typed for value GL_MAX_GEOMETRY_SHADER_STORAGE_BLOCKS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_storage_buffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
        MaxGeometryShaderStorageBlocks = Gl.MAX_GEOMETRY_SHADER_STORAGE_BLOCKS,

        /// <summary>
        /// Strongly typed for value GL_MAX_GEOMETRY_TEXTURE_IMAGE_UNITS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_geometry_shader4", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
        [RequiredByFeature("GL_EXT_geometry_shader4")]
        [RequiredByFeature("GL_NV_geometry_program4")]
        [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
        MaxGeometryTextureImageUnits = Gl.MAX_GEOMETRY_TEXTURE_IMAGE_UNITS,

        /// <summary>
        /// Strongly typed for value GL_MAX_INTEGER_SAMPLES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_texture_multisample", Api = "gl|glcore")]
        MaxIntegerSamples = Gl.MAX_INTEGER_SAMPLES,

        /// <summary>
        /// Strongly typed for value GL_MAX_LABEL_LENGTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        MaxLabelLength = Gl.MAX_LABEL_LENGTH,

        /// <summary>
        /// Strongly typed for value GL_MAX_PROGRAM_TEXEL_OFFSET.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_gpu_shader4")] [RequiredByFeature("GL_NV_gpu_program4")]
        MaxProgramTexelOffset = Gl.MAX_PROGRAM_TEXEL_OFFSET,

        /// <summary>
        /// Strongly typed for value GL_MAX_RECTANGLE_TEXTURE_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ARB_texture_rectangle")] [RequiredByFeature("GL_NV_texture_rectangle")]
        MaxRectangleTextureSize = Gl.MAX_RECTANGLE_TEXTURE_SIZE,

        /// <summary>
        /// Strongly typed for value GL_MAX_RENDERBUFFER_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        MaxRenderbufferSize = Gl.MAX_RENDERBUFFER_SIZE,

        /// <summary>
        /// Strongly typed for value GL_MAX_SAMPLE_MASK_WORDS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_ARB_texture_multisample", Api = "gl|glcore")]
        [RequiredByFeature("GL_NV_explicit_multisample")]
        MaxSampleMaskWords = Gl.MAX_SAMPLE_MASK_WORDS,

        /// <summary>
        /// Strongly typed for value GL_MAX_SERVER_WAIT_TIMEOUT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_sync", Api = "gl|glcore")]
        [RequiredByFeature("GL_APPLE_sync", Api = "gles1|gles2")]
        MaxServerWaitTimeout = Gl.MAX_SERVER_WAIT_TIMEOUT,

        /// <summary>
        /// Strongly typed for value GL_MAX_SHADER_STORAGE_BUFFER_BINDINGS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_storage_buffer_object", Api = "gl|glcore")]
        MaxShaderStorageBufferBindings = Gl.MAX_SHADER_STORAGE_BUFFER_BINDINGS,

        /// <summary>
        /// Strongly typed for value GL_MAX_TESS_CONTROL_ATOMIC_COUNTERS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
        MaxTessControlAtomicCounters = Gl.MAX_TESS_CONTROL_ATOMIC_COUNTERS,

        /// <summary>
        /// Strongly typed for value GL_MAX_TESS_CONTROL_SHADER_STORAGE_BLOCKS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_storage_buffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
        MaxTessControlShaderStorageBlocks = Gl.MAX_TESS_CONTROL_SHADER_STORAGE_BLOCKS,

        /// <summary>
        /// Strongly typed for value GL_MAX_TESS_EVALUATION_ATOMIC_COUNTERS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
        MaxTessEvaluationAtomicCounters = Gl.MAX_TESS_EVALUATION_ATOMIC_COUNTERS,

        /// <summary>
        /// Strongly typed for value GL_MAX_TESS_EVALUATION_SHADER_STORAGE_BLOCKS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_storage_buffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
        MaxTessEvaluationShaderStorageBlocks = Gl.MAX_TESS_EVALUATION_SHADER_STORAGE_BLOCKS,

        /// <summary>
        /// Strongly typed for value GL_MAX_TEXTURE_BUFFER_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_texture_buffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_texture_buffer", Api = "gles2")]
        [RequiredByFeature("GL_EXT_texture_buffer_object")]
        [RequiredByFeature("GL_OES_texture_buffer", Api = "gles2")]
        MaxTextureBufferSize = Gl.MAX_TEXTURE_BUFFER_SIZE,

        /// <summary>
        /// Strongly typed for value GL_MAX_TEXTURE_IMAGE_UNITS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_fragment_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_fragment_program")]
        MaxTextureImageUnits = Gl.MAX_TEXTURE_IMAGE_UNITS,

        /// <summary>
        /// Strongly typed for value GL_MAX_TEXTURE_LOD_BIAS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_texture_lod_bias", Api = "gl|gles1")]
        MaxTextureLodBias = Gl.MAX_TEXTURE_LOD_BIAS,

        /// <summary>
        /// Strongly typed for value GL_MAX_TEXTURE_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        MaxTextureSize = Gl.MAX_TEXTURE_SIZE,

        /// <summary>
        /// Strongly typed for value GL_MAX_UNIFORM_BLOCK_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        MaxUniformBlockSize = Gl.MAX_UNIFORM_BLOCK_SIZE,

        /// <summary>
        /// Strongly typed for value GL_MAX_UNIFORM_BUFFER_BINDINGS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        MaxUniformBufferBindings = Gl.MAX_UNIFORM_BUFFER_BINDINGS,

        /// <summary>
        /// Strongly typed for value GL_MAX_UNIFORM_LOCATIONS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_explicit_uniform_location", Api = "gl|glcore")]
        MaxUniformLocations = Gl.MAX_UNIFORM_LOCATIONS,

        /// <summary>
        /// Strongly typed for value GL_MAX_VARYING_VECTORS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_ES2_compatibility", Api = "gl|glcore")]
        MaxVaryingVectors = Gl.MAX_VARYING_VECTORS,

        /// <summary>
        /// Strongly typed for value GL_MAX_VERTEX_ATOMIC_COUNTERS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
        MaxVertexAtomicCounters = Gl.MAX_VERTEX_ATOMIC_COUNTERS,

        /// <summary>
        /// Strongly typed for value GL_MAX_VERTEX_ATTRIBS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        MaxVertexAttribs = Gl.MAX_VERTEX_ATTRIBS,

        /// <summary>
        /// Strongly typed for value GL_MAX_VERTEX_ATTRIB_BINDINGS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_vertex_attrib_binding", Api = "gl|glcore")]
        MaxVertexAttribBindings = Gl.MAX_VERTEX_ATTRIB_BINDINGS,

        /// <summary>
        /// Strongly typed for value GL_MAX_VERTEX_ATTRIB_RELATIVE_OFFSET.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_vertex_attrib_binding", Api = "gl|glcore")]
        MaxVertexAttribRelativeOffset = Gl.MAX_VERTEX_ATTRIB_RELATIVE_OFFSET,

        /// <summary>
        /// Strongly typed for value GL_MAX_VERTEX_OUTPUT_COMPONENTS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        MaxVertexOutputComponents = Gl.MAX_VERTEX_OUTPUT_COMPONENTS,

        /// <summary>
        /// Strongly typed for value GL_MAX_VERTEX_SHADER_STORAGE_BLOCKS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_storage_buffer_object", Api = "gl|glcore")]
        MaxVertexShaderStorageBlocks = Gl.MAX_VERTEX_SHADER_STORAGE_BLOCKS,

        /// <summary>
        /// Strongly typed for value GL_MAX_VERTEX_TEXTURE_IMAGE_UNITS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_NV_vertex_program3")]
        MaxVertexTextureImageUnits = Gl.MAX_VERTEX_TEXTURE_IMAGE_UNITS,

        /// <summary>
        /// Strongly typed for value GL_MAX_VERTEX_UNIFORM_BLOCKS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        MaxVertexUniformBlocks = Gl.MAX_VERTEX_UNIFORM_BLOCKS,

        /// <summary>
        /// Strongly typed for value GL_MAX_VERTEX_UNIFORM_COMPONENTS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_vertex_shader")]
        MaxVertexUniformComponents = Gl.MAX_VERTEX_UNIFORM_COMPONENTS,

        /// <summary>
        /// Strongly typed for value GL_MAX_VERTEX_UNIFORM_VECTORS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_ES2_compatibility", Api = "gl|glcore")]
        MaxVertexUniformVectors = Gl.MAX_VERTEX_UNIFORM_VECTORS,

        /// <summary>
        /// Strongly typed for value GL_MAX_VIEWPORTS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ARB_viewport_array", Api = "gl|glcore")]
        [RequiredByFeature("GL_NV_viewport_array", Api = "gles2")]
        [RequiredByFeature("GL_OES_viewport_array", Api = "gles2")]
        MaxViewports = Gl.MAX_VIEWPORTS,

        /// <summary>
        /// Strongly typed for value GL_MAX_VIEWPORT_DIMS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        MaxViewportDims = Gl.MAX_VIEWPORT_DIMS,

        /// <summary>
        /// Strongly typed for value GL_MINOR_VERSION.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        MinorVersion = Gl.MINOR_VERSION,

        /// <summary>
        /// Strongly typed for value GL_MIN_MAP_BUFFER_ALIGNMENT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")] [RequiredByFeature("GL_ARB_map_buffer_alignment", Api = "gl|glcore")]
        MinMapBufferAlignment = Gl.MIN_MAP_BUFFER_ALIGNMENT,

        /// <summary>
        /// Strongly typed for value GL_MIN_PROGRAM_TEXEL_OFFSET.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_gpu_shader4")] [RequiredByFeature("GL_NV_gpu_program4")]
        MinProgramTexelOffset = Gl.MIN_PROGRAM_TEXEL_OFFSET,

        /// <summary>
        /// Strongly typed for value GL_MULTISAMPLE_SGIS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ARB_multisample")]
        [RequiredByFeature("GL_EXT_multisample")]
        [RequiredByFeature("GL_EXT_multisampled_compatibility", Api = "gles2")]
        [RequiredByFeature("GL_SGIS_multisample")]
        Multisample = Gl.MULTISAMPLE,

        /// <summary>
        /// Strongly typed for value GL_NORMAL_ARRAY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        NormalArray = Gl.NORMAL_ARRAY,

        /// <summary>
        /// Strongly typed for value GL_NORMAL_ARRAY_STRIDE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        NormalArrayStride = Gl.NORMAL_ARRAY_STRIDE,

        /// <summary>
        /// Strongly typed for value GL_NORMAL_ARRAY_TYPE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        NormalArrayType = Gl.NORMAL_ARRAY_TYPE,

        /// <summary>
        /// Strongly typed for value GL_NUM_COMPRESSED_TEXTURE_FORMATS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_texture_compression")]
        NumCompressedTextureFormats = Gl.NUM_COMPRESSED_TEXTURE_FORMATS,

        /// <summary>
        /// Strongly typed for value GL_NUM_EXTENSIONS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        NumExtensions = Gl.NUM_EXTENSIONS,

        /// <summary>
        /// Strongly typed for value GL_NUM_PROGRAM_BINARY_FORMATS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_get_program_binary", Api = "gl|glcore")]
        [RequiredByFeature("GL_OES_get_program_binary", Api = "gles2")]
        NumProgramBinaryFormats = Gl.NUM_PROGRAM_BINARY_FORMATS,

        /// <summary>
        /// Strongly typed for value GL_NUM_SHADER_BINARY_FORMATS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_ES2_compatibility", Api = "gl|glcore")]
        NumShaderBinaryFormats = Gl.NUM_SHADER_BINARY_FORMATS,

        /// <summary>
        /// Strongly typed for value GL_PACK_ALIGNMENT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        PackAlignment = Gl.PACK_ALIGNMENT,

        /// <summary>
        /// Strongly typed for value GL_PACK_IMAGE_HEIGHT, GL_PACK_IMAGE_HEIGHT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_EXT_texture3D")]
        PackImageHeight = Gl.PACK_IMAGE_HEIGHT,

        /// <summary>
        /// Strongly typed for value GL_PACK_LSB_FIRST.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] PackLsbFirst = Gl.PACK_LSB_FIRST,

        /// <summary>
        /// Strongly typed for value GL_PACK_ROW_LENGTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        PackRowLength = Gl.PACK_ROW_LENGTH,

        /// <summary>
        /// Strongly typed for value GL_PACK_SKIP_IMAGES, GL_PACK_SKIP_IMAGES_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_EXT_texture3D")]
        PackSkipImages = Gl.PACK_SKIP_IMAGES,

        /// <summary>
        /// Strongly typed for value GL_PACK_SKIP_PIXELS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        PackSkipPixels = Gl.PACK_SKIP_PIXELS,

        /// <summary>
        /// Strongly typed for value GL_PACK_SKIP_ROWS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        PackSkipRows = Gl.PACK_SKIP_ROWS,

        /// <summary>
        /// Strongly typed for value GL_PACK_SWAP_BYTES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] PackSwapBytes = Gl.PACK_SWAP_BYTES,

        /// <summary>
        /// Strongly typed for value GL_PIXEL_PACK_BUFFER_BINDING.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_1")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_pixel_buffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_pixel_buffer_object")]
        [RequiredByFeature("GL_NV_pixel_buffer_object", Api = "gles2")]
        PixelPackBufferBinding = Gl.PIXEL_PACK_BUFFER_BINDING,

        /// <summary>
        /// Strongly typed for value GL_PIXEL_UNPACK_BUFFER_BINDING.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_1")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_pixel_buffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_pixel_buffer_object")]
        [RequiredByFeature("GL_NV_pixel_buffer_object", Api = "gles2")]
        PixelUnpackBufferBinding = Gl.PIXEL_UNPACK_BUFFER_BINDING,

        /// <summary>
        /// Strongly typed for value GL_POINT_FADE_THRESHOLD_SIZE, GL_POINT_FADE_THRESHOLD_SIZE_SGIS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ARB_point_parameters")]
        [RequiredByFeature("GL_EXT_point_parameters")]
        [RequiredByFeature("GL_SGIS_point_parameters")]
        PointFadeThresholdSize = Gl.POINT_FADE_THRESHOLD_SIZE,

        /// <summary>
        /// Strongly typed for value GL_POINT_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        PointSize = Gl.POINT_SIZE,

        /// <summary>
        /// Strongly typed for value GL_POINT_SIZE_GRANULARITY, GL_SMOOTH_POINT_SIZE_GRANULARITY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] PointSizeGranularity = Gl.POINT_SIZE_GRANULARITY,

        /// <summary>
        /// Strongly typed for value GL_POINT_SIZE_MAX_SGIS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ARB_point_parameters")]
        [RequiredByFeature("GL_EXT_point_parameters")]
        [RequiredByFeature("GL_SGIS_point_parameters")]
        [RemovedByFeature("GL_VERSION_3_2")]
        PointSizeMax = Gl.POINT_SIZE_MAX,

        /// <summary>
        /// Strongly typed for value GL_POINT_SIZE_MIN_SGIS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ARB_point_parameters")]
        [RequiredByFeature("GL_EXT_point_parameters")]
        [RequiredByFeature("GL_SGIS_point_parameters")]
        [RemovedByFeature("GL_VERSION_3_2")]
        PointSizeMin = Gl.POINT_SIZE_MIN,

        /// <summary>
        /// Strongly typed for value GL_POINT_SIZE_RANGE, GL_SMOOTH_POINT_SIZE_RANGE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] PointSizeRange = Gl.POINT_SIZE_RANGE,

        /// <summary>
        /// Strongly typed for value GL_POLYGON_MODE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_NV_polygon_mode", Api = "gles2")]
        PolygonMode = Gl.POLYGON_MODE,

        /// <summary>
        /// Strongly typed for value GL_POLYGON_OFFSET_FACTOR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_polygon_offset")]
        PolygonOffsetFactor = Gl.POLYGON_OFFSET_FACTOR,

        /// <summary>
        /// Strongly typed for value GL_POLYGON_OFFSET_FILL.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        PolygonOffsetFill = Gl.POLYGON_OFFSET_FILL,

        /// <summary>
        /// Strongly typed for value GL_POLYGON_OFFSET_LINE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_NV_polygon_mode", Api = "gles2")]
        PolygonOffsetLine = Gl.POLYGON_OFFSET_LINE,

        /// <summary>
        /// Strongly typed for value GL_POLYGON_OFFSET_POINT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_NV_polygon_mode", Api = "gles2")]
        PolygonOffsetPoint = Gl.POLYGON_OFFSET_POINT,

        /// <summary>
        /// Strongly typed for value GL_POLYGON_OFFSET_UNITS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        PolygonOffsetUnits = Gl.POLYGON_OFFSET_UNITS,

        /// <summary>
        /// Strongly typed for value GL_POLYGON_SMOOTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] PolygonSmooth = Gl.POLYGON_SMOOTH,

        /// <summary>
        /// Strongly typed for value GL_POLYGON_SMOOTH_HINT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] PolygonSmoothHint = Gl.POLYGON_SMOOTH_HINT,

        /// <summary>
        /// Strongly typed for value GL_PRIMITIVE_RESTART_INDEX.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] PrimitiveRestartIndex = Gl.PRIMITIVE_RESTART_INDEX,

        /// <summary>
        /// Strongly typed for value GL_PROGRAM_BINARY_FORMATS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_get_program_binary", Api = "gl|glcore")]
        [RequiredByFeature("GL_OES_get_program_binary", Api = "gles2")]
        ProgramBinaryFormats = Gl.PROGRAM_BINARY_FORMATS,

        /// <summary>
        /// Strongly typed for value GL_PROGRAM_PIPELINE_BINDING.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_ARB_separate_shader_objects", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_separate_shader_objects", Api = "gl|glcore|gles2")]
        ProgramPipelineBinding = Gl.PROGRAM_PIPELINE_BINDING,

        /// <summary>
        /// Strongly typed for value GL_PROGRAM_POINT_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ARB_vertex_program")] [RequiredByFeature("GL_ARB_vertex_shader")] [RequiredByFeature("GL_NV_vertex_program")]
        VertexProgramPointSize = Gl.VERTEX_PROGRAM_POINT_SIZE,

        /// <summary>
        /// Strongly typed for value GL_PROVOKING_VERTEX.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_ARB_provoking_vertex", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_viewport_array", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_provoking_vertex")]
        ProvokingVertex = Gl.PROVOKING_VERTEX,

        /// <summary>
        /// Strongly typed for value GL_READ_BUFFER, GL_READ_BUFFER_EXT, GL_READ_BUFFER_NV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_EXT_multiview_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_read_buffer", Api = "gles2")]
        ReadBuffer = Gl.READ_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_READ_FRAMEBUFFER_BINDING.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_ANGLE_framebuffer_blit", Api = "gles2")]
        [RequiredByFeature("GL_APPLE_framebuffer_multisample", Api = "gles1|gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_blit")]
        [RequiredByFeature("GL_NV_framebuffer_blit", Api = "gles2")]
        ReadFramebufferBinding = Gl.READ_FRAMEBUFFER_BINDING,

        /// <summary>
        /// Strongly typed for value GL_RENDERBUFFER_BINDING.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        RenderbufferBinding = Gl.RENDERBUFFER_BINDING,

        /// <summary>
        /// Strongly typed for value GL_RESCALE_NORMAL_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_rescale_normal")] [RemovedByFeature("GL_VERSION_3_2")]
        RescaleNormal = Gl.RESCALE_NORMAL,

        /// <summary>
        /// Strongly typed for value GL_SAMPLER_BINDING.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_3")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_sampler_objects", Api = "gl|glcore")]
        SamplerBinding = Gl.SAMPLER_BINDING,

        /// <summary>
        /// Strongly typed for value GL_SAMPLES, GL_SAMPLES_SGIS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_multisample")]
        [RequiredByFeature("GL_NV_multisample_coverage")]
        [RequiredByFeature("GL_EXT_multisample")]
        [RequiredByFeature("GL_SGIS_multisample")]
        Samples = Gl.SAMPLES,

        /// <summary>
        /// Strongly typed for value GL_SAMPLE_ALPHA_TO_ONE_SGIS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ARB_multisample")]
        [RequiredByFeature("GL_EXT_multisample")]
        [RequiredByFeature("GL_EXT_multisampled_compatibility", Api = "gles2")]
        [RequiredByFeature("GL_SGIS_multisample")]
        SampleAlphaToOne = Gl.SAMPLE_ALPHA_TO_ONE,

        /// <summary>
        /// Strongly typed for value GL_SAMPLE_BUFFERS, GL_SAMPLE_BUFFERS_SGIS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multisample")]
        [RequiredByFeature("GL_EXT_multisample")]
        [RequiredByFeature("GL_SGIS_multisample")]
        SampleBuffers = Gl.SAMPLE_BUFFERS,

        /// <summary>
        /// Strongly typed for value GL_SAMPLE_COVERAGE_INVERT, GL_SAMPLE_MASK_INVERT_SGIS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multisample")]
        SampleCoverageInvert = Gl.SAMPLE_COVERAGE_INVERT,

        /// <summary>
        /// Strongly typed for value GL_SAMPLE_COVERAGE_VALUE, GL_SAMPLE_MASK_VALUE_SGIS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multisample")]
        SampleCoverageValue = Gl.SAMPLE_COVERAGE_VALUE,

        /// <summary>
        /// Strongly typed for value GL_SCISSOR_BOX.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_viewport_array", Api = "gl|glcore")]
        [RequiredByFeature("GL_NV_viewport_array", Api = "gles2")]
        [RequiredByFeature("GL_OES_viewport_array", Api = "gles2")]
        ScissorBox = Gl.SCISSOR_BOX,

        /// <summary>
        /// Strongly typed for value GL_SCISSOR_TEST.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_viewport_array", Api = "gl|glcore")]
        [RequiredByFeature("GL_NV_viewport_array", Api = "gles2")]
        [RequiredByFeature("GL_OES_viewport_array", Api = "gles2")]
        ScissorTest = Gl.SCISSOR_TEST,

        /// <summary>
        /// Strongly typed for value GL_SELECTION_BUFFER_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RemovedByFeature("GL_VERSION_3_2")]
        SelectionBufferSize = Gl.SELECTION_BUFFER_SIZE,

        /// <summary>
        /// Strongly typed for value GL_SHADER_COMPILER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_ES2_compatibility", Api = "gl|glcore")]
        ShaderCompiler = Gl.SHADER_COMPILER,

        /// <summary>
        /// Strongly typed for value GL_SHADER_STORAGE_BUFFER_BINDING.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_storage_buffer_object", Api = "gl|glcore")]
        ShaderStorageBufferBinding = Gl.SHADER_STORAGE_BUFFER_BINDING,

        /// <summary>
        /// Strongly typed for value GL_SHADER_STORAGE_BUFFER_OFFSET_ALIGNMENT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_storage_buffer_object", Api = "gl|glcore")]
        ShaderStorageBufferOffsetAlignment = Gl.SHADER_STORAGE_BUFFER_OFFSET_ALIGNMENT,

        /// <summary>
        /// Strongly typed for value GL_SHADER_STORAGE_BUFFER_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_storage_buffer_object", Api = "gl|glcore")]
        ShaderStorageBufferSize = Gl.SHADER_STORAGE_BUFFER_SIZE,

        /// <summary>
        /// Strongly typed for value GL_SHADER_STORAGE_BUFFER_START.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_storage_buffer_object", Api = "gl|glcore")]
        ShaderStorageBufferStart = Gl.SHADER_STORAGE_BUFFER_START,

        /// <summary>
        /// Strongly typed for value GL_STENCIL_BACK_FAIL.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ATI_separate_stencil")]
        StencilBackFail = Gl.STENCIL_BACK_FAIL,

        /// <summary>
        /// Strongly typed for value GL_STENCIL_BACK_FUNC.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ATI_separate_stencil")]
        StencilBackFunc = Gl.STENCIL_BACK_FUNC,

        /// <summary>
        /// Strongly typed for value GL_STENCIL_BACK_PASS_DEPTH_FAIL.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ATI_separate_stencil")]
        StencilBackPassDepthFail = Gl.STENCIL_BACK_PASS_DEPTH_FAIL,

        /// <summary>
        /// Strongly typed for value GL_STENCIL_BACK_PASS_DEPTH_PASS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ATI_separate_stencil")]
        StencilBackPassDepthPass = Gl.STENCIL_BACK_PASS_DEPTH_PASS,

        /// <summary>
        /// Strongly typed for value GL_STENCIL_BACK_REF.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        StencilBackRef = Gl.STENCIL_BACK_REF,

        /// <summary>
        /// Strongly typed for value GL_STENCIL_BACK_VALUE_MASK.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        StencilBackValueMask = Gl.STENCIL_BACK_VALUE_MASK,

        /// <summary>
        /// Strongly typed for value GL_STENCIL_BACK_WRITEMASK.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        StencilBackWritemask = Gl.STENCIL_BACK_WRITEMASK,

        /// <summary>
        /// Strongly typed for value GL_STENCIL_CLEAR_VALUE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        StencilClearValue = Gl.STENCIL_CLEAR_VALUE,

        /// <summary>
        /// Strongly typed for value GL_STENCIL_FAIL.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        StencilFail = Gl.STENCIL_FAIL,

        /// <summary>
        /// Strongly typed for value GL_STENCIL_FUNC.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        StencilFunc = Gl.STENCIL_FUNC,

        /// <summary>
        /// Strongly typed for value GL_STENCIL_PASS_DEPTH_FAIL.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        StencilPassDepthFail = Gl.STENCIL_PASS_DEPTH_FAIL,

        /// <summary>
        /// Strongly typed for value GL_STENCIL_PASS_DEPTH_PASS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        StencilPassDepthPass = Gl.STENCIL_PASS_DEPTH_PASS,

        /// <summary>
        /// Strongly typed for value GL_STENCIL_REF.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        StencilRef = Gl.STENCIL_REF,

        /// <summary>
        /// Strongly typed for value GL_STENCIL_TEST.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        StencilTest = Gl.STENCIL_TEST,

        /// <summary>
        /// Strongly typed for value GL_STENCIL_VALUE_MASK.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        StencilValueMask = Gl.STENCIL_VALUE_MASK,

        /// <summary>
        /// Strongly typed for value GL_STENCIL_WRITEMASK.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        StencilWritemask = Gl.STENCIL_WRITEMASK,

        /// <summary>
        /// Strongly typed for value GL_STEREO.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] Stereo = Gl.STEREO,

        /// <summary>
        /// Strongly typed for value GL_SUBPIXEL_BITS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        SubpixelBits = Gl.SUBPIXEL_BITS,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_1D.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        Texture1d = Gl.TEXTURE_1D,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_2D.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_sparse_texture", Api = "gles2")]
        Texture2d = Gl.TEXTURE_2D,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_3D_BINDING_EXT, GL_TEXTURE_BINDING_3D.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")]
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        [RequiredByFeature("GL_OES_texture_3D", Api = "gles2")]
        TextureBinding3d = Gl.TEXTURE_BINDING_3D,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_3D_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_sparse_texture", Api = "gles2")]
        [RequiredByFeature("GL_EXT_texture3D")]
        [RequiredByFeature("GL_OES_texture_3D", Api = "gles2")]
        Texture3d = Gl.TEXTURE_3D,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_BINDING_1D.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        TextureBinding1d = Gl.TEXTURE_BINDING_1D,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_BINDING_1D_ARRAY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_texture_array")]
        TextureBinding1dArray = Gl.TEXTURE_BINDING_1D_ARRAY,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_BINDING_2D.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        TextureBinding2d = Gl.TEXTURE_BINDING_2D,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_BINDING_2D_ARRAY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_texture_array")]
        TextureBinding2dArray = Gl.TEXTURE_BINDING_2D_ARRAY,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_BINDING_2D_MULTISAMPLE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_texture_multisample", Api = "gl|glcore")]
        TextureBinding2dMultisample = Gl.TEXTURE_BINDING_2D_MULTISAMPLE,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_BINDING_2D_MULTISAMPLE_ARRAY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_texture_multisample", Api = "gl|glcore")]
        [RequiredByFeature("GL_OES_texture_storage_multisample_2d_array", Api = "gles2")]
        TextureBinding2dMultisampleArray = Gl.TEXTURE_BINDING_2D_MULTISAMPLE_ARRAY,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_BINDING_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")]
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_texture_buffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_texture_buffer", Api = "gles2")]
        [RequiredByFeature("GL_EXT_texture_buffer_object")]
        [RequiredByFeature("GL_OES_texture_buffer", Api = "gles2")]
        TextureBindingBuffer = Gl.TEXTURE_BINDING_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_BINDING_CUBE_MAP.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_texture_cube_map")]
        [RequiredByFeature("GL_EXT_texture_cube_map")]
        [RequiredByFeature("GL_OES_texture_cube_map", Api = "gles1")]
        TextureBindingCubeMap = Gl.TEXTURE_BINDING_CUBE_MAP,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_BINDING_RECTANGLE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")]
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_texture_rectangle")]
        [RequiredByFeature("GL_NV_texture_rectangle")]
        TextureBindingRectangle = Gl.TEXTURE_BINDING_RECTANGLE,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_BUFFER_OFFSET_ALIGNMENT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_texture_buffer_range", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_texture_buffer", Api = "gles2")]
        [RequiredByFeature("GL_OES_texture_buffer", Api = "gles2")]
        TextureBufferOffsetAlignment = Gl.TEXTURE_BUFFER_OFFSET_ALIGNMENT,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_COMPRESSION_HINT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")] [RequiredByFeature("GL_ARB_texture_compression")]
        TextureCompressionHint = Gl.TEXTURE_COMPRESSION_HINT,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_COORD_ARRAY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        TextureCoordArray = Gl.TEXTURE_COORD_ARRAY,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_COORD_ARRAY_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        TextureCoordArraySize = Gl.TEXTURE_COORD_ARRAY_SIZE,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_COORD_ARRAY_STRIDE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        TextureCoordArrayStride = Gl.TEXTURE_COORD_ARRAY_STRIDE,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_COORD_ARRAY_TYPE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        TextureCoordArrayType = Gl.TEXTURE_COORD_ARRAY_TYPE,

        /// <summary>
        /// Strongly typed for value GL_TIMESTAMP.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_3")] [RequiredByFeature("GL_ARB_timer_query", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_disjoint_timer_query", Api = "gles2")]
        Timestamp = Gl.TIMESTAMP,

        /// <summary>
        /// Strongly typed for value GL_TRANSFORM_FEEDBACK_BUFFER_BINDING.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_transform_feedback")] [RequiredByFeature("GL_NV_transform_feedback")]
        TransformFeedbackBufferBinding = Gl.TRANSFORM_FEEDBACK_BUFFER_BINDING,

        /// <summary>
        /// Strongly typed for value GL_TRANSFORM_FEEDBACK_BUFFER_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_transform_feedback")] [RequiredByFeature("GL_NV_transform_feedback")]
        TransformFeedbackBufferSize = Gl.TRANSFORM_FEEDBACK_BUFFER_SIZE,

        /// <summary>
        /// Strongly typed for value GL_TRANSFORM_FEEDBACK_BUFFER_START.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_transform_feedback")] [RequiredByFeature("GL_NV_transform_feedback")]
        TransformFeedbackBufferStart = Gl.TRANSFORM_FEEDBACK_BUFFER_START,

        /// <summary>
        /// Strongly typed for value GL_UNIFORM_BUFFER_BINDING.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        UniformBufferBinding = Gl.UNIFORM_BUFFER_BINDING,

        /// <summary>
        /// Strongly typed for value GL_UNIFORM_BUFFER_OFFSET_ALIGNMENT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        UniformBufferOffsetAlignment = Gl.UNIFORM_BUFFER_OFFSET_ALIGNMENT,

        /// <summary>
        /// Strongly typed for value GL_UNIFORM_BUFFER_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        UniformBufferSize = Gl.UNIFORM_BUFFER_SIZE,

        /// <summary>
        /// Strongly typed for value GL_UNIFORM_BUFFER_START.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        UniformBufferStart = Gl.UNIFORM_BUFFER_START,

        /// <summary>
        /// Strongly typed for value GL_UNPACK_ALIGNMENT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        UnpackAlignment = Gl.UNPACK_ALIGNMENT,

        /// <summary>
        /// Strongly typed for value GL_UNPACK_IMAGE_HEIGHT, GL_UNPACK_IMAGE_HEIGHT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_texture3D")]
        UnpackImageHeight = Gl.UNPACK_IMAGE_HEIGHT,

        /// <summary>
        /// Strongly typed for value GL_UNPACK_LSB_FIRST.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] UnpackLsbFirst = Gl.UNPACK_LSB_FIRST,

        /// <summary>
        /// Strongly typed for value GL_UNPACK_ROW_LENGTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_unpack_subimage", Api = "gles2")]
        UnpackRowLength = Gl.UNPACK_ROW_LENGTH,

        /// <summary>
        /// Strongly typed for value GL_UNPACK_SKIP_IMAGES, GL_UNPACK_SKIP_IMAGES_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_texture3D")]
        UnpackSkipImages = Gl.UNPACK_SKIP_IMAGES,

        /// <summary>
        /// Strongly typed for value GL_UNPACK_SKIP_PIXELS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_unpack_subimage", Api = "gles2")]
        UnpackSkipPixels = Gl.UNPACK_SKIP_PIXELS,

        /// <summary>
        /// Strongly typed for value GL_UNPACK_SKIP_ROWS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_unpack_subimage", Api = "gles2")]
        UnpackSkipRows = Gl.UNPACK_SKIP_ROWS,

        /// <summary>
        /// Strongly typed for value GL_UNPACK_SWAP_BYTES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] UnpackSwapBytes = Gl.UNPACK_SWAP_BYTES,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_ARRAY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_EXT_vertex_array")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RemovedByFeature("GL_VERSION_3_2")]
        VertexArray = Gl.VERTEX_ARRAY,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_ARRAY_BINDING.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_vertex_array_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_APPLE_vertex_array_object")]
        [RequiredByFeature("GL_OES_vertex_array_object", Api = "gles1|gles2")]
        VertexArrayBinding = Gl.VERTEX_ARRAY_BINDING,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_ARRAY_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        VertexArraySize = Gl.VERTEX_ARRAY_SIZE,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_ARRAY_STRIDE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        VertexArrayStride = Gl.VERTEX_ARRAY_STRIDE,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_ARRAY_TYPE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_EXT_vertex_array")] [RemovedByFeature("GL_VERSION_3_2")]
        VertexArrayType = Gl.VERTEX_ARRAY_TYPE,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_BINDING_DIVISOR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_vertex_attrib_binding", Api = "gl|glcore")]
        VertexBindingDivisor = Gl.VERTEX_BINDING_DIVISOR,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_BINDING_OFFSET.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_vertex_attrib_binding", Api = "gl|glcore")]
        VertexBindingOffset = Gl.VERTEX_BINDING_OFFSET,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_BINDING_STRIDE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_vertex_attrib_binding", Api = "gl|glcore")]
        VertexBindingStride = Gl.VERTEX_BINDING_STRIDE,

        /// <summary>
        /// Strongly typed for value GL_VIEWPORT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_viewport_array", Api = "gl|glcore")]
        [RequiredByFeature("GL_NV_viewport_array", Api = "gles2")]
        [RequiredByFeature("GL_OES_viewport_array", Api = "gles2")]
        Viewport = Gl.VIEWPORT,

        /// <summary>
        /// Strongly typed for value GL_VIEWPORT_BOUNDS_RANGE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ARB_viewport_array", Api = "gl|glcore")]
        [RequiredByFeature("GL_NV_viewport_array", Api = "gles2")]
        [RequiredByFeature("GL_OES_viewport_array", Api = "gles2")]
        ViewportBoundsRange = Gl.VIEWPORT_BOUNDS_RANGE,

        /// <summary>
        /// Strongly typed for value GL_VIEWPORT_INDEX_PROVOKING_VERTEX.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ARB_viewport_array", Api = "gl|glcore")]
        [RequiredByFeature("GL_NV_viewport_array", Api = "gles2")]
        [RequiredByFeature("GL_OES_viewport_array", Api = "gles2")]
        ViewportIndexProvokingVertex = Gl.VIEWPORT_INDEX_PROVOKING_VERTEX,

        /// <summary>
        /// Strongly typed for value GL_VIEWPORT_SUBPIXEL_BITS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ARB_viewport_array", Api = "gl|glcore")]
        [RequiredByFeature("GL_NV_viewport_array", Api = "gles2")]
        [RequiredByFeature("GL_OES_viewport_array", Api = "gles2")]
        ViewportSubpixelBits = Gl.VIEWPORT_SUBPIXEL_BITS,

        /// <summary>
        /// Strongly typed for value GL_NUM_SHADING_LANGUAGE_VERSIONS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] NumShadingLanguageVersions = Gl.NUM_SHADING_LANGUAGE_VERSIONS
    }

    /// <summary>
    /// Strongly typed enumeration GetPointervPName.
    /// </summary>
    public enum GetPointervPName
    {
        /// <summary>
        /// Strongly typed for value GL_DEBUG_CALLBACK_FUNCTION.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_ARB_debug_output", Api = "gl|glcore")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        DebugCallbackFunction = Gl.DEBUG_CALLBACK_FUNCTION,

        /// <summary>
        /// Strongly typed for value GL_DEBUG_CALLBACK_USER_PARAM.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_ARB_debug_output", Api = "gl|glcore")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        DebugCallbackUserParam = Gl.DEBUG_CALLBACK_USER_PARAM
    }

    /// <summary>
    /// Strongly typed enumeration GetTextureParameter.
    /// </summary>
    public enum GetTextureParameter
    {
        /// <summary>
        /// Strongly typed for value GL_GENERATE_MIPMAP_SGIS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_SGIS_generate_mipmap")] [RemovedByFeature("GL_VERSION_3_2")]
        GenerateMipmap = Gl.GENERATE_MIPMAP,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_ALPHA_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_EXT_texture")]
        TextureAlphaSize = Gl.TEXTURE_ALPHA_SIZE,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_BASE_LEVEL_SGIS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_SGIS_texture_lod")]
        TextureBaseLevel = Gl.TEXTURE_BASE_LEVEL,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_BLUE_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_EXT_texture")]
        TextureBlueSize = Gl.TEXTURE_BLUE_SIZE,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_BORDER_COLOR, GL_TEXTURE_BORDER_COLOR_NV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_EXT_texture_border_clamp", Api = "gles2")]
        [RequiredByFeature("GL_NV_texture_border_clamp", Api = "gles2")]
        [RequiredByFeature("GL_OES_texture_border_clamp", Api = "gles2")]
        TextureBorderColor = Gl.TEXTURE_BORDER_COLOR,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_COMPONENTS, GL_TEXTURE_INTERNAL_FORMAT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        TextureInternalFormat = Gl.TEXTURE_INTERNAL_FORMAT,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_DEPTH_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_EXT_texture3D")]
        TextureDepth = Gl.TEXTURE_DEPTH,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_GREEN_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_EXT_texture")]
        TextureGreenSize = Gl.TEXTURE_GREEN_SIZE,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_HEIGHT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        TextureHeight = Gl.TEXTURE_HEIGHT,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_INTENSITY_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture")] [RemovedByFeature("GL_VERSION_3_2")]
        TextureIntensitySize = Gl.TEXTURE_INTENSITY_SIZE,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_LUMINANCE_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture")] [RemovedByFeature("GL_VERSION_3_2")]
        TextureLuminanceSize = Gl.TEXTURE_LUMINANCE_SIZE,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_MAG_FILTER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        TextureMagFilter = Gl.TEXTURE_MAG_FILTER,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_MAX_LEVEL_SGIS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_APPLE_texture_max_level", Api = "gles1|gles2")]
        [RequiredByFeature("GL_SGIS_texture_lod")]
        TextureMaxLevel = Gl.TEXTURE_MAX_LEVEL,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_MAX_LOD_SGIS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_SGIS_texture_lod")]
        TextureMaxLod = Gl.TEXTURE_MAX_LOD,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_MIN_FILTER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        TextureMinFilter = Gl.TEXTURE_MIN_FILTER,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_MIN_LOD_SGIS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_SGIS_texture_lod")]
        TextureMinLod = Gl.TEXTURE_MIN_LOD,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_PRIORITY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture_object")] [RemovedByFeature("GL_VERSION_3_2")]
        TexturePriority = Gl.TEXTURE_PRIORITY,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_RED_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_EXT_texture")]
        TextureRedSize = Gl.TEXTURE_RED_SIZE,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_RESIDENT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture_object")] [RemovedByFeature("GL_VERSION_3_2")]
        TextureResident = Gl.TEXTURE_RESIDENT,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_WIDTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        TextureWidth = Gl.TEXTURE_WIDTH,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_WRAP_R_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_texture3D")] [RequiredByFeature("GL_OES_texture_3D", Api = "gles2")]
        TextureWrapR = Gl.TEXTURE_WRAP_R,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_WRAP_S.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        TextureWrapS = Gl.TEXTURE_WRAP_S,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_WRAP_T.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        TextureWrapT = Gl.TEXTURE_WRAP_T
    }

    /// <summary>
    /// Strongly typed enumeration GraphicsResetStatus.
    /// </summary>
    public enum GraphicsResetStatus
    {
        /// <summary>
        /// Strongly typed for value GL_NO_ERROR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_robustness", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_robustness", Api = "gles1|gles2")]
        [RequiredByFeature("GL_KHR_robustness", Api = "gl|glcore|gles2")]
        NoError = Gl.NO_ERROR,

        /// <summary>
        /// Strongly typed for value GL_GUILTY_CONTEXT_RESET.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_KHR_robustness", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_ARB_robustness", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_robustness", Api = "gles1|gles2")]
        [RequiredByFeature("GL_KHR_robustness", Api = "gl|glcore|gles2")]
        GuiltyContextReset = Gl.GUILTY_CONTEXT_RESET,

        /// <summary>
        /// Strongly typed for value GL_INNOCENT_CONTEXT_RESET.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_KHR_robustness", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_ARB_robustness", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_robustness", Api = "gles1|gles2")]
        [RequiredByFeature("GL_KHR_robustness", Api = "gl|glcore|gles2")]
        InnocentContextReset = Gl.INNOCENT_CONTEXT_RESET,

        /// <summary>
        /// Strongly typed for value GL_UNKNOWN_CONTEXT_RESET.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_KHR_robustness", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_ARB_robustness", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_robustness", Api = "gles1|gles2")]
        [RequiredByFeature("GL_KHR_robustness", Api = "gl|glcore|gles2")]
        UnknownContextReset = Gl.UNKNOWN_CONTEXT_RESET
    }

    /// <summary>
    /// Strongly typed enumeration HintMode.
    /// </summary>
    public enum HintMode
    {
        /// <summary>
        /// Strongly typed for value GL_DONT_CARE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        DontCare = Gl.DONT_CARE,

        /// <summary>
        /// Strongly typed for value GL_FASTEST.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Fastest = Gl.FASTEST,

        /// <summary>
        /// Strongly typed for value GL_NICEST.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Nicest = Gl.NICEST
    }

    /// <summary>
    /// Strongly typed enumeration HintTarget.
    /// </summary>
    public enum HintTarget
    {
        /// <summary>
        /// Strongly typed for value GL_FRAGMENT_SHADER_DERIVATIVE_HINT, GL_FRAGMENT_SHADER_DERIVATIVE_HINT_ARB,
        /// GL_FRAGMENT_SHADER_DERIVATIVE_HINT_OES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_fragment_shader")]
        [RequiredByFeature("GL_OES_standard_derivatives", Api = "gles2|glsc2")]
        FragmentShaderDerivativeHint = Gl.FRAGMENT_SHADER_DERIVATIVE_HINT,

        /// <summary>
        /// Strongly typed for value GL_GENERATE_MIPMAP_HINT, GL_GENERATE_MIPMAP_HINT_SGIS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_SGIS_generate_mipmap")]
        [RemovedByFeature("GL_VERSION_3_2")]
        GenerateMipmapHint = Gl.GENERATE_MIPMAP_HINT,

        /// <summary>
        /// Strongly typed for value GL_LINE_SMOOTH_HINT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        LineSmoothHint = Gl.LINE_SMOOTH_HINT,

        /// <summary>
        /// Strongly typed for value GL_POLYGON_SMOOTH_HINT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] PolygonSmoothHint = Gl.POLYGON_SMOOTH_HINT,

        /// <summary>
        /// Strongly typed for value GL_PROGRAM_BINARY_RETRIEVABLE_HINT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_get_program_binary", Api = "gl|glcore")]
        ProgramBinaryRetrievableHint = Gl.PROGRAM_BINARY_RETRIEVABLE_HINT,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_COMPRESSION_HINT, GL_TEXTURE_COMPRESSION_HINT_ARB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")] [RequiredByFeature("GL_ARB_texture_compression")]
        TextureCompressionHint = Gl.TEXTURE_COMPRESSION_HINT
    }

    /// <summary>
    /// Strongly typed enumeration IndexPointerType.
    /// </summary>
    public enum IndexPointerType
    {
        /// <summary>
        /// Strongly typed for value GL_DOUBLE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_ARB_gpu_shader_fp64", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_vertex_attrib_64bit")]
        Double = Gl.DOUBLE,

        /// <summary>
        /// Strongly typed for value GL_FLOAT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_OES_texture_float", Api = "gles2")]
        Float = Gl.FLOAT,

        /// <summary>
        /// Strongly typed for value GL_INT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Int = Gl.INT,

        /// <summary>
        /// Strongly typed for value GL_SHORT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_render_snorm", Api = "gles2")]
        Short = Gl.SHORT
    }

    /// <summary>
    /// Strongly typed enumeration InternalFormat.
    /// </summary>
    public enum InternalFormat
    {
        /// <summary>
        /// Strongly typed for value GL_INTENSITY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture")] [RemovedByFeature("GL_VERSION_3_2")]
        Intensity = Gl.INTENSITY,

        /// <summary>
        /// Strongly typed for value GL_RED, GL_RED_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_AMD_interleaved_elements")]
        [RequiredByFeature("GL_EXT_texture_rg", Api = "gles2")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        Red = Gl.RED,

        /// <summary>
        /// Strongly typed for value GL_R8, GL_R8_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_texture_rg", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_texture_rg", Api = "gles2")]
        [RequiredByFeature("GL_EXT_texture_storage", Api = "gles1|gles2")]
        R8 = Gl.R8,

        /// <summary>
        /// Strongly typed for value GL_R8_SNORM.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_EXT_render_snorm", Api = "gles2")]
        [RequiredByFeature("GL_EXT_texture_snorm")]
        R8Snorm = Gl.R8_SNORM,

        /// <summary>
        /// Strongly typed for value GL_R16, GL_R16_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ARB_texture_rg", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_texture_norm16", Api = "gles2")]
        R16 = Gl.R16,

        /// <summary>
        /// Strongly typed for value GL_R16_SNORM, GL_R16_SNORM_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")]
        [RequiredByFeature("GL_EXT_texture_snorm")]
        [RequiredByFeature("GL_EXT_render_snorm", Api = "gles2")]
        [RequiredByFeature("GL_EXT_texture_norm16", Api = "gles2")]
        R16Snorm = Gl.R16_SNORM,

        /// <summary>
        /// Strongly typed for value GL_R16F, GL_R16F_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_texture_rg", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_color_buffer_half_float", Api = "gles2")]
        [RequiredByFeature("GL_EXT_texture_storage", Api = "gles1|gles2")]
        R16F = Gl.R16F,

        /// <summary>
        /// Strongly typed for value GL_R32F, GL_R32F_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_texture_rg", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_texture_storage", Api = "gles1|gles2")]
        R32F = Gl.R32F,

        /// <summary>
        /// Strongly typed for value GL_R8I.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_texture_rg", Api = "gl|glcore")]
        R8I = Gl.R8I,

        /// <summary>
        /// Strongly typed for value GL_R16I.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_texture_rg", Api = "gl|glcore")]
        R16I = Gl.R16I,

        /// <summary>
        /// Strongly typed for value GL_R32I.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_texture_rg", Api = "gl|glcore")]
        R32I = Gl.R32I,

        /// <summary>
        /// Strongly typed for value GL_R8UI.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_texture_rg", Api = "gl|glcore")]
        R8UI = Gl.R8UI,

        /// <summary>
        /// Strongly typed for value GL_R16UI.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_texture_rg", Api = "gl|glcore")]
        R16UI = Gl.R16UI,

        /// <summary>
        /// Strongly typed for value GL_R32UI.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_texture_rg", Api = "gl|glcore")]
        R32UI = Gl.R32UI,

        /// <summary>
        /// Strongly typed for value GL_RG.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_texture_rg", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_texture_rg", Api = "gles2")]
        Rg = Gl.RG,

        /// <summary>
        /// Strongly typed for value GL_RG8, GL_RG8_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_texture_rg", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_texture_rg", Api = "gles2")]
        [RequiredByFeature("GL_EXT_texture_storage", Api = "gles1|gles2")]
        Rg8 = Gl.RG8,

        /// <summary>
        /// Strongly typed for value GL_RG8_SNORM.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_EXT_render_snorm", Api = "gles2")]
        [RequiredByFeature("GL_EXT_texture_snorm")]
        Rg8Snorm = Gl.RG8_SNORM,

        /// <summary>
        /// Strongly typed for value GL_RG16, GL_RG16_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ARB_texture_rg", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_texture_norm16", Api = "gles2")]
        Rg16 = Gl.RG16,

        /// <summary>
        /// Strongly typed for value GL_RG16_SNORM, GL_RG16_SNORM_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")]
        [RequiredByFeature("GL_EXT_texture_snorm")]
        [RequiredByFeature("GL_EXT_render_snorm", Api = "gles2")]
        [RequiredByFeature("GL_EXT_texture_norm16", Api = "gles2")]
        Rg16Snorm = Gl.RG16_SNORM,

        /// <summary>
        /// Strongly typed for value GL_RG16F, GL_RG16F_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_texture_rg", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_color_buffer_half_float", Api = "gles2")]
        [RequiredByFeature("GL_EXT_texture_storage", Api = "gles1|gles2")]
        Rg16F = Gl.RG16F,

        /// <summary>
        /// Strongly typed for value GL_RG32F, GL_RG32F_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_texture_rg", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_texture_storage", Api = "gles1|gles2")]
        Rg32F = Gl.RG32F,

        /// <summary>
        /// Strongly typed for value GL_RG8I.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_texture_rg", Api = "gl|glcore")]
        Rg8I = Gl.RG8I,

        /// <summary>
        /// Strongly typed for value GL_RG16I.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_texture_rg", Api = "gl|glcore")]
        Rg16I = Gl.RG16I,

        /// <summary>
        /// Strongly typed for value GL_RG32I.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_texture_rg", Api = "gl|glcore")]
        Rg32I = Gl.RG32I,

        /// <summary>
        /// Strongly typed for value GL_RG8UI.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_AMD_interleaved_elements")]
        [RequiredByFeature("GL_ARB_texture_rg", Api = "gl|glcore")]
        Rg8UI = Gl.RG8UI,

        /// <summary>
        /// Strongly typed for value GL_RG16UI.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_AMD_interleaved_elements")]
        [RequiredByFeature("GL_ARB_texture_rg", Api = "gl|glcore")]
        Rg16UI = Gl.RG16UI,

        /// <summary>
        /// Strongly typed for value GL_RG32UI.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_texture_rg", Api = "gl|glcore")]
        Rg32UI = Gl.RG32UI,

        /// <summary>
        /// Strongly typed for value GL_RGB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Rgb = Gl.RGB,

        /// <summary>
        /// Strongly typed for value GL_RGB4, GL_RGB4_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture")]
        Rgb4 = Gl.RGB4,

        /// <summary>
        /// Strongly typed for value GL_RGB5, GL_RGB5_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture")]
        Rgb5 = Gl.RGB5,

        /// <summary>
        /// Strongly typed for value GL_RGB8, GL_RGB8_EXT, GL_RGB8_OES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_texture")]
        [RequiredByFeature("GL_OES_required_internalformat", Api = "gles1|gles2")]
        [RequiredByFeature("GL_OES_rgb8_rgba8", Api = "gles1|gles2|glsc2")]
        Rgb8 = Gl.RGB8,

        /// <summary>
        /// Strongly typed for value GL_RGB8_SNORM.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_texture_snorm")]
        Rgb8Snorm = Gl.RGB8_SNORM,

        /// <summary>
        /// Strongly typed for value GL_RGB10, GL_RGB10_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_EXT_texture")]
        [RequiredByFeature("GL_EXT_texture_storage", Api = "gles1|gles2")]
        [RequiredByFeature("GL_OES_required_internalformat", Api = "gles1|gles2")]
        Rgb10 = Gl.RGB10,

        /// <summary>
        /// Strongly typed for value GL_RGB12, GL_RGB12_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture")]
        Rgb12 = Gl.RGB12,

        /// <summary>
        /// Strongly typed for value GL_RGB16, GL_RGB16_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture")] [RequiredByFeature("GL_EXT_texture_norm16", Api = "gles2")]
        Rgb16 = Gl.RGB16,

        /// <summary>
        /// Strongly typed for value GL_RGB16F, GL_RGB16F_ARB, GL_RGB16F_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_texture_float")]
        [RequiredByFeature("GL_EXT_color_buffer_half_float", Api = "gles2")]
        [RequiredByFeature("GL_EXT_texture_storage", Api = "gles1|gles2")]
        Rgb16F = Gl.RGB16F,

        /// <summary>
        /// Strongly typed for value GL_RGB16_SNORM, GL_RGB16_SNORM_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_EXT_texture_snorm")] [RequiredByFeature("GL_EXT_texture_norm16", Api = "gles2")]
        Rgb16Snorm = Gl.RGB16_SNORM,

        /// <summary>
        /// Strongly typed for value GL_RGB8I.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_texture_integer")]
        Rgb8I = Gl.RGB8I,

        /// <summary>
        /// Strongly typed for value GL_RGB16I.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_texture_integer")]
        Rgb16I = Gl.RGB16I,

        /// <summary>
        /// Strongly typed for value GL_RGB32I.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_texture_buffer_object_rgb32", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_vertex_attrib_64bit", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_texture_integer")]
        Rgb32I = Gl.RGB32I,

        /// <summary>
        /// Strongly typed for value GL_RGB8UI.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_texture_integer")]
        Rgb8UI = Gl.RGB8UI,

        /// <summary>
        /// Strongly typed for value GL_RGB16UI.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_texture_integer")]
        Rgb16UI = Gl.RGB16UI,

        /// <summary>
        /// Strongly typed for value GL_RGB32UI.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_texture_buffer_object_rgb32", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_texture_integer")]
        Rgb32UI = Gl.RGB32UI,

        /// <summary>
        /// Strongly typed for value GL_SRGB, GL_SRGB_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_sRGB", Api = "gles1|gles2")] [RequiredByFeature("GL_EXT_texture_sRGB")]
        Srgb = Gl.SRGB,

        /// <summary>
        /// Strongly typed for value GL_SRGB_ALPHA, GL_SRGB_ALPHA_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_1")] [RequiredByFeature("GL_EXT_sRGB", Api = "gles1|gles2")] [RequiredByFeature("GL_EXT_texture_sRGB")]
        SrgbAlpha = Gl.SRGB_ALPHA,

        /// <summary>
        /// Strongly typed for value GL_SRGB8, GL_SRGB8_EXT, GL_SRGB8_NV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_1")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_EXT_texture_sRGB")]
        [RequiredByFeature("GL_NV_sRGB_formats", Api = "gles2")]
        Srgb8 = Gl.SRGB8,

        /// <summary>
        /// Strongly typed for value GL_SRGB8_ALPHA8, GL_SRGB8_ALPHA8_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_sRGB", Api = "gles1|gles2")] [RequiredByFeature("GL_EXT_texture_sRGB")]
        Srgb8Alpha8 = Gl.SRGB8_ALPHA8,

        /// <summary>
        /// Strongly typed for value GL_R3_G3_B2.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] R3G3B2 = Gl.R3_G3_B2,

        /// <summary>
        /// Strongly typed for value GL_R11F_G11F_B10F, GL_R11F_G11F_B10F_APPLE, GL_R11F_G11F_B10F_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_APPLE_texture_packed_float", Api = "gles2")]
        [RequiredByFeature("GL_EXT_packed_float")]
        R11FG11FB10F = Gl.R11F_G11F_B10F,

        /// <summary>
        /// Strongly typed for value GL_RGB9_E5, GL_RGB9_E5_APPLE, GL_RGB9_E5_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_APPLE_texture_packed_float", Api = "gles2")]
        [RequiredByFeature("GL_EXT_texture_shared_exponent")]
        Rgb9E5 = Gl.RGB9_E5,

        /// <summary>
        /// Strongly typed for value GL_RGBA.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Rgba = Gl.RGBA,

        /// <summary>
        /// Strongly typed for value GL_RGBA4, GL_RGBA4_EXT, GL_RGBA4_OES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_texture")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        [RequiredByFeature("GL_OES_required_internalformat", Api = "gles1|gles2")]
        Rgba4 = Gl.RGBA4,

        /// <summary>
        /// Strongly typed for value GL_RGB5_A1, GL_RGB5_A1_EXT, GL_RGB5_A1_OES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_texture")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        [RequiredByFeature("GL_OES_required_internalformat", Api = "gles1|gles2")]
        Rgb5A1 = Gl.RGB5_A1,

        /// <summary>
        /// Strongly typed for value GL_RGBA8, GL_RGBA8_EXT, GL_RGBA8_OES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_texture")]
        [RequiredByFeature("GL_OES_required_internalformat", Api = "gles1|gles2")]
        [RequiredByFeature("GL_OES_rgb8_rgba8", Api = "gles1|gles2|glsc2")]
        Rgba8 = Gl.RGBA8,

        /// <summary>
        /// Strongly typed for value GL_RGBA8_SNORM.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_EXT_render_snorm", Api = "gles2")]
        [RequiredByFeature("GL_EXT_texture_snorm")]
        Rgba8Snorm = Gl.RGBA8_SNORM,

        /// <summary>
        /// Strongly typed for value GL_RGB10_A2, GL_RGB10_A2_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_EXT_texture")]
        [RequiredByFeature("GL_EXT_texture_storage", Api = "gles1|gles2")]
        [RequiredByFeature("GL_OES_required_internalformat", Api = "gles1|gles2")]
        Rgb10A2 = Gl.RGB10_A2,

        /// <summary>
        /// Strongly typed for value GL_RGBA12, GL_RGBA12_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture")]
        Rgba12 = Gl.RGBA12,

        /// <summary>
        /// Strongly typed for value GL_RGBA16, GL_RGBA16_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture")] [RequiredByFeature("GL_EXT_texture_norm16", Api = "gles2")]
        Rgba16 = Gl.RGBA16,

        /// <summary>
        /// Strongly typed for value GL_RGBA16F, GL_RGBA16F_ARB, GL_RGBA16F_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_texture_float")]
        [RequiredByFeature("GL_EXT_color_buffer_half_float", Api = "gles2")]
        [RequiredByFeature("GL_EXT_texture_storage", Api = "gles1|gles2")]
        Rgba16F = Gl.RGBA16F,

        /// <summary>
        /// Strongly typed for value GL_RGBA32F, GL_RGBA32F_ARB, GL_RGBA32F_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_texture_float")]
        [RequiredByFeature("GL_EXT_texture_storage", Api = "gles1|gles2")]
        Rgba32F = Gl.RGBA32F,

        /// <summary>
        /// Strongly typed for value GL_RGBA8I.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_texture_integer")]
        Rgba8I = Gl.RGBA8I,

        /// <summary>
        /// Strongly typed for value GL_RGBA16I.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_texture_integer")]
        Rgba16I = Gl.RGBA16I,

        /// <summary>
        /// Strongly typed for value GL_RGBA32I.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_texture_integer")]
        Rgba32I = Gl.RGBA32I,

        /// <summary>
        /// Strongly typed for value GL_RGBA8UI.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_AMD_interleaved_elements")] [RequiredByFeature("GL_EXT_texture_integer")]
        Rgba8UI = Gl.RGBA8UI,

        /// <summary>
        /// Strongly typed for value GL_RGBA16UI.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_texture_integer")]
        Rgba16UI = Gl.RGBA16UI,

        /// <summary>
        /// Strongly typed for value GL_RGBA32UI.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_texture_integer")]
        Rgba32UI = Gl.RGBA32UI,

        /// <summary>
        /// Strongly typed for value GL_RGB10_A2UI.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_3")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_texture_rgb10_a2ui", Api = "gl|glcore")]
        Rgb10A2UI = Gl.RGB10_A2UI,

        /// <summary>
        /// Strongly typed for value GL_DEPTH_COMPONENT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ANGLE_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_depth_texture", Api = "gles2")]
        DepthComponent = Gl.DEPTH_COMPONENT,

        /// <summary>
        /// Strongly typed for value GL_DEPTH_COMPONENT16, GL_DEPTH_COMPONENT16_ARB, GL_DEPTH_COMPONENT16_OES,
        /// GL_DEPTH_COMPONENT16_SGIX.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ANGLE_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_ARB_depth_texture")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        [RequiredByFeature("GL_OES_required_internalformat", Api = "gles1|gles2")]
        [RequiredByFeature("GL_SGIX_depth_texture")]
        DepthComponent16 = Gl.DEPTH_COMPONENT16,

        /// <summary>
        /// Strongly typed for value GL_DEPTH_COMPONENT24, GL_DEPTH_COMPONENT24_ARB, GL_DEPTH_COMPONENT24_OES,
        /// GL_DEPTH_COMPONENT24_SGIX.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_depth_texture")]
        [RequiredByFeature("GL_OES_depth24", Api = "gles1|gles2|glsc2")]
        [RequiredByFeature("GL_OES_required_internalformat", Api = "gles1|gles2")]
        [RequiredByFeature("GL_SGIX_depth_texture")]
        DepthComponent24 = Gl.DEPTH_COMPONENT24,

        /// <summary>
        /// Strongly typed for value GL_DEPTH_COMPONENT32_ARB, GL_DEPTH_COMPONENT32_OES, GL_DEPTH_COMPONENT32_SGIX,
        /// GL_DEPTH_COMPONENT32.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")]
        [RequiredByFeature("GL_ARB_depth_texture")]
        [RequiredByFeature("GL_ANGLE_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_depth32", Api = "gles1|gles2|glsc2")]
        [RequiredByFeature("GL_OES_required_internalformat", Api = "gles1|gles2")]
        [RequiredByFeature("GL_SGIX_depth_texture")]
        DepthComponent32 = Gl.DEPTH_COMPONENT32,

        /// <summary>
        /// Strongly typed for value GL_DEPTH_COMPONENT32F.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_depth_buffer_float", Api = "gl|glcore")]
        DepthComponent32F = Gl.DEPTH_COMPONENT32F,

        /// <summary>
        /// Strongly typed for value GL_DEPTH_STENCIL, GL_DEPTH_STENCIL_EXT, GL_DEPTH_STENCIL_NV, GL_DEPTH_STENCIL_OES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_packed_depth_stencil")]
        [RequiredByFeature("GL_NV_packed_depth_stencil")]
        [RequiredByFeature("GL_ANGLE_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_packed_depth_stencil", Api = "gles1|gles2")]
        DepthStencil = Gl.DEPTH_STENCIL,

        /// <summary>
        /// Strongly typed for value GL_DEPTH24_STENCIL8, GL_DEPTH24_STENCIL8_EXT, GL_DEPTH24_STENCIL8_OES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_packed_depth_stencil")]
        [RequiredByFeature("GL_ANGLE_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_packed_depth_stencil", Api = "gles1|gles2")]
        [RequiredByFeature("GL_OES_required_internalformat", Api = "gles1|gles2")]
        Depth24Stencil8 = Gl.DEPTH24_STENCIL8,

        /// <summary>
        /// Strongly typed for value GL_DEPTH32F_STENCIL8.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_depth_buffer_float", Api = "gl|glcore")]
        Depth32FStencil8 = Gl.DEPTH32F_STENCIL8,

        /// <summary>
        /// Strongly typed for value GL_COMPRESSED_RED.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] CompressedRed = Gl.COMPRESSED_RED,

        /// <summary>
        /// Strongly typed for value GL_COMPRESSED_RG.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] CompressedRg = Gl.COMPRESSED_RG,

        /// <summary>
        /// Strongly typed for value GL_COMPRESSED_RGB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")] [RequiredByFeature("GL_ARB_texture_compression")]
        CompressedRgb = Gl.COMPRESSED_RGB,

        /// <summary>
        /// Strongly typed for value GL_COMPRESSED_RGBA.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")] [RequiredByFeature("GL_ARB_texture_compression")]
        CompressedRgba = Gl.COMPRESSED_RGBA,

        /// <summary>
        /// Strongly typed for value GL_COMPRESSED_SRGB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_1")] [RequiredByFeature("GL_EXT_texture_sRGB")]
        CompressedSrgb = Gl.COMPRESSED_SRGB,

        /// <summary>
        /// Strongly typed for value GL_COMPRESSED_SRGB_ALPHA.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_1")] [RequiredByFeature("GL_EXT_texture_sRGB")]
        CompressedSrgbAlpha = Gl.COMPRESSED_SRGB_ALPHA,

        /// <summary>
        /// Strongly typed for value GL_COMPRESSED_RED_RGTC1, GL_COMPRESSED_RED_RGTC1_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ARB_texture_compression_rgtc", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_texture_compression_rgtc", Api = "gl|gles2")]
        CompressedRedRgtc1 = Gl.COMPRESSED_RED_RGTC1,

        /// <summary>
        /// Strongly typed for value GL_COMPRESSED_SIGNED_RED_RGTC1, GL_COMPRESSED_SIGNED_RED_RGTC1_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ARB_texture_compression_rgtc", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_texture_compression_rgtc", Api = "gl|gles2")]
        CompressedSignedRedRgtc1 = Gl.COMPRESSED_SIGNED_RED_RGTC1,

        /// <summary>
        /// Strongly typed for value GL_COMPRESSED_R11_EAC.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_ES3_compatibility", Api = "gl|glcore")]
        CompressedR11Eac = Gl.COMPRESSED_R11_EAC,

        /// <summary>
        /// Strongly typed for value GL_COMPRESSED_SIGNED_R11_EAC.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_ES3_compatibility", Api = "gl|glcore")]
        CompressedSignedR11Eac = Gl.COMPRESSED_SIGNED_R11_EAC,

        /// <summary>
        /// Strongly typed for value GL_COMPRESSED_RG_RGTC2.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ARB_texture_compression_rgtc", Api = "gl|glcore")]
        CompressedRgRgtc2 = Gl.COMPRESSED_RG_RGTC2,

        /// <summary>
        /// Strongly typed for value GL_COMPRESSED_SIGNED_RG_RGTC2.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ARB_texture_compression_rgtc", Api = "gl|glcore")]
        CompressedSignedRgRgtc2 = Gl.COMPRESSED_SIGNED_RG_RGTC2,

        /// <summary>
        /// Strongly typed for value GL_COMPRESSED_RGBA_BPTC_UNORM.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")] [RequiredByFeature("GL_ARB_texture_compression_bptc", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_texture_compression_bptc", Api = "gles2")]
        CompressedRgbaBptcUnorm = Gl.COMPRESSED_RGBA_BPTC_UNORM,

        /// <summary>
        /// Strongly typed for value GL_COMPRESSED_SRGB_ALPHA_BPTC_UNORM.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")] [RequiredByFeature("GL_ARB_texture_compression_bptc", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_texture_compression_bptc", Api = "gles2")]
        CompressedSrgbAlphaBptcUnorm = Gl.COMPRESSED_SRGB_ALPHA_BPTC_UNORM,

        /// <summary>
        /// Strongly typed for value GL_COMPRESSED_RGB_BPTC_SIGNED_FLOAT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")] [RequiredByFeature("GL_ARB_texture_compression_bptc", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_texture_compression_bptc", Api = "gles2")]
        CompressedRgbBptcSignedFloat = Gl.COMPRESSED_RGB_BPTC_SIGNED_FLOAT,

        /// <summary>
        /// Strongly typed for value GL_COMPRESSED_RGB_BPTC_UNSIGNED_FLOAT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")] [RequiredByFeature("GL_ARB_texture_compression_bptc", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_texture_compression_bptc", Api = "gles2")]
        CompressedRgbBptcUnsignedFloat = Gl.COMPRESSED_RGB_BPTC_UNSIGNED_FLOAT,

        /// <summary>
        /// Strongly typed for value GL_COMPRESSED_RGB8_ETC2.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_ES3_compatibility", Api = "gl|glcore")]
        CompressedRgb8Etc2 = Gl.COMPRESSED_RGB8_ETC2,

        /// <summary>
        /// Strongly typed for value GL_COMPRESSED_SRGB8_ETC2.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_ES3_compatibility", Api = "gl|glcore")]
        CompressedSrgb8Etc2 = Gl.COMPRESSED_SRGB8_ETC2,

        /// <summary>
        /// Strongly typed for value GL_COMPRESSED_RGB8_PUNCHTHROUGH_ALPHA1_ETC2.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_ES3_compatibility", Api = "gl|glcore")]
        CompressedRgb8PunchthroughAlpha1Etc2 = Gl.COMPRESSED_RGB8_PUNCHTHROUGH_ALPHA1_ETC2,

        /// <summary>
        /// Strongly typed for value GL_COMPRESSED_SRGB8_PUNCHTHROUGH_ALPHA1_ETC2.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_ES3_compatibility", Api = "gl|glcore")]
        CompressedSrgb8PunchthroughAlpha1Etc2 = Gl.COMPRESSED_SRGB8_PUNCHTHROUGH_ALPHA1_ETC2,

        /// <summary>
        /// Strongly typed for value GL_COMPRESSED_RGBA8_ETC2_EAC.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_ES3_compatibility", Api = "gl|glcore")]
        CompressedRgba8Etc2Eac = Gl.COMPRESSED_RGBA8_ETC2_EAC,

        /// <summary>
        /// Strongly typed for value GL_COMPRESSED_SRGB8_ALPHA8_ETC2_EAC.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_ES3_compatibility", Api = "gl|glcore")]
        CompressedSrgb8Alpha8Etc2Eac = Gl.COMPRESSED_SRGB8_ALPHA8_ETC2_EAC,

        /// <summary>
        /// Strongly typed for value GL_COMPRESSED_RG11_EAC.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_ES3_compatibility", Api = "gl|glcore")]
        CompressedRg11Eac = Gl.COMPRESSED_RG11_EAC,

        /// <summary>
        /// Strongly typed for value GL_COMPRESSED_SIGNED_RG11_EAC.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_ES3_compatibility", Api = "gl|glcore")]
        CompressedSignedRg11Eac = Gl.COMPRESSED_SIGNED_RG11_EAC
    }

    /// <summary>
    /// Strongly typed enumeration InternalFormatPName.
    /// </summary>
    public enum InternalFormatPName
    {
        /// <summary>
        /// Strongly typed for value GL_NUM_SAMPLE_COUNTS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_internalformat_query", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        NumSampleCounts = Gl.NUM_SAMPLE_COUNTS,

        /// <summary>
        /// Strongly typed for value GL_SAMPLES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_multisample")]
        [RequiredByFeature("GL_NV_multisample_coverage")]
        [RequiredByFeature("GL_EXT_multisample")]
        [RequiredByFeature("GL_SGIS_multisample")]
        Samples = Gl.SAMPLES,

        /// <summary>
        /// Strongly typed for value GL_INTERNALFORMAT_SUPPORTED.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        InternalformatSupported = Gl.INTERNALFORMAT_SUPPORTED,

        /// <summary>
        /// Strongly typed for value GL_INTERNALFORMAT_PREFERRED.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        InternalformatPreferred = Gl.INTERNALFORMAT_PREFERRED,

        /// <summary>
        /// Strongly typed for value GL_INTERNALFORMAT_RED_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        InternalformatRedSize = Gl.INTERNALFORMAT_RED_SIZE,

        /// <summary>
        /// Strongly typed for value GL_INTERNALFORMAT_GREEN_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        InternalformatGreenSize = Gl.INTERNALFORMAT_GREEN_SIZE,

        /// <summary>
        /// Strongly typed for value GL_INTERNALFORMAT_BLUE_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        InternalformatBlueSize = Gl.INTERNALFORMAT_BLUE_SIZE,

        /// <summary>
        /// Strongly typed for value GL_INTERNALFORMAT_ALPHA_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        InternalformatAlphaSize = Gl.INTERNALFORMAT_ALPHA_SIZE,

        /// <summary>
        /// Strongly typed for value GL_INTERNALFORMAT_DEPTH_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        InternalformatDepthSize = Gl.INTERNALFORMAT_DEPTH_SIZE,

        /// <summary>
        /// Strongly typed for value GL_INTERNALFORMAT_STENCIL_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        InternalformatStencilSize = Gl.INTERNALFORMAT_STENCIL_SIZE,

        /// <summary>
        /// Strongly typed for value GL_INTERNALFORMAT_SHARED_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        InternalformatSharedSize = Gl.INTERNALFORMAT_SHARED_SIZE,

        /// <summary>
        /// Strongly typed for value GL_INTERNALFORMAT_RED_TYPE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        InternalformatRedType = Gl.INTERNALFORMAT_RED_TYPE,

        /// <summary>
        /// Strongly typed for value GL_INTERNALFORMAT_GREEN_TYPE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        InternalformatGreenType = Gl.INTERNALFORMAT_GREEN_TYPE,

        /// <summary>
        /// Strongly typed for value GL_INTERNALFORMAT_BLUE_TYPE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        InternalformatBlueType = Gl.INTERNALFORMAT_BLUE_TYPE,

        /// <summary>
        /// Strongly typed for value GL_INTERNALFORMAT_ALPHA_TYPE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        InternalformatAlphaType = Gl.INTERNALFORMAT_ALPHA_TYPE,

        /// <summary>
        /// Strongly typed for value GL_INTERNALFORMAT_DEPTH_TYPE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        InternalformatDepthType = Gl.INTERNALFORMAT_DEPTH_TYPE,

        /// <summary>
        /// Strongly typed for value GL_INTERNALFORMAT_STENCIL_TYPE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        InternalformatStencilType = Gl.INTERNALFORMAT_STENCIL_TYPE,

        /// <summary>
        /// Strongly typed for value GL_MAX_WIDTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        MaxWidth = Gl.MAX_WIDTH,

        /// <summary>
        /// Strongly typed for value GL_MAX_HEIGHT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        MaxHeight = Gl.MAX_HEIGHT,

        /// <summary>
        /// Strongly typed for value GL_MAX_DEPTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        MaxDepth = Gl.MAX_DEPTH,

        /// <summary>
        /// Strongly typed for value GL_MAX_LAYERS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        MaxLayers = Gl.MAX_LAYERS,

        /// <summary>
        /// Strongly typed for value GL_COLOR_COMPONENTS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        ColorComponents = Gl.COLOR_COMPONENTS,

        /// <summary>
        /// Strongly typed for value GL_COLOR_RENDERABLE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        ColorRenderable = Gl.COLOR_RENDERABLE,

        /// <summary>
        /// Strongly typed for value GL_DEPTH_RENDERABLE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        DepthRenderable = Gl.DEPTH_RENDERABLE,

        /// <summary>
        /// Strongly typed for value GL_STENCIL_RENDERABLE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        StencilRenderable = Gl.STENCIL_RENDERABLE,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_RENDERABLE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        FramebufferRenderable = Gl.FRAMEBUFFER_RENDERABLE,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_RENDERABLE_LAYERED.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        FramebufferRenderableLayered = Gl.FRAMEBUFFER_RENDERABLE_LAYERED,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_BLEND.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        FramebufferBlend = Gl.FRAMEBUFFER_BLEND,

        /// <summary>
        /// Strongly typed for value GL_READ_PIXELS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        ReadPixels = Gl.READ_PIXELS,

        /// <summary>
        /// Strongly typed for value GL_READ_PIXELS_FORMAT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        ReadPixelsFormat = Gl.READ_PIXELS_FORMAT,

        /// <summary>
        /// Strongly typed for value GL_READ_PIXELS_TYPE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        ReadPixelsType = Gl.READ_PIXELS_TYPE,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_IMAGE_FORMAT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        TextureImageFormat = Gl.TEXTURE_IMAGE_FORMAT,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_IMAGE_TYPE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        TextureImageType = Gl.TEXTURE_IMAGE_TYPE,

        /// <summary>
        /// Strongly typed for value GL_GET_TEXTURE_IMAGE_FORMAT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        GetTextureImageFormat = Gl.GET_TEXTURE_IMAGE_FORMAT,

        /// <summary>
        /// Strongly typed for value GL_GET_TEXTURE_IMAGE_TYPE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        GetTextureImageType = Gl.GET_TEXTURE_IMAGE_TYPE,

        /// <summary>
        /// Strongly typed for value GL_MIPMAP.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        Mipmap = Gl.MIPMAP,

        /// <summary>
        /// Strongly typed for value GL_GENERATE_MIPMAP.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_SGIS_generate_mipmap")] [RemovedByFeature("GL_VERSION_3_2")]
        GenerateMipmap = Gl.GENERATE_MIPMAP,

        /// <summary>
        /// Strongly typed for value GL_AUTO_GENERATE_MIPMAP.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        AutoGenerateMipmap = Gl.AUTO_GENERATE_MIPMAP,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ENCODING.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        ColorEncoding = Gl.COLOR_ENCODING,

        /// <summary>
        /// Strongly typed for value GL_SRGB_READ.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        SrgbRead = Gl.SRGB_READ,

        /// <summary>
        /// Strongly typed for value GL_SRGB_WRITE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        SrgbWrite = Gl.SRGB_WRITE,

        /// <summary>
        /// Strongly typed for value GL_FILTER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        Filter = Gl.FILTER,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_TEXTURE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        VertexTexture = Gl.VERTEX_TEXTURE,

        /// <summary>
        /// Strongly typed for value GL_TESS_CONTROL_TEXTURE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        TessControlTexture = Gl.TESS_CONTROL_TEXTURE,

        /// <summary>
        /// Strongly typed for value GL_TESS_EVALUATION_TEXTURE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        TessEvaluationTexture = Gl.TESS_EVALUATION_TEXTURE,

        /// <summary>
        /// Strongly typed for value GL_GEOMETRY_TEXTURE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        GeometryTexture = Gl.GEOMETRY_TEXTURE,

        /// <summary>
        /// Strongly typed for value GL_FRAGMENT_TEXTURE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        FragmentTexture = Gl.FRAGMENT_TEXTURE,

        /// <summary>
        /// Strongly typed for value GL_COMPUTE_TEXTURE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        ComputeTexture = Gl.COMPUTE_TEXTURE,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_SHADOW.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        TextureShadow = Gl.TEXTURE_SHADOW,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_GATHER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        TextureGather = Gl.TEXTURE_GATHER,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_GATHER_SHADOW.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        TextureGatherShadow = Gl.TEXTURE_GATHER_SHADOW,

        /// <summary>
        /// Strongly typed for value GL_SHADER_IMAGE_LOAD.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        ShaderImageLoad = Gl.SHADER_IMAGE_LOAD,

        /// <summary>
        /// Strongly typed for value GL_SHADER_IMAGE_STORE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        ShaderImageStore = Gl.SHADER_IMAGE_STORE,

        /// <summary>
        /// Strongly typed for value GL_SHADER_IMAGE_ATOMIC.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        ShaderImageAtomic = Gl.SHADER_IMAGE_ATOMIC,

        /// <summary>
        /// Strongly typed for value GL_IMAGE_TEXEL_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        ImageTexelSize = Gl.IMAGE_TEXEL_SIZE,

        /// <summary>
        /// Strongly typed for value GL_IMAGE_COMPATIBILITY_CLASS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        ImageCompatibilityClass = Gl.IMAGE_COMPATIBILITY_CLASS,

        /// <summary>
        /// Strongly typed for value GL_IMAGE_PIXEL_FORMAT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        ImagePixelFormat = Gl.IMAGE_PIXEL_FORMAT,

        /// <summary>
        /// Strongly typed for value GL_IMAGE_PIXEL_TYPE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        ImagePixelType = Gl.IMAGE_PIXEL_TYPE,

        /// <summary>
        /// Strongly typed for value GL_IMAGE_FORMAT_COMPATIBILITY_TYPE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_shader_image_load_store", Api = "gl|glcore")]
        ImageFormatCompatibilityType = Gl.IMAGE_FORMAT_COMPATIBILITY_TYPE,

        /// <summary>
        /// Strongly typed for value GL_SIMULTANEOUS_TEXTURE_AND_DEPTH_TEST.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        SimultaneousTextureAndDepthTest = Gl.SIMULTANEOUS_TEXTURE_AND_DEPTH_TEST,

        /// <summary>
        /// Strongly typed for value GL_SIMULTANEOUS_TEXTURE_AND_STENCIL_TEST.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        SimultaneousTextureAndStencilTest = Gl.SIMULTANEOUS_TEXTURE_AND_STENCIL_TEST,

        /// <summary>
        /// Strongly typed for value GL_SIMULTANEOUS_TEXTURE_AND_DEPTH_WRITE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        SimultaneousTextureAndDepthWrite = Gl.SIMULTANEOUS_TEXTURE_AND_DEPTH_WRITE,

        /// <summary>
        /// Strongly typed for value GL_SIMULTANEOUS_TEXTURE_AND_STENCIL_WRITE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        SimultaneousTextureAndStencilWrite = Gl.SIMULTANEOUS_TEXTURE_AND_STENCIL_WRITE,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_COMPRESSED.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_texture_compression")]
        TextureCompressed = Gl.TEXTURE_COMPRESSED,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_COMPRESSED_BLOCK_WIDTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        TextureCompressedBlockWidth = Gl.TEXTURE_COMPRESSED_BLOCK_WIDTH,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_COMPRESSED_BLOCK_HEIGHT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        TextureCompressedBlockHeight = Gl.TEXTURE_COMPRESSED_BLOCK_HEIGHT,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_COMPRESSED_BLOCK_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        TextureCompressedBlockSize = Gl.TEXTURE_COMPRESSED_BLOCK_SIZE,

        /// <summary>
        /// Strongly typed for value GL_CLEAR_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        ClearBuffer = Gl.CLEAR_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_VIEW.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        TextureView = Gl.TEXTURE_VIEW,

        /// <summary>
        /// Strongly typed for value GL_VIEW_COMPATIBILITY_CLASS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        ViewCompatibilityClass = Gl.VIEW_COMPATIBILITY_CLASS,

        /// <summary>
        /// Strongly typed for value GL_CLEAR_TEXTURE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_4")] [RequiredByFeature("GL_ARB_clear_texture", Api = "gl|glcore")]
        ClearTexture = Gl.CLEAR_TEXTURE
    }

    /// <summary>
    /// Strongly typed enumeration ListNameType.
    /// </summary>
    public enum ListNameType
    {
        /// <summary>
        /// Strongly typed for value GL_BYTE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_render_snorm", Api = "gles2")]
        [RequiredByFeature("GL_OES_byte_coordinates", Api = "gl|gles1")]
        Byte = Gl.BYTE,

        /// <summary>
        /// Strongly typed for value GL_FLOAT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_OES_texture_float", Api = "gles2")]
        Float = Gl.FLOAT,

        /// <summary>
        /// Strongly typed for value GL_INT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Int = Gl.INT,

        /// <summary>
        /// Strongly typed for value GL_SHORT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_render_snorm", Api = "gles2")]
        Short = Gl.SHORT,

        /// <summary>
        /// Strongly typed for value GL_UNSIGNED_BYTE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        UnsignedByte = Gl.UNSIGNED_BYTE,

        /// <summary>
        /// Strongly typed for value GL_UNSIGNED_INT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ANGLE_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_element_index_uint", Api = "gles1|gles2")]
        UnsignedInt = Gl.UNSIGNED_INT,

        /// <summary>
        /// Strongly typed for value GL_UNSIGNED_SHORT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ANGLE_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_depth_texture", Api = "gles2")]
        UnsignedShort = Gl.UNSIGNED_SHORT
    }

    /// <summary>
    /// Strongly typed enumeration LogicOp.
    /// </summary>
    public enum LogicOp
    {
        /// <summary>
        /// Strongly typed for value GL_AND.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        And = Gl.AND,

        /// <summary>
        /// Strongly typed for value GL_AND_INVERTED.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        AndInverted = Gl.AND_INVERTED,

        /// <summary>
        /// Strongly typed for value GL_AND_REVERSE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        AndReverse = Gl.AND_REVERSE,

        /// <summary>
        /// Strongly typed for value GL_CLEAR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        Clear = Gl.CLEAR,

        /// <summary>
        /// Strongly typed for value GL_COPY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        Copy = Gl.COPY,

        /// <summary>
        /// Strongly typed for value GL_COPY_INVERTED.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        CopyInverted = Gl.COPY_INVERTED,

        /// <summary>
        /// Strongly typed for value GL_EQUIV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        Equiv = Gl.EQUIV,

        /// <summary>
        /// Strongly typed for value GL_INVERT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        Invert = Gl.INVERT,

        /// <summary>
        /// Strongly typed for value GL_NAND.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        Nand = Gl.NAND,

        /// <summary>
        /// Strongly typed for value GL_NOOP.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        Noop = Gl.NOOP,

        /// <summary>
        /// Strongly typed for value GL_NOR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        Nor = Gl.NOR,

        /// <summary>
        /// Strongly typed for value GL_OR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        Or = Gl.OR,

        /// <summary>
        /// Strongly typed for value GL_OR_INVERTED.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        OrInverted = Gl.OR_INVERTED,

        /// <summary>
        /// Strongly typed for value GL_OR_REVERSE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        OrReverse = Gl.OR_REVERSE,

        /// <summary>
        /// Strongly typed for value GL_SET.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        Set = Gl.SET,

        /// <summary>
        /// Strongly typed for value GL_XOR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        Xor = Gl.XOR
    }

    /// <summary>
    /// Strongly typed enumeration MapBufferUsageMask.
    /// </summary>
    [Flags]
    public enum MapBufferUsageMask : uint
    {
        /// <summary>
        /// Strongly typed for value GL_CLIENT_STORAGE_BIT, GL_CLIENT_STORAGE_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_4")] [RequiredByFeature("GL_ARB_buffer_storage", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_buffer_storage", Api = "gles2")]
        ClientStorageBit = Gl.CLIENT_STORAGE_BIT,

        /// <summary>
        /// Strongly typed for value GL_DYNAMIC_STORAGE_BIT, GL_DYNAMIC_STORAGE_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_4")] [RequiredByFeature("GL_ARB_buffer_storage", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_buffer_storage", Api = "gles2")]
        DynamicStorageBit = Gl.DYNAMIC_STORAGE_BIT,

        /// <summary>
        /// Strongly typed for value GL_MAP_COHERENT_BIT, GL_MAP_COHERENT_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_4")] [RequiredByFeature("GL_ARB_buffer_storage", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_buffer_storage", Api = "gles2")]
        MapCoherentBit = Gl.MAP_COHERENT_BIT,

        /// <summary>
        /// Strongly typed for value GL_MAP_FLUSH_EXPLICIT_BIT, GL_MAP_FLUSH_EXPLICIT_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_map_buffer_range", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_map_buffer_range", Api = "gles1|gles2")]
        MapFlushExplicitBit = Gl.MAP_FLUSH_EXPLICIT_BIT,

        /// <summary>
        /// Strongly typed for value GL_MAP_INVALIDATE_BUFFER_BIT, GL_MAP_INVALIDATE_BUFFER_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_map_buffer_range", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_map_buffer_range", Api = "gles1|gles2")]
        MapInvalidateBufferBit = Gl.MAP_INVALIDATE_BUFFER_BIT,

        /// <summary>
        /// Strongly typed for value GL_MAP_INVALIDATE_RANGE_BIT, GL_MAP_INVALIDATE_RANGE_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_map_buffer_range", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_map_buffer_range", Api = "gles1|gles2")]
        MapInvalidateRangeBit = Gl.MAP_INVALIDATE_RANGE_BIT,

        /// <summary>
        /// Strongly typed for value GL_MAP_PERSISTENT_BIT, GL_MAP_PERSISTENT_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_4")] [RequiredByFeature("GL_ARB_buffer_storage", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_buffer_storage", Api = "gles2")]
        MapPersistentBit = Gl.MAP_PERSISTENT_BIT,

        /// <summary>
        /// Strongly typed for value GL_MAP_READ_BIT, GL_MAP_READ_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_VERSION_4_4")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_buffer_storage", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_map_buffer_range", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_buffer_storage", Api = "gles2")]
        [RequiredByFeature("GL_EXT_map_buffer_range", Api = "gles1|gles2")]
        MapReadBit = Gl.MAP_READ_BIT,

        /// <summary>
        /// Strongly typed for value GL_MAP_UNSYNCHRONIZED_BIT, GL_MAP_UNSYNCHRONIZED_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_map_buffer_range", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_map_buffer_range", Api = "gles1|gles2")]
        MapUnsynchronizedBit = Gl.MAP_UNSYNCHRONIZED_BIT,

        /// <summary>
        /// Strongly typed for value GL_MAP_WRITE_BIT, GL_MAP_WRITE_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_VERSION_4_4")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_buffer_storage", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_map_buffer_range", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_buffer_storage", Api = "gles2")]
        [RequiredByFeature("GL_EXT_map_buffer_range", Api = "gles1|gles2")]
        MapWriteBit = Gl.MAP_WRITE_BIT,

        /// <summary>
        /// Strongly typed for value GL_NONE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_VERSION_4_6")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_KHR_context_flush_control", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_NV_register_combiners")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        None = Gl.NONE
    }

    /// <summary>
    /// Strongly typed enumeration MaterialFace.
    /// </summary>
    public enum MaterialFace
    {
        /// <summary>
        /// Strongly typed for value GL_BACK.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_ES3_1_compatibility", Api = "gl|glcore")]
        Back = Gl.BACK,

        /// <summary>
        /// Strongly typed for value GL_FRONT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Front = Gl.FRONT,

        /// <summary>
        /// Strongly typed for value GL_FRONT_AND_BACK.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        FrontAndBack = Gl.FRONT_AND_BACK
    }

    /// <summary>
    /// Strongly typed enumeration MemoryBarrierMask.
    /// </summary>
    [Flags]
    public enum MemoryBarrierMask : uint
    {
        /// <summary>
        /// Strongly typed for value GL_ALL_BARRIER_BITS, GL_ALL_BARRIER_BITS_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_image_load_store", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_shader_image_load_store")]
        AllBarrierBits = Gl.ALL_BARRIER_BITS,

        /// <summary>
        /// Strongly typed for value GL_ATOMIC_COUNTER_BARRIER_BIT, GL_ATOMIC_COUNTER_BARRIER_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_image_load_store", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_shader_image_load_store")]
        AtomicCounterBarrierBit = Gl.ATOMIC_COUNTER_BARRIER_BIT,

        /// <summary>
        /// Strongly typed for value GL_BUFFER_UPDATE_BARRIER_BIT, GL_BUFFER_UPDATE_BARRIER_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_image_load_store", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_shader_image_load_store")]
        BufferUpdateBarrierBit = Gl.BUFFER_UPDATE_BARRIER_BIT,

        /// <summary>
        /// Strongly typed for value GL_CLIENT_MAPPED_BUFFER_BARRIER_BIT, GL_CLIENT_MAPPED_BUFFER_BARRIER_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_4")] [RequiredByFeature("GL_ARB_buffer_storage", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_buffer_storage", Api = "gles2")]
        ClientMappedBufferBarrierBit = Gl.CLIENT_MAPPED_BUFFER_BARRIER_BIT,

        /// <summary>
        /// Strongly typed for value GL_COMMAND_BARRIER_BIT, GL_COMMAND_BARRIER_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_image_load_store", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_shader_image_load_store")]
        CommandBarrierBit = Gl.COMMAND_BARRIER_BIT,

        /// <summary>
        /// Strongly typed for value GL_ELEMENT_ARRAY_BARRIER_BIT, GL_ELEMENT_ARRAY_BARRIER_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_image_load_store", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_shader_image_load_store")]
        ElementArrayBarrierBit = Gl.ELEMENT_ARRAY_BARRIER_BIT,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER_BARRIER_BIT, GL_FRAMEBUFFER_BARRIER_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_image_load_store", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_shader_image_load_store")]
        FramebufferBarrierBit = Gl.FRAMEBUFFER_BARRIER_BIT,

        /// <summary>
        /// Strongly typed for value GL_PIXEL_BUFFER_BARRIER_BIT, GL_PIXEL_BUFFER_BARRIER_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_image_load_store", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_shader_image_load_store")]
        PixelBufferBarrierBit = Gl.PIXEL_BUFFER_BARRIER_BIT,

        /// <summary>
        /// Strongly typed for value GL_QUERY_BUFFER_BARRIER_BIT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_4")] [RequiredByFeature("GL_ARB_query_buffer_object", Api = "gl|glcore")]
        QueryBufferBarrierBit = Gl.QUERY_BUFFER_BARRIER_BIT,

        /// <summary>
        /// Strongly typed for value GL_SHADER_IMAGE_ACCESS_BARRIER_BIT, GL_SHADER_IMAGE_ACCESS_BARRIER_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_image_load_store", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_shader_image_load_store")]
        ShaderImageAccessBarrierBit = Gl.SHADER_IMAGE_ACCESS_BARRIER_BIT,

        /// <summary>
        /// Strongly typed for value GL_SHADER_STORAGE_BARRIER_BIT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_storage_buffer_object", Api = "gl|glcore")]
        ShaderStorageBarrierBit = Gl.SHADER_STORAGE_BARRIER_BIT,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_FETCH_BARRIER_BIT, GL_TEXTURE_FETCH_BARRIER_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_image_load_store", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_shader_image_load_store")]
        TextureFetchBarrierBit = Gl.TEXTURE_FETCH_BARRIER_BIT,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_UPDATE_BARRIER_BIT, GL_TEXTURE_UPDATE_BARRIER_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_image_load_store", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_shader_image_load_store")]
        TextureUpdateBarrierBit = Gl.TEXTURE_UPDATE_BARRIER_BIT,

        /// <summary>
        /// Strongly typed for value GL_TRANSFORM_FEEDBACK_BARRIER_BIT, GL_TRANSFORM_FEEDBACK_BARRIER_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_image_load_store", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_shader_image_load_store")]
        TransformFeedbackBarrierBit = Gl.TRANSFORM_FEEDBACK_BARRIER_BIT,

        /// <summary>
        /// Strongly typed for value GL_UNIFORM_BARRIER_BIT, GL_UNIFORM_BARRIER_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_image_load_store", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_shader_image_load_store")]
        UniformBarrierBit = Gl.UNIFORM_BARRIER_BIT,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_ATTRIB_ARRAY_BARRIER_BIT, GL_VERTEX_ATTRIB_ARRAY_BARRIER_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_image_load_store", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_shader_image_load_store")]
        VertexAttribArrayBarrierBit = Gl.VERTEX_ATTRIB_ARRAY_BARRIER_BIT
    }

    /// <summary>
    /// Strongly typed enumeration MeshMode1.
    /// </summary>
    public enum MeshMode1
    {
        /// <summary>
        /// Strongly typed for value GL_LINE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_NV_polygon_mode", Api = "gles2")]
        Line = Gl.LINE,

        /// <summary>
        /// Strongly typed for value GL_POINT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_NV_polygon_mode", Api = "gles2")]
        Point = Gl.POINT
    }

    /// <summary>
    /// Strongly typed enumeration MeshMode2.
    /// </summary>
    public enum MeshMode2
    {
        /// <summary>
        /// Strongly typed for value GL_FILL.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_NV_polygon_mode", Api = "gles2")]
        Fill = Gl.FILL,

        /// <summary>
        /// Strongly typed for value GL_LINE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_NV_polygon_mode", Api = "gles2")]
        Line = Gl.LINE,

        /// <summary>
        /// Strongly typed for value GL_POINT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_NV_polygon_mode", Api = "gles2")]
        Point = Gl.POINT
    }

    /// <summary>
    /// Strongly typed enumeration NormalPointerType.
    /// </summary>
    public enum NormalPointerType
    {
        /// <summary>
        /// Strongly typed for value GL_BYTE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_render_snorm", Api = "gles2")]
        [RequiredByFeature("GL_OES_byte_coordinates", Api = "gl|gles1")]
        Byte = Gl.BYTE,

        /// <summary>
        /// Strongly typed for value GL_DOUBLE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_ARB_gpu_shader_fp64", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_vertex_attrib_64bit")]
        Double = Gl.DOUBLE,

        /// <summary>
        /// Strongly typed for value GL_FLOAT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_OES_texture_float", Api = "gles2")]
        Float = Gl.FLOAT,

        /// <summary>
        /// Strongly typed for value GL_INT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Int = Gl.INT,

        /// <summary>
        /// Strongly typed for value GL_SHORT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_render_snorm", Api = "gles2")]
        Short = Gl.SHORT
    }

    /// <summary>
    /// Strongly typed enumeration ObjectIdentifier.
    /// </summary>
    public enum ObjectIdentifier
    {
        /// <summary>
        /// Strongly typed for value GL_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        Buffer = Gl.BUFFER,

        /// <summary>
        /// Strongly typed for value GL_SHADER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        Shader = Gl.SHADER,

        /// <summary>
        /// Strongly typed for value GL_PROGRAM.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        Program = Gl.PROGRAM,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_ARRAY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")]
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_EXT_vertex_array")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RemovedByFeature("GL_VERSION_3_2")]
        VertexArray = Gl.VERTEX_ARRAY,

        /// <summary>
        /// Strongly typed for value GL_QUERY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        Query = Gl.QUERY,

        /// <summary>
        /// Strongly typed for value GL_PROGRAM_PIPELINE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        ProgramPipeline = Gl.PROGRAM_PIPELINE,

        /// <summary>
        /// Strongly typed for value GL_TRANSFORM_FEEDBACK.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_transform_feedback2", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_debug_label", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_NV_transform_feedback2")]
        TransformFeedback = Gl.TRANSFORM_FEEDBACK,

        /// <summary>
        /// Strongly typed for value GL_SAMPLER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_EXT_debug_label", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
        Sampler = Gl.SAMPLER,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Texture = Gl.TEXTURE,

        /// <summary>
        /// Strongly typed for value GL_RENDERBUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        [RequiredByFeature("GL_NV_internalformat_sample_query", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        Renderbuffer = Gl.RENDERBUFFER,

        /// <summary>
        /// Strongly typed for value GL_FRAMEBUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        Framebuffer = Gl.FRAMEBUFFER
    }

    /// <summary>
    /// Strongly typed enumeration PatchParameterName.
    /// </summary>
    public enum PatchParameterName
    {
        /// <summary>
        /// Strongly typed for value GL_PATCH_VERTICES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_0")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
        PatchVertices = Gl.PATCH_VERTICES,

        /// <summary>
        /// Strongly typed for value GL_PATCH_DEFAULT_OUTER_LEVEL.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_0")] [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
        PatchDefaultOuterLevel = Gl.PATCH_DEFAULT_OUTER_LEVEL,

        /// <summary>
        /// Strongly typed for value GL_PATCH_DEFAULT_INNER_LEVEL.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_0")] [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
        PatchDefaultInnerLevel = Gl.PATCH_DEFAULT_INNER_LEVEL
    }

    /// <summary>
    /// Strongly typed enumeration PathFillMode.
    /// </summary>
    public enum PathFillMode
    {
        /// <summary>
        /// Strongly typed for value GL_INVERT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        Invert = Gl.INVERT
    }

    /// <summary>
    /// Strongly typed enumeration PathFontStyle.
    /// </summary>
    public enum PathFontStyle
    {
        /// <summary>
        /// Strongly typed for value GL_NONE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_VERSION_4_6")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_KHR_context_flush_control", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_NV_register_combiners")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        None = Gl.NONE
    }

    /// <summary>
    /// Strongly typed enumeration PathGenMode.
    /// </summary>
    public enum PathGenMode
    {
        /// <summary>
        /// Strongly typed for value GL_NONE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_VERSION_4_6")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_KHR_context_flush_control", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_NV_register_combiners")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        None = Gl.NONE
    }

    /// <summary>
    /// Strongly typed enumeration PipelineParameterName.
    /// </summary>
    public enum PipelineParameterName
    {
        /// <summary>
        /// Strongly typed for value GL_ACTIVE_PROGRAM.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_ARB_separate_shader_objects", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_separate_shader_objects", Api = "gl|glcore|gles2")]
        ActiveProgram = Gl.ACTIVE_PROGRAM,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_SHADER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_vertex_shader")]
        VertexShader = Gl.VERTEX_SHADER,

        /// <summary>
        /// Strongly typed for value GL_TESS_CONTROL_SHADER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_0")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
        TessControlShader = Gl.TESS_CONTROL_SHADER,

        /// <summary>
        /// Strongly typed for value GL_TESS_EVALUATION_SHADER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_0")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
        TessEvaluationShader = Gl.TESS_EVALUATION_SHADER,

        /// <summary>
        /// Strongly typed for value GL_GEOMETRY_SHADER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_geometry_shader4", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
        [RequiredByFeature("GL_EXT_geometry_shader4")]
        [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
        GeometryShader = Gl.GEOMETRY_SHADER,

        /// <summary>
        /// Strongly typed for value GL_FRAGMENT_SHADER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_fragment_shader")]
        FragmentShader = Gl.FRAGMENT_SHADER,

        /// <summary>
        /// Strongly typed for value GL_INFO_LOG_LENGTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        InfoLogLength = Gl.INFO_LOG_LENGTH
    }

    /// <summary>
    /// Strongly typed enumeration PixelCopyType.
    /// </summary>
    public enum PixelCopyType
    {
        /// <summary>
        /// Strongly typed for value GL_COLOR, GL_COLOR_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_discard_framebuffer", Api = "gles1|gles2")]
        Color = Gl.COLOR,

        /// <summary>
        /// Strongly typed for value GL_DEPTH, GL_DEPTH_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_discard_framebuffer", Api = "gles1|gles2")]
        Depth = Gl.DEPTH,

        /// <summary>
        /// Strongly typed for value GL_STENCIL, GL_STENCIL_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_discard_framebuffer", Api = "gles1|gles2")]
        Stencil = Gl.STENCIL
    }

    /// <summary>
    /// Strongly typed enumeration PixelFormat.
    /// </summary>
    public enum PixelFormat
    {
        /// <summary>
        /// Not used by GL
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Strongly typed for value GL_ALPHA.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_AMD_interleaved_elements")]
        Alpha = Gl.ALPHA,

        /// <summary>
        /// Strongly typed for value GL_BGR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_EXT_bgra")]
        Bgr = Gl.BGR,

        /// <summary>
        /// Strongly typed for value GL_BGR_INTEGER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_EXT_texture_integer")]
        BgrInteger = Gl.BGR_INTEGER,

        /// <summary>
        /// Strongly typed for value GL_BGRA.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")]
        [RequiredByFeature("GL_ARB_vertex_array_bgra", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_vertex_array_bgra")]
        [RequiredByFeature("GL_APPLE_texture_format_BGRA8888", Api = "gles1|gles2")]
        [RequiredByFeature("GL_EXT_bgra")]
        [RequiredByFeature("GL_EXT_read_format_bgra", Api = "gles1|gles2")]
        [RequiredByFeature("GL_EXT_texture_format_BGRA8888", Api = "gles1|gles2")]
        [RequiredByFeature("GL_IMG_read_format", Api = "gles1|gles2")]
        Bgra = Gl.BGRA,

        /// <summary>
        /// Strongly typed for value GL_BGRA_INTEGER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_EXT_texture_integer")]
        BgraInteger = Gl.BGRA_INTEGER,

        /// <summary>
        /// Strongly typed for value GL_BLUE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_AMD_interleaved_elements")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        Blue = Gl.BLUE,

        /// <summary>
        /// Strongly typed for value GL_BLUE_INTEGER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_EXT_texture_integer")]
        BlueInteger = Gl.BLUE_INTEGER,

        /// <summary>
        /// Strongly typed for value GL_DEPTH_COMPONENT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ANGLE_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_depth_texture", Api = "gles2")]
        DepthComponent = Gl.DEPTH_COMPONENT,

        /// <summary>
        /// Strongly typed for value GL_DEPTH_STENCIL.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_packed_depth_stencil")]
        [RequiredByFeature("GL_NV_packed_depth_stencil")]
        [RequiredByFeature("GL_ANGLE_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_packed_depth_stencil", Api = "gles1|gles2")]
        DepthStencil = Gl.DEPTH_STENCIL,

        /// <summary>
        /// Strongly typed for value GL_GREEN.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_AMD_interleaved_elements")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        Green = Gl.GREEN,

        /// <summary>
        /// Strongly typed for value GL_GREEN_INTEGER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_EXT_texture_integer")]
        GreenInteger = Gl.GREEN_INTEGER,

        /// <summary>
        /// Strongly typed for value GL_RED, GL_RED_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_AMD_interleaved_elements")]
        [RequiredByFeature("GL_EXT_texture_rg", Api = "gles2")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        Red = Gl.RED,

        /// <summary>
        /// Strongly typed for value GL_RED_INTEGER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_texture_integer")]
        RedInteger = Gl.RED_INTEGER,

        /// <summary>
        /// Strongly typed for value GL_RG.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_texture_rg", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_texture_rg", Api = "gles2")]
        Rg = Gl.RG,

        /// <summary>
        /// Strongly typed for value GL_RG_INTEGER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_texture_rg", Api = "gl|glcore")]
        RgInteger = Gl.RG_INTEGER,

        /// <summary>
        /// Strongly typed for value GL_RGB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Rgb = Gl.RGB,

        /// <summary>
        /// Strongly typed for value GL_RGB_INTEGER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_texture_integer")]
        RgbInteger = Gl.RGB_INTEGER,

        /// <summary>
        /// Strongly typed for value GL_RGBA.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Rgba = Gl.RGBA,

        /// <summary>
        /// Strongly typed for value GL_RGBA_INTEGER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_texture_integer")]
        RgbaInteger = Gl.RGBA_INTEGER,

        /// <summary>
        /// Strongly typed for value GL_STENCIL_INDEX.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_4_4")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_texture_stencil8", Api = "gl|glcore")]
        [RequiredByFeature("GL_OES_texture_stencil8", Api = "gles2")]
        StencilIndex = Gl.STENCIL_INDEX,

        /// <summary>
        /// Strongly typed for value GL_UNSIGNED_INT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ANGLE_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_element_index_uint", Api = "gles1|gles2")]
        UnsignedInt = Gl.UNSIGNED_INT,

        /// <summary>
        /// Strongly typed for value GL_UNSIGNED_SHORT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ANGLE_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_depth_texture", Api = "gles2")]
        UnsignedShort = Gl.UNSIGNED_SHORT
    }

    /// <summary>
    /// Strongly typed enumeration PixelStoreParameter.
    /// </summary>
    public enum PixelStoreParameter
    {
        /// <summary>
        /// Strongly typed for value GL_PACK_ALIGNMENT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        PackAlignment = Gl.PACK_ALIGNMENT,

        /// <summary>
        /// Strongly typed for value GL_PACK_IMAGE_HEIGHT, GL_PACK_IMAGE_HEIGHT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_EXT_texture3D")]
        PackImageHeight = Gl.PACK_IMAGE_HEIGHT,

        /// <summary>
        /// Strongly typed for value GL_PACK_LSB_FIRST.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] PackLsbFirst = Gl.PACK_LSB_FIRST,

        /// <summary>
        /// Strongly typed for value GL_PACK_ROW_LENGTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        PackRowLength = Gl.PACK_ROW_LENGTH,

        /// <summary>
        /// Strongly typed for value GL_PACK_SKIP_IMAGES, GL_PACK_SKIP_IMAGES_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_EXT_texture3D")]
        PackSkipImages = Gl.PACK_SKIP_IMAGES,

        /// <summary>
        /// Strongly typed for value GL_PACK_SKIP_PIXELS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        PackSkipPixels = Gl.PACK_SKIP_PIXELS,

        /// <summary>
        /// Strongly typed for value GL_PACK_SKIP_ROWS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        PackSkipRows = Gl.PACK_SKIP_ROWS,

        /// <summary>
        /// Strongly typed for value GL_PACK_SWAP_BYTES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] PackSwapBytes = Gl.PACK_SWAP_BYTES,

        /// <summary>
        /// Strongly typed for value GL_UNPACK_ALIGNMENT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        UnpackAlignment = Gl.UNPACK_ALIGNMENT,

        /// <summary>
        /// Strongly typed for value GL_UNPACK_IMAGE_HEIGHT, GL_UNPACK_IMAGE_HEIGHT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_texture3D")]
        UnpackImageHeight = Gl.UNPACK_IMAGE_HEIGHT,

        /// <summary>
        /// Strongly typed for value GL_UNPACK_LSB_FIRST.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] UnpackLsbFirst = Gl.UNPACK_LSB_FIRST,

        /// <summary>
        /// Strongly typed for value GL_UNPACK_ROW_LENGTH, GL_UNPACK_ROW_LENGTH_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_unpack_subimage", Api = "gles2")]
        UnpackRowLength = Gl.UNPACK_ROW_LENGTH,

        /// <summary>
        /// Strongly typed for value GL_UNPACK_SKIP_IMAGES, GL_UNPACK_SKIP_IMAGES_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_texture3D")]
        UnpackSkipImages = Gl.UNPACK_SKIP_IMAGES,

        /// <summary>
        /// Strongly typed for value GL_UNPACK_SKIP_PIXELS, GL_UNPACK_SKIP_PIXELS_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_unpack_subimage", Api = "gles2")]
        UnpackSkipPixels = Gl.UNPACK_SKIP_PIXELS,

        /// <summary>
        /// Strongly typed for value GL_UNPACK_SKIP_ROWS, GL_UNPACK_SKIP_ROWS_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_unpack_subimage", Api = "gles2")]
        UnpackSkipRows = Gl.UNPACK_SKIP_ROWS,

        /// <summary>
        /// Strongly typed for value GL_UNPACK_SWAP_BYTES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] UnpackSwapBytes = Gl.UNPACK_SWAP_BYTES
    }

    /// <summary>
    /// Strongly typed enumeration PixelTexGenMode.
    /// </summary>
    public enum PixelTexGenMode
    {
        /// <summary>
        /// Strongly typed for value GL_NONE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_VERSION_4_6")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_KHR_context_flush_control", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_NV_register_combiners")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        None = Gl.NONE,

        /// <summary>
        /// Strongly typed for value GL_RGB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Rgb = Gl.RGB,

        /// <summary>
        /// Strongly typed for value GL_RGBA.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Rgba = Gl.RGBA
    }

    /// <summary>
    /// Strongly typed enumeration PixelType.
    /// </summary>
    public enum PixelType
    {
        /// <summary>
        /// Strongly typed for value GL_BYTE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_render_snorm", Api = "gles2")]
        [RequiredByFeature("GL_OES_byte_coordinates", Api = "gl|gles1")]
        Byte = Gl.BYTE,

        /// <summary>
        /// Strongly typed for value GL_FLOAT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_OES_texture_float", Api = "gles2")]
        Float = Gl.FLOAT,

        /// <summary>
        /// Strongly typed for value GL_INT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Int = Gl.INT,

        /// <summary>
        /// Strongly typed for value GL_SHORT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_render_snorm", Api = "gles2")]
        Short = Gl.SHORT,

        /// <summary>
        /// Strongly typed for value GL_UNSIGNED_BYTE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        UnsignedByte = Gl.UNSIGNED_BYTE,

        /// <summary>
        /// Strongly typed for value GL_UNSIGNED_BYTE_3_3_2, GL_UNSIGNED_BYTE_3_3_2_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_EXT_packed_pixels")]
        UnsignedByte332 = Gl.UNSIGNED_BYTE_3_3_2,

        /// <summary>
        /// Strongly typed for value GL_UNSIGNED_INT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ANGLE_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_element_index_uint", Api = "gles1|gles2")]
        UnsignedInt = Gl.UNSIGNED_INT,

        /// <summary>
        /// Strongly typed for value GL_UNSIGNED_INT_10_10_10_2, GL_UNSIGNED_INT_10_10_10_2_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_EXT_packed_pixels")]
        UnsignedInt1010102 = Gl.UNSIGNED_INT_10_10_10_2,

        /// <summary>
        /// Strongly typed for value GL_UNSIGNED_INT_8_8_8_8, GL_UNSIGNED_INT_8_8_8_8_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_EXT_packed_pixels")]
        UnsignedInt8888 = Gl.UNSIGNED_INT_8_8_8_8,

        /// <summary>
        /// Strongly typed for value GL_UNSIGNED_SHORT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ANGLE_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_depth_texture", Api = "gles2")]
        UnsignedShort = Gl.UNSIGNED_SHORT,

        /// <summary>
        /// Strongly typed for value GL_UNSIGNED_SHORT_4_4_4_4, GL_UNSIGNED_SHORT_4_4_4_4_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_packed_pixels")]
        UnsignedShort4444 = Gl.UNSIGNED_SHORT_4_4_4_4,

        /// <summary>
        /// Strongly typed for value GL_UNSIGNED_SHORT_5_5_5_1, GL_UNSIGNED_SHORT_5_5_5_1_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_packed_pixels")]
        UnsignedShort5551 = Gl.UNSIGNED_SHORT_5_5_5_1,

        /// <summary>
        /// Strongly typed for value GL_DOUBLE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_ARB_gpu_shader_fp64", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_vertex_attrib_64bit")]
        Double = Gl.DOUBLE,

        /// <summary>
        /// Strongly typed for value GL_HALF_FLOAT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_half_float_vertex", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_half_float_pixel")]
        [RequiredByFeature("GL_NV_half_float")]
        HalfFloat = Gl.HALF_FLOAT,

        /// <summary>
        /// Strongly typed for value GL_UNSIGNED_BYTE_2_3_3_REV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] UnsignedByte233Rev = Gl.UNSIGNED_BYTE_2_3_3_REV,

        /// <summary>
        /// Strongly typed for value GL_UNSIGNED_SHORT_5_6_5.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        UnsignedShort565 = Gl.UNSIGNED_SHORT_5_6_5,

        /// <summary>
        /// Strongly typed for value GL_UNSIGNED_SHORT_1_5_5_5_REV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_EXT_read_format_bgra", Api = "gles1|gles2")]
        UnsignedShort1555Rev = Gl.UNSIGNED_SHORT_1_5_5_5_REV,

        /// <summary>
        /// Strongly typed for value GL_UNSIGNED_SHORT_5_6_5_REV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] UnsignedShort565Rev = Gl.UNSIGNED_SHORT_5_6_5_REV,

        /// <summary>
        /// Strongly typed for value GL_UNSIGNED_INT_2_10_10_10_REV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_vertex_type_2_10_10_10_rev", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_texture_type_2_10_10_10_REV", Api = "gles2")]
        UnsignedInt2101010Rev = Gl.UNSIGNED_INT_2_10_10_10_REV,

        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_packed_depth_stencil")]
        [RequiredByFeature("GL_NV_packed_depth_stencil")]
        [RequiredByFeature("GL_ANGLE_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_packed_depth_stencil", Api = "gles1|gles2")]
        UnsignedInt248 = Gl.UNSIGNED_INT_24_8
    }

    /// <summary>
    /// Strongly typed enumeration PolygonMode.
    /// </summary>
    public enum PolygonMode
    {
        /// <summary>
        /// Strongly typed for value GL_FILL.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_NV_polygon_mode", Api = "gles2")]
        Fill = Gl.FILL,

        /// <summary>
        /// Strongly typed for value GL_LINE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_NV_polygon_mode", Api = "gles2")]
        Line = Gl.LINE,

        /// <summary>
        /// Strongly typed for value GL_POINT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_NV_polygon_mode", Api = "gles2")]
        Point = Gl.POINT
    }

    /// <summary>
    /// Strongly typed enumeration PrecisionType.
    /// </summary>
    public enum PrecisionType
    {
        /// <summary>
        /// Strongly typed for value GL_LOW_FLOAT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_ES2_compatibility", Api = "gl|glcore")]
        LowFloat = Gl.LOW_FLOAT,

        /// <summary>
        /// Strongly typed for value GL_MEDIUM_FLOAT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_ES2_compatibility", Api = "gl|glcore")]
        MediumFloat = Gl.MEDIUM_FLOAT,

        /// <summary>
        /// Strongly typed for value GL_HIGH_FLOAT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_ES2_compatibility", Api = "gl|glcore")]
        HighFloat = Gl.HIGH_FLOAT,

        /// <summary>
        /// Strongly typed for value GL_LOW_INT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_ES2_compatibility", Api = "gl|glcore")]
        LowInt = Gl.LOW_INT,

        /// <summary>
        /// Strongly typed for value GL_MEDIUM_INT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_ES2_compatibility", Api = "gl|glcore")]
        MediumInt = Gl.MEDIUM_INT,

        /// <summary>
        /// Strongly typed for value GL_HIGH_INT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_ES2_compatibility", Api = "gl|glcore")]
        HighInt = Gl.HIGH_INT
    }

    /// <summary>
    /// Strongly typed enumeration PrimitiveType.
    /// </summary>
    public enum PrimitiveType
    {
        /// <summary>
        /// Strongly typed for value GL_LINES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Lines = Gl.LINES,

        /// <summary>
        /// Strongly typed for value GL_LINES_ADJACENCY, GL_LINES_ADJACENCY_ARB, GL_LINES_ADJACENCY_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_geometry_shader4", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
        [RequiredByFeature("GL_EXT_geometry_shader4")]
        [RequiredByFeature("GL_NV_geometry_program4")]
        [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
        LinesAdjacency = Gl.LINES_ADJACENCY,

        /// <summary>
        /// Strongly typed for value GL_LINE_LOOP.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        LineLoop = Gl.LINE_LOOP,

        /// <summary>
        /// Strongly typed for value GL_LINE_STRIP.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        LineStrip = Gl.LINE_STRIP,

        /// <summary>
        /// Strongly typed for value GL_LINE_STRIP_ADJACENCY, GL_LINE_STRIP_ADJACENCY_ARB, GL_LINE_STRIP_ADJACENCY_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_geometry_shader4", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
        [RequiredByFeature("GL_EXT_geometry_shader4")]
        [RequiredByFeature("GL_NV_geometry_program4")]
        [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
        LineStripAdjacency = Gl.LINE_STRIP_ADJACENCY,

        /// <summary>
        /// Strongly typed for value GL_PATCHES, GL_PATCHES_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_0")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
        [RequiredByFeature("GL_NV_gpu_shader5", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
        Patches = Gl.PATCHES,

        /// <summary>
        /// Strongly typed for value GL_POINTS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Points = Gl.POINTS,

        /// <summary>
        /// Strongly typed for value GL_TRIANGLES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
        Triangles = Gl.TRIANGLES,

        /// <summary>
        /// Strongly typed for value GL_TRIANGLES_ADJACENCY, GL_TRIANGLES_ADJACENCY_ARB, GL_TRIANGLES_ADJACENCY_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_geometry_shader4", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
        [RequiredByFeature("GL_EXT_geometry_shader4")]
        [RequiredByFeature("GL_NV_geometry_program4")]
        [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
        TrianglesAdjacency = Gl.TRIANGLES_ADJACENCY,

        /// <summary>
        /// Strongly typed for value GL_TRIANGLE_FAN.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        TriangleFan = Gl.TRIANGLE_FAN,

        /// <summary>
        /// Strongly typed for value GL_TRIANGLE_STRIP.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        TriangleStrip = Gl.TRIANGLE_STRIP,

        /// <summary>
        /// Strongly typed for value GL_TRIANGLE_STRIP_ADJACENCY, GL_TRIANGLE_STRIP_ADJACENCY_ARB, GL_TRIANGLE_STRIP_ADJACENCY_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_geometry_shader4", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
        [RequiredByFeature("GL_EXT_geometry_shader4")]
        [RequiredByFeature("GL_NV_geometry_program4")]
        [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
        TriangleStripAdjacency = Gl.TRIANGLE_STRIP_ADJACENCY
    }

    /// <summary>
    /// Strongly typed enumeration ProgramInterface.
    /// </summary>
    public enum ProgramInterface
    {
        /// <summary>
        /// Strongly typed for value GL_UNIFORM.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_program_interface_query", Api = "gl|glcore")]
        Uniform = Gl.UNIFORM,

        /// <summary>
        /// Strongly typed for value GL_UNIFORM_BLOCK.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_program_interface_query", Api = "gl|glcore")]
        UniformBlock = Gl.UNIFORM_BLOCK,

        /// <summary>
        /// Strongly typed for value GL_PROGRAM_INPUT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_program_interface_query", Api = "gl|glcore")]
        ProgramInput = Gl.PROGRAM_INPUT,

        /// <summary>
        /// Strongly typed for value GL_PROGRAM_OUTPUT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_program_interface_query", Api = "gl|glcore")]
        ProgramOutput = Gl.PROGRAM_OUTPUT,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_SUBROUTINE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_program_interface_query", Api = "gl|glcore")]
        VertexSubroutine = Gl.VERTEX_SUBROUTINE,

        /// <summary>
        /// Strongly typed for value GL_TESS_CONTROL_SUBROUTINE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_program_interface_query", Api = "gl|glcore")]
        TessControlSubroutine = Gl.TESS_CONTROL_SUBROUTINE,

        /// <summary>
        /// Strongly typed for value GL_TESS_EVALUATION_SUBROUTINE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_program_interface_query", Api = "gl|glcore")]
        TessEvaluationSubroutine = Gl.TESS_EVALUATION_SUBROUTINE,

        /// <summary>
        /// Strongly typed for value GL_GEOMETRY_SUBROUTINE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_program_interface_query", Api = "gl|glcore")]
        GeometrySubroutine = Gl.GEOMETRY_SUBROUTINE,

        /// <summary>
        /// Strongly typed for value GL_FRAGMENT_SUBROUTINE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_program_interface_query", Api = "gl|glcore")]
        FragmentSubroutine = Gl.FRAGMENT_SUBROUTINE,

        /// <summary>
        /// Strongly typed for value GL_COMPUTE_SUBROUTINE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_program_interface_query", Api = "gl|glcore")]
        ComputeSubroutine = Gl.COMPUTE_SUBROUTINE,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_SUBROUTINE_UNIFORM.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_program_interface_query", Api = "gl|glcore")]
        VertexSubroutineUniform = Gl.VERTEX_SUBROUTINE_UNIFORM,

        /// <summary>
        /// Strongly typed for value GL_TESS_CONTROL_SUBROUTINE_UNIFORM.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_program_interface_query", Api = "gl|glcore")]
        TessControlSubroutineUniform = Gl.TESS_CONTROL_SUBROUTINE_UNIFORM,

        /// <summary>
        /// Strongly typed for value GL_TESS_EVALUATION_SUBROUTINE_UNIFORM.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_program_interface_query", Api = "gl|glcore")]
        TessEvaluationSubroutineUniform = Gl.TESS_EVALUATION_SUBROUTINE_UNIFORM,

        /// <summary>
        /// Strongly typed for value GL_GEOMETRY_SUBROUTINE_UNIFORM.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_program_interface_query", Api = "gl|glcore")]
        GeometrySubroutineUniform = Gl.GEOMETRY_SUBROUTINE_UNIFORM,

        /// <summary>
        /// Strongly typed for value GL_FRAGMENT_SUBROUTINE_UNIFORM.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_program_interface_query", Api = "gl|glcore")]
        FragmentSubroutineUniform = Gl.FRAGMENT_SUBROUTINE_UNIFORM,

        /// <summary>
        /// Strongly typed for value GL_COMPUTE_SUBROUTINE_UNIFORM.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_program_interface_query", Api = "gl|glcore")]
        ComputeSubroutineUniform = Gl.COMPUTE_SUBROUTINE_UNIFORM,

        /// <summary>
        /// Strongly typed for value GL_TRANSFORM_FEEDBACK_VARYING.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_program_interface_query", Api = "gl|glcore")]
        TransformFeedbackVarying = Gl.TRANSFORM_FEEDBACK_VARYING,

        /// <summary>
        /// Strongly typed for value GL_TRANSFORM_FEEDBACK_BUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_VERSION_4_4")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_enhanced_layouts", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_transform_feedback")]
        [RequiredByFeature("GL_NV_transform_feedback")]
        TransformFeedbackBuffer = Gl.TRANSFORM_FEEDBACK_BUFFER,

        /// <summary>
        /// Strongly typed for value GL_BUFFER_VARIABLE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_program_interface_query", Api = "gl|glcore")]
        BufferVariable = Gl.BUFFER_VARIABLE,

        /// <summary>
        /// Strongly typed for value GL_SHADER_STORAGE_BLOCK.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_program_interface_query", Api = "gl|glcore")]
        ShaderStorageBlock = Gl.SHADER_STORAGE_BLOCK
    }

    /// <summary>
    /// Strongly typed enumeration ProgramInterfacePName.
    /// </summary>
    public enum ProgramInterfacePName
    {
        /// <summary>
        /// Strongly typed for value GL_ACTIVE_RESOURCES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_program_interface_query", Api = "gl|glcore")]
        ActiveResources = Gl.ACTIVE_RESOURCES,

        /// <summary>
        /// Strongly typed for value GL_MAX_NAME_LENGTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_program_interface_query", Api = "gl|glcore")]
        MaxNameLength = Gl.MAX_NAME_LENGTH,

        /// <summary>
        /// Strongly typed for value GL_MAX_NUM_ACTIVE_VARIABLES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_program_interface_query", Api = "gl|glcore")]
        MaxNumActiveVariables = Gl.MAX_NUM_ACTIVE_VARIABLES,

        /// <summary>
        /// Strongly typed for value GL_MAX_NUM_COMPATIBLE_SUBROUTINES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_program_interface_query", Api = "gl|glcore")]
        MaxNumCompatibleSubroutines = Gl.MAX_NUM_COMPATIBLE_SUBROUTINES
    }

    /// <summary>
    /// Strongly typed enumeration ProgramParameterPName.
    /// </summary>
    public enum ProgramParameterPName
    {
        /// <summary>
        /// Strongly typed for value GL_PROGRAM_BINARY_RETRIEVABLE_HINT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_get_program_binary", Api = "gl|glcore")]
        ProgramBinaryRetrievableHint = Gl.PROGRAM_BINARY_RETRIEVABLE_HINT,

        /// <summary>
        /// Strongly typed for value GL_PROGRAM_SEPARABLE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_ARB_separate_shader_objects", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_separate_shader_objects", Api = "gl|glcore|gles2")]
        ProgramSeparable = Gl.PROGRAM_SEPARABLE
    }

    /// <summary>
    /// Strongly typed enumeration ProgramProperty.
    /// </summary>
    public enum ProgramProperty
    {
        /// <summary>
        /// Strongly typed for value GL_DELETE_STATUS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        DeleteStatus = Gl.DELETE_STATUS,

        /// <summary>
        /// Strongly typed for value GL_LINK_STATUS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        LinkStatus = Gl.LINK_STATUS,

        /// <summary>
        /// Strongly typed for value GL_VALIDATE_STATUS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        ValidateStatus = Gl.VALIDATE_STATUS,

        /// <summary>
        /// Strongly typed for value GL_INFO_LOG_LENGTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        InfoLogLength = Gl.INFO_LOG_LENGTH,

        /// <summary>
        /// Strongly typed for value GL_ATTACHED_SHADERS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        AttachedShaders = Gl.ATTACHED_SHADERS,

        /// <summary>
        /// Strongly typed for value GL_ACTIVE_ATOMIC_COUNTER_BUFFERS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
        ActiveAtomicCounterBuffers = Gl.ACTIVE_ATOMIC_COUNTER_BUFFERS,

        /// <summary>
        /// Strongly typed for value GL_ACTIVE_ATTRIBUTES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        ActiveAttributes = Gl.ACTIVE_ATTRIBUTES,

        /// <summary>
        /// Strongly typed for value GL_ACTIVE_ATTRIBUTE_MAX_LENGTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        ActiveAttributeMaxLength = Gl.ACTIVE_ATTRIBUTE_MAX_LENGTH,

        /// <summary>
        /// Strongly typed for value GL_ACTIVE_UNIFORMS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        ActiveUniforms = Gl.ACTIVE_UNIFORMS,

        /// <summary>
        /// Strongly typed for value GL_ACTIVE_UNIFORM_BLOCKS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        ActiveUniformBlocks = Gl.ACTIVE_UNIFORM_BLOCKS,

        /// <summary>
        /// Strongly typed for value GL_ACTIVE_UNIFORM_BLOCK_MAX_NAME_LENGTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        ActiveUniformBlockMaxNameLength = Gl.ACTIVE_UNIFORM_BLOCK_MAX_NAME_LENGTH,

        /// <summary>
        /// Strongly typed for value GL_ACTIVE_UNIFORM_MAX_LENGTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        ActiveUniformMaxLength = Gl.ACTIVE_UNIFORM_MAX_LENGTH,

        /// <summary>
        /// Strongly typed for value GL_COMPUTE_WORK_GROUP_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_compute_shader", Api = "gl|glcore")]
        ComputeWorkGroupSize = Gl.COMPUTE_WORK_GROUP_SIZE,

        /// <summary>
        /// Strongly typed for value GL_PROGRAM_BINARY_LENGTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_get_program_binary", Api = "gl|glcore")]
        [RequiredByFeature("GL_OES_get_program_binary", Api = "gles2")]
        ProgramBinaryLength = Gl.PROGRAM_BINARY_LENGTH,

        /// <summary>
        /// Strongly typed for value GL_TRANSFORM_FEEDBACK_BUFFER_MODE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_transform_feedback")] [RequiredByFeature("GL_NV_transform_feedback")]
        TransformFeedbackBufferMode = Gl.TRANSFORM_FEEDBACK_BUFFER_MODE,

        /// <summary>
        /// Strongly typed for value GL_TRANSFORM_FEEDBACK_VARYINGS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_transform_feedback")] [RequiredByFeature("GL_NV_transform_feedback")]
        TransformFeedbackVaryings = Gl.TRANSFORM_FEEDBACK_VARYINGS,

        /// <summary>
        /// Strongly typed for value GL_TRANSFORM_FEEDBACK_VARYING_MAX_LENGTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_transform_feedback")]
        TransformFeedbackVaryingMaxLength = Gl.TRANSFORM_FEEDBACK_VARYING_MAX_LENGTH,

        /// <summary>
        /// Strongly typed for value GL_GEOMETRY_VERTICES_OUT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")] [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        GeometryVerticesOut = Gl.GEOMETRY_VERTICES_OUT,

        /// <summary>
        /// Strongly typed for value GL_GEOMETRY_INPUT_TYPE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")] [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        GeometryInputType = Gl.GEOMETRY_INPUT_TYPE,

        /// <summary>
        /// Strongly typed for value GL_GEOMETRY_OUTPUT_TYPE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")] [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        GeometryOutputType = Gl.GEOMETRY_OUTPUT_TYPE
    }

    /// <summary>
    /// Strongly typed enumeration ProgramStagePName.
    /// </summary>
    public enum ProgramStagePName
    {
        /// <summary>
        /// Strongly typed for value GL_ACTIVE_SUBROUTINE_UNIFORMS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_0")] [RequiredByFeature("GL_ARB_shader_subroutine", Api = "gl|glcore")]
        ActiveSubroutineUniforms = Gl.ACTIVE_SUBROUTINE_UNIFORMS,

        /// <summary>
        /// Strongly typed for value GL_ACTIVE_SUBROUTINE_UNIFORM_LOCATIONS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_0")] [RequiredByFeature("GL_ARB_shader_subroutine", Api = "gl|glcore")]
        ActiveSubroutineUniformLocations = Gl.ACTIVE_SUBROUTINE_UNIFORM_LOCATIONS,

        /// <summary>
        /// Strongly typed for value GL_ACTIVE_SUBROUTINES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_0")] [RequiredByFeature("GL_ARB_shader_subroutine", Api = "gl|glcore")]
        ActiveSubroutines = Gl.ACTIVE_SUBROUTINES,

        /// <summary>
        /// Strongly typed for value GL_ACTIVE_SUBROUTINE_UNIFORM_MAX_LENGTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_0")] [RequiredByFeature("GL_ARB_shader_subroutine", Api = "gl|glcore")]
        ActiveSubroutineUniformMaxLength = Gl.ACTIVE_SUBROUTINE_UNIFORM_MAX_LENGTH,

        /// <summary>
        /// Strongly typed for value GL_ACTIVE_SUBROUTINE_MAX_LENGTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_0")] [RequiredByFeature("GL_ARB_shader_subroutine", Api = "gl|glcore")]
        ActiveSubroutineMaxLength = Gl.ACTIVE_SUBROUTINE_MAX_LENGTH
    }

    /// <summary>
    /// Strongly typed enumeration QueryObjectParameterName.
    /// </summary>
    public enum QueryObjectParameterName
    {
        /// <summary>
        /// Strongly typed for value GL_QUERY_RESULT_AVAILABLE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_5")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_occlusion_query")]
        [RequiredByFeature("GL_EXT_disjoint_timer_query", Api = "gles2")]
        [RequiredByFeature("GL_EXT_occlusion_query_boolean", Api = "gles2")]
        QueryResultAvailable = Gl.QUERY_RESULT_AVAILABLE,

        /// <summary>
        /// Strongly typed for value GL_QUERY_RESULT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_5")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_occlusion_query")]
        [RequiredByFeature("GL_EXT_disjoint_timer_query", Api = "gles2")]
        [RequiredByFeature("GL_EXT_occlusion_query_boolean", Api = "gles2")]
        QueryResult = Gl.QUERY_RESULT,

        /// <summary>
        /// Strongly typed for value GL_QUERY_RESULT_NO_WAIT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_4")] [RequiredByFeature("GL_ARB_query_buffer_object", Api = "gl|glcore")] [RequiredByFeature("GL_AMD_query_buffer_object")]
        QueryResultNoWait = Gl.QUERY_RESULT_NO_WAIT,

        /// <summary>
        /// Strongly typed for value GL_QUERY_TARGET.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_direct_state_access", Api = "gl|glcore")]
        QueryTarget = Gl.QUERY_TARGET
    }

    /// <summary>
    /// Strongly typed enumeration QueryParameterName.
    /// </summary>
    public enum QueryParameterName
    {
        /// <summary>
        /// Strongly typed for value GL_CURRENT_QUERY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_5")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_occlusion_query")]
        [RequiredByFeature("GL_EXT_disjoint_timer_query", Api = "gles2")]
        [RequiredByFeature("GL_EXT_occlusion_query_boolean", Api = "gles2")]
        CurrentQuery = Gl.CURRENT_QUERY,

        /// <summary>
        /// Strongly typed for value GL_QUERY_COUNTER_BITS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_5")] [RequiredByFeature("GL_ARB_occlusion_query")] [RequiredByFeature("GL_EXT_disjoint_timer_query", Api = "gles2")]
        QueryCounterBits = Gl.QUERY_COUNTER_BITS
    }

    /// <summary>
    /// Strongly typed enumeration QueryTarget.
    /// </summary>
    public enum QueryTarget
    {
        /// <summary>
        /// Strongly typed for value GL_SAMPLES_PASSED.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_5")] [RequiredByFeature("GL_ARB_occlusion_query")]
        SamplesPassed = Gl.SAMPLES_PASSED,

        /// <summary>
        /// Strongly typed for value GL_ANY_SAMPLES_PASSED.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_3")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_occlusion_query2", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_occlusion_query_boolean", Api = "gles2")]
        AnySamplesPassed = Gl.ANY_SAMPLES_PASSED,

        /// <summary>
        /// Strongly typed for value GL_ANY_SAMPLES_PASSED_CONSERVATIVE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_ES3_compatibility", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_occlusion_query_boolean", Api = "gles2")]
        AnySamplesPassedConservative = Gl.ANY_SAMPLES_PASSED_CONSERVATIVE,

        /// <summary>
        /// Strongly typed for value GL_PRIMITIVES_GENERATED.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
        [RequiredByFeature("GL_EXT_transform_feedback")]
        [RequiredByFeature("GL_NV_transform_feedback")]
        [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
        PrimitivesGenerated = Gl.PRIMITIVES_GENERATED,

        /// <summary>
        /// Strongly typed for value GL_TRANSFORM_FEEDBACK_PRIMITIVES_WRITTEN.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_transform_feedback")] [RequiredByFeature("GL_NV_transform_feedback")]
        TransformFeedbackPrimitivesWritten = Gl.TRANSFORM_FEEDBACK_PRIMITIVES_WRITTEN,

        /// <summary>
        /// Strongly typed for value GL_TIME_ELAPSED.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_3")]
        [RequiredByFeature("GL_ARB_timer_query", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_disjoint_timer_query", Api = "gles2")]
        [RequiredByFeature("GL_EXT_timer_query")]
        TimeElapsed = Gl.TIME_ELAPSED
    }

    /// <summary>
    /// Strongly typed enumeration ReadBufferMode.
    /// </summary>
    public enum ReadBufferMode
    {
        /// <summary>
        /// Strongly typed for value GL_BACK.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_ES3_1_compatibility", Api = "gl|glcore")]
        Back = Gl.BACK,

        /// <summary>
        /// Strongly typed for value GL_BACK_LEFT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] BackLeft = Gl.BACK_LEFT,

        /// <summary>
        /// Strongly typed for value GL_BACK_RIGHT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] BackRight = Gl.BACK_RIGHT,

        /// <summary>
        /// Strongly typed for value GL_FRONT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Front = Gl.FRONT,

        /// <summary>
        /// Strongly typed for value GL_FRONT_LEFT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] FrontLeft = Gl.FRONT_LEFT,

        /// <summary>
        /// Strongly typed for value GL_FRONT_RIGHT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] FrontRight = Gl.FRONT_RIGHT,

        /// <summary>
        /// Strongly typed for value GL_LEFT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] Left = Gl.LEFT,

        /// <summary>
        /// Strongly typed for value GL_RIGHT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] Right = Gl.RIGHT,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT0.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        ColorAttachment0 = Gl.COLOR_ATTACHMENT0,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT1.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment1 = Gl.COLOR_ATTACHMENT1,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT2.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment2 = Gl.COLOR_ATTACHMENT2,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT3.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment3 = Gl.COLOR_ATTACHMENT3,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT4.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment4 = Gl.COLOR_ATTACHMENT4,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT5.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment5 = Gl.COLOR_ATTACHMENT5,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT6.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment6 = Gl.COLOR_ATTACHMENT6,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT7.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment7 = Gl.COLOR_ATTACHMENT7,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT8.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment8 = Gl.COLOR_ATTACHMENT8,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT9.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment9 = Gl.COLOR_ATTACHMENT9,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT10.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment10 = Gl.COLOR_ATTACHMENT10,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT11.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment11 = Gl.COLOR_ATTACHMENT11,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT12.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment12 = Gl.COLOR_ATTACHMENT12,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT13.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment13 = Gl.COLOR_ATTACHMENT13,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT14.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment14 = Gl.COLOR_ATTACHMENT14,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT15.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
        [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
        ColorAttachment15 = Gl.COLOR_ATTACHMENT15,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT16.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment16 = Gl.COLOR_ATTACHMENT16,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT17.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment17 = Gl.COLOR_ATTACHMENT17,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT18.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment18 = Gl.COLOR_ATTACHMENT18,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT19.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment19 = Gl.COLOR_ATTACHMENT19,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT20.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment20 = Gl.COLOR_ATTACHMENT20,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT21.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment21 = Gl.COLOR_ATTACHMENT21,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT22.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment22 = Gl.COLOR_ATTACHMENT22,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT23.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment23 = Gl.COLOR_ATTACHMENT23,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT24.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment24 = Gl.COLOR_ATTACHMENT24,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT25.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment25 = Gl.COLOR_ATTACHMENT25,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT26.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment26 = Gl.COLOR_ATTACHMENT26,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT27.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment27 = Gl.COLOR_ATTACHMENT27,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT28.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment28 = Gl.COLOR_ATTACHMENT28,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT29.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment29 = Gl.COLOR_ATTACHMENT29,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT30.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment30 = Gl.COLOR_ATTACHMENT30,

        /// <summary>
        /// Strongly typed for value GL_COLOR_ATTACHMENT31.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        ColorAttachment31 = Gl.COLOR_ATTACHMENT31
    }

    /// <summary>
    /// Strongly typed enumeration RenderbufferParameterName.
    /// </summary>
    public enum RenderbufferParameterName
    {
        /// <summary>
        /// Strongly typed for value GL_RENDERBUFFER_WIDTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        RenderbufferWidth = Gl.RENDERBUFFER_WIDTH,

        /// <summary>
        /// Strongly typed for value GL_RENDERBUFFER_HEIGHT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        RenderbufferHeight = Gl.RENDERBUFFER_HEIGHT,

        /// <summary>
        /// Strongly typed for value GL_RENDERBUFFER_INTERNAL_FORMAT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        RenderbufferInternalFormat = Gl.RENDERBUFFER_INTERNAL_FORMAT,

        /// <summary>
        /// Strongly typed for value GL_RENDERBUFFER_SAMPLES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_ANGLE_framebuffer_multisample", Api = "gles2")]
        [RequiredByFeature("GL_APPLE_framebuffer_multisample", Api = "gles1|gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_multisample")]
        [RequiredByFeature("GL_EXT_multisampled_render_to_texture", Api = "gles1|gles2")]
        [RequiredByFeature("GL_NV_framebuffer_multisample", Api = "gles2")]
        RenderbufferSamples = Gl.RENDERBUFFER_SAMPLES,

        /// <summary>
        /// Strongly typed for value GL_RENDERBUFFER_RED_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        RenderbufferRedSize = Gl.RENDERBUFFER_RED_SIZE,

        /// <summary>
        /// Strongly typed for value GL_RENDERBUFFER_GREEN_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        RenderbufferGreenSize = Gl.RENDERBUFFER_GREEN_SIZE,

        /// <summary>
        /// Strongly typed for value GL_RENDERBUFFER_BLUE_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        RenderbufferBlueSize = Gl.RENDERBUFFER_BLUE_SIZE,

        /// <summary>
        /// Strongly typed for value GL_RENDERBUFFER_ALPHA_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        RenderbufferAlphaSize = Gl.RENDERBUFFER_ALPHA_SIZE,

        /// <summary>
        /// Strongly typed for value GL_RENDERBUFFER_DEPTH_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        RenderbufferDepthSize = Gl.RENDERBUFFER_DEPTH_SIZE,

        /// <summary>
        /// Strongly typed for value GL_RENDERBUFFER_STENCIL_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        RenderbufferStencilSize = Gl.RENDERBUFFER_STENCIL_SIZE
    }

    /// <summary>
    /// Strongly typed enumeration RenderbufferTarget.
    /// </summary>
    public enum RenderbufferTarget
    {
        /// <summary>
        /// Strongly typed for value GL_RENDERBUFFER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        [RequiredByFeature("GL_NV_internalformat_sample_query", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_EXT_framebuffer_object")]
        [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
        Renderbuffer = Gl.RENDERBUFFER
    }

    /// <summary>
    /// Strongly typed enumeration SamplerParameterName.
    /// </summary>
    public enum SamplerParameterName
    {
        /// <summary>
        /// Strongly typed for value GL_TEXTURE_WRAP_S.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        TextureWrapS = Gl.TEXTURE_WRAP_S,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_WRAP_T.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        TextureWrapT = Gl.TEXTURE_WRAP_T,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_WRAP_R.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_texture3D")] [RequiredByFeature("GL_OES_texture_3D", Api = "gles2")]
        TextureWrapR = Gl.TEXTURE_WRAP_R,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_MIN_FILTER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        TextureMinFilter = Gl.TEXTURE_MIN_FILTER,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_MAG_FILTER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        TextureMagFilter = Gl.TEXTURE_MAG_FILTER,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_BORDER_COLOR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_EXT_texture_border_clamp", Api = "gles2")]
        [RequiredByFeature("GL_NV_texture_border_clamp", Api = "gles2")]
        [RequiredByFeature("GL_OES_texture_border_clamp", Api = "gles2")]
        TextureBorderColor = Gl.TEXTURE_BORDER_COLOR,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_MIN_LOD.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_SGIS_texture_lod")]
        TextureMinLod = Gl.TEXTURE_MIN_LOD,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_MAX_LOD.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_SGIS_texture_lod")]
        TextureMaxLod = Gl.TEXTURE_MAX_LOD,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_COMPARE_MODE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shadow")] [RequiredByFeature("GL_EXT_shadow_samplers", Api = "gles2")]
        TextureCompareMode = Gl.TEXTURE_COMPARE_MODE,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_COMPARE_FUNC.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shadow")] [RequiredByFeature("GL_EXT_shadow_samplers", Api = "gles2")]
        TextureCompareFunc = Gl.TEXTURE_COMPARE_FUNC
    }

    /// <summary>
    /// Strongly typed enumeration ShaderParameterName.
    /// </summary>
    public enum ShaderParameterName
    {
        /// <summary>
        /// Strongly typed for value GL_SHADER_TYPE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        ShaderType = Gl.SHADER_TYPE,

        /// <summary>
        /// Strongly typed for value GL_DELETE_STATUS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        DeleteStatus = Gl.DELETE_STATUS,

        /// <summary>
        /// Strongly typed for value GL_COMPILE_STATUS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        CompileStatus = Gl.COMPILE_STATUS,

        /// <summary>
        /// Strongly typed for value GL_INFO_LOG_LENGTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        InfoLogLength = Gl.INFO_LOG_LENGTH,

        /// <summary>
        /// Strongly typed for value GL_SHADER_SOURCE_LENGTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        ShaderSourceLength = Gl.SHADER_SOURCE_LENGTH
    }

    /// <summary>
    /// Strongly typed enumeration ShaderType.
    /// </summary>
    public enum ShaderType
    {
        /// <summary>
        /// Strongly typed for value GL_COMPUTE_SHADER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_compute_shader", Api = "gl|glcore")]
        ComputeShader = Gl.COMPUTE_SHADER,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_SHADER, GL_VERTEX_SHADER_ARB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_vertex_shader")]
        VertexShader = Gl.VERTEX_SHADER,

        /// <summary>
        /// Strongly typed for value GL_TESS_CONTROL_SHADER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_0")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
        TessControlShader = Gl.TESS_CONTROL_SHADER,

        /// <summary>
        /// Strongly typed for value GL_TESS_EVALUATION_SHADER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_0")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
        TessEvaluationShader = Gl.TESS_EVALUATION_SHADER,

        /// <summary>
        /// Strongly typed for value GL_GEOMETRY_SHADER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_geometry_shader4", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
        [RequiredByFeature("GL_EXT_geometry_shader4")]
        [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
        GeometryShader = Gl.GEOMETRY_SHADER,

        /// <summary>
        /// Strongly typed for value GL_FRAGMENT_SHADER, GL_FRAGMENT_SHADER_ARB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_ARB_fragment_shader")]
        FragmentShader = Gl.FRAGMENT_SHADER
    }

    /// <summary>
    /// Strongly typed enumeration StencilFaceDirection.
    /// </summary>
    public enum StencilFaceDirection
    {
        /// <summary>
        /// Strongly typed for value GL_FRONT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Front = Gl.FRONT,

        /// <summary>
        /// Strongly typed for value GL_BACK.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_4_5")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_ES3_1_compatibility", Api = "gl|glcore")]
        Back = Gl.BACK,

        /// <summary>
        /// Strongly typed for value GL_FRONT_AND_BACK.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        FrontAndBack = Gl.FRONT_AND_BACK
    }

    /// <summary>
    /// Strongly typed enumeration StencilFunction.
    /// </summary>
    public enum StencilFunction
    {
        /// <summary>
        /// Strongly typed for value GL_ALWAYS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Always = Gl.ALWAYS,

        /// <summary>
        /// Strongly typed for value GL_EQUAL.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
        Equal = Gl.EQUAL,

        /// <summary>
        /// Strongly typed for value GL_GEQUAL.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Gequal = Gl.GEQUAL,

        /// <summary>
        /// Strongly typed for value GL_GREATER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Greater = Gl.GREATER,

        /// <summary>
        /// Strongly typed for value GL_LEQUAL.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Lequal = Gl.LEQUAL,

        /// <summary>
        /// Strongly typed for value GL_LESS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Less = Gl.LESS,

        /// <summary>
        /// Strongly typed for value GL_NEVER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Never = Gl.NEVER,

        /// <summary>
        /// Strongly typed for value GL_NOTEQUAL.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Notequal = Gl.NOTEQUAL
    }

    /// <summary>
    /// Strongly typed enumeration StencilOp.
    /// </summary>
    public enum StencilOp
    {
        /// <summary>
        /// Strongly typed for value GL_DECR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Decr = Gl.DECR,

        /// <summary>
        /// Strongly typed for value GL_DECR_WRAP.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_stencil_wrap")]
        [RequiredByFeature("GL_OES_stencil_wrap", Api = "gles1")]
        DecrWrap = Gl.DECR_WRAP,

        /// <summary>
        /// Strongly typed for value GL_INCR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Incr = Gl.INCR,

        /// <summary>
        /// Strongly typed for value GL_INCR_WRAP.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_stencil_wrap")]
        [RequiredByFeature("GL_OES_stencil_wrap", Api = "gles1")]
        IncrWrap = Gl.INCR_WRAP,

        /// <summary>
        /// Strongly typed for value GL_INVERT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        Invert = Gl.INVERT,

        /// <summary>
        /// Strongly typed for value GL_KEEP.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Keep = Gl.KEEP,

        /// <summary>
        /// Strongly typed for value GL_REPLACE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Replace = Gl.REPLACE,

        /// <summary>
        /// Strongly typed for value GL_ZERO.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_draw_buffers_indexed", Api = "gles2")]
        [RequiredByFeature("GL_NV_blend_equation_advanced", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_NV_register_combiners")]
        [RequiredByFeature("GL_OES_draw_buffers_indexed", Api = "gles2")]
        Zero = Gl.ZERO
    }

    /// <summary>
    /// Strongly typed enumeration StringName.
    /// </summary>
    public enum StringName
    {
        /// <summary>
        /// Strongly typed for value GL_EXTENSIONS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Extensions = Gl.EXTENSIONS,

        /// <summary>
        /// Strongly typed for value GL_RENDERER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Renderer = Gl.RENDERER,

        /// <summary>
        /// Strongly typed for value GL_VENDOR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Vendor = Gl.VENDOR,

        /// <summary>
        /// Strongly typed for value GL_VERSION.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Version = Gl.VERSION,

        /// <summary>
        /// Strongly typed for value GL_SHADING_LANGUAGE_VERSION.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_shading_language_100")]
        ShadingLanguageVersion = Gl.SHADING_LANGUAGE_VERSION
    }

    /// <summary>
    /// Strongly typed enumeration SubroutineParameterName.
    /// </summary>
    public enum SubroutineParameterName
    {
        /// <summary>
        /// Strongly typed for value GL_NUM_COMPATIBLE_SUBROUTINES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_0")] [RequiredByFeature("GL_ARB_program_interface_query", Api = "gl|glcore")] [RequiredByFeature("GL_ARB_shader_subroutine", Api = "gl|glcore")]
        NumCompatibleSubroutines = Gl.NUM_COMPATIBLE_SUBROUTINES,

        /// <summary>
        /// Strongly typed for value GL_COMPATIBLE_SUBROUTINES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_0")] [RequiredByFeature("GL_ARB_program_interface_query", Api = "gl|glcore")] [RequiredByFeature("GL_ARB_shader_subroutine", Api = "gl|glcore")]
        CompatibleSubroutines = Gl.COMPATIBLE_SUBROUTINES,

        /// <summary>
        /// Strongly typed for value GL_UNIFORM_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_subroutine", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        UniformSize = Gl.UNIFORM_SIZE,

        /// <summary>
        /// Strongly typed for value GL_UNIFORM_NAME_LENGTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_subroutine", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        UniformNameLength = Gl.UNIFORM_NAME_LENGTH
    }

    /// <summary>
    /// Strongly typed enumeration SyncCondition.
    /// </summary>
    public enum SyncCondition
    {
        /// <summary>
        /// Strongly typed for value GL_SYNC_GPU_COMMANDS_COMPLETE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_sync", Api = "gl|glcore")]
        [RequiredByFeature("GL_APPLE_sync", Api = "gles1|gles2")]
        SyncGpuCommandsComplete = Gl.SYNC_GPU_COMMANDS_COMPLETE
    }

    /// <summary>
    /// Strongly typed enumeration SyncObjectMask.
    /// </summary>
    [Flags]
    public enum SyncObjectMask : uint
    {
        /// <summary>
        /// Strongly typed for value GL_SYNC_FLUSH_COMMANDS_BIT, GL_SYNC_FLUSH_COMMANDS_BIT_APPLE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_sync", Api = "gl|glcore")]
        [RequiredByFeature("GL_APPLE_sync", Api = "gles1|gles2")]
        SyncFlushCommandsBit = Gl.SYNC_FLUSH_COMMANDS_BIT
    }

    /// <summary>
    /// Strongly typed enumeration SyncParameterName.
    /// </summary>
    public enum SyncParameterName
    {
        /// <summary>
        /// Strongly typed for value GL_OBJECT_TYPE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_sync", Api = "gl|glcore")]
        [RequiredByFeature("GL_APPLE_sync", Api = "gles1|gles2")]
        ObjectType = Gl.OBJECT_TYPE,

        /// <summary>
        /// Strongly typed for value GL_SYNC_STATUS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_sync", Api = "gl|glcore")]
        [RequiredByFeature("GL_APPLE_sync", Api = "gles1|gles2")]
        SyncStatus = Gl.SYNC_STATUS,

        /// <summary>
        /// Strongly typed for value GL_SYNC_CONDITION.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_sync", Api = "gl|glcore")]
        [RequiredByFeature("GL_APPLE_sync", Api = "gles1|gles2")]
        SyncCondition = Gl.SYNC_CONDITION,

        /// <summary>
        /// Strongly typed for value GL_SYNC_FLAGS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_sync", Api = "gl|glcore")]
        [RequiredByFeature("GL_APPLE_sync", Api = "gles1|gles2")]
        SyncFlags = Gl.SYNC_FLAGS
    }

    /// <summary>
    /// Strongly typed enumeration SyncStatus.
    /// </summary>
    public enum SyncStatus
    {
        /// <summary>
        /// Strongly types for value GL_SIGNALED symbol.
        /// This is the returned value when checking fence status.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_sync", Api = "gl|glcore")]
        [RequiredByFeature("GL_APPLE_sync", Api = "gles1|gles2")]
        Signaled = Gl.SIGNALED,

        /// <summary>
        /// Strongly typed for value GL_ALREADY_SIGNALED.
        /// This is the returned value when blocking on a fence.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_sync", Api = "gl|glcore")]
        [RequiredByFeature("GL_APPLE_sync", Api = "gles1|gles2")]
        AlreadySignaled = Gl.ALREADY_SIGNALED,

        /// <summary>
        /// Strongly typed for value GL_TIMEOUT_EXPIRED.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_sync", Api = "gl|glcore")]
        [RequiredByFeature("GL_APPLE_sync", Api = "gles1|gles2")]
        TimeoutExpired = Gl.TIMEOUT_EXPIRED,

        /// <summary>
        /// Strongly typed for value GL_CONDITION_SATISFIED.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_sync", Api = "gl|glcore")]
        [RequiredByFeature("GL_APPLE_sync", Api = "gles1|gles2")]
        ConditionSatisfied = Gl.CONDITION_SATISFIED,

        /// <summary>
        /// Strongly typed for value GL_WAIT_FAILED.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_sync", Api = "gl|glcore")]
        [RequiredByFeature("GL_APPLE_sync", Api = "gles1|gles2")]
        WaitFailed = Gl.WAIT_FAILED
    }

    /// <summary>
    /// Strongly typed enumeration TexCoordPointerType.
    /// </summary>
    public enum TexCoordPointerType
    {
        /// <summary>
        /// Strongly typed for value GL_DOUBLE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_ARB_gpu_shader_fp64", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_vertex_attrib_64bit")]
        Double = Gl.DOUBLE,

        /// <summary>
        /// Strongly typed for value GL_FLOAT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_OES_texture_float", Api = "gles2")]
        Float = Gl.FLOAT,

        /// <summary>
        /// Strongly typed for value GL_INT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Int = Gl.INT,

        /// <summary>
        /// Strongly typed for value GL_SHORT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_render_snorm", Api = "gles2")]
        Short = Gl.SHORT
    }

    /// <summary>
    /// Strongly typed enumeration TextureMagFilter.
    /// </summary>
    public enum TextureMagFilter
    {
        /// <summary>
        /// Strongly typed for value GL_LINEAR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Linear = Gl.LINEAR,

        /// <summary>
        /// Strongly typed for value GL_NEAREST.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Nearest = Gl.NEAREST
    }

    /// <summary>
    /// Strongly typed enumeration TextureMinFilter.
    /// </summary>
    public enum TextureMinFilter
    {
        /// <summary>
        /// Strongly typed for value GL_LINEAR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Linear = Gl.LINEAR,

        /// <summary>
        /// Strongly typed for value GL_LINEAR_MIPMAP_LINEAR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        LinearMipmapLinear = Gl.LINEAR_MIPMAP_LINEAR,

        /// <summary>
        /// Strongly typed for value GL_LINEAR_MIPMAP_NEAREST.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        LinearMipmapNearest = Gl.LINEAR_MIPMAP_NEAREST,

        /// <summary>
        /// Strongly typed for value GL_NEAREST.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Nearest = Gl.NEAREST,

        /// <summary>
        /// Strongly typed for value GL_NEAREST_MIPMAP_LINEAR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        NearestMipmapLinear = Gl.NEAREST_MIPMAP_LINEAR,

        /// <summary>
        /// Strongly typed for value GL_NEAREST_MIPMAP_NEAREST.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        NearestMipmapNearest = Gl.NEAREST_MIPMAP_NEAREST
    }

    /// <summary>
    /// Strongly typed enumeration TextureParameterName.
    /// </summary>
    public enum TextureParameterName
    {
        /// <summary>
        /// Strongly typed for value GL_GENERATE_MIPMAP, GL_GENERATE_MIPMAP_SGIS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")] [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")] [RequiredByFeature("GL_SGIS_generate_mipmap")] [RemovedByFeature("GL_VERSION_3_2")]
        GenerateMipmap = Gl.GENERATE_MIPMAP,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_BORDER_COLOR, GL_TEXTURE_BORDER_COLOR_NV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_EXT_texture_border_clamp", Api = "gles2")]
        [RequiredByFeature("GL_NV_texture_border_clamp", Api = "gles2")]
        [RequiredByFeature("GL_OES_texture_border_clamp", Api = "gles2")]
        TextureBorderColor = Gl.TEXTURE_BORDER_COLOR,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_MAG_FILTER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        TextureMagFilter = Gl.TEXTURE_MAG_FILTER,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_MIN_FILTER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        TextureMinFilter = Gl.TEXTURE_MIN_FILTER,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_PRIORITY, GL_TEXTURE_PRIORITY_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture_object")] [RemovedByFeature("GL_VERSION_3_2")]
        TexturePriority = Gl.TEXTURE_PRIORITY,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_WRAP_R, GL_TEXTURE_WRAP_R_EXT, GL_TEXTURE_WRAP_R_OES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_texture3D")] [RequiredByFeature("GL_OES_texture_3D", Api = "gles2")]
        TextureWrapR = Gl.TEXTURE_WRAP_R,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_WRAP_S.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        TextureWrapS = Gl.TEXTURE_WRAP_S,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_WRAP_T.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        TextureWrapT = Gl.TEXTURE_WRAP_T,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_BASE_LEVEL, GL_TEXTURE_BASE_LEVEL_SGIS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_SGIS_texture_lod")]
        TextureBaseLevel = Gl.TEXTURE_BASE_LEVEL,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_COMPARE_MODE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shadow")] [RequiredByFeature("GL_EXT_shadow_samplers", Api = "gles2")]
        TextureCompareMode = Gl.TEXTURE_COMPARE_MODE,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_COMPARE_FUNC.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_shadow")] [RequiredByFeature("GL_EXT_shadow_samplers", Api = "gles2")]
        TextureCompareFunc = Gl.TEXTURE_COMPARE_FUNC,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_LOD_BIAS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_4")] [RequiredByFeature("GL_EXT_texture_lod_bias", Api = "gl|gles1")]
        TextureLodBias = Gl.TEXTURE_LOD_BIAS,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_MIN_LOD, GL_TEXTURE_MIN_LOD_SGIS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_SGIS_texture_lod")]
        TextureMinLod = Gl.TEXTURE_MIN_LOD,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_MAX_LOD, GL_TEXTURE_MAX_LOD_SGIS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_SGIS_texture_lod")]
        TextureMaxLod = Gl.TEXTURE_MAX_LOD,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_MAX_LEVEL, GL_TEXTURE_MAX_LEVEL_SGIS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_APPLE_texture_max_level", Api = "gles1|gles2")]
        [RequiredByFeature("GL_SGIS_texture_lod")]
        TextureMaxLevel = Gl.TEXTURE_MAX_LEVEL,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_SWIZZLE_R.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_3")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_texture_swizzle", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_texture_swizzle")]
        TextureSwizzleR = Gl.TEXTURE_SWIZZLE_R,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_SWIZZLE_G.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_3")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_texture_swizzle", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_texture_swizzle")]
        TextureSwizzleG = Gl.TEXTURE_SWIZZLE_G,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_SWIZZLE_B.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_3")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_texture_swizzle", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_texture_swizzle")]
        TextureSwizzleB = Gl.TEXTURE_SWIZZLE_B,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_SWIZZLE_A.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_3")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_texture_swizzle", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_texture_swizzle")]
        TextureSwizzleA = Gl.TEXTURE_SWIZZLE_A,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_SWIZZLE_RGBA.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_3")] [RequiredByFeature("GL_ARB_texture_swizzle", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_texture_swizzle")]
        TextureSwizzleRgba = Gl.TEXTURE_SWIZZLE_RGBA,

        /// <summary>
        /// Strongly typed for value GL_DEPTH_STENCIL_TEXTURE_MODE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_stencil_texturing", Api = "gl|glcore")]
        DepthStencilTextureMode = Gl.DEPTH_STENCIL_TEXTURE_MODE,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_ALPHA_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_EXT_texture")]
        TextureAlphaSize = Gl.TEXTURE_ALPHA_SIZE,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_BLUE_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_EXT_texture")]
        TextureBlueSize = Gl.TEXTURE_BLUE_SIZE,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_COMPONENTS, GL_TEXTURE_INTERNAL_FORMAT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        TextureInternalFormat = Gl.TEXTURE_INTERNAL_FORMAT,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_DEPTH_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_EXT_texture3D")]
        TextureDepth = Gl.TEXTURE_DEPTH,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_GREEN_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_EXT_texture")]
        TextureGreenSize = Gl.TEXTURE_GREEN_SIZE,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_HEIGHT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        TextureHeight = Gl.TEXTURE_HEIGHT,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_INTENSITY_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture")] [RemovedByFeature("GL_VERSION_3_2")]
        TextureIntensitySize = Gl.TEXTURE_INTENSITY_SIZE,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_LUMINANCE_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture")] [RemovedByFeature("GL_VERSION_3_2")]
        TextureLuminanceSize = Gl.TEXTURE_LUMINANCE_SIZE,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_RED_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_EXT_texture")]
        TextureRedSize = Gl.TEXTURE_RED_SIZE,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_RESIDENT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture_object")] [RemovedByFeature("GL_VERSION_3_2")]
        TextureResident = Gl.TEXTURE_RESIDENT,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_WIDTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        TextureWidth = Gl.TEXTURE_WIDTH
    }

    /// <summary>
    /// Strongly typed enumeration TextureTarget.
    /// </summary>
    public enum TextureTarget
    {
        /// <summary>
        /// Strongly typed for value GL_PROXY_TEXTURE_1D, GL_PROXY_TEXTURE_1D_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture")]
        ProxyTexture1d = Gl.PROXY_TEXTURE_1D,

        /// <summary>
        /// Strongly typed for value GL_PROXY_TEXTURE_1D_ARRAY, GL_PROXY_TEXTURE_1D_ARRAY_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_EXT_texture_array")]
        ProxyTexture1dArray = Gl.PROXY_TEXTURE_1D_ARRAY,

        /// <summary>
        /// Strongly typed for value GL_PROXY_TEXTURE_2D, GL_PROXY_TEXTURE_2D_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_EXT_texture")]
        ProxyTexture2d = Gl.PROXY_TEXTURE_2D,

        /// <summary>
        /// Strongly typed for value GL_PROXY_TEXTURE_2D_ARRAY, GL_PROXY_TEXTURE_2D_ARRAY_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_EXT_texture_array")]
        ProxyTexture2dArray = Gl.PROXY_TEXTURE_2D_ARRAY,

        /// <summary>
        /// Strongly typed for value GL_PROXY_TEXTURE_2D_MULTISAMPLE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")] [RequiredByFeature("GL_ARB_texture_multisample", Api = "gl|glcore")]
        ProxyTexture2dMultisample = Gl.PROXY_TEXTURE_2D_MULTISAMPLE,

        /// <summary>
        /// Strongly typed for value GL_PROXY_TEXTURE_2D_MULTISAMPLE_ARRAY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")] [RequiredByFeature("GL_ARB_texture_multisample", Api = "gl|glcore")]
        ProxyTexture2dMultisampleArray = Gl.PROXY_TEXTURE_2D_MULTISAMPLE_ARRAY,

        /// <summary>
        /// Strongly typed for value GL_PROXY_TEXTURE_3D, GL_PROXY_TEXTURE_3D_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_EXT_texture3D")]
        ProxyTexture3d = Gl.PROXY_TEXTURE_3D,

        /// <summary>
        /// Strongly typed for value GL_PROXY_TEXTURE_CUBE_MAP, GL_PROXY_TEXTURE_CUBE_MAP_ARB, GL_PROXY_TEXTURE_CUBE_MAP_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")] [RequiredByFeature("GL_ARB_texture_cube_map")] [RequiredByFeature("GL_EXT_texture_cube_map")]
        ProxyTextureCubeMap = Gl.PROXY_TEXTURE_CUBE_MAP,

        /// <summary>
        /// Strongly typed for value GL_PROXY_TEXTURE_CUBE_MAP_ARRAY, GL_PROXY_TEXTURE_CUBE_MAP_ARRAY_ARB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_0")] [RequiredByFeature("GL_ARB_texture_cube_map_array", Api = "gl|glcore")]
        ProxyTextureCubeMapArray = Gl.PROXY_TEXTURE_CUBE_MAP_ARRAY,

        /// <summary>
        /// Strongly typed for value GL_PROXY_TEXTURE_RECTANGLE, GL_PROXY_TEXTURE_RECTANGLE_ARB, GL_PROXY_TEXTURE_RECTANGLE_NV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ARB_texture_rectangle")] [RequiredByFeature("GL_NV_texture_rectangle")]
        ProxyTextureRectangle = Gl.PROXY_TEXTURE_RECTANGLE,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_1D.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        Texture1d = Gl.TEXTURE_1D,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_2D.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_sparse_texture", Api = "gles2")]
        Texture2d = Gl.TEXTURE_2D,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_3D, GL_TEXTURE_3D_EXT, GL_TEXTURE_3D_OES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_sparse_texture", Api = "gles2")]
        [RequiredByFeature("GL_EXT_texture3D")]
        [RequiredByFeature("GL_OES_texture_3D", Api = "gles2")]
        Texture3d = Gl.TEXTURE_3D,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_RECTANGLE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")]
        [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_texture_rectangle")]
        [RequiredByFeature("GL_NV_texture_rectangle")]
        TextureRectangle = Gl.TEXTURE_RECTANGLE,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_CUBE_MAP.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_sparse_texture", Api = "gles2")]
        [RequiredByFeature("GL_ARB_texture_cube_map")]
        [RequiredByFeature("GL_EXT_texture_cube_map")]
        [RequiredByFeature("GL_OES_texture_cube_map", Api = "gles1")]
        TextureCubeMap = Gl.TEXTURE_CUBE_MAP,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_CUBE_MAP_POSITIVE_X.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_texture_cube_map")]
        [RequiredByFeature("GL_EXT_texture_cube_map")]
        [RequiredByFeature("GL_OES_texture_cube_map", Api = "gles1")]
        TextureCubeMapPositiveX = Gl.TEXTURE_CUBE_MAP_POSITIVE_X,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_CUBE_MAP_NEGATIVE_X.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_texture_cube_map")]
        [RequiredByFeature("GL_EXT_texture_cube_map")]
        [RequiredByFeature("GL_OES_texture_cube_map", Api = "gles1")]
        TextureCubeMapNegativeX = Gl.TEXTURE_CUBE_MAP_NEGATIVE_X,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_CUBE_MAP_POSITIVE_Y.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_texture_cube_map")]
        [RequiredByFeature("GL_EXT_texture_cube_map")]
        [RequiredByFeature("GL_OES_texture_cube_map", Api = "gles1")]
        TextureCubeMapPositiveY = Gl.TEXTURE_CUBE_MAP_POSITIVE_Y,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_CUBE_MAP_NEGATIVE_Y.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_texture_cube_map")]
        [RequiredByFeature("GL_EXT_texture_cube_map")]
        [RequiredByFeature("GL_OES_texture_cube_map", Api = "gles1")]
        TextureCubeMapNegativeY = Gl.TEXTURE_CUBE_MAP_NEGATIVE_Y,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_CUBE_MAP_POSITIVE_Z.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_texture_cube_map")]
        [RequiredByFeature("GL_EXT_texture_cube_map")]
        [RequiredByFeature("GL_OES_texture_cube_map", Api = "gles1")]
        TextureCubeMapPositiveZ = Gl.TEXTURE_CUBE_MAP_POSITIVE_Z,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_CUBE_MAP_NEGATIVE_Z.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_texture_cube_map")]
        [RequiredByFeature("GL_EXT_texture_cube_map")]
        [RequiredByFeature("GL_OES_texture_cube_map", Api = "gles1")]
        TextureCubeMapNegativeZ = Gl.TEXTURE_CUBE_MAP_NEGATIVE_Z,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_CUBE_MAP_ARRAY, GL_TEXTURE_CUBE_MAP_ARRAY_ARB, GL_TEXTURE_CUBE_MAP_ARRAY_EXT,
        /// GL_TEXTURE_CUBE_MAP_ARRAY_OES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_0")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_texture_cube_map_array", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_texture_cube_map_array", Api = "gles2")]
        [RequiredByFeature("GL_EXT_sparse_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_texture_cube_map_array", Api = "gles2")]
        TextureCubeMapArray = Gl.TEXTURE_CUBE_MAP_ARRAY,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_1D_ARRAY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_texture_array")]
        Texture1dArray = Gl.TEXTURE_1D_ARRAY,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_2D_ARRAY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_sparse_texture", Api = "gles2")]
        [RequiredByFeature("GL_EXT_texture_array")]
        Texture2dArray = Gl.TEXTURE_2D_ARRAY,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_2D_MULTISAMPLE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_texture_multisample", Api = "gl|glcore")]
        [RequiredByFeature("GL_NV_internalformat_sample_query", Api = "gl|glcore|gles2")]
        Texture2dMultisample = Gl.TEXTURE_2D_MULTISAMPLE,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE_2D_MULTISAMPLE_ARRAY.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_internalformat_query2", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_texture_multisample", Api = "gl|glcore")]
        [RequiredByFeature("GL_NV_internalformat_sample_query", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_OES_texture_storage_multisample_2d_array", Api = "gles2")]
        Texture2dMultisampleArray = Gl.TEXTURE_2D_MULTISAMPLE_ARRAY
    }

    /// <summary>
    /// Strongly typed enumeration TextureUnit.
    /// </summary>
    public enum TextureUnit
    {
        /// <summary>
        /// Strongly typed for value GL_TEXTURE0.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        [RequiredByFeature("GL_NV_register_combiners")]
        Texture0 = Gl.TEXTURE0,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE1.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        [RequiredByFeature("GL_NV_register_combiners")]
        Texture1 = Gl.TEXTURE1,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE2.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture2 = Gl.TEXTURE2,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE3.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture3 = Gl.TEXTURE3,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE4.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture4 = Gl.TEXTURE4,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE5.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture5 = Gl.TEXTURE5,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE6.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture6 = Gl.TEXTURE6,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE7.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture7 = Gl.TEXTURE7,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE8.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture8 = Gl.TEXTURE8,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE9.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture9 = Gl.TEXTURE9,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE10.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture10 = Gl.TEXTURE10,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE11.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture11 = Gl.TEXTURE11,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE12.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture12 = Gl.TEXTURE12,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE13.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture13 = Gl.TEXTURE13,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE14.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture14 = Gl.TEXTURE14,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE15.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture15 = Gl.TEXTURE15,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE16.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture16 = Gl.TEXTURE16,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE17.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture17 = Gl.TEXTURE17,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE18.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture18 = Gl.TEXTURE18,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE19.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture19 = Gl.TEXTURE19,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE20.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture20 = Gl.TEXTURE20,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE21.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture21 = Gl.TEXTURE21,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE22.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture22 = Gl.TEXTURE22,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE23.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture23 = Gl.TEXTURE23,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE24.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture24 = Gl.TEXTURE24,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE25.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture25 = Gl.TEXTURE25,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE26.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture26 = Gl.TEXTURE26,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE27.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture27 = Gl.TEXTURE27,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE28.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture28 = Gl.TEXTURE28,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE29.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture29 = Gl.TEXTURE29,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE30.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture30 = Gl.TEXTURE30,

        /// <summary>
        /// Strongly typed for value GL_TEXTURE31.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_multitexture")]
        Texture31 = Gl.TEXTURE31
    }

    /// <summary>
    /// Strongly typed enumeration TextureWrapMode.
    /// </summary>
    public enum TextureWrapMode
    {
        /// <summary>
        /// Strongly typed for value GL_CLAMP_TO_BORDER, GL_CLAMP_TO_BORDER_ARB, GL_CLAMP_TO_BORDER_NV, GL_CLAMP_TO_BORDER_SGIS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_3")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_texture_border_clamp", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_texture_border_clamp", Api = "gles2")]
        [RequiredByFeature("GL_NV_texture_border_clamp", Api = "gles2")]
        [RequiredByFeature("GL_SGIS_texture_border_clamp")]
        [RequiredByFeature("GL_OES_texture_border_clamp", Api = "gles2")]
        ClampToBorder = Gl.CLAMP_TO_BORDER,

        /// <summary>
        /// Strongly typed for value GL_CLAMP_TO_EDGE, GL_CLAMP_TO_EDGE_SGIS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_SGIS_texture_edge_clamp")]
        ClampToEdge = Gl.CLAMP_TO_EDGE,

        /// <summary>
        /// Strongly typed for value GL_REPEAT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Repeat = Gl.REPEAT
    }

    /// <summary>
    /// Strongly typed enumeration TransformFeedbackPName.
    /// </summary>
    public enum TransformFeedbackPName
    {
        /// <summary>
        /// Strongly typed for value GL_TRANSFORM_FEEDBACK_BUFFER_BINDING.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_transform_feedback")] [RequiredByFeature("GL_NV_transform_feedback")]
        TransformFeedbackBufferBinding = Gl.TRANSFORM_FEEDBACK_BUFFER_BINDING,

        /// <summary>
        /// Strongly typed for value GL_TRANSFORM_FEEDBACK_BUFFER_START.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_transform_feedback")] [RequiredByFeature("GL_NV_transform_feedback")]
        TransformFeedbackBufferStart = Gl.TRANSFORM_FEEDBACK_BUFFER_START,

        /// <summary>
        /// Strongly typed for value GL_TRANSFORM_FEEDBACK_BUFFER_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_transform_feedback")] [RequiredByFeature("GL_NV_transform_feedback")]
        TransformFeedbackBufferSize = Gl.TRANSFORM_FEEDBACK_BUFFER_SIZE,

        /// <summary>
        /// Strongly typed for value GL_TRANSFORM_FEEDBACK_PAUSED.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_0")] [RequiredByFeature("GL_ARB_transform_feedback2", Api = "gl|glcore")] [RequiredByFeature("GL_NV_transform_feedback2")]
        TransformFeedbackBufferPaused = Gl.TRANSFORM_FEEDBACK_BUFFER_PAUSED,

        /// <summary>
        /// Strongly typed for value GL_TRANSFORM_FEEDBACK_ACTIVE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_0")] [RequiredByFeature("GL_ARB_transform_feedback2", Api = "gl|glcore")] [RequiredByFeature("GL_NV_transform_feedback2")]
        TransformFeedbackBufferActive = Gl.TRANSFORM_FEEDBACK_BUFFER_ACTIVE
    }

    /// <summary>
    /// Strongly typed enumeration TransformFeedbackTarget.
    /// </summary>
    public enum TransformFeedbackTarget
    {
        /// <summary>
        /// Strongly typed for value GL_TRANSFORM_FEEDBACK.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_transform_feedback2", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_debug_label", Api = "gl|glcore|gles2")]
        [RequiredByFeature("GL_NV_transform_feedback2")]
        TransformFeedback = Gl.TRANSFORM_FEEDBACK
    }

    /// <summary>
    /// Strongly typed enumeration UniformBlockPName.
    /// </summary>
    public enum UniformBlockPName
    {
        /// <summary>
        /// Strongly typed for value GL_UNIFORM_BLOCK_BINDING.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        UniformBlockBinding = Gl.UNIFORM_BLOCK_BINDING,

        /// <summary>
        /// Strongly typed for value GL_UNIFORM_BLOCK_DATA_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        UniformBlockDataSize = Gl.UNIFORM_BLOCK_DATA_SIZE,

        /// <summary>
        /// Strongly typed for value GL_UNIFORM_BLOCK_NAME_LENGTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        UniformBlockNameLength = Gl.UNIFORM_BLOCK_NAME_LENGTH,

        /// <summary>
        /// Strongly typed for value GL_UNIFORM_BLOCK_ACTIVE_UNIFORMS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        UniformBlockActiveUniforms = Gl.UNIFORM_BLOCK_ACTIVE_UNIFORMS,

        /// <summary>
        /// Strongly typed for value GL_UNIFORM_BLOCK_ACTIVE_UNIFORM_INDICES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        UniformBlockActiveUniformIndices = Gl.UNIFORM_BLOCK_ACTIVE_UNIFORM_INDICES,

        /// <summary>
        /// Strongly typed for value GL_UNIFORM_BLOCK_REFERENCED_BY_VERTEX_SHADER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        UniformBlockReferencedByVertexShader = Gl.UNIFORM_BLOCK_REFERENCED_BY_VERTEX_SHADER,

        /// <summary>
        /// Strongly typed for value GL_UNIFORM_BLOCK_REFERENCED_BY_TESS_CONTROL_SHADER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_0")] [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
        UniformBlockReferencedByTessControlShader = Gl.UNIFORM_BLOCK_REFERENCED_BY_TESS_CONTROL_SHADER,

        /// <summary>
        /// Strongly typed for value GL_UNIFORM_BLOCK_REFERENCED_BY_TESS_EVALUATION_SHADER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_0")] [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
        UniformBlockReferencedByTessEvaluationShader = Gl.UNIFORM_BLOCK_REFERENCED_BY_TESS_EVALUATION_SHADER,

        /// <summary>
        /// Strongly typed for value GL_UNIFORM_BLOCK_REFERENCED_BY_GEOMETRY_SHADER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        UniformBlockReferencedByGeometryShader = Gl.UNIFORM_BLOCK_REFERENCED_BY_GEOMETRY_SHADER,

        /// <summary>
        /// Strongly typed for value GL_UNIFORM_BLOCK_REFERENCED_BY_FRAGMENT_SHADER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        UniformBlockReferencedByFragmentShader = Gl.UNIFORM_BLOCK_REFERENCED_BY_FRAGMENT_SHADER,

        /// <summary>
        /// Strongly typed for value GL_UNIFORM_BLOCK_REFERENCED_BY_COMPUTE_SHADER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ARB_compute_shader", Api = "gl|glcore")]
        UniformBlockReferencedByComputeShader = Gl.UNIFORM_BLOCK_REFERENCED_BY_COMPUTE_SHADER
    }

    /// <summary>
    /// Strongly typed enumeration UniformPName.
    /// </summary>
    public enum UniformPName
    {
        /// <summary>
        /// Strongly typed for value GL_UNIFORM_TYPE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        UniformType = Gl.UNIFORM_TYPE,

        /// <summary>
        /// Strongly typed for value GL_UNIFORM_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_subroutine", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        UniformSize = Gl.UNIFORM_SIZE,

        /// <summary>
        /// Strongly typed for value GL_UNIFORM_NAME_LENGTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_shader_subroutine", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        UniformNameLength = Gl.UNIFORM_NAME_LENGTH,

        /// <summary>
        /// Strongly typed for value GL_UNIFORM_BLOCK_INDEX.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        UniformBlockIndex = Gl.UNIFORM_BLOCK_INDEX,

        /// <summary>
        /// Strongly typed for value GL_UNIFORM_OFFSET.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        UniformOffset = Gl.UNIFORM_OFFSET,

        /// <summary>
        /// Strongly typed for value GL_UNIFORM_ARRAY_STRIDE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        UniformArrayStride = Gl.UNIFORM_ARRAY_STRIDE,

        /// <summary>
        /// Strongly typed for value GL_UNIFORM_MATRIX_STRIDE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        UniformMatrixStride = Gl.UNIFORM_MATRIX_STRIDE,

        /// <summary>
        /// Strongly typed for value GL_UNIFORM_IS_ROW_MAJOR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
        UniformIsRowMajor = Gl.UNIFORM_IS_ROW_MAJOR,

        /// <summary>
        /// Strongly typed for value GL_UNIFORM_ATOMIC_COUNTER_BUFFER_INDEX.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_2")] [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
        UniformAtomicCounterBufferIndex = Gl.UNIFORM_ATOMIC_COUNTER_BUFFER_INDEX
    }

    /// <summary>
    /// Strongly typed enumeration UseProgramStageMask.
    /// </summary>
    [Flags]
    public enum UseProgramStageMask : uint
    {
        /// <summary>
        /// Strongly typed for value GL_VERTEX_SHADER_BIT, GL_VERTEX_SHADER_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_ARB_separate_shader_objects", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_separate_shader_objects", Api = "gl|glcore|gles2")]
        VertexShaderBit = Gl.VERTEX_SHADER_BIT,

        /// <summary>
        /// Strongly typed for value GL_FRAGMENT_SHADER_BIT, GL_FRAGMENT_SHADER_BIT_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_ARB_separate_shader_objects", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_separate_shader_objects", Api = "gl|glcore|gles2")]
        FragmentShaderBit = Gl.FRAGMENT_SHADER_BIT,

        /// <summary>
        /// Strongly typed for value GL_GEOMETRY_SHADER_BIT, GL_GEOMETRY_SHADER_BIT_EXT, GL_GEOMETRY_SHADER_BIT_OES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_separate_shader_objects", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
        GeometryShaderBit = Gl.GEOMETRY_SHADER_BIT,

        /// <summary>
        /// Strongly typed for value GL_TESS_CONTROL_SHADER_BIT, GL_TESS_CONTROL_SHADER_BIT_EXT, GL_TESS_CONTROL_SHADER_BIT_OES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_separate_shader_objects", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
        TessControlShaderBit = Gl.TESS_CONTROL_SHADER_BIT,

        /// <summary>
        /// Strongly typed for value GL_TESS_EVALUATION_SHADER_BIT, GL_TESS_EVALUATION_SHADER_BIT_EXT,
        /// GL_TESS_EVALUATION_SHADER_BIT_OES.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_separate_shader_objects", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
        [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
        TessEvaluationShaderBit = Gl.TESS_EVALUATION_SHADER_BIT,

        /// <summary>
        /// Strongly typed for value GL_COMPUTE_SHADER_BIT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_compute_shader", Api = "gl|glcore")]
        ComputeShaderBit = Gl.COMPUTE_SHADER_BIT,

        /// <summary>
        /// Strongly typed for value GL_ALL_SHADER_BITS, GL_ALL_SHADER_BITS_EXT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
        [RequiredByFeature("GL_ARB_separate_shader_objects", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_separate_shader_objects", Api = "gl|glcore|gles2")]
        AllShaderBits = Gl.ALL_SHADER_BITS
    }

    /// <summary>
    /// Strongly typed enumeration VertexArrayPName.
    /// </summary>
    public enum VertexArrayPName
    {
        /// <summary>
        /// Strongly typed for value GL_VERTEX_ATTRIB_ARRAY_ENABLED.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        VertexAttribArrayEnabled = Gl.VERTEX_ATTRIB_ARRAY_ENABLED,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_ATTRIB_ARRAY_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        VertexAttribArraySize = Gl.VERTEX_ATTRIB_ARRAY_SIZE,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_ATTRIB_ARRAY_STRIDE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        VertexAttribArrayStride = Gl.VERTEX_ATTRIB_ARRAY_STRIDE,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_ATTRIB_ARRAY_TYPE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        VertexAttribArrayType = Gl.VERTEX_ATTRIB_ARRAY_TYPE,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_ATTRIB_ARRAY_NORMALIZED.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        VertexAttribArrayNormalized = Gl.VERTEX_ATTRIB_ARRAY_NORMALIZED,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_ATTRIB_ARRAY_INTEGER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_gpu_shader4")] [RequiredByFeature("GL_NV_vertex_program4")]
        VertexAttribArrayInteger = Gl.VERTEX_ATTRIB_ARRAY_INTEGER,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_ATTRIB_ARRAY_LONG.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] VertexAttribArrayLong = Gl.VERTEX_ATTRIB_ARRAY_LONG,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_ATTRIB_ARRAY_DIVISOR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_3")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ANGLE_instanced_arrays", Api = "gles2")]
        [RequiredByFeature("GL_ARB_instanced_arrays", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_instanced_arrays", Api = "gles2")]
        [RequiredByFeature("GL_NV_instanced_arrays", Api = "gles2")]
        VertexAttribArrayDivisor = Gl.VERTEX_ATTRIB_ARRAY_DIVISOR,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_ATTRIB_RELATIVE_OFFSET.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")] [RequiredByFeature("GL_ARB_vertex_attrib_binding", Api = "gl|glcore")]
        VertexAttribRelativeOffset = Gl.VERTEX_ATTRIB_RELATIVE_OFFSET
    }

    /// <summary>
    /// Strongly typed enumeration VertexAttribEnum.
    /// </summary>
    public enum VertexAttribEnum
    {
        /// <summary>
        /// Strongly typed for value GL_VERTEX_ATTRIB_ARRAY_BUFFER_BINDING.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_5")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_buffer_object")]
        VertexAttribArrayBufferBinding = Gl.VERTEX_ATTRIB_ARRAY_BUFFER_BINDING,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_ATTRIB_ARRAY_ENABLED.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        VertexAttribArrayEnabled = Gl.VERTEX_ATTRIB_ARRAY_ENABLED,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_ATTRIB_ARRAY_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        VertexAttribArraySize = Gl.VERTEX_ATTRIB_ARRAY_SIZE,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_ATTRIB_ARRAY_STRIDE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        VertexAttribArrayStride = Gl.VERTEX_ATTRIB_ARRAY_STRIDE,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_ATTRIB_ARRAY_TYPE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        VertexAttribArrayType = Gl.VERTEX_ATTRIB_ARRAY_TYPE,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_ATTRIB_ARRAY_NORMALIZED.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        VertexAttribArrayNormalized = Gl.VERTEX_ATTRIB_ARRAY_NORMALIZED,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_ATTRIB_ARRAY_INTEGER.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_gpu_shader4")] [RequiredByFeature("GL_NV_vertex_program4")]
        VertexAttribArrayInteger = Gl.VERTEX_ATTRIB_ARRAY_INTEGER,

        /// <summary>
        /// Strongly typed for value GL_VERTEX_ATTRIB_ARRAY_DIVISOR.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_3")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ANGLE_instanced_arrays", Api = "gles2")]
        [RequiredByFeature("GL_ARB_instanced_arrays", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_instanced_arrays", Api = "gles2")]
        [RequiredByFeature("GL_NV_instanced_arrays", Api = "gles2")]
        VertexAttribArrayDivisor = Gl.VERTEX_ATTRIB_ARRAY_DIVISOR,

        /// <summary>
        /// Strongly typed for value GL_CURRENT_VERTEX_ATTRIB.
        /// </summary>
        [RequiredByFeature("GL_VERSION_2_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_program")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        CurrentVertexAttrib = Gl.CURRENT_VERTEX_ATTRIB
    }

    /// <summary>
    /// Strongly typed enumeration VertexAttribType.
    /// </summary>
    public enum VertexAttribType
    {
        /// <summary>
        /// Strongly typed for value GL_BYTE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_render_snorm", Api = "gles2")]
        [RequiredByFeature("GL_OES_byte_coordinates", Api = "gl|gles1")]
        Byte = Gl.BYTE,

        /// <summary>
        /// Strongly typed for value GL_SHORT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_render_snorm", Api = "gles2")]
        Short = Gl.SHORT,

        /// <summary>
        /// Strongly typed for value GL_INT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Int = Gl.INT,

        /// <summary>
        /// Strongly typed for value GL_FIXED.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_1")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_ES2_compatibility", Api = "gl|glcore")]
        [RequiredByFeature("GL_OES_fixed_point", Api = "gl|gles1")]
        Fixed = Gl.FIXED,

        /// <summary>
        /// Strongly typed for value GL_FLOAT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_OES_texture_float", Api = "gles2")]
        Float = Gl.FLOAT,

        /// <summary>
        /// Strongly typed for value GL_HALF_FLOAT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_half_float_vertex", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_half_float_pixel")]
        [RequiredByFeature("GL_NV_half_float")]
        HalfFloat = Gl.HALF_FLOAT,

        /// <summary>
        /// Strongly typed for value GL_DOUBLE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_ARB_gpu_shader_fp64", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_vertex_attrib_64bit")]
        Double = Gl.DOUBLE,

        /// <summary>
        /// Strongly typed for value GL_UNSIGNED_BYTE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        UnsignedByte = Gl.UNSIGNED_BYTE,

        /// <summary>
        /// Strongly typed for value GL_UNSIGNED_SHORT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ANGLE_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_depth_texture", Api = "gles2")]
        UnsignedShort = Gl.UNSIGNED_SHORT,

        /// <summary>
        /// Strongly typed for value GL_UNSIGNED_INT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ANGLE_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_depth_texture", Api = "gles2")]
        [RequiredByFeature("GL_OES_element_index_uint", Api = "gles1|gles2")]
        UnsignedInt = Gl.UNSIGNED_INT,

        /// <summary>
        /// Strongly typed for value GL_INT_2_10_10_10_REV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_3")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_vertex_type_2_10_10_10_rev", Api = "gl|glcore")]
        Int2101010Rev = Gl.INT_2_10_10_10_REV,

        /// <summary>
        /// Strongly typed for value GL_UNSIGNED_INT_2_10_10_10_REV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_2")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_vertex_type_2_10_10_10_rev", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_texture_type_2_10_10_10_REV", Api = "gles2")]
        UnsignedInt2101010Rev = Gl.UNSIGNED_INT_2_10_10_10_REV,

        /// <summary>
        /// Strongly typed for value GL_UNSIGNED_INT_10F_11F_11F_REV.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")]
        [RequiredByFeature("GL_VERSION_4_4")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_vertex_type_10f_11f_11f_rev", Api = "gl|glcore")]
        [RequiredByFeature("GL_APPLE_texture_packed_float", Api = "gles2")]
        [RequiredByFeature("GL_EXT_packed_float")]
        UnsignedInt10F11F11FRev = Gl.UNSIGNED_INT_10F_11F_11F_REV
    }

    /// <summary>
    /// Strongly typed enumeration VertexBufferObjectParameter.
    /// </summary>
    public enum VertexBufferObjectParameter
    {
        /// <summary>
        /// Strongly typed for value GL_BUFFER_ACCESS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_5")] [RequiredByFeature("GL_ARB_vertex_buffer_object")] [RequiredByFeature("GL_OES_mapbuffer", Api = "gles1|gles2")]
        BufferAccess = Gl.BUFFER_ACCESS,

        /// <summary>
        /// Strongly typed for value GL_BUFFER_ACCESS_FLAGS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        BufferAccessFlags = Gl.BUFFER_ACCESS_FLAGS,

        /// <summary>
        /// Strongly typed for value GL_BUFFER_IMMUTABLE_STORAGE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_4")] [RequiredByFeature("GL_ARB_buffer_storage", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_buffer_storage", Api = "gles2")]
        BufferImmutableStorage = Gl.BUFFER_IMMUTABLE_STORAGE,

        /// <summary>
        /// Strongly typed for value GL_BUFFER_MAPPED.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_5")]
        [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        [RequiredByFeature("GL_ARB_vertex_buffer_object")]
        [RequiredByFeature("GL_OES_mapbuffer", Api = "gles1|gles2")]
        BufferMapped = Gl.BUFFER_MAPPED,

        /// <summary>
        /// Strongly typed for value GL_BUFFER_MAP_LENGTH.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        BufferMapLength = Gl.BUFFER_MAP_LENGTH,

        /// <summary>
        /// Strongly typed for value GL_BUFFER_MAP_OFFSET.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
        BufferMapOffset = Gl.BUFFER_MAP_OFFSET,

        /// <summary>
        /// Strongly typed for value GL_BUFFER_SIZE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_5")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_buffer_object")]
        BufferSize = Gl.BUFFER_SIZE,

        /// <summary>
        /// Strongly typed for value GL_BUFFER_STORAGE_FLAGS.
        /// </summary>
        [RequiredByFeature("GL_VERSION_4_4")] [RequiredByFeature("GL_ARB_buffer_storage", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_buffer_storage", Api = "gles2")]
        BufferStorageFlags = Gl.BUFFER_STORAGE_FLAGS,

        /// <summary>
        /// Strongly typed for value GL_BUFFER_USAGE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_5")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_buffer_object")]
        BufferUsage = Gl.BUFFER_USAGE
    }

    /// <summary>
    /// Strongly typed enumeration VertexPointerType.
    /// </summary>
    public enum VertexPointerType
    {
        /// <summary>
        /// Strongly typed for value GL_DOUBLE.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_1")] [RequiredByFeature("GL_ARB_gpu_shader_fp64", Api = "gl|glcore")] [RequiredByFeature("GL_EXT_vertex_attrib_64bit")]
        Double = Gl.DOUBLE,

        /// <summary>
        /// Strongly typed for value GL_FLOAT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_ARB_vertex_shader")]
        [RequiredByFeature("GL_OES_texture_float", Api = "gles2")]
        Float = Gl.FLOAT,

        /// <summary>
        /// Strongly typed for value GL_INT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")] [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")] [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        Int = Gl.INT,

        /// <summary>
        /// Strongly typed for value GL_SHORT.
        /// </summary>
        [RequiredByFeature("GL_VERSION_1_0")]
        [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
        [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
        [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
        [RequiredByFeature("GL_EXT_render_snorm", Api = "gles2")]
        Short = Gl.SHORT
    }

    /// <summary>
    /// Strongly typed enumeration VertexProvokingMode.
    /// </summary>
    public enum VertexProvokingMode
    {
        /// <summary>
        /// Strongly typed for value GL_FIRST_VERTEX_CONVENTION.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_provoking_vertex", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_viewport_array", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
        [RequiredByFeature("GL_EXT_provoking_vertex")]
        [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
        FirstVertexConvention = Gl.FIRST_VERTEX_CONVENTION,

        /// <summary>
        /// Strongly typed for value GL_LAST_VERTEX_CONVENTION.
        /// </summary>
        [RequiredByFeature("GL_VERSION_3_2")]
        [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
        [RequiredByFeature("GL_ARB_provoking_vertex", Api = "gl|glcore")]
        [RequiredByFeature("GL_ARB_viewport_array", Api = "gl|glcore")]
        [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
        [RequiredByFeature("GL_EXT_provoking_vertex")]
        [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
        LastVertexConvention = Gl.LAST_VERTEX_CONVENTION
    }
}