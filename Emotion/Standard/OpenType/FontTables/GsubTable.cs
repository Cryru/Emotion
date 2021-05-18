#region Using

using Emotion.Utility;

#endregion

namespace Emotion.Standard.OpenType.FontTables
{
    public class GsubTable
    {
        public ushort Count;

        public byte[] GsubData;

        /// <summary>
        /// Parse the `Gsub` table.
        /// </summary>
        public GsubTable(ByteReader reader)
        {
            reader.ReadOpenTypeVersionBE();
            Count = reader.ReadUShortBE();

            if (Count != 0)
            {
                byte offsize = reader.ReadByte();
                reader.ReadBytes(offsize * Count);
                uint offset = reader.ReadVariableUIntBE(offsize);
                reader.ReadBytes((int) offset - 1);
            }

            // todo
        }
    }
}