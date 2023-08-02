#region Using

using Emotion.Game.World2D.EditorHelpers;
using Emotion.Graphics;
using Emotion.Platform.Input;
using Emotion.UI;

#endregion

#nullable enable

namespace Emotion.Editor.EditorHelpers;

public class EditorPanel : UIBaseWindow
{
	public string Header;

	public bool Modal
	{
		get => _modal;
		set
		{
			_modal = value;
			HandleInput = _modal;
		}
	}

	private bool _modal;

	protected UIBaseWindow _contentParent = null!;
	protected UIBaseWindow _container = null!;
	private bool _centered;

	public EditorPanel(string header)
	{
		Header = header;
	}

	public override void AttachedToController(UIController controller)
	{
		base.AttachedToController(controller);

		var container = new UIBaseWindow();
		container.StretchX = true;
		container.StretchY = true;
		container.MinSize = new Vector2(100, 100);
		container.MaxSize = new Vector2(500, 200);
		container.ParentAnchor = UIAnchor.CenterCenter;
		container.Anchor = UIAnchor.CenterCenter;
		container.HandleInput = true;
		_container = container;
		_centered = true;

		var topBar = new MapEditorPanelTopBar();
		container.AddChild(topBar);

		var closeButton = (MapEditorTopBarButton) topBar.GetWindowById("CloseButton")!;
		closeButton.OnClickedProxy = _ => { controller.RemoveChild(this); };

		var panelLabel = (UIText) topBar.GetWindowById("PanelLabel")!;
		panelLabel.Text = Header;

		var contentParent = new UIBaseWindow();
		contentParent.StretchX = true;
		contentParent.StretchY = true;
		contentParent.Id = "Content";
		contentParent.Margins = new Rectangle(5, 15, 5, 5);
		_contentParent = contentParent;
		container.AddChild(contentParent);

		AddChild(container);

		controller.SetInputFocus(container);
	}

	public override void InputFocusChanged(bool haveFocus)
	{
		if (haveFocus)
			ZOffset++;
		else
			ZOffset--;

		base.InputFocusChanged(haveFocus);
	}

	protected override void AfterLayout()
	{
		base.AfterLayout();

		if (_centered)
		{
			_container.Offset = _container.Position2 / GetScale();
			_container.ParentAnchor = UIAnchor.TopLeft;
			_container.Anchor = UIAnchor.TopLeft;
			_centered = false;
		}
	}

	protected override bool RenderInternal(RenderComposer c)
	{
		if (Modal) c.RenderSprite(Bounds, Color.Black * 0.7f);

		c.RenderSprite(_container.Bounds, MapEditorColorPalette.BarColor * 0.8f);
		c.RenderOutline(_container.Bounds, MapEditorColorPalette.ActiveButtonColor * 0.9f, 2);
		return base.RenderInternal(c);
	}

	public override bool OnKey(Key key, KeyStatus status, Vector2 mousePos)
	{
		bool returnVal = base.OnKey(key, status, mousePos);
		return !Modal && returnVal;
	}
}