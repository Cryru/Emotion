#nullable enable

#region Using

using System;
using System.Collections;
using System.Numerics;
using System.Threading.Channels;
using System.Threading.Tasks;
using Emotion.Editor.EditorHelpers;
using Emotion.Game.Time.Routines;
using Emotion.Game.World.Editor;
using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.UI;
using Emotion.WIPUpdates.One.Tools;

#endregion

namespace Emotion.Testing.Scenarios;

public class NewUITests : TestingScene
{
    public UIController UI = null!;

    public override IEnumerator LoadSceneRoutineAsync()
    {
        UI = new UIController();
        UI.UseNewLayoutSystem = true;
        yield return base.LoadSceneRoutineAsync();
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

    private IEnumerator WaitUILayout()
    {
        UI.Update();
        yield return new TaskRoutineWaiter(UI.PreloadUI());
        UI.Update();
        yield return new TestWaiterRunLoops(1);
    }

    public override void BetweenEachTest()
    {
        base.BetweenEachTest();

        UI.ClearChildren();
    }

    [Test]
    public IEnumerator Fill()
    {
        {
            var win = new UISolidColor();
            win.WindowColor = Color.PrettyOrange;
            win.Id = "test";
            UI.AddChild(win);
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot();
    }

    [Test]
    public IEnumerator FillXAxisWithChild()
    {
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
        yield return VerifyScreenshot();
    }

    [Test]
    public IEnumerator FillXAxisMinHeight()
    {
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
        yield return VerifyScreenshot();
    }

    [Test]
    public IEnumerator FillXAxisMinHeightAndWidth()
    {
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
        yield return VerifyScreenshot();
    }

    [Test]
    public IEnumerator FillNeitherAxisMinHeightAndWidth()
    {
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
        yield return VerifyScreenshot();
    }

    [Test]
    public IEnumerator TwoSquaresInFillY()
    {
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
                a.AnchorAndParentAnchor = UIAnchor.BottomLeft;
                win.AddChild(a);
            }

            UI.AddChild(win);
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot();
    }

    [Test]
    public IEnumerator FillList()
    {
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
        yield return VerifyScreenshot();

        {
            UIBaseWindow list = UI.GetWindowById("test")!;
            list.LayoutMode = LayoutMode.VerticalList;
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot();
    }

    [Test]
    public IEnumerator FillListThreeItems()
    {
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
            yield return VerifyScreenshot(screenshotExtraText);

            {
                UIBaseWindow list = UI.GetWindowById("test")!;
                list.FillX = false;
            }

            yield return WaitUILayout();
            yield return VerifyScreenshot(screenshotExtraText);

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
            yield return VerifyScreenshot(screenshotExtraText);

            {
                UIBaseWindow list = UI.GetWindowById("test")!;
                list.ListSpacing = new Vector2(5, 5);
            }

            yield return WaitUILayout();
            yield return VerifyScreenshot(screenshotExtraText);
        }
    }

    [Test]
    public IEnumerator FillListFillingItems()
    {
        yield break;

        //{
        //    var win = new UISolidColor();
        //    win.WindowColor = Color.PrettyOrange;
        //    win.Id = "test";
        //    win.LayoutMode = LayoutMode.HorizontalList;

        //    {
        //        var a = new UISolidColor();
        //        a.WindowColor = Color.White;
        //        a.FillXInList = true;
        //        a.FillYInList = true;
        //        win.AddChild(a);
        //    }

        //    {
        //        var a = new UISolidColor();
        //        a.WindowColor = Color.PrettyPink;
        //        a.MinSize = new Vector2(20);
        //        a.MaxSize = new Vector2(20);
        //        win.AddChild(a);
        //    }

        //    {
        //        var a = new UISolidColor();
        //        a.WindowColor = Color.Black;
        //        a.FillXInList = true;
        //        a.FillYInList = true;
        //        win.AddChild(a);
        //    }

        //    UI.AddChild(win);
        //}

        //for (var i = 0; i < 2; i++)
        //{
        //    // Do second pass with vertical layout.
        //    string? screenshotExtraText = null;
        //    if (i == 1)
        //    {
        //        UIBaseWindow list = UI.GetWindowById("test")!;
        //        list.LayoutMode = LayoutMode.VerticalList;
        //        list.FillY = true;
        //        list.FillX = true;
        //        list.ListSpacing = Vector2.Zero;
        //        screenshotExtraText = "+VerticalList";
        //    }

        //    yield return WaitUILayout();
        //    VerifyScreenshot(screenshotExtraText);

        //    {
        //        UIBaseWindow list = UI.GetWindowById("test")!;
        //        list.FillX = false;
        //    }

        //    yield return WaitUILayout();
        //    VerifyScreenshot(screenshotExtraText);

        //    {
        //        UIBaseWindow list = UI.GetWindowById("test")!;
        //        list.FillY = false;
        //        list.FillX = true;
        //    }

        //    yield return WaitUILayout();
        //    VerifyScreenshot(screenshotExtraText);

        //    {
        //        UIBaseWindow list = UI.GetWindowById("test")!;
        //        list.FillY = false;
        //        list.FillX = false;
        //    }

        //    yield return WaitUILayout();
        //    VerifyScreenshot(screenshotExtraText);

        //    {
        //        UIBaseWindow list = UI.GetWindowById("test")!;
        //        list.ListSpacing = new Vector2(5, 5);
        //    }

        //    yield return WaitUILayout();
        //    VerifyScreenshot(screenshotExtraText);

        //    {
        //        UIBaseWindow list = UI.GetWindowById("test")!;
        //        list.FillY = true;
        //        list.FillX = true;
        //    }

        //    yield return WaitUILayout();
        //    VerifyScreenshot(screenshotExtraText);
        //}
    }

    [Test]
    public IEnumerator WorldEditorTopBar()
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

        {
            var win = new UISolidColor();
            win.MaxSizeY = 17;
            win.WindowColor = Color.PrettyOrange;
            win.Id = "top-parent";

            var list = new UISolidColor();
            list.LayoutMode = LayoutMode.HorizontalList;
            list.ListSpacing = new Vector2(3, 0);
            list.Margins = new Rectangle(3, 3, 3, 3);
            list.Id = "list";
            list.WindowColor = Color.PrettyGreen;
            list.FillY = false;
            win.AddChild(list);

            {
                var a = new UISolidColor();
                a.WindowColor = Color.Black;
                a.Paddings = new Rectangle(2, 1, 2, 1);
                a.Id = "text-bg";
                a.FillX = false;
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
        yield return VerifyScreenshot();

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
                a.FillX = false;
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
        yield return VerifyScreenshot();

        // Bar v2 (Possible only with the new UI)
        {
            UIBaseWindow list = UI.GetWindowById("list")!;

            list.Margins = new Rectangle(3, 0, 3, 0);
            list.AnchorAndParentAnchor = UIAnchor.CenterLeft;
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot();

        // Add text on the right
        {
            UIBaseWindow parent = UI.GetWindowById("top-parent")!;

            var a = new UIText();
            a.ParentAnchor = UIAnchor.CenterRight;
            a.Anchor = UIAnchor.CenterRight;
            a.WindowColor = Color.Black;
            a.Text = "Text on the right";
            a.FontSize = 6;
            a.Margins = new Rectangle(0, 0, 5, 0);
            a.TextHeightMode = Game.Text.GlyphHeightMeasurement.NoMinY;

            parent.AddChild(a);
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot();
    }

    [Test]
    public IEnumerator VerticalListWithText()
    {
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
                a.FillX = false;
                list.AddChild(a);

                var text = new UIText();
                text.FontSize = 9;
                text.Text = "Pink";
                text.WindowColor = Color.White;
                text.ScaleMode = UIScaleMode.FloatScale;
                text.Id = "text";
                text.ParentAnchor = UIAnchor.CenterLeft;
                text.Anchor = UIAnchor.CenterLeft;
                text.FillX = false;

                a.AddChild(text);
            }

            UI.AddChild(list);
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot();

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
                a.FillX = false;
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
        yield return VerifyScreenshot();

        {
            UIBaseWindow? list = UI.GetWindowById("list");
            Assert.NotNull(list);
            Assert.NotNull(list.Children);
            for (var i = 0; i < list.Children.Count; i++)
            {
                UIBaseWindow child = list.Children[i];
                child.FillX = true;
            }
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot();

        {
            UIBaseWindow? list = UI.GetWindowById("list");
            Assert.NotNull(list);
            list.Margins = new Rectangle(0, 0, 8, 0);
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot();
    }

    [Test]
    public IEnumerator TextWithBackground()
    {
        // This is a layout that is not possible in the old UI system.

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
        yield return VerifyScreenshot();
    }

    [Test]
    public IEnumerator EditorDropDownRelativeToAndOutsideParent()
    {
        {
            var bg = new UISolidColor
            {
                WindowColor = Color.PrettyPink,
                Paddings = new Rectangle(2, 1, 2, 1),
                Id = "AttachToMe",
                FillX = false,
                FillY = false
            };
            UI.AddChild(bg);

            var text = new UIText
            {
                FontSize = 9,
                Text = "Attach To Me!",
                WindowColor = Color.White,
                ScaleMode = UIScaleMode.FloatScale,

                ParentAnchor = UIAnchor.CenterLeft,
                Anchor = UIAnchor.CenterLeft
            };
            bg.AddChild(text);
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
                a.FillX = false;
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
        yield return VerifyScreenshot();
    }

    [Test]
    public IEnumerator WorldEditorBottomBar()
    {
        // This also tests margins on all sides.

        {
            var bottomBar = new UISolidColor();
            bottomBar.MaxSizeY = 12;
            bottomBar.FillY = false;
            bottomBar.WindowColor = Color.PrettyOrange;
            bottomBar.Id = "BottomBar";
            bottomBar.Anchor = UIAnchor.BottomLeft;
            bottomBar.ParentAnchor = UIAnchor.BottomLeft;

            var label = new UIText();
            label.Text = "No object selected";
            label.Margins = new Rectangle(3, 3, 3, 3);
            label.FontSize = 7;
            label.ParentAnchor = UIAnchor.CenterLeft;
            label.Anchor = UIAnchor.CenterLeft;
            label.Id = "Label";
            bottomBar.AddChild(label);

            UI.AddChild(bottomBar);
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot();
    }

    [Test]
    public IEnumerator EditorPanelEmpty()
    {
        int oldTextSize = MapEditorColorPalette.EditorButtonTextSize;
        MapEditorColorPalette.EditorButtonTextSize = 9;
        {
            var editorPanel = new EditorPanel("Test");
            UI.AddChild(editorPanel);
        }

        yield return WaitUILayout();
        VerifyScreenshot();

        MapEditorColorPalette.EditorButtonTextSize = oldTextSize;
    }

    [Test]
    public IEnumerator OutsideParentWindow()
    {
        var win = new UISolidColor
        {
            WindowColor = Color.PrettyRed,
            Anchor = UIAnchor.TopLeft,
            ParentAnchor = UIAnchor.CenterCenter,

            MinSize = new Vector2(20, 20),
            FillY = false,

            SetChildren = new()
            {
                new UISolidColor()
                {
                    WindowColor = Color.PrettyGreen,
                    MinSize = new Vector2(20),
                    Anchor = UIAnchor.TopRight,
                    ParentAnchor = UIAnchor.TopLeft,
                },

                new UISolidColor()
                {
                    WindowColor = Color.PrettyPurple,
                    MinSize = new Vector2(20),
                    Anchor = UIAnchor.TopLeft,
                    ParentAnchor = UIAnchor.BottomLeft
                },
            },
        };
        UI.AddChild(win);

        yield return WaitUILayout();
        VerifyScreenshot();

        win.Paddings = new Rectangle(5, 5, 5, 5);

        yield return WaitUILayout();
        VerifyScreenshot();

        win.Paddings = Rectangle.Empty;
        win.LayoutMode = LayoutMode.HorizontalList;

        var a = new UISolidColor()
        {
            WindowColor = Color.White,
            MinSize = new Vector2(20),
            FillX = false,
            FillY = false,
        };
        win.AddChild(a);

        var b = new UISolidColor()
        {
            WindowColor = Color.Black,
            MinSize = new Vector2(20),
            FillX = false,
            FillY = false,
        };
        win.AddChild(b);

        var c = new UISolidColor()
        {
            WindowColor = Color.White,
            MinSize = new Vector2(20),
            FillX = false,
            FillY = false,
        };
        win.AddChild(c);

        yield return WaitUILayout();
        VerifyScreenshot();

        win.Paddings = new Rectangle(5, 5, 5, 5);

        yield return WaitUILayout();
        VerifyScreenshot();
    }

    [Test]
    public IEnumerator BackgroundWindow()
    {
        var win = new UISolidColor
        {
            WindowColor = Color.PrettyRed,

            Paddings = new Rectangle(5, 5, 5, 5),
            SetChildren = new()
            {
                new UISolidColor()
                {
                    WindowColor = Color.White,
                    MinSize = new Vector2(20),
                    FillX = false,
                    FillY = false,
                },
                new UISolidColor()
                {
                    WindowColor = Color.Black,
                    MinSize = new Vector2(20),
                    FillX = false,
                    FillY = false,
                },
                new UISolidColor()
                {
                    WindowColor = Color.White,
                    MinSize = new Vector2(20),
                    FillX = false,
                    FillY = false,
                },
                new UISolidColor()
                {
                    WindowColor = Color.PrettyGreen,
                    BackgroundWindow = true
                },
            },
        };
        UI.AddChild(win);

        yield return WaitUILayout();
        VerifyScreenshot();

        win.LayoutMode = LayoutMode.HorizontalList;
        win.InvalidateLayout();

        yield return WaitUILayout();
        VerifyScreenshot();
    }

    //[DebugTest]
    [Test]
    public IEnumerator HorizontalPanelLayout()
    {
        var win = new UISolidColor
        {
            WindowColor = Color.PrettyRed,
            LayoutMode = LayoutMode.HorizontalEditorPanel,

            Id = "Panel",
            Paddings = new Rectangle(5, 5, 5, 5),
            SetChildren = new()
            {
                new UISolidColor()
                {
                    FillX = false,
                    MinSize = new Vector2(50),
                    WindowColor = Color.PrettyGreen,
                },
                new HorizontalPanelSeparator(),
                new UISolidColor()
                {
                    FillX = false,
                    MinSize = new Vector2(50),
                    WindowColor = Color.PrettyBlue,
                },
            }
        };
        UI.AddChild(win);

        yield return WaitUILayout();
        //VerifyScreenshot();

        yield return new TestWaiterRunLoops(-1);

        yield break;
    }
}