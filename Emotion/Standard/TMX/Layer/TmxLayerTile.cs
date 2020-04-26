namespace Emotion.Standard.TMX.Layer
{
    public class TmxLayerTile
    {
        public int Gid { get; private set; }
        public bool HorizontalFlip;
        public bool VerticalFlip;
        public bool DiagonalFlip;

        public TmxLayerTile(uint id)
        {
            Gid = TmxHelpers.GetGidFlags(id, out HorizontalFlip, out VerticalFlip, out DiagonalFlip);
        }
    }
}