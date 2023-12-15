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

    public UICallbackListNavigator List;

    [DontSerialize] public Action? OnCloseProxy;

    public EditorDropDown(bool closeOnClick = false)
    {
        UseNewLayoutSystem = true;

        CloseOnClick = closeOnClick;
        WindowColor = MapEditorColorPalette.ActiveButtonColor;
        Offset = new Vector2(-2, 1);

        var innerBg = new UISolidColor
        {
            IgnoreParentColor = true,
            WindowColor = MapEditorColorPalette.BarColor.SetAlpha(255),
            Paddings = new Rectangle(3, 3, 3, 3),
        };

        AddChild(innerBg);

        var list = new UICallbackListNavigator
        {
            IgnoreParentColor = true,
            LayoutMode = LayoutMode.VerticalList,
            ListSpacing = new Vector2(0, 2),

            MaxSizeY = 100,
            HideScrollBarWhenNothingToScroll = true,
        };

        var scrollBar = new EditorScrollBar
        {
            MaxSizeY = 90
        };
        list.SetScrollbar(scrollBar);
        innerBg.AddChild(scrollBar);

        innerBg.AddChild(list);
        List = list;
    }

    protected override void AfterRenderChildren(RenderComposer c)
    {
        base.AfterRenderChildren(c);
        c.RenderOutline(Position, Size, WindowColor);
    }

    public void SetItems(EditorDropDownItem[]? items, Action<EditorDropDownItem>? selectedCallback = null)
    {
        List.ClearChildren();

        if (items == null) return;

        List<PropEditorBool>? checkBoxes = null;
        for (var i = 0; i < items.Length; i++)
        {
            EditorDropDownItem item = items[i];
            if (item is EditorDropDownCheckboxItem itemCheckBox)
            {
                checkBoxes ??= new List<PropEditorBool>();

                var checkMark = new PropEditorBool();
                checkMark.SetValue(itemCheckBox.Checked());
                checkMark.SetCallbackValueChanged(newVal =>
                {
                    if (item.Click == null) return;
                    if (item.Enabled != null)
                    {
                        bool enabled = item.Enabled();
                        if (!enabled) return;
                    }

                    selectedCallback?.Invoke(itemCheckBox);
                    item.Click(item, null);

                    AssertNotNull(checkBoxes);
                    for (var c = 0; c < checkBoxes.Count; c++)
                    {
                        PropEditorBool checkBox = checkBoxes[c];
                        EditorDropDownItem thatCheckBoxItem = items[c];
                        var thatCheckBoxItemAsCheckBoxItem = thatCheckBoxItem as EditorDropDownCheckboxItem;
                        checkBox.SetValue(thatCheckBoxItemAsCheckBoxItem?.Checked() ?? false);
                    }
                });
                checkMark.ZOffset = -1;

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
                    FillXInList = true,

                    Text = item.NameFunc != null ? item.NameFunc() : item.Name,
                };
                ddButton.OnClickedProxy = _ =>
                {
                    if (item.Click == null) return;
                    if (item.Enabled != null)
                    {
                        bool enabled = item.Enabled();
                        if (!enabled) return;
                    }

                    selectedCallback?.Invoke(item);
                    item.Click(item, ddButton);

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

    protected override Vector2 Measure(Vector2 space)
    {
        return base.Measure(space);
    }
}