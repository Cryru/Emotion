﻿#region Using

using Emotion.Common.Input;
using Emotion.Common.Threading;
using Emotion.Primitives;
using Emotion.Utility;

#endregion

namespace Emotion.Graphics.Camera;

/// <summary>
/// The basis for a camera object.
/// </summary>
public abstract class CameraBase : Positional, IDisposable
{
    #region Properties

    /// <summary>
    /// The FarZ clipping plane. All Z vertices past this wont be rendered.
    /// </summary>
    public float FarZ
    {
        get => _farZ;
        set
        {
            _farZ = value;
            RecreateProjectionMatrix();
        }
    }

    private float _farZ = 500f;

    /// <summary>
    /// The NearZ clipping plane. All Z vertices before this wont be rendered.
    /// </summary>
    public float NearZ
    {
        get => _nearZ;
        set
        {
            _nearZ = value;
            RecreateProjectionMatrix();
        }
    }

    private float _nearZ = 1f;

    /// <summary>
    /// The direction (normalized) vector the camera is looking in.
    /// </summary>
    public Vector3 LookAt
    {
        get => _lookAt;
        set
        {
            var oldLookAt = _lookAt;
            _lookAt = value;
            LookAtChanged(oldLookAt, value);
            RecreateViewMatrix();
        }
    }

    protected Vector3 _lookAt = new Vector3(0, 0, -1);

    /// <summary>
    /// Calculated camera scale from the zoom and render size scale.
    /// Is applied to the view matrix in 2d cameras.
    /// </summary>
    public float CalculatedScale { get; protected set; } = 1;

    /// <summary>
    /// How zoomed the camera is.
    /// </summary>
    public float Zoom
    {
        get => _zoom;
        set
        {
            _zoom = value;
            RecreateViewMatrix();
        }
    }

    protected float _zoom;

    #endregion

    #region Calculated

    /// <summary>
    /// The camera's matrix.
    /// </summary>
    public Matrix4x4 ViewMatrix
    {
        get => _viewMatrix;
        set
        {
            OnBeforeMatricesChange();
            _viewMatrix = value;
        }
    }

    /// <summary>
    /// The camera's projection matrix.
    /// </summary>
    public Matrix4x4 ProjectionMatrix
    {
        get => _projectionMatrix;
        set
        {
            OnBeforeMatricesChange();
            _projectionMatrix = value;
        }
    }

    private Matrix4x4 _viewMatrix = Matrix4x4.Identity;
    private Matrix4x4 _projectionMatrix = Matrix4x4.Identity;

    #endregion

    protected KeyListenerType _inputPriority;

    protected CameraBase(Vector3 position, float zoom, KeyListenerType inputPriority = KeyListenerType.Game)
    {
        Position = position;
        _zoom = zoom;
        _inputPriority = inputPriority;

        LookAtChanged(Vector3.Zero, _lookAt);
        RecreateViewMatrix();
        RecreateProjectionMatrix();
        OnMove += (s, e) => { RecreateViewMatrix(); };
    }

    /// <summary>
    /// Called when the camera becomes current.
    /// </summary>
    public virtual void Attach()
    {
        if (_inputPriority != KeyListenerType.None)
            Engine.Host.OnKey.AddListener(CameraKeyHandler, _inputPriority);
    }

    /// <summary>
    /// Called when the camera is no longer current.
    /// </summary>
    public virtual void Detach()
    {
        Engine.Host.OnKey.RemoveListener(CameraKeyHandler);
    }

    protected virtual bool CameraKeyHandler(Key key, KeyState status)
    {
        return true;
    }

    /// <summary>
    /// Update the camera. The current camera is updated each tick.
    /// </summary>
    public abstract void Update();

    /// <summary>
    /// Recreates the view matrix of the camera.
    /// </summary>
    public abstract void RecreateViewMatrix();

    /// <summary>
    /// Recreates the projection matrix of the camera.
    /// </summary>
    public abstract void RecreateProjectionMatrix();

