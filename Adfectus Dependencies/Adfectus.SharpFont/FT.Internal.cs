#region Using

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using SharpFont.Cache;
using SharpFont.Internal;
using SharpFont.PostScript;
using SharpFont.PostScript.Internal;
using SharpFont.TrueType;
using SharpFont.TrueType.Internal;

#endregion

namespace SharpFont
{
    /// <content>
    /// This file contains all the raw FreeType2 function signatures.
    /// </content>
    public static partial class FT
    {
        internal static bool? isMacOS;

        /// <summary>
        /// Returns true if the current .net platform is macOS.
        /// </summary>
        internal static bool IsMacOS
        {
            get
            {
                if (isMacOS != null)
                    return isMacOS.Value;
                lock (typeof(FT))
                {
                    if (isMacOS == null) // repeat the test
                    {
                        isMacOS = false;

                        object os = typeof(Environment)
                            ?.GetRuntimeProperty("OSVersion")
                            ?.GetValue(null);

                        object platformObj = os
                            ?.GetType().GetRuntimeProperty("Platform")
                            ?.GetValue(os);

                        if (platformObj != null)
                        {
                            int platform = (int)platformObj;
                            if (platform == 6)
                                isMacOS = true;
                        }
                    }
                }

                return isMacOS.Value;
            }
        }

        #region Internal Delegates

