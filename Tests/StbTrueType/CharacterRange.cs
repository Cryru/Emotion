namespace Tests.StbTrueType
{
#if !STBSHARP_INTERNAL
    public
#else
	internal
#endif
        struct CharacterRange
    {
        public static readonly CharacterRange BasicLatin = new CharacterRange(0x0020, 0x007F);
        public static readonly CharacterRange Latin1Supplement = new CharacterRange(0x00A0, 0x00FF);
        public static readonly CharacterRange LatinExtendedA = new CharacterRange(0x0100, 0x017F);
        public static readonly CharacterRange LatinExtendedB = new CharacterRange(0x0180, 0x024F);
        public static readonly CharacterRange Cyrillic = new CharacterRange(0x0400, 0x04FF);
        public static readonly CharacterRange CyrillicSupplement = new CharacterRange(0x0500, 0x052F);
        public static readonly CharacterRange Hiragana = new CharacterRange(0x3040, 0x309F);
        public static readonly CharacterRange Katakana = new CharacterRange(0x30A0, 0x30FF);
        public static readonly CharacterRange Greek = new CharacterRange(0x0370, 0x03FF);
        public static readonly CharacterRange CjkSymbolsAndPunctuation = new CharacterRange(0x3000, 0x303F);
        public static readonly CharacterRange CjkUnifiedIdeographs = new CharacterRange(0x4e00, 0x9fff);
        public static readonly CharacterRange HangulCompatibilityJamo = new CharacterRange(0x3130, 0x318f);
        public static readonly CharacterRange HangulSyllables = new CharacterRange(0xac00, 0xd7af);

        public int Start { get; }

        public int End { get; }

        public int Size
        {
            get => End - Start + 1;
        }

        public CharacterRange(int start, int end)
        {
            Start = start;
            End = end;
        }

        public CharacterRange(int single) : this(single, single)
        {
        }
    }
}