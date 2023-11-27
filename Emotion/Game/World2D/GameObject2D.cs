#region Using

using Emotion.Game.Animation2D;
using Emotion.Game.World;
using Emotion.Graphics;

#endregion

#nullable enable

namespace Emotion.Game.World2D
{
	public class GameObject2D : BaseGameObject
	{
		public GameObject2D(string name) : base(name)
		{
		}

		// Serialization constructor.
		protected GameObject2D()
		{
		}

		/// <inheritdoc />
		protected override void RenderInternal(RenderComposer c)
		{
			c.RenderSprite(Position, Size, Tint);
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