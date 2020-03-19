#region Using

using Khronos;

#endregion

// ReSharper disable InheritdocConsiderUsage
// ReSharper disable SwitchStatementMissingSomeCases
// ReSharper disable RedundantIfElseBlock
// ReSharper disable once CheckNamespace
namespace OpenGL
{
    public partial class Gl
    {
        /// <summary>
        /// Limits support listing.
        /// </summary>
        public sealed partial class Limits
        {
            /// <summary>
            /// [GL4] Gl.Get: data returns one value, the maximum number of application-defined clipping distances. The value must be
            /// at
            /// least 8.
            /// </summary>
            [Limit(MAX_CLIP_DISTANCES)] [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_APPLE_clip_distance", Api = "gles2")]
            public int MaxClipDistances;

            /// <summary>
            ///     <para>
            ///     [GL4] Gl.Get: data returns one value. The value gives a rough estimate of the largest texture that the GL can
            ///     handle.
            ///     The value must be at least 1024. Use a proxy texture target such as Gl.PROXY_TEXTURE_1D or Gl.PROXY_TEXTURE_2D to
            ///     determine if a texture is too large. See Gl.TexImage1D and Gl.TexImage2D.
            ///     </para>
            ///     <para>
            ///     [GLES3.2] Gl.Get: data returns one value. The value gives a rough estimate of the largest texture that the GL can
            ///     handle. The value must be at least 2048. See Gl.TexImage2D.
            ///     </para>
            /// </summary>
            [Limit(MAX_TEXTURE_SIZE)]
            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            public int MaxTextureSize;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns two values: the maximum supported width and height of the viewport. These must be at
            /// least as large as the visible dimensions of the display being rendered to. See Gl.Viewport.
            /// </summary>
            [Limit(MAX_VIEWPORT_DIMS, ArrayLength = 2)]
            [RequiredByFeature("GL_VERSION_1_0")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            public int[] MaxViewportDims = {0, 0};

            /// <summary>
            ///     <para>
            ///     [GL4] Gl.Get: data returns one value, a rough estimate of the largest 3D texture that the GL can handle. The value
            ///     must
            ///     be at least 64. Use Gl.PROXY_TEXTURE_3D to determine if a texture is too large. See Gl.TexImage3D.
            ///     </para>
            ///     <para>
            ///     [GLES3.2] Gl.Get: data returns one value, a rough estimate of the largest 3D texture that the GL can handle. The
            ///     value
            ///     must be at least 256. See Gl.TexImage3D.
            ///     </para>
            /// </summary>
            [Limit(MAX_3D_TEXTURE_SIZE)]
            [RequiredByFeature("GL_VERSION_1_2")]
            [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
            [RequiredByFeature("GL_EXT_texture3D")]
            [RequiredByFeature("GL_OES_texture_3D", Api = "gles2")]
            public int Max3dTextureSize;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the recommended maximum number of vertex array vertices. See
            /// Gl.DrawRangeElements.
            /// </summary>
            [Limit(MAX_ELEMENTS_VERTICES)] [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_draw_range_elements")]
            public int MaxElementsVertices;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the recommended maximum number of vertex array indices. See
            /// Gl.DrawRangeElements.
            /// </summary>
            [Limit(MAX_ELEMENTS_INDICES)] [RequiredByFeature("GL_VERSION_1_2")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_draw_range_elements")]
            public int MaxElementsIndices;

            /// <summary>
            /// [GL4] Gl.Get: data returns one value, the maximum number of simultaneous viewports that are supported. The value must
            /// be
            /// at least 16. See Gl.ViewportIndexed.
            /// </summary>
            [Limit(MAX_VIEWPORTS)]
            [RequiredByFeature("GL_VERSION_4_1")]
            [RequiredByFeature("GL_ARB_viewport_array", Api = "gl|glcore")]
            [RequiredByFeature("GL_NV_viewport_array", Api = "gles2")]
            [RequiredByFeature("GL_OES_viewport_array", Api = "gles2")]
            public int MaxViewports;

            /// <summary>
            /// [GL] Value of GL_MAX_COMPUTE_SHARED_MEMORY_SIZE symbol.
            /// </summary>
            [Limit(MAX_COMPUTE_SHARED_MEMORY_SIZE)]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_compute_shader", Api = "gl|glcore")]
            public int MaxComputeSharedMemorySize;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum number of individual floating-point, integer, or boolean
            /// values that can be held in uniform variable storage for a compute shader. The value must be at least 1024. See
            /// Gl.Uniform.
            /// </summary>
            [Limit(MAX_COMPUTE_UNIFORM_COMPONENTS)]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_compute_shader", Api = "gl|glcore")]
            public int MaxComputeUniformComponents;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns a single value, the maximum number of atomic counter buffers that may be accessed by
            /// a compute shader.
            /// </summary>
            [Limit(MAX_COMPUTE_ATOMIC_COUNTER_BUFFERS)]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_compute_shader", Api = "gl|glcore")]
            public int MaxComputeAtomicCounterBuffers;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns a single value, the maximum number of atomic counters available to compute shaders.
            /// </summary>
            [Limit(MAX_COMPUTE_ATOMIC_COUNTERS)]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_compute_shader", Api = "gl|glcore")]
            public int MaxComputeAtomicCounters;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the number of words for compute shader uniform variables in all uniform
            /// blocks (including default). The value must be at least 1. See Gl.Uniform.
            /// </summary>
            [Limit(MAX_COMBINED_COMPUTE_UNIFORM_COMPONENTS)]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_compute_shader", Api = "gl|glcore")]
            public int MaxCombinedComputeUniformComponents;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns a single value, the maximum depth of the debug message group stack.
            /// </summary>
            [Limit(MAX_DEBUG_GROUP_STACK_DEPTH)]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
            public int MaxDebugGroupStackDepth;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum number of explicitly assignable uniform locations, which must
            /// be at least 1024.
            /// </summary>
            [Limit(MAX_UNIFORM_LOCATIONS)]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_explicit_uniform_location", Api = "gl|glcore")]
            public int MaxUniformLocations;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns a single integer value containing the maximum offset that may be added to a vertex
            /// binding offset.
            /// </summary>
            [Limit(MAX_VERTEX_ATTRIB_RELATIVE_OFFSET)]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_vertex_attrib_binding", Api = "gl|glcore")]
            public int MaxVertexAttribRelativeOffset;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns a single integer value containing the maximum number of vertex buffers that may be
            /// bound.
            /// </summary>
            [Limit(MAX_VERTEX_ATTRIB_BINDINGS)]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_vertex_attrib_binding", Api = "gl|glcore")]
            public int MaxVertexAttribBindings;

            /// <summary>
            /// [GL] Value of GL_MAX_VERTEX_ATTRIB_STRIDE symbol.
            /// </summary>
            [Limit(MAX_VERTEX_ATTRIB_STRIDE)] [RequiredByFeature("GL_VERSION_4_4")] [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            public int MaxVertexAttribStride;

            /// <summary>
            ///     <para>
            ///     [GL4] Gl.Get: data returns one value, the maximum length of a label that may be assigned to an object. See
            ///     Gl.ObjectLabel and Gl.ObjectPtrLabel.
            ///     </para>
            ///     <para>
            ///     [GLES3.2] Gl.Get: data returns a single value, the maximum length of a label string.
            ///     </para>
            /// </summary>
            [Limit(MAX_LABEL_LENGTH)] [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")] [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
            public int MaxLabelLength;

            /// <summary>
            /// [GL] Value of GL_MAX_CULL_DISTANCES symbol.
            /// </summary>
            [Limit(MAX_CULL_DISTANCES)] [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_cull_distance", Api = "gl|glcore")]
            public int MaxCullDistances;

            /// <summary>
            /// [GL] Value of GL_MAX_COMBINED_CLIP_AND_CULL_DISTANCES symbol.
            /// </summary>
            [Limit(MAX_COMBINED_CLIP_AND_CULL_DISTANCES)] [RequiredByFeature("GL_VERSION_4_5")] [RequiredByFeature("GL_ARB_cull_distance", Api = "gl|glcore")]
            public int MaxCombinedClipAndCullDistances;

            /// <summary>
            ///     <para>
            ///     [GL4] Gl.Get: data returns one value. The value indicates the maximum supported size for renderbuffers. See
            ///     Gl.FramebufferRenderbuffer.
            ///     </para>
            ///     <para>
            ///     [GLES3.2] Gl.Get: data returns one value. The value indicates the maximum supported size for renderbuffers and must
            ///     be
            ///     at least 2048. See Gl.FramebufferRenderbuffer.
            ///     </para>
            /// </summary>
            [Limit(MAX_RENDERBUFFER_SIZE)]
            [RequiredByFeature("GL_VERSION_3_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_framebuffer_object")]
            [RequiredByFeature("GL_OES_framebuffer_object", Api = "gles1")]
            public int MaxRenderbufferSize;

            /// <summary>
            /// [GL4] Gl.Get: data returns one value. The value gives a rough estimate of the largest rectangular texture that the GL
            /// can handle. The value must be at least 1024. Use Gl.PROXY_TEXTURE_RECTANGLE to determine if a texture is too large. See
            /// Gl.TexImage2D.
            /// </summary>
            [Limit(MAX_RECTANGLE_TEXTURE_SIZE)] [RequiredByFeature("GL_VERSION_3_1")] [RequiredByFeature("GL_ARB_texture_rectangle")] [RequiredByFeature("GL_NV_texture_rectangle")]
            public int MaxRectangleTextureSize;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum, absolute value of the texture level-of-detail bias. The
            /// value
            /// must be at least 2.0.
            /// </summary>
            [Limit(MAX_TEXTURE_LOD_BIAS)] [RequiredByFeature("GL_VERSION_1_4")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_texture_lod_bias", Api = "gl|gles1")]
            public int MaxTextureLodBias;

            /// <summary>
            /// [GL] Value of GL_MAX_TEXTURE_MAX_ANISOTROPY symbol.
            /// </summary>
            [Limit(MAX_TEXTURE_MAX_ANISOTROPY)] [RequiredByFeature("GL_VERSION_4_6")] [RequiredByFeature("GL_ARB_texture_filter_anisotropic", Api = "gl|glcore")]
            public int MaxTextureMaxAnisotropy;

            /// <summary>
            ///     <para>
            ///     [GL4] Gl.Get: data returns one value. The value gives a rough estimate of the largest cube-map texture that the GL
            ///     can
            ///     handle. The value must be at least 1024. Use Gl.PROXY_TEXTURE_CUBE_MAP to determine if a texture is too large. See
            ///     Gl.TexImage2D.
            ///     </para>
            ///     <para>
            ///     [GLES3.2] Gl.Get: data returns one value. The value gives a rough estimate of the largest cube-map texture that the
            ///     GL
            ///     can handle. The value must be at least 2048. See Gl.TexImage2D.
            ///     </para>
            /// </summary>
            [Limit(MAX_CUBE_MAP_TEXTURE_SIZE)]
            [RequiredByFeature("GL_VERSION_1_3")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_texture_cube_map")]
            [RequiredByFeature("GL_EXT_texture_cube_map")]
            [RequiredByFeature("GL_OES_texture_cube_map", Api = "gles1")]
            public int MaxCubeMapTextureSize;

            /// <summary>
            ///     <para>
            ///     [GL4] Gl.Get: data returns one value, the maximum number of simultaneous outputs that may be written in a fragment
            ///     shader. The value must be at least 8. See Gl.DrawBuffers.
            ///     </para>
            ///     <para>
            ///     [GLES3.2] Gl.Get: data returns one value, the maximum number of simultaneous outputs that may be written in a
            ///     fragment
            ///     shader. The value must be at least 4. See Gl.DrawBuffers.
            ///     </para>
            /// </summary>
            [Limit(MAX_DRAW_BUFFERS)]
            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_draw_buffers")]
            [RequiredByFeature("GL_ATI_draw_buffers")]
            [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
            [RequiredByFeature("GL_NV_draw_buffers", Api = "gles2")]
            public int MaxDrawBuffers;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum number of 4-component generic vertex attributes accessible to
            /// a vertex shader. The value must be at least 16. See Gl.VertexAttrib.
            /// </summary>
            [Limit(MAX_VERTEX_ATTRIBS)]
            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            public int MaxVertexAttribs;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value, the maximum number of components of inputs read by a tesselation control
            /// shader, which must be at least 128.
            /// </summary>
            [Limit(MAX_TESS_CONTROL_INPUT_COMPONENTS)]
            [RequiredByFeature("GL_VERSION_4_0")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
            public int MaxTessControlInputComponents;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value, the maximum number of components of inputs read by a tesselation evaluation
            /// shader, which must be at least 128.
            /// </summary>
            [Limit(MAX_TESS_EVALUATION_INPUT_COMPONENTS)]
            [RequiredByFeature("GL_VERSION_4_0")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
            public int MaxTessEvaluationInputComponents;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum supported texture image units that can be used to access
            /// texture maps from the fragment shader. The value must be at least 16. See Gl.ActiveTexture.
            /// </summary>
            [Limit(MAX_TEXTURE_IMAGE_UNITS)]
            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_fragment_program")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_fragment_program")]
            public int MaxTextureImageUnits;

            /// <summary>
            /// [GL4] Gl.Get: data returns one value, the maximum number of active draw buffers when using dual-source blending. The
            /// value must be at least 1. See Gl.BlendFunc and Gl.BlendFuncSeparate.
            /// </summary>
            [Limit(MAX_DUAL_SOURCE_DRAW_BUFFERS)]
            [RequiredByFeature("GL_VERSION_3_3")]
            [RequiredByFeature("GL_ARB_blend_func_extended", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_blend_func_extended", Api = "gles2")]
            public int MaxDualSourceDrawBuffers;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value. The value indicates the maximum number of layers allowed in an array
            /// texture, and must be at least 256. See Gl.TexImage2D.
            /// </summary>
            [Limit(MAX_ARRAY_TEXTURE_LAYERS)] [RequiredByFeature("GL_VERSION_3_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_EXT_texture_array")]
            public int MaxArrayTextureLayers;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the minimum texel offset allowed in a texture lookup, which must be at
            /// most -8.
            /// </summary>
            [Limit(MIN_PROGRAM_TEXEL_OFFSET)]
            [RequiredByFeature("GL_VERSION_3_0")]
            [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
            [RequiredByFeature("GL_EXT_gpu_shader4")]
            [RequiredByFeature("GL_NV_gpu_program4")]
            public int MinProgramTexelOffset;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum texel offset allowed in a texture lookup, which must be at
            /// least 7.
            /// </summary>
            [Limit(MAX_PROGRAM_TEXEL_OFFSET)]
            [RequiredByFeature("GL_VERSION_3_0")]
            [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
            [RequiredByFeature("GL_EXT_gpu_shader4")]
            [RequiredByFeature("GL_NV_gpu_program4")]
            public int MaxProgramTexelOffset;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum number of uniform blocks per vertex shader. The value must be
            /// at least 12. See Gl.UniformBlockBinding.
            /// </summary>
            [Limit(MAX_VERTEX_UNIFORM_BLOCKS)]
            [RequiredByFeature("GL_VERSION_3_1")]
            [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
            public int MaxVertexUniformBlocks;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum number of uniform blocks per fragment shader. The value must
            /// be at least 12. See Gl.UniformBlockBinding.
            /// </summary>
            [Limit(MAX_FRAGMENT_UNIFORM_BLOCKS)]
            [RequiredByFeature("GL_VERSION_3_1")]
            [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
            public int MaxFragmentUniformBlocks;

            /// <summary>
            ///     <para>
            ///     [GL4] Gl.Get: data returns one value, the maximum number of uniform blocks per program. The value must be at least
            ///     70.
            ///     See Gl.UniformBlockBinding.
            ///     </para>
            ///     <para>
            ///     [GLES3.2] Gl.Get: data returns one value, the maximum number of uniform blocks per program. The value must be at
            ///     least
            ///     60. See Gl.UniformBlockBinding.
            ///     </para>
            /// </summary>
            [Limit(MAX_COMBINED_UNIFORM_BLOCKS)]
            [RequiredByFeature("GL_VERSION_3_1")]
            [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
            public int MaxCombinedUniformBlocks;

            /// <summary>
            ///     <para>
            ///     [GL4] Gl.Get: data returns one value, the maximum number of uniform buffer binding points on the context, which
            ///     must be
            ///     at least 36.
            ///     </para>
            ///     <para>
            ///     [GLES3.2] Gl.Get: data returns one value, the maximum number of uniform buffer binding points on the context, which
            ///     must
            ///     be at least 72.
            ///     </para>
            /// </summary>
            [Limit(MAX_UNIFORM_BUFFER_BINDINGS)]
            [RequiredByFeature("GL_VERSION_3_1")]
            [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
            public int MaxUniformBufferBindings;

            /// <summary>
            ///     <para>
            ///     [GL4] Gl.Get: data returns one value, the maximum size in basic machine units of a uniform block, which must be at
            ///     least
            ///     16384.
            ///     </para>
            ///     <para>
            ///     [GLES3.2] Gl.Get: data returns one value, the maximum size in basic machine units of a uniform block. The value
            ///     must be
            ///     at least 16384. See Gl.UniformBlockBinding.
            ///     </para>
            /// </summary>
            [Limit(MAX_UNIFORM_BLOCK_SIZE)]
            [RequiredByFeature("GL_VERSION_3_1")]
            [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
            public int MaxUniformBlockSize;

            /// <summary>
            ///     <para>
            ///     [GL4] Gl.Get: data returns one value, the number of words for vertex shader uniform variables in all uniform blocks
            ///     (including default). The value must be at least 1. See Gl.Uniform.
            ///     </para>
            ///     <para>
            ///     [GLES3.2] Gl.Get: data returns one value, the number of words for vertex shader uniform variables in all uniform
            ///     blocks
            ///     (including default). The value must be at least . Gl.MAX_VERTEX_UNIFORM_COMPONENTS + Gl.MAX_UNIFORM_BLOCK_SIZE *
            ///     Gl.MAX_VERTEX_UNIFORM_BLOCKS / 4. See Gl.Uniform.
            ///     </para>
            /// </summary>
            [Limit(MAX_COMBINED_VERTEX_UNIFORM_COMPONENTS)]
            [RequiredByFeature("GL_VERSION_3_1")]
            [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
            public int MaxCombinedVertexUniformComponents;

            /// <summary>
            ///     <para>
            ///     [GL4] Gl.Get: data returns one value, the number of words for fragment shader uniform variables in all uniform
            ///     blocks
            ///     (including default). The value must be at least 1. See Gl.Uniform.
            ///     </para>
            ///     <para>
            ///     [GLES3.2] Gl.Get: data returns one value, the number of words for fragment shader uniform variables in all uniform
            ///     blocks (including default). The value must be at least Gl.MAX_FRAGMENT_UNIFORM_COMPONENTS +
            ///     Gl.MAX_UNIFORM_BLOCK_SIZE *
            ///     Gl.MAX_FRAGMENT_UNIFORM_BLOCKS / 4. See Gl.Uniform.
            ///     </para>
            /// </summary>
            [Limit(MAX_COMBINED_FRAGMENT_UNIFORM_COMPONENTS)]
            [RequiredByFeature("GL_VERSION_3_1")]
            [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
            public int MaxCombinedFragmentUniformComponents;

            /// <summary>
            ///     <para>
            ///     [GL4] Gl.Get: data returns one value, the maximum number of individual floating-point, integer, or boolean values
            ///     that
            ///     can be held in uniform variable storage for a fragment shader. The value must be at least 1024. See Gl.Uniform.
            ///     </para>
            ///     <para>
            ///     [GLES3.2] Gl.Get: data returns one value, the maximum number of individual floating-point, integer, or boolean
            ///     values
            ///     that can be held in uniform variable storage for a fragment shader. The value must be at least 896. See Gl.Uniform.
            ///     </para>
            /// </summary>
            [Limit(MAX_FRAGMENT_UNIFORM_COMPONENTS)] [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_fragment_shader")]
            public int MaxFragmentUniformComponents;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum number of individual floating-point, integer, or boolean
            /// values that can be held in uniform variable storage for a vertex shader. The value must be at least 1024. See
            /// Gl.Uniform.
            /// </summary>
            [Limit(MAX_VERTEX_UNIFORM_COMPONENTS)] [RequiredByFeature("GL_VERSION_2_0")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_vertex_shader")]
            public int MaxVertexUniformComponents;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum supported texture image units that can be used to access
            /// texture maps from the vertex shader. The value may be at least 16. See Gl.ActiveTexture.
            /// </summary>
            [Limit(MAX_VERTEX_TEXTURE_IMAGE_UNITS)]
            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            [RequiredByFeature("GL_NV_vertex_program3")]
            public int MaxVertexTextureImageUnits;

            /// <summary>
            ///     <para>
            ///     [GL4] Gl.Get: data returns one value, the maximum supported texture image units that can be used to access texture
            ///     maps
            ///     from the vertex shader and the fragment processor combined. If both the vertex shader and the fragment processing
            ///     stage
            ///     access the same texture image unit, then that counts as using two texture image units against this limit. The value
            ///     must
            ///     be at least 48. See Gl.ActiveTexture.
            ///     </para>
            ///     <para>
            ///     [GLES3.2] Gl.Get: data returns one value, the maximum supported texture image units that can be used to access
            ///     texture
            ///     maps from the all the shader stages combined. If both the vertex shader and the fragment processing stage access
            ///     the
            ///     same texture image unit, then that counts as using two texture image units against this limit. The value must be at
            ///     least 96. See Gl.ActiveTexture.
            ///     </para>
            /// </summary>
            [Limit(MAX_COMBINED_TEXTURE_IMAGE_UNITS)]
            [RequiredByFeature("GL_VERSION_2_0")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_vertex_shader")]
            public int MaxCombinedTextureImageUnits;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum supported texture image units that can be used to access
            /// texture maps from the geometry shader. The value must be at least 16. See Gl.ActiveTexture.
            /// </summary>
            [Limit(MAX_GEOMETRY_TEXTURE_IMAGE_UNITS)]
            [RequiredByFeature("GL_VERSION_3_2")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_geometry_shader4", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
            [RequiredByFeature("GL_EXT_geometry_shader4")]
            [RequiredByFeature("GL_NV_geometry_program4")]
            [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
            public int MaxGeometryTextureImageUnits;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value. The value gives the maximum number of texels allowed in the texel array
            /// of
            /// a texture buffer object. Value must be at least 65536.
            /// </summary>
            [Limit(MAX_TEXTURE_BUFFER_SIZE)]
            [RequiredByFeature("GL_VERSION_3_1")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_texture_buffer_object", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_texture_buffer", Api = "gles2")]
            [RequiredByFeature("GL_EXT_texture_buffer_object")]
            [RequiredByFeature("GL_OES_texture_buffer", Api = "gles2")]
            public int MaxTextureBufferSize;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns a single floating-point value indicating the minimum sample shading fraction. See
            /// Gl.MinSampleShading.
            /// </summary>
            [Limit(MIN_SAMPLE_SHADING_VALUE)]
            [RequiredByFeature("GL_VERSION_4_0")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_sample_shading", Api = "gl|glcore")]
            [RequiredByFeature("GL_OES_sample_shading", Api = "gles2")]
            public int MinSampleShadingValue;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value, the maximum number of components which can be written per attribute or output
            /// in separate transform feedback mode. The value must be at least 4. See Gl.TransformFeedbackVaryings.
            /// </summary>
            [Limit(MAX_TRANSFORM_FEEDBACK_SEPARATE_COMPONENTS)]
            [RequiredByFeature("GL_VERSION_3_0")]
            [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
            [RequiredByFeature("GL_EXT_transform_feedback")]
            [RequiredByFeature("GL_NV_transform_feedback")]
            public int MaxTransformFeedbackSeparateComponents;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value, the maximum number of components which can be written to a single transform
            /// feedback buffer in interleaved mode. The value must be at least 64. See Gl.TransformFeedbackVaryings.
            /// </summary>
            [Limit(MAX_TRANSFORM_FEEDBACK_INTERLEAVED_COMPONENTS)]
            [RequiredByFeature("GL_VERSION_3_0")]
            [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
            [RequiredByFeature("GL_EXT_transform_feedback")]
            [RequiredByFeature("GL_NV_transform_feedback")]
            public int MaxTransformFeedbackInterleavedComponents;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value, the maximum separate attributes or outputs which can be captured in separate
            /// transform feedback mode. The value must be at least 4. See Gl.TransformFeedbackVaryings.
            /// </summary>
            [Limit(MAX_TRANSFORM_FEEDBACK_SEPARATE_ATTRIBS)]
            [RequiredByFeature("GL_VERSION_3_0")]
            [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
            [RequiredByFeature("GL_EXT_transform_feedback")]
            [RequiredByFeature("GL_NV_transform_feedback")]
            public int MaxTransformFeedbackSeparateAttribs;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value, the maximum number of color attachment points in a framebuffer object. The
            /// value must be at least 4. See Gl.FramebufferRenderbuffer and Gl.FramebufferTexture2D.
            /// </summary>
            [Limit(MAX_COLOR_ATTACHMENTS)]
            [RequiredByFeature("GL_VERSION_3_0")]
            [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_draw_buffers", Api = "gles2")]
            [RequiredByFeature("GL_EXT_framebuffer_object")]
            [RequiredByFeature("GL_NV_fbo_color_attachments", Api = "gles2")]
            public int MaxColorAttachments;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value. The value indicates the maximum supported number of samples for
            /// multisampling.
            /// The value must be at least 4. See Gl.GetInternalformativ.
            /// </summary>
            [Limit(MAX_SAMPLES)]
            [RequiredByFeature("GL_VERSION_3_0")]
            [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_framebuffer_object", Api = "gl|glcore")]
            [RequiredByFeature("GL_ANGLE_framebuffer_multisample", Api = "gles2")]
            [RequiredByFeature("GL_APPLE_framebuffer_multisample", Api = "gles1|gles2")]
            [RequiredByFeature("GL_EXT_framebuffer_multisample")]
            [RequiredByFeature("GL_EXT_multisampled_render_to_texture", Api = "gles1|gles2")]
            [RequiredByFeature("GL_NV_framebuffer_multisample", Api = "gles2")]
            public int MaxSamples;

            /// <summary>
            ///     <para>
            ///     [GL4] Gl.Get: data returns a single value, the maximum index that may be specified during the transfer of generic
            ///     vertex
            ///     attributes to the GL.
            ///     </para>
            ///     <para>
            ///     [GLES3.2] Gl.Get: data returns one value, the maximum index supported by the implementation. The value must be at
            ///     least
            ///     224-1.
            ///     </para>
            /// </summary>
            [Limit(MAX_ELEMENT_INDEX)] [RequiredByFeature("GL_VERSION_4_3")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")] [RequiredByFeature("GL_ARB_ES3_compatibility", Api = "gl|glcore")]
            public int MaxElementIndex;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum number of individual floating-point, integer, or boolean
            /// values that can be held in uniform variable storage for a geometry shader. The value must be at least 1024. See
            /// Gl.Uniform.
            /// </summary>
            [Limit(MAX_GEOMETRY_UNIFORM_COMPONENTS)]
            [RequiredByFeature("GL_VERSION_3_2")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_geometry_shader4", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
            [RequiredByFeature("GL_EXT_geometry_shader4")]
            [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
            public int MaxGeometryUniformComponents;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value, the maximum number of vertices emit by a geometry shader, which must be at
            /// least 256.
            /// </summary>
            [Limit(MAX_GEOMETRY_OUTPUT_VERTICES)]
            [RequiredByFeature("GL_VERSION_3_2")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_geometry_shader4", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
            [RequiredByFeature("GL_EXT_geometry_shader4")]
            [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
            public int MaxGeometryOutputVertices;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value, the maximum total number of components of active outputs for all vertices
            /// written by a geometry shader, which must be at least 1024.
            /// </summary>
            [Limit(MAX_GEOMETRY_TOTAL_OUTPUT_COMPONENTS)]
            [RequiredByFeature("GL_VERSION_3_2")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_geometry_shader4", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
            [RequiredByFeature("GL_EXT_geometry_shader4")]
            [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
            public int MaxGeometryTotalOutputComponents;

            /// <summary>
            /// [GL] Value of GL_MAX_SUBROUTINES symbol.
            /// </summary>
            [Limit(MAX_SUBROUTINES)] [RequiredByFeature("GL_VERSION_4_0")] [RequiredByFeature("GL_ARB_shader_subroutine", Api = "gl|glcore")]
            public int MaxSubroutines;

            /// <summary>
            /// [GL] Value of GL_MAX_SUBROUTINE_UNIFORM_LOCATIONS symbol.
            /// </summary>
            [Limit(MAX_SUBROUTINE_UNIFORM_LOCATIONS)] [RequiredByFeature("GL_VERSION_4_0")] [RequiredByFeature("GL_ARB_shader_subroutine", Api = "gl|glcore")]
            public int MaxSubroutineUniformLocations;

            /// <summary>
            ///     <para>
            ///     [GL4] Gl.Get: data returns one value, the maximum number of 4-vectors that may be held in uniform variable storage
            ///     for
            ///     the vertex shader. The value of Gl.MAX_VERTEX_UNIFORM_VECTORS is equal to the value of
            ///     Gl.MAX_VERTEX_UNIFORM_COMPONENTS
            ///     and must be at least 256.
            ///     </para>
            ///     <para>
            ///     [GLES3.2] Gl.Get: data returns one value, the maximum number of vector floating-point, integer, or boolean values
            ///     that
            ///     can be held in uniform variable storage for a vertex shader. The value must be at least 256. See Gl.Uniform.
            ///     </para>
            /// </summary>
            [Limit(MAX_VERTEX_UNIFORM_VECTORS)]
            [RequiredByFeature("GL_VERSION_4_1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_ES2_compatibility", Api = "gl|glcore")]
            public int MaxVertexUniformVectors;

            /// <summary>
            ///     <para>
            ///     [GL4] Gl.Get: data returns one value, the number 4-vectors for varying variables, which is equal to the value of
            ///     Gl.MAX_VARYING_COMPONENTS and must be at least 15.
            ///     </para>
            ///     <para>
            ///     [GLES3.2] Gl.Get: data returns one value, the maximum number of interpolators available for processing varying
            ///     variables
            ///     used by vertex and fragment shaders. This value represents the number of vector values that can be interpolated;
            ///     varying
            ///     variables declared as matrices and arrays will consume multiple interpolators. The value must be at least 15.
            ///     </para>
            /// </summary>
            [Limit(MAX_VARYING_VECTORS)]
            [RequiredByFeature("GL_VERSION_4_1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_ES2_compatibility", Api = "gl|glcore")]
            public int MaxVaryingVectors;

            /// <summary>
            ///     <para>
            ///     [GL4] Gl.Get: data returns one value, the maximum number of individual 4-vectors of floating-point, integer, or
            ///     boolean
            ///     values that can be held in uniform variable storage for a fragment shader. The value is equal to the value of
            ///     Gl.MAX_FRAGMENT_UNIFORM_COMPONENTS divided by 4 and must be at least 256. See Gl.Uniform.
            ///     </para>
            ///     <para>
            ///     [GLES3.2] Gl.Get: data returns one value, the maximum number of vector floating-point, integer, or boolean values
            ///     that
            ///     can be held in uniform variable storage for a fragment shader. The value must be at least 224. See Gl.Uniform.
            ///     </para>
            /// </summary>
            [Limit(MAX_FRAGMENT_UNIFORM_VECTORS)]
            [RequiredByFeature("GL_VERSION_4_1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            [RequiredByFeature("GL_ARB_ES2_compatibility", Api = "gl|glcore")]
            public int MaxFragmentUniformVectors;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value, the number of words for tesselation control shader uniform variables in all
            /// uniform blocks (including default). The value must be at least Gl.MAX_TESS_CONTROL_UNIFORM_COMPONENTS +
            /// Gl.MAX_UNIFORM_BLOCK_SIZE * Gl.MAX_TESS_CONTROL_UNIFORM_BLOCKS / 4. See Gl.Uniform.
            /// </summary>
            [Limit(MAX_COMBINED_TESS_CONTROL_UNIFORM_COMPONENTS)]
            [RequiredByFeature("GL_VERSION_4_0")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
            public int MaxCombinedTessControlUniformComponents;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value, the number of words for tesselation evaluation shader uniform variables in
            /// all
            /// uniform blocks (including default). The value must be at least Gl.MAX_TESS_EVALUATION_UNIFORM_COMPONENTS +
            /// Gl.MAX_UNIFORM_BLOCK_SIZE * Gl.MAX_TESS_EVALUATION_UNIFORM_BLOCKS / 4. See Gl.Uniform.
            /// </summary>
            [Limit(MAX_COMBINED_TESS_EVALUATION_UNIFORM_COMPONENTS)]
            [RequiredByFeature("GL_VERSION_4_0")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
            public int MaxCombinedTessEvaluationUniformComponents;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum number of sample mask words.
            /// </summary>
            [Limit(MAX_SAMPLE_MASK_WORDS)]
            [RequiredByFeature("GL_VERSION_3_2")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_texture_multisample", Api = "gl|glcore")]
            [RequiredByFeature("GL_NV_explicit_multisample")]
            public int MaxSampleMaskWords;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value, the maximum supported number of invocations per primitive of a geometry
            /// shader.
            /// </summary>
            [Limit(MAX_GEOMETRY_SHADER_INVOCATIONS)]
            [RequiredByFeature("GL_VERSION_4_0")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_gpu_shader5", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
            public int MaxGeometryShaderInvocations;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns a single floating-point value indicating the minimum valid offset for interpolation. See
            /// Gl.terpolateAtOffset.
            /// </summary>
            [Limit(MIN_FRAGMENT_INTERPOLATION_OFFSET)]
            [RequiredByFeature("GL_VERSION_4_0")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_gpu_shader5", Api = "gl|glcore")]
            [RequiredByFeature("GL_OES_shader_multisample_interpolation", Api = "gles2")]
            [RequiredByFeature("GL_NV_gpu_program5")]
            public int MinFragmentInterpolationOffset;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns a single floating-point value indicating the maximum valid offset for interpolation. See
            /// Gl.terpolateAtOffset.
            /// </summary>
            [Limit(MAX_FRAGMENT_INTERPOLATION_OFFSET)]
            [RequiredByFeature("GL_VERSION_4_0")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_gpu_shader5", Api = "gl|glcore")]
            [RequiredByFeature("GL_OES_shader_multisample_interpolation", Api = "gles2")]
            [RequiredByFeature("GL_NV_gpu_program5")]
            public int MaxFragmentInterpolationOffset;

            /// <summary>
            /// [GL] Value of GL_MIN_PROGRAM_TEXTURE_GATHER_OFFSET symbol.
            /// </summary>
            [Limit(MIN_PROGRAM_TEXTURE_GATHER_OFFSET)]
            [RequiredByFeature("GL_VERSION_4_0")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_texture_gather", Api = "gl|glcore")]
            [RequiredByFeature("GL_NV_gpu_program5")]
            public int MinProgramTextureGatherOffset;

            /// <summary>
            /// [GL] Value of GL_MAX_PROGRAM_TEXTURE_GATHER_OFFSET symbol.
            /// </summary>
            [Limit(MAX_PROGRAM_TEXTURE_GATHER_OFFSET)]
            [RequiredByFeature("GL_VERSION_4_0")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_texture_gather", Api = "gl|glcore")]
            [RequiredByFeature("GL_NV_gpu_program5")]
            public int MaxProgramTextureGatherOffset;

            /// <summary>
            /// [GL] Value of GL_MAX_TRANSFORM_FEEDBACK_BUFFERS symbol.
            /// </summary>
            [Limit(MAX_TRANSFORM_FEEDBACK_BUFFERS)] [RequiredByFeature("GL_VERSION_4_0")] [RequiredByFeature("GL_ARB_transform_feedback3", Api = "gl|glcore")]
            public int MaxTransformFeedbackBuffers;

            /// <summary>
            /// [GL] Value of GL_MAX_VERTEX_STREAMS symbol.
            /// </summary>
            [Limit(MAX_VERTEX_STREAMS)]
            [RequiredByFeature("GL_VERSION_4_0")]
            [RequiredByFeature("GL_ARB_gpu_shader5", Api = "gl|glcore")]
            [RequiredByFeature("GL_ARB_transform_feedback3", Api = "gl|glcore")]
            public int MaxVertexStreams;

            /// <summary>
            /// [GL] Value of GL_MAX_PATCH_VERTICES symbol.
            /// </summary>
            [Limit(MAX_PATCH_VERTICES)]
            [RequiredByFeature("GL_VERSION_4_0")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
            public int MaxPatchVertices;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns a single value, the maximum tessellation level supported by the tesselation primitive
            /// generator.
            /// </summary>
            [Limit(MAX_TESS_GEN_LEVEL)]
            [RequiredByFeature("GL_VERSION_4_0")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
            public int MaxTessGenLevel;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value, the maximum number of individual floating-point, integer, or boolean values
            /// that can be held in uniform variable storage for a tesselation control shader. The value must be at least 1024. See
            /// Gl.Uniform.
            /// </summary>
            [Limit(MAX_TESS_CONTROL_UNIFORM_COMPONENTS)]
            [RequiredByFeature("GL_VERSION_4_0")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
            public int MaxTessControlUniformComponents;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value, the maximum number of individual floating-point, integer, or boolean values
            /// that can be held in uniform variable storage for a tesselation evaluation shader. The value must be at least 1024. See
            /// Gl.Uniform.
            /// </summary>
            [Limit(MAX_TESS_EVALUATION_UNIFORM_COMPONENTS)]
            [RequiredByFeature("GL_VERSION_4_0")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
            public int MaxTessEvaluationUniformComponents;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value, the maximum supported texture image units that can be used to access texture
            /// maps from the tesselation control shader. The value may be at least 16. See Gl.ActiveTexture.
            /// </summary>
            [Limit(MAX_TESS_CONTROL_TEXTURE_IMAGE_UNITS)]
            [RequiredByFeature("GL_VERSION_4_0")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
            public int MaxTessControlTextureImageUnits;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value, the maximum supported texture image units that can be used to access texture
            /// maps from the tesselation evaluation shader. The value may be at least 16. See Gl.ActiveTexture.
            /// </summary>
            [Limit(MAX_TESS_EVALUATION_TEXTURE_IMAGE_UNITS)]
            [RequiredByFeature("GL_VERSION_4_0")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
            public int MaxTessEvaluationTextureImageUnits;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value, the maximum number of components of outputs written by a tesselation control
            /// shader, which must be at least 128.
            /// </summary>
            [Limit(MAX_TESS_CONTROL_OUTPUT_COMPONENTS)]
            [RequiredByFeature("GL_VERSION_4_0")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
            public int MaxTessControlOutputComponents;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value, the maximum number of components of per-patch outputs written by a
            /// tesselation
            /// control shader, which must be at least 128.
            /// </summary>
            [Limit(MAX_TESS_PATCH_COMPONENTS)]
            [RequiredByFeature("GL_VERSION_4_0")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
            public int MaxTessPatchComponents;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value, the maximum total number of components of active outputs for all vertices
            /// written by a tesselation control shader, including per-vertex and per-patch outputs, which must be at least 2048.
            /// </summary>
            [Limit(MAX_TESS_CONTROL_TOTAL_OUTPUT_COMPONENTS)]
            [RequiredByFeature("GL_VERSION_4_0")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
            public int MaxTessControlTotalOutputComponents;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value, the maximum number of components of outputs written by a tesselation
            /// evaluation shader, which must be at least 128.
            /// </summary>
            [Limit(MAX_TESS_EVALUATION_OUTPUT_COMPONENTS)]
            [RequiredByFeature("GL_VERSION_4_0")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
            public int MaxTessEvaluationOutputComponents;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value, the maximum number of uniform blocks per tesselation control shader. The
            /// value
            /// must be at least 12. See Gl.UniformBlockBinding.
            /// </summary>
            [Limit(MAX_TESS_CONTROL_UNIFORM_BLOCKS)]
            [RequiredByFeature("GL_VERSION_4_0")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
            public int MaxTessControlUniformBlocks;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value, the maximum number of uniform blocks per tesselation evaluation shader. The
            /// value must be at least 12. See Gl.UniformBlockBinding.
            /// </summary>
            [Limit(MAX_TESS_EVALUATION_UNIFORM_BLOCKS)]
            [RequiredByFeature("GL_VERSION_4_0")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_tessellation_shader", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
            public int MaxTessEvaluationUniformBlocks;

            /// <summary>
            /// [GL] Value of GL_MAX_IMAGE_UNITS symbol.
            /// </summary>
            [Limit(MAX_IMAGE_UNITS)]
            [RequiredByFeature("GL_VERSION_4_2")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_image_load_store", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_shader_image_load_store")]
            public int MaxImageUnits;

            /// <summary>
            /// [GL] Value of GL_MAX_COMBINED_IMAGE_UNITS_AND_FRAGMENT_OUTPUTS symbol.
            /// </summary>
            [Limit(MAX_COMBINED_IMAGE_UNITS_AND_FRAGMENT_OUTPUTS)]
            [RequiredByFeature("GL_VERSION_4_2")]
            [RequiredByFeature("GL_ARB_shader_image_load_store", Api = "gl|glcore")]
            [RequiredByFeature("GL_ARB_shader_storage_buffer_object", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_shader_image_load_store")]
            public int MaxCombinedImageUnitsAndFragmentOutputs;

            /// <summary>
            /// [GL] Value of GL_MAX_IMAGE_SAMPLES symbol.
            /// </summary>
            [Limit(MAX_IMAGE_SAMPLES)]
            [RequiredByFeature("GL_VERSION_4_2")]
            [RequiredByFeature("GL_ARB_shader_image_load_store", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_shader_image_load_store")]
            public int MaxImageSamples;

            /// <summary>
            /// [GL4] Gl.Get: data returns one value, the minimum alignment in basic machine units of pointers returned
            /// fromGl.MapBuffer
            /// and Gl.MapBufferRange. This value must be a power of two and must be at least 64.
            /// </summary>
            [Limit(MIN_MAP_BUFFER_ALIGNMENT)] [RequiredByFeature("GL_VERSION_4_2")] [RequiredByFeature("GL_ARB_map_buffer_alignment", Api = "gl|glcore")]
            public int MinMapBufferAlignment;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value, the maximum supported number of image variables in vertex shaders.
            /// </summary>
            [Limit(MAX_VERTEX_IMAGE_UNIFORMS)]
            [RequiredByFeature("GL_VERSION_4_2")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_image_load_store", Api = "gl|glcore")]
            public int MaxVertexImageUniforms;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value, the maximum supported number of image variables in tesselation control
            /// shaders.
            /// </summary>
            [Limit(MAX_TESS_CONTROL_IMAGE_UNIFORMS)]
            [RequiredByFeature("GL_VERSION_4_2")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_image_load_store", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
            public int MaxTessControlImageUniforms;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value, the maximum supported number of image variables in tesselation evaluation
            /// shaders.
            /// </summary>
            [Limit(MAX_TESS_EVALUATION_IMAGE_UNIFORMS)]
            [RequiredByFeature("GL_VERSION_4_2")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_image_load_store", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
            public int MaxTessEvaluationImageUniforms;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value, the maximum supported number of image variables in geometry shaders.
            /// </summary>
            [Limit(MAX_GEOMETRY_IMAGE_UNIFORMS)]
            [RequiredByFeature("GL_VERSION_4_2")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_image_load_store", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
            public int MaxGeometryImageUniforms;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value, the maximum supported number of image variables in fragment shaders. In GL ES
            /// 3.1, the minimum value is 0. in GL ES 3.2, the minimum value is 4.
            /// </summary>
            [Limit(MAX_FRAGMENT_IMAGE_UNIFORMS)]
            [RequiredByFeature("GL_VERSION_4_2")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_image_load_store", Api = "gl|glcore")]
            public int MaxFragmentImageUniforms;

            /// <summary>
            /// [GL] Value of GL_MAX_COMBINED_IMAGE_UNIFORMS symbol.
            /// </summary>
            [Limit(MAX_COMBINED_IMAGE_UNIFORMS)]
            [RequiredByFeature("GL_VERSION_4_2")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_image_load_store", Api = "gl|glcore")]
            public int MaxCombinedImageUniforms;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum number of active shader storage blocks that may be accessed
            /// by
            /// a vertex shader.
            /// </summary>
            [Limit(MAX_VERTEX_SHADER_STORAGE_BLOCKS)]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_storage_buffer_object", Api = "gl|glcore")]
            public int MaxVertexShaderStorageBlocks;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum number of active shader storage blocks that may be accessed
            /// by
            /// a geometry shader.
            /// </summary>
            [Limit(MAX_GEOMETRY_SHADER_STORAGE_BLOCKS)]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_storage_buffer_object", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
            public int MaxGeometryShaderStorageBlocks;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum number of active shader storage blocks that may be accessed
            /// by
            /// a tessellation control shader.
            /// </summary>
            [Limit(MAX_TESS_CONTROL_SHADER_STORAGE_BLOCKS)]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_storage_buffer_object", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
            public int MaxTessControlShaderStorageBlocks;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum number of active shader storage blocks that may be accessed
            /// by
            /// a tessellation evaluation shader.
            /// </summary>
            [Limit(MAX_TESS_EVALUATION_SHADER_STORAGE_BLOCKS)]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_storage_buffer_object", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
            public int MaxTessEvaluationShaderStorageBlocks;

            /// <summary>
            ///     <para>
            ///     [GL4] Gl.Get: data returns one value, the maximum number of active shader storage blocks that may be accessed by a
            ///     fragment shader.
            ///     </para>
            ///     <para>
            ///     [GLES3.2] Gl.Get: data returns one value, the maximum number of active shader storage blocks that may be accessed
            ///     by a
            ///     fragment shader. In GL ES 3.1, the minimum value is 0. in GL ES 3.2, the minimum value is 4.
            ///     </para>
            /// </summary>
            [Limit(MAX_FRAGMENT_SHADER_STORAGE_BLOCKS)]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_storage_buffer_object", Api = "gl|glcore")]
            public int MaxFragmentShaderStorageBlocks;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum number of active shader storage blocks that may be accessed
            /// by
            /// a compute shader.
            /// </summary>
            [Limit(MAX_COMPUTE_SHADER_STORAGE_BLOCKS)]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_storage_buffer_object", Api = "gl|glcore")]
            public int MaxComputeShaderStorageBlocks;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum total number of active shader storage blocks that may be
            /// accessed by all active shaders.
            /// </summary>
            [Limit(MAX_COMBINED_SHADER_STORAGE_BLOCKS)]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_storage_buffer_object", Api = "gl|glcore")]
            public int MaxCombinedShaderStorageBlocks;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum number of shader storage buffer binding points on the
            /// context,
            /// which must be at least 8.
            /// </summary>
            [Limit(MAX_SHADER_STORAGE_BUFFER_BINDINGS)]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_storage_buffer_object", Api = "gl|glcore")]
            public int MaxShaderStorageBufferBindings;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value, the maximum size in basic machine units of a shader storage block. The value
            /// must be at least 227.
            /// </summary>
            [Limit(MAX_SHADER_STORAGE_BLOCK_SIZE)]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_storage_buffer_object", Api = "gl|glcore")]
            public int MaxShaderStorageBlockSize;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the number of invocations in a single local work group (i.e., the product
            /// of the three dimensions) that may be dispatched to a compute shader.
            /// </summary>
            [Limit(MAX_COMPUTE_WORK_GROUP_INVOCATIONS)]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_compute_shader", Api = "gl|glcore")]
            public int MaxComputeWorkGroupInvocations;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum number of samples in a color multisample texture.
            /// </summary>
            [Limit(MAX_COLOR_TEXTURE_SAMPLES)]
            [RequiredByFeature("GL_VERSION_3_2")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_texture_multisample", Api = "gl|glcore")]
            public int MaxColorTextureSamples;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum number of samples supported in integer format multisample
            /// buffers.
            /// </summary>
            [Limit(MAX_INTEGER_SAMPLES)]
            [RequiredByFeature("GL_VERSION_3_2")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_texture_multisample", Api = "gl|glcore")]
            public int MaxIntegerSamples;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum Gl.WaitSync timeout interval.
            /// </summary>
            [Limit(MAX_SERVER_WAIT_TIMEOUT)]
            [RequiredByFeature("GL_VERSION_3_2")]
            [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_sync", Api = "gl|glcore")]
            [RequiredByFeature("GL_APPLE_sync", Api = "gles1|gles2")]
            public int MaxServerWaitTimeout;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum number of components of output written by a vertex shader,
            /// which must be at least 64.
            /// </summary>
            [Limit(MAX_VERTEX_OUTPUT_COMPONENTS)] [RequiredByFeature("GL_VERSION_3_2")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
            public int MaxVertexOutputComponents;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum number of components of inputs read by a geometry shader,
            /// which must be at least 64.
            /// </summary>
            [Limit(MAX_GEOMETRY_INPUT_COMPONENTS)]
            [RequiredByFeature("GL_VERSION_3_2")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
            public int MaxGeometryInputComponents;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum number of components of outputs written by a geometry shader,
            /// which must be at least 128.
            /// </summary>
            [Limit(MAX_GEOMETRY_OUTPUT_COMPONENTS)]
            [RequiredByFeature("GL_VERSION_3_2")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
            public int MaxGeometryOutputComponents;

            /// <summary>
            ///     <para>
            ///     [GL4] Gl.Get: data returns one value, the maximum number of components of the inputs read by the fragment shader,
            ///     which
            ///     must be at least 128.
            ///     </para>
            ///     <para>
            ///     [GLES3.2] Gl.Get: data returns one value, the maximum number of components of the inputs read by the fragment
            ///     shader,
            ///     which must be at least 60.
            ///     </para>
            /// </summary>
            [Limit(MAX_FRAGMENT_INPUT_COMPONENTS)] [RequiredByFeature("GL_VERSION_3_2")] [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
            public int MaxFragmentInputComponents;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns a single value, the maximum length of a debug message string, including its null
            /// terminator.
            /// </summary>
            [Limit(MAX_DEBUG_MESSAGE_LENGTH)]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
            [RequiredByFeature("GL_AMD_debug_output")]
            [RequiredByFeature("GL_ARB_debug_output", Api = "gl|glcore")]
            public int MaxDebugMessageLength;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns a single value, the maximum number of messages stored in the debug message log.
            /// </summary>
            [Limit(MAX_DEBUG_LOGGED_MESSAGES)]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_KHR_debug", Api = "gl|glcore|gles1|gles2")]
            [RequiredByFeature("GL_AMD_debug_output")]
            [RequiredByFeature("GL_ARB_debug_output", Api = "gl|glcore")]
            public int MaxDebugLoggedMessages;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum number of uniform blocks per compute shader. The value must
            /// be
            /// at least 14. See Gl.UniformBlockBinding.
            /// </summary>
            [Limit(MAX_COMPUTE_UNIFORM_BLOCKS)]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_compute_shader", Api = "gl|glcore")]
            public int MaxComputeUniformBlocks;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns one value, the maximum supported texture image units that can be used to access
            /// texture maps from the compute shader. The value may be at least 16. See Gl.ActiveTexture.
            /// </summary>
            [Limit(MAX_COMPUTE_TEXTURE_IMAGE_UNITS)]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_compute_shader", Api = "gl|glcore")]
            public int MaxComputeTextureImageUnits;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value, the maximum supported number of image variables in compute shaders.
            /// </summary>
            [Limit(MAX_COMPUTE_IMAGE_UNIFORMS)]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_compute_shader", Api = "gl|glcore")]
            public int MaxComputeImageUniforms;

            /// <summary>
            /// [GL] Value of GL_MAX_VERTEX_ATOMIC_COUNTER_BUFFERS symbol.
            /// </summary>
            [Limit(MAX_VERTEX_ATOMIC_COUNTER_BUFFERS)]
            [RequiredByFeature("GL_VERSION_4_2")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
            public int MaxVertexAtomicCounterBuffers;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns a single value, the maximum number of atomic counter buffers that may be accessed by a
            /// tesselation control shader.
            /// </summary>
            [Limit(MAX_TESS_CONTROL_ATOMIC_COUNTER_BUFFERS)]
            [RequiredByFeature("GL_VERSION_4_2")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
            public int MaxTessControlAtomicCounterBuffers;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns a single value, the maximum number of atomic counter buffers that may be accessed by a
            /// tesselation evaluation shader.
            /// </summary>
            [Limit(MAX_TESS_EVALUATION_ATOMIC_COUNTER_BUFFERS)]
            [RequiredByFeature("GL_VERSION_4_2")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
            public int MaxTessEvaluationAtomicCounterBuffers;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns a single value, the maximum number of atomic counter buffers that may be accessed by a
            /// geometry shader.
            /// </summary>
            [Limit(MAX_GEOMETRY_ATOMIC_COUNTER_BUFFERS)]
            [RequiredByFeature("GL_VERSION_4_2")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
            public int MaxGeometryAtomicCounterBuffers;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns a single value, the maximum number of atomic counter buffers that may be accessed by a
            /// fragment shader. In GL ES 3.1, the minimum value is 0. in GL ES 3.2, the minimum value is 1.
            /// </summary>
            [Limit(MAX_FRAGMENT_ATOMIC_COUNTER_BUFFERS)]
            [RequiredByFeature("GL_VERSION_4_2")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
            public int MaxFragmentAtomicCounterBuffers;

            /// <summary>
            /// [GL] Value of GL_MAX_COMBINED_ATOMIC_COUNTER_BUFFERS symbol.
            /// </summary>
            [Limit(MAX_COMBINED_ATOMIC_COUNTER_BUFFERS)]
            [RequiredByFeature("GL_VERSION_4_2")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
            public int MaxCombinedAtomicCounterBuffers;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns a single value, the maximum number of atomic counters available to vertex shaders.
            /// </summary>
            [Limit(MAX_VERTEX_ATOMIC_COUNTERS)]
            [RequiredByFeature("GL_VERSION_4_2")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
            public int MaxVertexAtomicCounters;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns a single value, the maximum number of atomic counters available to tessellation
            /// control shaders.
            /// </summary>
            [Limit(MAX_TESS_CONTROL_ATOMIC_COUNTERS)]
            [RequiredByFeature("GL_VERSION_4_2")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
            public int MaxTessControlAtomicCounters;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns a single value, the maximum number of atomic counters available to tessellation
            /// evaluation shaders.
            /// </summary>
            [Limit(MAX_TESS_EVALUATION_ATOMIC_COUNTERS)]
            [RequiredByFeature("GL_VERSION_4_2")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_tessellation_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_tessellation_shader", Api = "gles2")]
            public int MaxTessEvaluationAtomicCounters;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns a single value, the maximum number of atomic counters available to geometry shaders.
            /// </summary>
            [Limit(MAX_GEOMETRY_ATOMIC_COUNTERS)]
            [RequiredByFeature("GL_VERSION_4_2")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
            public int MaxGeometryAtomicCounters;

            /// <summary>
            ///     <para>
            ///     [GL4] Gl.Get: data returns a single value, the maximum number of atomic counters available to fragment shaders.
            ///     </para>
            ///     <para>
            ///     [GLES3.2] Gl.Get: data returns a single value, the maximum number of atomic counters available to fragment shaders.
            ///     In
            ///     GL ES 3.1, the minimum value is 0. in GL ES 3.2, the minimum value is 8.
            ///     </para>
            /// </summary>
            [Limit(MAX_FRAGMENT_ATOMIC_COUNTERS)]
            [RequiredByFeature("GL_VERSION_4_2")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
            public int MaxFragmentAtomicCounters;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns a single value, the maximum number of atomic counters available to all active
            /// shaders.
            /// </summary>
            [Limit(MAX_COMBINED_ATOMIC_COUNTERS)]
            [RequiredByFeature("GL_VERSION_4_2")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
            public int MaxCombinedAtomicCounters;

            /// <summary>
            /// [GL] Value of GL_MAX_ATOMIC_COUNTER_BUFFER_SIZE symbol.
            /// </summary>
            [Limit(MAX_ATOMIC_COUNTER_BUFFER_SIZE)]
            [RequiredByFeature("GL_VERSION_4_2")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
            public int MaxAtomicCounterBufferSize;

            /// <summary>
            /// [GLES3.2] Gl.Get: data returns one value, the maximum number of atomic counter buffer binding points. The value must be
            /// at least 1. See Gl.BindBuffer, Gl.BindBufferBase, and Gl.BindBufferRange.
            /// </summary>
            [Limit(MAX_ATOMIC_COUNTER_BUFFER_BINDINGS)]
            [RequiredByFeature("GL_VERSION_4_2")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_shader_atomic_counters", Api = "gl|glcore")]
            public int MaxAtomicCounterBufferBindings;

            /// <summary>
            ///     <para>
            ///     [GL4] Gl.Get: data returns one value, the maximum width for a framebuffer that has no attachments, which must be at
            ///     least 16384. See glFramebufferParameter.
            ///     </para>
            ///     <para>
            ///     [GLES3.2] Gl.Get: data returns one value, the maximum width for a framebuffer that has no attachments, which must
            ///     be at
            ///     least 16384. See Gl.FramebufferParameteri.
            ///     </para>
            /// </summary>
            [Limit(MAX_FRAMEBUFFER_WIDTH)]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_framebuffer_no_attachments", Api = "gl|glcore")]
            public int MaxFramebufferWidth;

            /// <summary>
            ///     <para>
            ///     [GL4] Gl.Get: data returns one value, the maximum height for a framebuffer that has no attachments, which must be
            ///     at
            ///     least 16384. See glFramebufferParameter.
            ///     </para>
            ///     <para>
            ///     [GLES3.2] Gl.Get: data returns one value, the maximum height for a framebuffer that has no attachments, which must
            ///     be at
            ///     least 16384. See Gl.FramebufferParameteri.
            ///     </para>
            /// </summary>
            [Limit(MAX_FRAMEBUFFER_HEIGHT)]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_framebuffer_no_attachments", Api = "gl|glcore")]
            public int MaxFramebufferHeight;

            /// <summary>
            ///     <para>
            ///     [GL4] Gl.Get: data returns one value, the maximum number of layers for a framebuffer that has no attachments, which
            ///     must
            ///     be at least 2048. See glFramebufferParameter.
            ///     </para>
            ///     <para>
            ///     [GLES3.2] Gl.Get: data returns one value, the maximum number of layers for a framebuffer that has no attachments,
            ///     which
            ///     must be at least 256. See Gl.FramebufferParameteri.
            ///     </para>
            /// </summary>
            [Limit(MAX_FRAMEBUFFER_LAYERS)]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_ES_VERSION_3_2", Api = "gles2")]
            [RequiredByFeature("GL_ARB_framebuffer_no_attachments", Api = "gl|glcore")]
            [RequiredByFeature("GL_EXT_geometry_shader", Api = "gles2")]
            [RequiredByFeature("GL_OES_geometry_shader", Api = "gles2")]
            public int MaxFramebufferLayers;

            /// <summary>
            ///     <para>
            ///     [GL4] Gl.Get: data returns one value, the maximum samples in a framebuffer that has no attachments, which must be
            ///     at
            ///     least 4. See glFramebufferParameter.
            ///     </para>
            ///     <para>
            ///     [GLES3.2] Gl.Get: data returns one value, the maximum samples in a framebuffer that has no attachments, which must
            ///     be at
            ///     least 4. See Gl.FramebufferParameteri.
            ///     </para>
            /// </summary>
            [Limit(MAX_FRAMEBUFFER_SAMPLES)]
            [RequiredByFeature("GL_VERSION_4_3")]
            [RequiredByFeature("GL_ES_VERSION_3_1", Api = "gles2")]
            [RequiredByFeature("GL_ARB_framebuffer_no_attachments", Api = "gl|glcore")]
            public int MaxFramebufferSamples;

            /// <summary>
            /// [GL2.1] Gl.Get: params returns two values: the smallest and largest supported widths for antialiased lines. See
            /// Gl.LineWidth.
            /// </summary>
            [Limit(LINE_WIDTH_RANGE, ArrayLength = 2)] [RequiredByFeature("GL_VERSION_1_0")]
            public float[] LineWidthRange = {0.0f, 0.0f};

            /// <summary>
            /// [GL2.1] Gl.Get: params returns one value, the width difference between adjacent supported widths for antialiased lines.
            /// See Gl.LineWidth.
            /// </summary>
            [Limit(LINE_WIDTH_GRANULARITY)] [RequiredByFeature("GL_VERSION_1_0")]
            public float LineWidthGranularity;

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns a pair of values indicating the range of widths supported for aliased lines. See
            /// Gl.LineWidth.
            /// </summary>
            [Limit(ALIASED_LINE_WIDTH_RANGE, ArrayLength = 2)]
            [RequiredByFeature("GL_VERSION_1_2")]
            [RequiredByFeature("GL_VERSION_ES_CM_1_0", Api = "gles1")]
            [RequiredByFeature("GL_ES_VERSION_2_0", Api = "gles2")]
            [RequiredByFeature("GL_SC_VERSION_2_0", Api = "glsc2")]
            public float[] AliasedLineWidthRange = {0.0f, 0.0f};

            /// <summary>
            /// [GL4|GLES3.2] Gl.Get: data returns a single value, the minimum required alignment for uniform buffer sizes and offset.
            /// The initial value is 1. See Gl.UniformBlockBinding.
            /// </summary>
            [Limit(UNIFORM_BUFFER_OFFSET_ALIGNMENT)]
            [RequiredByFeature("GL_VERSION_3_1")]
            [RequiredByFeature("GL_ES_VERSION_3_0", Api = "gles2")]
            [RequiredByFeature("GL_ARB_uniform_buffer_object", Api = "gl|glcore")]
            public int UniformBufferOffsetAlignment;
        }
    }
}