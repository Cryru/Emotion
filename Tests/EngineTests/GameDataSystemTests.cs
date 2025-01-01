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
        PlatformBase host = Engine.Host;
        if (host is not DesktopPlatform desktopHost) yield break;

        string projectFolder = desktopHost.DeveloperMode_GetProjectFolder();
        if (projectFolder == "") yield break;

        // First the game data doesn't exist, this script should fail.
        yield return TestExecutor.RunTestScriptInSubProcess($"" +
            $"using GameData;\n" +
            $"using Emotion.Testing;\n" +
            $"\n" +
            $"var gameDataExists = GameData.GameDataClassForTestDefs.Untitled;\n",
            TestExecutor.TestScriptResult.Error);

        string gameDataFolder = Path.Join(projectFolder, "GameData");
        string gameDataFolderForType = Path.Join(gameDataFolder, nameof(GameDataClassForTest));

        Assert.False(Directory.Exists(gameDataFolder));
        Assert.False(Directory.Exists(gameDataFolderForType));

        var newData = new GameDataClassForTest();
        GameDataDatabase.EditorAdapter.EditorAddObject(typeof(GameDataClassForTest), newData);

        string[] data = GameDataDatabase.EditorAdapter.GetObjectIdsOfType(typeof(GameDataClassForTest));
        Assert.Equal(data.Length, 1);
        Assert.True(data[0] == "Untitled");

        Assert.True(Directory.Exists(gameDataFolder));
        Assert.True(Directory.Exists(gameDataFolderForType));

        yield return TestExecutor.RunTestScriptInSubProcess($"" +
            $"using GameData;\n" +
            $"using Emotion.Testing;\n" +
            $"\n" +
            $"var gameDataExists = GameData.GameDataClassForTestDefs.Untitled;\n" +
            $"Assert.NotNull(gameDataExists);\n");

        Directory.Delete(gameDataFolder, true);

        yield break;
    }
}
