#region Using

using Emotion.Common.Threading;
using Emotion.Editor.EditorHelpers;
using Emotion.Editor.EditorWindows.ModelViewerUtil;
using Emotion.Editor.PropertyEditors;
using Emotion.Game.ThreeDee;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Graphics;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.Graphics.ThreeDee;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.UI;
using System.Threading.Tasks;

#endregion

#nullable enable

namespace Emotion.Editor.EditorWindows;

public class ModelViewer : MapEditorPanel
{
	private Camera3D _camera;
	private InfiniteGrid _grid;
	private FrameBuffer? _renderBuffer;

	private UIBaseWindow? _surface3D;

	private bool _panelDragResize;

	private Object3D _obj;

	private static RenderStreamBatch<VertexDataWithBones>? _boneVerticesStream;

	public ModelViewer() : base("Model Viewer")
	{
		_camera = new Camera3D(new Vector3(-290, 250, 260));
		_camera.LookAtPoint(new Vector3(0, 0, 0));
		_grid = new InfiniteGrid();
		_obj = new Object3D();
	}

	protected override async Task LoadContent()
	{
		if (_boneVerticesStream == null)
			GLThread.ExecuteGLThreadAsync(() => { _boneVerticesStream = new RenderStreamBatch<VertexDataWithBones>(0, 1, false); });

		await base.LoadContent();
	}

	public override void AttachedToController(UIController controller)
	{
		GLThread.ExecuteGLThreadAsync(() =>
		{
			_renderBuffer = new FrameBuffer(new Vector2(1920, 1080)).WithColor().WithDepth();
			_renderBuffer.ColorAttachment.Smooth = true;
		});

		base.AttachedToController(controller);

		var contentSplit = new UIBaseWindow();
		contentSplit.LayoutMode = LayoutMode.HorizontalList;
		contentSplit.StretchX = true;
		contentSplit.StretchY = true;
		contentSplit.InputTransparent = false;

		var surface3D = new UIBaseWindow();
		surface3D.Id = "Surface3D";
		_surface3D = surface3D;
		surface3D.MinSize = new Vector2(960, 540) / 2f;
		surface3D.StretchX = true;
		surface3D.StretchY = true;
		contentSplit.AddChild(surface3D);

		var editorButtons = new UIBaseWindow();
		editorButtons.StretchX = true;
		editorButtons.StretchY = true;
		editorButtons.MinSize = new Vector2(100, 0);
		editorButtons.MaxSize = new Vector2(100, DefaultMaxSizeF);
		editorButtons.LayoutMode = LayoutMode.VerticalList;
		editorButtons.ListSpacing = new Vector2(0, 2);
		editorButtons.InputTransparent = false;
		editorButtons.Paddings = new Rectangle(2, 0, 2, 0);

		var butObj = new MapEditorTopBarButton();
		butObj.Text = "Open MeshAsset";
		butObj.StretchY = true;
		butObj.StretchX = false;
		butObj.OnClickedProxy = _ => { Controller!.AddChild(new MapEditorModal(new EditorFileExplorer<MeshAsset>(asset => { SetEntity(asset.Entity); }))); };
		editorButtons.AddChild(butObj);

		var butSprite = new MapEditorTopBarButton();
		butSprite.Text = "Open Sprite Stack";
		butSprite.StretchY = true;
		butSprite.StretchX = false;
		butSprite.OnClickedProxy = _ =>
		{
			Controller!.AddChild(new MapEditorModal(new EditorFileExplorer<SpriteStackTexture>(asset =>
			{
				//_obj.Entity = asset.GetSpriteStackEntity(Vector2.Zero);
			})));
		};
		editorButtons.AddChild(butSprite);

		var gridSizeEdit = new PropEditorNumber<float>();
		gridSizeEdit.SetValue(_grid.TileSize);
		gridSizeEdit.SetCallbackValueChanged(newVal => { _grid.TileSize = (float) newVal; });
		editorButtons.AddChild(new FieldEditorWithLabel("Grid Size: ", gridSizeEdit));

		var posEditor = new PropEditorFloat3(false);
		posEditor.SetValue(_obj.Position);
		posEditor.SetCallbackValueChanged(newVal => { _obj.Position = (Vector3) newVal; });
		editorButtons.AddChild(new FieldEditorWithLabel("Position: ", posEditor, LayoutMode.VerticalList));

		var rotEditor = new PropEditorFloat3(false);
		rotEditor.SetValue(_obj.RotationDeg);
		rotEditor.SetCallbackValueChanged(newVal => { _obj.RotationDeg = (Vector3) newVal; });
		editorButtons.AddChild(new FieldEditorWithLabel("Rotation: ", rotEditor, LayoutMode.VerticalList));

		var scaleEditor = new PropEditorFloat3(false);
		scaleEditor.SetValue(_obj.Size);
		scaleEditor.SetCallbackValueChanged(newVal => { _obj.Size = (Vector3) newVal; });
		editorButtons.AddChild(new FieldEditorWithLabel("Scale: ", scaleEditor, LayoutMode.VerticalList));

		var meshListProp = new EditorCheckboxList("Meshes");
		meshListProp.Id = "MeshList";
		scaleEditor.SetValue(_obj.Size);
		scaleEditor.SetCallbackValueChanged(newVal => { _obj.Size = (Vector3) newVal; });
		editorButtons.AddChild(meshListProp);

		contentSplit.AddChild(editorButtons);
		_contentParent.AddChild(contentSplit);

		// Dragging
		// todo: move to panel property
		var dragArea = new UITexture();
		dragArea.TextureFile = "Editor/PanelDragArea.png";
		dragArea.RenderSize = new Vector2(8, 8);
		dragArea.Smooth = true;
		dragArea.WindowColor = MapEditorColorPalette.ButtonColor;

		var dragButton = new UICallbackButton();
		dragButton.StretchX = true;
		dragButton.StretchY = true;
		dragButton.OnMouseEnterProxy = _ => { dragArea.WindowColor = MapEditorColorPalette.ActiveButtonColor; };
		dragButton.OnMouseLeaveProxy = _ => { dragArea.WindowColor = MapEditorColorPalette.ButtonColor; };
		dragButton.OnClickedProxy = _ => { _panelDragResize = true; };
		dragButton.OnClickedUpProxy = _ => { _panelDragResize = false; };
		dragButton.AddChild(dragArea);
		dragButton.Anchor = UIAnchor.BottomRight;
		dragButton.ParentAnchor = UIAnchor.BottomRight;

		AddChild(dragButton);
	}

