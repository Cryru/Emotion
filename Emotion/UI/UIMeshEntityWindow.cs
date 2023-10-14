#nullable enable

using Emotion.Game.World2D;
using Emotion.Game.World3D;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Objects;
using Emotion.Graphics;
using Emotion.Platform.Debugger;
using System.Threading.Tasks;
using Emotion.Game.World;
using Emotion.IO;
using Emotion.Game.World3D.Objects;
using Emotion.Common.Threading;

namespace Emotion.UI;
public class UIMeshEntityWindow : UIBaseWindow
{
	public string? AssetPath;
	public Vector2 PreviewSize = new Vector2(64, 64);

	protected FrameBuffer? _previewImage;
	protected GameObject3D? _previewObject;
	protected Camera3D? previewCamera;

	protected bool _previewImageValid;
	protected MeshAsset? _meshAsset;

	protected override async Task LoadContent()
	{
		var loadedNew = false;
		if (AssetPath == null) return;
		if (_meshAsset == null || _meshAsset.Name != AssetPath || _meshAsset.Disposed)
		{
			_meshAsset = await Engine.AssetLoader.GetAsync<MeshAsset>(AssetPath);
			loadedNew = true;
		}

		if (_meshAsset == null) return;
		if (loadedNew)
		{
			InvalidateLayout();
		}
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
		if (_previewObject == null && _meshAsset != null)
		{
			_previewObject = new GameObject3D("UI_Object");
			_previewObject.Entity = _meshAsset.Entity;
		}

		if (!_previewImageValid)
		{
			_previewObject.ObjectState = ObjectState.Alive;
			_previewObject.Init();
			var boundSphere = _previewObject.BoundingSphere;

			previewCamera = new Camera3D(boundSphere.Origin + new Vector3(0, boundSphere.Radius * 2.5f, boundSphere.Radius));
			previewCamera.NearZ = 0.001f;
			previewCamera.LookAtPoint(boundSphere.Origin);
			previewCamera.Update();

			_previewImage ??= new FrameBuffer(PreviewSize).WithColor();

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

		if (_previewImage != null)
		{
			c.RenderSprite(Position, Size, _previewImage.ColorAttachment);
		}

		return base.RenderInternal(c);
	}
}
