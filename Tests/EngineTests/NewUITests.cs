#nullable enable

#region Using

using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Emotion.Core;
using Emotion.Core.Utility.Coroutines;
using Emotion.Editor;
using Emotion.Editor.EditorUI.Components;
using Emotion.Game.Systems.UI;
using Emotion.Game.Systems.UI.Text;
using Emotion.Game.Systems.UI.Text.TextUpdate;
using Emotion.Game.Systems.UI2;
using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.Testing;

#endregion

namespace Tests.EngineTests;

[DebugTest]
public class NewUITests : TestingScene
{
    protected override IEnumerator InternalLoadSceneRoutineAsync()
    {
        Engine.Configuration.UpdateUIAutomatically = false;
        yield break;
    }

    public override IEnumerator UnloadSceneRoutineAsync()
    {
        Engine.Configuration.UpdateUIAutomatically = true;
        return base.UnloadSceneRoutineAsync();
    }

    protected override void TestUpdate()
    {
        Engine.UI.Update();
    }

    protected override void TestDraw(Renderer c)
    {
        c.SetUseViewMatrix(false);
        Engine.UI.Render(c);
    }

    private IEnumerator WaitUILayout()
    {
        //UI.Update();
        //yield return new TaskRoutineWaiter(UI.PreloadUI());
        //UI.Update();
        yield return new TestWaiterRunLoops(1);
    }

    public override void BetweenEachTest()
    {
        base.BetweenEachTest();
        SceneUI.ClearChildren();
    }

