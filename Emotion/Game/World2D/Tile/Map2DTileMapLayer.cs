﻿#region Using

using System.Text;
using Emotion.Common.Serialization;
using Emotion.Editor;

#endregion

#nullable enable

namespace Emotion.Game.World2D.Tile
{
    /// <summary>
    /// Represents a single tile layer.
    /// </summary>
    public partial class Map2DTileMapLayer
    {
        /// <summary>
        /// The layer's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Whether the layer is visible.
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// The layer's opacity.
        /// </summary>
        public float Opacity { get; set; } = 1f;

        /// <summary>
        /// Data in layers is held packed in this property, and this
        /// is the property that is serialized/deserialized.
        /// However during runtime the data being operated upon is the _unpackedData member.
        /// </summary>
        [DontShowInEditor]
        [SerializeNonPublicGetSet]
        public string StringData { get => PackData(); set => _unpackedData = UnpackData(value); }

        protected uint[] _unpackedData = Array.Empty<uint>();

        /// <summary>
        /// The stride of the data, AKA the map's width. Used for tracking map resizes
        /// </summary>
        [DontShowInEditor]
        public int DataStride { get; set; } = -1;

        public Map2DTileMapLayer(string name, uint[] data)
        {
            Name = name;
            _unpackedData = data;
        }

        // Serialization constructor
        protected Map2DTileMapLayer()
        {
            Name = "";
        }

        /// <summary>
        /// Gets the tile data for this layer.
        /// For use only by Map2DTileMapData.
        /// Users should use its GetTileData method.
        /// </summary>
        public uint[] GetUnpackedTileData()
        {
            return _unpackedData;
        }

        /// <summary>
        /// Sets the tile data for this layer.
        /// For use only by Map2DTileMapData.
        /// Users should use its SetTileData method.
        /// </summary>
        public void SetUnpackedTileData(uint[] data)
        {
            _unpackedData = data;
        }

        #region Data Packing

        private uint[] UnpackData(string data)
        {
            // First pass - Count characters, including packed.
            var chars = 0;
            var lastSepIdx = 0;
            var charCount = 1;
            for (var i = 0; i < data.Length; i++)
            {
                char c = data[i];
                if (c == 'x')
                {
                    ReadOnlySpan<char> sinceLast = data.AsSpan(lastSepIdx, i - lastSepIdx);
                    if (int.TryParse(sinceLast, out int countPacked)) charCount = countPacked;
                }
                else if (c == ',')
                {
                    chars += charCount;
                    charCount = 1;
                    lastSepIdx = i + 1;
                }
            }

            chars += charCount;

            // Second pass, unpack.
            var unpackedData = new uint[chars];
            var arrayPtr = 0;
            lastSepIdx = 0;
            charCount = 1;
            for (var i = 0; i < data.Length; i++)
            {
                char c = data[i];
                if (c == 'x')
                {
                    ReadOnlySpan<char> sinceLast = data.AsSpan(lastSepIdx, i - lastSepIdx);
                    if (int.TryParse(sinceLast, out int countPacked))
                    {
                        charCount = countPacked;
                        lastSepIdx = i + 1;
                    }
                }
                else if (c == ',' || i == data.Length - 1)
                {
                    // Dumping last character, pretend the index is after the string so we
                    // read the final char below.
                    if (i == data.Length - 1) i++;

                    // Get tile value.
                    ReadOnlySpan<char> sinceLast = data.AsSpan(lastSepIdx, i - lastSepIdx);
                    uint.TryParse(sinceLast, out uint value);

                    for (var j = 0; j < charCount; j++)
                    {
                        unpackedData[arrayPtr] = value;
                        arrayPtr++;
                    }

                    charCount = 1;
                    lastSepIdx = i + 1;
                }
            }

            return unpackedData;
        }

        private string PackData()
        {
            uint[]? data = _unpackedData;
            if (data == null || data.Length == 0) return "";

            var b = new StringBuilder(data.Length * 2 + data.Length - 1);

            uint lastNumber = data[0];
            uint lastNumberCount = 1;
            var firstAppended = false;
            for (var i = 1; i <= data.Length; i++)
            {
                // There is an extra loop to dump last number.
                uint num = 0;
                if (i != data.Length)
                {
                    num = data[i];
                    // Same number as before, increment counter.
                    if (num == lastNumber)
                    {
                        lastNumberCount++;
                        continue;
                    }
                }

                if (firstAppended) b.Append(",");
                if (lastNumberCount == 1)
                {
                    // "0"
                    b.Append(lastNumber);
                }
                else
                {
                    // "2x0" = "0, 0"
                    b.Append(lastNumberCount);
                    b.Append('x');
                    b.Append(lastNumber);
                }

                lastNumber = num;
                lastNumberCount = 1;
                firstAppended = true;
            }

            return b.ToString();
        }

        #endregion

        public override string ToString()
        {
            if (Name == "") return "Unnamed Tile Layer";
            return Name;
        }
    }
}