#nullable enable

namespace Emotion.Graphics.Camera;

public struct CameraCullingContext
{
    public Frustum Frustum;
    public Rectangle Rect2D;
    public bool Is2D;

    public CameraCullingContext(CameraBase camera)
    {
        Frustum = camera.GetCameraView3D();
        Rect2D = camera.GetCameraView2D();
        Is2D = camera is Camera2D;
    }
}