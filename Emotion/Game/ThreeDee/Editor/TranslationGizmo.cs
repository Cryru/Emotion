#region Using

using Emotion.Graphics.ThreeDee;
using Emotion.Utility;

#endregion

namespace Emotion.Game.ThreeDee.Editor
{
	public class TranslationGizmo : Object3D
	{
		public TranslationGizmo()
		{
			var arrowCylinderGen = new CylinderMeshGenerator();
			arrowCylinderGen.RadiusBottom = 0.25f;
			arrowCylinderGen.RadiusTop = 0.25f;
			arrowCylinderGen.Height = 10;
			arrowCylinderGen.Capped = true;

			Entity = new MeshEntity
			{
				Meshes = new[]
				{
					arrowCylinderGen.GenerateMesh().TransformMeshVertices(
						Matrix4x4.CreateFromYawPitchRoll(Maths.DegreesToRadians(90), 0f, 0f)
					).ColorMeshVertices(new Color(240, 75, 65)),
					arrowCylinderGen.GenerateMesh().TransformMeshVertices(
						Matrix4x4.CreateFromYawPitchRoll(0, Maths.DegreesToRadians(-90), 0f)
					).ColorMeshVertices(new Color(75, 240, 65)),
					arrowCylinderGen.GenerateMesh().ColorMeshVertices(new Color(65, 75, 240))
				},
				Name = "Translation Gizmo",
			};
		}
	}
}