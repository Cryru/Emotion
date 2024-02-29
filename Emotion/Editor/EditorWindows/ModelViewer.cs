#region Using

using System.Threading.Tasks;
using Emotion.Common.Threading;
using Emotion.Editor.EditorHelpers;
using Emotion.Editor.PropertyEditors;
using Emotion.Game.Animation3D;
using Emotion.Game.ThreeDee;
using Emotion.Game.World.Editor;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Game.World3D;
using Emotion.Game.World3D.Objects;
using Emotion.Graphics.Camera;
using Emotion.Graphics.ThreeDee;
using Emotion.IO;
using Emotion.IO.MeshAssetTypes;
using Emotion.Platform.Input;
using Emotion.UI;
using Emotion.Utility;
using Extensions = Emotion.Utility.Extensions;

#endregion

#nullable enable

namespace Emotion.Editor.EditorWindows;

public class ModelViewer : EditorPanel
{
    private Camera3D _camera;
    private InfiniteGrid _grid;
    private Task _gridLoadingTask;
    private FrameBuffer? _renderBuffer;

    private UIBaseWindow? _surface3D;

    private EditorDropDownItem[] _noAnimationItems =
    {
        new()
        {
            Name = "No Animation"
        }
    };

    private bool _panelDragResize;
    private bool _renderSkeleton;

    private GameObject3D _obj;

    private class SpriteStackCreationEnvelope
    {
        public Vector2 TileSize;
    }

    public ModelViewer() : base("3D Mesh Viewer")
    {
        _camera = new Camera3D(new Vector3(-290, 250, 260));
        _camera.LookAtPoint(new Vector3(0, 0, 0));
        _grid = new InfiniteGrid();
        _gridLoadingTask = Task.Run(_grid.LoadAssetsAsync);
        _obj = new GameObject3D("ModelViewerDummy");
    }

