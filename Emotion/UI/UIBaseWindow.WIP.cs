#region Using

using System.Diagnostics;
using Emotion.Common.Serialization;

#endregion

#nullable enable

namespace Emotion.UI;

#if NEW_UI
public partial class UIBaseWindow : Transform, IRenderable, IComparable<UIBaseWindow>, IEnumerable<UIBaseWindow>
{
	/// <summary>
	/// The size returned by the window measure.
	/// This is the defacto minimum size the child would occupy.
	/// </summary>
	[DontSerialize] public Vector2 _measuredSize;

	public bool ChildrenAllSameWidth; // todo: delete

	protected virtual Vector2 InternalMeasure(Vector2 space)
	{
		return Vector2.Zero;
	}

	// On Rounding In UI Layout:
	// Sizes should always be rounded up.
	// Positions should always be rounded down.
	// Offsets (spacings) should always be rounded to the closest.

	/// <summary>
	/// Given the max space by the parent, return the minimum size this window needs.
	/// </summary>
	protected virtual Vector2 Measure(Vector2 space)
	{
		float scale = GetScale();
		bool amInsideParent = AnchorsInsideParent(ParentAnchor, Anchor);
		Vector2 usedSpace = Vector2.Zero;

		Rectangle scaledMargins = Margins * scale;

		// If inside the parent apply margins, otherwise we are are measuring against the whole controller.
		if (amInsideParent)
		{
			float marginsX = scaledMargins.X + scaledMargins.Width;
			float marginsY = scaledMargins.Y + scaledMargins.Height;

			space.X -= marginsX;
			space.Y -= marginsY;
			usedSpace.X += marginsX;
			usedSpace.Y += marginsY;
		}
		else
		{
			space = Controller!.Size;
		}

		// Reduce available space due to paddings.
		Rectangle scaledPadding = Paddings * scale;
		float paddingX = scaledPadding.X + scaledPadding.Width;
		float paddingY = scaledPadding.Y + scaledPadding.Height;
		space.X -= paddingX;
		space.Y -= paddingY;
		usedSpace.X += paddingX;
		usedSpace.Y += paddingY;

		Vector2 spaceClampedToConstraints = Vector2.Clamp(space, MinSize * scale, MaxSize * scale).Ceiling();

		// First calculate the size of children.
		// Windows of children need to be able to accomodate them.
		if (Children != null) usedSpace += MeasureChildrenLayoutWise(spaceClampedToConstraints);

		Vector2 minWindowSize = InternalMeasure(spaceClampedToConstraints);
		//Vector2 contentSize = usedSpace;
		//contentSize = Vector2.Clamp(contentSize, MinSize * scale, MaxSize * scale).Ceiling();

		Vector2 size = usedSpace; // + minWindowSize;
		size = Vector2.Clamp(size, MinSize * scale, MaxSize * scale).Ceiling();
		size = size.Ceiling();

		if (size.X < 0 || size.Y < 0)
		{
			Engine.Log.Warning($"UIWindow of id {Id} measured with a size smaller than 0.", MessageSource.UI, true);
			size.X = MathF.Max(size.X, 0);
			size.Y = MathF.Max(size.Y, 0);
		}

		_measuredSize = size;

		return size;
	}

