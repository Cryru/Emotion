#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text.RegularExpressions;
using Emotion.Common;
using Emotion.Graphics.Data;
using Emotion.Graphics.ThreeDee;
using Emotion.Standard.Logging;
using Emotion.Utility;

#endregion

namespace Emotion.IO
{
    /// <summary>
    /// Work in progress.
    /// </summary>
    public class ObjMeshAsset : Asset
    {
        public MeshEntity Entity
        {
            get => Entities != null && Entities.Length > 0 ? Entities[0] : null;
        }

        public MeshEntity[] Entities { get; protected set; }

        private class ObjFileBuildingSubObject
        {
            public string Name = "default";

            // Raw data.
            public List<Vector3> Normals;
            public List<Vector3> Vertices;
            public List<Vector2> UVs;
        }

        // Groups are a subset of data of the object. A file can contain more than one object and each can have many groups.
        // Some groups might be important to the overall representation of the object and should be rendered always, others are toggleable.
        private class ObjFileBuildingGroup
        {
            public string ObjName = "default";
            public string GroupName = "default";

            public Dictionary<string, ushort> IndexComboHash;
            public ushort IndexComboCount;
            public List<VertexData> VertexData;
            public List<ushort> VertexDataIndices;

            public MeshMaterial Material;

            public void Init()
            {
                if (IndexComboHash != null) return;

                IndexComboHash = new Dictionary<string, ushort>();
                VertexData = new List<VertexData>();
                VertexDataIndices = new List<ushort>();
            }
        }

