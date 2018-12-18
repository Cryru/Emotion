// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Emotion.Engine;
using Emotion.Tests.Interoperability;
using Emotion.Tests.Layers;

#endregion

namespace Emotion.Tests
{
    /// <summary>
    /// Test Helpers.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// The test host.
        /// </summary>
        private static TestHost _host;

        /// <summary>
        /// The image index being hashed.
        /// </summary>
        private static int _imageCounter;

        /// <summary>
        /// List of image hashes.
        /// </summary>
        public static Dictionary<int, string> Hashes = new Dictionary<int, string>();

        /// <summary>
        /// Get the test host. Don't call the helpers file before it has been created.
        /// </summary>
        static Helpers()
        {
            _host = TestInit.TestingHost;
        }

        /// <summary>
        /// Get the hash of an image.
        /// </summary>
        /// <param name="image">The image to hash.</param>
        /// <returns>The hash of the image as a base64 string.</returns>
        public static string Hash(this Image image)
        {
            byte[] bytes = new byte[1];
            bytes = (byte[]) new ImageConverter().ConvertTo(image, bytes.GetType());
            string hash = Convert.ToBase64String(new SHA256Managed().ComputeHash(bytes ?? throw new InvalidOperationException()));

            // If already contained, don't save a reference image.
            if (Hashes.ContainsValue(hash)) return hash;

            // Store the image for future reference.
            Directory.CreateDirectory("ReferenceImages");
            image.Save($"ReferenceImages\\{_imageCounter} - {hash.Replace("+", "_").Replace("/", "-").Replace("=", " ")}.png");
            Hashes.Add(_imageCounter, hash);
            _imageCounter++;
            return hash;
        }

        /// <summary>
        /// Load a layer. Handles waiting and running the test host's cycle.
        /// </summary>
        /// <param name="reference">A reference to the test layer to load.</param>
        /// <param name="name">Name of the layer.</param>
        /// <param name="priority">Priority of the layer.</param>
        public static void LoadLayer(LayerLoading reference, string name, int priority = 0)
        {
            // Add layer.
            Task loadingTask = Context.LayerManager.Add(reference, name, priority);
            // Wait for loading to complete.
            while (!loadingTask.IsCompleted || !reference.DrawCalled) _host.RunCycle();

            // Run an additional cycle to ensure buffers are swapped.
            _host.RunCycle();
        }

        /// <summary>
        /// Unload a layer. Handles waiting and running the test host's cycle.
        /// </summary>
        /// <param name="layer">A reference to the layer to remove.</param>
        public static void UnloadLayer(LayerLoading layer)
        {
            // Remove the layer.
            Task removingTask = Context.LayerManager.Remove(layer);

            // Wait for it to complete.
            while (!removingTask.IsCompleted || !layer.UnloadCalled) _host.RunCycle();
        }
    }
}