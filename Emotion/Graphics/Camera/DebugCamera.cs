#region Using

using Emotion.Platform.Input;
using Emotion.Utility;

#endregion

namespace Emotion.Graphics.Camera;

public class DebugCamera : Camera3D
{
    public DebugCamera(Vector3 position, Vector3 lookAt, float zoom = 1) : base(position, zoom, KeyListenerType.EditorCamera)
    {
        LookAt = lookAt;
    }

    public override bool CameraKeyHandler(Key key, KeyStatus status)
    {
        bool propagateInput = base.CameraKeyHandler(key, status);
        if (Engine.Host.IsAltModifierHeld()) return true;
        return propagateInput;
    }

    public override void Update()
    {
        if (Engine.Host.IsAltModifierHeld()) return;
        base.Update();
    }
}