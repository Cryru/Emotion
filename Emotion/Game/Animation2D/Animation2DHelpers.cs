namespace Emotion.Game.Animation2D
{
	public static class Animation2DHelpers
	{
		/// <summary>
		/// Returns the bounds of a frame within a spritesheet texture.
		/// </summary>
		/// <param name="textureSize">The size of the spritesheet texture.</param>
		/// <param name="frameSize">The size of individual frames.</param>
		/// <param name="spacing">The spacing between frames.</param>
		/// <param name="frameId">The index of the frame we are looking for. 0 based.</param>
		/// <returns>The bounds of a frame within a spritesheet texture.</returns>
		public static Rectangle GetGridFrameBounds(Vector2 textureSize, Vector2 frameSize, Vector2 spacing, int frameId)
		{
			// Get the total number of columns.
			var columns = (int) (textureSize.X / frameSize.X);

			// If invalid number of columns this means the texture size is larger than the frame size.
			if (columns == 0)
			{
				Engine.Log.Trace($"Invalid frame size of [{frameSize}] for image of size [{textureSize}].", MessageSource.Anim);
				return new Rectangle(Vector2.Zero, textureSize);
			}

			// Get the current row and column.
			var row = (int) (frameId / (float) columns);
			int column = frameId % columns;

			// Find the frame we are looking for.
			return new Rectangle((int) (frameSize.X * column + spacing.X * (column + 1)),
				(int) (frameSize.Y * row + spacing.Y * (row + 1)), (int) frameSize.X, (int) frameSize.Y);
		}

		/// <inheritdoc cref="GetGridFrameBounds(Vector2, Vector2, Vector2, int)" />
		public static Rectangle GetGridFrameBounds(Vector2 textureSize, Vector2 frameSize, Vector2 spacing, int row, int column)
		{
			// Get the total number of columns.
			var columns = (int) (textureSize.X / frameSize.X);
			var rows = (int) (textureSize.Y / frameSize.Y);

			Assert(columns >= column);
			Assert(rows >= row);

			// Find the frame we are looking for.
			return new Rectangle((int) (frameSize.X * column + spacing.X * (column + 1)),
				(int) (frameSize.Y * row + spacing.Y * (row + 1)), (int) frameSize.X, (int) frameSize.Y);
		}
	}
}