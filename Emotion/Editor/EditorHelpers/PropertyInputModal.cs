using Emotion.Game.World2D.EditorHelpers;
using Emotion.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.Editor.EditorHelpers
{
    public class StringInputModalEnvelope
    {
        public string Name;
    }

    public class PropertyInputModal<T> : GenericPropertiesEditorPanel where T : new()
    {
        public string Text;

        private Func<T, bool> _doneCallback;
        public PropertyInputModal(Func<T, bool> doneCallback, string text, T obj = default) : base(obj ?? new T())
        {
            _doneCallback = doneCallback;
            Text = text;
            Header = "Input";
            _spawnFieldGroupHeaders = false;
            PanelMode = PanelMode.Modal;
        }

        public override void AttachedToController(UIController controller)
        {
            base.AttachedToController(controller);
            _container.MinSizeY = 0;
            _contentParent.LayoutMode = LayoutMode.VerticalList;
            _contentParent.ListSpacing = new Vector2(0, 2);

            var innerContainer = GetWindowById("InnerContainer");
            MapEditorLabel textLbl = new MapEditorLabel(Text);
            textLbl.ZOffset = -100;
            _contentParent?.AddChild(textLbl);

            EditorButton button = new EditorButton();
            button.OnClickedProxy = (b) =>
            {
                bool success = _doneCallback((T)_obj);
                if (success) Controller.RemoveChild(this);
            };
            button.StretchY = true;
            button.Text = "Done";
            button.ZOffset = 100;
            button.Anchor = UIAnchor.CenterCenter;
            button.ParentAnchor = UIAnchor.CenterCenter;
            _contentParent?.AddChild(button);
        }
    }
}
