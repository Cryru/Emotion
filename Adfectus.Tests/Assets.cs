#region Using

using System.IO;
using System.Linq;
using Adfectus.Common;
using Adfectus.IO;
using Adfectus.Platform.DesktopGL.Assets;
using Xunit;

#endregion

namespace Adfectus.Tests
{
    /// <summary>
    /// Tests connected to assets.
    /// Some of the other tests like Drawing test Texture loading so that won't be tested here.
    /// This is more about the lifecycle.
    /// </summary>
    [Collection("main")]
    public class Assets
    {
        /// <summary>
        /// Test loading of embedded assets.
        /// </summary>
        [Fact]
        public void EmbeddedAssetLoading()
        {
            // Asset should exist.
            Assert.True(Engine.AssetLoader.Exists("Embedded/embedText.txt"));

            // Test loading of embedded text file.
            TextFile textFile = Engine.AssetLoader.Get<TextFile>("Embedded/embedText.txt");

            // Assert the file is as expected.
            Assert.True(Engine.AssetLoader.Exists("Embedded/embedText.txt"));
            Assert.Equal(31, textFile.Content.Length);
            Assert.Equal("Hello, I am an embedded file ^^", textFile.Content);

            // The asset should be considered loaded.
            Assert.True(Engine.AssetLoader.Loaded("Embedded/embedText.txt"));

            // The name should be case insensitive.
            TextFile insensitive = Engine.AssetLoader.Get<TextFile>("embedded/embedtext.txt");

            // Should even be the same reference.
            Assert.True(ReferenceEquals(insensitive, textFile));

            // Destroy it.
            Engine.AssetLoader.Destroy("Embedded/embedText.txt");

            // Should be gone.
            Assert.DoesNotContain("Embedded/embedText.txt", Engine.AssetLoader.LoadedAssets.Select(x => x.Name));

            // Load non existing asset.
            textFile = Engine.AssetLoader.Get<TextFile>("sadsadsa");

            // Should be null.
            Assert.Null(textFile);
            Assert.False(Engine.AssetLoader.Exists("sadsadsa"));

            // Destroy non existent.
            Engine.AssetLoader.Destroy("sadsadsa");

            // Create and destroy null.
            Engine.AssetLoader.Get<TextFile>(null);
            Engine.AssetLoader.Destroy(null);
        }

        /// <summary>
        /// Creation of a custom asset loader.
        /// </summary>
        [Fact]
        public void CustomAssetLoader()
        {
            // Refresh the file which the tests edit.
            File.WriteAllText($"OtherAssets{Path.DirectorySeparatorChar}LoremIpsum.txt", "Lorem ipsum");

            AssetLoader customLoader = new DesktopAssetLoader(new AssetSource[] {new FileAssetSource("OtherAssets")});
            TextFile loremIpsum = customLoader.Get<TextFile>("LoremIpsum.txt");

            // Assert the file loaded properly.
            Assert.Equal(11, loremIpsum.Content.Length);
            Assert.Equal("Lorem ipsum", loremIpsum.Content);

            // Shouldn't exist in the other asset loader.
            Assert.False(Engine.AssetLoader.Exists("LoremIpsum.txt"));

            // Loading a file which exists both embedded and file.
            // In this case the file loader will load the file, and the embedded file won't be considered a part of the main manifest.
            AssetLoader conflictLoader = new DesktopAssetLoader(new AssetSource[] {new FileAssetSource("OtherAssets"), new EmbeddedAssetSource(typeof(Assets).Assembly, "OtherAssets")});
            loremIpsum = conflictLoader.Get<TextFile>("LoremIpsum.txt");

            // Assert the file loaded properly.
            Assert.Equal(11, loremIpsum.Content.Length);
            Assert.Equal("Lorem ipsum", loremIpsum.Content);

            // Modify the file, and reload.
            File.WriteAllText($"OtherAssets{Path.DirectorySeparatorChar}LoremIpsum.txt", "Lorem Edited");
            loremIpsum = conflictLoader.Get<TextFile>("LoremIpsum.txt");

            // The file shouldn't have changed, as it is cached.
            Assert.Equal(11, loremIpsum.Content.Length);
            Assert.Equal("Lorem ipsum", loremIpsum.Content);

            // Delete. Reload, and check for change.
            conflictLoader.Destroy("LoremIpsum.txt");
            loremIpsum = conflictLoader.Get<TextFile>("LoremIpsum.txt");
            Assert.Equal(12, loremIpsum.Content.Length);
            Assert.Equal("Lorem Edited", loremIpsum.Content);

            // Only one file should exists.
            Assert.Single(conflictLoader.AllAssets);

            // Creating an asset loader with a non existent root directory should create that directory.
            if (Directory.Exists("NewDir")) Directory.Delete("NewDir");
            Assert.False(Directory.Exists("NewDir"));
            AssetLoader noDirectory = new DesktopAssetLoader(new AssetSource[] {new FileAssetSource("NewDir")});
            Assert.True(Directory.Exists("NewDir"));
        }
    }
}