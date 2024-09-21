#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using System.Xml.Linq;
using Emotion.Common;
using Emotion.Editor.EditorHelpers;
using Emotion.ExecTest.TestGame;
using Emotion.Game.Time.Routines;
using Emotion.Game.World2D;
using Emotion.Game.World2D.SceneControl;
using Emotion.Game.World3D;
using Emotion.Game.World3D.SceneControl;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.IO;
using Emotion.Network.Base;
using Emotion.Network.BasicMessageBroker;
using Emotion.Network.ClientSide;
using Emotion.Network.ServerSide;
using Emotion.Platform.Input;
using Emotion.Primitives;
using Emotion.Scenography;
using Emotion.Standard.Reflector;
using Emotion.Standard.XML;
using Emotion.Testing;
using Emotion.UI;
using Emotion.Utility;
using Emotion.WIPUpdates.One;
using Emotion.WIPUpdates.One.Work;
using WinApi.User32;

#endregion

namespace Emotion.ExecTest;

public class Program
{
    private static void Main(string[] args)
    {
        if (CommandLineParser.FindArgument(args, "tests", out string _))
        {
            MainTests(args);
            return;
        }

        Engine.Start(new Configurator
        {
            DebugMode = true,
            HostTitle = "Example"
        }, EntryPointAsync);
    }

    private static void MainTests(string[] args)
    {
        var config = new Configurator
        {
            DebugMode = true
        };

        TestExecutor.ExecuteTests(args, config);
    }

    private void ReflectorTest()
    {
        var data = ReflectorEngine.GetTypeHandler(typeof(Program));
        var members = data.GetMembers();
        foreach (var member in members)
        {
            member.ReadValueFromComplexObject(new Program(), out object? val);
        }
    }

    private static IEnumerator EntryPointAsync()
    {
        //yield return Engine.SceneManager.SetScene(new TestScene());
        yield return Engine.SceneManager.SetScene(new TimeSyncMultiplayer_TestScene());
    }
}