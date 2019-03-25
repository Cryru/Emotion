#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Adfectus.Common;
using Adfectus.Graphics;
using Adfectus.Tests.Scenes;
using FreeImageAPI;
using OpenGL;

#endregion

namespace Adfectus.Tests
{
    public static class Helpers
    {
        private static int _id;

        /// <summary>
        /// Some renders have one or two pixels difference on other GPUs. The default hashes are using the Intel Graphics driver,
        /// and these are the alternative hashes for each one.
        /// </summary>
        private static Dictionary<string, string> _alternativeHashes = new Dictionary<string, string>
        {
            // Left is alt, right is expected

            // Nvidia 940MX - One or two pixels are different per image.
            {"kgTAeFMB2FPHa1aA90hwPl15Jvr8se06zGINrfUf930=", "8aivucpanUVd+Ji0bDXfgk6H4N50z+MeNwlkzUhQGas="}, // alpha drawing
            {"sev0+9e8TKaNW776Phms0VFqJ1zDZeQWeMhCcK+qCqg=", "YnuPlWmWruXr2t3NIVrCDj4b+fEVf6/DGH/us3q6TvQ="}, // depth test
            {"E6TiSM+rzoZnOQhVl77DUk8UZGT8XQyulGjfP96NFGU=", "MZqlv1R57P1ofwNJzE4XOmyO2Bp2ZLuWaYwzgAgcBcY="},
            {"/ROXYjWg1FazjMK4uk0+8Sl2wyUPWOm3Kt6ca1dA8c4=", "l5ymQ0MWYBzsM7iFYqKsiKMjRqA2UWQmqOsWW19K7Iw="},

            // Color off by 1
            {"z+S0N65sLYB+lL1awHRBuQ7nl8CAyulYzDpV60BBv6g=", "PKlb8X5gkVtCWie3LIKpcYzEc5nNjr2Xrz/gCqs54yA="},
            {"UZJ8LxZonLgmzxvtN/ohbj8fOXLwZ8hsDh4luZafyk8=", "wPWltXJb1nv7uc9zvVZ9ks+sKvLXRoUw1wfjL7iBo9I="},

            // MacOS Intel 4000 - A couple of pixels off from the Nvidia one.
            {"Q9SEFh9vZPGjkr1PAU4CQaLpr6Mw1TzqfkfwEShj4M0=", "8aivucpanUVd+Ji0bDXfgk6H4N50z+MeNwlkzUhQGas="}, // alpha drawing
            {"eyG5KLwoB+n7zb3YBlDHKUx88J2Nei0a5tYU1q6uJyk=", "YnuPlWmWruXr2t3NIVrCDj4b+fEVf6/DGH/us3q6TvQ="}, // Lots of pixels shifted on this one. Depth test
            {"ZMMGuCf1hB9/M5nSpzskBKSPntp43gD/pdpB0R+/WcI=", "eJmRj5eeyngfsGEkmMJDYBVqK9pzZpOCK6GNctWpVUg="}, // Stream buffer test. A couple of pixels on the heart logo.
            {"gjRM42NnqSthaDCODi8la+7jklcrik52G18woKYa1s0=", "MZqlv1R57P1ofwNJzE4XOmyO2Bp2ZLuWaYwzgAgcBcY="}, // Weird tilemap drawing. The differences in this test really show the sampler differences.
            {"4Z0J5whko6/LROZxJNklqov6Y/YlZckSgfbnR6Iy9CQ=", "l5ymQ0MWYBzsM7iFYqKsiKMjRqA2UWQmqOsWW19K7Iw=" } // Arbitrary vertices draw. Differences in color.

        };

        /// <summary>
        /// Load a scene and ensure it has ran at least one cycle.
        /// </summary>
        /// <param name="scene">A reference to the scene to load.</param>
        public static void LoadScene(TestScene scene)
        {
            Engine.SceneManager.SetScene(scene);
            scene.WaitFrames(1).Wait();
        }

        /// <summary>
        /// Unloads the scene, and waits for the unload function to be called.
        /// </summary>
        public static void UnloadScene()
        {
            TestScene scene = (TestScene) Engine.SceneManager.Current;
            if (scene == Engine.SceneManager.LoadingScreen) return;
            Engine.SceneManager.SetScene(null);
            while (scene != null && !scene.UnloadCalled)
            {
            }
        }

        /// <summary>
        /// Take a screenshot of the host framebuffer.
        /// </summary>
        /// <returns>Hash of the screenshot.</returns>
        public static unsafe string TakeScreenshot()
        {
            int w = (int) Engine.Host.Size.X;
            int h = (int) Engine.Host.Size.Y;

            byte[] pixels = new byte[3 * w * h];

            // Read the pixels.
            GLThread.ExecuteGLThread(() =>
            {
                Engine.GraphicsManager.CheckError("before screenshot");
                fixed (byte* pixel = &pixels[0])
                {
                    Gl.ReadPixels(0, 0, w, h, PixelFormat.Bgr, PixelType.UnsignedByte, new IntPtr(pixel));
                }

                Engine.GraphicsManager.CheckError("after screenshot");
            });

            // Compute the hash.
            string hash = Convert.ToBase64String(new SHA256Managed().ComputeHash(pixels));

            // Save the file.
            FIBITMAP image = FreeImage.ConvertFromRawBitsArray(pixels, w, h, w * 3, 24, 0, 0, 0, false);
            FreeImage.Save(FREE_IMAGE_FORMAT.FIF_PNG, image,
                $".{Path.DirectorySeparatorChar}ReferenceImages{Path.DirectorySeparatorChar}{_id} ~ {hash.Replace("+", "_").Replace("/", "-").Replace("=", " ")}.png",
                FREE_IMAGE_SAVE_FLAGS.DEFAULT);
            _id++;

            // Check if the hash is an alternative.
            if (_alternativeHashes.ContainsKey(hash)) return _alternativeHashes[hash];

            // Return hash.
            return hash;
        }
    }
}