#region Using

using System.Collections.Generic;
using System.IO;
using Emotion.Standard.Image.PNG;
using Emotion.Utility;

#endregion

namespace Tests.Results
{
    /// <summary>
    /// A database of render results to compare against.
    /// </summary>
    public static class ResultDb
    {
        public static string ClearColorBasicRender = "ClearColorBasicRender";
        public static string RenderComposerSpriteLimitTest = "RenderComposerSpriteLimitTest";
        public static string RenderComposerCustomBatch = "RenderComposerCustomBatch";
        public static string Bmp16ColorDecode = "Bmp16ColorDecode";
        public static string Bmp24BitDecode = "Bmp24BitDecode";
        public static string Bmp256ColorDecode = "Bmp256ColorDecode";
        public static string PngDecode = "PngDecode";
        public static string PngDecodeInterlaced = "PngDecodeInterlaced";
        public static string EmotionTtAtlas = "EmotionTTAtlas";
        public static string EmotionCffAtlas = "EmotionCffAtlas";

        public static string EmotionCompositeAtlas = "EmotionCompositeAtlas";
        public static string ComposerRenderText = "ComposerRenderText";
        public static string ComposerRender = "ComposerRender";
        public static string ComposerRenderSorted = "ComposerRenderSorted";
        public static string ComposerDepthTest = "ComposerDepthTest";

        public static string RenderRichText = "RenderRichText";

        public static string AnimatedTextureTest1 = "AnimatedTextureTest1";
        public static string AnimatedTextureTest2 = "AnimatedTextureTest2";
        public static string AnimatedTextureTest3 = "AnimatedTextureTest3";
        public static string AnimatedTextureTest4 = "AnimatedTextureTest4";
        public static string AnimatedTextureTest5 = "AnimatedTextureTest5";
        public static string AnimatedTextureTest6 = "AnimatedTextureTest6";
        public static string AnimatedTextureTest7 = "AnimatedTextureTest7";
        public static string AnimatedTextureTest8 = "AnimatedTextureTest8";
        public static string AnimatedTextureTest9 = "AnimatedTextureTest9";
        public static string AnimatedTextureTest10 = "AnimatedTextureTest10";

        public static string ShaderTest0 = "ShaderTest0";
        public static string ShaderTest1 = "ShaderTest1";
        public static string ShaderTest2 = "ShaderTest2";
        public static string ShaderTest3 = "ShaderTest3";
        public static string BrokenShader = "BrokenShader";
        public static string ShaderFallback = "ShaderFallback";

        public static string TilemapRender = "TilemapRender";
        public static string TestDepthFromOtherFrameBuffer = "TestDepthFromOtherFrameBuffer";

        public static string TestFullScale = "TestFullScale";
        public static string TestFullScaleInteger = "TestFullScaleInteger";

        public static Dictionary<string, byte[]> CachedResults = new Dictionary<string, byte[]>();

        public static void LoadCache()
        {
            string folderName = Path.Join("Assets", "CachedResults");

            if (!Directory.Exists(folderName)) return;

            string[] files = Directory.GetFiles(folderName);

            foreach (string file in files)
            {
                byte[] fileData = File.ReadAllBytes(file);
                string fileName = Path.GetFileNameWithoutExtension(file);
                byte[] pixels = PngFormat.Decode(fileData, out PngFileHeader header);
                ImageUtil.FlipImageY(pixels, header.Height);
                CachedResults.Add(fileName, pixels);
            }
        }
    }
}