    public override void AttachedToController(UIController controller)
    {
        GLThread.ExecuteGLThreadAsync(() =>
        {
            _renderBuffer = new FrameBuffer(new Vector2(1920, 1080)).WithColor().WithDepth();
            _renderBuffer.ColorAttachment.Smooth = true;
        });

        base.AttachedToController(controller);

        var contentSplit = new UIBaseWindow
        {
            LayoutMode = LayoutMode.HorizontalList,
            StretchX = true,
            StretchY = true
        };

        var surface3D = new UIBaseWindow
        {
            Id = "Surface3D",
            MinSize = new Vector2(960, 540) / 2f,
            StretchX = true,
            StretchY = true,
            HandleInput = true
        };
        contentSplit.AddChild(surface3D);
        _surface3D = surface3D;

        var editorButtons = new UIBaseWindow
        {
            StretchX = true,
            StretchY = true,
            MinSize = new Vector2(130, 0),
            MaxSize = new Vector2(130, DefaultMaxSizeF),
            LayoutMode = LayoutMode.VerticalList,
            ListSpacing = new Vector2(0, 2),
            Paddings = new Rectangle(2, 0, 2, 0)
        };

        var butObj = new EditorButton
        {
            Text = "Open MeshAsset",
            StretchY = true,
            StretchX = false,
            OnClickedProxy = _ => { Controller!.AddChild(new EditorFileExplorer<MeshAsset>(asset => { SetEntity(asset.Entity); })); }
        };
        editorButtons.AddChild(butObj);

        var butSprite = new EditorButton
        {
            Text = "Open Sprite Stack",
            StretchY = true,
            StretchX = false,
            OnClickedProxy = _ =>
            {
                Controller!.AddChild(new EditorFileExplorer<SpriteStackTexture>(asset =>
                {
                    var createMapModal = new PropertyInputModal<SpriteStackCreationEnvelope>(data =>
                    {
                        if (data.TileSize == Vector2.Zero) return false;

                        MeshEntity? entity = asset.GetSpriteStackEntity(data.TileSize);
                        if (entity != null)
                        {
                            SetEntity(entity);
                            return true;
                        }

                        return false;
                    }, "", "SpriteStack Entity", "Create");

                    Controller!.AddChild(createMapModal);
                }));
            }
        };
        editorButtons.AddChild(butSprite);

        var saveAsEm3Button = new EditorButton
        {
            Text = "Export as Em3",
            StretchY = true,
            StretchX = false,
            OnClickedProxy = _ =>
            {
                if (_obj.Entity == null) return;
                byte[]? data = EmotionMeshAsset.EntityToByteArray(_obj.Entity);
                Engine.AssetLoader.Save(data, $"Player/Em3Export/{_obj.Entity.Name}.em3");
            },
            Id = "ButtonExportEm3",
            Enabled = false
        };
        editorButtons.AddChild(saveAsEm3Button);

        var gridSizeEdit = new PropEditorNumber<float>();
        gridSizeEdit.SetValue(_grid.TileSize);
        gridSizeEdit.SetCallbackValueChanged(newVal => { _grid.TileSize = (float) newVal; });
        editorButtons.AddChild(new FieldEditorWithLabel("Grid Size: ", gridSizeEdit));

        var label = new MapEditorLabel("No model loaded");
        label.Id = "ModelLabel";
        editorButtons.AddChild(label);

        var editAnimationProps = new EditorButton
        {
            Text = "Edit Props",
            StretchY = true,
            StretchX = true,
            Id = "buttonEditProps",
            Enabled = false,
            OnClickedProxy = _ =>
            {
                AssertNotNull(_obj.Entity);
                var panel = new GenericPropertiesEditorPanel(_obj.Entity);
                Controller!.AddChild(panel);
            }
        };
        editorButtons.AddChild(editAnimationProps);

        var posEditor = new PropEditorFloat3(false);
        posEditor.SetValue(_obj.Position);
        posEditor.SetCallbackValueChanged(newVal => { _obj.Position = (Vector3) newVal; });
        editorButtons.AddChild(new FieldEditorWithLabel("Position: ", posEditor, LayoutMode.VerticalList));

        var rotEditor = new PropEditorFloat3(false);
        rotEditor.SetValue(_obj.RotationDeg);
        rotEditor.SetCallbackValueChanged(newVal => { _obj.RotationDeg = (Vector3) newVal; });
        editorButtons.AddChild(new FieldEditorWithLabel("Rotation: ", rotEditor, LayoutMode.VerticalList));

        var scaleEditor = new PropEditorFloat3(false);
        scaleEditor.SetValue(_obj.Size3D);
        scaleEditor.SetCallbackValueChanged(newVal => { _obj.Size3D = (Vector3) newVal; });
        editorButtons.AddChild(new FieldEditorWithLabel("Scale: ", scaleEditor, LayoutMode.VerticalList));

        var meshListProp = new EditorButtonDropDown
        {
            Text = "Meshes: ",
            Id = "MeshList",
            LayoutMode = LayoutMode.VerticalList
        };
        editorButtons.AddChild(meshListProp);

        var animationsList = new EditorButtonDropDown
        {
            Id = "Animations",
            Text = "Animation: ",
            LayoutMode = LayoutMode.VerticalList
        };
        animationsList.SetItems(_noAnimationItems, 0);
        _noAnimationItems[0].Click = SetAnimationDropDownCallback;
        editorButtons.AddChild(animationsList);

        var animButtonsContainer = new UIBaseWindow();
        animButtonsContainer.LayoutMode = LayoutMode.HorizontalList;
        animButtonsContainer.StretchX = true;
        animButtonsContainer.StretchY = true;
        animButtonsContainer.ListSpacing = new Vector2(2, 2);
        editorButtons.AddChild(animButtonsContainer);

        var mergeAnimation = new EditorButton
        {
            Text = "Import Animations",
            StretchY = true,
            StretchX = true,
            Id = "buttonImportAnim",
            Enabled = false,
            OnClickedProxy = _ =>
            {
                Controller!.AddChild(new EditorFileExplorer<MeshAsset>(asset =>
                {
                    var currentEntity = _obj.Entity;
                    if (currentEntity == null || currentEntity.Animations == null) return;

                    var assetEntity = asset.Entity;
                    if (assetEntity == null || assetEntity.Animations == null) return;

                    HashSet<string> takenNames = new HashSet<string>();
                    for (int a = 0; a < currentEntity.Animations.Length; a++)
                    {
                        var animCurrent = currentEntity.Animations[a];
                        takenNames.Add(animCurrent.Name);
                    }

                    for (int i = 0; i < assetEntity.Animations.Length; i++)
                    {
                        var anim = assetEntity.Animations[i];
                        anim.Name = Helpers.EnsureNoStringCollision(takenNames, anim.Name);
                    }

                    currentEntity.Animations = Extensions.JoinArrays(currentEntity.Animations, assetEntity.Animations);
                    UpdateAnimationList();
                }));
            }
        };
        animButtonsContainer.AddChild(mergeAnimation);

        var viewSkeleton = new PropEditorBool();
        viewSkeleton.SetValue(false);
        viewSkeleton.SetCallbackValueChanged(newVal => { _renderSkeleton = (bool) newVal; });
        editorButtons.AddChild(new FieldEditorWithLabel("Render Skeleton: ", viewSkeleton));

        contentSplit.AddChild(editorButtons);
        _contentParent.AddChild(contentSplit);

        // Dragging
        // todo: move to panel property
        var dragArea = new UITexture
        {
            TextureFile = "Editor/PanelDragArea.png",
            RenderSize = new Vector2(8, 8),
            Smooth = true,
            WindowColor = MapEditorColorPalette.ButtonColor
        };

        var dragButton = new UICallbackButton
        {
            StretchX = true,
            StretchY = true,
            OnMouseEnterProxy = _ => { dragArea.WindowColor = MapEditorColorPalette.ActiveButtonColor; },
            OnMouseLeaveProxy = _ => { dragArea.WindowColor = MapEditorColorPalette.ButtonColor; },
            OnClickedProxy = _ => { _panelDragResize = true; },
            OnClickedUpProxy = _ => { _panelDragResize = false; }
        };
        dragButton.AddChild(dragArea);
        dragButton.Anchor = UIAnchor.BottomRight;
        dragButton.ParentAnchor = UIAnchor.BottomRight;

        _container.AddChild(dragButton);
    }

