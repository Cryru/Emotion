#region Using

using System.Runtime.CompilerServices;
using Emotion.Common.Serialization;
using Emotion.Game.Animation2D;

#endregion

namespace Emotion.Game.World2D;

#nullable enable

public partial class GameObject2D
{
    #region Properties

    /// <summary>
    /// The location of the object on the X-axis.
    /// </summary>
    [DontSerialize]
    public float X
    {
        get => _x;
        set
        {
            if (_x == value) return;

            _x = value;
            Moved();
        }
    }

    /// <summary>
    /// The location of the object on the Y-axis.
    /// </summary>
    [DontSerialize]
    public float Y
    {
        get => _y;
        set
        {
            if (_y == value) return;

            _y = value;
            Moved();
        }
    }

    /// <summary>
    /// The location of the object on the Z-axis.
    /// </summary>
    [DontSerialize]
    public float Z
    {
        get => _z;
        set
        {
            if (_z == value) return;

            _z = value;
            Moved();
        }
    }

    /// <summary>
    /// The position of the origin relative to the object bounding 2d rectangle.
    /// TopLeft by default as in RenderSprite.
    /// </summary>
    public OriginPosition OriginPos = OriginPosition.TopLeft;

    private int _boundsHash;
    private Rectangle _cachedBounds = Rectangle.Empty;

    /// <summary>
    /// The object's width.
    /// </summary>
    [DontSerialize]
    public float Width
    {
        get => _width;
        set
        {
            if (_width == value) return;

            _width = value;
            Resized();
        }
    }

    /// <summary>
    /// The object's height.
    /// </summary>
    [DontSerialize]
    public float Height
    {
        get => _height;
        set
        {
            if (_height == value) return;

            _height = value;
            Resized();
        }
    }

    #endregion

    #region Derived Properties

    /// <summary>
    /// The position within 3D space.
    /// This is the property that is serialized.
    /// </summary>
    public override Vector3 Position
    {
        get => new Vector3(_x, _y, _z);
        set
        {
            if (_x == value.X && _y == value.Y && _z == value.Z) return;

            _x = value.X;
            _y = value.Y;
            _z = value.Z;

            Moved();
        }
    }

    /// <summary>
    /// The position within 2D space.
    /// </summary>
    [DontSerialize]
    public override Vector2 Position2
    {
        get => new Vector2(_x, _y);
        set
        {
            if (_x == value.X && _y == value.Y) return;

            _x = value.X;
            _y = value.Y;

            Moved();
        }
    }

    /// <summary>
    /// The size of the transform.
    /// This is the property that gets serialized.
    /// </summary>
    public virtual Vector2 Size
    {
        get => new Vector2(_width, _height);
        set
        {
            if (_width == value.X && _height == value.Y) return;

            _width = value.X;
            _height = value.Y;

            Resized();
        }
    }

    [DontSerialize]
    public override Rectangle Bounds2D
    {
        get => Bounds;
        set => Bounds = value;
    }

    /// <summary>
    /// Rectangle that encompasses the object.
    /// </summary>
    [DontSerialize]
    public virtual Rectangle Bounds
    {
        get
        {
            var basicBounds = new Rectangle(Position, Size);
            if (OriginPos == OriginPosition.TopLeft) return basicBounds;

            int hash = HashCode.Combine(OriginPos, _x, _y, _width, _height);
            if (hash == _boundsHash) return _cachedBounds;

            Vector2 offset = OriginPos.MoveOriginOfRectangle(_x, _y, _width, _height);
            _cachedBounds = basicBounds.Offset(offset);
            _boundsHash = hash;

            return _cachedBounds;
        }
        set
        {
            _x = value.X;
            _y = value.Y;
            _width = value.Width;
            _height = value.Height;

            Moved();
            Resized();
        }
    }

    /// <summary>
    /// The center of the object's bounds.
    /// </summary>
    [DontSerialize]
    public Vector2 Center
    {
        get => Bounds.Center;
        set
        {
            if (OriginPos == OriginPosition.TopLeft)
            {
                _x = value.X - _width / 2;
                _y = value.Y - _height / 2;

                Moved();
            }

            Vector2 offset = OriginPos.MoveOriginOfRectangle(value.X, value.Y, _width, _height);

            _x = value.X + _width / 2 + offset.X;
            _y = value.Y + _height / 2 + offset.Y;
            Moved();
        }
    }

    #endregion

    #region Private Holders

    protected float _x;
    protected float _y;
    protected float _z;
    protected float _width;
    protected float _height;

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void Resized()
    {
        Assert(!float.IsNaN(_width));
        Assert(!float.IsNaN(_height));
        base.Resized();
    }
}