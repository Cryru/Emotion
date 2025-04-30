#nullable enable

#region Using

#if CSHARP_SCRIPTING
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
#endif
using System.Reflection;
using System.Threading.Tasks;
using Emotion.Common.Threading;
using Emotion.Editor.EditorHelpers;
using Emotion.Editor.EditorWindows;
using Emotion.Game.Text;
using Emotion.Game.World.Editor.Actions;
using Emotion.Game.World.Prefab;
using Emotion.Game.World2D;
using Emotion.Game.World3D;
using Emotion.IO;
using Emotion.Platform.Implementation.Win32;
using Emotion.UI;
using Emotion.Utility;
using Emotion.Game.Data;
using Emotion.WIPUpdates.One.EditorUI.Components;

#endregion

namespace Emotion.Game.World.Editor;

public abstract partial class WorldBaseEditor
{
    // Selection and MouseOver
    protected bool _canObjectSelect;

    // Interface
    protected Dictionary<BaseGameObject, MapEditorObjectNameplate>? _namePlates;

    private UIBaseWindow? _bottomBar;
    protected MapEditorLabel? _bottomBarText;

    protected void InitializeEditorInterface()
    {
        //_editUI = new UIController(KeyListenerType.EditorUI)
        //{
        //    Id = "WorldEditor"
        //};

        UIBaseWindow topBar = GetEditorTopBar();
        _editUI.AddChild(topBar);

        UIBaseWindow bottomBar = GetEditorBottomBar();
        _bottomBar = bottomBar;
        _bottomBarText = (MapEditorLabel)bottomBar.GetWindowById("Label")!;
        _editUI.AddChild(bottomBar);

        UIBaseWindow worldInspect = GetWorldAttachInspectWindow();
        _editUI.AddChild(worldInspect);

        //_setControllersToVisible = UIController.GetControllersLesserPriorityThan(KeyListenerType.Editor);
        //for (var i = 0; i < _setControllersToVisible.Count; i++)
        //{
        //    _setControllersToVisible[i].SetVisible(false);
        //}
    }

    protected void DisposeEditorInterface()
    {
        if (_setControllersToVisible != null)
        {
            for (var i = 0; i < _setControllersToVisible.Count; i++)
            {
                _setControllersToVisible[i].SetVisible(true);
            }

            _setControllersToVisible = null;
        }

        _editUI!.Dispose();
        _editUI = null;
    }

    private UIBaseWindow GetEditorTopBar()
    {
        BaseMap? map = CurrentMap;

        var topBar = new UISolidColor();
        topBar.MaxSizeY = 15;
        topBar.ScaleMode = UIScaleMode.FloatScale;
        topBar.WindowColor = MapEditorColorPalette.BarColor;
        topBar.Id = "EditorTopBar";
        topBar.UseNewLayoutSystem = true;
        topBar.HandleInput = true;

        var mapName = new UIText();
        mapName.ParentAnchor = UIAnchor.CenterRight;
        mapName.Anchor = UIAnchor.CenterRight;
        mapName.ScaleMode = UIScaleMode.FloatScale;
        mapName.WindowColor = MapEditorColorPalette.TextColor;
        mapName.Text = map == null ? "No map loaded." : $"{map.MapName} @ {map.FileName ?? "Unsaved"}";
        mapName.FontFile = FontAsset.DefaultBuiltInFontName;
        mapName.FontSize = 6;
        mapName.Margins = new Rectangle(0, 0, 5, 0);
        topBar.AddChild(mapName);

        var topBarList = new UIBaseWindow();
        topBarList.ScaleMode = UIScaleMode.FloatScale;
        topBarList.LayoutMode = LayoutMode.HorizontalList;
        topBarList.ListSpacing = new Vector2(3, 0);
        topBarList.Margins = new Rectangle(3, 0, 3, 0);
        topBarList.Paddings = new Rectangle(0, 3, 0, 3);
        topBarList.AnchorAndParentAnchor = UIAnchor.CenterLeft;
        topBarList.Id = "List";
        topBar.AddChild(topBarList);

        var accent = new UISolidColor();
        accent.WindowColor = MapEditorColorPalette.ActiveButtonColor;
        accent.MaxSizeY = 1;
        accent.Anchor = UIAnchor.BottomLeft;
        accent.ParentAnchor = UIAnchor.BottomLeft;
        topBar.AddChild(accent);

        EditorAttachTopBarButtons(topBarList);

        return topBar;
    }

