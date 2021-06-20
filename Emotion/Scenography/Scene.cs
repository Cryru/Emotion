#region Using

using System.Threading.Tasks;
using Emotion.Graphics;

#endregion

namespace Emotion.Scenography
{
    public abstract class Scene
    {
        public abstract Task LoadAsync();
        public abstract void Update();
        public abstract void Draw(RenderComposer composer);

        public virtual void Unload()
        {
        }
    }
}