        internal delegate void FT_Library_VersionInternal(IntPtr library, out int amajor, out int aminor, out int apatch);
        [return: MarshalAs(UnmanagedType.U1)]
        internal delegate bool FT_Face_CheckTrueTypePatentsInternal(IntPtr face);
        [return: MarshalAs(UnmanagedType.U1)]
        internal delegate bool FT_Face_SetUnpatentedHintingInternal(IntPtr face, [MarshalAs(UnmanagedType.U1)] bool value);
        internal delegate Error FT_Init_FreeTypeInternal(out IntPtr alibrary);
        internal delegate Error FT_Done_FreeTypeInternal(IntPtr library);
        internal delegate Error FT_New_FaceInternal(IntPtr library, string filepathname, int face_index, out IntPtr aface);
        internal delegate Error FT_New_Memory_FaceInternal(IntPtr library, IntPtr file_base, int file_size, int face_index, out IntPtr aface);
        internal delegate Error FT_Open_FaceInternal(IntPtr library, IntPtr args, int face_index, out IntPtr aface);
        internal delegate Error FT_Attach_FileInternal(IntPtr face, string filepathname);
        internal delegate Error FT_Attach_StreamInternal(IntPtr face, IntPtr parameters);
        internal delegate Error FT_Reference_FaceInternal(IntPtr face);
        internal delegate Error FT_Done_FaceInternal(IntPtr face);
        internal delegate Error FT_Select_SizeInternal(IntPtr face, int strike_index);
        internal delegate Error FT_Request_SizeInternal(IntPtr face, IntPtr req);
        internal delegate Error FT_Set_Char_SizeInternal(IntPtr face, IntPtr char_width, IntPtr char_height, uint horz_resolution, uint vert_resolution);
        internal delegate Error FT_Set_Pixel_SizesInternal(IntPtr face, uint pixel_width, uint pixel_height);
        internal delegate Error FT_Load_GlyphInternal(IntPtr face, uint glyph_index, int load_flags);
        internal delegate Error FT_Load_CharInternal(IntPtr face, uint char_code, int load_flags);
        internal delegate void FT_Set_TransformInternal(IntPtr face, IntPtr matrix, IntPtr delta);
        internal delegate Error FT_Render_GlyphInternal(IntPtr slot, RenderMode render_mode);
        internal delegate Error FT_Get_KerningInternal(IntPtr face, uint left_glyph, uint right_glyph, uint kern_mode, out FTVector26Dot6 akerning);
        internal delegate Error FT_Get_Track_KerningInternal(IntPtr face, IntPtr point_size, int degree, out IntPtr akerning);
        internal delegate Error FT_Get_Glyph_NameInternal(IntPtr face, uint glyph_index, IntPtr buffer, uint buffer_max);
        internal delegate IntPtr FT_Get_Postscript_NameInternal(IntPtr face);
        internal delegate Error FT_Select_CharmapInternal(IntPtr face, Encoding encoding);
        internal delegate Error FT_Set_CharmapInternal(IntPtr face, IntPtr charmap);
        internal delegate int FT_Get_Charmap_IndexInternal(IntPtr charmap);
        internal delegate uint FT_Get_Char_IndexInternal(IntPtr face, uint charcode);
        internal delegate uint FT_Get_First_CharInternal(IntPtr face, out uint agindex);
        internal delegate uint FT_Get_Next_CharInternal(IntPtr face, uint char_code, out uint agindex);
        internal delegate uint FT_Get_Name_IndexInternal(IntPtr face, IntPtr glyph_name);
        internal delegate Error FT_Get_SubGlyph_InfoInternal(IntPtr glyph, uint sub_index, out int p_index, out SubGlyphFlags p_flags, out int p_arg1, out int p_arg2, out FTMatrix p_transform);
        internal delegate EmbeddingTypes FT_Get_FSType_FlagsInternal(IntPtr face);
        internal delegate uint FT_Face_GetCharVariantIndexInternal(IntPtr face, uint charcode, uint variantSelector);
        internal delegate int FT_Face_GetCharVariantIsDefaultInternal(IntPtr face, uint charcode, uint variantSelector);
        internal delegate IntPtr FT_Face_GetVariantSelectorsInternal(IntPtr face);
        internal delegate IntPtr FT_Face_GetVariantsOfCharInternal(IntPtr face, uint charcode);
        internal delegate IntPtr FT_Face_GetCharsOfVariantInternal(IntPtr face, uint variantSelector);
        internal delegate Error FT_Get_GlyphInternal(IntPtr slot, out IntPtr aglyph);
        internal delegate Error FT_Glyph_CopyInternal(IntPtr source, out IntPtr target);
        internal delegate Error FT_Glyph_TransformInternal(IntPtr glyph, ref FTMatrix matrix, ref FTVector delta);
        internal delegate void FT_Glyph_Get_CBoxInternal(IntPtr glyph, GlyphBBoxMode bbox_mode, out BBox acbox);
        internal delegate Error FT_Glyph_To_BitmapInternal(ref IntPtr the_glyph, RenderMode render_mode, ref FTVector26Dot6 origin, [MarshalAs(UnmanagedType.U1)] bool destroy);
        internal delegate void FT_Done_GlyphInternal(IntPtr glyph);
        internal delegate Error FT_New_Face_From_FONDInternal(IntPtr library, IntPtr fond, int face_index, out IntPtr aface);
        internal delegate Error FT_GetFile_From_Mac_NameInternal(string fontName, out IntPtr pathSpec, out int face_index);
        internal delegate Error FT_GetFile_From_Mac_ATS_NameInternal(string fontName, out IntPtr pathSpec, out int face_index);
        internal delegate Error FT_GetFilePath_From_Mac_ATS_NameInternal(string fontName, IntPtr path, int maxPathSize, out int face_index);
        internal delegate Error FT_New_Face_From_FSSpecInternal(IntPtr library, IntPtr spec, int face_index, out IntPtr aface);
        internal delegate Error FT_New_Face_From_FSRefInternal(IntPtr library, IntPtr @ref, int face_index, out IntPtr aface);
        internal delegate Error FT_New_SizeInternal(IntPtr face, out IntPtr size);
        internal delegate Error FT_Done_SizeInternal(IntPtr size);
        internal delegate Error FT_Activate_SizeInternal(IntPtr size);
        internal delegate Error FT_Get_Multi_MasterInternal(IntPtr face, out IntPtr amaster);
        internal delegate Error FT_Get_MM_VarInternal(IntPtr face, out IntPtr amaster);
        internal delegate Error FT_Set_MM_Design_CoordinatesInternal(IntPtr face, uint num_coords, IntPtr coords);
        internal delegate Error FT_Set_Var_Design_CoordinatesInternal(IntPtr face, uint num_coords, IntPtr coords);
        internal delegate Error FT_Set_MM_Blend_CoordinatesInternal(IntPtr face, uint num_coords, IntPtr coords);
        internal delegate Error FT_Set_Var_Blend_CoordinatesInternal(IntPtr face, uint num_coords, IntPtr coords);
        internal delegate IntPtr FT_Get_Sfnt_TableInternal(IntPtr face, SfntTag tag);
        internal delegate Error FT_Load_Sfnt_TableInternal(IntPtr face, uint tag, int offset, IntPtr buffer, ref uint length);
        internal unsafe delegate Error FT_Sfnt_Table_InfoInternal(IntPtr face, uint table_index, SfntTag* tag, out uint length);
        internal delegate uint FT_Get_CMap_Language_IDInternal(IntPtr charmap);
        internal delegate int FT_Get_CMap_FormatInternal(IntPtr charmap);
        [return: MarshalAs(UnmanagedType.U1)]
        internal delegate bool FT_Has_PS_Glyph_NamesInternal(IntPtr face);
        internal delegate Error FT_Get_PS_Font_InfoInternal(IntPtr face, out FontInfoRec afont_info);
        internal delegate Error FT_Get_PS_Font_PrivateInternal(IntPtr face, out PrivateRec afont_private);
        internal delegate int FT_Get_PS_Font_ValueInternal(IntPtr face, DictionaryKeys key, uint idx, ref IntPtr value, int value_len);
        internal delegate uint FT_Get_Sfnt_Name_CountInternal(IntPtr face);
        internal delegate Error FT_Get_Sfnt_NameInternal(IntPtr face, uint idx, out SfntNameRec aname);
        internal delegate Error FT_Get_BDF_Charset_IDInternal(IntPtr face, out string acharset_encoding, out string acharset_registry);
        internal delegate Error FT_Get_BDF_PropertyInternal(IntPtr face, string prop_name, out IntPtr aproperty);
        internal delegate Error FT_Get_CID_Registry_Ordering_SupplementInternal(IntPtr face, out string registry, out string ordering, out int aproperty);
        internal delegate Error FT_Get_CID_Is_Internally_CID_KeyedInternal(IntPtr face, out byte is_cid);
        internal delegate Error FT_Get_CID_From_Glyph_IndexInternal(IntPtr face, uint glyph_index, out uint cid);
        internal delegate Error FT_Get_PFR_MetricsInternal(IntPtr face, out uint aoutline_resolution, out uint ametrics_resolution, out IntPtr ametrics_x_scale, out IntPtr ametrics_y_scale);
        internal delegate Error FT_Get_PFR_KerningInternal(IntPtr face, uint left, uint right, out FTVector avector);
        internal delegate Error FT_Get_PFR_AdvanceInternal(IntPtr face, uint gindex, out int aadvance);
        internal delegate Error FT_Get_WinFNT_HeaderInternal(IntPtr face, out IntPtr aheader);
        internal delegate IntPtr FT_Get_X11_Font_FormatInternal(IntPtr face);
        internal delegate Gasp FT_Get_GaspInternal(IntPtr face, uint ppem);
        internal delegate IntPtr FT_MulDivInternal(IntPtr a, IntPtr b, IntPtr c);
        internal delegate IntPtr FT_MulFixInternal(IntPtr a, IntPtr b);
        internal delegate IntPtr FT_DivFixInternal(IntPtr a, IntPtr b);
        internal delegate IntPtr FT_RoundFixInternal(IntPtr a);
        internal delegate IntPtr FT_CeilFixInternal(IntPtr a);
        internal delegate IntPtr FT_FloorFixInternal(IntPtr a);
        internal delegate void FT_Vector_TransformInternal(ref FTVector vec, ref FTMatrix matrix);
        internal delegate void FT_Matrix_MultiplyInternal(ref FTMatrix a, ref FTMatrix b);
        internal delegate Error FT_Matrix_InvertInternal(ref FTMatrix matrix);
        internal delegate IntPtr FT_SinInternal(IntPtr angle);
        internal delegate IntPtr FT_CosInternal(IntPtr angle);
        internal delegate IntPtr FT_TanInternal(IntPtr angle);
        internal delegate IntPtr FT_Atan2Internal(IntPtr x, IntPtr y);
        internal delegate IntPtr FT_Angle_DiffInternal(IntPtr angle1, IntPtr angle2);
        internal delegate void FT_Vector_UnitInternal(out FTVector vec, IntPtr angle);
        internal delegate void FT_Vector_RotateInternal(ref FTVector vec, IntPtr angle);
        internal delegate IntPtr FT_Vector_LengthInternal(ref FTVector vec);
        internal delegate void FT_Vector_PolarizeInternal(ref FTVector vec, out IntPtr length, out IntPtr angle);
        internal delegate void FT_Vector_From_PolarInternal(out FTVector vec, IntPtr length, IntPtr angle);
        internal delegate IntPtr FT_List_FindInternal(IntPtr list, IntPtr data);
        internal delegate void FT_List_AddInternal(IntPtr list, IntPtr node);
        internal delegate void FT_List_InsertInternal(IntPtr list, IntPtr node);
        internal delegate void FT_List_RemoveInternal(IntPtr list, IntPtr node);
        internal delegate void FT_List_UpInternal(IntPtr list, IntPtr node);
        internal delegate Error FT_List_IterateInternal(IntPtr list, ListIterator iterator, IntPtr user);
        internal delegate void FT_List_FinalizeInternal(IntPtr list, ListDestructor destroy, IntPtr memory, IntPtr user);
        internal delegate Error FT_Outline_NewInternal(IntPtr library, uint numPoints, int numContours, out IntPtr anoutline);
        internal delegate Error FT_Outline_New_InternalInternal(IntPtr memory, uint numPoints, int numContours, out IntPtr anoutline);
        internal delegate Error FT_Outline_DoneInternal(IntPtr library, IntPtr outline);
        internal delegate Error FT_Outline_Done_InternalInternal(IntPtr memory, IntPtr outline);
        internal delegate Error FT_Outline_CopyInternal(IntPtr source, ref IntPtr target);
        internal delegate void FT_Outline_TranslateInternal(IntPtr outline, int xOffset, int yOffset);
        internal delegate void FT_Outline_TransformInternal(IntPtr outline, ref FTMatrix matrix);
        internal delegate Error FT_Outline_EmboldenInternal(IntPtr outline, IntPtr strength);
        internal delegate Error FT_Outline_EmboldenXYInternal(IntPtr outline, int xstrength, int ystrength);
        internal delegate void FT_Outline_ReverseInternal(IntPtr outline);
        internal delegate Error FT_Outline_CheckInternal(IntPtr outline);
        internal delegate Error FT_Outline_Get_BBoxInternal(IntPtr outline, out BBox abbox);
        internal delegate Error FT_Outline_DecomposeInternal(IntPtr outline, ref OutlineFuncsRec func_interface, IntPtr user);
        internal delegate void FT_Outline_Get_CBoxInternal(IntPtr outline, out BBox acbox);
        internal delegate Error FT_Outline_Get_BitmapInternal(IntPtr library, IntPtr outline, IntPtr abitmap);
        internal delegate Error FT_Outline_RenderInternal(IntPtr library, IntPtr outline, IntPtr @params);
        internal delegate Orientation FT_Outline_Get_OrientationInternal(IntPtr outline);
        internal delegate Error FT_Get_AdvanceInternal(IntPtr face, uint gIndex, LoadFlags load_flags, out IntPtr padvance);
        internal delegate Error FT_Get_AdvancesInternal(IntPtr face, uint start, uint count, LoadFlags load_flags, out IntPtr padvance);
        internal delegate void FT_Bitmap_NewInternal(IntPtr abitmap);
        internal delegate Error FT_Bitmap_CopyInternal(IntPtr library, IntPtr source, IntPtr target);
        internal delegate Error FT_Bitmap_EmboldenInternal(IntPtr library, IntPtr bitmap, IntPtr xStrength, IntPtr yStrength);
        internal delegate Error FT_Bitmap_ConvertInternal(IntPtr library, IntPtr source, IntPtr target, int alignment);
        internal delegate Error FT_GlyphSlot_Own_BitmapInternal(IntPtr slot);
        internal delegate Error FT_Bitmap_DoneInternal(IntPtr library, IntPtr bitmap);
        internal delegate StrokerBorder FT_Outline_GetInsideBorderInternal(IntPtr outline);
        internal delegate StrokerBorder FT_Outline_GetOutsideBorderInternal(IntPtr outline);
        internal delegate Error FT_Stroker_NewInternal(IntPtr library, out IntPtr astroker);
        internal delegate void FT_Stroker_SetInternal(IntPtr stroker, int radius, StrokerLineCap line_cap, StrokerLineJoin line_join, IntPtr miter_limit);
        internal delegate void FT_Stroker_RewindInternal(IntPtr stroker);
        internal delegate Error FT_Stroker_ParseOutlineInternal(IntPtr stroker, IntPtr outline, [MarshalAs(UnmanagedType.U1)] bool opened);
        internal delegate Error FT_Stroker_BeginSubPathInternal(IntPtr stroker, ref FTVector to, [MarshalAs(UnmanagedType.U1)] bool open);
        internal delegate Error FT_Stroker_EndSubPathInternal(IntPtr stroker);
        internal delegate Error FT_Stroker_LineToInternal(IntPtr stroker, ref FTVector to);
        internal delegate Error FT_Stroker_ConicToInternal(IntPtr stroker, ref FTVector control, ref FTVector to);
        internal delegate Error FT_Stroker_CubicToInternal(IntPtr stroker, ref FTVector control1, ref FTVector control2, ref FTVector to);
        internal delegate Error FT_Stroker_GetBorderCountsInternal(IntPtr stroker, StrokerBorder border, out uint anum_points, out uint anum_contours);
        internal delegate void FT_Stroker_ExportBorderInternal(IntPtr stroker, StrokerBorder border, IntPtr outline);
        internal delegate Error FT_Stroker_GetCountsInternal(IntPtr stroker, out uint anum_points, out uint anum_contours);
        internal delegate void FT_Stroker_ExportInternal(IntPtr stroker, IntPtr outline);
        internal delegate void FT_Stroker_DoneInternal(IntPtr stroker);
        internal delegate Error FT_Glyph_StrokeInternal(ref IntPtr pglyph, IntPtr stoker, [MarshalAs(UnmanagedType.U1)] bool destroy);
        internal delegate Error FT_Glyph_StrokeBorderInternal(ref IntPtr pglyph, IntPtr stoker, [MarshalAs(UnmanagedType.U1)] bool inside, [MarshalAs(UnmanagedType.U1)] bool destroy);
        internal delegate Error FT_Add_ModuleInternal(IntPtr library, IntPtr clazz);
        internal delegate IntPtr FT_Get_ModuleInternal(IntPtr library, string module_name);
        internal delegate Error FT_Remove_ModuleInternal(IntPtr library, IntPtr module);
        internal delegate Error FT_Property_SetInternal(IntPtr library, string module_name, string property_name, IntPtr value);
        internal delegate Error FT_Property_GetInternal(IntPtr library, string module_name, string property_name, IntPtr value);
        internal delegate Error FT_Reference_LibraryInternal(IntPtr library);
        internal delegate Error FT_New_LibraryInternal(IntPtr memory, out IntPtr alibrary);
        internal delegate Error FT_Done_LibraryInternal(IntPtr library);
        internal delegate void FT_Set_Debug_HookInternal(IntPtr library, uint hook_index, IntPtr debug_hook);
        internal delegate void FT_Add_Default_ModulesInternal(IntPtr library);
        internal delegate IntPtr FT_Get_RendererInternal(IntPtr library, GlyphFormat format);
        internal delegate Error FT_Set_RendererInternal(IntPtr library, IntPtr renderer, uint num_params, IntPtr parameters);
        internal delegate Error FT_Stream_OpenGzipInternal(IntPtr stream, IntPtr source);
        internal delegate Error FT_Gzip_UncompressInternal(IntPtr memory, IntPtr output, ref IntPtr output_len, IntPtr input, IntPtr input_len);
        internal delegate Error FT_Stream_OpenLZWInternal(IntPtr stream, IntPtr source);
        internal delegate Error FT_Stream_OpenBzip2Internal(IntPtr stream, IntPtr source);
        internal delegate Error FT_Library_SetLcdFilterInternal(IntPtr library, LcdFilter filter);
        internal delegate Error FT_Library_SetLcdFilterWeightsInternal(IntPtr library, byte[] weights);
        internal delegate Error FTC_Manager_NewInternal(IntPtr library, uint max_faces, uint max_sizes, ulong maxBytes, FaceRequester requester, IntPtr req_data, out IntPtr amanager);
        internal delegate void FTC_Manager_ResetInternal(IntPtr manager);
        internal delegate void FTC_Manager_DoneInternal(IntPtr manager);
        internal delegate Error FTC_Manager_LookupFaceInternal(IntPtr manager, IntPtr face_id, out IntPtr aface);
        internal delegate Error FTC_Manager_LookupSizeInternal(IntPtr manager, IntPtr scaler, out IntPtr asize);
        internal delegate void FTC_Node_UnrefInternal(IntPtr node, IntPtr manager);
        internal delegate void FTC_Manager_RemoveFaceIDInternal(IntPtr manager, IntPtr face_id);
        internal delegate Error FTC_CMapCache_NewInternal(IntPtr manager, out IntPtr acache);
        internal delegate uint FTC_CMapCache_LookupInternal(IntPtr cache, IntPtr face_id, int cmap_index, uint char_code);
        internal delegate Error FTC_ImageCache_NewInternal(IntPtr manager, out IntPtr acache);
        internal delegate Error FTC_ImageCache_LookupInternal(IntPtr cache, IntPtr type, uint gindex, out IntPtr aglyph, out IntPtr anode);
        internal delegate Error FTC_ImageCache_LookupScalerInternal(IntPtr cache, IntPtr scaler, LoadFlags load_flags, uint gindex, out IntPtr aglyph, out IntPtr anode);
        internal delegate Error FTC_SBitCache_NewInternal(IntPtr manager, out IntPtr acache);
        internal delegate Error FTC_SBitCache_LookupInternal(IntPtr cache, IntPtr type, uint gindex, out IntPtr sbit, out IntPtr anode);
        internal delegate Error FTC_SBitCache_LookupScalerInternal(IntPtr cache, IntPtr scaler, LoadFlags load_flags, uint gindex, out IntPtr sbit, out IntPtr anode);
        internal delegate Error FT_OpenType_ValidateInternal(IntPtr face, OpenTypeValidationFlags validation_flags, out IntPtr base_table, out IntPtr gdef_table, out IntPtr gpos_table,
                    out IntPtr gsub_table, out IntPtr jsft_table);
        internal delegate void FT_OpenType_FreeInternal(IntPtr face, IntPtr table);
        internal delegate EngineType FT_Get_TrueType_Engine_TypeInternal(IntPtr library);
        internal delegate Error FT_TrueTypeGX_ValidateInternal(IntPtr face, TrueTypeValidationFlags validation_flags, IntPtr[] tables, uint tableLength);
        internal delegate Error FT_TrueTypeGX_FreeInternal(IntPtr face, IntPtr table);
        internal delegate Error FT_ClassicKern_ValidateInternal(IntPtr face, ClassicKernValidationFlags validation_flags, out IntPtr ckern_table);
        internal delegate Error FT_ClassicKern_FreeInternal(IntPtr face, IntPtr table);

