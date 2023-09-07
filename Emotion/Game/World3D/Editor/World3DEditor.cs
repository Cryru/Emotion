#region Using

using Emotion.Game.World.Editor;
using Emotion.Game.World.SceneControl;
using Emotion.Graphics;
using Emotion.Graphics.Camera;

#endregion

namespace Emotion.Game.World3D.Editor
{
	public class World3DEditor : WorldBaseEditorGeneric<Map3D>
	{
		public World3DEditor(IWorldAwareScene<Map3D> scene, Type mapType) : base(scene, mapType)
		{
		}

		protected override CameraBase GetEditorCamera()
		{
			return new Camera3D(Vector3.Zero);
		}

		protected override void EnterEditorInternal()
		{
		}

		protected override void ExitEditorInternal()
		{
		}

		protected override void UpdateInternal(float dt)
		{
		}

		protected override void RenderInternal(RenderComposer c)
		{
		}
	}
}