#nullable enable

#region Using

using System.Collections;
using System.Threading.Tasks;
using Emotion.Game.Time.Routines;
using Emotion.Graphics;
using Emotion.UI;

#endregion

namespace Emotion.Testing;

#if NEW_UI
public class NewUITests : TestingScene
{
	public UIController UI = null!;

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

			WorldEditorTopBar,
			VerticalListWithText,

			TextWithBackground,
			EditorDropDownRelativeToAndOutsideParent
		};
	}

	private IEnumerator WaitUILayout()
	{
		UI.Update();
		yield return new TaskRoutineWaiter(UI.PreloadUI());
		yield return new TestWaiterRunLoops(1);
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

		yield return WaitUILayout();
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

		yield return WaitUILayout();
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

		yield return WaitUILayout();
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

		yield return WaitUILayout();
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

		yield return WaitUILayout();
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

		yield return WaitUILayout();
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

		yield return WaitUILayout();
		VerifyScreenshot();

		{
			UIBaseWindow list = UI.GetWindowById("test")!;
			list.LayoutMode = LayoutMode.VerticalList;
		}

		yield return WaitUILayout();
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

			yield return WaitUILayout();
			VerifyScreenshot(screenshotExtraText);

			{
				UIBaseWindow list = UI.GetWindowById("test")!;
				list.FillX = false;
			}

			yield return WaitUILayout();
			VerifyScreenshot(screenshotExtraText);

			{
				UIBaseWindow list = UI.GetWindowById("test")!;
				list.FillY = false;
				list.FillX = true;
			}

			yield return WaitUILayout();
			VerifyScreenshot(screenshotExtraText);

			{
				UIBaseWindow list = UI.GetWindowById("test")!;
				list.FillY = false;
				list.FillX = false;
			}

			yield return WaitUILayout();
			VerifyScreenshot(screenshotExtraText);

			{
				UIBaseWindow list = UI.GetWindowById("test")!;
				list.ListSpacing = new Vector2(5, 5);
			}

			yield return WaitUILayout();
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

			yield return WaitUILayout();
			VerifyScreenshot(screenshotExtraText);

			{
				UIBaseWindow list = UI.GetWindowById("test")!;
				list.FillX = false;
			}

			yield return WaitUILayout();
			VerifyScreenshot(screenshotExtraText);

			{
				UIBaseWindow list = UI.GetWindowById("test")!;
				list.FillY = false;
				list.FillX = true;
			}

			yield return WaitUILayout();
			VerifyScreenshot(screenshotExtraText);

			{
				UIBaseWindow list = UI.GetWindowById("test")!;
				list.FillY = false;
				list.FillX = false;
			}

			yield return WaitUILayout();
			VerifyScreenshot(screenshotExtraText);

			{
				UIBaseWindow list = UI.GetWindowById("test")!;
				list.ListSpacing = new Vector2(5, 5);
			}

			yield return WaitUILayout();
			VerifyScreenshot(screenshotExtraText);

			{
				UIBaseWindow list = UI.GetWindowById("test")!;
				list.FillY = true;
				list.FillX = true;
			}

			yield return WaitUILayout();
			VerifyScreenshot(screenshotExtraText);
		}
	}

	private IEnumerator WorldEditorTopBar()
	{
		// This tests:
		// 1. Whether the world editor toolbar (first UI you see) layouts the same.
		// 2. list spacing
		// 3. margins in free layout
		// 4. anchors in free layout
		// 5. paddings in free layout
		// 6. UIText in a window in a list
		// 7. UIText in a window with paddings
		// 8. Whether children can expand their parents beyond grandparent's maxsize.

		UI.ClearChildren();

		{
			var win = new UISolidColor();
			win.MaxSizeY = 15;
			win.WindowColor = Color.PrettyOrange;
			win.Id = "test";

			var list = new UISolidColor();
			list.LayoutMode = LayoutMode.HorizontalList;
			list.ListSpacing = new Vector2(3, 0);
			list.Margins = new Rectangle(3, 3, 3, 3);
			list.Id = "list";
			list.WindowColor = Color.PrettyGreen;
			win.AddChild(list);

			{
				var a = new UISolidColor();
				a.WindowColor = Color.Black;
				a.Paddings = new Rectangle(2, 1, 2, 1);
				a.Id = "text-bg";
				list.AddChild(a);

				var text = new UIText();
				text.FontSize = 9;
				text.Text = "Black";
				text.WindowColor = Color.White;
				text.ScaleMode = UIScaleMode.FloatScale;
				text.Id = "text";
				text.ParentAnchor = UIAnchor.CenterLeft;
				text.Anchor = UIAnchor.CenterLeft;

				a.AddChild(text);
			}

			UI.AddChild(win);
		}

		yield return WaitUILayout();
		VerifyScreenshot();

		{
			UIBaseWindow list = UI.GetWindowById("list")!;

			{
				var a = new UISolidColor();
				a.WindowColor = Color.White;
				a.Paddings = new Rectangle(2, 1, 2, 1);
				//a.Id = "text-bg";
				list.AddChild(a);

				var text = new UIText();
				text.FontSize = 9;
				text.Text = "White";
				text.WindowColor = Color.Black;
				text.ScaleMode = UIScaleMode.FloatScale;
				text.Id = "text";
				text.ParentAnchor = UIAnchor.CenterLeft;
				text.Anchor = UIAnchor.CenterLeft;

				a.AddChild(text);
			}

			{
				var a = new UISolidColor();
				a.WindowColor = Color.PrettyPink;
				a.Paddings = new Rectangle(2, 1, 2, 1);
				//a.Id = "text-bg";
				list.AddChild(a);

				var text = new UIText();
				text.FontSize = 9;
				text.Text = "Pink";
				text.WindowColor = Color.White;
				text.ScaleMode = UIScaleMode.FloatScale;
				text.Id = "text";
				text.ParentAnchor = UIAnchor.CenterLeft;
				text.Anchor = UIAnchor.CenterLeft;

				a.AddChild(text);
			}
		}

		yield return WaitUILayout();
		VerifyScreenshot();

		// Bar v2 (Possible only with the new UI)
		{
			UIBaseWindow list = UI.GetWindowById("list")!;

			list.Margins = new Rectangle(3, 0, 3, 0);
			list.AlignAnchor = UIAnchor.CenterLeft;
		}

		yield return WaitUILayout();
		VerifyScreenshot();
	}

	private IEnumerator VerticalListWithText()
	{
		UI.ClearChildren();

		{
			var list = new UISolidColor
			{
				WindowColor = Color.PrettyOrange,
				FillX = false,
				FillY = false,
				LayoutMode = LayoutMode.VerticalList
			};

			{
				var a = new UISolidColor();
				a.WindowColor = Color.Black;
				a.Paddings = new Rectangle(2, 1, 2, 1);
				a.Id = "text-bg";
				list.AddChild(a);

				var text = new UIText();
				text.FontSize = 9;
				text.Text = "Black";
				text.WindowColor = Color.White;
				text.ScaleMode = UIScaleMode.FloatScale;
				text.Id = "text";
				text.ParentAnchor = UIAnchor.CenterLeft;
				text.Anchor = UIAnchor.CenterLeft;

				a.AddChild(text);
			}

			{
				var a = new UISolidColor();
				a.WindowColor = Color.White;
				a.Paddings = new Rectangle(2, 1, 2, 1);
				//a.Id = "text-bg";
				list.AddChild(a);

				var text = new UIText();
				text.FontSize = 9;
				text.Text = "White";
				text.WindowColor = Color.Black;
				text.ScaleMode = UIScaleMode.FloatScale;
				text.Id = "text";
				text.ParentAnchor = UIAnchor.CenterLeft;
				text.Anchor = UIAnchor.CenterLeft;

				a.AddChild(text);
			}

			{
				var a = new UISolidColor();
				a.WindowColor = Color.PrettyPink;
				a.Paddings = new Rectangle(2, 1, 2, 1);
				//a.Id = "text-bg";
				list.AddChild(a);

				var text = new UIText();
				text.FontSize = 9;
				text.Text = "Pink";
				text.WindowColor = Color.White;
				text.ScaleMode = UIScaleMode.FloatScale;
				text.Id = "text";
				text.ParentAnchor = UIAnchor.CenterLeft;
				text.Anchor = UIAnchor.CenterLeft;

				a.AddChild(text);
			}

			UI.AddChild(list);
		}

		yield return WaitUILayout();
		VerifyScreenshot();

		UI.ClearChildren();

		// Prototype of the editor dropdown.
		// This tests:
		// 1. paddings on all sides of a fill
		// 2. Vertical UIList
		// 3. FillInListX

		{
			var dropDown = new UISolidColor
			{
				WindowColor = Color.PrettyOrange,
				FillX = false,
				FillY = false
			};

			var innerBg = new UISolidColor
			{
				IgnoreParentColor = true,
				WindowColor = Color.PrettyGreen,
				Paddings = new Rectangle(3, 3, 3, 3),
			};
			dropDown.AddChild(innerBg);

			var list = new UIList
			{
				Id = "list",
				LayoutMode = LayoutMode.VerticalList,
				ListSpacing = new Vector2(0, 2),
			};
			innerBg.AddChild(list);

			for (var i = 0; i < 5; i++)
			{
				var a = new UISolidColor();
				a.WindowColor = Color.Black;
				a.Paddings = new Rectangle(2, 1, 2, 1);
				//a.Id = "text-bg";
				list.AddChild(a);

				var text = new UIText();
				text.FontSize = 9;
				text.Text = "Black " + new string('A', i);
				text.WindowColor = Color.White;
				text.ScaleMode = UIScaleMode.FloatScale;
				text.Id = "text";
				text.ParentAnchor = UIAnchor.CenterLeft;
				text.Anchor = UIAnchor.CenterLeft;

				a.AddChild(text);
			}

			UI.AddChild(dropDown);
		}

		yield return WaitUILayout();
		VerifyScreenshot();

		{
			UIBaseWindow? list = UI.GetWindowById("list");
			AssertNotNull(list);
			AssertNotNull(list.Children);
			for (var i = 0; i < list.Children.Count; i++)
			{
				UIBaseWindow child = list.Children[i];
				child.FillXInList = true;
			}
		}

		yield return WaitUILayout();
		VerifyScreenshot();

		{
			UIBaseWindow? list = UI.GetWindowById("list");
			AssertNotNull(list);
			list.Margins = new Rectangle(0, 0, 8, 0);
		}

		yield return WaitUILayout();
		VerifyScreenshot();
	}

	private IEnumerator TextWithBackground()
	{
		// This is a layout that is not possible in the old UI system.

		UI.ClearChildren();

		{
			var container = new UIBaseWindow();
			container.FillX = false;
			container.FillY = false;

			var bg = new UISolidColor();
			bg.WindowColor = Color.Red * 0.5f;
			container.AddChild(bg);

			var textc = new UIText();
			textc.Text = "Hello ladies and gentlemen, and welcome to the show!";
			container.AddChild(textc);

			UI.AddChild(container);
		}

		yield return WaitUILayout();
		VerifyScreenshot();
	}

	private IEnumerator EditorDropDownRelativeToAndOutsideParent()
	{
		UI.ClearChildren();

		{
			var a = new UISolidColor();
			a.WindowColor = Color.PrettyPink;
			a.Paddings = new Rectangle(2, 1, 2, 1);
			a.Id = "AttachToMe";
			a.FillX = false;
			a.FillY = false;
			UI.AddChild(a);

			var text = new UIText();
			text.FontSize = 9;
			text.Text = "Attach To Me!";
			text.WindowColor = Color.White;
			text.ScaleMode = UIScaleMode.FloatScale;
			
			text.ParentAnchor = UIAnchor.CenterLeft;
			text.Anchor = UIAnchor.CenterLeft;

			a.AddChild(text);
		}
		
		{
			var dropDown = new UISolidColor
			{
				WindowColor = Color.PrettyOrange,
				FillX = false,
				FillY = false,
				RelativeTo = "AttachToMe",
				ParentAnchor = UIAnchor.BottomLeft,
				Anchor = UIAnchor.TopLeft
			};

			var innerBg = new UISolidColor
			{
				IgnoreParentColor = true,
				WindowColor = Color.PrettyGreen,
				Paddings = new Rectangle(3, 3, 3, 3),
			};
			dropDown.AddChild(innerBg);

			var list = new UIList
			{
				Id = "list",
				LayoutMode = LayoutMode.VerticalList,
				ListSpacing = new Vector2(0, 2),
			};
			innerBg.AddChild(list);

			for (var i = 0; i < 5; i++)
			{
				var a = new UISolidColor();
				a.WindowColor = Color.Black;
				a.Paddings = new Rectangle(2, 1, 2, 1);
				//a.Id = "text-bg";
				list.AddChild(a);

				var text = new UIText();
				text.FontSize = 9;
				text.Text = "Black " + new string('A', i);
				text.WindowColor = Color.White;
				text.ScaleMode = UIScaleMode.FloatScale;
				text.Id = "text";
				text.ParentAnchor = UIAnchor.CenterLeft;
				text.Anchor = UIAnchor.CenterLeft;

				a.AddChild(text);
			}

			UI.AddChild(dropDown);
		}

		yield return WaitUILayout();
		VerifyScreenshot();
	}
}
#endif