        #endregion


        #region Function Handles

        internal static FT_Library_VersionInternal FT_Library_Version;
        internal static FT_Face_CheckTrueTypePatentsInternal FT_Face_CheckTrueTypePatents;
        internal static FT_Face_SetUnpatentedHintingInternal FT_Face_SetUnpatentedHinting;
        internal static FT_Init_FreeTypeInternal FT_Init_FreeType;
        internal static FT_Done_FreeTypeInternal FT_Done_FreeType;
        internal static FT_New_FaceInternal FT_New_Face;
        internal static FT_New_Memory_FaceInternal FT_New_Memory_Face;
        internal static FT_Open_FaceInternal FT_Open_Face;
        internal static FT_Attach_FileInternal FT_Attach_File;
        internal static FT_Attach_StreamInternal FT_Attach_Stream;
        internal static FT_Reference_FaceInternal FT_Reference_Face;
        internal static FT_Done_FaceInternal FT_Done_Face;
        internal static FT_Select_SizeInternal FT_Select_Size;
        internal static FT_Request_SizeInternal FT_Request_Size;
        internal static FT_Set_Char_SizeInternal FT_Set_Char_Size;
        internal static FT_Set_Pixel_SizesInternal FT_Set_Pixel_Sizes;
        internal static FT_Load_GlyphInternal FT_Load_Glyph;
        internal static FT_Load_CharInternal FT_Load_Char;
        internal static FT_Set_TransformInternal FT_Set_Transform;
        internal static FT_Render_GlyphInternal FT_Render_Glyph;
        internal static FT_Get_KerningInternal FT_Get_Kerning;
        internal static FT_Get_Track_KerningInternal FT_Get_Track_Kerning;
        internal static FT_Get_Glyph_NameInternal FT_Get_Glyph_Name;
        internal static FT_Get_Postscript_NameInternal FT_Get_Postscript_Name;
        internal static FT_Select_CharmapInternal FT_Select_Charmap;
        internal static FT_Set_CharmapInternal FT_Set_Charmap;
        internal static FT_Get_Charmap_IndexInternal FT_Get_Charmap_Index;
        internal static FT_Get_Char_IndexInternal FT_Get_Char_Index;
        internal static FT_Get_First_CharInternal FT_Get_First_Char;
        internal static FT_Get_Next_CharInternal FT_Get_Next_Char;
        internal static FT_Get_Name_IndexInternal FT_Get_Name_Index;
        internal static FT_Get_SubGlyph_InfoInternal FT_Get_SubGlyph_Info;
        internal static FT_Get_FSType_FlagsInternal FT_Get_FSType_Flags;
        internal static FT_Face_GetCharVariantIndexInternal FT_Face_GetCharVariantIndex;
        internal static FT_Face_GetCharVariantIsDefaultInternal FT_Face_GetCharVariantIsDefault;
        internal static FT_Face_GetVariantSelectorsInternal FT_Face_GetVariantSelectors;
        internal static FT_Face_GetVariantsOfCharInternal FT_Face_GetVariantsOfChar;
        internal static FT_Face_GetCharsOfVariantInternal FT_Face_GetCharsOfVariant;
        internal static FT_Get_GlyphInternal FT_Get_Glyph;
        internal static FT_Glyph_CopyInternal FT_Glyph_Copy;
        internal static FT_Glyph_TransformInternal FT_Glyph_Transform;
        internal static FT_Glyph_Get_CBoxInternal FT_Glyph_Get_CBox;
        internal static FT_Glyph_To_BitmapInternal FT_Glyph_To_Bitmap;
        internal static FT_Done_GlyphInternal FT_Done_Glyph;
        internal static FT_New_Face_From_FONDInternal FT_New_Face_From_FOND;
        internal static FT_GetFile_From_Mac_NameInternal FT_GetFile_From_Mac_Name;
        internal static FT_GetFile_From_Mac_ATS_NameInternal FT_GetFile_From_Mac_ATS_Name;
        internal static FT_GetFilePath_From_Mac_ATS_NameInternal FT_GetFilePath_From_Mac_ATS_Name;
        internal static FT_New_Face_From_FSSpecInternal FT_New_Face_From_FSSpec;
        internal static FT_New_Face_From_FSRefInternal FT_New_Face_From_FSRef;
        internal static FT_New_SizeInternal FT_New_Size;
        internal static FT_Done_SizeInternal FT_Done_Size;
        internal static FT_Activate_SizeInternal FT_Activate_Size;
        internal static FT_Get_Multi_MasterInternal FT_Get_Multi_Master;
        internal static FT_Get_MM_VarInternal FT_Get_MM_Var;
        internal static FT_Set_MM_Design_CoordinatesInternal FT_Set_MM_Design_Coordinates;
        internal static FT_Set_Var_Design_CoordinatesInternal FT_Set_Var_Design_Coordinates;
        internal static FT_Set_MM_Blend_CoordinatesInternal FT_Set_MM_Blend_Coordinates;
        internal static FT_Set_Var_Blend_CoordinatesInternal FT_Set_Var_Blend_Coordinates;
        internal static FT_Get_Sfnt_TableInternal FT_Get_Sfnt_Table;
        internal static FT_Load_Sfnt_TableInternal FT_Load_Sfnt_Table;
        internal static FT_Sfnt_Table_InfoInternal FT_Sfnt_Table_Info;
        internal static FT_Get_CMap_Language_IDInternal FT_Get_CMap_Language_ID;
        internal static FT_Get_CMap_FormatInternal FT_Get_CMap_Format;
        internal static FT_Has_PS_Glyph_NamesInternal FT_Has_PS_Glyph_Names;
        internal static FT_Get_PS_Font_InfoInternal FT_Get_PS_Font_Info;
        internal static FT_Get_PS_Font_PrivateInternal FT_Get_PS_Font_Private;
        internal static FT_Get_PS_Font_ValueInternal FT_Get_PS_Font_Value;
        internal static FT_Get_Sfnt_Name_CountInternal FT_Get_Sfnt_Name_Count;
        internal static FT_Get_Sfnt_NameInternal FT_Get_Sfnt_Name;
        internal static FT_Get_BDF_Charset_IDInternal FT_Get_BDF_Charset_ID;
        internal static FT_Get_BDF_PropertyInternal FT_Get_BDF_Property;
        internal static FT_Get_CID_Registry_Ordering_SupplementInternal FT_Get_CID_Registry_Ordering_Supplement;
        internal static FT_Get_CID_Is_Internally_CID_KeyedInternal FT_Get_CID_Is_Internally_CID_Keyed;
        internal static FT_Get_CID_From_Glyph_IndexInternal FT_Get_CID_From_Glyph_Index;
        internal static FT_Get_PFR_MetricsInternal FT_Get_PFR_Metrics;
        internal static FT_Get_PFR_KerningInternal FT_Get_PFR_Kerning;
        internal static FT_Get_PFR_AdvanceInternal FT_Get_PFR_Advance;
        internal static FT_Get_WinFNT_HeaderInternal FT_Get_WinFNT_Header;
        internal static FT_Get_X11_Font_FormatInternal FT_Get_X11_Font_Format;
        internal static FT_Get_GaspInternal FT_Get_Gasp;
        internal static FT_MulDivInternal FT_MulDiv;
        internal static FT_MulFixInternal FT_MulFix;
        internal static FT_DivFixInternal FT_DivFix;
        internal static FT_RoundFixInternal FT_RoundFix;
        internal static FT_CeilFixInternal FT_CeilFix;
        internal static FT_FloorFixInternal FT_FloorFix;
        internal static FT_Vector_TransformInternal FT_Vector_Transform;
        internal static FT_Matrix_MultiplyInternal FT_Matrix_Multiply;
        internal static FT_Matrix_InvertInternal FT_Matrix_Invert;
        internal static FT_SinInternal FT_Sin;
        internal static FT_CosInternal FT_Cos;
        internal static FT_TanInternal FT_Tan;
        internal static FT_Atan2Internal FT_Atan2;
        internal static FT_Angle_DiffInternal FT_Angle_Diff;
        internal static FT_Vector_UnitInternal FT_Vector_Unit;
        internal static FT_Vector_RotateInternal FT_Vector_Rotate;
        internal static FT_Vector_LengthInternal FT_Vector_Length;
        internal static FT_Vector_PolarizeInternal FT_Vector_Polarize;
        internal static FT_Vector_From_PolarInternal FT_Vector_From_Polar;
        internal static FT_List_FindInternal FT_List_Find;
        internal static FT_List_AddInternal FT_List_Add;
        internal static FT_List_InsertInternal FT_List_Insert;
        internal static FT_List_RemoveInternal FT_List_Remove;
        internal static FT_List_UpInternal FT_List_Up;
        internal static FT_List_IterateInternal FT_List_Iterate;
        internal static FT_List_FinalizeInternal FT_List_Finalize;
        internal static FT_Outline_NewInternal FT_Outline_New;
        internal static FT_Outline_New_InternalInternal FT_Outline_New_Internal;
        internal static FT_Outline_DoneInternal FT_Outline_Done;
        internal static FT_Outline_Done_InternalInternal FT_Outline_Done_Internal;
        internal static FT_Outline_CopyInternal FT_Outline_Copy;
        internal static FT_Outline_TranslateInternal FT_Outline_Translate;
        internal static FT_Outline_TransformInternal FT_Outline_Transform;
        internal static FT_Outline_EmboldenInternal FT_Outline_Embolden;
        internal static FT_Outline_EmboldenXYInternal FT_Outline_EmboldenXY;
        internal static FT_Outline_ReverseInternal FT_Outline_Reverse;
        internal static FT_Outline_CheckInternal FT_Outline_Check;
        internal static FT_Outline_Get_BBoxInternal FT_Outline_Get_BBox;
        internal static FT_Outline_DecomposeInternal FT_Outline_Decompose;
        internal static FT_Outline_Get_CBoxInternal FT_Outline_Get_CBox;
        internal static FT_Outline_Get_BitmapInternal FT_Outline_Get_Bitmap;
        internal static FT_Outline_RenderInternal FT_Outline_Render;
        internal static FT_Outline_Get_OrientationInternal FT_Outline_Get_Orientation;
        internal static FT_Get_AdvanceInternal FT_Get_Advance;
        internal static FT_Get_AdvancesInternal FT_Get_Advances;
        internal static FT_Bitmap_NewInternal FT_Bitmap_New;
        internal static FT_Bitmap_CopyInternal FT_Bitmap_Copy;
        internal static FT_Bitmap_EmboldenInternal FT_Bitmap_Embolden;
        internal static FT_Bitmap_ConvertInternal FT_Bitmap_Convert;
        internal static FT_GlyphSlot_Own_BitmapInternal FT_GlyphSlot_Own_Bitmap;
        internal static FT_Bitmap_DoneInternal FT_Bitmap_Done;
        internal static FT_Outline_GetInsideBorderInternal FT_Outline_GetInsideBorder;
        internal static FT_Outline_GetOutsideBorderInternal FT_Outline_GetOutsideBorder;
        internal static FT_Stroker_NewInternal FT_Stroker_New;
        internal static FT_Stroker_SetInternal FT_Stroker_Set;
        internal static FT_Stroker_RewindInternal FT_Stroker_Rewind;
        internal static FT_Stroker_ParseOutlineInternal FT_Stroker_ParseOutline;
        internal static FT_Stroker_BeginSubPathInternal FT_Stroker_BeginSubPath;
        internal static FT_Stroker_EndSubPathInternal FT_Stroker_EndSubPath;
        internal static FT_Stroker_LineToInternal FT_Stroker_LineTo;
        internal static FT_Stroker_ConicToInternal FT_Stroker_ConicTo;
        internal static FT_Stroker_CubicToInternal FT_Stroker_CubicTo;
        internal static FT_Stroker_GetBorderCountsInternal FT_Stroker_GetBorderCounts;
        internal static FT_Stroker_ExportBorderInternal FT_Stroker_ExportBorder;
        internal static FT_Stroker_GetCountsInternal FT_Stroker_GetCounts;
        internal static FT_Stroker_ExportInternal FT_Stroker_Export;
        internal static FT_Stroker_DoneInternal FT_Stroker_Done;
        internal static FT_Glyph_StrokeInternal FT_Glyph_Stroke;
        internal static FT_Glyph_StrokeBorderInternal FT_Glyph_StrokeBorder;
        internal static FT_Add_ModuleInternal FT_Add_Module;
        internal static FT_Get_ModuleInternal FT_Get_Module;
        internal static FT_Remove_ModuleInternal FT_Remove_Module;
        internal static FT_Property_SetInternal FT_Property_Set;
        internal static FT_Property_GetInternal FT_Property_Get;
        internal static FT_Reference_LibraryInternal FT_Reference_Library;
        internal static FT_New_LibraryInternal FT_New_Library;
        internal static FT_Done_LibraryInternal FT_Done_Library;
        internal static FT_Set_Debug_HookInternal FT_Set_Debug_Hook;
        internal static FT_Add_Default_ModulesInternal FT_Add_Default_Modules;
        internal static FT_Get_RendererInternal FT_Get_Renderer;
        internal static FT_Set_RendererInternal FT_Set_Renderer;
        internal static FT_Stream_OpenGzipInternal FT_Stream_OpenGzip;
        internal static FT_Gzip_UncompressInternal FT_Gzip_Uncompress;
        internal static FT_Stream_OpenLZWInternal FT_Stream_OpenLZW;
        internal static FT_Stream_OpenBzip2Internal FT_Stream_OpenBzip2;
        internal static FT_Library_SetLcdFilterInternal FT_Library_SetLcdFilter;
        internal static FT_Library_SetLcdFilterWeightsInternal FT_Library_SetLcdFilterWeights;
        internal static FTC_Manager_NewInternal FTC_Manager_New;
        internal static FTC_Manager_ResetInternal FTC_Manager_Reset;
        internal static FTC_Manager_DoneInternal FTC_Manager_Done;
        internal static FTC_Manager_LookupFaceInternal FTC_Manager_LookupFace;
        internal static FTC_Manager_LookupSizeInternal FTC_Manager_LookupSize;
        internal static FTC_Node_UnrefInternal FTC_Node_Unref;
        internal static FTC_Manager_RemoveFaceIDInternal FTC_Manager_RemoveFaceID;
        internal static FTC_CMapCache_NewInternal FTC_CMapCache_New;
        internal static FTC_CMapCache_LookupInternal FTC_CMapCache_Lookup;
        internal static FTC_ImageCache_NewInternal FTC_ImageCache_New;
        internal static FTC_ImageCache_LookupInternal FTC_ImageCache_Lookup;
        internal static FTC_ImageCache_LookupScalerInternal FTC_ImageCache_LookupScaler;
        internal static FTC_SBitCache_NewInternal FTC_SBitCache_New;
        internal static FTC_SBitCache_LookupInternal FTC_SBitCache_Lookup;
        internal static FTC_SBitCache_LookupScalerInternal FTC_SBitCache_LookupScaler;
        internal static FT_OpenType_ValidateInternal FT_OpenType_Validate;
        internal static FT_OpenType_FreeInternal FT_OpenType_Free;
        internal static FT_Get_TrueType_Engine_TypeInternal FT_Get_TrueType_Engine_Type;
        internal static FT_TrueTypeGX_ValidateInternal _FT_TrueTypeGX_Validate;
        internal static FT_TrueTypeGX_FreeInternal FT_TrueTypeGX_Free;
        internal static FT_ClassicKern_ValidateInternal FT_ClassicKern_Validate;
        internal static FT_ClassicKern_FreeInternal FT_ClassicKern_Free;

