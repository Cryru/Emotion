#nullable enable

#region Using

using System.Collections;
using System.Threading.Tasks;
using Emotion.Graphics;
using Emotion.UI;

#endregion

namespace Emotion.Testing;

#if NEW_UI
public class NewUITests : TestingScene
{
	public UIController UI;

	public override Task LoadAsync()
	{
		UI = new UIController();
		return Task.CompletedTask;
	}

	protected override void TestUpdate()
	{
		UI.Update();
	}

	protected override void TestDraw(RenderComposer c)
	{
		c.SetUseViewMatrix(false);
		UI.Render(c);
	}

	public override Func<IEnumerator>[] GetTestCoroutines()
	{
		return new[]
		{
			Fill,
			FillXAxisWithChild,
			FillXAxisMinHeight,
			FillXAxisMinHeightAndWidth,
			FillNeitherAxisMinHeightAndWidth,
			TwoSquaresInFillY,

			FillList,
			FillListThreeItems,
			FillListFillingItems,
		};
	}

	private IEnumerator Fill()
	{
		UI.ClearChildren();

		{
			var win = new UISolidColor();
			win.WindowColor = Color.PrettyOrange;
			win.Id = "test";
			UI.AddChild(win);
		}

		yield return new TestWaiterRunLoops(1);
		VerifyScreenshot();
	}

	private IEnumerator FillXAxisWithChild()
	{
		UI.ClearChildren();

		{
			var win = new UISolidColor();
			win.WindowColor = Color.PrettyOrange;
			win.FillY = false;
			win.Id = "test";

			{
				var a = new UISolidColor();
				a.WindowColor = Color.White;
				win.AddChild(a);
			}

			UI.AddChild(win);
		}

		yield return new TestWaiterRunLoops(1);
		VerifyScreenshot();
	}

	private IEnumerator FillXAxisMinHeight()
	{
		UI.ClearChildren();

		{
			var win = new UISolidColor();
			win.WindowColor = Color.PrettyOrange;
			win.FillY = false;
			win.Id = "test";

			{
				var a = new UISolidColor();
				a.WindowColor = Color.White;
				a.MinSizeY = 20;
				win.AddChild(a);
			}

			UI.AddChild(win);
		}

		yield return new TestWaiterRunLoops(1);
		VerifyScreenshot();
	}

	private IEnumerator FillXAxisMinHeightAndWidth()
	{
		UI.ClearChildren();

		{
			var win = new UISolidColor();
			win.WindowColor = Color.PrettyOrange;
			win.FillY = false;
			win.Id = "test";

			{
				var a = new UISolidColor();
				a.WindowColor = Color.White;
				a.MinSizeY = 20;
				a.MinSizeX = 20;
				win.AddChild(a);
			}

			UI.AddChild(win);
		}

		yield return new TestWaiterRunLoops(1);
		VerifyScreenshot();
	}

	private IEnumerator FillNeitherAxisMinHeightAndWidth()
	{
		UI.ClearChildren();

		{
			var win = new UISolidColor();
			win.WindowColor = Color.PrettyOrange;
			win.FillY = false;
			win.FillX = false;
			win.Id = "test";

			{
				var a = new UISolidColor();
				a.WindowColor = Color.White;
				a.MinSizeY = 20;
				a.MinSizeX = 20;
				win.AddChild(a);
			}

			UI.AddChild(win);
		}

		yield return new TestWaiterRunLoops(1);
		VerifyScreenshot();
	}

	private IEnumerator TwoSquaresInFillY()
	{
		UI.ClearChildren();

		{
			var win = new UISolidColor();
			win.WindowColor = Color.PrettyOrange;
			win.FillX = false;
			win.Id = "test";

			{
				var a = new UISolidColor();
				a.WindowColor = Color.White;
				a.MinSize = new Vector2(20);
				a.MaxSize = new Vector2(20);
				win.AddChild(a);
			}

			{
				var a = new UISolidColor();
				a.WindowColor = Color.Black;
				a.MinSize = new Vector2(20);
				a.MaxSize = new Vector2(20);
				a.AlignAnchor = UIAnchor.BottomLeft;
				win.AddChild(a);
			}

			UI.AddChild(win);
		}

		yield return new TestWaiterRunLoops(1);
		VerifyScreenshot();
	}

	private IEnumerator FillList()
	{
		UI.ClearChildren();

		{
			var win = new UISolidColor();
			win.WindowColor = Color.PrettyOrange;
			win.Id = "test";
			win.LayoutMode = LayoutMode.HorizontalList;

			{
				var a = new UISolidColor();
				a.WindowColor = Color.White;
				win.AddChild(a);
			}

			UI.AddChild(win);
		}

		yield return new TestWaiterRunLoops(1);
		VerifyScreenshot();

		{
			UIBaseWindow list = UI.GetWindowById("test")!;
			list.LayoutMode = LayoutMode.VerticalList;
		}

		yield return new TestWaiterRunLoops(1);
		VerifyScreenshot();
	}

