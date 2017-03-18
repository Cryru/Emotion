using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using SoulEngine.Objects.Components;
using SoulEngine.Enums;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// The base for engine objects.
    /// </summary>
    public class GameObject : IDisposable
    {
        #region "Variables"
        /// <summary>
        /// Whether component adding should be disabled.
        /// </summary>
        protected bool lockComponentAdding = false;
        /// <summary>
        /// Whether component removing should be disabled.
        /// </summary>
        protected bool lockComponentRemoving = false;
        /// <summary>
        /// Priority of the object.
        /// </summary>
        public float Priority = 0;
        /// <summary>
        /// Whether to update the object and its components.
        /// </summary>
        public bool Updating = true;
        /// <summary>
        /// Whether to draw the object and its components.
        /// </summary>
        public bool Drawing = true;
        /// <summary>
        /// The layer the object should be drawn on.
        /// </summary>
        public ObjectLayer Layer = ObjectLayer.World;
        #endregion

        #region "Components"
        /// <summary>
        /// The list of components attached to this object. Accessed through functions.
        /// </summary>
        private List<Component> Components = new List<Component>();
        #endregion

        /// <summary>
        /// Is run every tick.
        /// </summary>
        public virtual void Update()
        {
            //Check if updating.
            if (Updating == false) return;

            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].Update();
            }
        }

        /// <summary>
        /// Is run every frame.
        /// </summary>
        public virtual void Draw()
        {
            //Check if drawing.
            if (Drawing == false) return;

            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].Draw();
            }
        }

        /// <summary>
        /// Is run every frame outside of an ink binding.
        /// </summary>
        public virtual void DrawFree()
        {
            //Check if drawing.
            if (Drawing == false) return;

            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].DrawFree();
            }
        }

        #region "Components Functions"
        /// <summary>
        /// Returns an attached component, will throw a NullReference if not attached.
        /// </summary>
        /// <typeparam name="T">The type of component.</typeparam>
        /// <returns>The component object.</returns>
        public T Component<T>()
        {
            return (T)Convert.ChangeType(Components[IdComponent<T>()], typeof(T));
        }
        /// <summary>
        /// Returns an attached component, will throw a NullReference if not attached.
        /// </summary>
        /// <param name="componentName">The name of the component. Case insensitive.</param>
        /// <returns>The component object, needs to be cast.</returns>
        public object Component(string componentName)
        {
            return Components[IdComponent(componentName)];
        }

        /// <summary>
        /// Attaches a component to the object, if the object already has a component of this type it is overwritten.
        /// </summary>
        /// <param name="ComponentObject">The component object.</param>
        public void AddComponent(Component ComponentObject)
        {
            //Check if adding components is forbidden.
            if (lockComponentAdding) return;

            //Check if the component is already added.
            if (HasComponent(ComponentObject.GetType().Name))
                //If not add it.
            { Components[IdComponent(ComponentObject.GetType().Name)] = ComponentObject; ComponentObject.attachedObject = this; }
            else
                //If it is then ovewrite the existing one.
            { Components.Add(ComponentObject); ComponentObject.attachedObject = this; }
        }

        /// <summary>
        /// Removes a component from the object, if such a component isn't attached nothing happens.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        public void RemoveComponent<T>()
        {
            RemoveComponent(typeof(T).Name);
        }
        /// <summary>
        /// Removes a component from the object, if such a component isn't attached nothing happens.
        /// </summary>
        /// <param name="componentName">The name of the component. Case insensitive.</param>
        public void RemoveComponent(string componentName)
        {
            //Check if removing components is forbidden.
            if (lockComponentRemoving) return;

            //Get the ID of the component and use IT to remove the component.
            int id = IdComponent(componentName);
            if (id != -1) { Components[id].Dispose(); Components.RemoveAt(id); }
        }

        /// <summary>
        /// Returns whether the specified component is attached to the object.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <returns>True if attached, false if not.</returns>
        public bool HasComponent<T>()
        {
            return HasComponent(typeof(T).Name);
        }
        /// <summary>
        /// Returns whether the specified component is attached to the object.
        /// </summary>
        /// <param name="componentName">The name of the component. Case insensitive.</param>
        /// <returns>True if attached, false if not.</returns>
        public bool HasComponent(string componentName)
        {
            return IdComponent(componentName) != -1;
        }

        /// <summary>
        /// Returns the id of the component within the internal component list.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <returns>The id of the component, -1 if not attached.</returns>
        private int IdComponent<T>()
        {
            return IdComponent(typeof(T).Name);
        }
        /// <summary>
        /// Returns the id of the component within the internal component list.
        /// </summary>
        /// <param name="componentName">The name of the component. Case insensitive.</param>
        /// <returns>The id of the component, -1 if not attached.</returns>
        private int IdComponent(string componentName)
        {
            //Note: This is the primary function other component functions rely on, optimization is to be done HERE.
            for (int i = 0; i < Components.Count; i++)
                if (Components[i].GetType().Name.ToLower() == componentName.ToLower())
                    return i;

            return -1;
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
                    for (int i = 0; i < Components.Count; i++)
                    {
                        Components[i].Dispose();
                    }
                }

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

        #region "Templates"
        /// <summary>
        /// Generic object with positioning and rendering.
        /// </summary>
        public static GameObject GenericDrawObject
        {
            get
            {
                GameObject a = new GameObject();
                a.AddComponent(new Transform());
                a.AddComponent(new ActiveTexture());
                a.AddComponent(new Renderer());
                return a;
            }
        }

        #endregion
    }
}
