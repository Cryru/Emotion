#nullable enable

#region Using

using Emotion.Game.Systems.Animation.ThreeDee;
using Emotion.Graphics.Data;
using Emotion.ThirdParty.TopologicalSort;

#endregion

namespace Emotion.Game.World.ThreeDee;

/// <summary>
/// A collection of meshes which make up one visual object.
/// Not all of the meshes are always visible.
/// </summary>
public class MeshEntity
{
    private const string DEFAULT_ENTITY_NAME = "Untitled";

    public string Name { get; set; } = string.Empty;

    public float Scale { get; set; } = 1f;

    public Matrix4x4 LocalTransform { get; set; } = Matrix4x4.Identity;

    public Mesh[] Meshes { get; set; } = Array.Empty<Mesh>();

    public Vector3 Forward = Renderer.Forward;

    public SkeletalAnimationSkin[] AnimationSkins { get; set; } = Array.Empty<SkeletalAnimationSkin>();

    // Animation
    public SkeletalAnimation[] Animations { get; set; } = Array.Empty<SkeletalAnimation>();

    public SkeletonAnimRigNode[] AnimationRig { get; set; } = Array.Empty<SkeletonAnimRigNode>();

    // Render settings
    public bool BackFaceCulling { get; set; } = true; // todo: move to material

    // Caches
    private Dictionary<string, (Sphere, Cube)> _cachedBounds = new();

    public MeshEntity(Mesh[] meshes, string? name = null)
    {
        Meshes = meshes;
        Name = name ?? DEFAULT_ENTITY_NAME;
    }

    public MeshEntity()
    {

    }

    public Mesh? GetMeshByName(string id)
    {
        if (Meshes == null) return null;
        for (int i = 0; i < Meshes.Length; i++)
        {
            Mesh mesh = Meshes[i];
            if (mesh.Name == id) return mesh;
        }
        return null;
    }

    public void EnsureCachedBounds(string? anim, bool forceRecalc = false)
    {
        if (anim == null) anim = "<null>";

        if (!forceRecalc && _cachedBounds.ContainsKey(anim))
            return;

        lock (this)
        {
            // Recheck after obtaining lock.
            if (!forceRecalc && _cachedBounds.ContainsKey(anim))
                return;

            CalculateBounds(anim, out Sphere sphere, out Cube cube);
            _cachedBounds.Add(anim, (sphere, cube));
        }
    }

    /// <summary>
    /// Sets the bounding sphere and cube for the specified animation.
    /// </summary>
    public void SetCachedBounds(string? anim, Sphere sphere, Cube cube)
    {
        anim ??= "<null>";
        _cachedBounds.Add(anim, (sphere, cube));
    }

    public void ResetCachedBounds()
    {
        _cachedBounds.Clear();
    }

    /// <summary>
    /// Returns the bounds of the entity in a specific animation.
    /// </summary>
    public void GetBounds(string? anim, out Sphere sphere, out Cube cube)
    {
        sphere = new Sphere();
        cube = new Cube();

        EnsureCachedBounds(anim);

        lock (this)
        {
            anim ??= "<null>";
            if (_cachedBounds.TryGetValue(anim, out (Sphere, Cube) bounds))
            {
                sphere = bounds.Item1;
                cube = bounds.Item2;
            }
        }
    }

    protected void CalculateBounds(string anim, out Sphere sphere, out Cube cube)
    {
        var first = true;
        var min = new Vector3(0);
        var max = new Vector3(0);

        IEnumerator<Vector3> vertices = IterateAllMeshVertices(anim);
        while (vertices.MoveNext())
        {
            Vector3 vertex = vertices.Current;
            if (first)
            {
                min = vertex;
                max = vertex;
                first = false;
            }
            else
            {
                // Find the minimum and maximum extents of the vertices
                min = Vector3.Min(min, vertex);
                max = Vector3.Max(max, vertex);
            }
        }

        Vector3 center = (min + max) / 2f;

        float radius = Vector3.Distance(center, max);
        sphere = new Sphere(center, radius);

        Vector3 halfExtent = (max - min) / 2f;
        cube = new Cube(center, halfExtent);
    }

    protected IEnumerator<Vector3> IterateAllMeshVertices(string animation)
    {
        Mesh[]? meshes = Meshes;
        if (meshes == null) yield break;

        var boneMatricesPerMesh = new Matrix4x4[meshes.Length][];
        for (var i = 0; i < meshes.Length; i++)
        {
            Mesh mesh = meshes[i];
            VertexDataAllocation vertexAllocation = mesh.VertexAllocation;
            foreach (var vPos in vertexAllocation.ForEachVertexPosition())
            {
                yield return vPos;
            }
        }

        // todo: animation
    }

    public static void PostProcess_FixAnimationRigOrder(MeshEntity entity)
    {
        SkeletonAnimRigNode[] rig = entity.AnimationRig;

        var nodeIds = new List<int>();
        var dependencies = new List<(int, int)>();
        for (int i = 0; i < entity.AnimationRig.Length; i++)
        {
            SkeletonAnimRigNode rigNode = entity.AnimationRig[i];
            nodeIds.Add(i);

            if (rigNode.ParentIdx != -1)
                dependencies.Add((rigNode.ParentIdx, i));
        }

        // index is new order, value is old index
        List<int> sorted = TopologicalSort.Sort(nodeIds, dependencies);

        // Construct the new list - update parent dependencies
        SkeletonAnimRigNode[] newRig = new SkeletonAnimRigNode[rig.Length];
        for (int i = 0; i < newRig.Length; i++)
        {
            int oldRigNodeIdx = sorted[i];
            SkeletonAnimRigNode oldRigNode = rig[oldRigNodeIdx];
            if (oldRigNode.ParentIdx != -1)
            {
                int parentIdxNew = sorted.IndexOf(oldRigNode.ParentIdx);
                Assert(parentIdxNew != -1);
                oldRigNode.ParentIdx = parentIdxNew;
            }
            newRig[i] = oldRigNode;
        }
        entity.AnimationRig = newRig;

        // Update skins
        SkeletalAnimationSkin[] skins = entity.AnimationSkins;
        for (int i = 0; i < skins.Length; i++)
        {
            SkeletalAnimationSkin skin = skins[i];
            SkeletalAnimationSkinJoint[] joints = skin.Joints;
            for (int j = 0; j < joints.Length; j++)
            {
                ref SkeletalAnimationSkinJoint joint = ref joints[j];
                joint.RigNodeIdx = sorted.IndexOf(joint.RigNodeIdx);
            }
        }

        // Update animation channels
        // todo: need entity for testing
    }
}