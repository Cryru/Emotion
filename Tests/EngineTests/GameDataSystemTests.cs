using Emotion.Common;
using Emotion.Game.Data;
using Emotion.Platform;
using Emotion.Platform.Implementation.CommonDesktop;
using Emotion.Testing;
using System.Collections;
using System.IO;
namespace Tests.EngineTests;

public class GameDataClassForTest : GameDataObject
{
    public string StringProperty;
    public int NumberProperty;

    public string StringPropertyWithDefault = "Hi";
    public int NumberPropertyWithDefault = 1;
}

[Test]
public class GameDataSystemTests
{
    [Test]
    public IEnumerator CreateData()
    {
        const string NEW_DATA_NAME = "TestData1";

        PlatformBase host = Engine.Host;
        if (host is not DesktopPlatform desktopHost) yield break;

        string projectFolder = desktopHost.DeveloperMode_GetProjectFolder();
        if (projectFolder == "") yield break;

        // First the game data doesn't exist, this script should fail.
        yield return TestExecutor.RunTestScriptInSubProcess($"" +
            $"using GameData;\n" +
            $"using Emotion.Testing;\n" +
            $"\n" +
            $"var gameDataExists = GameData.GameDataClassForTestDefs.{NEW_DATA_NAME};\n",
            TestExecutor.TestScriptResult.Error);

        string gameDataFolder = Path.Join(projectFolder, "GameData");
        string gameDataFolderForType = Path.Join(gameDataFolder, nameof(GameDataClassForTest));

        Assert.False(Directory.Exists(gameDataFolder));
        Assert.False(Directory.Exists(gameDataFolderForType));

        var newData = new GameDataClassForTest();
        newData.Id = NEW_DATA_NAME;
        newData.StringProperty = "Yo";
        newData.NumberProperty = 1337;
        GameDatabase.EditorAdapter.SaveChanges(typeof(GameDataClassForTest), [newData]);

        GameDataObject[] data = GameDatabase.GetObjectsOfType(typeof(GameDataClassForTest));
        Assert.Equal(data.Length, 1);
        Assert.True(data[0].Id == NEW_DATA_NAME);

        GameDataObject[] objects = GameDatabase.GetObjectsOfType(typeof(GameDataClassForTest));
        Assert.Equal(objects[0], newData);

        GameDataClassForTest obj = GameDatabase.GetObject<GameDataClassForTest>(NEW_DATA_NAME);
        Assert.Equal(obj, newData);

        Assert.True(Directory.Exists(gameDataFolder));
        Assert.True(Directory.Exists(gameDataFolderForType));

        // Now the game data exists, this script should pass.
        yield return TestExecutor.RunTestScriptInSubProcess($@"
            using GameData;
            using Emotion.Testing;

            var data = GameData.GameDataClassForTestDefs.{NEW_DATA_NAME};
            Assert.NotNull(data);
            Assert.Equal(data.StringProperty, ""{newData.StringProperty}"");
            Assert.Equal(data.NumberProperty, {newData.NumberProperty});
            Assert.Equal(data.NumberPropertyWithDefault, {newData.NumberPropertyWithDefault});
            Assert.Equal(data.StringPropertyWithDefault, ""{newData.StringPropertyWithDefault}"");
        ");

        Directory.Delete(gameDataFolder, true);

        // We've deleted the data, the script should once again fail.
        yield return TestExecutor.RunTestScriptInSubProcess($"" +
            $"using GameData;\n" +
            $"using Emotion.Testing;\n" +
            $"\n" +
            $"var gameDataExists = GameData.GameDataClassForTestDefs.{NEW_DATA_NAME};\n",
            TestExecutor.TestScriptResult.Error);

        yield break;
    }
}
