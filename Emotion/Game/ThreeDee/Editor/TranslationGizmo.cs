#region Using

using Emotion.Graphics;
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

		/// <summary>
		/// The object the gizmo is currently affecting.
		/// </summary>
		public Positional? Target { get; protected set; }

		public Mesh XAxis { get; protected set; }
		public Mesh YAxis { get; protected set; }
		public Mesh ZAxis { get; protected set; }

		protected Mesh? _meshMouseover;
		protected Vector3 _dragPointStart;
		protected Vector3 _virtualPos;
		protected Vector3 _positionMoveStart;

		public TranslationGizmo()
		{
			var arrowCylinderGen = new CylinderMeshGenerator();
			arrowCylinderGen.RadiusBottom = 2;
			arrowCylinderGen.RadiusTop = 2;
			arrowCylinderGen.Height = 35;
			arrowCylinderGen.Capped = true;

			var arrowGen = new CylinderMeshGenerator();
			arrowGen.RadiusBottom = 4f;
			arrowGen.RadiusTop = 0;
			arrowGen.Height = 7;
			arrowGen.Capped = true;

			Mesh xCylinder = arrowCylinderGen.GenerateMesh().TransformMeshVertices(
				Matrix4x4.CreateFromYawPitchRoll(Maths.DegreesToRadians(90), 0f, 0f)
			).ColorMeshVertices(new Color(240, 75, 65, Alpha));

			Mesh xArrow = arrowGen.GenerateMesh().TransformMeshVertices(
				Matrix4x4.CreateFromYawPitchRoll(Maths.DegreesToRadians(90), 0f, 0f) *
				Matrix4x4.CreateTranslation(arrowCylinderGen.Height, 0, 0)
			).ColorMeshVertices(new Color(165, 40, 40, Alpha));

			XAxis = Mesh.CombineMeshes(xCylinder, xArrow, "X");

			Mesh yCylinder = arrowCylinderGen.GenerateMesh("YCylinder").TransformMeshVertices(
				Matrix4x4.CreateFromYawPitchRoll(0, Maths.DegreesToRadians(-90), 0f)
			).ColorMeshVertices(new Color(75, 240, 65, Alpha));

			Mesh yArrow = arrowGen.GenerateMesh("YArrow").TransformMeshVertices(
				Matrix4x4.CreateFromYawPitchRoll(0, Maths.DegreesToRadians(-90), 0f) *
				Matrix4x4.CreateTranslation(0, arrowCylinderGen.Height, 0)
			).ColorMeshVertices(new Color(40, 165, 40, Alpha));

			YAxis = Mesh.CombineMeshes(yCylinder, yArrow, "Y");

			Mesh zCylinder = arrowCylinderGen.GenerateMesh("ZCylinder").ColorMeshVertices(new Color(65, 75, 240, Alpha));

			Mesh zArrow = arrowGen.GenerateMesh("ZArrow").TransformMeshVertices(
				Matrix4x4.CreateTranslation(0, 0, arrowCylinderGen.Height)
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
					_positionMoveStart = Position;
					_virtualPos = Position;
					return false;
				}

				if (status == KeyStatus.Up)
				{
					_dragPointStart = Vector3.Zero;
					return false;
				}
			}

			if (key is Key.LeftControl or Key.RightControl && status == KeyStatus.Up && Target != null) Position = Target.Position;

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
			{
				Ray3D ray = camera.GetCameraMouseRay();
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

				Position = _virtualPos;
				if (Target != null) Target.Position = p;
			}
		}

		private Vector3 GetPointAlongPlane()
		{
			if (_meshMouseover == null) return Vector3.Zero;

			Vector3 axis;
			if (_meshMouseover.Name == "X")
				axis = RenderComposer.XAxis;
			else if (_meshMouseover.Name == "Y")
				axis = RenderComposer.YAxis;
			else // Z
				axis = RenderComposer.ZAxis;

			CameraBase? camera = Engine.Renderer.Camera;
			Ray3D ray = camera.GetCameraMouseRay();

			// Find plane that contains the axis and faces the camera.
			Vector3 planeTangent = Vector3.Cross(axis, Position - camera.Position);
			Vector3 planeNormal = Vector3.Cross(axis, planeTangent);
			planeNormal = planeNormal.Normalize();

			Vector3 intersection = ray.IntersectWithPlane(planeNormal, Position);

			// Limit movement along axis
			intersection = Position + axis * Vector3.Dot(intersection - Position, axis);

			return intersection;
		}

		public override void Dispose()
		{
			base.Dispose();
			Engine.Host.OnKey.RemoveListener(KeyHandler);
		}
	}
}