    private UIBaseWindow GetEditorBottomBar()
    {
        var bottomBar = new UISolidColor();
        bottomBar.UseNewLayoutSystem = true;
        bottomBar.GrowY = false;
        bottomBar.ScaleMode = UIScaleMode.FloatScale;
        bottomBar.WindowColor = MapEditorColorPalette.BarColor;
        bottomBar.Id = "BottomBar";
        bottomBar.Anchor = UIAnchor.BottomLeft;
        bottomBar.ParentAnchor = UIAnchor.BottomLeft;

        var label = new MapEditorLabel("");
        label.Margins = new Rectangle(3, 3, 3, 3);
        label.FontSize = MapEditorColorPalette.EditorButtonTextSize - 2;
        label.ParentAnchor = UIAnchor.CenterLeft;
        label.Anchor = UIAnchor.CenterLeft;
        label.Id = "Label";
        bottomBar.AddChild(label);

        var bottomBarLogTextContainer = new UIBaseWindow();
        bottomBarLogTextContainer.Anchor = UIAnchor.BottomLeft;
        bottomBarLogTextContainer.ParentAnchor = UIAnchor.TopLeft;
        bottomBarLogTextContainer.LayoutMode = LayoutMode.VerticalList;
        bottomBarLogTextContainer.Id = "LogContainer";
        bottomBarLogTextContainer.GrowY = false;
        bottomBar.AddChild(bottomBarLogTextContainer);

        return bottomBar;
    }

    private class CreateNewMapEnvelope
    {
        public string? Name;
        public string? FilePath;
    }

