#region Using

using System.Collections;
using System.Threading;
using Emotion.Graphics;
using Emotion.Scenography;

#endregion

#nullable enable

namespace Emotion.Testing;

public abstract class TestingScene : Scene
{
	public override void Update()
	{
		if (!_runUpdateLoop.IsSet || _runLoopsConstant)
		{
			TestUpdate();
			_runUpdateLoop.Set();
		}
	}

	public override void Draw(RenderComposer composer)
	{
		if (!_runRenderLoop.IsSet || _runLoopsConstant)
		{
			TestDraw(composer);
			_runRenderLoop.Set();
		}
	}

	protected abstract void TestUpdate();
	protected abstract void TestDraw(RenderComposer c);
	public abstract Func<IEnumerator>[] GetTestCoroutines();

	private ManualResetEventSlim _runUpdateLoop = new(true);
	private ManualResetEventSlim _runRenderLoop = new(true);
	private bool _runLoopsConstant;

	public void RunLoop()
	{
		_runUpdateLoop.Reset();
		_runRenderLoop.Reset();

		_runUpdateLoop.Wait();
		_runRenderLoop.Wait();
	}

	public void RunLoopsConstant(bool toggle)
	{
		_runLoopsConstant = toggle;
		_runUpdateLoop.Reset();
		_runRenderLoop.Reset();
	}
}