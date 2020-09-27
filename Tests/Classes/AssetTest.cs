#region Using

using System.IO;
using System.Linq;
using Emotion.Common;
using Emotion.Game;
using Emotion.IO;
using Emotion.Platform.Implementation.CommonDesktop;
using Emotion.Test;

#endregion

namespace Tests.Classes
{
    /// <summary>
    /// Tests connected to assets.
    /// Some of the other tests like Drawing test Texture loading so that won't be tested here.
    /// This is more about the lifecycle.
    /// </summary>
    [Test("Assets", true)]
    public class AssetTest
    {
        /// <summary>
        /// Test loading of embedded assets.
        /// </summary>
        [Test]
        public void EmbeddedAssetLoading()
        {
            // Asset should exist.
            Assert.True(Engine.AssetLoader.Exists("Embedded/embedText.txt"));

            // Test loading of embedded text file.
            var textFile = Engine.AssetLoader.Get<TextAsset>("Embedded/embedText.txt");

            // Assert the file is as expected.
            Assert.True(Engine.AssetLoader.Exists("Embedded/embedText.txt"));
            Assert.Equal(31, textFile.Content.Length);
            Assert.Equal("Hello, I am an embedded file ^^", textFile.Content);

            // The asset should be considered loaded.
            Assert.True(Engine.AssetLoader.Loaded("Embedded/embedText.txt"));

            // The name should be case insensitive.
            var insensitive = Engine.AssetLoader.Get<TextAsset>("embedded/embedtext.txt");

            // Should even be the same reference.
            Assert.True(ReferenceEquals(insensitive, textFile));

            // Destroy it.
            Engine.AssetLoader.Destroy("Embedded/embedText.txt");

            // Should be gone.
            Assert.True(Engine.AssetLoader.LoadedAssets.Select(x => x.Name).FirstOrDefault(x => x == "Embedded/embedText.txt") == null);

            // Load non existing asset.
            textFile = Engine.AssetLoader.Get<TextAsset>("sadsadsa");

            // Should be null.
            Assert.True(textFile == null);
            Assert.False(Engine.AssetLoader.Exists("sadsadsa"));

            // Destroy non existent.
            Engine.AssetLoader.Destroy("sadsadsa");

            // Create and destroy null.
            Engine.AssetLoader.Get<TextAsset>(null);
            Engine.AssetLoader.Destroy(null);
        }

        /// <summary>
        /// Creation of a custom asset loader.
        /// </summary>
        [Test]
        public void CustomAssetLoader()
        {
            // Refresh the file which the tests edit.
            File.WriteAllText($"OtherAssets{Path.DirectorySeparatorChar}LoremIpsum.txt", "Lorem ipsum");

            var customLoader = new AssetLoader(new AssetSource[] {new FileAssetSource("OtherAssets")});
            var loremIpsum = customLoader.Get<TextAsset>("LoremIpsum.txt");

            // Assert the file loaded properly.
            Assert.Equal(11, loremIpsum.Content.Length);
            Assert.Equal("Lorem ipsum", loremIpsum.Content);

            // Shouldn't exist in the other asset loader.
            Assert.False(Engine.AssetLoader.Exists("LoremIpsum.txt"));

            // Loading a file which exists both embedded and file.
            // In this case the file loader will load the file, and the embedded file won't be considered a part of the main manifest.
            var conflictLoader = new AssetLoader(new AssetSource[] {new FileAssetSource("OtherAssets"), new EmbeddedAssetSource(typeof(AssetTest).Assembly, "OtherAssets")});
            loremIpsum = conflictLoader.Get<TextAsset>("LoremIpsum.txt");

            // Assert the file loaded properly.
            Assert.Equal(11, loremIpsum.Content.Length);
            Assert.Equal("Lorem ipsum", loremIpsum.Content);

            // Modify the file, and reload.
            File.WriteAllText($"OtherAssets{Path.DirectorySeparatorChar}LoremIpsum.txt", "Lorem Edited");
            loremIpsum = conflictLoader.Get<TextAsset>("LoremIpsum.txt");

            // The file shouldn't have changed, as it is cached.
            Assert.Equal(11, loremIpsum.Content.Length);
            Assert.Equal("Lorem ipsum", loremIpsum.Content);

            // Delete. Reload, and check for change.
            conflictLoader.Destroy("LoremIpsum.txt");
            loremIpsum = conflictLoader.Get<TextAsset>("LoremIpsum.txt");
            Assert.Equal(12, loremIpsum.Content.Length);
            Assert.Equal("Lorem Edited", loremIpsum.Content);

            // Only one file should exists.
            Assert.True(conflictLoader.AllAssets.Length == 1);

            // Creating an asset loader with a non existent root directory should create that directory.
            if (Directory.Exists("NewDir")) Directory.Delete("NewDir");
            Assert.False(Directory.Exists("NewDir"));
            var noDirectory = new AssetLoader(new AssetSource[] {new FileAssetSource("NewDir")});
            Assert.True(Directory.Exists("NewDir"));
        }

        public class TestStorage
        {
            public string Text { get; set; }
        }

        /// <summary>
        /// Asset store
        /// </summary>
        [Test]
        public void AssetStorage()
        {
            var save = Engine.AssetLoader.Get<SaveFile<TestStorage>>("saveFile.save");
            Assert.True(save == null);

            var saveFilePath = "Player/saveFile.save";
            var saveFile = new SaveFile<TestStorage>(saveFilePath);
            Assert.True(File.Exists(Path.Join(".", "Player", "saveFile.save")));

            saveFile.Content.Text = "Wassaa";
            saveFile.Save();

            // It will now exist!
            save = Engine.AssetLoader.Get<SaveFile<TestStorage>>("Player/saveFile.save");
            Assert.Equal(save.Content.Text, saveFile.Content.Text);
            Engine.AssetLoader.Destroy(saveFilePath);

            saveFile.Save();
            Assert.True(File.Exists(Path.Join("./Player", "saveFile.save.backup")));
        }
    }
}