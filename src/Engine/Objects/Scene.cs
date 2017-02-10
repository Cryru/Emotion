using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Objects.Components;
using SoulEngine.Events;
using SoulEngine.Enums;

namespace SoulEngine.Objects
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// A scene object, different scenes are like different views.
    /// </summary>
    public abstract class Scene : IDisposable
    {
        #region "Declarations"
        /// <summary>
        /// The scene's loaded assets.
        /// </summary>
        public Assets Assets;
        /// <summary>
        /// Objects that belong to this scene.
        /// </summary>
        private List<GameObject> Objects;
        #endregion

        /// <summary>
        /// Setups the scene as the current scene.
        /// </summary>
        public void SetupScene()
        {
            //Setup loop hooks.
            SetupHooks();

            Start();
        }

        #region "Hooks"
        private void SetupHooks()
        {
            ESystem.Add(new Listen(EType.GAME_FRAMEEND, DrawHook));
            ESystem.Add(new Listen(EType.GAME_TICKEND, UpdateHook));
        }
        private void DrawHook()
        {
            Context.ink.Start(DrawChannel.World);
            Draw();
            Context.ink.End();
            Context.ink.Start(DrawChannel.Screen);
            Draw_UI();
            Context.ink.End();
        }
        private void UpdateHook()
        {

        }
        #endregion

        #region "Object Manager"
        /// <summary>
        /// Adds an object to the scene.
        /// </summary>
        /// <param name="Object">The object to add.</param>
        public void AddObject(GameObject Object)
        {
            Objects.Add(Object);

            Objects.OrderBy(x => x.Priority);
        }
        /// <summary>
        /// Removes an object from the scene.
        /// </summary>
        /// <param name="Object">The object to remove.</param>
        public void RemoveObject(GameObject Object)
        {
            Objects.Remove(Object);
        }
        #endregion

        /// <summary>
        /// Runs when the scene takes control.
        /// </summary>
        abstract public void Start();
        /// <summary>
        /// Draws objects in the world.
        /// </summary>
        abstract public void Draw();
        /// <summary>
        /// Draws objects on the UI.
        /// </summary>
        abstract public void Draw_UI();
        /// <summary>
        /// Updates objects in the world.
        /// </summary>
        abstract public void Update();

        //Other
        #region "Disposing"
        /// <summary>
        /// Disposing flag to detect redundant calls.
        /// </summary>
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                //Free resources.
                Assets.Content.Unload();
                ESystem.Remove(DrawHook);
                ESystem.Remove(UpdateHook);

                //Set disposing flag.
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
