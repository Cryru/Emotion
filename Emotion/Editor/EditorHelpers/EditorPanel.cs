#region Using

using Emotion.Game.World.Editor;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Graphics;
using Emotion.Platform.Input;
using Emotion.UI;

#endregion

#nullable enable

namespace Emotion.Editor.EditorHelpers;

public enum PanelMode
{
	Default,
	Modal,
	Embedded
}

public class EditorPanel : UIBaseWindow
{
	public string Header;

	public PanelMode PanelMode
	{
		get => _panelMode;
		set
		{
			if ((value == PanelMode.Embedded || _panelMode == PanelMode.Embedded) && Controller != null)
			{
				Assert(false, "Embedded mode can only be set prior to it attaching to a controller.");
				return;
			}

			_panelMode = value;
			HandleInput = value == PanelMode.Modal;
		}
	}
		
	private PanelMode _panelMode = PanelMode.Default;

    protected UIBaseWindow _contentParent = null!;
	protected UIBaseWindow _container = null!;
	protected bool _centered;

	public EditorPanel(string header)
	{
		Header = header;
	}

	public override void AttachedToController(UIController controller)
	{
		base.AttachedToController(controller);

		var container = new UIBaseWindow
		{
			HandleInput = true
		};
		_container = container;
		_centered = true;

		if (PanelMode != PanelMode.Embedded)
		{
            container.StretchX = true;
            container.StretchY = true;

            container.MinSize = new Vector2(100, 100);
            container.MaxSize = new Vector2(500, 200);
            container.ParentAnchor = UIAnchor.CenterCenter;
            container.Anchor = UIAnchor.CenterCenter;

            var topBar = new MapEditorPanelTopBar();
            container.AddChild(topBar);

            var closeButton = (EditorButton)topBar.GetWindowById("CloseButton")!;
            closeButton.OnClickedProxy = _ => { controller.RemoveChild(this); };

            var panelLabel = (UIText)topBar.GetWindowById("PanelLabel")!;
            panelLabel.Text = Header;
        }
		else
		{
            container.StretchX = true;
            container.StretchY = true;
            container.Margins = new Rectangle(5, 5, 5, 5);
		}
		
		var contentParent = new UIBaseWindow();
		contentParent.StretchX = true;
		contentParent.StretchY = true;
		contentParent.Id = "Content";
		contentParent.Margins = new Rectangle(5, PanelMode == PanelMode.Embedded ? 5 : 15, 5, 5);
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

		if (_centered && PanelMode != PanelMode.Embedded)
		{
			_container.Offset = _container.Position2 / GetScale();
			_container.ParentAnchor = UIAnchor.TopLeft;
			_container.Anchor = UIAnchor.TopLeft;
			_centered = false;
		}
	}

	protected override bool RenderInternal(RenderComposer c)
	{
		if (PanelMode == PanelMode.Modal) c.RenderSprite(Bounds, Color.Black * 0.7f);

		c.RenderSprite(_container.Bounds, MapEditorColorPalette.BarColor * 0.8f);
		c.RenderOutline(_container.Bounds, MapEditorColorPalette.ActiveButtonColor * 0.9f, 2);
		return base.RenderInternal(c);
	}

	public override bool OnKey(Key key, KeyStatus status, Vector2 mousePos)
	{
		bool returnVal = base.OnKey(key, status, mousePos);
		return PanelMode != PanelMode.Modal && returnVal;
	}
}