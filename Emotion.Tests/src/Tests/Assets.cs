// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.IO;
using System.Linq;
using Emotion.Engine;
using Emotion.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Emotion.Tests.Tests
{
    /// <summary>
    /// Tests connected to assets.
    /// Some of the other tests like Drawing test Texture loading so that won't be tested here.
    /// This is more about the lifecycle.
    /// </summary>
    [TestClass]
    public class Assets
    {
        /// <summary>
        /// Test loading of embedded assets.
        /// </summary>
        [TestMethod]
        public void EmbeddedAssetLoading()
        {
            // Asset should exist.
            Assert.IsTrue(Context.AssetLoader.Exists("Embedded/embedText.txt"));

            // Test loading of embedded text file.
            TextFile textFile = Context.AssetLoader.Get<TextFile>("Embedded/embedText.txt");

            // Assert the file is as expected.
            Assert.IsTrue(Context.AssetLoader.Exists("Embedded/embedText.txt"));
            Assert.AreEqual(1, textFile.Content.Length);
            Assert.AreEqual("Hello, I am an embedded file ^^", textFile.Content[0]);

            // The asset should be considered loaded.
            Assert.IsTrue(Context.AssetLoader.LoadedAssets.Select(x => x.Name).Contains("Embedded/embedText.txt"));

            // The name should be case insensitive.
            TextFile insensitive = Context.AssetLoader.Get<TextFile>("embedded/embedtext.txt");

            // Should even be the same reference.
            Assert.IsTrue(ReferenceEquals(insensitive, textFile));

            // Destroy it.
            Context.AssetLoader.Destroy("Embedded/embedText.txt");

            // Should be gone.
            Assert.IsFalse(Context.AssetLoader.LoadedAssets.Select(x => x.Name).Contains("Embedded/embedText.txt"));

            // Load non existing asset.
            textFile = Context.AssetLoader.Get<TextFile>("sadsadsa");

            // Should be null.
            Assert.AreEqual(null, textFile);
            Assert.IsFalse(Context.AssetLoader.Exists("sadsadsa"));

            // Destroy non existent.
            Context.AssetLoader.Destroy("sadsadsa");

            // Create and destroy null.
            Context.AssetLoader.Get<TextFile>(null);
            Context.AssetLoader.Destroy(null);
        }

        /// <summary>
        /// Creation of a custom asset loader.
        /// </summary>
        [TestMethod]
        public void CustomAssetLoader()
        {
            AssetLoader customLoader = new AssetLoader("OtherAssets");

            TextFile loremIpsum = customLoader.Get<TextFile>("LoremIpsum.txt");

            // Assert the file loaded properly.
            Assert.AreEqual(1, loremIpsum.Content.Length);
            Assert.AreEqual("Lorem ipsum", loremIpsum.Content[0]);

            // Shouldn't exist in the other asset loader.
            Assert.IsFalse(Context.AssetLoader.Exists("LoremIpsum.txt"));

            // Loading a file which exists both embedded and file.
            // In this case the file loader will load the file, and the embedded file won't be considered a part of the main manifest.
            AssetLoader conflictLoader = new AssetLoader("OtherAssets", new[] {typeof(Assets).Assembly});
            loremIpsum = conflictLoader.Get<TextFile>("LoremIpsum.txt");

            // Assert the file loaded properly.
            Assert.AreEqual(1, loremIpsum.Content.Length);
            Assert.AreEqual("Lorem ipsum", loremIpsum.Content[0]);

            // Modify the file, and reload.
            File.WriteAllText("OtherAssets\\LoremIpsum.txt", "Lorem Edited");
            loremIpsum = conflictLoader.Get<TextFile>("LoremIpsum.txt");

            // The file shouldn't have changed, as it is cached.
            Assert.AreEqual(1, loremIpsum.Content.Length);
            Assert.AreEqual("Lorem ipsum", loremIpsum.Content[0]);

            // Delete. Reload, and check for change.
            conflictLoader.Destroy("LoremIpsum.txt");
            loremIpsum = conflictLoader.Get<TextFile>("LoremIpsum.txt");
            Assert.AreEqual(1, loremIpsum.Content.Length);
            Assert.AreEqual("Lorem Edited", loremIpsum.Content[0]);

            // Only one file should exists.
            Assert.AreEqual(1, conflictLoader.AllAssets.Length);

            // Creating an asset loader with a non existent root directory should create that directory.
            if (Directory.Exists("NewDir")) Directory.Delete("NewDir");
            Assert.IsFalse(Directory.Exists("NewDir"));
            AssetLoader noDirectory = new AssetLoader("NewDir");
            Assert.IsTrue(Directory.Exists("NewDir"));
        }
    }
}