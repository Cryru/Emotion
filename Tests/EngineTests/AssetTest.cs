#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Emotion.Core;
using Emotion.Core.Systems.IO;
using Emotion.Core.Systems.IO.Sources;
using Emotion.Testing;

#endregion

namespace Tests.EngineTests;

/// <summary>
/// Tests connected to asset loading.
/// Other tests implicitly test loading of particular assets (like textures),
/// so this test is mostly about the asset loading API
/// </summary>
[Test]
public class AssetTest
{
    private class MockAssetSource : IAssetSource<string>
    {
        public static HashSet<string> ValidPaths = new();
        public static byte MagicByte = 13;

        public static FileReadRoutineResult GetAssetContent(string sourceName)
        {
            if (ValidPaths.Contains(sourceName))
            {
                var result = new FileReadRoutineResult();
                result.SetData(new byte[1] { MagicByte });
                return result;
            }

            return FileReadRoutineResult.GenericErrored;
        }
    }

    [Test]
    public IEnumerator GeneralAPI()
    {
        MockAssetSource.ValidPaths.Add("test.ext");

        var loader = new AssetLoader();

        Assert.False(loader.Exists("test.ext"));
        loader.MountCustomSourceAsset<MockAssetSource, string>("test.ext", "test.ext");
        Assert.True(loader.Exists("test.ext"));

        var nonExistingAsset = loader.Get<OtherAsset>("fake.ext", null, true);
        Assert.False(nonExistingAsset.Loaded);
        Assert.True(nonExistingAsset.Processed);

        var asset = loader.Get<OtherAsset>("test.ext", null, true);
        Assert.True(asset.Loaded);
        Assert.True(asset.Content.Span[0] == 13);

        var assetCaseInsensitive = loader.Get<OtherAsset>("tEsT.ext", null, true);
        Assert.True(assetCaseInsensitive.Loaded);
        Assert.Equal(asset, assetCaseInsensitive);

        // Test reloading
        MockAssetSource.MagicByte = 15;
        loader.ReloadAsset("Test.exT"); // also test case insensitive
        loader.Update();
        yield return 100;
        Assert.True(asset.Loaded);
        Assert.True(asset.Content.Span[0] == 15);

        // Test folders and sensitivity
        loader.MountCustomSourceAsset<MockAssetSource, string>("aAeEbB/tEsT.ext", "test.ext");
        Assert.True(loader.Exists("aAeEbB/tEsT.ext"));
        Assert.True(loader.Exists("aaeebb/test.ext"));
        Assert.True(loader.Exists("aaeebb/Test.ext"));

        loader.MountCustomSourceAsset<MockAssetSource, string>("aaeebb\\gushter.png", "test.ext");
        Assert.True(loader.Exists("aAeEbB/GusHTer.png"));
        Assert.True(loader.Exists("aaeebb/gushter.png"));
        Assert.True(loader.Exists("aaeebb/Gushter.png"));

        int count = 0;
        foreach (string item in loader.ForEachAssetInFolder("aaeebb"))
        {
            Assert.True(item == "aaeebb/test.ext" || item == "aaeebb/gushter.png");
            count++;
        }
        Assert.Equal(count, 2);

        // Alias
        loader.MountAssetAlias("pepega/omega.png", "test.ext");
        loader.MountAssetAlias("pepega/two.txt", "not_created_yet.png");

        Assert.True(loader.Exists("pepega/omega.png"));
        Assert.False(loader.Exists("pepega/two.txt"));

        var assetViaAlias = loader.Get<OtherAsset>("pepega/omega.png", null, true);
        Assert.Equal(asset, assetViaAlias);

        var assetNotYetCreated = loader.Get<OtherAsset>("pepega/two.txt", null, true);
        Assert.False(assetNotYetCreated.Loaded);

        loader.MountCustomSourceAsset<MockAssetSource, string>("not_created_yet.png", "test.ext");
        Assert.True(loader.Exists("pepega/two.txt"));

        // Unmounting
        loader.UnmountFile("pepega\\two.txt");
        Assert.False(loader.Exists("pepega/two.txt"));

        // Mounting, unmounting, and renaming singular file (used by hot reload)
        loader.MountFile("myfolder\\myfile.txt", "Data\\myfile.txt");
        Assert.True(loader.Exists("Data/MYFILE.TXT"));
        loader.UnmountFile("Data\\myfile.txt");
        Assert.False(loader.Exists("data/MYFILE.TXT"));
    }

    [Test]
    public void Misc()
    {
        // All must be engine names
        Assert.True(Engine.AssetLoader.ForEachLoadedAsset().All(static x => !x.Name.Contains('\\') && x.Name.Equals(x.Name, StringComparison.InvariantCultureIgnoreCase)));

    }

