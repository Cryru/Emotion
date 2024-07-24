using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Emotion.Graphics.Batches;

/// <summary>
/// This class pretends to be a texture for the purposes of TextureAtlas.
/// This allows for arbitrary draw calls to be batched in the atlas.
/// </summary>
public class VirtualTextureAtlasTexture : Texture
{
    private static FrameBuffer? _virtualTextureAlphaBlendBuffer;

    public VirtualTextureAtlasTexture() : base(EmptyWhiteTexture.Pointer)
    {
        PixelFormat = OpenGL.PixelFormat.Rgba;
        PixelType = OpenGL.PixelType.UnsignedByte;
        _smooth = true;
    }

    public void SetVirtualSize(Vector2 size)
    {
        Size = size.Ceiling();
    }

    public void UpVersion()
    {
        Version++;
    }

    public virtual void VirtualTextureRenderToBatch(RenderComposer c)
    {

    }

    public void StartVirtualTextureRender(RenderComposer c, Vector2 sizeRequired)
    {
        _virtualTextureAlphaBlendBuffer ??= new FrameBuffer(sizeRequired).WithColor();
        _virtualTextureAlphaBlendBuffer.Resize(sizeRequired, true);
        c.RenderToAndClear(_virtualTextureAlphaBlendBuffer);
    }

    public Texture EndVirtualTextureRender(RenderComposer c)
    {
        c.RenderTo(null);
        return _virtualTextureAlphaBlendBuffer!.ColorAttachment;
    }
}
