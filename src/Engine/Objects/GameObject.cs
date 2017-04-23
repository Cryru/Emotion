using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using SoulEngine.Objects.Components;
using SoulEngine.Enums;
using System.Linq;

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
        #region "Declarations"
        /// <summary>
        /// The layer the object should be drawn on.
        /// </summary>
        public ObjectLayer Layer = ObjectLayer.World;
        #region "Component Related"
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
        public int Priority = 0;
        /// <summary>
        /// Whether to update the object and its components.
        /// </summary>
        public bool Updating = true;
        /// <summary>
        /// Whether to draw the object and its components.
        /// </summary>
        public bool Drawing = true;
        #endregion
        //The position of the object within the scene.
        #region "Positional"
        /// <summary>
        /// 
        /// </summary>
        public int X { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Vector3 PositionFull
        {
            get
            {
                return new Vector3(X, Y, Priority);
            }
            set
            {
                X = (int) value.X;
                Y = (int) value.Y;
                Priority = (int) value.Z;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return new Vector2(X, Y);
            }
            set
            {
                X = (int) value.X;
                Y = (int) value.Y;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public Vector2 Center
        {
            get
            {
                return Bounds.Center.ToVector2();
                //return new Vector2(X + Width / 2, Y + Height / 2); <-- Custom Implementation
            }
            set
            {
                X = (int) value.X - Width / 2;
                Y = (int) value.Y - Height / 2;
            }
        }
        #endregion
        //The size of the box wrapping the object.
        #region "Size"
        /// <summary>
        /// 
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Vector2 Size
        {
            get
            {
                return new Vector2(Width, Height);
            }
            set
            {
                Width = (int) value.X;
                Height = (int) value.Y;
            }
        }
        #endregion
        //The rotation of the object.
        #region "Rotation"
        /// <summary>
        /// The object's rotation in radians.
        /// </summary>
        public float Rotation { get; set; }
        /// <summary>
        /// The object's rotation in degrees.
        /// </summary>
        public int RotationDegree
        {
            get
            {
                return (int)MathHelper.ToDegrees(Rotation);
            }
            set
            {
                Rotation = MathHelper.ToRadians(value);
            }
        }
        #endregion
        //Main variables.
        #region "Primary"
        /// <summary>
        /// The box wrapping the object.
        /// </summary>
        public Rectangle Bounds
        {
            get
            {
                return new Rectangle(X, Y, Width, Height);
            }
            set
            {
                X = value.X;
                Y = value.Y;
                Width = value.Width;
                Height = value.Height;
            }
        }
        #endregion
        #endregion

        #region "Components"
        /// <summary>
        /// The list of components attached to this object. Accessed through functions.
        /// </summary>
        private List<Component> Components = new List<Component>();
        /// <summary>
        /// Returns the number of components attached to the object.
        /// </summary>
        public int ComponentCount
        {
            get
            {
                return Components.Count;
            }
        }
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

            Components = Components.OrderBy(x => x.Priority).ToList();

            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].Draw();
            }
        }

        /// <summary>
        /// Is run every frame outside of an ink binding and used to compose component textures.
        /// </summary>
        public virtual void Compose()
        {
            //Check if drawing.
            if (Drawing == false) return;

            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].Compose();
            }
        }

        #region "Initialization"
        /// <summary>
        /// 
        /// </summary>
        public GameObject()
        {
            PositionFull = new Vector3(0, 0, 0);
            Size = new Vector2(100, 100);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Position"></param>
        /// <param name="Size"></param>
        public GameObject(Vector3 Position, Vector2 Size)
        {
            PositionFull = Position;
            this.Size = Size;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Position"></param>
        /// <param name="Size"></param>
        public GameObject(Vector2 Position, Vector2 Size)
        {
            this.Position = Position;
            this.Size = Size;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Position"></param>
        public GameObject(Vector3 Position)
        {
            PositionFull = Position;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Position"></param>
        public GameObject(Vector2 Position)
        {
            this.Position = Position;
        }
        #endregion

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
        /// <param name="Component">The type of the component.</typeparam>
        public void RemoveComponent(Type Component)
        {
            RemoveComponent(Component.Name);
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

        #region "Positioning"
        /// <summary>
        /// Center the object within the window.
        /// </summary>
        public void CenterObject()
        {
            CenterObjectX();
            CenterObjectY();
        }
        /// <summary>
        /// Center the object within the window on the X axis.
        /// </summary>
        public void CenterObjectX()
        {
            X = Settings.Width / 2 - Width / 2;
        }
        /// <summary>
        /// Center the object within the window on the Y axis.
        /// </summary>
        public void CenterObjectY()
        {
            Y = Settings.Height / 2 - Height / 2;
        }
        /// <summary>
        /// Makes the object fit the whole screen.
        /// </summary>
        public void ObjectFullscreen()
        {
            Width = Settings.Width;
            Height = Settings.Height;
            X = 0;
            Y = 0;
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
                    Components.Clear();
                    Components = null;
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

        ~GameObject()
        {
            Dispose();
        }
        #endregion

        #region "Templates"
        /// <summary>
        /// Generic object with positioning and a texture.
        /// </summary>
        public static GameObject GenericDrawObject
        {
            get
            {
                GameObject a = new GameObject();
                a.AddComponent(new ActiveTexture());
                return a;
            }
        }

        /// <summary>
        /// Generic object with positioning and text.
        /// </summary>
        public static GameObject GenericTextObject
        {
            get
            {
                GameObject a = new GameObject();
                a.AddComponent(new ActiveText());
                return a;
            }
        }

        /// <summary>
        /// Generic object for mouse input.
        /// </summary>
        public static GameObject GenericUIObject
        {
            get
            {
                GameObject a = new GameObject();
                a.AddComponent(new ActiveTexture());
                a.AddComponent(new MouseInput());
                a.Layer = ObjectLayer.UI;
                return a;
            }
        }
        #endregion
    }
}