    /// <summary>
    /// Test loading of embedded assets.
    /// </summary>
    [Test]
    public void OLD_EmbeddedAssetLoading()
    {
        // Asset should exist.
        Assert.True(Engine.AssetLoader.Exists("Embedded/embedText.txt"));

        // Test loading of embedded text file.
        var textFile = Engine.AssetLoader.LEGACY_Get<TextAsset>("Embedded/embedText.txt");

        // Assert the file is as expected.
        Assert.True(Engine.AssetLoader.Exists("Embedded/embedText.txt"));
        Assert.Equal(31, textFile.Content.Length);
        Assert.Equal("Hello, I am an embedded file ^^", textFile.Content);

        // The asset should be considered loaded.
        Assert.True(Engine.AssetLoader.ForEachLoadedAsset().Any(x => x.Name.Equals("Embedded/embedText.txt", StringComparison.InvariantCultureIgnoreCase)));

        // The name should be case insensitive.
        var insensitive = Engine.AssetLoader.LEGACY_Get<TextAsset>("embedded/embedtext.txt");

        // Should even be the same reference.
        Assert.True(ReferenceEquals(insensitive, textFile));

        // Destroy it.
        Engine.AssetLoader.DisposeOf(textFile);

        // Should be gone.
        Assert.False(Engine.AssetLoader.ForEachLoadedAsset().Any(x => x.Name.Equals("Embedded/embedText.txt", StringComparison.InvariantCultureIgnoreCase)));

        // Load non existing asset.
        textFile = Engine.AssetLoader.LEGACY_Get<TextAsset>("sadsadsa");

        // Never return null, but asset shouldnt be loaded
        Assert.False(textFile == null);
        Assert.False(textFile.Loaded);
        Assert.True(textFile.Processed);
        Assert.True(textFile.Finished);
        Assert.False(Engine.AssetLoader.Exists("sadsadsa"));

        // Destroy non existent.
        Engine.AssetLoader.DisposeOf(textFile);

        // Create and destroy null.
        Engine.AssetLoader.LEGACY_Get<TextAsset>(null);
        Engine.AssetLoader.DisposeOf(null);
    }

    [Test]
    public IEnumerator OLD_FileSourceLoading()
    {
        string testFilePath = $"OtherAssets{Path.DirectorySeparatorChar}LoremIpsum.txt";

        // Refresh the file which the tests edit.
        File.WriteAllText(testFilePath, "Lorem ipsum");

        var customLoader = new AssetLoader();
        customLoader.MountFileSystemFolder("OtherAssets", ".");
        var loremIpsum = customLoader.LEGACY_Get<TextAsset>("LoremIpsum.txt");

        // Assert the file loaded properly.
        Assert.Equal(11, loremIpsum.Content.Length);
        Assert.Equal("Lorem ipsum", loremIpsum.Content);

        // Shouldn't exist in the other asset loader.
        Assert.False(Engine.AssetLoader.Exists("LoremIpsum.txt"));

        // Modify the file, and reload.
        File.WriteAllText(testFilePath, "Lorem Edited");
        loremIpsum = customLoader.LEGACY_Get<TextAsset>("LoremIpsum.txt");

        // The file shouldn't have changed, as it is cached.
        Assert.Equal(11, loremIpsum.Content.Length);
        Assert.Equal("Lorem ipsum", loremIpsum.Content);

        // Delete. Reload, and check for change.
        customLoader.DisposeOf(loremIpsum);
        loremIpsum = customLoader.LEGACY_Get<TextAsset>("LoremIpsum.txt");
        Assert.Equal(12, loremIpsum.Content.Length);
        Assert.Equal("Lorem Edited", loremIpsum.Content);

        // Test the automatic reload
        File.WriteAllText(testFilePath, "Lorem Edited Again");
        yield return 100;
        customLoader.Update();
        yield return 100;

        Assert.Equal(18, loremIpsum.Content.Length);
        Assert.Equal("Lorem Edited Again", loremIpsum.Content);

        File.Delete(testFilePath);
        yield return 100;
        Assert.False(customLoader.Exists("LoremIpsum.txt"));
    }

    public class TestStorage
    {
        public string Text { get; set; }
    }

    /// <summary>
    /// Asset store
    /// </summary>
    [Test]
    public IEnumerator AssetStorage()
    {
        var saveFilePath = "Player/saveFile.save";
        var save = Engine.AssetLoader.Get<XMLAsset<TestStorage>>(saveFilePath);
        yield return save;

        Assert.False(save.Loaded);
        Engine.AssetLoader.DisposeOf(save);

        XMLAsset<TestStorage> saveFile = XMLAsset<TestStorage>.LoadOrCreate(saveFilePath);
        yield return saveFile;

        Assert.True(File.Exists(Path.Join(".", "Player", "savefile.save")));
        Assert.True(Engine.AssetLoader.Exists(saveFilePath));

        saveFile.Content.Text = "Wassaa";
        saveFile.Save();

        // It will now exist!
        save = Engine.AssetLoader.Get<XMLAsset<TestStorage>>("Player/saveFile.save");
        yield return save;

        Assert.Equal(save.Content.Text, saveFile.Content.Text);
        Engine.AssetLoader.DisposeOf(save);

        saveFile.Save();
        Assert.True(File.Exists(Path.Join("./Player", "savefile.save.backup")));

        File.Delete(Path.Join(".", "Player", "savefile.save"));
    }
}