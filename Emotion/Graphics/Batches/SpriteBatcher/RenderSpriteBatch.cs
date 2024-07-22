using Emotion.Graphics.Data;
using System.Buffers;

#nullable enable

namespace Emotion.Graphics.Batches.SpriteBatcher;

public class BatchBucketTextureMapping
{
    public int UpToStruct;

    public Texture Texture;

    public byte Atlas;
}

public class RenderSpriteBatchBucket
{
    public RenderState State;

    public uint TexturePtr;
    public VertexData[] QuadData;
    public int VerticesUsed;

    public List<BatchBucketTextureMapping> TextureMappings = new List<BatchBucketTextureMapping>(32);

    public RenderSpriteBatchBucket(RenderState state, uint texture)
    {
        State = state;
        TexturePtr = texture;
        QuadData = ArrayPool<VertexData>.Shared.Rent(1024);
    }

    public bool CanFitQuadIn()
    {
        return VerticesUsed + 4 < QuadData.Length;
    }

    public void Free()
    {
        ArrayPool<VertexData>.Shared.Return(QuadData);
        TextureMappings.Clear();
    }
}

public class RenderSpriteBatch
{
    private List<RenderSpriteBatchBucket> _buckets = new List<RenderSpriteBatchBucket>();

    public Span<VertexData> AddSprite(RenderState currentState, Texture? texture)
    {
        var renderStream = Engine.Renderer.RenderStream;
        uint texturePointer = Texture.EmptyWhiteTexture.Pointer;
        byte textureIsAtlas = 0;
        if (texture != null)
        {
            texturePointer = texture.Pointer;

            // Try to batch the texture.
            if (texture.Smooth && renderStream._smoothAtlas.TryBatchTexture(texture))
            {
                texturePointer = renderStream._smoothAtlas.AtlasPointer;
                textureIsAtlas = 1;
            }
            else if (!texture.Smooth && renderStream._atlas.TryBatchTexture(texture))
            {
                texturePointer = renderStream._atlas.AtlasPointer;
                textureIsAtlas = 2;
            }
        }

        RenderSpriteBatchBucket? myBucket = null;
        for (int i = 0; i < _buckets.Count; i++)
        {
            RenderSpriteBatchBucket bucket = _buckets[i];
            if (bucket.TexturePtr == texturePointer && bucket.State.Equals(currentState) && bucket.CanFitQuadIn())
            {
                myBucket = bucket;
                break;
            }
        }
        if (myBucket == null)
        {
            myBucket = new RenderSpriteBatchBucket(currentState, texturePointer);
            _buckets.Add(myBucket);
        }

        Span<VertexData> dataSpan = myBucket.QuadData.AsSpan().Slice(myBucket.VerticesUsed, 4);
        myBucket.VerticesUsed += 4;

        if (textureIsAtlas != 0)
        {
            myBucket.TextureMappings.Add(new BatchBucketTextureMapping()
            {
                Texture = texture,
                UpToStruct = myBucket.VerticesUsed,
                Atlas = textureIsAtlas
            });
        }

        return dataSpan;
    }

    public void Clear()
    {
        for (int i = 0; i < _buckets.Count; i++)
        {
            RenderSpriteBatchBucket bucket = _buckets[i];
            var state = bucket.State;

            Engine.Renderer.SetState(state);

            for (int ii = 0; ii < bucket.TextureMappings.Count; ii++)
            {
                var mapping = bucket.TextureMappings[ii];
                if (mapping.Atlas == 1)
                    Engine.Renderer.RenderStream._smoothAtlas.RecordTextureMapping(mapping.Texture, mapping.UpToStruct);
                else
                    Engine.Renderer.RenderStream._atlas.RecordTextureMapping(mapping.Texture, mapping.UpToStruct);
            }

            Span<VertexData> vertData = Engine.Renderer.RenderStream.GetStreamMemory((uint)bucket.VerticesUsed, BatchMode.Quad, bucket.TexturePtr);
            bucket.QuadData.AsSpan().Slice(0, bucket.VerticesUsed).CopyTo(vertData);

            Engine.Renderer.FlushRenderStream();

            bucket.Free();
        }

        _buckets.Clear();
    }
}
