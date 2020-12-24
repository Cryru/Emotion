#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Emotion.Common;
using Emotion.Game;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.Primitives;
using OpenGL;

#endregion

namespace Emotion.Graphics.Batches
{
    public class TextureMapping
    {
        public int UpToStruct;
        public Texture Texture;
    }

    public class TextureAtlasBinningState : Binning.BinningResumableState
    {
        public Dictionary<Texture, Vector2> TextureToUV = new Dictionary<Texture, Vector2>();
        public Dictionary<Texture, bool> TextureNeedDraw = new Dictionary<Texture, bool>();

        public uint AtlasPointer
        {
            get => _fbo.ColorAttachment.Pointer;
        }

        private readonly FrameBuffer _fbo;
        private readonly VertexBuffer _vbo;
        private readonly VertexArrayObject<VertexData> _vao;
        private readonly VertexData[] _vboLocal;

        private readonly RenderState _atlasFillState;

        protected bool _haveDirtyTextures;
        protected bool _firstDraw;

        public TextureAtlasBinningState(Vector2 canvasDimensions) : base(canvasDimensions)
        {
            _vbo = new VertexBuffer((uint) (VertexData.SizeInBytes * 4), BufferUsage.StaticDraw);
            _vboLocal = new VertexData[4];
            var ibo = new IndexBuffer(6 * sizeof(ushort));
            var quadIndices = new ushort[6];
            IndexBuffer.FillQuadIndices(quadIndices, 0);
            ibo.Upload(quadIndices);
            _vao = new VertexArrayObject<VertexData>(_vbo, ibo);
            _fbo = new FrameBuffer(canvasDimensions).WithColor();
            _firstDraw = true;

            _atlasFillState = RenderState.Default.Clone();
            _atlasFillState.AlphaBlending = false;
            _atlasFillState.ViewMatrix = false;
            _atlasFillState.DepthTest = false;
        }

        public bool StoreTexture(Texture texture)
        {
            if (TextureToUV.ContainsKey(texture)) return !TextureNeedDraw[texture]; // Not drawn to the atlas yet.

            Vector2? offset = Binning.FitRectanglesResumable(texture.Size, this);
            if (offset == null) return false;
            TextureToUV.Add(texture, offset.Value);
            TextureNeedDraw.Add(texture, true);
            _haveDirtyTextures = true;

            return false;
        }

        public void UpdateTextureAtlas(RenderComposer c)
        {
            if (!_haveDirtyTextures) return;

            c.SetState(_atlasFillState);
            c.RenderTo(_fbo);
            if (_firstDraw) Gl.Clear(ClearBufferMask.ColorBufferBit);
            VertexArrayObject.EnsureBound(_vao);

            foreach ((Texture texture, bool needDraw) in TextureNeedDraw)
            {
                if (!needDraw) continue;

                Vector2 offset = TextureToUV[texture];
                VertexData.SpriteToVertexData(_vboLocal, new Vector3(offset, 0), texture.Size, Color.White, texture);
                _vbo.Upload(_vboLocal);

                Texture.EnsureBound(texture.Pointer);
                Gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedShort, IntPtr.Zero);

                TextureNeedDraw[texture] = false;
            }

            c.RenderTo(null);
            _haveDirtyTextures = false;
            _firstDraw = false;
        }

        public Rectangle GetTextureUVMinMax(Texture texture)
        {
            Debug.Assert(TextureToUV.ContainsKey(texture));
            Debug.Assert(!TextureNeedDraw[texture]);

            Vector2 atlasOffset = TextureToUV[texture];
            Rectangle minMaxUV = new Rectangle(atlasOffset, atlasOffset + texture.Size);

            return new Rectangle(minMaxUV.Position / Size, minMaxUV.Size / Size);
        }

        public Vector2 GetTextureOffset(Texture texture)
        {
            Debug.Assert(TextureToUV.ContainsKey(texture));
            Debug.Assert(!TextureNeedDraw[texture]);

            return TextureToUV[texture];
        }
    }
}