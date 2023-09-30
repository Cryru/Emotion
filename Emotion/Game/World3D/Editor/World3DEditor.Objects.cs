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
	public TranslationGizmo? MoveGizmo;

	protected override void InitializeObjectEditor()
	{
		base.InitializeObjectEditor();

		if (MoveGizmo == null)
			Task.Run(async () =>
			{
				var gizmo = new TranslationGizmo();
				await gizmo.LoadAssetsAsync();
				MoveGizmo = gizmo;
				gizmo.TargetMoved = (obj, start, movedTo) => { EditorRegisterMoveAction((BaseGameObject) obj, start, movedTo); };
			});

		// Cache collision vertices for all objects on editor enter.
		// Since animations are frozen this will allow for skinned meshes to be mouse picked.
		if (CurrentMap != null)
			foreach (BaseGameObject obj in CurrentMap.GetObjects())
			{
				if (obj is GameObject3D obj3D) obj3D.CacheVerticesForCollision();
			}
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
			MoveGizmo?.Render(c);

			if (_selectedObject is GameObject3D obj3D)
			{
				Cube boundCube = obj3D.Bounds3D;
				boundCube.RenderOutline(c, Color.PrettyYellow);
			}
		}

		if (_rolloverObject is GameObject3D rollover3D)
		{
			Cube boundCube = rollover3D.Bounds3D;
			boundCube.RenderOutline(c, Color.PrettyYellow * 0.5f);
		}
	}

	protected override void UpdateObjectEditor()
	{
		base.UpdateObjectEditor();

		if (_selectedObject != null) MoveGizmo?.Update(Engine.DeltaTime);
	}

	public override void SelectObject(BaseGameObject? obj)
	{
		base.SelectObject(obj);

		if (obj != null && MoveGizmo != null) MoveGizmo.SetTarget(obj);
	}
}