    [Test]
    public IEnumerator Fill()
    {
        var win = new UIBaseWindow
        {
            Name = "test",
            Visuals =
            {
                BackgroundColor = Color.PrettyOrange
            }
        };
        SceneUI.AddChildAsync(win);

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITests), nameof(Fill));
    }

    [Test]
    public IEnumerator FillXAxisWithChild()
    {
        var win = new UIBaseWindow
        {
            Visuals =
            {
                BackgroundColor = Color.PrettyOrange,
            },
            Layout =
            {
                SizingY = UISizing.Fit()
            }
        };

        {
            var child = new UIBaseWindow
            {
                Visuals =
                {
                    BackgroundColor = Color.White
                }
            };
            win.AddChild(child);
        }

        SceneUI.AddChildAsync(win);

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITests), nameof(FillXAxisWithChild));
    }

    [Test]
    public IEnumerator FillXAxisMinHeight()
    {
        var win = new UIBaseWindow
        {
            Visuals =
            {
                BackgroundColor = Color.PrettyOrange,
            },
            Layout =
            {
                SizingY = UISizing.Fit()
            }
        };

        {
            var child = new UIBaseWindow
            {
                Visuals =
                {
                    BackgroundColor = Color.White
                },
                Layout =
                {
                    SizingY = UISizing.Fixed(20)
                }
            };
            win.AddChild(child);
        }

        SceneUI.AddChildAsync(win);

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITests), nameof(FillXAxisMinHeight));
    }

    //[Test]
    //public IEnumerator FillXAxisMinHeightAndWidth()
    //{
    //    {
    //        var win = new UISolidColor();
    //        win.WindowColor = Color.PrettyOrange;
    //        win.GrowY = false;
    //        win.Id = "test";

    //        {
    //            var a = new UISolidColor();
    //            a.WindowColor = Color.White;
    //            a.MinSizeY = 20;
    //            a.MinSizeX = 20;
    //            win.AddChild(a);
    //        }

    //        UI.AddChild(win);
    //    }

    //    yield return WaitUILayout();
    //    yield return VerifyScreenshot(nameof(NewUITests), nameof(FillXAxisMinHeightAndWidth));
    //}

    //[Test]
    //public IEnumerator FillNeitherAxisMinHeightAndWidth()
    //{
    //    {
    //        var win = new UISolidColor();
    //        win.WindowColor = Color.PrettyOrange;
    //        win.GrowY = false;
    //        win.GrowX = false;
    //        win.Id = "test";

    //        {
    //            var a = new UISolidColor();
    //            a.WindowColor = Color.White;
    //            a.MinSizeY = 20;
    //            a.MinSizeX = 20;
    //            win.AddChild(a);
    //        }

    //        UI.AddChild(win);
    //    }

    //    yield return WaitUILayout();
    //    yield return VerifyScreenshot(nameof(NewUITests), nameof(FillNeitherAxisMinHeightAndWidth));
    //}

    //[Test]
    //public IEnumerator TwoSquaresInFillY()
    //{
    //    {
    //        var win = new UISolidColor();
    //        win.WindowColor = Color.PrettyOrange;
    //        win.GrowX = false;
    //        win.Id = "test";

    //        {
    //            var a = new UISolidColor();
    //            a.WindowColor = Color.White;
    //            a.MinSize = new Vector2(20);
    //            a.MaxSize = new Vector2(20);
    //            win.AddChild(a);
    //        }

    //        {
    //            var a = new UISolidColor();
    //            a.WindowColor = Color.Black;
    //            a.MinSize = new Vector2(20);
    //            a.MaxSize = new Vector2(20);
    //            a.AnchorAndParentAnchor = UIAnchor.BottomLeft;
    //            win.AddChild(a);
    //        }

    //        UI.AddChild(win);
    //    }

    //    yield return WaitUILayout();
    //    yield return VerifyScreenshot(nameof(NewUITests), nameof(TwoSquaresInFillY));
    //}

    //[Test]
    //public IEnumerator FillList()
    //{
    //    {
    //        var win = new UISolidColor();
    //        win.WindowColor = Color.PrettyOrange;
    //        win.Id = "test";
    //        win.LayoutMode = LayoutMode.HorizontalList;

    //        {
    //            var a = new UISolidColor();
    //            a.WindowColor = Color.White;
    //            win.AddChild(a);
    //        }

    //        UI.AddChild(win);
    //    }

    //    yield return WaitUILayout();
    //    yield return VerifyScreenshot(nameof(NewUITests), nameof(FillList));

    //    {
    //        UIBaseWindow list = UI.GetWindowById("test")!;
    //        list.LayoutMode = LayoutMode.VerticalList;
    //    }

    //    yield return WaitUILayout();
    //    yield return VerifyScreenshot(nameof(NewUITests), nameof(FillList));
    //}

    //[Test]
    //public IEnumerator FillListThreeItems()
    //{
    //    {
    //        var win = new UISolidColor();
    //        win.WindowColor = Color.PrettyOrange;
    //        win.Id = "test";
    //        win.LayoutMode = LayoutMode.HorizontalList;

    //        {
    //            var a = new UISolidColor();
    //            a.WindowColor = Color.White;
    //            a.MinSize = new Vector2(20);
    //            a.MaxSize = new Vector2(20);
    //            win.AddChild(a);
    //        }

    //        {
    //            var a = new UISolidColor();
    //            a.WindowColor = Color.Black;
    //            a.MinSize = new Vector2(20);
    //            a.MaxSize = new Vector2(20);
    //            win.AddChild(a);
    //        }

    //        {
    //            var a = new UISolidColor();
    //            a.WindowColor = Color.PrettyPink;
    //            a.MinSize = new Vector2(20);
    //            a.MaxSize = new Vector2(20);
    //            win.AddChild(a);
    //        }

    //        UI.AddChild(win);
    //    }

    //    for (var i = 0; i < 2; i++)
    //    {
    //        // Do second pass with vertical layout.
    //        string? screenshotExtraText = null;
    //        if (i == 1)
    //        {
    //            UIBaseWindow list = UI.GetWindowById("test")!;
    //            list.LayoutMode = LayoutMode.VerticalList;
    //            list.GrowY = true;
    //            list.GrowX = true;
    //            list.ListSpacing = Vector2.Zero;
    //            screenshotExtraText = "+VerticalList";
    //        }

    //        yield return WaitUILayout();
    //        yield return VerifyScreenshot(nameof(NewUITests), nameof(FillListThreeItems), screenshotExtraText);

    //        {
    //            UIBaseWindow list = UI.GetWindowById("test")!;
    //            list.GrowX = false;
    //        }

    //        yield return WaitUILayout();
    //        yield return VerifyScreenshot(nameof(NewUITests), nameof(FillListThreeItems), screenshotExtraText);

    //        {
    //            UIBaseWindow list = UI.GetWindowById("test")!;
    //            list.GrowY = false;
    //            list.GrowX = true;
    //        }

    //        yield return WaitUILayout();
    //        VerifyScreenshot(nameof(NewUITests), nameof(FillListThreeItems), screenshotExtraText);

    //        {
    //            UIBaseWindow list = UI.GetWindowById("test")!;
    //            list.GrowY = false;
    //            list.GrowX = false;
    //        }

    //        yield return WaitUILayout();
    //        yield return VerifyScreenshot(nameof(NewUITests), nameof(FillListThreeItems), screenshotExtraText);

    //        {
    //            UIBaseWindow list = UI.GetWindowById("test")!;
    //            list.ListSpacing = new Vector2(5, 5);
    //        }

    //        yield return WaitUILayout();
    //        yield return VerifyScreenshot(nameof(NewUITests), nameof(FillListThreeItems), screenshotExtraText);
    //    }
    //}

    //[Test]
    //public IEnumerator FillListFillingItems()
    //{
    //    yield break;

    //    //{
    //    //    var win = new UISolidColor();
    //    //    win.WindowColor = Color.PrettyOrange;
    //    //    win.Id = "test";
    //    //    win.LayoutMode = LayoutMode.HorizontalList;

    //    //    {
    //    //        var a = new UISolidColor();
    //    //        a.WindowColor = Color.White;
    //    //        a.FillXInList = true;
    //    //        a.FillYInList = true;
    //    //        win.AddChild(a);
    //    //    }

    //    //    {
    //    //        var a = new UISolidColor();
    //    //        a.WindowColor = Color.PrettyPink;
    //    //        a.MinSize = new Vector2(20);
    //    //        a.MaxSize = new Vector2(20);
    //    //        win.AddChild(a);
    //    //    }

    //    //    {
    //    //        var a = new UISolidColor();
    //    //        a.WindowColor = Color.Black;
    //    //        a.FillXInList = true;
    //    //        a.FillYInList = true;
    //    //        win.AddChild(a);
    //    //    }

    //    //    UI.AddChild(win);
    //    //}

    //    //for (var i = 0; i < 2; i++)
    //    //{
    //    //    // Do second pass with vertical layout.
    //    //    string? screenshotExtraText = null;
    //    //    if (i == 1)
    //    //    {
    //    //        UIBaseWindow list = UI.GetWindowById("test")!;
    //    //        list.LayoutMode = LayoutMode.VerticalList;
    //    //        list.FillY = true;
    //    //        list.FillX = true;
    //    //        list.ListSpacing = Vector2.Zero;
    //    //        screenshotExtraText = "+VerticalList";
    //    //    }

    //    //    yield return WaitUILayout();
    //    //    VerifyScreenshot(screenshotExtraText);

    //    //    {
    //    //        UIBaseWindow list = UI.GetWindowById("test")!;
    //    //        list.FillX = false;
    //    //    }

    //    //    yield return WaitUILayout();
    //    //    VerifyScreenshot(screenshotExtraText);

    //    //    {
    //    //        UIBaseWindow list = UI.GetWindowById("test")!;
    //    //        list.FillY = false;
    //    //        list.FillX = true;
    //    //    }

    //    //    yield return WaitUILayout();
    //    //    VerifyScreenshot(screenshotExtraText);

    //    //    {
    //    //        UIBaseWindow list = UI.GetWindowById("test")!;
    //    //        list.FillY = false;
    //    //        list.FillX = false;
    //    //    }

    //    //    yield return WaitUILayout();
    //    //    VerifyScreenshot(screenshotExtraText);

    //    //    {
    //    //        UIBaseWindow list = UI.GetWindowById("test")!;
    //    //        list.ListSpacing = new Vector2(5, 5);
    //    //    }

    //    //    yield return WaitUILayout();
    //    //    VerifyScreenshot(screenshotExtraText);

    //    //    {
    //    //        UIBaseWindow list = UI.GetWindowById("test")!;
    //    //        list.FillY = true;
    //    //        list.FillX = true;
    //    //    }

    //    //    yield return WaitUILayout();
    //    //    VerifyScreenshot(screenshotExtraText);
    //    //}
    //}

    //[Test]
    //public IEnumerator WorldEditorTopBar()
    //{
    //    // This tests:
    //    // 1. Whether the world editor toolbar (first UI you see) layouts the same.
    //    // 2. list spacing
    //    // 3. margins in free layout
    //    // 4. anchors in free layout
    //    // 5. paddings in free layout
    //    // 6. UIText in a window in a list
    //    // 7. UIText in a window with paddings
    //    // 8. Whether children can expand their parents beyond grandparent's maxsize.

    //    {
    //        var win = new UIBaseWindow()
    //        {
    //            Name = "top-parent",
    //            Layout =
    //            {
    //                SizingY = UISizing.Fixed(17)
    //            },
    //            Visuals =
    //            {
    //                BackgroundColor = Color.PrettyOrange
    //            },
    //            Children =
    //            {
    //                new UIBaseWindow
    //                {
    //                    LayoutMode = LayoutMode.HorizontalList,
    //                    ListSpacing = new Vector2(3, 0),
    //                    Margins = new Rectangle(3, 3, 3, 3),
    //                    Name = "list",
    //                    WindowColor = Color.PrettyGreen,
    //                    GrowY = false,

    //                    Children =
    //                    {
    //                        new UIBaseWindow
    //                        {
    //                            Name = "text-bg",
    //                            Visuals =
    //                            {
    //                                BackgroundColor = Color.Black,
    //                            },
    //                            Layout =
    //                            {
    //                                Padding = new UISpacing(2, 1, 2, 1),
    //                                SizingX = UISizing.Fit()
    //                            },
    //                            Children =
    //                            {
    //                                new NewUIText
    //                                {
    //                                    Name = "text",
    //                                    FontSize = 9,
    //                                    Text = "Black",
    //                                    TextColor = Color.White,
    //                                    Layout =
    //                                    {
    //                                        AnchorAndParentAnchor = UIAnchor.CenterLeft
    //                                    }
    //                                }
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        };

    //        UI.AddChild(win);
    //    }

    //    yield return WaitUILayout();
    //    yield return VerifyScreenshot(nameof(NewUITests), nameof(WorldEditorTopBar));

    //    {
    //        UIBaseWindow list = UI.GetWindowById("list")!;

    //        {
    //            var a = new UISolidColor();
    //            a.WindowColor = Color.White;
    //            a.Paddings = new Rectangle(2, 1, 2, 1);
    //            //a.Id = "text-bg";
    //            list.AddChild(a);

    //            var text = new UIText();
    //            text.FontSize = 9;
    //            text.Text = "White";
    //            text.WindowColor = Color.Black;
    //            text.ScaleMode = UIScaleMode.FloatScale;
    //            text.Id = "text";
    //            text.ParentAnchor = UIAnchor.CenterLeft;
    //            text.Anchor = UIAnchor.CenterLeft;

    //            a.AddChild(text);
    //        }

    //        {
    //            var a = new UISolidColor();
    //            a.WindowColor = Color.PrettyPink;
    //            a.Paddings = new Rectangle(2, 1, 2, 1);
    //            //a.Id = "text-bg";
    //            a.GrowX = false;
    //            list.AddChild(a);

    //            var text = new UIText();
    //            text.FontSize = 9;
    //            text.Text = "Pink";
    //            text.WindowColor = Color.White;
    //            text.ScaleMode = UIScaleMode.FloatScale;
    //            text.Id = "text";
    //            text.ParentAnchor = UIAnchor.CenterLeft;
    //            text.Anchor = UIAnchor.CenterLeft;

    //            a.AddChild(text);
    //        }
    //    }

    //    yield return WaitUILayout();
    //    yield return VerifyScreenshot(nameof(NewUITests), nameof(WorldEditorTopBar));

    //    // Bar v2 (Possible only with the new UI)
    //    {
    //        UIBaseWindow list = UI.GetWindowById("list")!;

    //        list.Margins = new Rectangle(3, 0, 3, 0);
    //        list.AnchorAndParentAnchor = UIAnchor.CenterLeft;
    //    }

    //    yield return WaitUILayout();
    //    yield return VerifyScreenshot(nameof(NewUITests), nameof(WorldEditorTopBar));

    //    // Add text on the right
    //    {
    //        UIBaseWindow parent = UI.GetWindowById("top-parent")!;

    //        var a = new UIText();
    //        a.ParentAnchor = UIAnchor.CenterRight;
    //        a.Anchor = UIAnchor.CenterRight;
    //        a.WindowColor = Color.Black;
    //        a.Text = "Text on the right";
    //        a.FontSize = 6;
    //        a.Margins = new Rectangle(0, 0, 5, 0);
    //        a.TextHeightMode = GlyphHeightMeasurement.NoMinY;

    //        parent.AddChild(a);
    //    }

    //    yield return WaitUILayout();
    //    yield return VerifyScreenshot(nameof(NewUITests), nameof(WorldEditorTopBar));
    //}

    //[Test]
    //public IEnumerator VerticalListWithText()
    //{
    //    {
    //        var list = new UISolidColor
    //        {
    //            WindowColor = Color.PrettyOrange,
    //            GrowX = false,
    //            GrowY = false,
    //            LayoutMode = LayoutMode.VerticalList
    //        };

    //        {
    //            var a = new UISolidColor();
    //            a.WindowColor = Color.Black;
    //            a.Paddings = new Rectangle(2, 1, 2, 1);
    //            a.Id = "text-bg";
    //            list.AddChild(a);

    //            var text = new UIText();
    //            text.FontSize = 9;
    //            text.Text = "Black";
    //            text.WindowColor = Color.White;
    //            text.ScaleMode = UIScaleMode.FloatScale;
    //            text.Id = "text";
    //            text.ParentAnchor = UIAnchor.CenterLeft;
    //            text.Anchor = UIAnchor.CenterLeft;

    //            a.AddChild(text);
    //        }

    //        {
    //            var a = new UISolidColor();
    //            a.WindowColor = Color.White;
    //            a.Paddings = new Rectangle(2, 1, 2, 1);
    //            //a.Id = "text-bg";
    //            list.AddChild(a);

    //            var text = new UIText();
    //            text.FontSize = 9;
    //            text.Text = "White";
    //            text.WindowColor = Color.Black;
    //            text.ScaleMode = UIScaleMode.FloatScale;
    //            text.Id = "text";
    //            text.ParentAnchor = UIAnchor.CenterLeft;
    //            text.Anchor = UIAnchor.CenterLeft;

    //            a.AddChild(text);
    //        }

    //        {
    //            var a = new UISolidColor();
    //            a.WindowColor = Color.PrettyPink;
    //            a.Paddings = new Rectangle(2, 1, 2, 1);
    //            //a.Id = "text-bg";
    //            a.GrowX = false;
    //            list.AddChild(a);

    //            var text = new UIText();
    //            text.FontSize = 9;
    //            text.Text = "Pink";
    //            text.WindowColor = Color.White;
    //            text.ScaleMode = UIScaleMode.FloatScale;
    //            text.Id = "text";
    //            text.ParentAnchor = UIAnchor.CenterLeft;
    //            text.Anchor = UIAnchor.CenterLeft;
    //            text.GrowX = false;

    //            a.AddChild(text);
    //        }

    //        UI.AddChild(list);
    //    }

    //    yield return WaitUILayout();
    //    yield return VerifyScreenshot(nameof(NewUITests), nameof(VerticalListWithText));

    //    UI.ClearChildren();

    //    // Prototype of the editor dropdown.
    //    // This tests:
    //    // 1. paddings on all sides of a fill
    //    // 2. Vertical UIList
    //    // 3. FillInListX

    //    {
    //        var dropDown = new UISolidColor
    //        {
    //            WindowColor = Color.PrettyOrange,
    //            GrowX = false,
    //            GrowY = false
    //        };

    //        var innerBg = new UISolidColor
    //        {
    //            IgnoreParentColor = true,
    //            WindowColor = Color.PrettyGreen,
    //            Paddings = new Rectangle(3, 3, 3, 3),
    //        };
    //        dropDown.AddChild(innerBg);

    //        var list = new UIList
    //        {
    //            Id = "list",
    //            LayoutMode = LayoutMode.VerticalList,
    //            ListSpacing = new Vector2(0, 2),
    //        };
    //        innerBg.AddChild(list);

    //        for (var i = 0; i < 5; i++)
    //        {
    //            var a = new UISolidColor();
    //            a.WindowColor = Color.Black;
    //            a.Paddings = new Rectangle(2, 1, 2, 1);
    //            a.GrowX = false;
    //            //a.Id = "text-bg";
    //            list.AddChild(a);

    //            var text = new UIText();
    //            text.FontSize = 9;
    //            text.Text = "Black " + new string('A', i);
    //            text.WindowColor = Color.White;
    //            text.ScaleMode = UIScaleMode.FloatScale;
    //            text.Id = "text";
    //            text.ParentAnchor = UIAnchor.CenterLeft;
    //            text.Anchor = UIAnchor.CenterLeft;

    //            a.AddChild(text);
    //        }

    //        UI.AddChild(dropDown);
    //    }

    //    yield return WaitUILayout();
    //    yield return VerifyScreenshot(nameof(NewUITests), nameof(VerticalListWithText));

    //    {
    //        UIBaseWindow? list = UI.GetWindowById("list");
    //        Assert.NotNull(list);
    //        Assert.NotNull(list.Children);
    //        for (var i = 0; i < list.Children.Count; i++)
    //        {
    //            UIBaseWindow child = list.Children[i];
    //            child.GrowX = true;
    //        }
    //    }

    //    yield return WaitUILayout();
    //    yield return VerifyScreenshot(nameof(NewUITests), nameof(VerticalListWithText));

    //    {
    //        UIBaseWindow? list = UI.GetWindowById("list");
    //        Assert.NotNull(list);
    //        list.Margins = new Rectangle(0, 0, 8, 0);
    //    }

    //    yield return WaitUILayout();
    //    yield return VerifyScreenshot(nameof(NewUITests), nameof(VerticalListWithText));
    //}

    //[Test]
    //public IEnumerator TextWithBackground()
    //{
    //    // This is a layout that is not possible in the old UI system.

    //    {
    //        var container = new UIBaseWindow();
    //        container.GrowX = false;
    //        container.GrowY = false;

    //        var bg = new UISolidColor();
    //        bg.WindowColor = Color.Red * 0.5f;
    //        container.AddChild(bg);

    //        var textc = new UIText();
    //        textc.Text = "Hello ladies and gentlemen, and welcome to the show!";
    //        container.AddChild(textc);

    //        UI.AddChild(container);
    //    }

    //    yield return WaitUILayout();
    //    yield return VerifyScreenshot(nameof(NewUITests), nameof(TextWithBackground));
    //}

    //[Test]
    //public IEnumerator EditorDropDownRelativeToAndOutsideParent()
    //{
    //    {
    //        var bg = new UISolidColor
    //        {
    //            WindowColor = Color.PrettyPink,
    //            Paddings = new Rectangle(2, 1, 2, 1),
    //            Id = "AttachToMe",
    //            GrowX = false,
    //            GrowY = false
    //        };
    //        UI.AddChild(bg);

    //        var text = new UIText
    //        {
    //            FontSize = 9,
    //            Text = "Attach To Me!",
    //            WindowColor = Color.White,
    //            ScaleMode = UIScaleMode.FloatScale,

    //            ParentAnchor = UIAnchor.CenterLeft,
    //            Anchor = UIAnchor.CenterLeft
    //        };
    //        bg.AddChild(text);
    //    }

    //    {
    //        var dropDown = new UISolidColor
    //        {
    //            WindowColor = Color.PrettyOrange,
    //            GrowX = false,
    //            GrowY = false,
    //            RelativeTo = "AttachToMe",
    //            ParentAnchor = UIAnchor.BottomLeft,
    //            Anchor = UIAnchor.TopLeft
    //        };

    //        var innerBg = new UISolidColor
    //        {
    //            IgnoreParentColor = true,
    //            WindowColor = Color.PrettyGreen,
    //            Paddings = new Rectangle(3, 3, 3, 3),
    //        };
    //        dropDown.AddChild(innerBg);

    //        var list = new UIList
    //        {
    //            Id = "list",
    //            LayoutMode = LayoutMode.VerticalList,
    //            ListSpacing = new Vector2(0, 2),
    //        };
    //        innerBg.AddChild(list);

    //        for (var i = 0; i < 5; i++)
    //        {
    //            var a = new UISolidColor();
    //            a.WindowColor = Color.Black;
    //            a.Paddings = new Rectangle(2, 1, 2, 1);
    //            a.GrowX = false;
    //            //a.Id = "text-bg";
    //            list.AddChild(a);

    //            var text = new UIText();
    //            text.FontSize = 9;
    //            text.Text = "Black " + new string('A', i);
    //            text.WindowColor = Color.White;
    //            text.ScaleMode = UIScaleMode.FloatScale;
    //            text.Id = "text";
    //            text.ParentAnchor = UIAnchor.CenterLeft;
    //            text.Anchor = UIAnchor.CenterLeft;

    //            a.AddChild(text);
    //        }

    //        UI.AddChild(dropDown);
    //    }

    //    yield return WaitUILayout();
    //    yield return VerifyScreenshot(nameof(NewUITests), nameof(EditorDropDownRelativeToAndOutsideParent));
    //}

    //[Test]
    //public IEnumerator WorldEditorBottomBar()
    //{
    //    // This also tests margins on all sides.

    //    {
    //        var bottomBar = new UISolidColor();
    //        bottomBar.MaxSizeY = 12;
    //        bottomBar.GrowY = false;
    //        bottomBar.WindowColor = Color.PrettyOrange;
    //        bottomBar.Id = "BottomBar";
    //        bottomBar.Anchor = UIAnchor.BottomLeft;
    //        bottomBar.ParentAnchor = UIAnchor.BottomLeft;

    //        var label = new UIText();
    //        label.Text = "No object selected";
    //        label.Margins = new Rectangle(3, 3, 3, 3);
    //        label.FontSize = 7;
    //        label.ParentAnchor = UIAnchor.CenterLeft;
    //        label.Anchor = UIAnchor.CenterLeft;
    //        label.Id = "Label";
    //        bottomBar.AddChild(label);

    //        UI.AddChild(bottomBar);
    //    }

    //    yield return WaitUILayout();
    //    yield return VerifyScreenshot(nameof(NewUITests), nameof(WorldEditorBottomBar));
    //}

    ////[Test]
    ////public IEnumerator EditorPanelEmpty()
    ////{
    ////    int oldTextSize = EditorColorPalette.EditorButtonTextSize;
    ////    EditorColorPalette.EditorButtonTextSize = 9;
    ////    {
    ////        var editorPanel = new EditorWindow("Test");
    ////        UI.AddChild(editorPanel);
    ////    }

    ////    yield return WaitUILayout();
    ////    VerifyScreenshot(nameof(NewUITests), nameof(EditorPanelEmpty));

    ////    EditorColorPalette.EditorButtonTextSize = oldTextSize;
    ////}

    //[Test]
    //public IEnumerator OutsideParentWindow()
    //{
    //    var win = new UISolidColor
    //    {
    //        WindowColor = Color.PrettyRed,
    //        Anchor = UIAnchor.TopLeft,
    //        ParentAnchor = UIAnchor.CenterCenter,

    //        MinSize = new Vector2(20, 20),
    //        GrowY = false,

    //        SetChildren = new()
    //        {
    //            new UISolidColor()
    //            {
    //                WindowColor = Color.PrettyGreen,
    //                MinSize = new Vector2(20),
    //                Anchor = UIAnchor.TopRight,
    //                ParentAnchor = UIAnchor.TopLeft,
    //            },

    //            new UISolidColor()
    //            {
    //                WindowColor = Color.PrettyPurple,
    //                MinSize = new Vector2(20),
    //                Anchor = UIAnchor.TopLeft,
    //                ParentAnchor = UIAnchor.BottomLeft
    //            },
    //        },
    //    };
    //    UI.AddChild(win);

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(OutsideParentWindow));

    //    win.Paddings = new Rectangle(5, 5, 5, 5);

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(OutsideParentWindow));

    //    win.Paddings = Rectangle.Empty;
    //    win.LayoutMode = LayoutMode.HorizontalList;

    //    var a = new UISolidColor()
    //    {
    //        WindowColor = Color.White,
    //        MinSize = new Vector2(20),
    //        GrowX = false,
    //        GrowY = false,
    //    };
    //    win.AddChild(a);

    //    var b = new UISolidColor()
    //    {
    //        WindowColor = Color.Black,
    //        MinSize = new Vector2(20),
    //        GrowX = false,
    //        GrowY = false,
    //    };
    //    win.AddChild(b);

    //    var c = new UISolidColor()
    //    {
    //        WindowColor = Color.White,
    //        MinSize = new Vector2(20),
    //        GrowX = false,
    //        GrowY = false,
    //    };
    //    win.AddChild(c);

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(OutsideParentWindow));

    //    win.Paddings = new Rectangle(5, 5, 5, 5);

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(OutsideParentWindow));
    //}

    //[Test]
    //public IEnumerator BackgroundWindow()
    //{
    //    var win = new UISolidColor
    //    {
    //        WindowColor = Color.PrettyRed,

    //        Paddings = new Rectangle(5, 5, 5, 5),
    //        SetChildren = new()
    //        {
    //            new UISolidColor()
    //            {
    //                WindowColor = Color.White,
    //                MinSize = new Vector2(20),
    //                GrowX = false,
    //                GrowY = false,
    //            },
    //            new UISolidColor()
    //            {
    //                WindowColor = Color.Black,
    //                MinSize = new Vector2(20),
    //                GrowX = false,
    //                GrowY = false,
    //            },
    //            new UISolidColor()
    //            {
    //                WindowColor = Color.White,
    //                MinSize = new Vector2(20),
    //                GrowX = false,
    //                GrowY = false,
    //            },
    //            new UISolidColor()
    //            {
    //                WindowColor = Color.PrettyGreen,
    //                BackgroundWindow = true
    //            },
    //        },
    //    };
    //    UI.AddChild(win);

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(BackgroundWindow));

    //    win.LayoutMode = LayoutMode.HorizontalList;
    //    win.InvalidateLayout();

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(BackgroundWindow));
    //}

    //[Test]
    //public IEnumerator HorizontalPanelLayout()
    //{
    //    var win = new UISolidColor
    //    {
    //        WindowColor = Color.PrettyRed,
    //        LayoutMode = LayoutMode.HorizontalEditorPanel,

    //        Id = "Panel",
    //        Paddings = new Rectangle(5, 5, 5, 5),
    //        SetChildren = new()
    //        {
    //            new UISolidColor()
    //            {
    //                GrowX = false,
    //                MinSize = new Vector2(50),
    //                WindowColor = Color.PrettyGreen,
    //            },
    //            new HorizontalPanelSeparator(),
    //            new UISolidColor()
    //            {
    //                GrowX = false,
    //                MinSize = new Vector2(50),
    //                WindowColor = Color.PrettyBlue,
    //            },
    //        }
    //    };
    //    UI.AddChild(win);

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(HorizontalPanelLayout));

    //    //yield return new TestWaiterRunLoops(-1);

    //    yield break;
    //}

    //[Test]
    //public IEnumerator IsWithinTest()
    //{
    //    UIBaseWindow parent = new UIBaseWindow();
    //    UI.AddChild(parent);

    //    UIBaseWindow child = new UIBaseWindow();
    //    parent.AddChild(child);

    //    Assert.True(child.IsWithin(parent));
    //    Assert.False(parent.IsWithin(child));

    //    UIBaseWindow otherParent = new UIBaseWindow();
    //    UI.AddChild(otherParent);

    //    UIBaseWindow relative = new UIBaseWindow();
    //    parent.Id = "Parent";
    //    relative.RelativeTo = "Parent";
    //    otherParent.AddChild(relative);

    //    Assert.True(relative.IsWithin(parent));
    //    Assert.False(relative.IsWithin(otherParent));

    //    Assert.True(parent.IsWithin(UI));
    //    Assert.True(child.IsWithin(UI));
    //    Assert.True(otherParent.IsWithin(UI));

    //    yield break;
    //}

    //[Test]
    //public IEnumerator OverlayWindow()
    //{
    //    UISolidColor win = new UISolidColor()
    //    {
    //        WindowColor = Color.Black,
    //        LayoutMode = LayoutMode.VerticalList,
    //        Paddings = new Rectangle(10, 10, 10, 10),
    //        GrowY = false,
    //        Id = "MainWin"
    //    };
    //    UI.AddChild(win);

    //    UIBaseWindow insideWin = new UISolidColor()
    //    {
    //        WindowColor = Color.Red,
    //        MinSizeY = 20,
    //        MaxSizeY = 20,
    //        Offset = new Vector2(0, 10),
    //        Id = "FirstWin"
    //    };
    //    win.AddChild(insideWin);

    //    UIBaseWindow insideWin2 = new UISolidColor()
    //    {
    //        WindowColor = Color.Green,
    //        MinSizeY = 20,
    //        MaxSizeY = 20,
    //    };
    //    win.AddChild(insideWin2);

    //    UIBaseWindow insideWin3 = new UISolidColor()
    //    {
    //        WindowColor = Color.Blue,
    //        MinSizeY = 20,
    //        MaxSizeY = 20,
    //    };
    //    win.AddChild(insideWin3);

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(OverlayWindow));

    //    insideWin.OverlayWindow = true;

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(OverlayWindow));

    //    insideWin.OverlayWindow = false;

    //    UIBaseWindow relativeWin = new UISolidColor()
    //    {
    //        WindowColor = Color.PrettyYellow,

    //        MinSizeY = 20,
    //        MaxSizeY = 20,
    //        RelativeTo = "FirstWin",
    //        Offset = new Vector2(0, 10)
    //    };
    //    UI.AddChild(relativeWin);

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(OverlayWindow));

    //    relativeWin.OverlayWindow = true;

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(OverlayWindow));
    //}

    //[Test]
    //public IEnumerator RichTextCases()
    //{
    //    UIRichText label = new UIRichText();
    //    label.Text = "The quick brown fox jumped over the lazy dog.";
    //    label.WindowColor = Color.Red;
    //    label.FontSize = 20;
    //    UI.AddChild(label);

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(RichTextCases));

    //    // Check for tag false positives
    //    label.Text = "<text in brackets that isnt a tag>";

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(RichTextCases));

    //    label.Text = "<word>";

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(RichTextCases));

    //    // Check color tag functionality

    //    label.Text = "The quick brown <color #00FF00>fox</> jumped over the lazy dog.";

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(RichTextCases));

    //    // Check outline tag functionality
    //    // (Currently text outline functionality doesn't work)

    //    label.Text = "The quick brown <outline #00FF00 size=5>fox</> jumped over the lazy dog.";

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(RichTextCases));

    //    // Check center tag functionality

    //    label.Text = "<center>The quick brown fox,</>\n<center> jumped over the lazy dog.</>";

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(RichTextCases));

    //    label.Text = "<center>The quick brown fox,\n jumped over the lazy dog.</>";

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(RichTextCases));

    //    // Check right tag functionality

    //    label.Text = "<right>The quick brown fox,</>\n<right> jumped over the lazy dog.</>";

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(RichTextCases));

    //    label.Text = "<right>The quick brown fox,\n jumped over the lazy dog.</>";

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(RichTextCases));

    //    // Check some edge cases

    //    label.Text = "<unclosed The quick brown fox, jumped over the lazy dog.";

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(RichTextCases));

    //    label.Text = "</>The quick brown fox, jumped over the lazy dog.";

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(RichTextCases));
    //}

    //[Test]
    //public IEnumerator CenterAndFillWarningCheck()
    //{
    //    var parent = new UISolidColor()
    //    {
    //        WindowColor = Color.CornflowerBlue,

    //        MinSizeY = 10,
    //        GrowY = false,
    //    };
    //    UI.AddChild(parent);

    //    var windowOne = new UISolidColor()
    //    {
    //        LayoutMode = LayoutMode.HorizontalList,
    //        AnchorAndParentAnchor = UIAnchor.CenterLeft,
    //        ListSpacing = new Vector2(5, 0),
    //        WindowColor = Color.PrettyOrange
    //    };
    //    parent.AddChild(windowOne);

    //    var windowOneChild = new UISolidColor()
    //    {
    //        WindowColor = Color.Red,

    //        MinSizeX = 10,
    //        MinSizeY = 10,

    //        GrowX = false,
    //        GrowY = false,
    //    };
    //    windowOne.AddChild(windowOneChild);

    //    var windowTwo = new UIBaseWindow()
    //    {
    //        LayoutMode = LayoutMode.HorizontalList,
    //        AnchorAndParentAnchor = UIAnchor.CenterRight,
    //        ListSpacing = new Vector2(5, 0),
    //        Margins = new Rectangle(0, 10, 0, 0),

    //        GrowX = false
    //    };
    //    parent.AddChild(windowTwo);

    //    var windowTwoChild = new UISolidColor()
    //    {
    //        WindowColor = Color.Red,

    //        MinSizeX = 200,
    //        MinSizeY = 200,

    //        GrowX = false,
    //        GrowY = false,
    //    };
    //    windowTwo.AddChild(windowTwoChild);

    //    List<UIBaseWindow.UIWarning> warnings = windowOne.GetWarnings();
    //    bool found = false;
    //    foreach (var warning in warnings)
    //    {
    //        if (warning.Warning == UIBaseWindow.WARN_CENTER_FILL)
    //            found = true;
    //    }

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(CenterAndFillWarningCheck));

    //    Assert.True(found);
    //}

    //[Test]
    //public IEnumerator ScrollAreaTests()
    //{
    //    var parent = new UISolidColor()
    //    {
    //        WindowColor = Color.CornflowerBlue,
    //        AnchorAndParentAnchor = UIAnchor.CenterCenter,
    //        MinSize = new Vector2(200, 200),
    //        MaxSize = new Vector2(200, 200)
    //    };
    //    UI.AddChild(parent);

    //    var scrollArea = new UIScrollArea()
    //    {
    //    };
    //    parent.AddChild(scrollArea);

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(ScrollAreaTests));

    //    var list = new UIBaseWindow()
    //    {
    //        LayoutMode = LayoutMode.VerticalList,
    //        ListSpacing = new Vector2(0, 5),
    //        ChildrenHandleInput = false
    //    };
    //    scrollArea.AddChildInside(list);

    //    for (int i = 0; i < 10; i++)
    //    {
    //        var editorButton = new EditorButton();
    //        editorButton.GrowX = true;
    //        editorButton.Text = $"Button {i}";
    //        list.AddChild(editorButton);
    //    }

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(ScrollAreaTests));

    //    scrollArea.ScrollTo(new Vector2(100, 100));

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(ScrollAreaTests));

    //    scrollArea.AutoHideScrollX = false;
    //    scrollArea.InvalidateLayout();

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(ScrollAreaTests));

    //    scrollArea.AutoHideScrollX = true;
    //    scrollArea.AutoHideScrollY = true;

    //    list.ClearChildren();
    //    for (int i = 0; i < 3; i++)
    //    {
    //        var editorButton = new EditorButton();
    //        editorButton.GrowX = true;
    //        editorButton.Text = $"Button {i} ----------";
    //        list.AddChild(editorButton);
    //    }

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(ScrollAreaTests));

    //    scrollArea.ScrollTo(new Vector2(100, 100));

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(ScrollAreaTests));

    //    list.Close();
    //    scrollArea.ScrollTo(new Vector2(0, 0));
    //    UITexture texture = new UITexture()
    //    {
    //        TextureFile = "Images/logoAlpha.png",
    //        ImageScale = new Vector2(3f)
    //    };
    //    scrollArea.AddChildInside(texture);

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(ScrollAreaTests));

    //    scrollArea.ScrollTo(new Vector2(100, 100));

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(ScrollAreaTests));

    //    // todo: test input (scroll wheel, scroll bar dragging etc)
    //}
}