        #endregion

        /// <summary>
        /// Initialize the library.
        /// </summary>
        /// <param name="lib">A pointer to the loaded library using the NativeLibrary class.</param>
        /// <returns>Whether initialization was successful.</returns>
        public static int Init(IntPtr lib)
        {
            GetFuncPointer(lib, "FT_Library_Version", ref FT_Library_Version);
            GetFuncPointer(lib, "FT_Face_CheckTrueTypePatents", ref FT_Face_CheckTrueTypePatents);
            GetFuncPointer(lib, "FT_Face_SetUnpatentedHinting", ref FT_Face_SetUnpatentedHinting);
            GetFuncPointer(lib, "FT_Init_FreeType", ref FT_Init_FreeType);
            GetFuncPointer(lib, "FT_Done_FreeType", ref FT_Done_FreeType);
            GetFuncPointer(lib, "FT_New_Face", ref FT_New_Face);
            GetFuncPointer(lib, "FT_New_Memory_Face", ref FT_New_Memory_Face);
            GetFuncPointer(lib, "FT_Open_Face", ref FT_Open_Face);
            GetFuncPointer(lib, "FT_Attach_File", ref FT_Attach_File);
            GetFuncPointer(lib, "FT_Attach_Stream", ref FT_Attach_Stream);
            GetFuncPointer(lib, "FT_Reference_Face", ref FT_Reference_Face);
            GetFuncPointer(lib, "FT_Done_Face", ref FT_Done_Face);
            GetFuncPointer(lib, "FT_Select_Size", ref FT_Select_Size);
            GetFuncPointer(lib, "FT_Request_Size", ref FT_Request_Size);
            GetFuncPointer(lib, "FT_Set_Char_Size", ref FT_Set_Char_Size);
            GetFuncPointer(lib, "FT_Set_Pixel_Sizes", ref FT_Set_Pixel_Sizes);
            GetFuncPointer(lib, "FT_Load_Glyph", ref FT_Load_Glyph);
            GetFuncPointer(lib, "FT_Load_Char", ref FT_Load_Char);
            GetFuncPointer(lib, "FT_Set_Transform", ref FT_Set_Transform);
            GetFuncPointer(lib, "FT_Render_Glyph", ref FT_Render_Glyph);
            GetFuncPointer(lib, "FT_Get_Kerning", ref FT_Get_Kerning);
            GetFuncPointer(lib, "FT_Get_Track_Kerning", ref FT_Get_Track_Kerning);
            GetFuncPointer(lib, "FT_Get_Glyph_Name", ref FT_Get_Glyph_Name);
            GetFuncPointer(lib, "FT_Get_Postscript_Name", ref FT_Get_Postscript_Name);
            GetFuncPointer(lib, "FT_Select_Charmap", ref FT_Select_Charmap);
            GetFuncPointer(lib, "FT_Set_Charmap", ref FT_Set_Charmap);
            GetFuncPointer(lib, "FT_Get_Charmap_Index", ref FT_Get_Charmap_Index);
            GetFuncPointer(lib, "FT_Get_Char_Index", ref FT_Get_Char_Index);
            GetFuncPointer(lib, "FT_Get_First_Char", ref FT_Get_First_Char);
            GetFuncPointer(lib, "FT_Get_Next_Char", ref FT_Get_Next_Char);
            GetFuncPointer(lib, "FT_Get_Name_Index", ref FT_Get_Name_Index);
            GetFuncPointer(lib, "FT_Get_SubGlyph_Info", ref FT_Get_SubGlyph_Info);
            GetFuncPointer(lib, "FT_Get_FSType_Flags", ref FT_Get_FSType_Flags);
            GetFuncPointer(lib, "FT_Face_GetCharVariantIndex", ref FT_Face_GetCharVariantIndex);
            GetFuncPointer(lib, "FT_Face_GetCharVariantIsDefault", ref FT_Face_GetCharVariantIsDefault);
            GetFuncPointer(lib, "FT_Face_GetVariantSelectors", ref FT_Face_GetVariantSelectors);
            GetFuncPointer(lib, "FT_Face_GetVariantsOfChar", ref FT_Face_GetVariantsOfChar);
            GetFuncPointer(lib, "FT_Face_GetCharsOfVariant", ref FT_Face_GetCharsOfVariant);
            GetFuncPointer(lib, "FT_Get_Glyph", ref FT_Get_Glyph);
            GetFuncPointer(lib, "FT_Glyph_Copy", ref FT_Glyph_Copy);
            GetFuncPointer(lib, "FT_Glyph_Transform", ref FT_Glyph_Transform);
            GetFuncPointer(lib, "FT_Glyph_Get_CBox", ref FT_Glyph_Get_CBox);
            GetFuncPointer(lib, "FT_Glyph_To_Bitmap", ref FT_Glyph_To_Bitmap);
            GetFuncPointer(lib, "FT_Done_Glyph", ref FT_Done_Glyph);
            GetFuncPointer(lib, "FT_New_Face_From_FOND", ref FT_New_Face_From_FOND);
            GetFuncPointer(lib, "FT_GetFile_From_Mac_Name", ref FT_GetFile_From_Mac_Name);
            GetFuncPointer(lib, "FT_GetFile_From_Mac_ATS_Name", ref FT_GetFile_From_Mac_ATS_Name);
            GetFuncPointer(lib, "FT_GetFilePath_From_Mac_ATS_Name", ref FT_GetFilePath_From_Mac_ATS_Name);
            GetFuncPointer(lib, "FT_New_Face_From_FSSpec", ref FT_New_Face_From_FSSpec);
            GetFuncPointer(lib, "FT_New_Face_From_FSRef", ref FT_New_Face_From_FSRef);
            GetFuncPointer(lib, "FT_New_Size", ref FT_New_Size);
            GetFuncPointer(lib, "FT_Done_Size", ref FT_Done_Size);
            GetFuncPointer(lib, "FT_Activate_Size", ref FT_Activate_Size);
            GetFuncPointer(lib, "FT_Get_Multi_Master", ref FT_Get_Multi_Master);
            GetFuncPointer(lib, "FT_Get_MM_Var", ref FT_Get_MM_Var);
            GetFuncPointer(lib, "FT_Set_MM_Design_Coordinates", ref FT_Set_MM_Design_Coordinates);
            GetFuncPointer(lib, "FT_Set_Var_Design_Coordinates", ref FT_Set_Var_Design_Coordinates);
            GetFuncPointer(lib, "FT_Set_MM_Blend_Coordinates", ref FT_Set_MM_Blend_Coordinates);
            GetFuncPointer(lib, "FT_Set_Var_Blend_Coordinates", ref FT_Set_Var_Blend_Coordinates);
            GetFuncPointer(lib, "FT_Get_Sfnt_Table", ref FT_Get_Sfnt_Table);
            GetFuncPointer(lib, "FT_Load_Sfnt_Table", ref FT_Load_Sfnt_Table);
            GetFuncPointer(lib, "Error FT_Sfnt_Table_Info", ref FT_Sfnt_Table_Info);
            GetFuncPointer(lib, "FT_Get_CMap_Language_ID", ref FT_Get_CMap_Language_ID);
            GetFuncPointer(lib, "FT_Get_CMap_Format", ref FT_Get_CMap_Format);
            GetFuncPointer(lib, "FT_Has_PS_Glyph_Names", ref FT_Has_PS_Glyph_Names);
            GetFuncPointer(lib, "FT_Get_PS_Font_Info", ref FT_Get_PS_Font_Info);
            GetFuncPointer(lib, "FT_Get_PS_Font_Private", ref FT_Get_PS_Font_Private);
            GetFuncPointer(lib, "FT_Get_PS_Font_Value", ref FT_Get_PS_Font_Value);
            GetFuncPointer(lib, "FT_Get_Sfnt_Name_Count", ref FT_Get_Sfnt_Name_Count);
            GetFuncPointer(lib, "FT_Get_Sfnt_Name", ref FT_Get_Sfnt_Name);
            GetFuncPointer(lib, "FT_Get_BDF_Charset_ID", ref FT_Get_BDF_Charset_ID);
            GetFuncPointer(lib, "FT_Get_BDF_Property", ref FT_Get_BDF_Property);
            GetFuncPointer(lib, "FT_Get_CID_Registry_Ordering_Supplement", ref FT_Get_CID_Registry_Ordering_Supplement);
            GetFuncPointer(lib, "FT_Get_CID_Is_Internally_CID_Keyed", ref FT_Get_CID_Is_Internally_CID_Keyed);
            GetFuncPointer(lib, "FT_Get_CID_From_Glyph_Index", ref FT_Get_CID_From_Glyph_Index);
            GetFuncPointer(lib, "FT_Get_PFR_Metrics", ref FT_Get_PFR_Metrics);
            GetFuncPointer(lib, "FT_Get_PFR_Kerning", ref FT_Get_PFR_Kerning);
            GetFuncPointer(lib, "FT_Get_PFR_Advance", ref FT_Get_PFR_Advance);
            GetFuncPointer(lib, "FT_Get_WinFNT_Header", ref FT_Get_WinFNT_Header);
            GetFuncPointer(lib, "FT_Get_X11_Font_Format", ref FT_Get_X11_Font_Format);
            GetFuncPointer(lib, "FT_Get_Gasp", ref FT_Get_Gasp);
            GetFuncPointer(lib, "FT_MulDiv", ref FT_MulDiv);
            GetFuncPointer(lib, "FT_MulFix", ref FT_MulFix);
            GetFuncPointer(lib, "FT_DivFix", ref FT_DivFix);
            GetFuncPointer(lib, "FT_RoundFix", ref FT_RoundFix);
            GetFuncPointer(lib, "FT_CeilFix", ref FT_CeilFix);
            GetFuncPointer(lib, "FT_FloorFix", ref FT_FloorFix);
            GetFuncPointer(lib, "FT_Vector_Transform", ref FT_Vector_Transform);
            GetFuncPointer(lib, "FT_Matrix_Multiply", ref FT_Matrix_Multiply);
            GetFuncPointer(lib, "FT_Matrix_Invert", ref FT_Matrix_Invert);
            GetFuncPointer(lib, "FT_Sin", ref FT_Sin);
            GetFuncPointer(lib, "FT_Cos", ref FT_Cos);
            GetFuncPointer(lib, "FT_Tan", ref FT_Tan);
            GetFuncPointer(lib, "FT_Atan2", ref FT_Atan2);
            GetFuncPointer(lib, "FT_Angle_Diff", ref FT_Angle_Diff);
            GetFuncPointer(lib, "FT_Vector_Unit", ref FT_Vector_Unit);
            GetFuncPointer(lib, "FT_Vector_Rotate", ref FT_Vector_Rotate);
            GetFuncPointer(lib, "FT_Vector_Length", ref FT_Vector_Length);
            GetFuncPointer(lib, "FT_Vector_Polarize", ref FT_Vector_Polarize);
            GetFuncPointer(lib, "FT_Vector_From_Polar", ref FT_Vector_From_Polar);
            GetFuncPointer(lib, "FT_List_Find", ref FT_List_Find);
            GetFuncPointer(lib, "FT_List_Add", ref FT_List_Add);
            GetFuncPointer(lib, "FT_List_Insert", ref FT_List_Insert);
            GetFuncPointer(lib, "FT_List_Remove", ref FT_List_Remove);
            GetFuncPointer(lib, "FT_List_Up", ref FT_List_Up);
            GetFuncPointer(lib, "FT_List_Iterate", ref FT_List_Iterate);
            GetFuncPointer(lib, "FT_List_Finalize", ref FT_List_Finalize);
            GetFuncPointer(lib, "FT_Outline_New", ref FT_Outline_New);
            GetFuncPointer(lib, "FT_Outline_New_Internal", ref FT_Outline_New_Internal);
            GetFuncPointer(lib, "FT_Outline_Done", ref FT_Outline_Done);
            GetFuncPointer(lib, "FT_Outline_Done_Internal", ref FT_Outline_Done_Internal);
            GetFuncPointer(lib, "FT_Outline_Copy", ref FT_Outline_Copy);
            GetFuncPointer(lib, "FT_Outline_Translate", ref FT_Outline_Translate);
            GetFuncPointer(lib, "FT_Outline_Transform", ref FT_Outline_Transform);
            GetFuncPointer(lib, "FT_Outline_Embolden", ref FT_Outline_Embolden);
            GetFuncPointer(lib, "FT_Outline_EmboldenXY", ref FT_Outline_EmboldenXY);
            GetFuncPointer(lib, "FT_Outline_Reverse", ref FT_Outline_Reverse);
            GetFuncPointer(lib, "FT_Outline_Check", ref FT_Outline_Check);
            GetFuncPointer(lib, "FT_Outline_Get_BBox", ref FT_Outline_Get_BBox);
            GetFuncPointer(lib, "FT_Outline_Decompose", ref FT_Outline_Decompose);
            GetFuncPointer(lib, "FT_Outline_Get_CBox", ref FT_Outline_Get_CBox);
            GetFuncPointer(lib, "FT_Outline_Get_Bitmap", ref FT_Outline_Get_Bitmap);
            GetFuncPointer(lib, "FT_Outline_Render", ref FT_Outline_Render);
            GetFuncPointer(lib, "FT_Outline_Get_Orientation", ref FT_Outline_Get_Orientation);
            GetFuncPointer(lib, "FT_Get_Advance", ref FT_Get_Advance);
            GetFuncPointer(lib, "FT_Get_Advances", ref FT_Get_Advances);
            GetFuncPointer(lib, "FT_Bitmap_New", ref FT_Bitmap_New);
            GetFuncPointer(lib, "FT_Bitmap_Copy", ref FT_Bitmap_Copy);
            GetFuncPointer(lib, "FT_Bitmap_Embolden", ref FT_Bitmap_Embolden);
            GetFuncPointer(lib, "FT_Bitmap_Convert", ref FT_Bitmap_Convert);
            GetFuncPointer(lib, "FT_GlyphSlot_Own_Bitmap", ref FT_GlyphSlot_Own_Bitmap);
            GetFuncPointer(lib, "FT_Bitmap_Done", ref FT_Bitmap_Done);
            GetFuncPointer(lib, "FT_Outline_GetInsideBorder", ref FT_Outline_GetInsideBorder);
            GetFuncPointer(lib, "FT_Outline_GetOutsideBorder", ref FT_Outline_GetOutsideBorder);
            GetFuncPointer(lib, "FT_Stroker_New", ref FT_Stroker_New);
            GetFuncPointer(lib, "FT_Stroker_Set", ref FT_Stroker_Set);
            GetFuncPointer(lib, "FT_Stroker_Rewind", ref FT_Stroker_Rewind);
            GetFuncPointer(lib, "FT_Stroker_ParseOutline", ref FT_Stroker_ParseOutline);
            GetFuncPointer(lib, "FT_Stroker_BeginSubPath", ref FT_Stroker_BeginSubPath);
            GetFuncPointer(lib, "FT_Stroker_EndSubPath", ref FT_Stroker_EndSubPath);
            GetFuncPointer(lib, "FT_Stroker_LineTo", ref FT_Stroker_LineTo);
            GetFuncPointer(lib, "FT_Stroker_ConicTo", ref FT_Stroker_ConicTo);
            GetFuncPointer(lib, "FT_Stroker_CubicTo", ref FT_Stroker_CubicTo);
            GetFuncPointer(lib, "FT_Stroker_GetBorderCounts", ref FT_Stroker_GetBorderCounts);
            GetFuncPointer(lib, "FT_Stroker_ExportBorder", ref FT_Stroker_ExportBorder);
            GetFuncPointer(lib, "FT_Stroker_GetCounts", ref FT_Stroker_GetCounts);
            GetFuncPointer(lib, "FT_Stroker_Export", ref FT_Stroker_Export);
            GetFuncPointer(lib, "FT_Stroker_Done", ref FT_Stroker_Done);
            GetFuncPointer(lib, "FT_Glyph_Stroke", ref FT_Glyph_Stroke);
            GetFuncPointer(lib, "FT_Glyph_StrokeBorder", ref FT_Glyph_StrokeBorder);
            GetFuncPointer(lib, "FT_Add_Module", ref FT_Add_Module);
            GetFuncPointer(lib, "FT_Get_Module", ref FT_Get_Module);
            GetFuncPointer(lib, "FT_Remove_Module", ref FT_Remove_Module);
            GetFuncPointer(lib, "FT_Property_Set", ref FT_Property_Set);
            GetFuncPointer(lib, "FT_Property_Get", ref FT_Property_Get);
            GetFuncPointer(lib, "FT_Reference_Library", ref FT_Reference_Library);
            GetFuncPointer(lib, "FT_New_Library", ref FT_New_Library);
            GetFuncPointer(lib, "FT_Done_Library", ref FT_Done_Library);
            GetFuncPointer(lib, "FT_Set_Debug_Hook", ref FT_Set_Debug_Hook);
            GetFuncPointer(lib, "FT_Add_Default_Modules", ref FT_Add_Default_Modules);
            GetFuncPointer(lib, "FT_Get_Renderer", ref FT_Get_Renderer);
            GetFuncPointer(lib, "FT_Set_Renderer", ref FT_Set_Renderer);
            GetFuncPointer(lib, "FT_Stream_OpenGzip", ref FT_Stream_OpenGzip);
            GetFuncPointer(lib, "FT_Gzip_Uncompress", ref FT_Gzip_Uncompress);
            GetFuncPointer(lib, "FT_Stream_OpenLZW", ref FT_Stream_OpenLZW);
            GetFuncPointer(lib, "FT_Stream_OpenBzip2", ref FT_Stream_OpenBzip2);
            GetFuncPointer(lib, "FT_Library_SetLcdFilter", ref FT_Library_SetLcdFilter);
            GetFuncPointer(lib, "FT_Library_SetLcdFilterWeights", ref FT_Library_SetLcdFilterWeights);
            GetFuncPointer(lib, "FTC_Manager_New", ref FTC_Manager_New);
            GetFuncPointer(lib, "FTC_Manager_Reset", ref FTC_Manager_Reset);
            GetFuncPointer(lib, "FTC_Manager_Done", ref FTC_Manager_Done);
            GetFuncPointer(lib, "FTC_Manager_LookupFace", ref FTC_Manager_LookupFace);
            GetFuncPointer(lib, "FTC_Manager_LookupSize", ref FTC_Manager_LookupSize);
            GetFuncPointer(lib, "FTC_Node_Unref", ref FTC_Node_Unref);
            GetFuncPointer(lib, "FTC_Manager_RemoveFaceID", ref FTC_Manager_RemoveFaceID);
            GetFuncPointer(lib, "FTC_CMapCache_New", ref FTC_CMapCache_New);
            GetFuncPointer(lib, "FTC_CMapCache_Lookup", ref FTC_CMapCache_Lookup);
            GetFuncPointer(lib, "FTC_ImageCache_New", ref FTC_ImageCache_New);
            GetFuncPointer(lib, "FTC_ImageCache_Lookup", ref FTC_ImageCache_Lookup);
            GetFuncPointer(lib, "FTC_ImageCache_LookupScaler", ref FTC_ImageCache_LookupScaler);
            GetFuncPointer(lib, "FTC_SBitCache_New", ref FTC_SBitCache_New);
            GetFuncPointer(lib, "FTC_SBitCache_Lookup", ref FTC_SBitCache_Lookup);
            GetFuncPointer(lib, "FTC_SBitCache_LookupScaler", ref FTC_SBitCache_LookupScaler);
            GetFuncPointer(lib, "FT_OpenType_Validate", ref FT_OpenType_Validate);
            GetFuncPointer(lib, "FT_OpenType_Free", ref FT_OpenType_Free);
            GetFuncPointer(lib, "FT_Get_TrueType_Engine_Type", ref FT_Get_TrueType_Engine_Type);
            GetFuncPointer(lib, "FT_TrueTypeGX_Validate", ref _FT_TrueTypeGX_Validate);
            GetFuncPointer(lib, "FT_TrueTypeGX_Free", ref FT_TrueTypeGX_Free);
            GetFuncPointer(lib, "FT_ClassicKern_Validate", ref FT_ClassicKern_Validate);
            GetFuncPointer(lib, "FT_ClassicKern_Free", ref FT_ClassicKern_Free);

            return 1;
        }

