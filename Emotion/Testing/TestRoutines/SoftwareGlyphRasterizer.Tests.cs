#region Using

using System.Collections;
using System.Threading.Tasks;
using Emotion.Graphics;
using Emotion.Standard.OpenType;

#endregion

namespace Emotion.Testing.TestRoutines;

public static class SoftwareGlyphRasterizer
{
	public class SoftwareGlyphRasterizerTestScene : TestingScene
	{
		public override Task LoadAsync()
		{
			return Task.CompletedTask;
		}

		protected override void TestUpdate()
		{
		}

		protected override void TestDraw(RenderComposer c)
		{
		}

		public override Func<IEnumerator>[] GetTestCoroutines()
		{
			return null;
		}
	}

	public static class SoftwareGlyphRasterizerTests
	{
		private static unsafe void VerifyGlyphVertices(stbtt_fontinfo info, int glyph, float scale_x, float scale_y)
		{
			float scale = scale_x > scale_y ? scale_y : scale_x;

			// Get Emotion metrics
			float flatness = FLATNESS_IN_PIXELS / scale;
			GlyphVertex[] glyphVertices = GetGlyphVertices(info, glyph);

			// Get STB metrics
			stbtt_vertex* vertices;
			int numVerts = stbtt_GetGlyphShape(info, glyph, &vertices);

			// Compare
			TestLib.Assert(numVerts, glyphVertices.Length);

			for (var i = 0; i < numVerts; i++)
			{
				GlyphVertex vertexEmotion = glyphVertices[i];
				stbtt_vertex vertexStb = vertices[i];

				TestLib.Assert(vertexStb.x, vertexEmotion.X);
				TestLib.Assert(vertexStb.y, vertexEmotion.Y);
				TestLib.Assert(vertexStb.cx, vertexEmotion.Cx);
				TestLib.Assert(vertexStb.cx, vertexEmotion.Cx);
				TestLib.Assert(vertexStb.cx1, vertexEmotion.Cx1);
				TestLib.Assert(vertexStb.cy1, vertexEmotion.Cy1);
				TestLib.Assert(vertexStb.type, vertexEmotion.Flags);
			}
		}

		private static unsafe void VerifyCurves(stbtt_fontinfo info, int glyph, float scale_x, float scale_y)
		{
			float scale = scale_x > scale_y ? scale_y : scale_x;

			// Get Emotion metrics
			float flatness = FLATNESS_IN_PIXELS / scale;
			GlyphVertex[] glyphVertices = GetGlyphVertices(info, glyph);
			VerticesToContours(glyphVertices, flatness, out Vector2[] points, out int[] contourLengths);

			// Get STB metrics
			stbtt_vertex* vertices;
			int numVerts = stbtt_GetGlyphShape(info, glyph, &vertices);

			var windingCount = 0;
			int* windingLengths = null;
			stbtt__point* windings = stbtt_FlattenCurves(vertices, numVerts, FLATNESS_IN_PIXELS / scale, &windingLengths, &windingCount, (void*) 0);

			// Compare
			var totalWindings = 0;
			for (var i = 0; i < windingCount; i++)
			{
				totalWindings += windingLengths[i];
			}

			TestLib.Assert(totalWindings, points.Length);
			TestLib.Assert(windingCount, contourLengths.Length);

			for (var i = 0; i < points.Length; i++)
			{
				Vector2 point = points[i];
				stbtt__point winding = windings[i];

				TestLib.Assert(winding.x, point.X);
				TestLib.Assert(winding.y, point.Y);
			}
		}
	}
}