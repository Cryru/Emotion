#region Using

using System;
using System.Threading.Tasks;
using Emotion.Common.Serialization;
using Emotion.Graphics;
using Emotion.Primitives;

#endregion

#nullable enable

namespace Emotion.Game.World2D
{
    [Flags]
    public enum Map2DObjectFlags : uint
    {
        None = 0,

        UpdateWorldTree = 2 << 0,
        Serializable = 2 << 1,
    }

    public enum ObjectState : byte
    {
        None = 0,
        Loading = 1,
        Alive = 2,
        Destroyed = 3
    }

    public class GameObject2D : Transform
    {
        /// <summary>
        /// The object's name. Should be unique map-wide, but
        /// isn't actually enforced.
        /// </summary>
        public string ObjectName { get; set; }

        /// <summary>
        /// The object's multiplicative color tint.
        /// </summary>
        public Color Tint { get; set; } = Color.White;

        #region Runtime

        /// <summary>
        /// The map this object is in. Is set after Init.
        /// </summary>
        public Map2D Map { get; protected set; }

        /// <summary>
        /// The object state, managed by the map in runtime.
        /// </summary>
        [DontSerialize]
        public ObjectState ObjectState { get; set; }

        /// <summary>
        /// Object flags managed by the map in runtime.
        /// </summary>
        [DontSerialize]
        public Map2DObjectFlags MapFlags { get; set; }

        #endregion

        public GameObject2D(string name)
        {
            ObjectName = name;
            Map = null!;
        }

        // Serialization constructor.
        protected GameObject2D()
        {
            ObjectName = null!;
            Map = null!;
        }

        /// <summary>
        /// Load all assets in use by the object. This is called first and is expected to be ran
        /// in parallel with other objects.
        /// </summary>
        public virtual Task LoadAssetsAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Init game data. All changes from this process shouldn't be serialized.
        /// It is assumed that LoadAssetsAsync has finished completion at the time this is called.
        /// </summary>
        public virtual void Init(Map2D map)
        {
            Map = map;
        }

        /// <summary>
        /// Called after all objects init and LoadAssetsAsync is complete.
        /// </summary>
        public virtual void LateInit()
        {

        }

        /// <summary>
        /// Free any resources and cleanup.
        /// </summary>
        public virtual void Destroy()
        {
        }

        protected override void Moved()
        {
            base.Moved();
            Map?.InvalidateObjectBounds(this);
        }

        protected override void Resized()
        {
            base.Resized();
            Map?.InvalidateObjectBounds(this);
        }

        public virtual bool IsPartOfMapLayer(int layer)
        {
            return layer == 0;
        }

        public virtual void Update(float dt)
        {
        }

        public virtual void Render(RenderComposer c)
        {
            c.RenderSprite(Position, Size, Color.White);
        }

        public virtual void PreMapEditorSave()
        {
            // you can prepare the obj for serialization here.
        }
    }
}