	private Vector2 MeasureChildrenLayoutWise(Vector2 spaceForChildren)
	{
		Debug.Assert(Children != null);

		float scale = GetScale();
		bool wrap = LayoutMode is LayoutMode.HorizontalListWrap or LayoutMode.VerticalListWrap;
		Vector2 scaledSpacing = (ListSpacing * scale).RoundClosest();

		Vector2 pen = Vector2.Zero;

		float highestOnRow = 0;
		float widestInColumn = 0;

		Vector2 usedSpace = Vector2.Zero;

		switch (LayoutMode)
		{
			case LayoutMode.HorizontalList:
			case LayoutMode.HorizontalListWrap:
				for (var i = 0; i < Children.Count; i++)
				{
					UIBaseWindow child = Children[i];
					if (child.RelativeTo != null) continue;

					bool insideParent = AnchorsInsideParent(child.ParentAnchor, child.Anchor);
					bool addSpacing = insideParent && pen.X != 0; // Skip spacing at start of row.

					// Give full space as available space if wrapping.
					// If we run out of space in the non-wrapping direction we're screwed anyway.
					Vector2 childSpace = wrap ? spaceForChildren : spaceForChildren - pen;
					if (addSpacing)
						childSpace.X -= scaledSpacing.X;

					Vector2 childSize = child.Measure(childSpace);
					if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;
					if (!insideParent) continue;

					highestOnRow = MathF.Max(highestOnRow, childSize.Y);

					if (wrap && pen.X + childSize.X > spaceForChildren.X)
					{
						pen.X = 0;
						pen.Y += highestOnRow + scaledSpacing.Y;
						highestOnRow = 0;
						addSpacing = false;
					}

					if (addSpacing) pen.X += scaledSpacing.X;
					pen.X += childSize.X;

					usedSpace.X = MathF.Max(usedSpace.X, pen.X);
					usedSpace.Y = pen.Y + highestOnRow;
				}

				break;
			case LayoutMode.VerticalList:
			case LayoutMode.VerticalListWrap:
				for (var i = 0; i < Children.Count; i++)
				{
					UIBaseWindow child = Children[i];
					if (child.RelativeTo != null) continue;

					bool insideParent = AnchorsInsideParent(child.ParentAnchor, child.Anchor);
					bool addSpacing = insideParent && pen.Y != 0; // Skip spacing at start of row.

					// Give full space as available space if wrapping.
					// If we run out of space in the non-wrapping direction we're screwed anyway.
					Vector2 childSpace = wrap ? spaceForChildren : spaceForChildren - pen;
					if (addSpacing)
						childSpace.Y -= scaledSpacing.Y;

					Vector2 childSize = child.Measure(childSpace);
					if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;
					if (!insideParent) continue;

					if (wrap && pen.Y + childSize.Y > spaceForChildren.Y)
					{
						pen.Y = 0;
						pen.X += widestInColumn + scaledSpacing.X;
						widestInColumn = 0;
						addSpacing = false;
					}

					if (addSpacing) pen.Y += scaledSpacing.Y;
					pen.Y += childSize.Y;
					widestInColumn = MathF.Max(widestInColumn, childSize.X);

					usedSpace.X = pen.X + widestInColumn;
					usedSpace.Y = MathF.Max(usedSpace.Y, pen.Y);
				}

				break;
			case LayoutMode.Free:
				for (var i = 0; i < Children.Count; i++)
				{
					UIBaseWindow child = Children[i];
					if (child.RelativeTo != null) continue;

					Vector2 childSize = child.Measure(spaceForChildren);
					if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;
					if (!AnchorsInsideParent(child.ParentAnchor, child.Anchor)) continue;

					usedSpace = Vector2.Max(usedSpace, childSize);
				}

				break;
		}

		// Layout children relative to other windows. Might be a better idea to do this in a separate pass to
		// ensure that everything else layouted, but for now we'll do it here.
		for (int i = 0; i < Children.Count; i++)
		{
			UIBaseWindow child = Children[i];
			if (child.RelativeTo == null) continue;
		}

		return usedSpace;
	}

	public Rectangle GetSpaceForChild(UIBaseWindow child)
	{
		float scale = GetScale();
		float childScale = child.GetScale();
		var parentSpaceForChild = new Rectangle(0, 0, Size);
		Rectangle childScaledMargins = child.Margins * childScale;

		parentSpaceForChild.X += childScaledMargins.X;
		parentSpaceForChild.Y += childScaledMargins.Y;
		parentSpaceForChild.Width -= childScaledMargins.Width;
		parentSpaceForChild.Height -= childScaledMargins.Height;

		Rectangle parentScaledPaddings = Paddings * scale;
		parentSpaceForChild.X += parentScaledPaddings.X;
		parentSpaceForChild.Y += parentScaledPaddings.Y;
		parentSpaceForChild.Width -= parentScaledPaddings.Width;
		parentSpaceForChild.Height -= parentScaledPaddings.Height;

		return parentSpaceForChild;
	}

