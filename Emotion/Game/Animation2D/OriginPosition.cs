#region Using

using System.Runtime.CompilerServices;

#endregion

namespace Emotion.Game.Animation2D
{
	public enum OriginPosition
	{
		TopLeft,
		TopCenter,
		TopRight,

		CenterLeft,
		CenterCenter,
		CenterRight,

		BottomLeft,
		BottomCenter,
		BottomRight,
	}

	public static class OriginPositionExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 MoveOriginOfRectangle(this OriginPosition origin, float x, float y, float w, float h)
		{
			Vector2 renderPos = Vector2.Zero;
			switch (origin)
			{
				case OriginPosition.TopLeft:
					break;
				case OriginPosition.TopCenter:
					renderPos.X -= w / 2;
					break;
				case OriginPosition.TopRight:
					renderPos.X -= w;
					break;
				case OriginPosition.CenterLeft:
					renderPos.Y -= w / 2;
					break;
				case OriginPosition.CenterCenter:
					renderPos.X -= w / 2;
					renderPos.Y -= h / 2;
					break;
				case OriginPosition.CenterRight:
					renderPos.X -= w;
					renderPos.Y -= h / 2;
					break;
				case OriginPosition.BottomLeft:
					renderPos.Y -= h;
					break;
				case OriginPosition.BottomCenter:
					renderPos.X -= w / 2;
					renderPos.Y -= h;
					break;
				case OriginPosition.BottomRight:
					renderPos.X -= x;
					renderPos.Y -= h;
					break;
			}

			return renderPos;
		}
	}
}