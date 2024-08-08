#region Using

using System;
using Emotion.Graphics;
using Emotion.Plugins.ImGuiNet.Windowing;

#endregion

namespace Emotion.Tools.Windows
{
    public class ProxyEditor : ImGuiWindow
    {
        private Action _update;
        private Action<RenderComposer> _render;

        public ProxyEditor(string title, Action update, Action<RenderComposer> render) : base(title)
        {
            _update = update;
            _render = render;
        }

        public override void Update()
        {
            _update?.Invoke();
        }

        protected override void RenderContent(RenderComposer composer)
        {
            _render?.Invoke(composer);
        }
    }
}