#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Game.ThreeDee.Editor;
using Emotion.Game.World;
using Emotion.Graphics;

#endregion

namespace Emotion.Game.World3D.Editor;

public partial class World3DEditor
{
	protected TranslationGizmo? _moveGizmo;

	protected override void InitializeObjectEditor()
	{
		base.InitializeObjectEditor();

		if (_moveGizmo == null)
			Task.Run(() =>
			{
				var gizmo = new TranslationGizmo();
				gizmo.LoadAssetsAsync().Wait();
				_moveGizmo = gizmo;
			});
	}

	protected override void DisposeObjectEditor()
	{
		base.DisposeObjectEditor();
	}

	protected override void RenderObjectSelection(RenderComposer c)
	{
		// todo
		if (_selectedObject != null)
		{
			c.ClearDepth();
			_moveGizmo?.Render(c);
		}
	}

	protected override void UpdateObjectEditor()
	{
		base.UpdateObjectEditor();

		if (_selectedObject != null)
		{
			_moveGizmo?.Update(Engine.DeltaTime);
		}
	}

	public override void SelectObject(BaseGameObject? obj)
	{
		base.SelectObject(obj);

		if (obj != null && _moveGizmo != null)
		{
			_moveGizmo.SetTarget(obj);
		}
	}
}