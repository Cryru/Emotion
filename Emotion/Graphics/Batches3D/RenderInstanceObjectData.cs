﻿#nullable enable

#region Using

using Emotion.Game.ThreeDee;

#endregion

namespace Emotion.Graphics.Batches3D;

// Per GameObject3D
public unsafe struct RenderInstanceObjectData
{
    public bool BackfaceCulling;
    public bool BackfaceCullingFrontFace;
    public MeshEntityMetaState MetaState;
    public Matrix4x4 ModelMatrix;
    public float DistanceToCamera;

    public fixed bool FrustumCulling[MeshEntityBatchRenderer.CASCADE_COUNT + 1];
    public Sphere FrustumCullingSphere;
}