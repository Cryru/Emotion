namespace Emotion.Platform
{
    /// <summary>
    /// Describes framebuffers and their properties.
    /// </summary>
    public sealed class FramebufferConfig
    {
        public int RedBits;
        public int GreenBits;
        public int BlueBits;
        public int AlphaBits;
        public int DepthBits;
        public int StencilBits;
        public int AccumRedBits;
        public int AccumGreenBits;
        public int AccumBlueBits;
        public int AccumAlphaBits;
        public int AuxBuffers;
        public bool Stereo;
        public int Samples;
        public bool SRgb;
        public bool Doublebuffer;
        public bool Transparent;
        public int Handle;
    }
}