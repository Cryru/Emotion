#nullable enable

#region Using

using Emotion.Game.World;
using Emotion.Game.World.Components;
using Emotion.Game.World.ThreeDee;
using Emotion.Graphics.Camera;
using Emotion.Standard.MeshGenerators;

#endregion

namespace Emotion.Editor.EditorUI.MapObjectEditor;

[DontSerialize]
public class TranslationGizmo : GameObject
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
    public GameObject? Target { get; protected set; }

    public Mesh XAxis { get; protected set; }

    public Mesh YAxis { get; protected set; }

    public Mesh ZAxis { get; protected set; }

    public Mesh ZPlane { get; protected set; }

    public Action<GameObject, Vector3, Vector3>? TargetMoved;
    public Action<GameObject>? TargetStartMoving;

    protected bool _startMovingEventFired;

    protected Mesh? _meshMouseover;
    protected Vector3 _dragPointStart;
    protected Vector3 _dragPointStartAbsolute;
    protected Vector3 _virtualPos;
    protected Vector3 _positionMoveStart;

    protected Vector3 _lastCameraPos;

    public TranslationGizmo()
    {
        MeshEntity translationGizmoEntity = GetTranslationGizmoEntity();
        XAxis = translationGizmoEntity.GetMeshByName("X")!;
        YAxis = translationGizmoEntity.GetMeshByName("Y")!;
        ZAxis = translationGizmoEntity.GetMeshByName("Z")!;

        ZPlane = translationGizmoEntity.GetMeshByName("Z-Plane")!;

        //foreach (Mesh mesh in translationGizmoEntity.Meshes)
        //{
        //    mesh.Material.DiffuseColor = mesh.Material.DiffuseColor.SetAlpha(100);
        //}

        var meshComponent = new MeshComponent(translationGizmoEntity);
        AddComponent(meshComponent);

        //SetEntity(translationGizmoEntity);
        //RenderState.CustomRenderState
        //RenderState.CustomObjectFlags
        Scale3D = new Vector3(0.1f);

        //ObjectFlags |= ObjectFlags.Map3DDontReceiveShadow;
        //ObjectFlags |= ObjectFlags.Map3DDontThrowShadow;
        //ObjectFlags |= ObjectFlags.Map3DDontReceiveAmbient;
    }

    private const float ALMOST_ONE = 0.999f;

    public static MeshEntity GetTranslationGizmoEntity(float height = 30, float arrowHeight = 10, bool planes = true)
    {
        var arrowCylinderGen = new CylinderMeshGenerator();
        arrowCylinderGen.RadiusBottom = 3;
        arrowCylinderGen.RadiusTop = 3;
        arrowCylinderGen.Height = height;
        arrowCylinderGen.Capped = true;

        var arrowGen = new CylinderMeshGenerator();
        arrowGen.RadiusBottom = 5f;
        arrowGen.RadiusTop = 0;
        arrowGen.Height = arrowHeight;
        arrowGen.Capped = true;

        Mesh xCylinder = arrowCylinderGen.GenerateMesh().TransformMeshVertices(
            Matrix4x4.CreateFromYawPitchRoll(Maths.DegreesToRadians(90), 0f, 0f)
        );
        Mesh xArrow = arrowGen.GenerateMesh().TransformMeshVertices(
            Matrix4x4.CreateFromYawPitchRoll(Maths.DegreesToRadians(90), 0f, 0f) *
            Matrix4x4.CreateTranslation(arrowCylinderGen.Height * ALMOST_ONE, 0, 0)
        );
        var meshXAxis = Mesh.CombineMeshes(xCylinder, xArrow, "X");

        Mesh yCylinder = arrowCylinderGen.GenerateMesh("YCylinder").TransformMeshVertices(
            Matrix4x4.CreateFromYawPitchRoll(0, Maths.DegreesToRadians(-90), 0f)
        );
        Mesh yArrow = arrowGen.GenerateMesh("YArrow").TransformMeshVertices(
            Matrix4x4.CreateFromYawPitchRoll(0, Maths.DegreesToRadians(-90), 0f) *
            Matrix4x4.CreateTranslation(0, arrowCylinderGen.Height * ALMOST_ONE, 0)
        );
        var meshYAxis = Mesh.CombineMeshes(yCylinder, yArrow, "Y");

        Mesh zCylinder = arrowCylinderGen.GenerateMesh("ZCylinder");
        Mesh zArrow = arrowGen.GenerateMesh("ZArrow").TransformMeshVertices(
            Matrix4x4.CreateTranslation(0, 0, arrowCylinderGen.Height * ALMOST_ONE)
        );
        var meshZAxis = Mesh.CombineMeshes(zCylinder, zArrow, "Z");

        var materialX = new MeshMaterial
        {
            Name = "Material-Tool-X",
            DiffuseColor = new Color(165, 40, 40)
        };
        meshXAxis.Material = materialX;

        var materialY = new MeshMaterial
        {
            Name = "Material-Tool-Y",
            DiffuseColor = new Color(40, 165, 40)
        };
        meshYAxis.Material = materialY;

        var materialZ = new MeshMaterial
        {
            Name = "Material-Tool-Z",
            DiffuseColor = new Color(40, 40, 165)
        };
        meshZAxis.Material = materialZ;

        Mesh[] meshes;
        if (planes)
        {
            var materialPlaneZ = new MeshMaterial
            {
                Name = "Material-Plane-Z",
                DiffuseColor = Color.Blue,
                State =
                {
                    FaceCulling = false
                }
            };
            Mesh meshZPlane = Mesh.ShallowCopyMesh_DeepCopyVertexData(Quad3D.QuadEntity.Meshes[0]);
            meshZPlane.TransformMeshVertices(
                Matrix4x4.CreateScale(30, 30, 30) *
                Matrix4x4.CreateTranslation(arrowCylinderGen.Height / 2, arrowCylinderGen.Height / 2, 0)
            );
            meshZPlane.Material = materialPlaneZ;
            meshZPlane.Name = "Z-Plane";

            meshes = new[]
            {
                meshXAxis,
                meshYAxis,
                meshZAxis,

                meshZPlane
            };
        }
        else
        {
            meshes = new[]
            {
                meshXAxis,
                meshYAxis,
                meshZAxis,
            };
        }

        return new MeshEntity
        {
            Meshes = meshes,
            Name = "Translation Gizmo",
        };
    }

    public bool KeyHandler(Key key, KeyState status)
    {
        if (key == Key.MouseKeyLeft)
        {
            if (_meshMouseover != null && status == KeyState.Down)
            {
                _dragPointStart = GetPointAlongPlane();
                _positionMoveStart = Position3D;
                _virtualPos = Position3D;
                _dragPointStartAbsolute = Position3D;
                return false;
            }

            if (status == KeyState.Up)
            {
                _dragPointStart = Vector3.Zero;
                _startMovingEventFired = false;
                return false;
            }
        }

        return true;
    }

    public void SetTarget(GameObject? target)
    {
        Target = target;
        if (target != null)
        {
            Position3D = target.Position3D;

            float height = 35;
            float targetHeight = 35;
            targetHeight = target.GetBoundingSphere().Radius / 2f;
            targetHeight = Math.Max(targetHeight, 5f);
            targetHeight = Math.Min(targetHeight, 300f);

            float scale = targetHeight / height;
            Scale3D = new Vector3(scale);
        }
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        CameraBase? camera = Engine.Renderer.Camera;
        if (camera.Position != _lastCameraPos) _dragPointStart = Vector3.Zero;
        _lastCameraPos = camera.Position;

        // Update mouseover
        if (_dragPointStart == Vector3.Zero)
        {
            Ray3D ray = camera.GetCameraMouseRay();
            ray.IntersectWithObject(this, this.GetComponent<MeshComponent>(), out Mesh? collidedMesh, out Vector3 _, out Vector3 _, out int _, true);

            if (_meshMouseover != collidedMesh)
            {
                _meshMouseover?.SetVerticesAlpha((byte)Alpha);
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
            Position3D = _virtualPos;
            if (Target != null)
            {
                Target.Position3D = targetPos;
                TargetMoved?.Invoke(Target, _dragPointStartAbsolute, targetPos);

                if (!_startMovingEventFired && change != Vector3.Zero)
                {
                    TargetStartMoving?.Invoke(Target);
                    _startMovingEventFired = true;
                }
            }
        }

        if (Target != null && _dragPointStart == Vector3.Zero)
            Position3D = Target.Position3D;
    }

    private Vector3 GetPointAlongPlane()
    {
        if (_meshMouseover == null) return Vector3.Zero;

        CameraBase? camera = Engine.Renderer.Camera;
        Ray3D ray = camera.GetCameraMouseRay();

        Vector3 intersection;
        if (_meshMouseover.Name == "Z-Plane")
        {
            intersection = ray.IntersectWithPlane(new Vector3(0, 0, 1), Position3D);
        }
        else
        {
            Vector3 axis = Vector3.Zero;
            if (_meshMouseover.Name == "X")
                axis = Graphics.Renderer.XAxis;
            else if (_meshMouseover.Name == "Y")
                axis = Graphics.Renderer.YAxis;
            else if (_meshMouseover.Name == "Z")
                axis = Graphics.Renderer.ZAxis;

            // Find plane that contains the axis and faces the camera.
            Vector3 planeTangent = Vector3.Cross(axis, Position3D - camera.Position);
            Vector3 planeNormal = Vector3.Cross(axis, planeTangent);
            planeNormal = planeNormal.SafeNormalize();

            intersection = ray.IntersectWithPlane(planeNormal, Position3D);

            // Limit movement along axis
            intersection = Position3D + axis * Vector3.Dot(intersection - Position3D, axis);
        }

        return intersection;
    }

    public override void Done()
    {
        base.Done();
        Engine.Host.OnKey.RemoveListener(KeyHandler);
    }
}