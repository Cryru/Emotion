#nullable enable

#region Using

using System.Collections;
using System.Threading.Tasks;
using Emotion.Graphics;

#endregion

// This is a template on declaring test scenes and routines within them.

namespace Emotion.Testing.Templates;

public class EmptyTestSceneTemplate : TestingScene
{
	public override Task LoadAsync()
	{
		return Task.CompletedTask;
	}

	protected override void TestUpdate()
	{
	}

	protected override void TestDraw(RenderComposer c)
	{
	}

	public override Func<IEnumerator>[] GetTestCoroutines()
	{
		return new Func<IEnumerator>[]
		{
			TemplateTestRoutine
		};
	}

	public IEnumerator TemplateTestRoutine()
	{
		yield break;
	}
}