    protected virtual void EditorAttachTopBarButtons(UIBaseWindow parentList)
    {
        BaseMap? map = CurrentMap;
        Type mapType = _mapType;

        EditorButton fileMenu = EditorDropDownButton("Map", new[]
        {
            new EditorDropDownItem
            {
                Name = "New",
                Click = (_, __) =>
                {
                    var createMapModal = new PropertyInputModal<CreateNewMapEnvelope>(data =>
                    {
                        if (string.IsNullOrEmpty(data.FilePath) || string.IsNullOrEmpty(data.Name)) return false;

                        string fileName = data.FilePath;
                        if (!fileName.EndsWith(".xml")) fileName += ".xml";

                        string name = data.Name;
                        if (name.EndsWith(".xml")) name = name.Replace(".xml", "");

                        var newMap = (BaseMap) Activator.CreateInstance(_mapType, true)!;
                        newMap.MapName = data.Name;
                        newMap.FileName = fileName;

                        newMap.EditorCreateInitialize();

                        EditorSaveMap(newMap);
                        ChangeSceneMap(newMap);
                        EditorUtility.RegisterAssetAsCopyNewerInProjectFile($"Assets\\{fileName.Replace("/", "\\")}");

                        return true;
                    }, "", "New Map", "Create");

                    _editUI!.AddChild(createMapModal);
                }
            },
            new EditorDropDownItem
            {
                Name = "Open",
                Click = (_, __) =>
                {
                    var filePicker = new EditorFileExplorer<XMLAsset<BaseMap>>(
                        asset => { ChangeSceneMap(asset.Content!); },
                        assetName =>
                        {
                            // Verify if the xml file is of the editor map type.
                            if (!assetName.Contains(".xml")) return false;

                            string xmlTag;
                            if (_mapType != typeof(Map2D) && _mapType.IsAssignableTo(typeof(Map2D)))
                            {
                                xmlTag = "<Map2D";
                            }
                            else if (_mapType != typeof(Map3D) && _mapType.IsAssignableTo(typeof(Map3D)))
                            {
                                xmlTag = "<Map3D";
                            }
                            else
                            {
                                string mapTypeName = _mapType.FullName ?? "";
                                xmlTag = $"<BaseMap type=\"{mapTypeName}\"";
                            }

                            var assetLoaded = Engine.AssetLoader.Get<TextAsset>(assetName, false);
                            return assetLoaded?.Content != null && assetLoaded.Content.Contains(xmlTag);
                        }
                    );

                    _editUI!.AddChild(filePicker);
                }
            },
            new EditorDropDownItem
            {
                Name = "Save (Ctrl+S)",
                Click = (_, __) => { Task.Run(() => EditorSaveMap()); },
                Enabled = () => map?.FileName != null
            },
            // todo: generic text input or explorer ui
            //new EditorDropDownButtonDescription
            //{
            //	Name = "Save As"
            //}
            new EditorDropDownItem
            {
                Name = "Reload",
                Click = (_, __) => Task.Run(map!.Reset),
                Enabled = () => map != null
            },
            new EditorDropDownItem
            {
                Name = "Reset From File",
                Click = (_, __) => Task.Run(() => ChangeSceneMap(map!.FileName!)), // todo: pending changes
                Enabled = () => map?.FileName != null
            },
            new EditorDropDownItem
            {
                Name = "Properties",
                Click = (_, __) =>
                {
                    AssertNotNull(map);
                    var panel = new GenericPropertiesEditorPanel(map);
                    panel.OnPropertyEdited = (propName, propVal) =>
                    {
                        // todo: special case for reload, move somehow to world2d files
                        if (map is Map2D map2D && propName == "MapSize")
                        {
                            map2D.Tiles.InitRuntimeState(map2D).Wait();
                        }
                    };
                    _editUI!.AddChild(panel);
                },
                Enabled = () => map != null
            },
        });

        Type[]? dataTypes = GameDatabase.GetDataTypes();
        var dataEditors = new EditorDropDownItem[dataTypes?.Length ?? 0];
        for (var i = 0; i < dataEditors.Length; i++)
        {
            AssertNotNull(dataTypes);
            Type type = dataTypes[i];

            var editor = new EditorDropDownItem
            {
                Name = $"{type.Name} Editor",
                Click = (_, __) =>
                {
                    var editor = DataEditorGeneric.CreateEditorInstanceForType(type);
                    _editorUIAlways!.AddChild(editor);
                }
            };
            dataEditors[i] = editor;
        }

        EditorButton dataEditorsButton = EditorDropDownButton("GameData", dataEditors);

        string GetObjectSelectionLabel()
        {
            return $"Selection: {(_canObjectSelect ? "Enabled" : "Disabled")}";
        }

        EditorButton objectsMenu = EditorDropDownButton("Objects", new[]
        {
            // true by default, mouseover shows props
            // click selects the obj and shows prop editor window
            // alt switch between overlapping objects
            new EditorDropDownItem
            {
                NameFunc = GetObjectSelectionLabel,
                Click = (_, button) => { SetObjectSelectionEnabled(!_canObjectSelect); }
            },
            new EditorDropDownItem
            {
                Name = "Object Filters",
                Click = (_, __) => { }
            },
            new EditorDropDownItem
            {
                Name = "View Object List",
                Click = (_, __) =>
                {
                    AssertNotNull(map);

                    var panel = new EditorListOfItemsPanel<BaseGameObject>(
                        "All Objects",
                        map.ObjectsGet(null),
                        EditorOpenPropertiesPanelForObject,
                        obj => { RolloverObjects(new List<BaseGameObject> {obj}); }
                    )
                    {
                        Id = "ObjectListPanel"
                    };
                    _editUI!.AddChild(panel);
                },
                Enabled = () => map != null
            },
            // Object creation dialog
            new EditorDropDownItem
            {
                Name = "Add Object",
                Click = (_, __) =>
                {
                    AssertNotNull(map);

                    List<Type> objectTypes = map.GetValidObjectTypes();

                    var panel = new EditorListOfItemsPanel<Type>("Add Object", objectTypes, EditorAddObject);
                    panel.Text = "These are all classes with parameterless constructors\nthat inherit GameObject2D.\nChoose class of object to add:";
                    panel.CloseOnClick = true;

                    _editUI!.AddChild(panel);
                },
                Enabled = () => map != null
            },
            new EditorDropDownItem
            {
                Name = "Add From Prefab",
                Click = (_, __) =>
                {
                    var panel = new EditorListOfItemsPanel<GameObjectPrefab>("Prefab Library", _prefabDatabase.Values, PlaceObjectFromPrefab);
                    panel.Text = "Choose prefab to add as a new object:";
                    panel.CloseOnClick = true;

                    _editUI!.AddChild(panel);
                },
                Enabled = () => map != null && _prefabDatabase.Count > 0
            }
        });

        EditorButton editorMenu = EditorDropDownButton("Editor", new[]
        {
            // Shows actions done in the editor, can be undone
            new EditorDropDownItem
            {
                Name = "Undo History",
                Click = (_, __) =>
                {
                    var panel = new EditorListOfItemsPanel<IWorldEditorAction>("Actions", _actions, obj => { });
                    _editUI!.AddChild(panel);
                }
            },
            new EditorDropDownItem
            {
                Name = "3D Mesh Viewer",
                Click = (_, __) =>
                {
                    var panel = new ModelViewer();
                    _editUI!.AddChild(panel);
                },
            },
            new EditorDropDownItem
            {
                Name = "UI Studio",
                Click = (_, __) =>
                {
                    var panel = new InterfaceEditor();
                    _editorUIAlways!.AddChild(panel);
                },
            },
        });

        EditorButton otherTools = EditorDropDownButton("Other", new[]
        {
            new EditorDropDownItem
            {
                Name = "Open Folder",
                Click = (_, __) => { Process.Start("explorer.exe", "."); },
                Enabled = () => Engine.Host is Win32Platform
            },

            new EditorDropDownItem
            {
                Name = "Performance Monitor",
                Click = (_, __) =>
                {
                    var panel = new PerformanceMonitor();
                    _editorUIAlways!.AddChild(panel);
                },
            },
#if CSHARP_SCRIPTING
            new EditorDropDownItem
            {
                Name = "Eval Code",
                Click = (_, __) =>
                {
                    var input = new PropertyInputModal<StringInputModalEnvelope>(input =>
                    {
                        string text = input.Text;
                        if (text.Length < 1) return false;

                        EditorMsg($"Evaluating code: {text}");
                        try
                        {
                            var options = ScriptOptions.Default;
                            options = options.AddReferences(Helpers.AssociatedAssemblies);
                            for (var i = 0; i < Helpers.AssociatedAssemblies.Length; i++)
                            {
                                Assembly? assembly = Helpers.AssociatedAssemblies[i];
                                Type[] allTypes = assembly.GetTypes();
                                for (var j = 0; j < allTypes.Length; j++)
                                {
                                    var type = allTypes[j];
                                    if (type.Namespace == null) continue;
                                    options = options.AddImports(type.Namespace);
                                }
                            }

                            var result = CSharpScript.EvaluateAsync(text, options).Result;
                            if (result == null)
                            {
                                EditorMsg("Code evaluation returned null");
                                return true;
                            }

                            GenericPropertiesEditorPanel panel = new GenericPropertiesEditorPanel(result);
                            panel.Header = text;
                            GLThread.ExecuteOnGLThreadAsync(_editorUIAlways!.AddChild, panel);
                        }
                        catch (Exception e)
                        {
                            EditorMsg($"Code evaluation error: {e.Message}");
                            return false;
                        }

                        return true;
                    }, "Input Code", "Evaluate Code", "Evaluate");
                    _editUI!.AddChild(input);
                }
            },
#endif
        });

        parentList.AddChild(fileMenu);
        parentList.AddChild(dataEditorsButton);
        parentList.AddChild(objectsMenu);
        parentList.AddChild(editorMenu);
        parentList.AddChild(otherTools);

        foreach (var gameSpecificTool in _gameSpecificTopBarItems)
        {
            var button = EditorDropDownButton(gameSpecificTool.Key, gameSpecificTool.Value(this));
            parentList.AddChild(button);
        }
    }