	private IEnumerator FillListThreeItems()
	{
		UI.ClearChildren();

		{
			var win = new UISolidColor();
			win.WindowColor = Color.PrettyOrange;
			win.Id = "test";
			win.LayoutMode = LayoutMode.HorizontalList;

			{
				var a = new UISolidColor();
				a.WindowColor = Color.White;
				a.MinSize = new Vector2(20);
				a.MaxSize = new Vector2(20);
				win.AddChild(a);
			}

			{
				var a = new UISolidColor();
				a.WindowColor = Color.Black;
				a.MinSize = new Vector2(20);
				a.MaxSize = new Vector2(20);
				win.AddChild(a);
			}

			{
				var a = new UISolidColor();
				a.WindowColor = Color.PrettyPink;
				a.MinSize = new Vector2(20);
				a.MaxSize = new Vector2(20);
				win.AddChild(a);
			}

			UI.AddChild(win);
		}

		for (var i = 0; i < 2; i++)
		{
			// Do second pass with vertical layout.
			string? screenshotExtraText = null;
			if (i == 1)
			{
				UIBaseWindow list = UI.GetWindowById("test")!;
				list.LayoutMode = LayoutMode.VerticalList;
				list.FillY = true;
				list.FillX = true;
				list.ListSpacing = Vector2.Zero;
				screenshotExtraText = "+VerticalList";
			}

			yield return new TestWaiterRunLoops(1);
			VerifyScreenshot(screenshotExtraText);

			{
				UIBaseWindow list = UI.GetWindowById("test")!;
				list.FillX = false;
			}

			yield return new TestWaiterRunLoops(1);
			VerifyScreenshot(screenshotExtraText);

			{
				UIBaseWindow list = UI.GetWindowById("test")!;
				list.FillY = false;
				list.FillX = true;
			}

			yield return new TestWaiterRunLoops(1);
			VerifyScreenshot(screenshotExtraText);

			{
				UIBaseWindow list = UI.GetWindowById("test")!;
				list.FillY = false;
				list.FillX = false;
			}

			yield return new TestWaiterRunLoops(1);
			VerifyScreenshot(screenshotExtraText);

			{
				UIBaseWindow list = UI.GetWindowById("test")!;
				list.ListSpacing = new Vector2(5, 5);
			}

			yield return new TestWaiterRunLoops(1);
			VerifyScreenshot(screenshotExtraText);
		}
	}

	private IEnumerator FillListFillingItems()
	{
		UI.ClearChildren();

		{
			var win = new UISolidColor();
			win.WindowColor = Color.PrettyOrange;
			win.Id = "test";
			win.LayoutMode = LayoutMode.HorizontalList;

			{
				var a = new UISolidColor();
				a.WindowColor = Color.White;
				a.FillXInList = true;
				a.FillYInList = true;
				win.AddChild(a);
			}

			{
				var a = new UISolidColor();
				a.WindowColor = Color.PrettyPink;
				a.MinSize = new Vector2(20);
				a.MaxSize = new Vector2(20);
				win.AddChild(a);
			}

			{
				var a = new UISolidColor();
				a.WindowColor = Color.Black;
				a.FillXInList = true;
				a.FillYInList = true;
				win.AddChild(a);
			}

			UI.AddChild(win);
		}

		for (var i = 0; i < 2; i++)
		{
			// Do second pass with vertical layout.
			string? screenshotExtraText = null;
			if (i == 1)
			{
				UIBaseWindow list = UI.GetWindowById("test")!;
				list.LayoutMode = LayoutMode.VerticalList;
				list.FillY = true;
				list.FillX = true;
				list.ListSpacing = Vector2.Zero;
				screenshotExtraText = "+VerticalList";
			}

			yield return new TestWaiterRunLoops(1);
			VerifyScreenshot(screenshotExtraText);

			{
				UIBaseWindow list = UI.GetWindowById("test")!;
				list.FillX = false;
			}

			yield return new TestWaiterRunLoops(1);
			VerifyScreenshot(screenshotExtraText);

			{
				UIBaseWindow list = UI.GetWindowById("test")!;
				list.FillY = false;
				list.FillX = true;
			}

			yield return new TestWaiterRunLoops(1);
			VerifyScreenshot(screenshotExtraText);

			{
				UIBaseWindow list = UI.GetWindowById("test")!;
				list.FillY = false;
				list.FillX = false;
			}

			yield return new TestWaiterRunLoops(1);
			VerifyScreenshot(screenshotExtraText);

			{
				UIBaseWindow list = UI.GetWindowById("test")!;
				list.ListSpacing = new Vector2(5, 5);
			}

			yield return new TestWaiterRunLoops(1);
			VerifyScreenshot(screenshotExtraText);

			{
				UIBaseWindow list = UI.GetWindowById("test")!;
				list.FillY = true;
				list.FillX = true;
			}

			yield return new TestWaiterRunLoops(1);
			VerifyScreenshot(screenshotExtraText);
		}
	}
}
#endif