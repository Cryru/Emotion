#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Emotion.Utility;

#endregion

namespace Emotion.IO
{
    /// <summary>
    /// Work in progress.
    /// </summary>
    public class ObjMeshAsset : Asset
    {
        public Dictionary<string, List<Vector3>> Vertices;
        public Dictionary<string, List<object>> Parsed;

        protected override void CreateInternal(ReadOnlyMemory<byte> data)
        {
            Parsed = new Dictionary<string, List<object>>();
            Vertices = new Dictionary<string, List<Vector3>>();

            var stream = new ReadOnlyMemoryStream(data);
            var reader = new StreamReader(stream);
            var currentGroup = "base";
            while (!reader.EndOfStream)
            {
                string currentLine = reader.ReadLine();
                if (currentLine == null) continue;
                currentLine = currentLine.Trim();
                if (currentLine[0] == '#') continue; // Comment

                string[] args = currentLine.Split(' ');
                string identifier = args[0];
                if (!Parsed.TryGetValue(identifier, out List<object> list))
                {
                    list = new List<object>();
                    Parsed.Add(identifier, list);
                }

                int argCount = args.Length - 1;
                if (argCount == 3)
                {
                    float.TryParse(args[1], out float x);
                    float.TryParse(args[2], out float y);
                    float.TryParse(args[3], out float z);
                    var vec3 = new Vector3(x, y, z);
                    list.Add(vec3);
                    if (identifier == "v")
                    {
                        if (!Vertices.TryGetValue(currentGroup, out List<Vector3> vertices))
                        {
                            vertices = new List<Vector3>();
                            Vertices.Add(currentGroup, vertices);
                        }
                        vertices.Add(vec3);
                    }
                }
                else if (identifier == "g")
                {
                    currentGroup = args[1];
                }
                else
                {
                    list.Add(args);
                }
            }
        }

        protected override void DisposeInternal()
        {
            throw new NotImplementedException();
        }
    }
}