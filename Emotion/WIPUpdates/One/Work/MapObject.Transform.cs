#nullable enable

using Emotion.Common.Serialization;
using Emotion.Editor;
using Emotion.Utility;
using System.Runtime.CompilerServices;

namespace Emotion.WIPUpdates.One.Work;

public partial class MapObject
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

    public float Width { get => SizeX; set => SizeX = value; }

    public float Height { get => SizeY; set => SizeY = value; }

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
    public Vector3 Position
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
    public Vector2 Position2D
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
    /// The scale of the 2D object
    /// </summary>
    public Vector2 Size2D
    {
        get => new Vector2(_sizeX, _sizeY);
        set
        {
            if (_sizeX == value.X && _sizeY == value.Y) return;

            _sizeX = value.X;
            _sizeY = value.Y;
            Resized();
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
            if (_sizeX == value.X && _sizeY == value.Y && _sizeZ == value.Z) return;

            _sizeX = value.X;
            _sizeY = value.Y;
            _sizeZ = value.Z;
            Resized();
        }
    }

    #endregion

    #region Bounds

    /// <summary>
    /// The center of the object's bounds.
    /// </summary>
    [DontSerialize]
    public Vector2 Center2D
    {
        get => Bounds2D.Center;
        set
        {
            _x = value.X - _sizeX / 2;
            _y = value.Y - _sizeY / 2;

            Moved();
        }
    }

    /// <summary>
    /// Rectangle that encompasses the object.
    /// </summary>
    [DontSerialize]
    public virtual Rectangle Bounds2D
    {
        get
        {
            return new Rectangle(_x, _y, _sizeX, _sizeY);
        }
        set
        {
            _x = value.X;
            _y = value.Y;
            _sizeX = value.Width;
            _sizeY = value.Height;

            Moved();
            Resized();
        }
    }

    #endregion

    #region Events

    /// <summary>
    /// Is invoked when the object moves.
    /// </summary>
    public event EventHandler? OnMove;

    /// <summary>
    /// Is invoked when the object's scale changes.
    /// </summary>
    public event EventHandler? OnResize;

    /// <summary>
    /// Is invoked when the object's rotation changes.
    /// </summary>
    public event EventHandler? OnRotate;

    #endregion

    #region Private Holders

    protected float _x;
    protected float _y;
    protected float _z;
    protected float _sizeX = 1;
    protected float _sizeY = 1;
    protected float _sizeZ = 1;
    protected Vector3 _rotation;

    #endregion

    #region ModelMatrix

    protected bool _modelMatrixDirty = true;
    protected Matrix4x4 _modelMatrix;

    protected Matrix4x4 _rotationMatrix;
    protected Matrix4x4 _scaleMatrix;
    protected Matrix4x4 _translationMatrix;

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void Moved()
    {
        OnMove?.Invoke(this, EventArgs.Empty);
        InvalidateModelMatrix();
    }

    protected virtual void Rotated() // todo: support rotation in BaseGameObject so Object2D can have it too!
    {
        OnRotate?.Invoke(this, EventArgs.Empty);
        InvalidateModelMatrix();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void Resized()
    {
        Assert(!float.IsNaN(_sizeX));
        Assert(!float.IsNaN(_sizeY));
        Assert(!float.IsNaN(_sizeZ));

        OnResize?.Invoke(this, EventArgs.Empty);
        InvalidateModelMatrix();
    }

    protected virtual void InvalidateModelMatrix()
    {
        _modelMatrixDirty = true;
    }

    public virtual Matrix4x4 GetModelMatrix(bool ignoreRotation = false)
    {
        if (_modelMatrixDirty)
        {
            _modelMatrix = UpdateModelMatrix();
            _modelMatrixDirty = false;
        }

        return _modelMatrix;
    }

    protected virtual Matrix4x4 UpdateModelMatrix()
    {
        _translationMatrix = Matrix4x4.CreateTranslation(_x, _y, _z);
        _rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(_rotation.Y, _rotation.X, _rotation.Z);
        _scaleMatrix = Matrix4x4.CreateScale(_sizeX, _sizeY, _sizeZ);
        return _scaleMatrix * _rotationMatrix * _translationMatrix;
    }
}
