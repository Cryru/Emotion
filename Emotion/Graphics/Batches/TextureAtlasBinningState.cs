#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Emotion.Common;
using Emotion.Game;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Standard.Logging;
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
        public Dictionary<Texture, Vector2> TextureToOffset = new Dictionary<Texture, Vector2>();
        public Dictionary<Texture, bool> TextureNeedDraw = new Dictionary<Texture, bool>();
        public Dictionary<Texture, int> TextureActivity = new Dictionary<Texture, int>();

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
        protected int _textureActivityFrames;

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
            _fbo.CheckErrors();
            _firstDraw = true;

            _atlasFillState = RenderState.Default.Clone();
            _atlasFillState.AlphaBlending = false;
            _atlasFillState.ViewMatrix = false;
            _atlasFillState.DepthTest = false;

            var blitShader = Engine.AssetLoader.Get<ShaderAsset>("Shaders/AtlasBlit.xml");
            _atlasFillState.Shader = blitShader.Shader;
        }

        public bool StoreTexture(Texture texture)
        {
            if (TextureToOffset.ContainsKey(texture)) return !TextureNeedDraw[texture]; // Not drawn to the atlas yet.

            Vector2? offset = Binning.FitRectanglesResumable(texture.Size, this);
            if (offset == null) return false;
            TextureToOffset.Add(texture, offset.Value);
            TextureNeedDraw.Add(texture, true);
            _haveDirtyTextures = true;

            // Welcome, or welcome back.
            if (TextureActivity.ContainsKey(texture))
                TextureActivity[texture] = 0;
            else
                TextureActivity.Add(texture, 0);

            return false;
        }

        public void UpdateTextureUsage()
        {
            _textureActivityFrames++;
            if (_textureActivityFrames <= 100) return;

            var repack = false;
            foreach ((Texture texture, int timesUsed) in TextureActivity)
            {
                // Texture never entered cache, or was ejected already.
                if (timesUsed == -1 || TextureNeedDraw[texture]) continue;

                // If the texture wasn't used in the last 100 frames, or it was disposed,
                // eject it from the atlas.
                if (timesUsed == 0 || texture.Pointer == 0)
                {
                    repack = true;
                    TextureToOffset.Remove(texture);
                    TextureNeedDraw.Remove(texture);
                    TextureActivity[texture] = -1; // This texture will forever remain in this dictionary lol.
                    continue;
                }

                TextureActivity[texture] = 0;
            }

            _textureActivityFrames = 0;

            if (!repack) return;

            CanvasPos = Vector2.Zero;
            PackingSpaces.Clear();
            IOrderedEnumerable<Texture> allTextures = TextureToOffset.Keys.ToArray().OrderBy(x => x.Size.X + x.Size.Y);
            foreach (Texture texture in allTextures)
            {
                Vector2? offset = Binning.FitRectanglesResumable(texture.Size, this);
                if (offset.HasValue)
                {
                    TextureToOffset[texture] = offset.Value;
                    TextureNeedDraw[texture] = true;
                    _haveDirtyTextures = true;
                }
                else
                {
                    Engine.Log.Trace($"Texture {texture.Pointer} previously fit in the atlas, but no longer does.", MessageSource.Renderer);
                    TextureToOffset.Remove(texture);
                    TextureNeedDraw.Remove(texture);
                    TextureActivity[texture] = -1;
                }
            }
        }

        public void UpdateTextureAtlas(RenderComposer c)
        {
            if (!_haveDirtyTextures) return;

            // Draw all textures that need to be drawn to the atlas.
            c.SetState(_atlasFillState);
            c.RenderTo(_fbo);
            if (_firstDraw) Gl.Clear(ClearBufferMask.ColorBufferBit);
            VertexArrayObject.EnsureBound(_vao);

            foreach ((Texture texture, bool needDraw) in TextureNeedDraw)
            {
                if (!needDraw) continue;

                Vector2 offset = TextureToOffset[texture];
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
            Debug.Assert(TextureToOffset.ContainsKey(texture));
            Debug.Assert(!TextureNeedDraw[texture]);

            TextureActivity[texture]++;
            Vector2 atlasOffset = TextureToOffset[texture];
            var minMaxUV = new Rectangle(atlasOffset, atlasOffset + texture.Size);
            return new Rectangle(minMaxUV.Position / Size, minMaxUV.Size / Size);
        }
    }
}