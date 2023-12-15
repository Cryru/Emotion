#region Using

using System;

#endregion

namespace ImGuiNET
{
    [Flags]
    public enum ImGuiBackendFlags
    {
        None = 0,
        HasGamepad = 1,
        HasMouseCursors = 2,
        HasSetMousePos = 4,
        RendererHasVtxOffset = 8,
        PlatformHasViewports = 1024,
        HasMouseHoveredViewport = 2048,
        RendererHasViewports = 4096,
    }
}