	protected virtual void Layout(Vector2 pos, Vector2 size)
	{
		float scale = GetScale();
		Size = size;

		pos = pos + Offset * scale;
		Position = pos.RoundClosest().ToVec3(Z);

		// Invalidate transformations.
		if (_transformationStackBacking != null) _transformationStackBacking.MatrixDirty = true;

		if (Children != null)
		{
			bool wrap = LayoutMode is LayoutMode.HorizontalListWrap or LayoutMode.VerticalListWrap;
			Vector2 scaledSpacing = (ListSpacing * scale).RoundClosest();

			Vector2 pen = Vector2.Zero;
			Vector2 freeSpace = size;

			float highestOnRow = 0;
			float widestInColumn = 0;

			switch (LayoutMode)
			{
				case LayoutMode.HorizontalList:
				case LayoutMode.HorizontalListWrap:
					for (var i = 0; i < Children.Count; i++)
					{
						UIBaseWindow child = Children[i];
						if (child.RelativeTo != null) continue;
						if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;

						bool insideParent = AnchorsInsideParent(child.ParentAnchor, child.Anchor);
						bool addSpacing = insideParent && pen.X != 0; // Skip spacing at start of row.

						Vector2 childSize = child._measuredSize;
						if (child.StretchY)
							childSize.Y = Height;

						if (!insideParent)
							if (child.StretchX)
								childSize.X = Width;

						child.Layout(pen, childSize);

						// Dont count space taken by windows outside parent.
						bool windowTakesSpace = insideParent && (child.Visible || !child.DontTakeSpaceWhenHidden);
						if (windowTakesSpace) widestInColumn = MathF.Max(widestInColumn, childSize.X);

						if (wrap && pen.X + childSize.X > freeSpace.X)
						{
							pen.X = 0;
							pen.Y += highestOnRow + scaledSpacing.Y;
							highestOnRow = 0;
							addSpacing = false;
						}

						if (addSpacing)
							pen.X += scaledSpacing.X;
					}

					break;
				case LayoutMode.VerticalList:
				case LayoutMode.VerticalListWrap:
					//for (var i = 0; i < Children.Count; i++)
					//{
					//	UIBaseWindow child = Children[i];
					//	if (child.RelativeTo != null) continue;

					//	bool insideParent = AnchorsInsideParent(child.ParentAnchor, child.Anchor);
					//	bool addSpacing = insideParent && pen.Y != 0; // Skip spacing at start of row.

					//	// Give full space as available space if wrapping.
					//	// If we run out of space in the non-wrapping direction we're screwed anyway.
					//	Vector2 childSpace = wrap ? spaceForChildren : spaceForChildren - pen; 
					//	if (addSpacing)
					//		childSpace.Y -= scaledSpacing.Y;

					//	Vector2 childSize = child.Measure(childSpace);
					//	if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;
					//	if (!insideParent) continue;

					//	float spaceTaken = childSize.Y;
					//	if (wrap && pen.Y + spaceTaken > spaceForChildren.Y)
					//	{
					//		pen.Y = 0;
					//		pen.X += widestInColumn + scaledSpacing.X;
					//		widestInColumn = 0;
					//		addSpacing = false;
					//	}

					//	if (addSpacing) pen.Y += scaledSpacing.Y;
					//	pen.Y += spaceTaken;
					//	widestInColumn = MathF.Max(widestInColumn, childSize.X);

					//	usedSpace.X = pen.X + widestInColumn;
					//	usedSpace.Y = MathF.Max(usedSpace.Y, pen.Y);
					//}
					break;
				case LayoutMode.Free:
					for (var i = 0; i < Children.Count; i++)
					{
						UIBaseWindow child = Children[i];
						if (child.RelativeTo != null) continue;
						if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;

						Vector2 childPos;
						Vector2 childSize;
						bool childIsInsideMe = AnchorsInsideParent(child.ParentAnchor, child.Anchor);
						if (childIsInsideMe)
						{
							Rectangle spaceForChild = GetSpaceForChild(child);

							childSize = child._measuredSize;
							childPos = GetUIAnchorPosition(child.ParentAnchor, Size, spaceForChild, child.Anchor, childSize);
							if (child.StretchX)
								childSize.X = spaceForChild.Width - childPos.X;
							if (child.StretchY)
								childSize.Y = spaceForChild.Height - childPos.Y;

							childPos += Position2;
						}
						else
						{
							childSize = child._measuredSize;
							if (child.StretchX)
								childSize.X = Width;
							if (child.StretchY)
								childSize.Y = Height;
							child._measuredSize = childSize;
							childPos = child.CalculateContentPos(Position2, Size, Paddings * GetScale());
						}

						child.Layout(childPos, childSize);
					}


					//for (var i = 0; i < Children.Count; i++)
					//{
					//	UIBaseWindow child = Children[i];
					//	if (child.RelativeTo != null) continue; 

					//	Vector2 childSize = child.Measure(spaceForChildren);
					//	if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;
					//	if (!AnchorsInsideParent(child.ParentAnchor, child.Anchor)) continue;

					//	usedSpace = Vector2.Max(usedSpace, childSize);
					//}
					break;
			}

			// Layout children relative to other windows. Might be a better idea to do this in a separate pass to
			// ensure that everything else layouted, but for now we'll do it here.
			for (int i = 0; i < Children.Count; i++)
			{
				UIBaseWindow child = Children[i];
				if (child.RelativeTo == null) continue;
			}
		}

		// Construct input detecting boundary.
		_inputBoundsWithChildren = Bounds;
		if (Children != null)
			for (var i = 0; i < Children.Count; i++)
			{
				UIBaseWindow child = Children[i];
				_inputBoundsWithChildren = Rectangle.Union(child._inputBoundsWithChildren, _inputBoundsWithChildren);
			}
	}
}

#endif