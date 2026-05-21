#nullable enable

using Emotion.Core.Systems.Scenography;
using Emotion.GameDataLib;
using GameData.MonsterDefs;
using ReflectorGen;
using System.Collections;
using System.IO;
using System.Text;

namespace Emotion.ExecTest.ExamplesOne;

public class ExampleEmpty : SceneWithMap
{
    protected override IEnumerator InternalLoadSceneRoutineAsync()
    {
        //ReflectorDataGameDataMonsterDefsTestData
        TestData a = new TestData();
        var aa = a.Id;
        StringBuilder b = new StringBuilder();
        //GameDataGenerator.WriteSerialized(b, a);

        //string dataFile = b.ToString();
        //MemoryStream str = new MemoryStream(System.Text.Encoding.Default.GetBytes(dataFile));
        //b.Clear();
        //GameDataGenerator.WriteClassDef(b, str);

        yield break;
    }
}