    protected EditorButton EditorDropDownButton(string label, EditorDropDownItem[] menuButtons)
    {
        // todo: maybe the drop down exclusivity logic should be handled by the top bar or some kind of parent ui.
        // though either way a SetDropDownMode function will need to exist on buttons to change their style.

        var button = new EditorButton();

        void SpawnDropDown()
        {
            bool openOnMe = _editUI?.DropDown?.OwningObject == button;
            _editUI?.RemoveChild(_editUI.DropDown);
            if (openOnMe) return;

            // Make sure we don't display an empty dropdown.
            if (menuButtons.Length == 0)
                menuButtons = new[]
                {
                    new EditorDropDownItem
                    {
                        Name = "None",
                        Enabled = () => false
                    }
                };

            var dropDownWin = new EditorDropDown(true)
            {
                ParentAnchor = UIAnchor.BottomLeft,
                OwningObject = button,
                RelativeTo = $"button{button.Text}"
            };
            dropDownWin.SetItems(menuButtons);

            List<UIBaseWindow> siblings = button.Parent!.Children!;
            for (var i = 0; i < siblings.Count; i++)
            {
                UIBaseWindow child = siblings[i];
                if (child is EditorButton but) but.SetActiveMode(child == button);
            }

            dropDownWin.OnCloseProxy = () => button.SetActiveMode(false);

            _editUI!.AddChild(dropDownWin);
        }

        button.Text = label;
        button.OnMouseEnterProxy = _ =>
        {
            if (_editUI?.DropDown != null && _editUI.DropDown?.OwningObject != button && _editUI.DropDown?.OwningObject is EditorButton)
                SpawnDropDown();
        };
        button.OnClickedProxy = _ => SpawnDropDown();
        button.Id = $"button{label}";

        return button;
    }

