#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using Emotion.Common;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Primitives;
using Emotion.UI;

#endregion

namespace Emotion.Game.World2D
{
    public class MapEditorAddObjectPanel : MapEditorPanel
    {
        private Action<Type> _addObjectCallback;

        public MapEditorAddObjectPanel(Action<Type> callback) : base("Add Object")
        {
            _addObjectCallback = callback;
        }

        public override void AttachedToController(UIController controller)
        {
            UIBaseWindow contentWin = _contentParent;
            contentWin.InputTransparent = false;

            UIBaseWindow innerContainer = new UIBaseWindow();
            innerContainer.StretchX = true;
            innerContainer.StretchY = true;
            innerContainer.InputTransparent = false;
            innerContainer.LayoutMode = LayoutMode.VerticalList;
            innerContainer.ListSpacing = new Vector2(0, 3);
            innerContainer.ChildrenAllSameWidth = true;
            contentWin.AddChild(innerContainer);

            var txt = new UIText();
            txt.ScaleMode = UIScaleMode.FloatScale;
            txt.WindowColor = MapEditorColorPalette.TextColor;
            txt.Id = "buttonText";
            txt.FontFile = "Editor/UbuntuMono-Regular.ttf";
            txt.FontSize = MapEditorColorPalette.EditorButtonTextSize;
            txt.IgnoreParentColor = true;
            txt.Text = "These are all classes with parameterless constructors\nthat inherit GameObject2D.\nChoose class of object to add:";
            innerContainer.AddChild(txt);

            var listContainer = new UIBaseWindow();
            listContainer.StretchX = true;
            listContainer.StretchY = true;
            listContainer.InputTransparent = false;
            innerContainer.AddChild(listContainer);

            var listNav = new UICallbackListNavigator();
            listNav.LayoutMode = LayoutMode.VerticalList;
            listNav.StretchX = true;
            listNav.ListSpacing = new Vector2(0, 1);
            listNav.Margins = new Rectangle(0, 0, 10, 0);
            listNav.MinSize = new Vector2(0, 200);
            listNav.MaxSize = new Vector2(9999, 200);
            listContainer.AddChild(listNav);

            List<Type> objectTypes = EditorUtility.GetTypesWhichInherit<GameObject2D>();
            for (var i = 0; i < objectTypes.Count; i++)
            {
                Type objectType = objectTypes[i];

                var button = new MapEditorTopBarButton();
                button.StretchY = true;
                button.Text = objectType.Name;
                button.OnClickedProxy = (_) =>
                {
                    _addObjectCallback(objectType);
                    Parent!.RemoveChild(this);
                };
                listNav.AddChild(button);
            }

            var scrollBar = new UIScrollbar();
            scrollBar.DefaultSelectorColor = MapEditorColorPalette.ButtonColor;
            scrollBar.SelectorMouseInColor = MapEditorColorPalette.ActiveButtonColor;
            scrollBar.WindowColor = Color.Black * 0.5f;
            scrollBar.Anchor = UIAnchor.TopRight;
            scrollBar.ParentAnchor = UIAnchor.TopRight;
            scrollBar.MinSize = new Vector2(5, 0);
            scrollBar.MaxSize = new Vector2(5, 999);
            listNav.SetScrollbar(scrollBar);
            listContainer.AddChild(scrollBar);

            base.AttachedToController(controller);
        }
    }
}