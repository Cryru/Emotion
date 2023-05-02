namespace Tests.StbTrueType
{
#if !STBSHARP_INTERNAL
    public
#else
	internal
# endif
        struct GlyphInfo
    {
        public int X, Y, Width, Height;
        public int XOffset, YOffset;
        public int XAdvance;
    }
}