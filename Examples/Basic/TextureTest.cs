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


        public static void Main(string[] args)
        {
            // Start the engine.
            Core.Start(new TextureTest(), "textureTest");
        }

        public override void Initialize()
        {
            byte[] data = Read.FileAsBytes("Assets//imageTest.png");
            Raya.Graphics.Texture loadedTexture = new Raya.Graphics.Texture(data);

            bool traktor = true;
        }

        public override void Update()
        {
           
        }
    }
}
