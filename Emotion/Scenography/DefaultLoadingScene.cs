#region Using

using System.Collections;
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
        protected override IEnumerator LoadSceneRoutineAsync()
        {
            yield break;
        }

        public override void UpdateScene(float dt)
        {

        }

        public override void RenderScene(RenderComposer c)
        {
            
        }
    }
}