#region Using

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

#endregion

namespace ImGuiNET
{
    public struct ImGuiStyle
    {
        public float Alpha;
        public float DisabledAlpha;
        public Vector2 WindowPadding;
        public float WindowRounding;
        public float WindowBorderSize;
        public Vector2 WindowMinSize;
        public Vector2 WindowTitleAlign;
        public ImGuiDir WindowMenuButtonPosition;
        public float ChildRounding;
        public float ChildBorderSize;
        public float PopupRounding;
        public float PopupBorderSize;
        public Vector2 FramePadding;
        public float FrameRounding;
        public float FrameBorderSize;
        public Vector2 ItemSpacing;
        public Vector2 ItemInnerSpacing;
        public Vector2 CellPadding;
        public Vector2 TouchExtraPadding;
        public float IndentSpacing;
        public float ColumnsMinSpacing;
        public float ScrollbarSize;
        public float ScrollbarRounding;
        public float GrabMinSize;
        public float GrabRounding;
        public float LogSliderDeadzone;
        public float TabRounding;
        public float TabBorderSize;
        public float TabMinWidthForCloseButton;
        public ImGuiDir ColorButtonPosition;
        public Vector2 ButtonTextAlign;
        public Vector2 SelectableTextAlign;
        public Vector2 DisplayWindowPadding;
        public Vector2 DisplaySafeAreaPadding;
        public float MouseCursorScale;
        public byte AntiAliasedLines;
        public byte AntiAliasedLinesUseTex;
        public byte AntiAliasedFill;
        public float CurveTessellationTol;
        public float CircleTessellationMaxError;
        public Vector4 Colors_0;
        public Vector4 Colors_1;
        public Vector4 Colors_2;
        public Vector4 Colors_3;
        public Vector4 Colors_4;
        public Vector4 Colors_5;
        public Vector4 Colors_6;
        public Vector4 Colors_7;
        public Vector4 Colors_8;
        public Vector4 Colors_9;
        public Vector4 Colors_10;
        public Vector4 Colors_11;
        public Vector4 Colors_12;
        public Vector4 Colors_13;
        public Vector4 Colors_14;
        public Vector4 Colors_15;
        public Vector4 Colors_16;
        public Vector4 Colors_17;
        public Vector4 Colors_18;
        public Vector4 Colors_19;
        public Vector4 Colors_20;
        public Vector4 Colors_21;
        public Vector4 Colors_22;
        public Vector4 Colors_23;
        public Vector4 Colors_24;
        public Vector4 Colors_25;
        public Vector4 Colors_26;
        public Vector4 Colors_27;
        public Vector4 Colors_28;
        public Vector4 Colors_29;
        public Vector4 Colors_30;
        public Vector4 Colors_31;
        public Vector4 Colors_32;
        public Vector4 Colors_33;
        public Vector4 Colors_34;
        public Vector4 Colors_35;
        public Vector4 Colors_36;
        public Vector4 Colors_37;
        public Vector4 Colors_38;
        public Vector4 Colors_39;
        public Vector4 Colors_40;
        public Vector4 Colors_41;
        public Vector4 Colors_42;
        public Vector4 Colors_43;
        public Vector4 Colors_44;
        public Vector4 Colors_45;
        public Vector4 Colors_46;
        public Vector4 Colors_47;
        public Vector4 Colors_48;
        public Vector4 Colors_49;
        public Vector4 Colors_50;
        public Vector4 Colors_51;
        public Vector4 Colors_52;
        public Vector4 Colors_53;
        public Vector4 Colors_54;
    }

    public unsafe struct ImGuiStylePtr
    {
        public ImGuiStyle* NativePtr { get; }

        public ImGuiStylePtr(ImGuiStyle* nativePtr)
        {
            NativePtr = nativePtr;
        }

        public ImGuiStylePtr(IntPtr nativePtr)
        {
            NativePtr = (ImGuiStyle*) nativePtr;
        }

        public static implicit operator ImGuiStylePtr(ImGuiStyle* nativePtr)
        {
            return new ImGuiStylePtr(nativePtr);
        }

        public static implicit operator ImGuiStyle*(ImGuiStylePtr wrappedPtr)
        {
            return wrappedPtr.NativePtr;
        }

        public static implicit operator ImGuiStylePtr(IntPtr nativePtr)
        {
            return new ImGuiStylePtr(nativePtr);
        }

