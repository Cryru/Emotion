#region Using

global using Emotion.ExecTest.Examples;
using System.Collections.Generic;
using System.Numerics;
using Emotion.Common;
using Emotion.Game.ThreeDee.Editor;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.Graphics.ThreeDee;
using Emotion.Primitives;
using Emotion.Scenography;
using Emotion.Testing;

#endregion

namespace Emotion.ExecTest
{
	public class Program : IScene
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

			//var config = new Configurator();
			//TestExecutor.SetupConfigForTests(config);
			//Engine.Setup(config);
			//TestExecutor.ExecuteTests(args);
			//Engine.Run();
		}

		private List<Quad3D> _quads = new List<Quad3D>();
		private TranslationGizmo _gizmo;

		public void Load()
		{
			var cam3d = new Camera3D(new Vector3(-290, 250, 260));
			cam3d.LookAtPoint(new Vector3(0, 0, 0));
			Engine.Renderer.Camera = cam3d;

			var ground = new InfiniteGrid();
			_quads.Add(ground);

			var wall = new Quad3D();
			wall.Position = new Vector3(0, 0, 50);
			wall.Size = new Vector3(100, 100, 1);
			wall.RotationDeg = new Vector3(0, -90, 0);
			wall.Tint = Color.Black;
			_quads.Add(wall);

			var transGizmo = new TranslationGizmo();
			transGizmo.SetTarget(wall);
			_gizmo = transGizmo;
		}

		public void Update()
		{
			_gizmo.Update(Engine.DeltaTime);
		}

		public void Draw(RenderComposer composer)
		{
			composer.SetUseViewMatrix(false);
			composer.RenderSprite(Vector3.Zero, composer.CurrentTarget.Size, Color.CornflowerBlue);
			composer.ClearDepth();
			composer.SetUseViewMatrix(true);

			//composer.RenderLine(new Vector3(0, 0, 0), new Vector3(short.MaxValue, 0, 0), Color.Red, snapToPixel: false);
			//composer.RenderLine(new Vector3(0, 0, 0), new Vector3(0, short.MaxValue, 0), Color.Green, snapToPixel: false);
			//composer.RenderLine(new Vector3(0, 0, 0), new Vector3(0, 0, short.MaxValue), Color.Blue, snapToPixel: false);

			for (var i = 0; i < _quads.Count; i++)
			{
				Quad3D quad = _quads[i];
				quad.Render(composer);
			}

			composer.ClearDepth();
			_gizmo.Render(composer);
		}

		public void Unload()
		{
		}
	}
}