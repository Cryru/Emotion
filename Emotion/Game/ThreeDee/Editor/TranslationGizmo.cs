#region Using

using Emotion.Graphics.Camera;
using Emotion.Graphics.ThreeDee;
using Emotion.Platform.Input;
using Emotion.Utility;

#endregion

#nullable enable

namespace Emotion.Game.ThreeDee.Editor
{
	public class TranslationGizmo : Object3D
	{
		public int Alpha = 200;
		public int SnapSize = 50;

		public Positional? Target;

		public Mesh XAxis;
		public Mesh YAxis;
		public Mesh ZAxis;

		protected Mesh? _meshMouseover;
		protected Vector3 _dragPointStart;
		protected Vector3 _virtualPos;

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

			Mesh xCylinder = arrowCylinderGen.GenerateMesh().TransformMeshVertices(
				Matrix4x4.CreateFromYawPitchRoll(Maths.DegreesToRadians(90), 0f, 0f)
			).ColorMeshVertices(new Color(240, 75, 65, Alpha));

			Mesh xArrow = arrowGen.GenerateMesh().TransformMeshVertices(
				Matrix4x4.CreateFromYawPitchRoll(Maths.DegreesToRadians(90), 0f, 0f) *
				Matrix4x4.CreateTranslation(40, 0, 0)
			).ColorMeshVertices(new Color(165, 40, 40, Alpha));

			XAxis = Mesh.CombineMeshes(xCylinder, xArrow, "X");

			Mesh yCylinder = arrowCylinderGen.GenerateMesh("YCylinder").TransformMeshVertices(
				Matrix4x4.CreateFromYawPitchRoll(0, Maths.DegreesToRadians(-90), 0f)
			).ColorMeshVertices(new Color(75, 240, 65, Alpha));

			Mesh yArrow = arrowGen.GenerateMesh("YArrow").TransformMeshVertices(
				Matrix4x4.CreateFromYawPitchRoll(0, Maths.DegreesToRadians(-90), 0f) *
				Matrix4x4.CreateTranslation(0, 40, 0)
			).ColorMeshVertices(new Color(40, 165, 40, Alpha));

			YAxis = Mesh.CombineMeshes(yCylinder, yArrow, "Y");

			Mesh zCylinder = arrowCylinderGen.GenerateMesh("ZCylinder").ColorMeshVertices(new Color(65, 75, 240, Alpha));

			Mesh zArrow = arrowGen.GenerateMesh("ZArrow").TransformMeshVertices(
				Matrix4x4.CreateTranslation(0, 0, 40)
			).ColorMeshVertices(new Color(40, 40, 165, Alpha));

			ZAxis = Mesh.CombineMeshes(zCylinder, zArrow, "Z");

			Entity = new MeshEntity
			{
				Meshes = new[]
				{
					XAxis,
					YAxis,
					ZAxis
				},
				Name = "Translation Gizmo",
			};

			Engine.Host.OnKey.AddListener(KeyHandler, KeyListenerType.Editor);
		}

		private bool KeyHandler(Key key, KeyStatus status)
		{
			if (key == Key.MouseKeyLeft)
			{
				if (_meshMouseover != null && status == KeyStatus.Down)
				{
					_dragPointStart = GetPointAlongPlane();
					_virtualPos = Position;
					return false;
				}

				if (status == KeyStatus.Up)
				{
					_dragPointStart = Vector3.Zero;
					return false;
				}
			}

			return true;
		}

		public void SetTarget(Positional target)
		{
			Target = target;
			Position = target.Position;
		}

		public override void Update(float dt)
		{
			base.Update(dt);

			CameraBase? camera = Engine.Renderer.Camera;

			// Update mouseover
			if (_dragPointStart == Vector3.Zero)
				if (camera is Camera3D cam3D)
				{
					Ray3D ray = cam3D.GetCameraMouseRay();
					ray.IntersectWithObject(this, out Mesh collidedMesh, out Vector3 _, out Vector3 _, out int _);

					if (_meshMouseover != collidedMesh)
					{
						_meshMouseover?.SetVerticesAlpha((byte) Alpha);
						collidedMesh?.SetVerticesAlpha(255);
						_meshMouseover = collidedMesh;
					}
				}

			if (_dragPointStart != Vector3.Zero)
			{
				Vector3 newPoint = GetPointAlongPlane();
				Vector3 change = newPoint - _dragPointStart;

				bool snap = Engine.Host.IsCtrlModifierHeld();

				_virtualPos += change;
				_dragPointStart = newPoint;

				Vector3 p = _virtualPos;
				if (snap) p = (p / SnapSize).RoundClosest() * SnapSize;

				Position = p;
				if (Target != null) Target.Position = Position;
			}
		}

		private Vector3 GetPointAlongPlane()
		{
			if (_meshMouseover == null) return Vector3.Zero;

			Vector3 planeStart = Vector3.Zero;
			Vector3 planeNormal;
			if (_meshMouseover.Name == "X")
				planeNormal = new Vector3(0f, 1, 0);
			else if (_meshMouseover.Name == "Y")
				planeNormal = new Vector3(1f, 0f, 0);
			else
				planeNormal = new Vector3(0, 1, 0f);

			CameraBase? camera = Engine.Renderer.Camera;
			if (camera is Camera3D cam3D)
			{
				Ray3D ray = cam3D.GetCameraMouseRay();
				Vector3 point = ray.IntersectWithPlane(planeNormal, planeStart);

				if (_meshMouseover.Name == "X")
					point.Z = 0;
				else if (_meshMouseover.Name == "Y")
					point.Z = 0;
				else
					point.X = 0;

				return point;
			}

			return Vector3.Zero;
		}

		public override void Dispose()
		{
			base.Dispose();
			Engine.Host.OnKey.RemoveListener(KeyHandler);
		}
	}
}