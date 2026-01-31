#nullable enable

using Emotion.Core.Utility.Time;
using Emotion.Editor;
using Emotion.Game.World.Components;
using Emotion.Network.LockStep;
using Emotion.Network.New.Base;
using Emotion.Primitives.DataStructures.OctTree;
using System.Runtime.CompilerServices;

namespace Emotion.Game.World;

public partial class GameObject : IOctTreeStorable
{
    [DontSerialize]
    private IGameObjectTransformProvider _transformProvider = new DefaultGameObjectTransformProvider();

    #region Properties

    /// <summary>
    /// The location of the object on the X-axis.
    /// </summary>
    [DontSerialize]
    public float X
    {
        get => GetPosition3D().X;
        set
        {
            Vector3 pos = GetPosition3D();
            pos.X = value;
            SetPosition3D(pos, 0);
        }
    }

    /// <summary>
    /// The location of the object on the Y-axis.
    /// </summary>
    [DontSerialize]
    public float Y
    {
        get => GetPosition3D().Y;
        set
        {
            Vector3 pos = GetPosition3D();
            pos.Y = value;
            SetPosition3D(pos, 0);
        }
    }

    /// <summary>
    /// The location of the object on the Z-axis.
    /// </summary>
    [DontSerialize]
    public float Z
    {
        get => GetPosition3D().Z;
        set
        {
            Vector3 pos = GetPosition3D();
            pos.Z = value;
            SetPosition3D(pos, 0);
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
    [DontSerializeButShowInEditor]
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

    #region Higher Order Properties

    /// <summary>
    /// The position within 3D space.
    /// This is the property that is serialized.
    /// </summary>
    public Vector3 Position3D
    {
        get => GetPosition3D();
        set => SetPosition3D(value, 0);
    }

    /// <summary>
    /// The position within 2D space.
    /// </summary>
    [DontSerialize]
    public Vector2 Position2D
    {
        get => GetPosition3D().ToVec2();
        set => SetPosition3D(value.ToVec3(Z), 0);
    }

    /// <summary>
    /// The scale of the object in two dimensions (missing z)
    /// </summary>
    public Vector2 Scale2D
    {
        get => new Vector2(_scaleX, _scaleY);
        set
        {
            if (_scaleX == value.X && _scaleY == value.Y) return;

            _scaleX = value.X;
            _scaleY = value.Y;
            Resized();
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
    /// An axis aligned 2D rectangle that encompasses the object.
    /// </summary>
    public virtual Rectangle GetBoundingRect()
    {
        return _transformProvider.GetBoundingRect(this);
    }

    /// <summary>
    /// An axis aligned 3D cube that encompasses the object.
    /// </summary>
    public virtual Cube GetBoundingCube()
    {
        return _transformProvider.GetBoundingCube(this);
    }

    /// <summary>
    /// Returns the sphere that encompasses the object.
    /// </summary>
    public virtual Sphere GetBoundingSphere()
    {
        return _transformProvider.GetBoundingSphere(this);
    }

    #endregion

    #region Events

    /// <summary>
    /// Is invoked when the object moves.
    /// </summary>
    public event Action<GameObject>? OnMove;

    /// <summary>
    /// Is invoked when the object's scale changes.
    /// </summary>
    public event Action<GameObject>? OnResize;

    /// <summary>
    /// Is invoked when the object's rotation changes.
    /// </summary>
    public event Action<GameObject>? OnRotate;

    #endregion

    #region Private Holders

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

    public virtual Matrix4x4 GetModelMatrix(bool ignoreRotation = false)
    {
        if (_modelMatrixDirty)
        {
            _modelMatrix = CalculateModelMatrix();
            _modelMatrixDirty = false;
        }

        return _modelMatrix;
    }

    public Matrix4x4 GetModelMatrixScale()
    {
        if (_modelMatrixDirty)
        {
            _modelMatrix = CalculateModelMatrix();
            _modelMatrixDirty = false;
        }

        return _scaleMatrix;
    }

    public Matrix4x4 GetModelMatrixRotation()
    {
        if (_modelMatrixDirty)
        {
            _modelMatrix = CalculateModelMatrix();
            _modelMatrixDirty = false;
        }

        return _rotationMatrix;
    }

    public Matrix4x4 GetModelMatrixTranslation()
    {
        if (_modelMatrixDirty)
        {
            _modelMatrix = CalculateModelMatrix();
            _modelMatrixDirty = false;
        }

        return _translationMatrix;
    }

    protected Matrix4x4 CalculateModelMatrix()
    {
        return _transformProvider.CalculateModelMatrix(this, out _scaleMatrix, out _rotationMatrix, out _translationMatrix);
    }

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

    public void InvalidateModelMatrix()
    {
        _transformProvider.OnModelMatrixInvalidated();
        _modelMatrixDirty = true;
    }

    #region Helpers

    public Vector3 RotateVectorToObjectFacing(Vector3 vec)
    {
        return Vector3.Transform(vec, _rotationMatrix);
    }

    public Vector2 RotateVectorToObjectFacing(Vector2 vec)
    {
        return Vector3.Transform(vec.ToVec3(), Matrix4x4.CreateRotationZ(_rotation.Z)).ToVec2();
    }

    public Vector3 GetForwardModelSpace()
    {
        return _transformProvider.GetForwardModelSpace();
    }

    public void RotateToFacePoint(Vector3 pt)
    {
        Vector3 forward = GetForwardModelSpace();

        Vector3 direction = Vector3.Normalize(pt - Position3D);
        float angle = MathF.Atan2(direction.Y, direction.X) + MathF.Atan2(forward.Y, forward.X);
        if (float.IsNaN(angle)) return;

        Vector3 rotation = Rotation;
        rotation.Z = angle;
        Rotation = rotation;
    }

    #endregion

    #region Interpolators

    private Vector3 _currentPos;

    private bool _interpPos;
    private Vector3 _interpStartPos;
    private Vector3 _interpTargetPos;
    private ValueTimer _interpPosTimer = new ValueTimer(0);

    public Vector3 GetPosition3D()
    {
        if (_interpPos)
            return Vector3.Lerp(_interpStartPos, _interpTargetPos, _interpPosTimer.GetFactor());
        return _currentPos;
    }

    public void SetPosition3D(Vector3 pos, float time = 0)
    {
        if (time <= 0) // Teleport
        {
            if (!_interpPos && _currentPos == pos)
                _currentPos = pos;

            _currentPos = pos;
            _interpPos = false;
            Moved();
            return;
        }

        _interpStartPos = GetPosition3D();
        _interpTargetPos = pos;

        if (_interpStartPos == _interpTargetPos)
        {
            _currentPos = _interpStartPos;
            _interpPos = false;
            Moved();
            return;
        }

        _interpPosTimer = new ValueTimer(time);
        _interpPos = true;
    }

    private void UpdateTransformInterpolations(float dt)
    {
        if (_interpPos)
        {
            _interpPosTimer.Update(dt);
            if (_interpPosTimer.Finished)
            {
                _currentPos = GetPosition3D();
                _interpPos = false;
            }
            Moved();
        }
    }

    #endregion

    #region Interfaces

    public Cube GetOctTreeBound()
    {
        return GetBoundingCube();
    }

    #endregion
}
