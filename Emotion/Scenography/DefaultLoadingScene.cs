#region Using

using System.Threading.Tasks;
using Emotion.Graphics;

#endregion

namespace Emotion.Scenography
{
    /// <inheritdoc />
    /// <summary>
    /// The default loading scene.
    /// </summary>
    public class DefaultLoadingScene : Scene
    {
        public override Task LoadAsync()
        {
            return Task.CompletedTask;
        }

        public override void Update()
        {

        }

        public override void Draw(RenderComposer composer)
        {

        }
    }
}