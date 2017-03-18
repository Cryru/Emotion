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
        private Dictionary<string, GameObject> Objects;
        #endregion

        /// <summary>
        /// Setups the scene as the current scene.
        /// </summary>
        public void SetupScene()
        {
            //Set the current scene in core.
            Context.Core.Scene = this;

            //Setup assets loader and objects dictionary.
            Assets = new Assets();
            Objects = new Dictionary<string, GameObject>();

            //Run the start code.
            Start();
        }

        #region "Hooks"
        /// <summary>
        /// The core's hook for updating.
        /// </summary>
        public void UpdateHook()
        {
            //Run the scene's update code.
            Update();

            //Update the scene's object.
            foreach (var item in Objects)
            {
                item.Value.Update();
            }
        }
        /// <summary>
        /// The core's hook for drawing.
        /// </summary>
        public void DrawHook()
        {
            //Run the free draw function outside an ink binding.
            Objects.Select(x => x.Value).ToList().ForEach(x => x.DrawFree());

            //Run the draw function.
            Context.ink.Start(DrawChannel.World);
            Objects.Select(x => x.Value).Where(x => x.Layer == ObjectLayer.World).ToList().ForEach(x => x.Draw());
            Context.ink.End();
            Context.ink.Start(DrawChannel.Screen);
            Objects.Select(x => x.Value).Where(x => x.Layer == ObjectLayer.UI).ToList().ForEach(x => x.Draw());
            Context.ink.End();
        }
        #endregion

        #region "Object Manager"
        /// <summary>
        /// Adds an object to the scene.
        /// </summary>
        /// <param name="Label">The object's label to access the object with.</param>
        /// <param name="Object">The object to add.</param>
        public void AddObject(string Label, GameObject Object)
        {
            Objects.Add(Label, Object);

            Objects.OrderBy(x => x.Value.Priority);
        }
        /// <summary>
        /// Removes an object from the scene.
        /// </summary>
        /// <param name="Label">The label of the object you want to remove.</param>
        public void RemoveObject(string Label)
        {
            Objects.Remove(Label);
        }
        #endregion

        /// <summary>
        /// Runs when the scene takes control.
        /// </summary>
        abstract public void Start();
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
