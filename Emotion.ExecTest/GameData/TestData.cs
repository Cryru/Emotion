using Emotion.Game.Systems.GameData;
using System;

namespace GameData.MonsterDefs;

//public partial class MonsterDefs
//{
//    public static TestData Test { get => GameDatabase.GetObject(typeof(Monster), "Test") as TestData; }

//    public static void ReloadPrototypes()
//    {
//    }
//}

public partial class TestData
{
    public string Hum = "mda";

    public override void TestInvoke()
    {
        Console.WriteLine("yoo");
        base.TestInvoke();
    }
}

public class Monster : GameDataObject
{
    public int Hp;

    public virtual void TestInvoke()
    {

    }
}