        public ref float Alpha
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->Alpha);
        }

        public ref float DisabledAlpha
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->DisabledAlpha);
        }

        public ref Vector2 WindowPadding
        {
            get => ref Unsafe.AsRef<Vector2>(&NativePtr->WindowPadding);
        }

        public ref float WindowRounding
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->WindowRounding);
        }

        public ref float WindowBorderSize
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->WindowBorderSize);
        }

        public ref Vector2 WindowMinSize
        {
            get => ref Unsafe.AsRef<Vector2>(&NativePtr->WindowMinSize);
        }

        public ref Vector2 WindowTitleAlign
        {
            get => ref Unsafe.AsRef<Vector2>(&NativePtr->WindowTitleAlign);
        }

        public ref ImGuiDir WindowMenuButtonPosition
        {
            get => ref Unsafe.AsRef<ImGuiDir>(&NativePtr->WindowMenuButtonPosition);
        }

        public ref float ChildRounding
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->ChildRounding);
        }

        public ref float ChildBorderSize
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->ChildBorderSize);
        }

        public ref float PopupRounding
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->PopupRounding);
        }

        public ref float PopupBorderSize
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->PopupBorderSize);
        }

        public ref Vector2 FramePadding
        {
            get => ref Unsafe.AsRef<Vector2>(&NativePtr->FramePadding);
        }

        public ref float FrameRounding
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->FrameRounding);
        }

        public ref float FrameBorderSize
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->FrameBorderSize);
        }

        public ref Vector2 ItemSpacing
        {
            get => ref Unsafe.AsRef<Vector2>(&NativePtr->ItemSpacing);
        }

        public ref Vector2 ItemInnerSpacing
        {
            get => ref Unsafe.AsRef<Vector2>(&NativePtr->ItemInnerSpacing);
        }

        public ref Vector2 CellPadding
        {
            get => ref Unsafe.AsRef<Vector2>(&NativePtr->CellPadding);
        }

        public ref Vector2 TouchExtraPadding
        {
            get => ref Unsafe.AsRef<Vector2>(&NativePtr->TouchExtraPadding);
        }

        public ref float IndentSpacing
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->IndentSpacing);
        }

        public ref float ColumnsMinSpacing
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->ColumnsMinSpacing);
        }

        public ref float ScrollbarSize
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->ScrollbarSize);
        }

        public ref float ScrollbarRounding
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->ScrollbarRounding);
        }

        public ref float GrabMinSize
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->GrabMinSize);
        }

        public ref float GrabRounding
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->GrabRounding);
        }

        public ref float LogSliderDeadzone
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->LogSliderDeadzone);
        }

        public ref float TabRounding
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->TabRounding);
        }

        public ref float TabBorderSize
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->TabBorderSize);
        }

        public ref float TabMinWidthForCloseButton
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->TabMinWidthForCloseButton);
        }

        public ref ImGuiDir ColorButtonPosition
        {
            get => ref Unsafe.AsRef<ImGuiDir>(&NativePtr->ColorButtonPosition);
        }

        public ref Vector2 ButtonTextAlign
        {
            get => ref Unsafe.AsRef<Vector2>(&NativePtr->ButtonTextAlign);
        }

        public ref Vector2 SelectableTextAlign
        {
            get => ref Unsafe.AsRef<Vector2>(&NativePtr->SelectableTextAlign);
        }

        public ref Vector2 DisplayWindowPadding
        {
            get => ref Unsafe.AsRef<Vector2>(&NativePtr->DisplayWindowPadding);
        }

        public ref Vector2 DisplaySafeAreaPadding
        {
            get => ref Unsafe.AsRef<Vector2>(&NativePtr->DisplaySafeAreaPadding);
        }

        public ref float MouseCursorScale
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->MouseCursorScale);
        }

        public ref bool AntiAliasedLines
        {
            get => ref Unsafe.AsRef<bool>(&NativePtr->AntiAliasedLines);
        }

        public ref bool AntiAliasedLinesUseTex
        {
            get => ref Unsafe.AsRef<bool>(&NativePtr->AntiAliasedLinesUseTex);
        }

        public ref bool AntiAliasedFill
        {
            get => ref Unsafe.AsRef<bool>(&NativePtr->AntiAliasedFill);
        }

        public ref float CurveTessellationTol
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->CurveTessellationTol);
        }

        public ref float CircleTessellationMaxError
        {
            get => ref Unsafe.AsRef<float>(&NativePtr->CircleTessellationMaxError);
        }

        public RangeAccessor<Vector4> Colors
        {
            get => new RangeAccessor<Vector4>(&NativePtr->Colors_0, 55);
        }

        public void Destroy()
        {
            ImGuiNative.ImGuiStyle_destroy(NativePtr);
        }

        public void ScaleAllSizes(float scale_factor)
        {
            ImGuiNative.ImGuiStyle_ScaleAllSizes(NativePtr, scale_factor);
        }
    }
}