#region Using

using Emotion.Game.World;
using Emotion.Game.World3D;
using Emotion.Game.World3D.Objects;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.Graphics.ThreeDee;
using Emotion.Platform.Input;
using Emotion.Utility;

#endregion

#nullable enable

namespace Emotion.Game.ThreeDee.Editor
{
    public class TranslationGizmo : GameObject3D
    {
        public int Alpha = 200;
        //public int SnapSize = 50;

        public bool MouseInside
        {
            get => _meshMouseover != null;
        }

        /// <summary>
        /// The object the gizmo is currently affecting.
        /// </summary>
        public BaseGameObject? Target { get; protected set; }

        public Mesh XAxis { get; protected set; }

        public Mesh YAxis { get; protected set; }

        public Mesh ZAxis { get; protected set; }

        public Mesh ZPlane { get; protected set; }

        public Action<BaseGameObject, Vector3, Vector3>? TargetMoved;
        public Action<BaseGameObject>? TargetStartMoving;

        protected bool _startMovingEventFired;

        protected Mesh? _meshMouseover;
        protected Vector3 _dragPointStart;
        protected Vector3 _dragPointStartAbsolute;
        protected Vector3 _virtualPos;
        protected Vector3 _positionMoveStart;

        protected Vector3 _lastCameraPos;

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
            );

            Mesh xArrow = arrowGen.GenerateMesh().TransformMeshVertices(
                Matrix4x4.CreateFromYawPitchRoll(Maths.DegreesToRadians(90), 0f, 0f) *
                Matrix4x4.CreateTranslation(arrowCylinderGen.Height, 0, 0)
            );

            XAxis = Mesh.CombineMeshes(xCylinder, xArrow, "X");

            Mesh yCylinder = arrowCylinderGen.GenerateMesh("YCylinder").TransformMeshVertices(
                Matrix4x4.CreateFromYawPitchRoll(0, Maths.DegreesToRadians(-90), 0f)
            );

            Mesh yArrow = arrowGen.GenerateMesh("YArrow").TransformMeshVertices(
                Matrix4x4.CreateFromYawPitchRoll(0, Maths.DegreesToRadians(-90), 0f) *
                Matrix4x4.CreateTranslation(0, arrowCylinderGen.Height, 0)
            );

            YAxis = Mesh.CombineMeshes(yCylinder, yArrow, "Y");

            Mesh zCylinder = arrowCylinderGen.GenerateMesh("ZCylinder");

            Mesh zArrow = arrowGen.GenerateMesh("ZArrow").TransformMeshVertices(
                Matrix4x4.CreateTranslation(0, 0, arrowCylinderGen.Height)
            );

            ZAxis = Mesh.CombineMeshes(zCylinder, zArrow, "Z");

            var materialX = new MeshMaterial
            {
                Name = "Tool-X",
                DiffuseColor = new Color(165, 40, 40)
            };
            XAxis.Material = materialX;
            XAxis.SetVerticesAlpha((byte) Alpha);

            var materialY = new MeshMaterial
            {
                Name = "Tool-Y",
                DiffuseColor = new Color(40, 165, 40)
            };
            YAxis.Material = materialY;
            YAxis.SetVerticesAlpha((byte) Alpha);

            var materialZ = new MeshMaterial
            {
                Name = "Tool-Z",
                DiffuseColor = new Color(40, 40, 165)
            };
            ZAxis.Material = materialZ;
            ZAxis.SetVerticesAlpha((byte) Alpha);

            var materialPlaneZ = new MeshMaterial
            {
                Name = "Plane-Z",
                DiffuseColor = Color.PrettyBlue
            };
            ZPlane = Mesh.ShallowCopyMesh_DeepCopyVertexData(Quad3D.QuadEntity.Meshes[0]);
            ZPlane.TransformMeshVertices(
                Matrix4x4.CreateScale(30, 30, 30) *
                Matrix4x4.CreateTranslation(arrowCylinderGen.Height / 2, arrowCylinderGen.Height / 2, 0)
            );
            ZPlane.Material = materialPlaneZ;
            ZPlane.Name = "Z-Plane";
            ZPlane.SetVerticesAlpha((byte)Alpha);

