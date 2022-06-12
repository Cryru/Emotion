#region Using

using System;
using Emotion.Primitives;
using Emotion.UI;

#endregion

#nullable enable

namespace Emotion.Tools.EmUI
{
    public class QuestionModal : ModalWindow
    {
        public Action<string>? OnAnswered;

        public QuestionModal(string title, string text, params string[] options) : base(title)
        {
            var textWnd = new EditorTextWindow();
            textWnd.Text = text;
            textWnd.Anchor = UIAnchor.TopCenter;
            textWnd.ParentAnchor = UIAnchor.TopCenter;
            ModalContent.AddChild(textWnd);

            var optionsList = new UIBaseWindow();
            optionsList.LayoutMode = LayoutMode.HorizontalList;
            optionsList.InputTransparent = false;
            optionsList.StretchY = true;
            optionsList.StretchX = true;
            optionsList.Anchor = UIAnchor.TopCenter;
            optionsList.ParentAnchor = UIAnchor.TopCenter;
            optionsList.ListSpacing = new System.Numerics.Vector2(3, 0);
            ModalContent.AddChild(optionsList);

            ModalContent.Paddings = new Rectangle(0, 0, 0, 4);

            for (var i = 0; i < options.Length; i++)
            {
                string value =  options[i];
                var optionWindow = new TextCallbackButton(value);
                optionWindow.OnClickedProxy = (_) =>
                {
                    OnAnswered?.Invoke(value);
                    Controller?.RemoveChild(this);
                };
                optionsList.AddChild(optionWindow);
            }
        }
    }
}