    protected void SetEntity(MeshEntity? entity)
    {
        _obj.Entity = entity;

        var meshList = (EditorButtonDropDown?) GetWindowById("MeshList");
        if (meshList != null)
        {
            Mesh[] meshes = entity?.Meshes ?? Array.Empty<Mesh>();

            var checkboxItemList = new EditorDropDownCheckboxItem[meshes.Length];
            for (var i = 0; i < meshes.Length; i++)
            {
                Mesh mesh = meshes[i];
                int idx = i;
                checkboxItemList[i] = new EditorDropDownCheckboxItem
                {
                    Name = mesh.Name,
                    Checked = () => { return _obj.EntityMetaState!.RenderMesh[idx]; },
                    Click = (_, __) => { _obj.EntityMetaState!.RenderMesh[idx] = !_obj.EntityMetaState!.RenderMesh[idx]; }
                };
            }

            meshList.SetItems(checkboxItemList, 0);
            meshList.Text = entity == null ? "Meshes" : $"Meshes [{entity.Meshes.Length}]";
        }

        UpdateAnimationList();

        var label = (MapEditorLabel?) GetWindowById("ModelLabel");
        if (label != null) label.Text = $"Entity: {entity.Name}\nRadius: {_obj.BoundingSphere.Radius}";

        var exportButton = (EditorButton?) GetWindowById("ButtonExportEm3");
        if (exportButton != null) exportButton.Enabled = true;

        var editPropsButton = (EditorButton?) GetWindowById("buttonEditProps");
        if (editPropsButton != null) editPropsButton.Enabled = true;

        var importAnimButton = (EditorButton?) GetWindowById("buttonImportAnim");
        if (importAnimButton != null) importAnimButton.Enabled = entity?.Animations != null;
    }

