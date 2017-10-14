using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Soul.Engine;
using Raya;
using Soul.IO;

namespace Examples.Basic
{
    public class TextureTest : Scene
    {
        private Raya.Graphics.Sprite sprite;

        public static void Main(string[] args)
        {
            // Lock assets.
            Soul.Engine.Modules.AssetLoader.Lock("7B76987910917447042EAFE5CB089799", " nhx:Pq^Mx6%\"BT Rjc. <QI5dk^zy~b2uUqo`~/&ek(T+^e.HfM~Koc<>bVe3dT");

            // Start the engine.
            Core.Start(new TextureTest(), "textureTest");
        }

        public override void Initialize()
        {
            Soul.Engine.Modules.AssetLoader.LoadTexture("imageTest.png");

            //sprite = new Raya.Graphics.Sprite(loadedTexture);

            //bool traktor = true;
        }

        public override void Update()
        {
            //Core.Draw(sprite);
        }
    }
}
