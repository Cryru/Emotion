using Emotion.Editor.EditorUI;
using Emotion.Editor.EditorUI.Components;
using Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.Editor.Tools.InterfaceTool;
using Emotion.Game.Systems.UI;
using Emotion.Game.World.TwoDee;

#nullable enable

namespace Emotion.Editor.Tools.SpriteEntityTool;

public class SpriteEntityEditor : TwoSplitEditorWindowFileSupport<UIViewport, ObjectPropertyWindow, SpriteEntity>
{
    private UIList _list = null!;

    private SpriteEntityMetaState? _entityMetaState;
    private SpriteAnimation? _selectedAnim;

    private float _animTime;
    private TypeEditor? _animTimeEditor;

    private bool _paused;
    private EditorButton? _pauseButton;

    public SpriteEntityEditor() : base("Sprite Entity Editor")
    {
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);
        NewFile();

        var contentParent = GetContentParent();

        var animationControlButtons = new UIBaseWindow()
        {
            LayoutMode = LayoutMode.HorizontalList,
            ListSpacing = new Vector2(5, 0),
            GrowY = false,
            OrderInParent = -1
        };
        contentParent.AddChild(animationControlButtons);

        var playPause = new EditorButton("Pause");
        playPause.OnClickedProxy = (_) => TogglePauseAnimation();
        animationControlButtons.AddChild(playPause);
        _pauseButton = playPause;

        var animTime = TypeEditor.CreateCustomWithLabel("Animation Time", _animTime, (v) =>
        {
            _animTime = v;
            if (!_paused) TogglePauseAnimation();
        });
        animationControlButtons.AddChild(animTime);
        _animTimeEditor = animTime.GetWindowById<TypeEditor>("Editor");
    }

    protected override UIViewport GetLeftSideContent()
    {
        var viewPort = new UIViewport()
        {
            IgnoreParentColor = true,
            Id = "Viewport",
            OnRender = RenderViewport,
            WindowColor = Color.CornflowerBlue
        };
        return viewPort;
    }

    protected override ObjectPropertyWindow GetRightSideContent()
    {
        var objProps = new ObjectPropertyWindow()
        {
            Id = "EntityData",
            IgnoreParentColor = true,
            MinSize = new Vector2(50)
        };
        return objProps;
    }

    protected override bool UpdateInternal()
    {
        if (_rightContent != null)
        {
            TypeEditor? animEditor = _rightContent.GetEditorForProperty(nameof(SpriteEntity.Animations));
            if (animEditor is ListEditor<SpriteAnimation> animEdit)
                _selectedAnim = animEdit.GetSelected();
        }

        if (_selectedAnim != null)
        {
            if (!_paused)
            {
                _animTime += Engine.DeltaTime;

                _animTimeEditor?.SetValue(_animTime % _entityMetaState?.GetCurrentAnimationTime() ?? 1);
            }

            _entityMetaState?.UpdateAnimation(_selectedAnim, _animTime, true);
        }

        return base.UpdateInternal();
    }

    protected void RenderViewport(UIBaseWindow win, Renderer c)
    {
        Vector3 center = (win.Size / 4f).ToVec3();
        center = center.Round();

        const float lineLength = 10;
        c.RenderSprite(center - new Vector3(0, lineLength, 0), new Vector2(1, lineLength * 2f), Color.Red);
        c.RenderSprite(center - new Vector3(lineLength, 0, 0), new Vector2(lineLength * 2f, 1), Color.Red);
        c.RenderSprite(center, new Vector2(1f, 1f), Color.PrettyYellow);

        if (ObjectBeingEdited != null)
        {
            AssertNotNull(_entityMetaState);
            c.RenderEntityStandalone(ObjectBeingEdited, _entityMetaState, center);
        }
    }

    protected override void CreateTopBarButtons(UIBaseWindow topBar)
    {
        base.CreateTopBarButtons(topBar);

        //EditorButton fileButton = new EditorButton("Animation");
        //fileButton.OnClickedProxy = (me) =>
        //{
        //    UIDropDown dropDown = EditorDropDown.OpenListDropdown(me);

        //    {
        //        EditorButton button = new EditorButton("Add Frame To Current (Single Texture)");
        //        button.GrowX = true;
        //        button.OnClickedProxy = (_) =>
        //        {
        //            Controller?.DropDown?.Close();

        //            ObjectPropertyWindow? entityData = _rightContent;
        //            TypeEditor? animEditor = entityData?.GetEditorForProperty(nameof(SpriteEntity.Animations));
        //            if (animEditor is ListEditor<SpriteAnimation> listEditor)
        //            {
        //                SpriteAnimation? selectedAnim = listEditor.GetSelected();
        //                if (selectedAnim != null)
        //                {
        //                    FilePicker<TextureAsset>.SelectFile(this, (file) =>
        //                    {
        //                        if (file == null) return;

        //                    });
        //                }
        //            }
        //        };
        //        dropDown.AddChild(button);
        //    }
        //};
        //topBar.AddChild(fileButton);
    }

    protected override void OnObjectBeingEditedChange(SpriteEntity? newObj)
    {
        EngineEditor.UnregisterForObjectChanges(this);
        if (newObj != null)
            EngineEditor.RegisterForObjectChanges(newObj, (_) => MarkUnsavedChanges(), this);

        if (newObj != null)
            _entityMetaState = new SpriteEntityMetaState(newObj);
        else
            _entityMetaState = null;
        _selectedAnim = null;

        ObjectPropertyWindow? entityData = _rightContent;
        AssertNotNull(entityData);
        entityData.SetEditor(newObj);
    }

    protected override Action<UIBaseWindow, Action<SpriteAsset?>> GetFileOpenFunction()
    {
        return FilePicker<SpriteAsset>.SelectFile;
    }

    #region Controls

    private void TogglePauseAnimation()
    {
        _paused = !_paused;

        if (_pauseButton != null)
            _pauseButton.Text = _paused ? "Play" : "Pause";
    }

    #endregion
}