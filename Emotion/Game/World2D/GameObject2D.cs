#region Using

using System.Threading.Tasks;
using Emotion.Common.Serialization;
using Emotion.Game.Animation2D;
using Emotion.Game.World2D.Editor;
using Emotion.Graphics;

#endregion

#nullable enable

namespace Emotion.Game.World2D
{
	public class GameObject2D : Transform
	{
		/// <summary>
		/// The unique id of the object. Is assigned when added to the map.
		/// </summary>
		[DontSerialize] public int UniqueId;

		/// <summary>
		/// The object's name. Should be unique map-wide, but
		/// isn't actually enforced.
		/// </summary>
		public string? ObjectName { get; set; }

		/// <summary>
		/// If the object is spawned from a prefab this is the a handle to that prefab.
		/// </summary>
		public GameObjectPrefabOriginData? PrefabOrigin { get; set; }

		/// <summary>
		/// The object's multiplicative color tint.
		/// </summary>
		public Color Tint { get; set; } = Color.White;

		/// <summary>
		/// Flags that specify systemic treatment of the object.
		/// </summary>
		// [DontSerializeFlagValue((uint) ObjectFlags.Persistent)]
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
			Map = null!;
		}

		protected override void Moved()
		{
			base.Moved();
			// ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
			Map?.InvalidateObjectBounds(this);
		}

		protected override void Resized()
		{
			base.Resized();
			// ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
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

		public override string ToString()
		{
			return $"[{UniqueId}] {ObjectName ?? $"Object {GetType().Name}"}";
		}

		#region Changing Origin

		/// <summary>
		/// The position of the origin relative to the object bounding 2d rectangle.
		/// TopLeft by default as in RenderSprite.
		/// </summary>
		public OriginPosition OriginPos = OriginPosition.TopLeft;

		private int _boundsHash;
		private Rectangle _cachedBounds = Rectangle.Empty;

		/// <inheritdoc />
		public override Rectangle Bounds
		{
			get => GetBoundsWithOriginModified();
			set => base.Bounds = value;
		}

		/// <inheritdoc />
		public override Vector2 Center
		{
			get => Bounds.Center;
			set
			{
				if (OriginPos == OriginPosition.TopLeft)
				{
					base.Center = value;
					return;
				}

				Vector2 offset = OriginPos.MoveOriginOfRectangle(value.X, value.Y, Width, Height);

				X = value.X + Width / 2 + offset.X;
				Y = value.Y + Height / 2 + offset.Y;
			}
		}

		private Rectangle GetBoundsWithOriginModified()
		{
			if (OriginPos == OriginPosition.TopLeft) return base.Bounds;

			int hash = HashCode.Combine(OriginPos, _x, _y, _width, _height);
			if (hash == _boundsHash) return _cachedBounds;

			Vector2 offset = OriginPos.MoveOriginOfRectangle(_x, _y, _width, _height);
			_cachedBounds = base.Bounds.Offset(offset);
			_boundsHash = hash;

			return _cachedBounds;
		}

		#endregion
	}
}