    private UIBaseWindow GetWorldAttachInspectWindow()
    {
        // todo: Move this window spawning to the class V
        var worldAttachUI = new MapEditorInfoWorldAttach
        {
            ScaleMode = UIScaleMode.FloatScale,
            Id = "WorldAttach",
            Visible = false
        };

        var worldAttachBg = new UISolidColor
        {
            WindowColor = Color.Black * 0.7f,
            StretchX = true,
            StretchY = true,
            Paddings = new Rectangle(3, 3, 3, 3),
            ScaleMode = UIScaleMode.FloatScale
        };
        worldAttachUI.AddChild(worldAttachBg);

        var txt = new UIText
        {
            ScaleMode = UIScaleMode.FloatScale,
            WindowColor = MapEditorColorPalette.TextColor,
            Id = "text",
            FontFile = FontAsset.DefaultBuiltInFontName,
            FontSize = MapEditorColorPalette.EditorButtonTextSize,
            TextShadow = Color.Black,
            TextHeightMode = GlyphHeightMeasurement.NoMinY,
            IgnoreParentColor = true
        };
        worldAttachBg.AddChild(txt);

        return worldAttachUI;
    }

    #region Game Specific Items API

    private static Dictionary<string, Func<WorldBaseEditor, EditorDropDownItem[]>> _gameSpecificTopBarItems = new();

    public static void GameAddTopBarCategory(string title, Func<WorldBaseEditor, EditorDropDownItem[]> itemsGenFunc)
    {
        _gameSpecificTopBarItems.Add(title, itemsGenFunc);
    }

    #endregion
}