    /// <summary>
    /// Takes in a screen position and returns the corresponding world position.
    /// </summary>
    public abstract Vector3 ScreenToWorld(Vector2 screenPos);

    /// <summary>
    /// Takes in a world position and returns that position on the screen.
    /// </summary>
    public abstract Vector2 WorldToScreen(Vector3 worldPos);

    /// <summary>
    /// Get a world space ray in the direction of the mouse on the screen, starting from the camera.
    /// Generally used to detect what the player is clicking on.
    /// </summary>
    public abstract Ray3D GetCameraMouseRay();

    protected virtual void LookAtChanged(Vector3 oldVal, Vector3 newVal)
    {
       
    }

    /// <summary>
    /// Notify the renderer that the matrix has changed, causing a state change.
    /// </summary>
    protected void OnBeforeMatricesChange()
    {
        if (!Engine.Renderer.InFrame || Engine.Renderer.Camera != this || !GLThread.IsGLThread()) return;
        Engine.Renderer.FlushRenderStream();
        Engine.Renderer.SyncShader();
    }

    /// <summary>
    /// Destroy the camera and any internal resources it owns.
    /// </summary>
    public virtual void Dispose()
    {
    }

    #region Helpers

    public Vector3 GetCameraWorldUp()
    {
        Vector3 worldUp = RenderComposer.Up;
        if (MathF.Abs(Vector3.Dot(_lookAt, RenderComposer.Up)) == 1f)
            worldUp = RenderComposer.Up2D;
        return worldUp;
    }

    /// <summary>
    /// Set the camera's look at to a point in space.
    /// </summary>
    public void LookAtPoint(Vector3 point)
    {
        LookAt = Vector3.Normalize(point - Position);
    }

    /// <summary>
    /// Return a Rectangle which bounds the section of the game world seen by the camera.
    /// </summary>
    public virtual Rectangle GetCameraView2D()
    {
        Vector2 start = ScreenToWorld(Vector2.Zero).ToVec2();
        return new Rectangle(
            start,
            ScreenToWorld(Engine.Renderer.DrawBuffer.Size).ToVec2() - start
        );
    }

    public Frustum GetCameraView3D()
    {
        return new Frustum(ViewMatrix * ProjectionMatrix);
    }

    public Matrix4x4 GetRotationMatrix()
    {
        Vector3 up = GetCameraWorldUp();
        Vector3 cameraForward = Vector3.Normalize(_lookAt);
        Vector3 cameraRight = -Vector3.Normalize(Vector3.Cross(cameraForward, up));

        Vector3 rotatedCameraUp = Vector3.Cross(cameraRight, cameraForward);
        Matrix4x4 rotationMatrix = new Matrix4x4(
            cameraRight.X, rotatedCameraUp.X, cameraForward.X, 0.0f,
            cameraRight.Y, rotatedCameraUp.Y, cameraForward.Y, 0.0f,
            cameraRight.Z, rotatedCameraUp.Z, cameraForward.Z, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f
        );
        return rotationMatrix;
    }

    #endregion

    #region Statics

    public static float NearZDefault2DProjection = -1000;
    public static float FarZDefault2DProjection = 1000;

    /// <summary>
    /// Get the default 2d projection matrix for the currently bound framebuffer.
    /// </summary>
    /// <returns></returns>
    public static Matrix4x4 GetDefault2DProjection(float nearZ = 1337, float farZ = 1337)
    {
        if (nearZ == 1337) nearZ = NearZDefault2DProjection;
        if (farZ == 1337) farZ = FarZDefault2DProjection;

        RenderComposer renderer = Engine.Renderer;
        return Matrix4x4.CreateOrthographicOffCenterLeftHanded(0, renderer.CurrentTarget.Size.X, renderer.CurrentTarget.Size.Y, 0, nearZ, farZ);
    }

    #endregion
}