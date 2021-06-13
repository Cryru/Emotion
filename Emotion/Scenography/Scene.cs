#region Using

using System.Threading.Tasks;
using Emotion.Graphics;
using Emotion.UI;

#endregion

namespace Emotion.Scenography
{
    public class Scene
    {
        public UIController UI
        {
            get => _ui ??= new UIController();
        }

        private UIController _ui;

        public virtual async Task LoadAsync()
        {
            UI.Update();
            await UI.UILoadingThread;
        }

        public virtual void Update()
        {
            UI?.Update();
        }

        public virtual void Draw(RenderComposer composer)
        {
            if (UI == null) return;
            composer.SetUseViewMatrix(false);
            UI.Render(composer);
        }

        public virtual void Unload()
        {
        }
    }
}