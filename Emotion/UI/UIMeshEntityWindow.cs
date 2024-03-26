#nullable enable

#region Using

using System.Threading;
using System.Threading.Tasks;
using Emotion.Common.Threading;
using Emotion.Game.World;
using Emotion.Game.World3D;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Objects;
using Emotion.IO;
using OpenGL;

#endregion

namespace Emotion.UI;

public class UIMeshEntityWindow : UIBaseWindow
{
    public string? AssetPath;
    public bool Async;
    public Vector2 PreviewSize = new Vector2(64, 64);

    protected FrameBuffer? _previewImage;
    protected GameObject3D? _previewObject;
    protected Camera3D? previewCamera;

    protected MeshAsset? _meshAsset;
    protected string? _meshAssetLoaded;

    protected Task? _asyncLoading;
    protected CancellationTokenSource? _asyncLoadingTokenSource;

    protected override async Task LoadContent()
    {
        var loadedNew = false;
        if (AssetPath == null) return;
        if (_meshAssetLoaded == null || _meshAssetLoaded != AssetPath || (_meshAsset != null && _meshAsset.Disposed))
        {
            _meshAssetLoaded = AssetPath;

            if (_asyncLoading != null && !_asyncLoading.IsCompleted) _asyncLoadingTokenSource!.Cancel();
            if (Async)
            {
                _asyncLoadingTokenSource ??= new();
                var token = _asyncLoadingTokenSource.Token;
                _asyncLoading = Task.Run(async () =>
                {
                    token.ThrowIfCancellationRequested();
                    var asset = await Engine.AssetLoader.GetAsync<MeshAsset>(AssetPath);
                    token.ThrowIfCancellationRequested();
                    _meshAsset = asset;
                    InvalidateLayout();
                }, token);
            }
            else
            {
                _meshAsset = await Engine.AssetLoader.GetAsync<MeshAsset>(_meshAssetLoaded);
                loadedNew = true;
            }
        }

        if (_meshAsset == null) return;
        if (loadedNew) InvalidateLayout();
    }

    protected override Vector2 InternalMeasure(Vector2 space)
    {
        return PreviewSize;
    }

    public override void DetachedFromController(UIController controller)
    {
        GLThread.ExecuteGLThread(() =>
        {
            _previewImage?.Dispose();
            _previewImage = null;
        });

        base.DetachedFromController(controller);
    }

    protected override bool RenderInternal(RenderComposer c)
    {
        if (
            (_previewObject == null && _meshAsset != null) || // Not created
            (_meshAsset != null && _previewObject != null && _meshAsset.Entity != _previewObject.Entity) // Entity changed
        )
        {
            _previewObject ??= new GameObject3D("UI_Object");
            _previewObject.Entity = _meshAsset.Entity;
        }

        // Update preview
        if (_previewObject != null)
        {
            _previewObject.ObjectState = ObjectState.Alive;
            _previewObject.Init();
            var boundSphere = _previewObject.BoundingSphere;

            previewCamera = new Camera3D(boundSphere.Origin + new Vector3(0, boundSphere.Radius * 2.5f, boundSphere.Radius));
            previewCamera.NearZ = 0.001f;
            previewCamera.LookAtPoint(boundSphere.Origin);
            previewCamera.Update();

            _previewImage ??= new FrameBuffer(PreviewSize).WithColor().WithDepth();

            CameraBase oldCamera = c.Camera;
            c.RenderToAndClear(_previewImage);
            c.SetUseViewMatrix(true);
            c.PushModelMatrix(Matrix4x4.Identity, false);
            c.Camera = previewCamera;

            _previewObject.Update(0);
            _previewObject.Render(c);
            c.RenderTo(null);
            c.SetUseViewMatrix(false);
            c.PopModelMatrix();
            c.Camera = oldCamera;
        }

        c.RenderSprite(Position, Size, new Color(53, 53, 53) * 0.50f);

        if (_previewImage != null) c.RenderSprite(Position, Size, _previewImage.ColorAttachment);

        return base.RenderInternal(c);
    }
}