            Entity = new MeshEntity
            {
                Meshes = new[]
                {
                    XAxis,
                    YAxis,
                    ZAxis,

                    ZPlane
                },
                Name = "Translation Gizmo",
            };

            ObjectFlags |= ObjectFlags.Map3DDontReceiveShadow;
            ObjectFlags |= ObjectFlags.Map3DDontThrowShadow;
            ObjectFlags |= ObjectFlags.Map3DDontReceiveAmbient;
        }

        public bool KeyHandler(Key key, KeyStatus status)
        {
            if (key == Key.MouseKeyLeft)
            {
                if (_meshMouseover != null && status == KeyStatus.Down)
                {
                    _dragPointStart = GetPointAlongPlane();
                    _positionMoveStart = Position;
                    _virtualPos = Position;
                    _dragPointStartAbsolute = Position;
                    return false;
                }

                if (status == KeyStatus.Up)
                {
                    _dragPointStart = Vector3.Zero;
                    _startMovingEventFired = false;
                    return false;
                }
            }

            return true;
        }

        public void SetTarget(BaseGameObject? target)
        {
            Target = target;
            if (target != null)
            {
                Position = target.Position;

                float height = 35;
                float targetHeight = 35;
                if (Target is GameObject3D g3D)
                {
                    targetHeight = g3D.BoundingSphere.Radius / 2f;
                    targetHeight = Math.Max(targetHeight, 5f);
                }

                float scale = targetHeight / height;
                Size3D = new Vector3(scale);
            }
        }

        protected override void UpdateInternal(float dt)
        {
            base.UpdateInternal(dt);

            CameraBase? camera = Engine.Renderer.Camera;
            if (camera.Position != _lastCameraPos) _dragPointStart = Vector3.Zero;
            _lastCameraPos = camera.Position;

            // Update mouseover
            if (_dragPointStart == Vector3.Zero)
            {
                Ray3D ray = camera.GetCameraMouseRay();
                ray.IntersectWithObject(this, out Mesh? collidedMesh, out Vector3 _, out Vector3 _, out int _);

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

                _virtualPos += change;
                _dragPointStart = newPoint;

                Vector3 targetPos = _virtualPos;
                Position = _virtualPos;
                if (Target != null)
                {
                    Target.Position = targetPos;
                    TargetMoved?.Invoke(Target, _dragPointStartAbsolute, targetPos);

                    if (!_startMovingEventFired && change != Vector3.Zero)
                    {
                        TargetStartMoving?.Invoke(Target);
                        _startMovingEventFired = true;
                    }
                }
            }

            if (Target != null && _dragPointStart == Vector3.Zero)
                Position = Target.Position;
        }

        private Vector3 GetPointAlongPlane()
        {
            if (_meshMouseover == null) return Vector3.Zero;

            CameraBase? camera = Engine.Renderer.Camera;
            Ray3D ray = camera.GetCameraMouseRay();

            Vector3 intersection;
            if (_meshMouseover.Name == "Z-Plane")
            {
                intersection = ray.IntersectWithPlane(new Vector3(0, 0, 1), Position);
            }
            else
            {
                Vector3 axis = Vector3.Zero;
                if (_meshMouseover.Name == "X")
                    axis = RenderComposer.XAxis;
                else if (_meshMouseover.Name == "Y")
                    axis = RenderComposer.YAxis;
                else if (_meshMouseover.Name == "Z")
                    axis = RenderComposer.ZAxis;

                // Find plane that contains the axis and faces the camera.
                Vector3 planeTangent = Vector3.Cross(axis, Position - camera.Position);
                Vector3 planeNormal = Vector3.Cross(axis, planeTangent);
                planeNormal = planeNormal.Normalize();

                intersection = ray.IntersectWithPlane(planeNormal, Position);

                // Limit movement along axis
                intersection = Position + axis * Vector3.Dot(intersection - Position, axis);
            }

            return intersection;
        }

        public override void Destroy()
        {
            base.Destroy();
            Engine.Host.OnKey.RemoveListener(KeyHandler);
        }
    }
}