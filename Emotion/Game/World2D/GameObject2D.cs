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
    public class GameObject2D : Transform
    {
        /// <summary>
        /// The unique id of the object. Is assigned when added to the map.
        /// </summary>
        public int UniqueId;

        /// <summary>
        /// The object's name. Should be unique map-wide, but
        /// isn't actually enforced.
        /// </summary>
        public string? ObjectName { get; set; }

        /// <summary>
        /// The object's multiplicative color tint.
        /// </summary>
        public Color Tint { get; set; } = Color.White;

        /// <summary>
        /// Flags that specify systemic treatment of the object.
        /// </summary>
        public ObjectFlags ObjectFlags { get; set; }

        #region Runtime

        /// <summary>
        /// The map this object is in. Is set after Init.
        /// </summary>
        [DontSerialize]
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
        /// Whether this serialized object should be spawned when the map is loaded.
        /// This should always return true in EditorMode
        /// </summary>
        public virtual bool ShouldSpawnSerializedObject(Map2D map)
        {
            return true;
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
        /// Attach the object to the map. It is assumed that after this call the object will be
        /// returned by various map querries. Note that if the object's size or position is calculated
        /// at init/based on assets it might not be where you expect it to be spatially.
        /// </summary>
        public virtual void AttachToMap(Map2D map)
        {
            Map = map;
        }

        /// <summary>
        /// Init game data. All changes from this process shouldn't be serialized.
        /// It is assumed that LoadAssetsAsync has finished completion at the time this is called.
        /// </summary>
        public virtual void Init()
        {

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
	        Map = null;
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

        /// <summary>
        /// Whether the object is part of the specified layer in the map's world tree.
        /// Objects are always part of layer 0 - ALL.
        /// The layers must have been added using _worldTree.AddLayer prior
        /// to the object being added to the map, as this function is checked only then.
        /// The best place to do this is overriding the Map's SetupWorldTreeLayers function and adding them there.
        /// </summary>
        public virtual bool IsPartOfMapLayer(int layer)
        {
            return layer == 0;
        }

        public void Update(float dt)
        {
            UpdateInternal(dt);
        }

        public void Render(RenderComposer c)
        {
            RenderInternal(c);
        }

        /// <summary>
        /// Is run every tick. By default the map will update all of its objects,
        /// but by overriding the map's update function you can optimize or customize this.
        /// </summary>
        protected virtual void UpdateInternal(float dt)
        {
        }

        /// <summary>
        /// Is run every frame. By default the map will render all objects in layer 0 which are returned by
        /// a camera world bounds query.
        /// </summary>
        protected virtual void RenderInternal(RenderComposer c)
        {
            c.RenderSprite(Position, Size, Color.White);
        }

        /// <summary>
        /// Prior to saving a map from the editor this function will be called on all serializable objects to
        /// prevent the saving of junk data that is obvious.
        /// </summary>
        public virtual void TrimPropertiesForSerialize()
        {
	        // Dont save Z coordinate as the editor has no way of setting it anyway, but game code can. It has to be able to recalculate it.
            _z = 0;
            UniqueId = 0;
            ObjectFlags &= ~ObjectFlags.Persistent; // duh
        }

        public override string ToString()
        {
            return $"{UniqueId} {ObjectName} {GetType().Name} {base.ToString()}";
        }
    }
}