#nullable enable

using Emotion.Common.Serialization;
using Emotion.Editor;
using Emotion.Game.OctTree;
using Emotion.Utility;
using System.Runtime.CompilerServices;

namespace Emotion.WIPUpdates.One.Work;

public partial class MapObject : IOctTreeStorable
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
    public float ScaleX
    {
        get => _scaleX;
        set
        {
            if (_scaleX == value) return;

            _scaleX = value;
            Resized();
        }
    }

    /// <summary>
    /// The scale of the object in the y axis.
    /// </summary>
    [DontSerialize]
    public float ScaleY
    {
        get => _scaleY;
        set
        {
            if (_scaleY == value) return;

            _scaleY = value;
            Resized();
        }
    }

    /// <summary>
    /// The scale of the object in the z axis. (height)
    /// </summary>
    [DontSerialize]
    public float ScaleZ
    {
        get => _scaleZ;
        set
        {
            if (_scaleZ == value) return;

            _scaleZ = value;
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
    /// The scale of the 3D object
    /// </summary>
    public Vector3 Scale3D
    {
        get => new Vector3(_scaleX, _scaleY, _scaleZ);
        set
        {
            if (_scaleX == value.X && _scaleY == value.Y && _scaleZ == value.Z) return;

            _scaleX = value.X;
            _scaleY = value.Y;
            _scaleZ = value.Z;
            Resized();
        }
    }

    #endregion

    #region Bounds

    /// <summary>
    /// Rectangle that encompasses the object.
    /// </summary>
    [DontSerialize]
    public virtual Rectangle BoundingRect
    {
        get => new Primitives.Rectangle(0, 0, 1, 1);
    }

    /// <summary>
    /// An axis aligned 3D cube that encompasses the object.
    /// </summary>
    public virtual Cube BoundingCube
    {
        get => new Cube(Vector3.Zero, Vector3.One / 2f);
    }

    #endregion

    #region Events

    /// <summary>
    /// Is invoked when the object moves.
    /// </summary>
    public event Action<MapObject>? OnMove;

    /// <summary>
    /// Is invoked when the object's scale changes.
    /// </summary>
    public event Action<MapObject>? OnResize;

    /// <summary>
    /// Is invoked when the object's rotation changes.
    /// </summary>
    public event Action<MapObject>? OnRotate;

    #endregion

    #region Private Holders

    protected float _x;
    protected float _y;
    protected float _z;
    protected float _scaleX = 1;
    protected float _scaleY = 1;
    protected float _scaleZ = 1;
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
        OnMove?.Invoke(this);
        InvalidateModelMatrix();
    }

    protected virtual void Rotated() // todo: support rotation in BaseGameObject so Object2D can have it too!
    {
        OnRotate?.Invoke(this);
        InvalidateModelMatrix();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void Resized()
    {
        Assert(!float.IsNaN(_scaleX));
        Assert(!float.IsNaN(_scaleY));
        Assert(!float.IsNaN(_scaleZ));

        OnResize?.Invoke(this);
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
        _scaleMatrix = Matrix4x4.CreateScale(_scaleX, _scaleY, _scaleZ);
        return _scaleMatrix * _rotationMatrix * _translationMatrix;
    }

    public Vector3 RotateVectorToObjectFacing(Vector3 vec)
    {
        return Vector3.Transform(vec, _rotationMatrix);
    }

    public Vector2 RotateVectorToObjectFacing(Vector2 vec)
    {
        return Vector3.Transform(vec.ToVec3(), Matrix4x4.CreateRotationZ(_rotation.Z)).ToVec2();
    }

    #region Interfaces

    public Cube GetOctTreeBound()
    {
        return BoundingCube;
    }

    #endregion
}
