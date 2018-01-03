//// SoulEngine - https://github.com/Cryru/SoulEngine

//#region Using

//using Raya.Input;
//using Raya.Primitives;
//using Soul.Engine;
//using Soul.Engine.Enums;
//using Soul.Engine.Modules;
//using Soul.Engine.Objects;

//#endregion

//namespace Examples.Basic
//{
//    public class TextureTest : Scene
//    {
//        public static void Main(string[] args)
//        {
//            // Start the engine with this scene.
//            Core.Start(
//                new TextureTest(),
//                "textureTest"
//            );
//        }

//        public override void Initialize()
//        {
//            AssetLoader.LoadTexture("imageTest.png");

//            GameObject texture = new GameObject
//            {
//                Position = new Vector2(50, 50),
//                Size = new Vector2(50, 50)
//            };
//            texture.AddChild("texture", new Texture("imageTest.png"));
//            texture.AddChild("movedTexture", new Texture("imageTest.png"));
//            texture.GetChild<Texture>("movedTexture").Position = new Vector2(60, 0);
//            texture.AddChild("scaledTexture", new Texture("imageTest.png"));
//            texture.GetChild<Texture>("scaledTexture").Position = new Vector2(120, 0);
//            texture.GetChild<Texture>("scaledTexture").Size = new Vector2(20, 20);
//            texture.AddChild("rotatedTexture", new Texture("imageTest.png"));
//            texture.GetChild<Texture>("rotatedTexture").Position = new Vector2(200, 0);

//            AddChild("textureMother", texture);

//            GameObject invisibleTexture = new GameObject
//            {
//                Position = new Vector2(50, 110),
//                Size = new Vector2(50, 50)
//            };
//            invisibleTexture.AddChild("invisibleTexture", new Texture("imageTest.png"));
//            invisibleTexture.GetChild<Texture>("invisibleTexture").Color = new Color(255, 255, 255, 100);
//            invisibleTexture.AddChild("movedTexture", new Texture("imageTest.png"));
//            invisibleTexture.GetChild<Texture>("movedTexture").Position = new Vector2(60, 0);
//            invisibleTexture.GetChild<Texture>("movedTexture").Color = new Color(0, 0, 255, 100);
//            invisibleTexture.AddChild("scaledTexture", new Texture("imageTest.png"));
//            invisibleTexture.GetChild<Texture>("scaledTexture").Position = new Vector2(120, 20);
//            invisibleTexture.GetChild<Texture>("scaledTexture").Size = new Vector2(20, 20);
//            invisibleTexture.GetChild<Texture>("scaledTexture").Color = new Color(0, 255, 0, 100);
//            invisibleTexture.AddChild("rotatedTexture", new Texture("imageTest.png"));
//            invisibleTexture.GetChild<Texture>("rotatedTexture").Position = new Vector2(200, 0);
//            invisibleTexture.GetChild<Texture>("rotatedTexture").Color = new Color(255, 0, 0, 100);

//            AddChild("invisibleTextureMother", invisibleTexture);

//            GameObject textureFrame = new GameObject
//            {
//                Position = new Vector2(50, 170),
//                Size = new Vector2(50, 50)
//            };
//            textureFrame.AddChild("frame", new Texture("imageTest.png"));
//            textureFrame.GetChild<Texture>("frame").DrawArea(new Rectangle(55, 20, 50, 50));
//            textureFrame.AddChild("animatedFrame", new Texture("imageTest.png"));
//            textureFrame.GetChild<Texture>("animatedFrame").Position = new Vector2(0, 60);
//            textureFrame.GetChild<Texture>("animatedFrame").Animate(new Vector2(50, 50), 300);
//            textureFrame.AddChild("animatedFrameReverse", new Texture("imageTest.png"));
//            textureFrame.GetChild<Texture>("animatedFrameReverse").Position = new Vector2(60, 60);
//            textureFrame.GetChild<Texture>("animatedFrameReverse").Animate(new Vector2(50, 50), 300, LoopType.Reverse);
//            textureFrame.AddChild("animatedFrameNormalThenReverse", new Texture("imageTest.png"));
//            textureFrame.GetChild<Texture>("animatedFrameNormalThenReverse").Position = new Vector2(120, 60);
//            textureFrame.GetChild<Texture>("animatedFrameNormalThenReverse")
//                .Animate(new Vector2(50, 50), 300, LoopType.NormalThenReverse);
//            textureFrame.AddChild("animatedFrameOnce", new Texture("imageTest.png"));
//            textureFrame.GetChild<Texture>("animatedFrameOnce").Position = new Vector2(0, 120);
//            textureFrame.GetChild<Texture>("animatedFrameOnce").Animate(new Vector2(50, 50), 300, LoopType.None);
//            textureFrame.AddChild("animatedFrameOnceReverse", new Texture("imageTest.png"));
//            textureFrame.GetChild<Texture>("animatedFrameOnceReverse").Position = new Vector2(60, 120);
//            textureFrame.GetChild<Texture>("animatedFrameOnceReverse")
//                .Animate(new Vector2(50, 50), 300, LoopType.NoneReverse);
//            textureFrame.AddChild("animatedLimitedRange", new Texture("imageTest.png"));
//            textureFrame.GetChild<Texture>("animatedLimitedRange").Position = new Vector2(120, 120);
//            textureFrame.GetChild<Texture>("animatedLimitedRange")
//                .Animate(new Vector2(50, 50), 300, LoopType.NormalThenReverse, 4, 6);

//            AddChild("textureFrame", textureFrame);

//            GameObject stretched = new GameObject
//            {
//                Position = new Vector2(320, 50),
//                Size = new Vector2(300, 300)
//            };
//            stretched.AddChild("texture", new Texture("imageTest.png"));

//            AddChild("stretched", stretched);

//            GameObject tiled = new GameObject
//            {
//                Position = new Vector2(630, 50),
//                Size = new Vector2(300, 300)
//            };
//            tiled.AddChild("texture", new Texture("imageTest.png"));
//            tiled.GetChild<Texture>("texture").DrawArea(new Rectangle(0, 0, 300, 300));

//            AddChild("tiled", tiled);

//            // Rotate the rotating textures.
//            ScriptEngine.Expose("rotatedTexture", texture.GetChild<Texture>("rotatedTexture"));
//            ScriptEngine.Expose("rotatedTextureInvisible", invisibleTexture.GetChild<Texture>("rotatedTexture"));
//            ScriptEngine.RunScript("register(function() {" +
//                                   " rotatedTexture.Rotation += 0.1; " +
//                                   " rotatedTextureInvisible.Rotation += 0.1; " +
//                                   "});");
//        }

//        public override void Update()
//        {
//            if (Input.KeyPressed(Keyboard.Key.Num1))
//                AssetLoader.GetTexture("imageTest.png").Smooth = !AssetLoader.GetTexture("imageTest.png").Smooth;
//        }
//    }
//}