        protected override void CreateInternal(ReadOnlyMemory<byte> data)
        {
            var groupBuild = new List<ObjFileBuildingGroup>();

            // Read the file.
            // todo: Use spans instead of splits.
            // https://en.wikipedia.org/wiki/Wavefront_.obj_file
            var stream = new ReadOnlyMemoryStream(data);
            var reader = new StreamReader(stream);

            var currentSubObject = new ObjFileBuildingSubObject();
            var currentGroup = new ObjFileBuildingGroup();
            groupBuild.Add(currentGroup);

            MtlAsset objMaterialBank = null;
            while (!reader.EndOfStream)
            {
                string currentLine = reader.ReadLine();
                if (string.IsNullOrEmpty(currentLine)) continue;
                currentLine = currentLine.Trim();
                if (currentLine == "" || currentLine[0] == '#') continue; // Comment

                string[] args = Regex.Replace(currentLine, @"\s+", " ").Split(' ');
                string identifier = args[0];

                switch (identifier)
                {
                    case "o":
                        currentSubObject = new ObjFileBuildingSubObject();
                        currentSubObject.Name = args[1];
                        break;
                    case "g":
                        currentGroup = new ObjFileBuildingGroup();
                        currentGroup.ObjName = currentSubObject.Name;
                        currentGroup.GroupName = args[1];
                        groupBuild.Add(currentGroup);
                        break;
                    case "mtllib":
                        string path = AssetLoader.JoinPath(AssetLoader.GetDirectoryName(Name), args[1]);
                        objMaterialBank = Engine.AssetLoader.Get<MtlAsset>(path);
                        break;
                    case "usemtl" when objMaterialBank != null:
                        currentGroup.Material = objMaterialBank.GetMaterial(args[1]);
                        break;
                    case "v":
                    case "vn":
                        float.TryParse(args[1], out float x);
                        float.TryParse(args[2], out float y);
                        float.TryParse(args[3], out float z);
                        var vec3 = new Vector3(x, -y, -z); // -Y is up
                        if (identifier == "v")
                        {
                            currentSubObject.Vertices ??= new List<Vector3>();
                            currentSubObject.Vertices.Add(vec3);
                        }
                        else if (identifier == "vn")
                        {
                            currentSubObject.Normals ??= new List<Vector3>();
                            currentSubObject.Normals.Add(vec3);
                        }

                        break;
                    case "vt":
                        float.TryParse(args[1], out float uvX);
                        float.TryParse(args[2], out float uvY);
                        var vec2 = new Vector2(uvX, uvY);
                        currentSubObject.UVs ??= new List<Vector2>();
                        currentSubObject.UVs.Add(vec2);
                        break;
                    case "f":
                        currentGroup.Init();

                        Debug.Assert(args.Length == 4); // Triangle faces expected.
                        for (var i = 1; i < args.Length; i++)
                        {
                            string combo = args[i];

                            // Check if triangle is unique.
                            if (currentGroup.IndexComboHash.TryGetValue(combo, out ushort idx))
                            {
                                currentGroup.VertexDataIndices.Add(idx);
                                continue;
                            }

                            currentGroup.IndexComboHash.Add(combo, currentGroup.IndexComboCount);
                            currentGroup.IndexComboCount++;
                            currentGroup.VertexDataIndices.Add((ushort) currentGroup.VertexData.Count);
                            var vtxData = new VertexData
                            {
                                Color = (currentGroup.Material?.DiffuseColor ?? MeshMaterial.DefaultMaterial.DiffuseColor).ToUint()
                            };

                            // v/vt, v/vt/vn, v//vn
                            string[] spl = args[i].Split('/');
                            if (spl.Length > 0 && ushort.TryParse(spl[0], out ushort vertexIdx))
                            {
                                if (vertexIdx <= currentSubObject.Vertices.Count)
                                    vtxData.Vertex = currentSubObject.Vertices[vertexIdx - 1];
                                else
                                    Engine.Log.Warning($"Referenced vertex out of range ({vertexIdx}) in {Name}/{currentSubObject.Name}.", MessageSource.ObjFile);
                            }

                            if (spl.Length > 1 && spl[1] != "" && ushort.TryParse(spl[1], out ushort uvIdx))
                            {
                                if (uvIdx <= currentSubObject.UVs.Count)
                                    vtxData.UV = currentSubObject.UVs[uvIdx - 1];
                                else
                                    Engine.Log.Warning($"Referenced vertex out of range ({uvIdx}) in {Name}/{currentSubObject.Name}.", MessageSource.ObjFile);
                            }

                            if (spl.Length > 2 && ushort.TryParse(spl[2], out ushort _))
                            {
                                // vtxData.Normal = currentSubObject.Normals[normalIdx - 1];
                            }

                            currentGroup.VertexData.Add(vtxData);
                        }

                        break;
                }
            }

            // Process objects into Emotion readable format.
            var validEntitiesAndMeshes = new Dictionary<string, List<ObjFileBuildingGroup>>();
            for (var i = 0; i < groupBuild.Count; i++)
            {
                ObjFileBuildingGroup obj = groupBuild[i];
                if (obj.VertexDataIndices == null) continue;

                if (validEntitiesAndMeshes.TryGetValue(obj.ObjName, out List<ObjFileBuildingGroup> meshes))
                    meshes.Add(obj);
                else
                    validEntitiesAndMeshes.Add(obj.ObjName, new List<ObjFileBuildingGroup> {obj});
            }

            Entities = new MeshEntity[validEntitiesAndMeshes.Count];
            var entityIdx = 0;
            foreach ((string entityName, List<ObjFileBuildingGroup> entityMeshes) in validEntitiesAndMeshes)
            {
                var entity = new MeshEntity
                {
                    Name = entityName,
                    Meshes = new Mesh[entityMeshes.Count]
                };

                for (var i = 0; i < entityMeshes.Count; i++)
                {
                    ObjFileBuildingGroup builtMesh = entityMeshes[i];
                    var mesh = new Mesh
                    {
                        Name = builtMesh.GroupName,
                        Vertices = builtMesh.VertexData.GetRange(0, Math.Min(builtMesh.VertexData.Count, ushort.MaxValue)).ToArray(),
                        Indices = builtMesh.VertexDataIndices.GetRange(0, Math.Min(builtMesh.VertexDataIndices.Count, ushort.MaxValue)).ToArray(),
                        Material = builtMesh.Material ?? new MeshMaterial()
                    };

                    // Flip texture UVs.
                    for (var v = 0; v < mesh.Vertices.Length; v++)
                    {
                        Vector2 uv = mesh.Vertices[v].UV;
                        mesh.Vertices[v].UV = new Vector2(uv.X, 1.0f - uv.Y);
                    }

                    entity.Meshes[i] = mesh;
                }

                Entities[entityIdx] = entity;
                entityIdx++;
            }
        }

        protected override void DisposeInternal()
        {
            throw new NotImplementedException();
        }
    }
}