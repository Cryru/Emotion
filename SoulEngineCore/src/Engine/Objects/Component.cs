using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Objects.Components
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Base class for components.
    /// </summary>
    public abstract class Component : IDisposable
    {
        #region "Declarations"
        /// <summary>
        /// The object this component is attached to.
        /// </summary>
        public GameObject attachedObject;
        /// <summary>
        /// The component's priority.
        /// </summary>
        public int Priority
        {
            get
            {
                return priority;
            }
            set
            {
                if (value == priority) return;
                priority = value;
                if(attachedObject != null) attachedObject.OrderComponents();
            }
        }
        protected int priority = 0;
        #endregion

        //Main functions.
        #region "Functions"
        public virtual void Update()
        {

        }
        public virtual void Compose()
        {

        }
        public virtual void Draw()
        {

        }
        /// <summary>
        /// Initializes the component. Is automatically called when adding the component to an object.
        /// </summary>
        public virtual void Initialize()
        {

        }
        #endregion

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
                attachedObject = null;

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
