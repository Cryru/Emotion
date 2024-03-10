using Emotion.Common.Serialization;
using Emotion.Editor;
using Emotion.Utility;
using System.Runtime.CompilerServices;

namespace Emotion.Game.World3D;

#nullable enable

public partial class GameObject3D
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
    /// /// The scale of the object in the x axis.
    /// </summary>
    [DontSerialize]
    public float SizeX
    {
        get => _sizeX;
        set
        {
            if (_sizeX == value) return;

            _sizeX = value;
            Resized();
        }
    }

    /// <summary>
    /// The scale of the object in the y axis.
    /// </summary>
    [DontSerialize]
    public float SizeY
    {
        get => _sizeY;
        set
        {
            if (_sizeY == value) return;

            _sizeY = value;
            Resized();
        }
    }

    /// <summary>
    /// The scale of the object in the z axis. (height)
    /// </summary>
    [DontSerialize]
    public float SizeZ
    {
        get => _sizeZ;
        set
        {
            if (_sizeZ == value) return;

            _sizeZ = value;
            Resized();
        }
    }


    /// <summary>
    /// The rotation of the object in radians.
    /// We serialize radians, but show degrees in the editor.
    /// </summary>
    [DontShowInEditor]
    public Vector3 Rotation
    {
        get => _rotation;
        set
        {
            if (_rotation == value) return;
            _rotation = value;
            Rotated();
        }
    }

    /// <summary>
    /// The rotation of the transform in decimal degrees.
    /// </summary>
    [DontSerialize]
    public Vector3 RotationDeg
    {
        get => new Vector3(Maths.RadiansToDegrees(_rotation.X), Maths.RadiansToDegrees(_rotation.Y), Maths.RadiansToDegrees(_rotation.Z));
        set
        {
            _rotation = new Vector3(Maths.DegreesToRadians(value.X), Maths.DegreesToRadians(value.Y), Maths.DegreesToRadians(value.Z));
            Rotated();
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
    /// The scale of the 3D object
    /// </summary>
    public Vector3 Size3D
    {
        get => new Vector3(_sizeX, _sizeY, _sizeZ);
        set
        {
            if (_sizeX == value.X && _sizeY == value.Y && _sizeZ == value.Z)
                return;

            _sizeX = value.X;
            _sizeY = value.Y;
            _sizeZ = value.Z;
            Resized();
        }
    }

    /// <summary>
    /// The rectangle bounding the transform.
    /// </summary>
    [DontSerialize]
    public override Rectangle Bounds2D
    {
        get
        {
            var bounds = Bounds3D;
            Rectangle r = new Rectangle();
            r.Size = bounds.HalfExtents.ToVec2() * 2f;
            r.Center = bounds.Origin.ToVec2();
            return r;
        }
        set
        {
            Position2 = value.Center;
        }
    }

    #endregion

    #region Events

    /// <summary>
    /// Is invoked when the object's rotation changes.
    /// </summary>
    public event EventHandler? OnRotate;

    #endregion

    #region Private Holders

    protected float _x;
    protected float _y;
    protected float _z;
    protected float _sizeX;
    protected float _sizeY;
    protected float _sizeZ;
    protected Vector3 _rotation;

    #endregion

    #region ModelMatrix

    protected Matrix4x4 _rotationMatrix;
    protected Matrix4x4 _scaleMatrix;
    protected Matrix4x4 _translationMatrix;

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void Moved()
    {
        base.Moved();
        _translationMatrix = Matrix4x4.CreateTranslation(_x, _y, _z);
    }

    protected virtual void Rotated() // todo: support rotation in BaseGameObject so Object2D can have it too!
    {
        OnRotate?.Invoke(this, EventArgs.Empty);

        _rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(_rotation.Y, _rotation.X, _rotation.Z);

        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        Map?.InvalidateObjectBounds(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void Resized()
    {
        Assert(!float.IsNaN(_sizeX));
        Assert(!float.IsNaN(_sizeY));
        Assert(!float.IsNaN(_sizeZ));
        base.Resized();

        float entityScale = Entity?.Scale ?? 1f;
        entityScale = entityScale * EntityMetaState?.Scale ?? 1f;
        _scaleMatrix = Matrix4x4.CreateScale(_sizeX * entityScale, _sizeY * entityScale, _sizeZ * entityScale);
    }

    public Matrix4x4 GetModelMatrix(bool ignoreRotation = false) // todo: cache this? needs to be updated only when one of the matrices or the entity changes.
    {
        Matrix4x4 rotMatrix = ignoreRotation ? Matrix4x4.Identity : _rotationMatrix;

        if (_entity != null) return _entity.LocalTransform * _scaleMatrix * rotMatrix * _translationMatrix;
        return _scaleMatrix * rotMatrix * _translationMatrix;
    }
}
