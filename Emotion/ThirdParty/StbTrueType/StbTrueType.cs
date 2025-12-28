namespace StbTrueTypeSharp
{
	internal static unsafe partial class StbTrueType
	{
		public class stbtt_fontinfo : IDisposable
		{
			public stbtt__buf cff;
			public stbtt__buf charstrings;
			public byte* data = null;
			public stbtt__buf fdselect;
			public stbtt__buf fontdicts;
			public int fontstart;
			public int glyf;
			public int gpos;
			public stbtt__buf gsubrs;
			public int head;
			public int hhea;
			public int hmtx;
			public int index_map;
			public int indexToLocFormat;
			public bool isDataCopy;
			public int kern;
			public int loca;
			public int numGlyphs;
			public stbtt__buf subrs;
			public int svg;
			public void* userdata;

			public void Dispose()
			{
				Dispose(true);
			}

			~stbtt_fontinfo()
			{
				Dispose(false);
			}

			protected virtual void Dispose(bool disposing)
			{
				if (isDataCopy && data != null)
				{
					StbRuntime.free(data);
					data = null;
				}
			}
		}

		public static uint stbtt__find_table(byte* data, uint fontstart, string tag)
		{
			int num_tables = ttUSHORT(data + fontstart + 4);
			var tabledir = fontstart + 12;
			int i;
			for (i = 0; i < num_tables; ++i)
			{
				var loc = (uint)(tabledir + 16 * i);
				if ((data + loc + 0)[0] == tag[0] && (data + loc + 0)[1] == tag[1] &&
					(data + loc + 0)[2] == tag[2] && (data + loc + 0)[3] == tag[3])
					return ttULONG(data + loc + 8);
			}

			return 0;
		}

		/// <summary>
		///     Creates and initializes a font from ttf/otf/ttc data
		/// </summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <returns>null if the data was invalid</returns>
		public static stbtt_fontinfo CreateFont(ReadOnlyMemory<byte> data, int offset)
		{
			var dataCopy = (byte*)StbRuntime.malloc(data.Length);
            data.Span.CopyTo(new Span<byte>(dataCopy, data.Length));

			var info = new stbtt_fontinfo
			{
				isDataCopy = true
			};

			if (stbtt_InitFont_internal(info, dataCopy, offset) == 0)
			{
				info.Dispose();
				return null;
			}

			return info;
		}
	}
}