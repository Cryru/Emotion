#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text.RegularExpressions;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.Primitives;
using Emotion.Standard.Logging;
using Emotion.Utility;
using OpenGL;

#endregion

namespace Emotion.IO
{
    /// <summary>
    /// Work in progress.
    /// </summary>
    public class ObjMeshAsset : Asset
    {
        public Dictionary<string, ObjFileSubObject> Objects;

        public class ObjFileSubObject
        {
            public string ObjName;
            public string GroupName;

            public VertexData[] Vertices;
            public ushort[] Indices;
            public MtlAsset.MtlMaterial Material;
        }

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

            public List<string> IndexCombinations;
            public List<VertexData> VertexData;
            public List<ushort> VertexDataIndices;

            public MtlAsset.MtlMaterial Material;

            public void Init()
            {
                if (IndexCombinations != null) return;

                IndexCombinations = new List<string>();
                VertexData = new List<VertexData>();
                VertexDataIndices = new List<ushort>();
            }
        }

        protected override void CreateInternal(ReadOnlyMemory<byte> data)
        {
            //var objectBuild = new List<ObjFileBuildingSubObject>();
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
                        var vec3 = new Vector3(x, y, z);
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

                            // Check if vertex is unique.
                            ushort? idx = null;
                            for (ushort j = 0; j < currentGroup.IndexCombinations.Count; j++)
                            {
                                if (currentGroup.IndexCombinations[j] == combo)
                                {
                                    idx = j;
                                    break;
                                }
                            }

                            // Non unique index.
                            if (idx != null)
                            {
                                currentGroup.VertexDataIndices.Add(idx.Value);
                                continue;
                            }

                            currentGroup.IndexCombinations.Add(combo);
                            currentGroup.VertexDataIndices.Add((ushort) currentGroup.VertexData.Count);
                            var vtxData = new VertexData
                            {
                                Color = (currentGroup.Material?.DiffuseColor ?? MtlAsset.MtlMaterial.DefaultMaterial.DiffuseColor).ToUint()
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
            Objects = new Dictionary<string, ObjFileSubObject>();
            for (var i = 0; i < groupBuild.Count; i++)
            {
                ObjFileBuildingGroup obj = groupBuild[i];
                if (obj.VertexDataIndices == null) continue;

                var newObj = new ObjFileSubObject
                {
                    GroupName = obj.GroupName,
                    ObjName = obj.ObjName,
                    Vertices = obj.VertexData.ToArray(),
                    Indices = obj.VertexDataIndices.ToArray(),
                    Material = obj.Material
                };
                for (var v = 0; v < newObj.Vertices.Length; v++)
                {
                    Vector2 uv = newObj.Vertices[v].UV;
                    newObj.Vertices[v].UV = new Vector2(uv.X, 1.0f - uv.Y);
                }

                Objects.Add($"{newObj.GroupName}@{newObj.ObjName}", newObj);
            }
        }

        public void DebugDraw(RenderComposer c)
        {
            c.FarZ = 10000;
            c.SetUseViewMatrix(false);
            c.RenderSprite(new Vector3(0, 0, 0), Engine.Renderer.CurrentTarget.Size, Color.CornflowerBlue);
            c.SetUseViewMatrix(true);
            c.ClearDepth();

            // These need to be rendered with the camera's projection.
            // todo: camera projection state, smart: when camera is on use camera, else default, off always use default, on always use camera.
            // c.RenderLineScreenSpace(new Vector3(short.MinValue, 0, 0), new Vector3(short.MaxValue, 0, 0), Color.Red);
            // c.RenderLineScreenSpace(new Vector3(0, short.MinValue, 0), new Vector3(0, short.MaxValue, 0), Color.Green);
            // c.RenderLineScreenSpace(new Vector3(0, 0, short.MinValue), new Vector3(0, 0, short.MaxValue), Color.Blue);
            c.FlushRenderStream();

            // todo: culling state
            Gl.Enable(EnableCap.CullFace);
            Gl.CullFace(CullFaceMode.Back);
            Gl.FrontFace(FrontFaceDirection.Ccw);

            c.PushModelMatrix(Matrix4x4.CreateScale(30, 30, 30));
            foreach (KeyValuePair<string, ObjFileSubObject> objPair in Objects)
            {
                ObjFileSubObject obj = objPair.Value;
                VertexData[] vertData = obj.Vertices;
                ushort[] indices = obj.Indices;
                Texture texture = null;
                if (obj.Material != null && obj.Material.Texture != null) texture = obj.Material.Texture;
                RenderStreamBatch<VertexData>.StreamData memory = c.RenderStream.GetStreamMemory((uint) vertData.Length, (uint) indices.Length, BatchMode.SequentialTriangles, texture);

                vertData.CopyTo(memory.VerticesData);
                indices.CopyTo(memory.IndicesData);

                ushort structOffset = memory.StructIndex;
                for (var j = 0; j < memory.IndicesData.Length; j++)
                {
                    memory.IndicesData[j] = (ushort) (memory.IndicesData[j] + structOffset);
                }
            }

            c.PopModelMatrix();
            Gl.Disable(EnableCap.CullFace);
        }

        protected override void DisposeInternal()
        {
            throw new NotImplementedException();
        }
    }
}