#nullable enable

#region Using

using Emotion.Core;
using Emotion.Editor.EditorUI.Components;
using Emotion.Game.Systems.UI;
using Emotion.Game.Systems.UI2;
using Emotion.Game.Systems.UI2.Editor;
using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.Testing;
using System;
using System.Collections;
using System.Numerics;

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

    protected IEnumerator WaitUILayout()
    {
        Engine.UI.Update();
        yield return Engine.UI.WaitLoadingRoutine();
        yield return new TestWaiterRunLoops(1);
    }

    public override void BetweenEachTest()
    {
        base.BetweenEachTest();
        SceneUI.ClearChildren();
    }

    [Test]
    public IEnumerator TestWindow()
    {
        var win = new UIBaseWindow()
        {
            Visuals =
            {
                BackgroundColor = Color.White
            },
            Layout =
            {
                SizingX = UISizing.Grow(),
                SizingY = UISizing.Grow()
            }
        };
        SceneUI.AddChild(win);

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITests), nameof(TestWindow));
    }

    [Test]
    public IEnumerator TestFreeLayout()
    {
        // Test all anchor combinations
        UIAnchor[] anchorValues = Enum.GetValues<UIAnchor>();
        foreach (UIAnchor anchorVal in anchorValues)
        {
            foreach (UIAnchor parentAnchor in anchorValues)
            {
                var win = new UIBaseWindow()
                {
                    Name = $"{anchorVal}-{parentAnchor}",
                    Visuals =
                    {
                        BackgroundColor = Color.White
                    },
                    Layout =
                    {
                        SizingX = UISizing.Fixed(45),
                        SizingY = UISizing.Fixed(45),
                        Anchor = anchorVal,
                        ParentAnchor = parentAnchor
                    }
                };

                SceneUI.AddChild(win);
            }
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITests), nameof(TestFreeLayout));
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
        SceneUI.AddChild(win);

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

        SceneUI.AddChild(win);

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

        SceneUI.AddChild(win);

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITests), nameof(FillXAxisMinHeight));
    }

    [Test]
    public IEnumerator TestPaddings()
    {
        var winParent = new UIBaseWindow()
        {
            Layout =
            {
                SizingX = UISizing.Fit()
            }
        };
        SceneUI.AddChild(winParent);

        var win = new UIBaseWindow
        {
            Visuals =
            {
                BackgroundColor = Color.PrettyOrange,
            },
            Layout =
            {
                SizingX = UISizing.Grow(),
                SizingY = UISizing.Grow(),
                Padding = new UISpacing(5, 5, 5, 5)
            }
        };
        winParent.AddChild(win);

        var winChildList = new UIBaseWindow
        {
            Visuals =
            {
                BackgroundColor = Color.White
            },
            Layout =
            {
                LayoutMethod = UILayoutMethod.VerticalList(5)
            }
        };
        win.AddChild(winChildList);

        for (int i = 0; i < 5; i++)
        {
            winChildList.AddChild(new UIBaseWindow()
            {
                Visuals =
                {
                    BackgroundColor = Color.Blue,
                },
                Layout =
                {
                    SizingX = UISizing.Grow(),
                    SizingY = UISizing.Grow(),
                },
                Children =
                {
                    new UIBaseWindow()
                    {
                        Layout =
                        {
                            SizingX = UISizing.Fixed(10 + 20 * i)
                        }
                    }
                }
            });
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITests), nameof(TestPaddings));
    }

    private class UIWindowForTestingCentering : UIBaseWindow
    {
        protected override void InternalRender(Renderer r)
        {
            var bounds = CalculatedMetrics.Bounds.ToRect();

            r.RenderLine(bounds.Position + new Vector2(0, bounds.Height / 2), bounds.Position + new Vector2(bounds.Width, bounds.Height / 2), Color.White, 3);
            r.RenderLine(bounds.Position + new Vector2(bounds.Width / 2, 0), bounds.Position + new Vector2(bounds.Width / 2, bounds.Height), Color.White, 3);
            base.InternalRender(r);
        }
    }

    [DebugTest]
    [Test]
    public IEnumerator ComplicatedLayoutTest()
    {
        UIContainer container = new()
        {
            Visuals =
            {
                BackgroundColor = Color.PrettyPurple
            },
            Layout =
            {
                LayoutMethod = UILayoutMethod.VerticalList(16),
                Padding = new UISpacing(16, 16, 16, 16),
                MinSizeX = 430,
                MaxSizeX = 630,
            }
        };
        SceneUI.AddChild(container);

        string[] items = ["Copy", "Paste", "Delete", "Layer", "Comment", "Look up in dictionary"];
        for (int i = 0; i < items.Length; i++)
        {
            UIBaseWindow item = new()
            {
                Layout =
                {
                    LayoutMethod = UILayoutMethod.HorizontalList(32),

                    Padding = new UISpacing(32, 16, 32, 16),
                    MinSizeY = 80
                },
                Visuals =
                {
                    BackgroundColor = Color.PrettyPink
                },
                Children =
                {
                    new UIText()
                    {
                        Text = items[i],
                        FontSize = 50,
                        Layout =
                        {
                            AnchorAndParentAnchor = UIAnchor.CenterLeft,
                            SizingX = UISizing.Grow()
                        }
                    },
                    new UIPicture()
                    {
                        Texture = "Editor/Checkmark.png",
                        Layout =
                        {
                            AnchorAndParentAnchor = UIAnchor.CenterLeft,
                            SizingX = UISizing.Fixed(60),
                            SizingY = UISizing.Fixed(60),
                        },
                        Smooth = true
                    }
                }
            };
            container.AddChild(item);
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITests), nameof(ComplicatedLayoutTest));
    }

    [Test]
    public IEnumerator ListOfBars()
    {
        var bg = new UIBaseWindow()
        {
            Visuals =
            {
                BackgroundColor = Color.CornflowerBlue
            }
        };
        SceneUI.AddChild(bg);

        var healthBars = new UIBaseWindow()
        {
            Layout =
            {
                LayoutMethod = UILayoutMethod.HorizontalList(2),
                AnchorAndParentAnchor = UIAnchor.BottomCenter,
                Margins = new UISpacing(0, 0, 0, 130),
                SizingX = UISizing.Fit(),
                SizingY = UISizing.Fit()
            },
        };
        bg.AddChild(healthBars);

        float[] percents = new float[] { 100, 90, 80, 70, 60 };
        for (int i = 0; i < percents.Length; i++)
        {
            var frameParent = new UIBaseWindow();
            frameParent.Layout.SizingX = UISizing.Fixed(90);
            frameParent.Layout.SizingY = UISizing.Fixed(55);
            frameParent.Visuals.BorderColor = Color.Black;
            frameParent.Visuals.BackgroundColor = Color.Black * 0.3f;
            frameParent.Visuals.Border = 1;
            healthBars.AddChild(frameParent);

            var healthBarFill = new UIBaseWindow()
            {
                Layout =
                {
                    Offset = new IntVector2(1, 1),
                    SizingX = UISizing.Fixed(88),
                    SizingY = UISizing.Fixed(53),
                },
                Visuals =
                {
                    BackgroundColor = Color.PrettyBrown
                }
            };
            frameParent.AddChild(healthBarFill);

            var selectionFrame = new UIBaseWindow()
            {
                Visuals =
                {
                    BorderColor = Color.PrettyYellow,
                    Border = 3,

                    Visible = false
                }
            };
            frameParent.AddChild(selectionFrame);

            int fillAmount = (int)MathF.Round(88 * (percents[i] / 100f));
            healthBarFill.Layout.SizingX = UISizing.Fixed(fillAmount);
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITests), nameof(ListOfBars));
    }

    [Test]
    public IEnumerator FillXAxisMinHeightAndWidth()
    {
        {
            var win = new UIBaseWindow()
            {
                Name = "test",
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
                var a = new UIBaseWindow()
                {
                    Visuals =
                    {
                        BackgroundColor = Color.White
                    },
                    Layout =
                    {
                        SizingY = UISizing.Fixed(60)
                    }
                };
                win.AddChild(a);
            }

            SceneUI.AddChild(win);
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITests), nameof(FillXAxisMinHeightAndWidth));
    }

    [Test]
    public IEnumerator FillNeitherAxisMinHeightAndWidth()
    {
        {
            var win = new UIBaseWindow()
            {
                Name = "test",
                Visuals =
                {
                    BackgroundColor = Color.PrettyOrange,
                },
                Layout =
                {
                    SizingX = UISizing.Fit(),
                    SizingY = UISizing.Fit(),
                }
            };

            {
                var a = new UIBaseWindow()
                {
                    Visuals =
                    {
                        BackgroundColor = Color.White
                    },
                    Layout =
                    {
                        MinSizeX = 60,
                        MinSizeY = 60
                    }
                };
                win.AddChild(a);
            }

            SceneUI.AddChild(win);
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITests), nameof(FillNeitherAxisMinHeightAndWidth));
    }

    [Test]
    public IEnumerator TwoSquaresInFillY()
    {
        {
            var win = new UIBaseWindow()
            {
                Name = "test",
                Visuals =
                {
                    BackgroundColor = Color.PrettyOrange
                },
                Layout =
                {
                    SizingX = UISizing.Fit()
                }
            };
            SceneUI.AddChild(win);

            {
                var a = new UIBaseWindow()
                {
                    Visuals =
                    {
                        BackgroundColor = Color.White
                    },
                    Layout =
                    {
                        SizingX = UISizing.Fixed(60),
                        SizingY = UISizing.Fixed(60)
                    }
                };
                win.AddChild(a);
            }

            {
                var a = new UIBaseWindow()
                {
                    Visuals =
                    {
                        BackgroundColor = Color.Black
                    },
                    Layout =
                    {
                        AnchorAndParentAnchor = UIAnchor.BottomLeft,
                        SizingX = UISizing.Fixed(60),
                        SizingY = UISizing.Fixed(60)
                    }
                };
                win.AddChild(a);
            }
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITests), nameof(TwoSquaresInFillY));
    }

    [Test]
    public IEnumerator FillList()
    {
        {
            var win = new UIBaseWindow()
            {
                Name = "test",
                Visuals =
                {
                    BackgroundColor = Color.PrettyOrange
                },
                Layout =
                {
                    LayoutMethod = UILayoutMethod.HorizontalList(0)
                }
            };

            {
                var a = new UIBaseWindow()
                {
                    Visuals =
                    {
                        BackgroundColor = Color.White
                    }
                };
                win.AddChild(a);
            }

            SceneUI.AddChild(win);
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITests), nameof(FillList));

        {
            UIBaseWindow list = SceneUI.GetWindowById("test")!;
            list.Layout.LayoutMethod = UILayoutMethod.VerticalList(0);
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITests), nameof(FillList));
    }

    [Test]
    public IEnumerator FillListThreeItems()
    {
        {
            var win = new UIBaseWindow()
            {
                Name = "test",
                Visuals =
                {
                    BackgroundColor = Color.PrettyOrange
                },
                Layout =
                {
                    LayoutMethod = UILayoutMethod.HorizontalList(0)
                }
            };

            {
                var a = new UIBaseWindow()
                {
                    Visuals =
                    {
                        BackgroundColor = Color.White
                    },
                    Layout =
                    {
                        SizingX = UISizing.Fixed(60),
                        SizingY = UISizing.Fixed(60),
                    }
                };
                win.AddChild(a);
            }

            {
                var a = new UIBaseWindow()
                {
                    Visuals =
                    {
                        BackgroundColor = Color.Black
                    },
                    Layout =
                    {
                        SizingX = UISizing.Fixed(60),
                        SizingY = UISizing.Fixed(60),
                    }
                };
                win.AddChild(a);
            }

            {
                var a = new UIBaseWindow()
                {
                    Visuals =
                    {
                        BackgroundColor = Color.PrettyPink
                    },
                    Layout =
                    {
                        SizingX = UISizing.Fixed(60),
                        SizingY = UISizing.Fixed(60),
                    }
                };
                win.AddChild(a);
            }

            SceneUI.AddChild(win);
        }

        for (var i = 0; i < 2; i++)
        {
            // Do second pass with vertical layout.
            string? screenshotExtraText = null;
            if (i == 1)
            {
                UIBaseWindow list = SceneUI.GetWindowById("test")!;
                list.Layout.LayoutMethod = UILayoutMethod.VerticalList(0);
                list.Layout.SizingX = UISizing.Grow();
                list.Layout.SizingY = UISizing.Grow();
                screenshotExtraText = "+VerticalList";
            }

            yield return WaitUILayout();
            yield return VerifyScreenshot(nameof(NewUITests), nameof(FillListThreeItems), screenshotExtraText);

            {
                UIBaseWindow list = SceneUI.GetWindowById("test")!;
                list.Layout.SizingX = UISizing.Fit();
            }

            yield return WaitUILayout();
            yield return VerifyScreenshot(nameof(NewUITests), nameof(FillListThreeItems), screenshotExtraText);

            {
                UIBaseWindow list = SceneUI.GetWindowById("test")!;
                list.Layout.SizingX = UISizing.Grow();
                list.Layout.SizingY = UISizing.Fit();
            }

            yield return WaitUILayout();
            yield return VerifyScreenshot(nameof(NewUITests), nameof(FillListThreeItems), screenshotExtraText);

            {
                UIBaseWindow list = SceneUI.GetWindowById("test")!;
                list.Layout.SizingX = UISizing.Fit();
                list.Layout.SizingY = UISizing.Fit();
            }

            yield return WaitUILayout();
            yield return VerifyScreenshot(nameof(NewUITests), nameof(FillListThreeItems), screenshotExtraText);

            {
                UIBaseWindow list = SceneUI.GetWindowById("test")!;
                if (i == 0)
                    list.Layout.LayoutMethod = UILayoutMethod.HorizontalList(15);
                else
                    list.Layout.LayoutMethod = UILayoutMethod.VerticalList(15);
            }

            yield return WaitUILayout();
            yield return VerifyScreenshot(nameof(NewUITests), nameof(FillListThreeItems), screenshotExtraText);
        }
    }

    [Test]
    public IEnumerator FillListFillingItems()
    {
        {
            var win = new UIBaseWindow()
            {
                Name = "test",
                Visuals =
                {
                    BackgroundColor = Color.PrettyOrange
                },
                Layout =
                {
                    LayoutMethod = UILayoutMethod.HorizontalList(0)
                }
            };

            {
                var a = new UIBaseWindow()
                {
                    Visuals =
                    {
                        BackgroundColor = Color.White
                    },
                    Layout =
                    {
                        SizingX = UISizing.Grow(),
                        SizingY = UISizing.Grow(),
                    }
                };
                win.AddChild(a);
            }

            {
                var a = new UIBaseWindow()
                {
                    Visuals =
                    {
                        BackgroundColor = Color.PrettyPink
                    },
                    Layout =
                    {
                        SizingX = UISizing.Fixed(60),
                        SizingY = UISizing.Fixed(60)
                    }
                };
                win.AddChild(a);
            }

            {
                var a = new UIBaseWindow()
                {
                    Visuals =
                    {
                        BackgroundColor = Color.Black
                    },
                    Layout =
                    {
                        SizingX = UISizing.Grow(),
                        SizingY = UISizing.Grow(),
                    }
                };
                win.AddChild(a);
            }

            SceneUI.AddChild(win);
        }

        for (var i = 0; i < 2; i++)
        {
            // Do second pass with vertical layout.
            string? screenshotExtraText = null;
            if (i == 1)
            {
                UIBaseWindow list = SceneUI.GetWindowById("test")!;
                list.Layout.LayoutMethod = UILayoutMethod.VerticalList(0);
                list.Layout.SizingX = UISizing.Grow();
                list.Layout.SizingY = UISizing.Grow();
                screenshotExtraText = "+VerticalList";
            }

            yield return WaitUILayout();
            yield return VerifyScreenshot(nameof(NewUITests), nameof(FillListFillingItems), screenshotExtraText);

            {
                UIBaseWindow list = SceneUI.GetWindowById("test")!;
                list.Layout.SizingX = UISizing.Fit();
            }

            yield return WaitUILayout();
            yield return VerifyScreenshot(nameof(NewUITests), nameof(FillListFillingItems), screenshotExtraText);

            {
                UIBaseWindow list = SceneUI.GetWindowById("test")!;
                list.Layout.SizingX = UISizing.Grow();
                list.Layout.SizingY = UISizing.Fit();
            }

            yield return WaitUILayout();
            yield return VerifyScreenshot(nameof(NewUITests), nameof(FillListFillingItems), screenshotExtraText);

            {
                UIBaseWindow list = SceneUI.GetWindowById("test")!;
                list.Layout.SizingX = UISizing.Fit();
                list.Layout.SizingY = UISizing.Fit();
            }

            yield return WaitUILayout();
            yield return VerifyScreenshot(nameof(NewUITests), nameof(FillListFillingItems), screenshotExtraText);

            {
                UIBaseWindow list = SceneUI.GetWindowById("test")!;
                if (i == 0)
                    list.Layout.LayoutMethod = UILayoutMethod.HorizontalList(15);
                else
                    list.Layout.LayoutMethod = UILayoutMethod.VerticalList(15);
            }

            yield return WaitUILayout();
            yield return VerifyScreenshot(nameof(NewUITests), nameof(FillListFillingItems), screenshotExtraText);

            {
                UIBaseWindow list = SceneUI.GetWindowById("test")!;
                list.Layout.SizingX = UISizing.Grow();
                list.Layout.SizingY = UISizing.Grow();
            }

            yield return WaitUILayout();
            yield return VerifyScreenshot(nameof(NewUITests), nameof(FillListFillingItems), screenshotExtraText);
        }
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
            var win = new UIBaseWindow()
            {
                Name = "top-parent",
                Layout =
                {
                    SizingY = UISizing.Fixed(51)
                },
                Visuals =
                {
                    BackgroundColor = Color.PrettyOrange
                },
                Children =
                {
                    new UIBaseWindow
                    {
                        Name = "list",
                        Layout =
                        {
                            LayoutMethod = UILayoutMethod.HorizontalList(9),
                            Margins = new UISpacing(9, 9, 9, 9),
                            SizingY = UISizing.Fit()
                        },
                        Visuals =
                        {
                            BackgroundColor = Color.PrettyGreen
                        },
                        Children =
                        {
                            new UIBaseWindow
                            {
                                Name = "text-bg",
                                Visuals =
                                {
                                    BackgroundColor = Color.Black,
                                },
                                Layout =
                                {
                                    Padding = new UISpacing(6, 3, 6, 3),
                                    SizingX = UISizing.Fit()
                                },
                                Children =
                                {
                                    new UIText
                                    {
                                        Name = "text",
                                        FontSize = 27,
                                        Text = "Black",
                                        TextColor = Color.White,
                                        Layout =
                                        {
                                            AnchorAndParentAnchor = UIAnchor.CenterLeft
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            SceneUI.AddChild(win);
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITests), nameof(WorldEditorTopBar));

        {
            UIBaseWindow list = SceneUI.GetWindowById("list")!;

            {
                var a = new UIBaseWindow()
                {
                    Name = "text-bg",
                    Visuals =
                    {
                        BackgroundColor = Color.White,
                    },
                    Layout =
                    {
                        Padding = new UISpacing(6, 3, 6, 3),
                        SizingX = UISizing.Fit()
                    }
                };
                list.AddChild(a);

                var text = new UIText()
                {
                    Name = "text",
                    TextColor = Color.Black,
                    FontSize = 27,
                    Text = "White",
                    Layout =
                    {
                        AnchorAndParentAnchor = UIAnchor.CenterLeft
                    }
                };
                a.AddChild(text);
            }

            {
                var a = new UIBaseWindow()
                {
                    Visuals =
                    {
                        BackgroundColor = Color.PrettyPink
                    },
                    Layout =
                    {
                        Padding = new UISpacing(6, 3, 6, 3),
                        SizingX = UISizing.Fit()
                    }
                };
                list.AddChild(a);

                var text = new UIText()
                {
                    Name = "text",
                    TextColor = Color.White,
                    FontSize = 27,
                    Text = "Pink",
                    Layout =
                    {
                        AnchorAndParentAnchor = UIAnchor.CenterLeft
                    }
                };
                a.AddChild(text);
            }

            yield return WaitUILayout();
            yield return VerifyScreenshot(nameof(NewUITests), nameof(WorldEditorTopBar));
        }

        // Bar v2 (Possible only with the new UI)
        {
            UIBaseWindow list = SceneUI.GetWindowById("list")!;

            list.Layout.Margins = new UISpacing(9, 0, 9, 0);
            list.Layout.AnchorAndParentAnchor = UIAnchor.CenterLeft;
            list.InvalidateLayout();
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITests), nameof(WorldEditorTopBar));

        // Add text on the right
        {
            UIBaseWindow parent = SceneUI.GetWindowById("top-parent")!;

            var a = new UIText()
            {
                Layout =
                {
                    AnchorAndParentAnchor = UIAnchor.CenterRight,
                    Margins = new UISpacing(0, 0, 15, 0)
                },
                Text = "Text on the right",
                FontSize = 18,
                TextColor = Color.Black
            };
            parent.AddChild(a);
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITests), nameof(WorldEditorTopBar));
    }

    [Test]
    public IEnumerator VerticalListWithText()
    {
        {
            var list = new UIBaseWindow
            {
                Visuals =
                {
                    BackgroundColor = Color.PrettyOrange
                },
                Layout =
                {
                    SizingX = UISizing.Fit(),
                    SizingY = UISizing.Fit(),
                    LayoutMethod = UILayoutMethod.VerticalList(0)
                }
            };

            {
                var a = new UIBaseWindow()
                {
                    Name = "text-bg",
                    Visuals =
                    {
                        BackgroundColor = Color.Black
                    },
                    Layout =
                    {
                       Padding = new UISpacing(6, 3, 6, 3)
                    }
                };
                list.AddChild(a);

                var text = new UIText()
                {
                    Name = "text",
                    FontSize = 27,
                    Text = "Black",
                    TextColor = Color.White,
                    Layout =
                    {
                        AnchorAndParentAnchor = UIAnchor.CenterLeft
                    }
                };
                a.AddChild(text);
            }

            {
                var a = new UIBaseWindow()
                {
                    Visuals =
                    {
                        BackgroundColor = Color.White
                    },
                    Layout =
                    {
                        Padding = new UISpacing(6, 3, 6, 3)
                    }
                };
                list.AddChild(a);

                var text = new UIText()
                {
                    Name = "text",
                    FontSize = 27,
                    Text = "White",
                    TextColor = Color.Black,
                    Layout =
                    {
                        AnchorAndParentAnchor = UIAnchor.CenterLeft
                    }
                };
                a.AddChild(text);
            }

            {
                var a = new UIBaseWindow()
                {
                    Visuals =
                    {
                        BackgroundColor = Color.PrettyPink
                    },
                    Layout =
                    {
                        Padding = new UISpacing(6, 3, 6, 3),
                        SizingX = UISizing.Fit()
                    }
                };
                list.AddChild(a);

                var text = new UIText()
                {
                    Name = "text",
                    FontSize = 27,
                    Text = "Pink",
                    TextColor = Color.White,
                    Layout =
                    {
                        AnchorAndParentAnchor = UIAnchor.CenterLeft,
                        SizingX = UISizing.Fit()
                    }
                };
                a.AddChild(text);
            }

            SceneUI.AddChild(list);
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITests), nameof(VerticalListWithText));

        SceneUI.ClearChildren();

        // Prototype of the editor dropdown.
        // This tests:
        // 1. paddings on all sides of a fill
        // 2. Vertical UIList
        // 3. FillInListX

        {
            var dropDown = new UIBaseWindow
            {
                Visuals =
                {
                    BackgroundColor = Color.PrettyOrange
                },
                Layout =
                {
                    SizingX = UISizing.Fit(),
                    SizingY = UISizing.Fit(),
                }
            };

            var innerBg = new UIBaseWindow
            {
                Visuals =
                {
                  BackgroundColor = Color.PrettyGreen,
                },
                Layout =
                {
                    Padding = new UISpacing(9, 9, 9, 9)
                }
                //IgnoreParentColor = true,
            };
            dropDown.AddChild(innerBg);

            var list = new UIBaseWindow // UIList
            {
                Name = "list",
                Layout =
                {
                    LayoutMethod = UILayoutMethod.VerticalList(6)
                }
            };
            innerBg.AddChild(list);

            for (var i = 0; i < 5; i++)
            {
                var a = new UIBaseWindow()
                {
                    Visuals =
                    {
                        BackgroundColor = Color.Black
                    },
                    Layout =
                    {
                        Padding = new UISpacing(6, 3, 6, 3),
                        SizingX = UISizing.Fit()
                    }
                };
                list.AddChild(a);

                var text = new UIText()
                {
                    Name = "text",
                    FontSize = 27,
                    Text = "Black " + new string('A', i),
                    TextColor = Color.White,
                    Layout =
                    {
                        AnchorAndParentAnchor = UIAnchor.CenterLeft
                    }
                };
                a.AddChild(text);
            }

            SceneUI.AddChild(dropDown);
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITests), nameof(VerticalListWithText));

        {
            UIBaseWindow? list = SceneUI.GetWindowById("list");
            Assert.NotNull(list);
            Assert.NotNull(list.Children);
            for (var i = 0; i < list.Children.Count; i++)
            {
                UIBaseWindow child = list.Children[i];
                child.Layout.SizingX = UISizing.Grow();
            }
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITests), nameof(VerticalListWithText));

        {
            UIBaseWindow? list = SceneUI.GetWindowById("list");
            Assert.NotNull(list);
            list.Layout.Margins = new UISpacing(0, 0, 24, 0);
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITests), nameof(VerticalListWithText));
    }

    [Test]
    public IEnumerator TextWithBackground()
    {
        var container = new UIBaseWindow()
        {
            Layout =
            {
                SizingX = UISizing.Fit(),
                SizingY = UISizing.Fit(),
            }
        };

        var bg = new UIBaseWindow()
        {
            Visuals =
            {
                BackgroundColor = Color.Red * 0.5f
            }
        };
        container.AddChild(bg);

        var text = new UIText()
        {
            Text = "Hello ladies and gentlemen, and welcome to the show!",
            TextColor = Color.White,
            FontSize = 35
        };
        container.AddChild(text);

        SceneUI.AddChild(container);

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITests), nameof(TextWithBackground));
    }

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

    [Test]
    public IEnumerator WorldEditorBottomBar()
    {
        var bottomBar = new UIBaseWindow()
        {
            Layout =
            {
                MaxSizeY = 36,
                SizingY = UISizing.Fit(),
                AnchorAndParentAnchor = UIAnchor.BottomLeft,
            },
            Visuals =
            {
                BackgroundColor = Color.PrettyOrange
            },
            Name = "BottomBar"
        };

        var label = new UIText()
        {
            Text = "No object selected",
            FontSize = 21,
            TextColor = Color.White,
            Layout =
            {
                Margins = new UISpacing(9, 9, 9, 9),
                AnchorAndParentAnchor = UIAnchor.CenterLeft,
            },
            Name = "Label"
        };
        bottomBar.AddChild(label);

        SceneUI.AddChild(bottomBar);

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITests), nameof(WorldEditorBottomBar));
    }

    //[Test]
    //public IEnumerator EditorPanelEmpty()
    //{
    //    {
    //        var editorPanel = new EditorWindow("Test");
    //        SceneUI.AddChild(editorPanel);
    //    }

    //    yield return WaitUILayout();
    //    VerifyScreenshot(nameof(NewUITests), nameof(EditorPanelEmpty));
    //}

    [Test]
    public IEnumerator OutsideParentWindow()
    {
        var list = new UIBaseWindow()
        {
            Visuals =
            {
                BackgroundColor = Color.PrettyRed
            },
            Layout =
            {
                Anchor = UIAnchor.TopLeft,
                ParentAnchor = UIAnchor.CenterCenter,
                MinSize = new IntVector2(60, 60),
                SizingY = UISizing.Fit(),
            }
        };
        SceneUI.AddChild(list);

        var greenChild = new UIBaseWindow()
        {
            Visuals =
            {
                BackgroundColor = Color.PrettyGreen
            },
            Layout =
            {
                SizingX = UISizing.Fixed(60),
                SizingY = UISizing.Fixed(60),
                Anchor = UIAnchor.TopRight,
                ParentAnchor = UIAnchor.TopLeft,
            }
        };
        list.AddChild(greenChild);

        var purpleChild = new UIBaseWindow()
        {
            Visuals =
            {
                BackgroundColor = Color.PrettyPurple
            },
            Layout =
            {
                SizingX = UISizing.Fixed(60),
                SizingY = UISizing.Fixed(60),
                Anchor = UIAnchor.TopLeft,
                ParentAnchor = UIAnchor.BottomLeft,
            }
        };
        list.AddChild(purpleChild);

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITests), nameof(OutsideParentWindow));

        list.Layout.Padding = new UISpacing(15, 15, 15, 15);

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITests), nameof(OutsideParentWindow));

        list.Layout.Padding = new UISpacing(0, 0, 0, 0);
        list.Layout.LayoutMethod = UILayoutMethod.HorizontalList(0);

        var whiteChild = new UIBaseWindow()
        {
            Visuals = { BackgroundColor = Color.White },
            Layout =
            {
                SizingX = UISizing.Fixed(60),
                SizingY = UISizing.Fixed(60),
            }
        };
        list.AddChild(whiteChild);

        var blackChild = new UIBaseWindow()
        {
            Visuals = { BackgroundColor = Color.Black },
            Layout =
            {
                SizingX = UISizing.Fixed(60),
                SizingY = UISizing.Grow(),
            }
        };
        list.AddChild(blackChild);

        var whiteChild2 = new UIBaseWindow()
        {
            Visuals = { BackgroundColor = Color.White },
            Layout =
            {
                SizingX = UISizing.Fixed(60),
                SizingY = UISizing.Fixed(60),
            }
        };
        list.AddChild(whiteChild2);

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITests), nameof(OutsideParentWindow));

        list.Layout.Padding = new UISpacing(15, 15, 15, 15);

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITests), nameof(OutsideParentWindow));
    }

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

    [Test]
    public void IsWithinTest()
    {
        UIBaseWindow parent = new UIBaseWindow();
        SceneUI.AddChild(parent);

        UIBaseWindow child = new UIBaseWindow();
        parent.AddChild(child);

        Assert.True(child.IsWithin(parent));
        Assert.False(parent.IsWithin(child));

        UIBaseWindow otherParent = new UIBaseWindow();
        SceneUI.AddChild(otherParent);

        //UIBaseWindow relative = new UIBaseWindow();
        //parent.Name = "Parent";
        //relative.RelativeTo = "Parent";
        //otherParent.AddChild(relative);

        //Assert.True(relative.IsWithin(parent));
        //Assert.False(relative.IsWithin(otherParent));

        Assert.True(parent.IsWithin(SceneUI));
        Assert.True(child.IsWithin(SceneUI));
        Assert.True(otherParent.IsWithin(SceneUI));
    }

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