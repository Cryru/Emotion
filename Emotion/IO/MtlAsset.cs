#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Emotion.Common;
using Emotion.Graphics.Objects;
using Emotion.Primitives;
using Emotion.Utility;

#endregion

namespace Emotion.IO
{
    public class MtlAsset : Asset
    {
        public class MtlMaterial
        {
            public string Name;
            public Texture Texture;
            public Color DiffuseColor = Color.White;

            public static MtlMaterial DefaultMaterial = new MtlMaterial();
        }

        public List<MtlMaterial> Materials;

        public MtlMaterial GetMaterial(string name)
        {
            for (var i = 0; i < Materials.Count; i++)
            {
                if (Materials[i].Name == name) return Materials[i];
            }

            return null;
        }

        protected override void CreateInternal(ReadOnlyMemory<byte> data)
        {
            Materials = new List<MtlMaterial>();

            // Read the file.
            // todo: Use spans instead of splits.
            // https://en.wikipedia.org/wiki/Wavefront_.obj_file
            var stream = new ReadOnlyMemoryStream(data);
            var reader = new StreamReader(stream);
            MtlMaterial currentMaterial = null;
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
                    case "newmtl":
                    {
                        var newMat = new MtlMaterial
                        {
                            Name = args[1]
                        };
                        currentMaterial = newMat;
                        Materials.Add(newMat);
                        break;
                    }
                    case "map_Kd" when currentMaterial != null:
                    {
                        string directory = AssetLoader.GetDirectoryName(Name);
                        string texturePath = AssetLoader.GetNonRelativePath(directory, AssetLoader.NameToEngineName(args[1]));
                        var texture = Engine.AssetLoader.Get<TextureAsset>(texturePath);
                        if (texture != null)
                        {
                            texture.Texture.Tile = true;
                            texture.Texture.Smooth = true;
                            currentMaterial.Texture = texture?.Texture;
                        }
                        break;
                    }
                }
            }
        }

        protected override void DisposeInternal()
        {
        }
    }
}