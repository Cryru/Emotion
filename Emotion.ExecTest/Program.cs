#region Using

global using Emotion.ExecTest.Examples;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Game.World2D;
using Emotion.Game.World2D.SceneControl;
using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.Testing;

#endregion

namespace Emotion.ExecTest;

public class Program : World2DBaseScene<Map2D>
{
	private static void Main(string[] args)
	{
		var config = new Configurator
		{
			DebugMode = true
		};

		Engine.Setup(config);
		Engine.SceneManager.SetScene(new Program());
		Engine.Run();
	}

	private static void MainTests(string[] args)
	{
		var config = new Configurator
		{
			DebugMode = true
		};

		Engine.Setup(config);
		TestExecutor.ExecuteTests(args, config);
		Engine.Run();
	}

	public override Task LoadAsync()
	{
		_editor.EnterEditor();
		return Task.CompletedTask;
	}

	public override void Draw(RenderComposer composer)
	{
		composer.SetUseViewMatrix(false);
		composer.RenderSprite(Vector3.Zero, composer.CurrentTarget.Size, Color.CornflowerBlue);
		composer.ClearDepth();
		composer.SetUseViewMatrix(true);

		base.Draw(composer);
	}
}