﻿#nullable enable

#region Using

using Emotion.Graphics.Shading;
using Emotion.Graphics.ThreeDee;
using Emotion.Utility;

#endregion

namespace Emotion.Graphics.Batches3D;

// Per mesh
public struct RenderInstanceMeshData : IStructArenaLinkedListItem
{
    public int NextItem { get; set; }

    public int ObjectRegistrationId; // -> RenderInstanceObjectData
    public Matrix4x4[]? BoneData;
}

// Per mesh (transparent mode)
public struct RenderInstanceMeshDataTransparent
{
    public Mesh Mesh;
    public ShaderProgram Shader;
    public bool UploadMetaStateToShader;

    public int ObjectRegistrationId; // -> RenderInstanceObjectData
    public Matrix4x4[]? BoneData;
}