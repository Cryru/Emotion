#region Using

using Emotion.Game.World2D.EditorHelpers;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;

#endregion

namespace Emotion.Editor.EditorHelpers
{
    public class StringInputModalEnvelope
    {
        public string Text;
    }

    public class PropertyInputModal<T> : GenericPropertiesEditorPanel where T : new()
    {
        public string Text;

        private string _buttonText;

        private Func<T, bool> _doneCallback;

        public PropertyInputModal(Func<T, bool> doneCallback, string text, string headerText = "Input", string buttonText = "Done", T obj = default) : base(obj ?? new T())
        {
            _doneCallback = doneCallback;
            Text = text;
            Header = headerText;
            _spawnFieldGroupHeaders = false;
            PanelMode = PanelMode.Modal;
            _buttonText = buttonText;
        }

        public override void AttachedToController(UIController controller)
        {
            base.AttachedToController(controller);
            _container.MinSizeY = 0;
            _contentParent.LayoutMode = LayoutMode.VerticalList;
            _contentParent.ListSpacing = new Vector2(0, 2);

            var textLbl = new MapEditorLabel(Text)
            {
                OrderInParent = -100
            };
            _contentParent.AddChild(textLbl);

            var button = new EditorButton
            {
                OnClickedProxy = b =>
                {
                    bool success = _doneCallback((T) _obj);
                    if (success) Controller?.RemoveChild(this);
                },
                StretchY = true,
                Text = _buttonText,
                OrderInParent = 100,
                Anchor = UIAnchor.CenterCenter,
                ParentAnchor = UIAnchor.CenterCenter
            };
            _contentParent.AddChild(button);
        }
    }
}