	protected void SetEntity(MeshEntity? entity)
	{
		_obj.Entity = entity;

		var meshList = (EditorCheckboxList?) GetWindowById("MeshList");
		if (meshList != null)
		{
			meshList.SetItems(MeshVisibleCheckboxListItem.CreateItemsFromObject3D(_obj));
			meshList.Text = entity == null ? "Meshes" : $"Meshes [{entity.Meshes.Length}]";
		}
	}

	public override void DetachedFromController(UIController controller)
	{
		GLThread.ExecuteGLThreadAsync(() =>
		{
			_renderBuffer?.Dispose();
			_renderBuffer = null;
		});
		base.DetachedFromController(controller);
	}

	public override bool OnKey(Key key, KeyStatus status, Vector2 mousePos)
	{
		return _camera.CameraKeyHandler(key, status);
	}

	protected override bool UpdateInternal()
	{
		_camera.Update();

		if (_panelDragResize && _surface3D != null)
		{
			Vector2 curMouse = Engine.Host.MousePosition;
			Rectangle r = Rectangle.FromMinMaxPoints(_surface3D.Position2 + new Vector2(100, 0) * GetScale(), curMouse);
			r.SnapToAspectRatio(16f / 9f);
			if (r.Size.X > 100 && r.Size.Y > 200)
			{
				_surface3D.MinSize = r.Size / GetScale();
				_surface3D.InvalidateLayout();
			}
		}

		return base.UpdateInternal();
	}

	protected override bool RenderInternal(RenderComposer c)
	{
		if (_renderBuffer == null) return true;

		CameraBase oldCamera = c.Camera;
		c.RenderTo(_renderBuffer);
		c.Camera = _camera;

		RenderState oldState = c.CurrentState.Clone();
		c.SetState(RenderState.Default);
		c.SetUseViewMatrix(false);
		c.RenderSprite(new Vector3(0, 0, 0), Engine.Renderer.CurrentTarget.Size, Color.CornflowerBlue);
		c.SetUseViewMatrix(true);
		c.ClearDepth();

		_grid.Render(c);

		c.RenderLine(new Vector3(0, 0, 0), new Vector3(short.MaxValue, 0, 0), Color.Red, snapToPixel: false);
		c.RenderLine(new Vector3(0, 0, 0), new Vector3(0, short.MaxValue, 0), Color.Green, snapToPixel: false);
		c.RenderLine(new Vector3(0, 0, 0), new Vector3(0, 0, short.MaxValue), Color.Blue, snapToPixel: false);

		if (_obj.Entity?.AnimationRig != null && _boneVerticesStream != null)
		{
			_obj.RenderAnimated(c, _boneVerticesStream);
		}
		else
		{
			_obj.Render(c);
		}

		c.RenderTo(null);
		_boneVerticesStream?.DoTasks(c);

		c.SetState(oldState);
		c.Camera = oldCamera;

		base.RenderInternal(c);

		if (_surface3D != null)
			c.RenderSprite(_surface3D.Position, _surface3D.Size, _renderBuffer.Texture);

		return true;
	}
}