// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Soul.Engine;
using Soul.Engine.Components;
using Soul.Engine.ECS;
using Soul.Engine.Enums;
using Soul.Engine.Modules;
using Soul.Engine.Scenography;

#endregion

namespace Soul.Examples
{
    public class Drawing : Scene
    {
        public static void Main()
        {
            Core.Setup(new Drawing());
        }

        protected override void Setup()
        {
            Entity drawTest = new Entity("drawTest");
            drawTest.AttachComponent<RenderData>();
            drawTest.Position = new Vector2(50, 50);
            drawTest.Size = new Vector2(100, 100);
            AddEntity(drawTest);

            Entity rotatedTest = new Entity("rotatedTest");
            rotatedTest.AttachComponent<RenderData>();
            rotatedTest.Position = new Vector2(200, 50);
            rotatedTest.Size = new Vector2(100, 100);
            rotatedTest.Rotation = Convert.DegreesToRadians(45);
            AddEntity(rotatedTest);

            Entity textureTest = new Entity("textureTest");
            textureTest.AttachComponent<RenderData>();
            textureTest.GetComponent<RenderData>().Texture = AssetLoader.LoadAsset<Texture2D>("Defaults/missing");
            textureTest.Position = new Vector2(350, 50);
            textureTest.Size = new Vector2(100, 100);
            AddEntity(textureTest);

            Entity textureTinted = new Entity("textureTinted");
            textureTinted.AttachComponent<RenderData>();
            textureTinted.GetComponent<RenderData>().Texture = AssetLoader.LoadAsset<Texture2D>("Defaults/missing");
            textureTinted.GetComponent<RenderData>().Tint = new Color(50, 100, 100);
            textureTinted.Position = new Vector2(500, 50);
            textureTinted.Size = new Vector2(100, 100);
            AddEntity(textureTinted);

            Entity animatedTest = new Entity("animatedTest");
            animatedTest.AttachComponent<RenderData>();
            animatedTest.GetComponent<RenderData>().Texture = AssetLoader.LoadAsset<Texture2D>("Defaults/imageTest");
            animatedTest.AttachComponent<AnimationData>();
            animatedTest.GetComponent<AnimationData>().FrameSize = new Vector2(50, 50);
            animatedTest.GetComponent<AnimationData>().LoopType = AnimationLoopType.None;
            animatedTest.GetComponent<AnimationData>().FrameTime = 500;
            animatedTest.Position = new Vector2(50, 200);
            animatedTest.Size = new Vector2(100, 100);
            AddEntity(animatedTest);

            Entity animatedTestLoop = new Entity("animatedTestLoop");
            animatedTestLoop.AttachComponent<RenderData>();
            animatedTestLoop.GetComponent<RenderData>().Texture = AssetLoader.LoadAsset<Texture2D>("Defaults/imageTest");
            animatedTestLoop.AttachComponent<AnimationData>();
            animatedTestLoop.GetComponent<AnimationData>().FrameSize = new Vector2(50, 50);
            animatedTestLoop.GetComponent<AnimationData>().LoopType = AnimationLoopType.Normal;
            animatedTestLoop.GetComponent<AnimationData>().FrameTime = 500;
            animatedTestLoop.Position = new Vector2(200, 200);
            animatedTestLoop.Size = new Vector2(100, 100);
            AddEntity(animatedTestLoop);

            Entity animatedTestLoopReverse = new Entity("animatedTestLoopReverse");
            animatedTestLoopReverse.AttachComponent<RenderData>();
            animatedTestLoopReverse.GetComponent<RenderData>().Texture = AssetLoader.LoadAsset<Texture2D>("Defaults/imageTest");
            animatedTestLoopReverse.AttachComponent<AnimationData>();
            animatedTestLoopReverse.GetComponent<AnimationData>().FrameSize = new Vector2(50, 50);
            animatedTestLoopReverse.GetComponent<AnimationData>().LoopType = AnimationLoopType.Reverse;
            animatedTestLoopReverse.GetComponent<AnimationData>().FrameTime = 500;
            animatedTestLoopReverse.Position = new Vector2(350, 200);
            animatedTestLoopReverse.Size = new Vector2(100, 100);
            AddEntity(animatedTestLoopReverse);

            Entity animatedTestLoopAndBack = new Entity("animatedTestLoopAndBack");
            animatedTestLoopAndBack.AttachComponent<RenderData>();
            animatedTestLoopAndBack.GetComponent<RenderData>().Texture = AssetLoader.LoadAsset<Texture2D>("Defaults/imageTest");
            animatedTestLoopAndBack.AttachComponent<AnimationData>();
            animatedTestLoopAndBack.GetComponent<AnimationData>().FrameSize = new Vector2(50, 50);
            animatedTestLoopAndBack.GetComponent<AnimationData>().LoopType = AnimationLoopType.NormalThenReverse;
            animatedTestLoopAndBack.GetComponent<AnimationData>().FrameTime = 500;
            animatedTestLoopAndBack.Position = new Vector2(500, 200);
            animatedTestLoopAndBack.Size = new Vector2(100, 100);
            AddEntity(animatedTestLoopAndBack);

            Entity sortTestTop = new Entity("sortTestTop");
            sortTestTop.AttachComponent<RenderData>();
            sortTestTop.GetComponent<RenderData>().Tint = Color.Red;
            sortTestTop.Position = new Vector2(75, 375);
            sortTestTop.Size = new Vector2(100, 100);
            sortTestTop.Priority = 2;
            AddEntity(sortTestTop);

            Entity sortTestBottom = new Entity("sortTestBottom");
            sortTestBottom.AttachComponent<RenderData>();
            sortTestBottom.GetComponent<RenderData>().Tint = Color.Blue;
            sortTestBottom.Position = new Vector2(50, 350);
            sortTestBottom.Size = new Vector2(100, 100);
            AddEntity(sortTestBottom);
        }

        protected override void Update()
        {
        }
    }
}