#nullable enable

using Emotion.Game.Systems.UI;
using Emotion.Game.Systems.UI2;
using Emotion.Primitives;
using Emotion.Testing;
using System.Collections;

namespace Tests.EngineTests;

public class NewUITestsGrids : NewUITests
{
    [Test]
    public IEnumerator GridBasic3x2()
    {
        var gridContainer = new UIBaseWindow
        {
            Visuals =
            {
                BackgroundColor = Color.PrettyPurple
            },
            Layout =
            {
                LayoutMethod = UILayoutMethod.Grid_FixedColumns(3, 5, 5),
                Padding = new UISpacing(10, 10, 10, 10),
                SizingX = UISizing.Fit(),
                SizingY = UISizing.Fit()
            }
        };
        SceneUI.AddChild(gridContainer);

        for (int i = 0; i < 6; i++)
        {
            var cell = new UIBaseWindow
            {
                Visuals =
                {
                    BackgroundColor = Color.White
                },
                Layout =
                {
                    MinSize = new IntVector2(50),
                    SizingX = UISizing.Grow(),
                    SizingY = UISizing.Grow()
                }
            };
            gridContainer.AddChild(cell);
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridBasic3x2));

        gridContainer.Layout.SizingX = UISizing.Grow();

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridBasic3x2));

        gridContainer.Layout.SizingY = UISizing.Grow();

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridBasic3x2));

        // For fun lets also compare a list and a grid with a single row
        // Should look and behave the same
        SceneUI.ClearChildren();

        gridContainer = new UIBaseWindow
        {
            Visuals =
            {
                BackgroundColor = Color.PrettyPurple
            },
            Layout =
            {
                LayoutMethod = UILayoutMethod.Grid_FixedColumns(3, 5, 5),
                Padding = new UISpacing(10, 10, 10, 10),
                SizingX = UISizing.Fit(),
                SizingY = UISizing.Fit()
            }
        };
        SceneUI.AddChild(gridContainer);

        for (int i = 0; i < 3; i++)
        {
            var cell = new UIBaseWindow
            {
                Visuals =
                {
                    BackgroundColor = Color.White
                },
                Layout =
                {
                    MinSize = new IntVector2(50),
                    SizingX = UISizing.Grow(),
                    SizingY = UISizing.Grow()
                }
            };
            gridContainer.AddChild(cell);
        }

        var list = new UIBaseWindow
        {
            Visuals =
            {
                BackgroundColor = Color.PrettyPurple
            },
            Layout =
            {
                LayoutMethod = UILayoutMethod.HorizontalList(5),
                Padding = new UISpacing(10, 10, 10, 10),
                SizingX = UISizing.Fit(),
                SizingY = UISizing.Fit(),
                Margins = new UISpacing(0, 100, 0, 0)
            }
        };
        SceneUI.AddChild(list);

        for (int i = 0; i < 3; i++)
        {
            var cell = new UIBaseWindow
            {
                Visuals =
                {
                    BackgroundColor = Color.White
                },
                Layout =
                {
                    MinSize = new IntVector2(50),
                    SizingX = UISizing.Grow(),
                    SizingY = UISizing.Grow()
                }
            };
            list.AddChild(cell);
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridBasic3x2));

        gridContainer.Layout.SizingX = UISizing.Grow();
        list.Layout.SizingX = UISizing.Grow();

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridBasic3x2));
    }

    [Test]
    public IEnumerator GridBasic2x3()
    {
        var gridContainer = new UIBaseWindow
        {
            Visuals =
            {
                BackgroundColor = Color.PrettyPurple
            },
            Layout =
            {
                LayoutMethod = UILayoutMethod.Grid_FixedRows(3, 5, 5),
                Padding = new UISpacing(10, 10, 10, 10),
                SizingX = UISizing.Fit(),
                SizingY = UISizing.Fit()
            }
        };
        SceneUI.AddChild(gridContainer);

        for (int i = 0; i < 6; i++)
        {
            var cell = new UIBaseWindow
            {
                Visuals =
                {
                    BackgroundColor = Color.White
                },
                Layout =
                {
                    MinSize = new IntVector2(50),
                    SizingX = UISizing.Grow(),
                    SizingY = UISizing.Grow()
                }
            };
            gridContainer.AddChild(cell);
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridBasic2x3));

        gridContainer.Layout.SizingX = UISizing.Grow();

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridBasic2x3));

        gridContainer.Layout.SizingY = UISizing.Grow();

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridBasic2x3));

        // For fun lets also compare a list and a grid with a single row
        // Should look and behave the same
        SceneUI.ClearChildren();

        gridContainer = new UIBaseWindow
        {
            Visuals =
            {
                BackgroundColor = Color.PrettyPurple
            },
            Layout =
            {
                LayoutMethod = UILayoutMethod.Grid_FixedRows(3, 5, 5),
                Padding = new UISpacing(10, 10, 10, 10),
                SizingX = UISizing.Fit(),
                SizingY = UISizing.Fit()
            }
        };
        SceneUI.AddChild(gridContainer);

        for (int i = 0; i < 3; i++)
        {
            var cell = new UIBaseWindow
            {
                Visuals =
                {
                    BackgroundColor = Color.White
                },
                Layout =
                {
                    MinSize = new IntVector2(50),
                    SizingX = UISizing.Grow(),
                    SizingY = UISizing.Grow()
                }
            };
            gridContainer.AddChild(cell);
        }

        var list = new UIBaseWindow
        {
            Visuals =
            {
                BackgroundColor = Color.PrettyPurple
            },
            Layout =
            {
                LayoutMethod = UILayoutMethod.VerticalList(5),
                Padding = new UISpacing(10, 10, 10, 10),
                SizingX = UISizing.Fit(),
                SizingY = UISizing.Fit(),
                Margins = new UISpacing(0, 300, 0, 0)
            }
        };
        SceneUI.AddChild(list);

        for (int i = 0; i < 3; i++)
        {
            var cell = new UIBaseWindow
            {
                Visuals =
                {
                    BackgroundColor = Color.White
                },
                Layout =
                {
                    MinSize = new IntVector2(50),
                    SizingX = UISizing.Grow(),
                    SizingY = UISizing.Grow()
                }
            };
            list.AddChild(cell);
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridBasic2x3));

        gridContainer.Layout.SizingX = UISizing.Grow();
        list.Layout.SizingX = UISizing.Grow();

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridBasic2x3));
    }

    //[Test]
    //public IEnumerator GridAutoWrap()
    //{
    //    var gridContainer = new UIBaseWindow
    //    {
    //        Visuals =
    //        {
    //            BackgroundColor = Color.PrettyPurple
    //        },
    //        Layout =
    //        {
    //            LayoutMethod = UILayoutMethod.Grid_Auto(5, 5),
    //            Padding = new UISpacing(10, 10, 10, 10),
    //            SizingX = UISizing.Fixed(220),
    //            SizingY = UISizing.Fixed(140)
    //        }
    //    };
    //    SceneUI.AddChild(gridContainer);

    //    for (int i = 0; i < 5; i++)
    //    {
    //        var cell = new UIBaseWindow
    //        {
    //            Visuals =
    //            {
    //                BackgroundColor = Color.White
    //            },
    //            Layout =
    //            {
    //                MinSize = new IntVector2(50),
    //                SizingX = UISizing.Fit(),
    //                SizingY = UISizing.Fit()
    //            }
    //        };
    //        gridContainer.AddChild(cell);
    //    }

    //    yield return WaitUILayout();
    //    yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridAutoWrap));

    //    for (int i = 0; i < 50; i++)
    //    {
    //        var cell = new UIBaseWindow
    //        {
    //            Visuals =
    //            {
    //                BackgroundColor = Color.White
    //            },
    //            Layout =
    //            {
    //                MinSize = new IntVector2(50),
    //                SizingX = UISizing.Fit(),
    //                SizingY = UISizing.Fit()
    //            }
    //        };
    //        gridContainer.AddChild(cell);
    //    }

    //    yield return WaitUILayout();
    //    yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridAutoWrap));
    //}

    [Test]
    public IEnumerator GridUniformRows()
    {
        var gridContainer = new UIBaseWindow
        {
            Visuals =
            {
                BackgroundColor = Color.PrettyPurple
            },
            Layout =
            {
                LayoutMethod = UILayoutMethod.Grid_FixedColumns(3, 5, 5, true),
                Padding = new UISpacing(10, 10, 10, 10),
                SizingX = UISizing.Fit(),
                SizingY = UISizing.Fit()
            }
        };
        SceneUI.AddChild(gridContainer);

        // Create cells with varying heights
        int[] heights = { 30, 60, 40, 50, 70, 45 };
        for (int i = 0; i < 6; i++)
        {
            var cell = new UIBaseWindow
            {
                Visuals =
                {
                    BackgroundColor = Color.PrettyOrange
                },
                Layout =
                {
                    MinSizeX = 50,
                    SizingX = UISizing.Grow(),
                    SizingY = UISizing.Fixed(heights[i])
                }
            };
            gridContainer.AddChild(cell);
        }

        // Since children are fixed there should be no change in their height
        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridUniformRows));

        gridContainer.Layout.SizingX = UISizing.Grow();

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridUniformRows));

        gridContainer.Layout.SizingY = UISizing.Grow();

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridUniformRows));

        // But if they were grow/fit

        foreach (var item in gridContainer)
        {
            item.Layout.MinSizeY = item.Layout.SizingY.Size;
            item.Layout.SizingY = UISizing.Grow();
        }

        gridContainer.Layout.SizingX = UISizing.Fit();
        gridContainer.Layout.SizingY = UISizing.Fit();

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridUniformRows));

        gridContainer.Layout.SizingX = UISizing.Grow();

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridUniformRows));

        gridContainer.Layout.SizingY = UISizing.Grow();

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridUniformRows));
    }

    [Test]
    public IEnumerator GridUniformColumns()
    {
        var gridContainer = new UIBaseWindow
        {
            Visuals =
            {
                BackgroundColor = Color.PrettyPurple
            },
            Layout =
            {
                LayoutMethod = UILayoutMethod.Grid_FixedColumns(3, 5, 5, true),
                Padding = new UISpacing(10, 10, 10, 10),
                SizingX = UISizing.Fit(),
                SizingY = UISizing.Fit()
            }
        };
        SceneUI.AddChild(gridContainer);

        // Create cells with varying widths
        int[] widths = { 40, 70, 50, 60, 80, 45 };
        for (int i = 0; i < 6; i++)
        {
            var cell = new UIBaseWindow
            {
                Visuals =
                {
                    BackgroundColor = Color.PrettyGreen
                },
                Layout =
                {
                    MinSizeY = 50,
                    SizingY = UISizing.Grow(),
                    SizingX = UISizing.Fixed(widths[i])
                }
            };
            gridContainer.AddChild(cell);
        }

        // Since children are fixed there should be no change in their height
        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridUniformColumns));

        gridContainer.Layout.SizingX = UISizing.Grow();

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridUniformColumns));

        gridContainer.Layout.SizingY = UISizing.Grow();

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridUniformColumns));

        // But if they were grow/fit

        foreach (var item in gridContainer)
        {
            item.Layout.MinSizeX = item.Layout.SizingX.Size;
            item.Layout.SizingX = UISizing.Grow();
        }

        gridContainer.Layout.SizingX = UISizing.Fit();
        gridContainer.Layout.SizingY = UISizing.Fit();

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridUniformColumns));

        gridContainer.Layout.SizingX = UISizing.Grow();

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridUniformColumns));

        gridContainer.Layout.SizingY = UISizing.Grow();

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridUniformColumns));
    }

    [Test]
    public IEnumerator GridUniformBoth()
    {
        var gridContainer = new UIBaseWindow
        {
            Visuals =
            {
                BackgroundColor = Color.PrettyPurple
            },
            Layout =
            {
                LayoutMethod = UILayoutMethod.Grid_FixedColumns(3, 5, 5, true, true),
                Padding = new UISpacing(10, 10, 10, 10),
                SizingX = UISizing.Fit(),
                SizingY = UISizing.Fit()
            }
        };
        SceneUI.AddChild(gridContainer);

        int[] widths = { 40, 70, 50, 60, 80, 45 };
        int[] heights = { 30, 60, 40, 50, 70, 45 };
        for (int i = 0; i < 6; i++)
        {
            var cell = new UIBaseWindow
            {
                Visuals =
                {
                    BackgroundColor = Color.PrettyPink
                },
                Layout =
                {
                    MinSizeX = widths[i],
                    MinSizeY = heights[i]
                }
            };
            gridContainer.AddChild(cell);
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridUniformBoth));

        gridContainer.Layout.SizingX = UISizing.Grow();

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridUniformBoth));

        gridContainer.Layout.SizingY = UISizing.Grow();

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridUniformBoth));
    }

    [Test]
    public IEnumerator GridPartialRow()
    {
        var gridContainer = new UIBaseWindow
        {
            Visuals =
            {
                BackgroundColor = Color.PrettyPurple
            },
            Layout =
            {
                LayoutMethod = UILayoutMethod.Grid_FixedColumns(columnCount: 4, cellSpacingX: 5, cellSpacingY: 5),
                Padding = new UISpacing(10, 10, 10, 10),
                SizingX = UISizing.Fit(),
                SizingY = UISizing.Fit()
            }
        };
        SceneUI.AddChild(gridContainer);

        // 5 items in 4-column grid (partial last row)
        for (int i = 0; i < 5; i++)
        {
            var cell = new UIBaseWindow
            {
                Visuals =
                {
                    BackgroundColor = Color.PrettyYellow
                },
                Layout =
                {
                    MinSizeX = 50,
                    MinSizeY = 50
                }
            };
            gridContainer.AddChild(cell);
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridPartialRow));
    }

    [Test]
    public IEnumerator GridWithMargins()
    {
        var gridContainer = new UIBaseWindow
        {
            Visuals =
            {
                BackgroundColor = Color.PrettyPurple
            },
            Layout =
            {
                LayoutMethod = UILayoutMethod.Grid_FixedColumns(2, 8, 8),
                Padding = new UISpacing(15, 15, 15, 15),
                SizingX = UISizing.Fit(),
                SizingY = UISizing.Fit()
            }
        };
        SceneUI.AddChild(gridContainer);

        for (int i = 0; i < 4; i++)
        {
            var cell = new UIBaseWindow
            {
                Visuals =
                {
                    BackgroundColor = Color.PrettyBlue
                },
                Layout =
                {
                    MinSizeX = 50,
                    MinSizeY = 50,
                    Margins = new UISpacing(5, 5, 5, 5)
                }
            };
            gridContainer.AddChild(cell);
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridWithMargins));

        gridContainer.Layout.SizingX = UISizing.Grow();
        gridContainer.Layout.LayoutMethod = UILayoutMethod.Grid_FixedColumns(2, 8, 8, true, true);

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridWithMargins));
    }

    [Test]
    public IEnumerator GridSingleColumn()
    {
        var gridContainer = new UIBaseWindow
        {
            Visuals =
            {
                BackgroundColor = Color.PrettyPurple
            },
            Layout =
            {
                LayoutMethod = UILayoutMethod.Grid_FixedColumns(1, 0, 5),
                Padding = new UISpacing(10, 10, 10, 10),
                SizingX = UISizing.Fit(),
                SizingY = UISizing.Fit()
            }
        };
        SceneUI.AddChild(gridContainer);

        for (int i = 0; i < 4; i++)
        {
            var cell = new UIBaseWindow
            {
                Visuals =
                {
                    BackgroundColor = Color.PrettyGreen
                },
                Layout =
                {
                    MinSizeX = 60,
                    MinSizeY = 40
                }
            };
            gridContainer.AddChild(cell);
        }

        yield return WaitUILayout();
        yield return VerifyScreenshot(nameof(NewUITestsGrids), nameof(GridSingleColumn));
    }
}