        /// <summary>
        /// Load a function from the library.
        /// </summary>
        /// <typeparam name="TDelegate">The delegate type to load the function as.</typeparam>
        /// <param name="lib">Pointer to the library.</param>
        /// <param name="name">The name of the function.</param>
        /// <param name="funcDel">The delegate variable which to load the function into.</param>
        internal static void GetFuncPointer<TDelegate>(IntPtr lib, string name, ref TDelegate funcDel)
        {
            bool success = NativeLibrary.TryGetExport(lib, name, out IntPtr func);
            if (success) funcDel = Marshal.GetDelegateForFunctionPointer<TDelegate>(func);
        }

        internal static void FT_TrueTypeGX_Validate(IntPtr face, TrueTypeValidationFlags validation_flags, byte[][] tables, uint tableLength)
        {
            // Copy the nested array to GCHandles.
            GCHandle[] handles = new GCHandle[tables.Length];
            for (int i = 0; i < tables.Length; i++)
            {
                handles[i] = GCHandle.Alloc(tables[i], GCHandleType.Pinned);
            }

            // Convert to an array of pointers
            IntPtr[] pointers = new IntPtr[handles.Length];
            for (int i = 0; i < handles.Length; i++)
            {
                pointers[i] = handles[i].AddrOfPinnedObject();
            }
            _FT_TrueTypeGX_Validate(face, validation_flags, pointers, tableLength);

            // Free created data.
            for (int i = 0; i < handles.Length; i++)
            {
                handles[i].Free();
            }
        }
    }
}