    protected void UpdateAnimationList()
    {
        MeshEntity? entity = _obj.Entity;

        var animationList = (EditorButtonDropDown?) GetWindowById("Animations");
        if (animationList != null)
        {
            SkeletalAnimation[]? animations = entity?.Animations;
            if (animations != null)
            {
                var animationButtons = new EditorDropDownItem[animations.Length + 1];
                animationButtons[0] = _noAnimationItems[0];
                for (var i = 0; i < animations.Length; i++)
                {
                    SkeletalAnimation anim = animations[i];
                    animationButtons[i + 1] = new EditorDropDownItem
                    {
                        Name = anim.Name,
                        UserData = anim.Name,
                        Click = SetAnimationDropDownCallback
                    };
                }

                animationList.SetItems(animationButtons, 0);
            }
            else
            {
                animationList.SetItems(_noAnimationItems, 0);
            }
        }
    }

    protected void SetAnimationDropDownCallback(EditorDropDownItem item, EditorButton? _)
    {
        _obj.SetAnimation(item.Name);
    }

    public override void DetachedFromController(UIController controller)
    {
        GLThread.ExecuteGLThreadAsync(() =>
        {
            _renderBuffer?.Dispose();
            _renderBuffer = null;
        });
        base.DetachedFromController(controller);
    }

    public override bool OnKey(Key key, KeyStatus status, Vector2 mousePos)
    {
        if (Controller?.InputFocus == _surface3D)
        {
            _camera.CameraKeyHandler(key, status);
            return false;
        }

        return true;
    }

    protected override bool UpdateInternal()
    {
        _camera.Update();
        _obj.Update(Engine.DeltaTime);

        if (_panelDragResize && _surface3D != null)
        {
            Vector2 curMouse = Engine.Host.MousePosition;
            Rectangle r = Rectangle.FromMinMaxPoints(_surface3D.Position2 + new Vector2(100, 0) * GetScale(), curMouse);
            r.SnapToAspectRatio(16f / 9f);
            if (r.Size.X > 100 && r.Size.Y > 200)
            {
                _surface3D.MinSize = r.Size / GetScale();
                _surface3D.InvalidateLayout();
            }
        }

        return base.UpdateInternal();
    }

    protected override bool RenderInternal(RenderComposer c)
    {
        if (_renderBuffer == null) return true;

        CameraBase oldCamera = c.Camera;
        c.RenderTo(_renderBuffer);
        c.Camera = _camera;

        RenderState oldState = c.CurrentState.Clone();
        c.SetState(RenderState.Default);
        c.SetUseViewMatrix(false);
        c.RenderSprite(new Vector3(0, 0, 0), Engine.Renderer.CurrentTarget.Size, Color.CornflowerBlue);
        c.SetUseViewMatrix(true);
        c.ClearDepth();

        if (_gridLoadingTask.IsCompletedSuccessfully)
            _grid.Render(c);

        c.RenderLine(new Vector3(0, 0, 0), new Vector3(short.MaxValue, 0, 0), Color.Red, snapToPixel: false);
        c.RenderLine(new Vector3(0, 0, 0), new Vector3(0, short.MaxValue, 0), Color.Green, snapToPixel: false);
        c.RenderLine(new Vector3(0, 0, 0), new Vector3(0, 0, short.MaxValue), Color.Blue, snapToPixel: false);

        if (_renderSkeleton)
            _obj.DebugDrawSkeleton(c);
        else
            _obj.Render(c);

        c.RenderTo(null);

        c.SetState(oldState);
        c.Camera = oldCamera;

        base.RenderInternal(c);

        if (_surface3D != null)
            c.RenderSprite(_surface3D.Position, _surface3D.Size, _renderBuffer.Texture);

        return true;
    }
}