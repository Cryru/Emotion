#nullable enable

#region Using

using System.Collections;
using System.Threading.Tasks;
using Emotion.Graphics;
using Emotion.UI;
using Emotion.Utility;

#endregion

namespace Emotion.Testing;

public class TestsUI : TestingScene
{
	public UIController UI = null!;

	public override async Task LoadAsync()
	{
		UI = new UIController();
		await UI.PreloadUI();
	}

	protected override void TestUpdate()
	{
		UI.InvalidateLayout();
		UI.Update();
	}

	protected override void TestDraw(RenderComposer c)
	{
		c.SetUseViewMatrix(false);
		UI.Render(c);
	}

	public override Func<IEnumerator>[] GetTestCoroutines()
	{
		return new Func<IEnumerator>[]
		{
			TestWindow,
			TestFreeLayout,
			TestFreeLayoutWithStretching
		};
	}

	private IEnumerator TestWindow()
	{
		UI.ClearChildren();

		var win = new UISolidColor();
		win.StretchX = true;
		win.StretchY = true;
		UI.AddChild(win);

		yield break;
		//yield return new TestWaiterRunLoops(-1);
	}

	private IEnumerator TestFreeLayout()
	{
		UI.ClearChildren();

		// Test all anchor combinations
		UIAnchor[] anchorValues = Enum.GetValues<UIAnchor>();
		foreach (UIAnchor anchorVal in anchorValues)
		{
			foreach (UIAnchor parentAnchor in anchorValues)
			{
				var win = new UISolidColor();
				win.SetExactRequestedSize(new Vector2(15, 15));
				win.WindowColor = Color.White;
				win.Anchor = anchorVal;
				win.ParentAnchor = parentAnchor;
				win.Id = $"{anchorVal}-{parentAnchor}";
				UI.AddChild(win);
			}
		}

		yield break;
		//yield return new TestWaiterRunLoops(-1);
	}

	private IEnumerator TestFreeLayoutWithStretching()
	{
		UI.ClearChildren();

		{
			var win = new UISolidColor();
			win.StretchX = true;
			win.MinSizeY = 50;
			UI.AddChild(win);
		}

		{
			var win = new UISolidColor();
			win.StretchX = true;
			win.MinSizeY = 50;
			win.Anchor = UIAnchor.CenterCenter;
			win.ParentAnchor = UIAnchor.CenterCenter;
			win.Offset = new Vector2(-10, 0);
			UI.AddChild(win);
		}

		{
			var win = new UISolidColor();
			win.StretchX = true;
			win.MinSizeY = 50;
			win.Anchor = UIAnchor.BottomRight;
			win.ParentAnchor = UIAnchor.BottomLeft;
			win.Offset = new Vector2(10, 0);
			UI.AddChild(win);
		}

		{
			var win = new UISolidColor();
			win.StretchY = true;
			win.MinSizeX = 50;
			win.WindowColor = Color.Red * 0.5f;
			UI.AddChild(win);
		}

		{
			var win = new UISolidColor();
			win.StretchY = true;
			win.MinSizeX = 50;
			win.Anchor = UIAnchor.CenterCenter;
			win.ParentAnchor = UIAnchor.CenterCenter;
			win.Offset = new Vector2(0, -10);
			win.WindowColor = Color.Red * 0.5f;
			UI.AddChild(win);
		}

		{
			var win = new UISolidColor();
			win.StretchY = true;
			win.MinSizeX = 50;
			win.Anchor = UIAnchor.BottomRight;
			win.ParentAnchor = UIAnchor.TopRight;
			win.Offset = new Vector2(0, 10);
			win.WindowColor = Color.Red * 0.5f;
			UI.AddChild(win);
		}

		yield return null;
		//yield return new TestWaiterRunLoops(-1);

		// Test interactions between stretching and margins
		for (var i = 0; i < UI.Children!.Count; i++)
		{
			UIBaseWindow child = UI.Children[i];
			child.Margins = new Rectangle(10, 10, 10, 10);
			child.Offset = Vector2.Zero;
		}

		yield return null;
		//yield return new TestWaiterRunLoops(-1);
		
		// Test interactions between stretching and paddings
		for (var i = 0; i < UI.Children!.Count; i++)
		{
			UIBaseWindow child = UI.Children[i];
			child.Margins = Rectangle.Empty;
			child.Offset = Vector2.Zero;
		}

		UI.Paddings = new Rectangle(10, 10, 10, 10);

		yield return null;
		//yield return new TestWaiterRunLoops(-1);

		// Restore for the next test.
		UI.Paddings = Rectangle.Empty;
	}
}