#region Using

using Emotion.Graphics;
#if WEB
using System.Threading.Tasks;

#endif

#endregion

namespace Emotion.Scenography
{
    /// <summary>
    /// A single scene.
    /// </summary>
    public interface IScene
    {
        /// <summary>
        /// Is run when the scene is loading.
        /// </summary>
        void Load();

        /// <summary>
        /// Is run every tick while the window is focused.
        /// </summary>
        void Update();

        /// <summary>
        /// Is run every frame while the window is focused.
        /// </summary>
        void Draw(RenderComposer composer);

        /// <summary>
        /// Is run when the scene is unloading.
        /// </summary>
        void Unload();
    }
}