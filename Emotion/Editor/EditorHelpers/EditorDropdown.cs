#nullable enable

#region Using

using Emotion.Common.Serialization;
using Emotion.Editor.PropertyEditors;
using Emotion.Game.World.Editor;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Graphics;
using Emotion.UI;

#endregion

namespace Emotion.Editor.EditorHelpers;

public class EditorDropDown : UIDropDown
{
    public bool CloseOnClick;

    [DontSerialize]
    public UICallbackListNavigator List;

    [DontSerialize]
    public Action? OnCloseProxy;

    public EditorDropDown(bool closeOnClick = false)
    {
        UseNewLayoutSystem = true;

        CloseOnClick = closeOnClick;
        Offset = new Vector2(-2, 1);

        var inner = new UIBaseWindow()
        {
            Paddings = new Rectangle(3, 3, 3, 3),
            MaxSizeY = 100,
        };
        AddChild(inner);

        var list = new UICallbackListNavigator
        {
            LayoutMode = LayoutMode.VerticalList,
            ListSpacing = new Vector2(0, 2),
            HideScrollBarWhenNothingToScroll = true,
        };

        //var scrollBar = new EditorScrollBar
        //{
        //    MaxSizeY = 90,
        //    Dock = UIDockDirection.Right,
        //    DontTakeSpaceWhenHidden = true,
        //    Margins = new Rectangle(2, 0, 0, 0)
        //};
        //list.SetScrollbar(scrollBar);

        //inner.AddChild(scrollBar);
        inner.AddChild(list);

        List = list;
    }

    protected override bool RenderInternal(RenderComposer c)
    {
        c.RenderSprite(Position, Size, MapEditorColorPalette.BarColor.SetAlpha(255));
        c.RenderOutline(Position, Size, MapEditorColorPalette.ActiveButtonColor);
        return base.RenderInternal(c);
    }

    protected override void AfterRenderChildren(RenderComposer c)
    {
        base.AfterRenderChildren(c);
    }

    public void SetItems(EditorDropDownItem[]? items, Action<int, EditorDropDownItem>? selectedCallback = null)
    {
        List.ClearChildren();

        if (items == null) return;

        List<PropEditorBool>? checkBoxes = null;
        for (var i = 0; i < items.Length; i++)
        {
            var myIdx = i;

            EditorDropDownItem item = items[i];
            if (item is EditorDropDownCheckboxItem itemCheckBox)
            {
                checkBoxes ??= new List<PropEditorBool>();

                var checkMark = new PropEditorBool();
                checkMark.SetValue(itemCheckBox.Checked());
                checkMark.SetCallbackValueChanged(newVal =>
                {
                    if (item.Enabled != null)
                    {
                        bool enabled = item.Enabled();
                        if (!enabled) return;
                    }

                    selectedCallback?.Invoke(i, itemCheckBox);
                    item.Click?.Invoke(item, null);

                    AssertNotNull(checkBoxes);
                    for (var c = 0; c < checkBoxes.Count; c++)
                    {
                        PropEditorBool checkBox = checkBoxes[c];
                        EditorDropDownItem thatCheckBoxItem = items[c];
                        var thatCheckBoxItemAsCheckBoxItem = thatCheckBoxItem as EditorDropDownCheckboxItem;
                        checkBox.SetValue(thatCheckBoxItemAsCheckBoxItem?.Checked() ?? false);
                    }
                });
                checkMark.Priority = -1;

                checkBoxes.Add(checkMark);

                var editor = new FieldEditorWithLabel(itemCheckBox.Name, checkMark)
                {
                    Margins = Rectangle.Empty
                };
                List.AddChild(editor);
            }
            else
            {
                var ddButton = new EditorButton
                {
                    FillX = true,

                    Text = item.NameFunc != null ? item.NameFunc() : item.Name,
                };
                ddButton.OnClickedProxy = _ =>
                {
                    if (item.Enabled != null)
                    {
                        bool enabled = item.Enabled();
                        if (!enabled) return;
                    }

                    selectedCallback?.Invoke(myIdx, item);
                    item.Click?.Invoke(item, ddButton);

                    if (CloseOnClick) Controller?.RemoveChild(this);
                };
                ddButton.Enabled = item.Enabled?.Invoke() ?? true;

                List.AddChild(ddButton);
            }
        }
    }

    public void SetChildren(UIBaseWindow[] items)
    {
        List.ClearChildren();

        for (var i = 0; i < items.Length; i++)
        {
            UIBaseWindow item = items[i];
            List.AddChild(item);
        }
    }

    public override void DetachedFromController(UIController controller)
    {
        base.DetachedFromController(controller);
        OnCloseProxy?.Invoke();
    }
}