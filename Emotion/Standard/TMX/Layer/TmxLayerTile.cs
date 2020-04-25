namespace Emotion.Standard.TMX.Layer
{
    public class TmxLayerTile
    {
        // Tile flip bit flags
        private const uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
        private const uint FLIPPED_VERTICALLY_FLAG = 0x40000000;
        private const uint FLIPPED_DIAGONALLY_FLAG = 0x20000000;

        public int Gid { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public bool HorizontalFlip { get; private set; }
        public bool VerticalFlip { get; private set; }
        public bool DiagonalFlip { get; private set; }

        public TmxLayerTile(uint id, int x, int y)
        {
            uint rawGid = id;
            X = x;
            Y = y;

            // Scan for tile flip bit flags
            bool flip = (rawGid & FLIPPED_HORIZONTALLY_FLAG) != 0;
            HorizontalFlip = flip;

            flip = (rawGid & FLIPPED_VERTICALLY_FLAG) != 0;
            VerticalFlip = flip;

            flip = (rawGid & FLIPPED_DIAGONALLY_FLAG) != 0;
            DiagonalFlip = flip;

            // Zero the bit flags
            rawGid &= ~(FLIPPED_HORIZONTALLY_FLAG |
                        FLIPPED_VERTICALLY_FLAG |
                        FLIPPED_DIAGONALLY_FLAG);

            // Save GID remainder to int
            Gid = (int) rawGid;
        }
    }
}