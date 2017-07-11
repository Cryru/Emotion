using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using SoulEngine.Objects.Components;
using SoulEngine.Enums;
using System.Linq;
using SoulEngine.Modules;

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
        public virtual ObjectLayer Layer
        {
            get
            {
                return _layer;
            }
            set
            {
                _layer = value;
            }
        }
        private ObjectLayer _layer = ObjectLayer.World;

        /// <summary>
        /// The name of the object, assigned by the scene.
        /// </summary>
        public string Name = "";

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
        public int Priority
        {
            get
            {
                return priority;
            }
            set
            {
                if (value == priority) return;

                // Check if below 0.
                if (value < 0) value = 0;

                priority = value;

                // Order the objects in the current scene.
                if(Context.Core.isModuleLoaded<SceneManager>()) Context.Core.Module<SceneManager>().currentScene.OrderObjects();
            }
        }
        private int priority = 0;
        /// <summary>
        /// Whether to update the object and its components.
        /// </summary>
        public bool Updating = true;
        /// <summary>
        /// Whether to draw the object and its components.
        /// </summary>
        public bool Drawing = true;
        /// <summary>
        /// Whether to compose textures.
        /// </summary>
        public bool Composing = true;
        #endregion
        #region "Positional"
        /// <summary>
        /// The position of the object within the X axis.
        /// </summary>
        public int X
        {
            get
            {
                return x;
            }
            set
            {
                if (x != value) OnMove?.Invoke(this, EventArgs.Empty);
                x = value;
            }
        }
        private int x;
        /// <summary>
        /// The position of the object within the Y axis.
        /// </summary>
        public int Y
        {
            get
            {
                return y;
            }
            set
            {
                if (y != value) OnMove?.Invoke(this, EventArgs.Empty);
                y = value;
            }
        }
        private int y;
        /// <summary>
        /// The full position of the object including the Z axis.
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
        /// The position of the object within 2D space.
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
        /// The center of the object.
        /// </summary>
        public Vector2 Center
        {
            get
            {
                return Bounds.Center.ToVector2();
            }
            set
            {
                X = (int) value.X - Width / 2;
                Y = (int) value.Y - Height / 2;
            }
        }
        #endregion
        #region "Size"
        /// <summary>
        /// The width of the object.
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// The height of the object.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The size of the object.
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
        #region "Primary"
        /// <summary>
        /// The bounds of the object represented as a bounding rectangle.
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

        #region "Events"
        /// <summary>
        /// Triggered when the object's position changes.
        /// </summary>
        public static event EventHandler<EventArgs> OnMove;
        #endregion

        #region "Components"
        /// <summary>
        /// The list of components attached to this object. Accessed through functions.
        /// </summary>
        public List<Component> Components = new List<Component>();
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

            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].Draw();
            }

            //Check if drawing bounds.
            if (Settings.DrawBounds)
            {
                Context.ink.DrawRectangle(Bounds, Math.Max(1, Functions.ManualRatio(1, 540)), Color.Red);
            }   
        }

        /// <summary>
        /// Is run every frame outside of an ink binding and used to compose component textures.
        /// </summary>
        public virtual void Compose()
        {
            //Check if drawing.
            if (Composing == false) return;

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
        /// Internal index of types.
        /// </summary>
        List<string> TypeIndex = new List<string>();

        /// <summary>
        /// Returns an attached component by type, will throw a NullReference if not attached.
        /// </summary>
        /// <typeparam name="T">The type of component.</typeparam>
        /// <returns>The component object.</returns>
        public T Component<T>()
        {
            return (T)Convert.ChangeType(Components[IdComponent<T>()], typeof(T));
        }

        /// <summary>
        /// Attaches a component to the object, if the object already has a component of this type it is overwritten.
        /// </summary>
        /// <param name="ComponentObject">The component to add.</param>
        public void AddComponent(Component ComponentObject)
        {
            //Check if adding components is forbidden.
            if (lockComponentAdding) return;

            //Get the index of that component type.
            int ComponentIndex = IdComponent(ComponentObject.GetType());

            //Check if the component is already added.
            if (ComponentIndex >= 0)
            //If it is replace it.
            {
                Components[ComponentIndex] = ComponentObject;
                ComponentObject.attachedObject = this;
            }
            else
            //If it isn't add it.
            {
                Components.Add(ComponentObject);
                ComponentObject.attachedObject = this;
                //Index the object.
                TypeIndex.Add(ComponentObject.GetType().ToString());
            }

            //Run the component additional initialization.
            ComponentObject.Initialize();

            //Order components by priority.
            OrderComponents();
        }

        /// <summary>
        /// Order components by priority.
        /// </summary>
        public void OrderComponents()
        {
            Components = Components.OrderBy(x => x.Priority).ToList();
            TypeIndex.Clear();
            for (int i = 0; i < Components.Count; i++)
            {
                TypeIndex.Add(Components[i].GetType().ToString());
            }
        }

        /// <summary>
        /// Removes a component from the object, if such a component isn't attached nothing happens.
        /// </summary>
        /// <param name="Component">The type of the component.</typeparam>
        public void RemoveComponent<T>()
        {
            //Check if removing components is forbidden.
            if (lockComponentRemoving) return;

            //Get the ID of the component and use IT to remove the component.
            int id = IdComponent<T>();
            if (id != -1)
            {
                Components[id].Dispose();
                Components.RemoveAt(id);
                TypeIndex.Remove(typeof(T).ToString());
            }
        }

        /// <summary>
        /// Returns whether the specified component is attached to the object.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <returns>True if attached, false if not.</returns>
        public bool HasComponent<T>()
        {
            return IdComponent<T>() != -1;
        }

        /// <summary>
        /// Returns whether the specified component is attached to the object.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <returns>True if attached, false if not.</returns>
        public bool HasComponent(Type Type)
        {
            return TypeIndex.IndexOf(Type.ToString()) != -1;
        }

        /// <summary>
        /// Returns the id of the component within the internal component list.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <returns>The id of the component, -1 if not attached.</returns>
        private int IdComponent<T>()
        {
            return TypeIndex.IndexOf(typeof(T).ToString());        
        }

        /// <summary>
        /// Returns the id of the component within the internal component list.
        /// </summary>
        /// <param name="Type">The type of the component.</typeparam>
        /// <returns>The id of the component, -1 if not attached.</returns>
        private int IdComponent(Type Type)
        {
            return TypeIndex.IndexOf(Type.ToString());
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
                        Components[i] = null;
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
                a.AddComponent(new ActiveTexture(TextureMode.Stretch, AssetManager.MissingTexture));
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
        #endregion
    }
}