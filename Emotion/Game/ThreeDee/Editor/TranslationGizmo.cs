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
			arrowCylinderGen.RadiusBottom = 1;
			arrowCylinderGen.RadiusTop = 1;
			arrowCylinderGen.Height = 40;
			arrowCylinderGen.Capped = true;

			var arrowGen = new CylinderMeshGenerator();
			arrowGen.RadiusBottom = 2f;
			arrowGen.RadiusTop = 0;
			arrowGen.Height = 8;
			arrowGen.Capped = true;

			Entity = new MeshEntity
			{
				Meshes = new[]
				{
					arrowCylinderGen.GenerateMesh("XCylinder").TransformMeshVertices(
						Matrix4x4.CreateFromYawPitchRoll(Maths.DegreesToRadians(90), 0f, 0f)
					).ColorMeshVertices(new Color(240, 75, 65)),
					arrowCylinderGen.GenerateMesh("YCylinder").TransformMeshVertices(
						Matrix4x4.CreateFromYawPitchRoll(0, Maths.DegreesToRadians(-90), 0f)
					).ColorMeshVertices(new Color(75, 240, 65)),
					arrowCylinderGen.GenerateMesh("ZCylinder").ColorMeshVertices(new Color(65, 75, 240)),

					arrowGen.GenerateMesh("XArrow").TransformMeshVertices(
						Matrix4x4.CreateFromYawPitchRoll(Maths.DegreesToRadians(90), 0f, 0f) *
						Matrix4x4.CreateTranslation(40, 0, 0)
					).ColorMeshVertices(new Color(165, 40, 40)),
					arrowGen.GenerateMesh("YArrow").TransformMeshVertices(
						Matrix4x4.CreateFromYawPitchRoll(0, Maths.DegreesToRadians(-90), 0f) *
						Matrix4x4.CreateTranslation(0, 40, 0)
					).ColorMeshVertices(new Color(40, 165, 40)),
					arrowGen.GenerateMesh("ZArrow").TransformMeshVertices(
							Matrix4x4.CreateTranslation(0, 0, 40)
					).ColorMeshVertices(new Color(40, 40, 165))
				},
				Name = "Translation Gizmo",
			};
		}
	}
}