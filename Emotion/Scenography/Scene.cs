#region Using

using System.Threading.Tasks;
using Emotion.Graphics;

#endregion

namespace Emotion.Scenography
{
    public class Scene
    {
        public virtual Task LoadAsync()
        {
            return Task.CompletedTask;
        }

        public virtual void Update()
        {
        }

        public virtual void Draw(RenderComposer composer)
        {
        }

        public virtual void Unload()
        {
        }
    }
}