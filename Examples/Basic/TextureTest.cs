// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using Breath.Primitives;
using OpenTK;
using Soul.Engine;
using Soul.Engine.ECS;
using Soul.Engine.ECS.Components;
using Soul.Engine.Enums;
using Soul.Engine.Graphics.Components;
using Soul.Engine.Modules;
using Soul.Engine.Scenography;

#endregion

namespace Examples.Basic
{
    public class TextureTest : Scene
    {
        public static void Main()
        {
            Core.Setup(new TextureTest());
        }

        #region Declarations

        #endregion

        protected override void Setup()
        {
            // Load text texture.
            AssetLoader.LoadTexture("imageTest.png");

            Entity basicTexture = Entity.CreateBasicDrawable("basicTexture");
            basicTexture.GetComponent<Transform>().Position = new Vector2(50, 50);
            basicTexture.GetComponent<Transform>().Size = new Vector2(50, 50);
            basicTexture.GetComponent<RenderData>().ApplyTexture(AssetLoader.GetTexture("imageTest.png"));

            AddEntity(basicTexture);

            Entity movedTexture = Entity.CreateBasicDrawable("movedTexture");
            movedTexture.GetComponent<Transform>().Position = new Vector2(110, 50);
            movedTexture.GetComponent<Transform>().Size = new Vector2(50, 50);
            movedTexture.GetComponent<RenderData>().ApplyTexture(AssetLoader.GetTexture("imageTest.png"));

            AddEntity(movedTexture);

            Entity scaledTexture = Entity.CreateBasicDrawable("scaledTexture");
            scaledTexture.GetComponent<Transform>().Position = new Vector2(170, 50);
            scaledTexture.GetComponent<Transform>().Size = new Vector2(20, 20);
            scaledTexture.GetComponent<RenderData>().ApplyTexture(AssetLoader.GetTexture("imageTest.png"));

            AddEntity(scaledTexture);

            Entity rotatedTexture = Entity.CreateBasicDrawable("rotatedTexture");
            rotatedTexture.GetComponent<Transform>().Position = new Vector2(250, 50);
            rotatedTexture.GetComponent<Transform>().Size = new Vector2(50, 50);
            rotatedTexture.GetComponent<RenderData>().ApplyTexture(AssetLoader.GetTexture("imageTest.png"));

            AddEntity(rotatedTexture);

            Scripting.Expose("rotatedTexture", GetEntity("rotatedTexture").GetComponent<Transform>());
            Scripting.Register("rotatedTexture.Rotation += 0.1;");

            Entity invisibleTexture = Entity.CreateBasicDrawable("invisibleTexture");
            invisibleTexture.GetComponent<Transform>().Position = new Vector2(50, 110);
            invisibleTexture.GetComponent<Transform>().Size = new Vector2(50, 50);
            invisibleTexture.GetComponent<RenderData>().ApplyTexture(AssetLoader.GetTexture("imageTest.png"));
            invisibleTexture.GetComponent<RenderData>().Color = new Color(255, 255, 255, 100);

            AddEntity(invisibleTexture);

            Entity invisibleMovedTexture = Entity.CreateBasicDrawable("invisibleMovedTexture");
            invisibleMovedTexture.GetComponent<Transform>().Position = new Vector2(110, 110);
            invisibleMovedTexture.GetComponent<Transform>().Size = new Vector2(50, 50);
            invisibleMovedTexture.GetComponent<RenderData>().ApplyTexture(AssetLoader.GetTexture("imageTest.png"));
            invisibleMovedTexture.GetComponent<RenderData>().Color = new Color(0, 0, 255, 100);

            AddEntity(invisibleMovedTexture);

            Entity invisibleScaledTexture = Entity.CreateBasicDrawable("invisibleScaledTexture");
            invisibleScaledTexture.GetComponent<Transform>().Position = new Vector2(170, 110);
            invisibleScaledTexture.GetComponent<Transform>().Size = new Vector2(20, 20);
            invisibleScaledTexture.GetComponent<RenderData>().ApplyTexture(AssetLoader.GetTexture("imageTest.png"));
            invisibleScaledTexture.GetComponent<RenderData>().Color = new Color(0, 255, 0, 100);

            AddEntity(invisibleScaledTexture);

            Entity invisibleRotatedTexture = Entity.CreateBasicDrawable("invisibleRotatedTexture");
            invisibleRotatedTexture.GetComponent<Transform>().Position = new Vector2(250, 110);
            invisibleRotatedTexture.GetComponent<Transform>().Size = new Vector2(50, 50);
            invisibleRotatedTexture.GetComponent<RenderData>().ApplyTexture(AssetLoader.GetTexture("imageTest.png"));
            invisibleRotatedTexture.GetComponent<RenderData>().Color = new Color(255, 0, 0, 100);

            AddEntity(invisibleRotatedTexture);

            Scripting.Expose("invisibleRotatedTexture", GetEntity("invisibleRotatedTexture").GetComponent<Transform>());
            Scripting.Register("invisibleRotatedTexture.Rotation += 0.1;");

            Entity textureFrame = Entity.CreateBasicDrawable("textureFrame");
            textureFrame.GetComponent<Transform>().Position = new Vector2(50, 170);
            textureFrame.GetComponent<Transform>().Size = new Vector2(50, 50);
            textureFrame.GetComponent<RenderData>().ApplyTexture(AssetLoader.GetTexture("imageTest.png"));
            textureFrame.GetComponent<RenderData>().TextureArea = new Rectangle(55, 20, 50, 50);
            AddEntity(textureFrame);

            Entity animatedFrame = Entity.CreateBasicDrawable("animatedFrame");
            animatedFrame.GetComponent<Transform>().Position = new Vector2(50, 230);
            animatedFrame.GetComponent<Transform>().Size = new Vector2(50, 50);
            animatedFrame.GetComponent<RenderData>().ApplyTexture(AssetLoader.GetTexture("imageTest.png"));
            animatedFrame.AttachComponent<AnimationData>();
            animatedFrame.GetComponent<AnimationData>().FrameSize = new Vector2(50, 50);
            animatedFrame.GetComponent<AnimationData>().FrameTime = 300;

            AddEntity(animatedFrame);

            Entity animatedFrameReverse = Entity.CreateBasicDrawable("animatedFrameReverse");
            animatedFrameReverse.GetComponent<Transform>().Position = new Vector2(50, 290);
            animatedFrameReverse.GetComponent<Transform>().Size = new Vector2(50, 50);
            animatedFrameReverse.GetComponent<RenderData>().ApplyTexture(AssetLoader.GetTexture("imageTest.png"));
            animatedFrameReverse.AttachComponent<AnimationData>();
            animatedFrameReverse.GetComponent<AnimationData>().FrameSize = new Vector2(50, 50);
            animatedFrameReverse.GetComponent<AnimationData>().FrameTime = 300;
            animatedFrameReverse.GetComponent<AnimationData>().LoopType = AnimationLoopType.Reverse;

            AddEntity(animatedFrameReverse);

            Entity animatedFrameNormalThenReverse = Entity.CreateBasicDrawable("animatedFrameNormalThenReverse");
            animatedFrameNormalThenReverse.GetComponent<Transform>().Position = new Vector2(50, 350);
            animatedFrameNormalThenReverse.GetComponent<Transform>().Size = new Vector2(50, 50);
            animatedFrameNormalThenReverse.GetComponent<RenderData>()
                .ApplyTexture(AssetLoader.GetTexture("imageTest.png"));
            animatedFrameNormalThenReverse.AttachComponent<AnimationData>();
            animatedFrameNormalThenReverse.GetComponent<AnimationData>().FrameSize = new Vector2(50, 50);
            animatedFrameNormalThenReverse.GetComponent<AnimationData>().FrameTime = 300;
            animatedFrameNormalThenReverse.GetComponent<AnimationData>().LoopType = AnimationLoopType.NormalThenReverse;

            AddEntity(animatedFrameNormalThenReverse);

            Entity animatedFrameOnce = Entity.CreateBasicDrawable("animatedFrameOnce");
            animatedFrameOnce.GetComponent<Transform>().Position = new Vector2(50, 410);
            animatedFrameOnce.GetComponent<Transform>().Size = new Vector2(50, 50);
            animatedFrameOnce.GetComponent<RenderData>().ApplyTexture(AssetLoader.GetTexture("imageTest.png"));
            animatedFrameOnce.AttachComponent<AnimationData>();
            animatedFrameOnce.GetComponent<AnimationData>().FrameSize = new Vector2(50, 50);
            animatedFrameOnce.GetComponent<AnimationData>().FrameTime = 300;
            animatedFrameOnce.GetComponent<AnimationData>().LoopType = AnimationLoopType.None;

            AddEntity(animatedFrameOnce);

            Entity animatedFrameOnceReverse = Entity.CreateBasicDrawable("animatedFrameOnceReverse");
            animatedFrameOnceReverse.GetComponent<Transform>().Position = new Vector2(50, 470);
            animatedFrameOnceReverse.GetComponent<Transform>().Size = new Vector2(50, 50);
            animatedFrameOnceReverse.GetComponent<RenderData>().ApplyTexture(AssetLoader.GetTexture("imageTest.png"));
            animatedFrameOnceReverse.AttachComponent<AnimationData>();
            animatedFrameOnceReverse.GetComponent<AnimationData>().FrameSize = new Vector2(50, 50);
            animatedFrameOnceReverse.GetComponent<AnimationData>().FrameTime = 300;
            animatedFrameOnceReverse.GetComponent<AnimationData>().LoopType = AnimationLoopType.NoneReverse;

            AddEntity(animatedFrameOnceReverse);

            Entity animatedLimitedRange = Entity.CreateBasicDrawable("animatedLimitedRange");
            animatedLimitedRange.GetComponent<Transform>().Position = new Vector2(110, 230);
            animatedLimitedRange.GetComponent<Transform>().Size = new Vector2(50, 50);
            animatedLimitedRange.GetComponent<RenderData>().ApplyTexture(AssetLoader.GetTexture("imageTest.png"));
            animatedLimitedRange.AttachComponent<AnimationData>();
            animatedLimitedRange.GetComponent<AnimationData>().FrameSize = new Vector2(50, 50);
            animatedLimitedRange.GetComponent<AnimationData>().FrameTime = 300;
            animatedLimitedRange.GetComponent<AnimationData>().StartingFrame = 3;
            animatedLimitedRange.GetComponent<AnimationData>().EndingFrame = 5;
            animatedLimitedRange.GetComponent<AnimationData>().LoopType = AnimationLoopType.Normal;

            AddEntity(animatedLimitedRange);

            Entity stretchedTextureFrame = Entity.CreateBasicDrawable("stretchedTextureFrame");
            stretchedTextureFrame.GetComponent<Transform>().Position = new Vector2(250, 180);
            stretchedTextureFrame.GetComponent<Transform>().Size = new Vector2(200, 200);
            stretchedTextureFrame.GetComponent<RenderData>().ApplyTexture(AssetLoader.GetTexture("imageTest.png"));
            AddEntity(stretchedTextureFrame);
     
            Entity tiledTextureFrame = Entity.CreateBasicDrawable("tiledTextureFrame");
            tiledTextureFrame.GetComponent<Transform>().Position = new Vector2(470, 180);
            tiledTextureFrame.GetComponent<Transform>().Size = new Vector2(200, 200);
            tiledTextureFrame.GetComponent<RenderData>().ApplyTexture(AssetLoader.GetTexture("imageTest.png"));
            tiledTextureFrame.GetComponent<RenderData>().TextureArea = new Rectangle(0, 0, 300, 300);
            AddEntity(tiledTextureFrame);
        }

        protected override void Update()
        {
        }
    }
}
