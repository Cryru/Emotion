#region Using

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
	[DontSerialize] protected Vector2 _measuredSize;

	/// <summary>
	/// The amount of space used by the children of this window during measurement.
	/// </summary>
	[DontSerialize] protected Vector2 _measureChildrenUsedSpace;

	public bool ChildrenAllSameWidth; // todo: delete

	/// <summary>
	/// Whether the window should fill the available space of its parent.
	/// On by default. Doesn't apply when the parent is of a list layout.
	/// </summary>
	public bool FillX
	{
		get => _fillX;
		set
		{
			if (_fillX == value) return;
			_fillX = value;
			InvalidateLayout();
		}
	}

	private bool _fillX = true;

	/// <inheritdoc cref="FillX" />
	public bool FillY
	{
		get => _fillY; // && Anchor is not UIAnchor.CenterLeft and not UIAnchor.CenterCenter and not UIAnchor.CenterRight;
		set
		{
			if (_fillY == value) return;
			_fillY = value;
			InvalidateLayout();
		}
	}

	private bool _fillY = true;

	/// <summary>
	/// Whether the window should fill the available space of its parent, when
	/// its parent is using a list layout. Off by default.
	/// </summary>
	public bool FillXInList
	{
		get => _fillXInList;
		set
		{
			if (_fillXInList == value) return;
			_fillXInList = value;
			InvalidateLayout();
		}
	}

	private bool _fillXInList;

	/// <inheritdoc cref="FillXInList" />
	public bool FillYInList
	{
		get => _fillYInList;
		set
		{
			if (_fillYInList == value) return;
			_fillYInList = value;
			InvalidateLayout();
		}
	}

	private bool _fillYInList;

	[DontSerialize]
	public UIAnchor AlignAnchor
	{
		get => ParentAnchor == Anchor ? Anchor : UIAnchor.TopLeft;
		set
		{
			ParentAnchor = value;
			Anchor = value;
		}
	}

	// ReSharper disable once InconsistentNaming
	protected static List<UIBaseWindow> EMPTY_CHILDREN_LIST = new(0);

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

		Rectangle scaledMargins = Margins * scale;
		float marginsX = scaledMargins.X + scaledMargins.Width;
		float marginsY = scaledMargins.Y + scaledMargins.Height;

		Rectangle scaledPadding = Paddings * scale;
		float paddingX = scaledPadding.X + scaledPadding.Width;
		float paddingY = scaledPadding.Y + scaledPadding.Height;

		// If inside the parent subtract margins from the total space.
		if (amInsideParent)
		{
			space.X -= marginsX;
			space.Y -= marginsY;
		}
		else
		{
			space = Controller!.Size;
		}

		// Measure myself so we know how much space we have for children.
		Vector2 minWindowSize = InternalMeasure(space);
		minWindowSize = Vector2.Clamp(minWindowSize, MinSize * scale, MaxSize * scale).Ceiling();
		minWindowSize = minWindowSize.Ceiling();

		List<UIBaseWindow> children = Children ?? EMPTY_CHILDREN_LIST;
		Vector2 childrenUsed = Vector2.Zero;
		if (children.Count > 0)
		{
			// Then calculate how much the children will use.
			// If we're going to fill we need to give children that extra space.
			// todo: we probably shouldnt if not expanded by children and assert on usedspace higher?
			Vector2 spaceForChildren = space;
			//if (FillX) spaceForChildren.X = space.X;
			//if (FillY) spaceForChildren.Y = space.Y;
			spaceForChildren = Vector2.Clamp(spaceForChildren, MinSize * scale, MaxSize * scale).Ceiling();

			// Reduce their space by paddings.
			// This does mean that paddings will affect children outside the parent.
			spaceForChildren.X -= paddingX;
			spaceForChildren.Y -= paddingY;

			switch (LayoutMode)
			{
				case LayoutMode.HorizontalList:
				case LayoutMode.HorizontalListWrap:
				case LayoutMode.VerticalList:
				case LayoutMode.VerticalListWrap:
					bool wrap = LayoutMode is LayoutMode.HorizontalListWrap or LayoutMode.VerticalListWrap;
					int mask = LayoutMode switch
					{
						LayoutMode.HorizontalList or LayoutMode.HorizontalListWrap => 0,
						LayoutMode.VerticalList or LayoutMode.VerticalListWrap => 1,

						_ => 0 // ???
					};

					childrenUsed = LayoutMode_ListMeasure(children, spaceForChildren, wrap, mask);
					break;
				case LayoutMode.Free:
					childrenUsed = LayoutMode_FreeMeasure(children, spaceForChildren);
					break;
			}
		}

		// Even if we have no children our paddings should be part of the used space.
		childrenUsed.X += paddingX;
		childrenUsed.Y += paddingY;

		Vector2 size = minWindowSize;

		// Windows with children will accomodate their children.
		// todo: ChildrenCanExpandX, ChildrenCanExpandY
		if (true)
		{
			childrenUsed.X = MathF.Min(childrenUsed.X, space.X); // Dont allow expansion higher than space.
			size.X = MathF.Max(childrenUsed.X, size.X);
		}

		if (true)
		{
			childrenUsed.Y = MathF.Min(childrenUsed.Y, space.Y);
			size.Y = MathF.Max(childrenUsed.Y, size.Y);
		}

		size.X += marginsX;
		size.Y += marginsY;

		if (size.X < 0 || size.Y < 0)
		{
			Assert(false, $"UIWindow of id {Id} measured with a size smaller than 0.");
			size.X = MathF.Max(size.X, 0);
			size.Y = MathF.Max(size.Y, 0);
		}

		_measuredSize = size;
		_measureChildrenUsedSpace = childrenUsed;
		Size = size;

		return size;
	}

	private Vector2 LayoutMode_FreeMeasure(List<UIBaseWindow> children, Vector2 spaceForChildren)
	{
		Vector2 usedSpace = Vector2.Zero;
		for (var i = 0; i < children.Count; i++)
		{
			UIBaseWindow child = children[i];
			if (child.RelativeTo != null) continue;

			bool insideParent = AnchorsInsideParent(child.ParentAnchor, child.Anchor);

			Vector2 spaceForChild = spaceForChildren;
			//if (insideParent)
			//{
			//	float childScale = child.GetScale();
			//	Rectangle childScaledMargins = child.Margins * childScale;
			//	var marginSize = new Vector2(
			//		childScaledMargins.X + childScaledMargins.Width,
			//		childScaledMargins.Y + childScaledMargins.Height
			//	);
			//	spaceForChild -= marginSize;
			//}

			Vector2 childSize = child.Measure(spaceForChildren);
			if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;
			if (!insideParent) continue;

			usedSpace = Vector2.Max(usedSpace, childSize);
		}

		return usedSpace;
	}

	private void LayoutMode_FreeLayout(List<UIBaseWindow> children, Rectangle childSpaceRect)
	{
		for (var i = 0; i < children.Count; i++)
		{
			UIBaseWindow child = children[i];
			if (child.RelativeTo != null) continue;
			if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;

			bool insideParent = AnchorsInsideParent(child.ParentAnchor, child.Anchor);

			Vector2 childPos = Vector2.Zero;
			Vector2 childSize = child._measuredSize;

			if (insideParent)
			{
				Rectangle thisChildSpaceRect = childSpaceRect;

				float childScale = child.GetScale();
				Rectangle childScaledMargins = child.Margins * childScale;
				thisChildSpaceRect.X += childScaledMargins.X;
				thisChildSpaceRect.Y += childScaledMargins.Y;
				thisChildSpaceRect.Width -= childScaledMargins.Width + childScaledMargins.X;
				thisChildSpaceRect.Height -= childScaledMargins.Height + childScaledMargins.Y;

				bool childFillX = child.FillX && child.Anchor is not UIAnchor.TopCenter and not UIAnchor.CenterCenter and not UIAnchor.BottomCenter;
				bool childFillY = child.FillY && child.Anchor is not UIAnchor.CenterLeft and not UIAnchor.CenterCenter and not UIAnchor.CenterRight;

				Vector2 childSizeFilled = childSize;
				if (childFillX) childSizeFilled.X = thisChildSpaceRect.Width;
				if (childFillY) childSizeFilled.Y = thisChildSpaceRect.Height;
				childSizeFilled = Vector2.Clamp(childSizeFilled, child.MinSize * childScale, child.MaxSize * childScale).Ceiling();

				childPos = GetChildPositionAnchorWise(child.ParentAnchor, child.Anchor, thisChildSpaceRect, childSizeFilled);

				// Subtract only right and bottom margins as the childPos is the top left.
				if (childFillX) childSize.X = childSpaceRect.Width - childScaledMargins.Width - (childPos.X - childSpaceRect.X);
				if (childFillY) childSize.Y = childSpaceRect.Height - childScaledMargins.Height - (childPos.Y - childSpaceRect.Y);
				childSize = Vector2.Clamp(childSize, child.MinSize * childScale, child.MaxSize * childScale).Ceiling();
			}
			//if (childIsInsideMe)
			//{
			//	Rectangle spaceForChild = GetSpaceForChild(child);

			//	childSize = child._measuredSize;
			//	childPos = GetUIAnchorPosition(child.ParentAnchor, Size, spaceForChild, child.Anchor, childSize);
			//	if (child.StretchX)
			//		childSize.X = spaceForChild.Width - childPos.X;
			//	if (child.StretchY)
			//		childSize.Y = spaceForChild.Height - childPos.Y;

			//	childPos += Position2;
			//}
			//else
			//{
			//	childSize = child._measuredSize;
			//	if (child.StretchX)
			//		childSize.X = Width;
			//	if (child.StretchY)
			//		childSize.Y = Height;
			//	child._measuredSize = childSize;
			//	childPos = child.CalculateContentPos(Position2, Size, Paddings * GetScale());
			//}

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
	}

	private Vector2 LayoutMode_ListMeasure(List<UIBaseWindow> children, Vector2 freeSpace, bool wrap, int axisMask)
	{
		Vector2 pen = Vector2.Zero;

		Vector2 usedSpace = Vector2.Zero;
		float highestOtherAxis = 0;

		int invertedMask = 1 - axisMask;

		float scale = GetScale();
		Vector2 spacing = (ListSpacing * scale).RoundClosest();

		for (var i = 0; i < children.Count; i++)
		{
			UIBaseWindow child = children[i];
			if (child.RelativeTo != null) continue;
			if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;

			Vector2 childSize = child.Measure(freeSpace - pen);

			// Dont count space taken by windows outside parent.
			bool insideParent = AnchorsInsideParent(child.ParentAnchor, child.Anchor);
			if (insideParent)
			{
				pen[axisMask] += childSize[axisMask];
				highestOtherAxis = MathF.Max(highestOtherAxis, childSize[invertedMask]);
				usedSpace[axisMask] = MathF.Max(usedSpace[axisMask], pen[axisMask]);
			}

			bool addSpacing = insideParent && pen[axisMask] != 0; // Skip spacing at start.
			if (addSpacing) pen[axisMask] += spacing[axisMask];
		}

		usedSpace[invertedMask] = pen[invertedMask] + highestOtherAxis;
		return usedSpace;
	}

	private Vector2 LayoutMode_ListLayout(List<UIBaseWindow> children, Rectangle childSpaceRect, bool wrap, int axisMask)
	{
		Vector2 pen = Vector2.Zero;

		Vector2 usedSpace = Vector2.Zero;
		float highestOtherAxis = 0;

		int invertedMask = 1 - axisMask;

		float scale = GetScale();
		Vector2 spacing = (ListSpacing * scale).RoundClosest();

		// Calculate the size for filling windows.
		// All leftover size will be equally distributed.
		// For this we basically need to precalculate the layout.
		Vector2 fillingPen = Vector2.Zero;
		Vector2 fillingWindowPerAxis = Vector2.Zero;
		for (var i = 0; i < children.Count; i++)
		{
			UIBaseWindow child = children[i];
			if (child.RelativeTo != null) continue;
			if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;

			Vector2 childSize = child._measuredSize;

			bool insideParent = AnchorsInsideParent(child.ParentAnchor, child.Anchor);

			// Don't add spacing before 0 size windows.
			bool addSpacing = insideParent && childSize[axisMask] != 0;
			if (addSpacing) fillingPen[axisMask] += spacing[axisMask];

			// Dont count space taken by windows outside parent.
			if (insideParent)
			{
				fillingPen[axisMask] += childSize[axisMask];

				// Count up filling windows per axis.
				if (child.FillXInList && axisMask == 0)
					fillingWindowPerAxis.X++;
				if (child.FillYInList && axisMask == 1)
					fillingWindowPerAxis.Y++;
			}
		}

		fillingWindowPerAxis = Vector2.Max(fillingWindowPerAxis, Vector2.One);
		Vector2 fillWindowsSize = (childSpaceRect.Size - fillingPen) / fillingWindowPerAxis;

		// Layout the children.
		Vector2 prevChildSize = Vector2.Zero;
		for (var i = 0; i < children.Count; i++)
		{
			UIBaseWindow child = children[i];
			if (child.RelativeTo != null) continue;
			if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;

			Vector2 childSize = child._measuredSize;
			if (child.FillXInList || child.FillYInList)
			{
				if (child.FillXInList) childSize.X = fillWindowsSize.X;
				if (child.FillYInList) childSize.Y = fillWindowsSize.Y;

				// todo: different max/min sizes in a list would mean that not all filling windows would
				// be the same size. This is currently unhandled.
				float childScale = child.GetScale();
				childSize = Vector2.Clamp(childSize, child.MinSize * childScale, child.MaxSize * childScale).Ceiling();
			}

			bool insideParent = AnchorsInsideParent(child.ParentAnchor, child.Anchor);

			// Don't add spacing before 0 size windows.
			bool addSpacing = insideParent && childSize[axisMask] != 0 && prevChildSize[axisMask] != 0;
			if (addSpacing) pen[axisMask] += spacing[axisMask];

			child.Layout(pen + childSpaceRect.Position, childSize);

			// Dont count space taken by windows outside parent.
			if (insideParent)
			{
				pen[axisMask] += childSize[axisMask];
				prevChildSize = childSize;

				highestOtherAxis = MathF.Max(highestOtherAxis, childSize[invertedMask]);
				usedSpace[axisMask] = MathF.Max(usedSpace[axisMask], pen[axisMask]);
			}
		}

		usedSpace[invertedMask] = pen[invertedMask] + highestOtherAxis;
		return usedSpace;
	}

	protected virtual void Layout(Vector2 pos, Vector2 size)
	{
		if (size.X < 0 || size.Y < 0)
		{
			Assert(false, $"UIWindow of id {Id} layouted with a size smaller than 0.");
			size.X = MathF.Max(size.X, 0);
			size.Y = MathF.Max(size.Y, 0);
		}

		Size = size;

		float scale = GetScale();
		pos += Offset * scale;
		pos = pos.RoundClosest();
		Position = pos.ToVec3(Z);

		// Invalidate transformations.
		if (_transformationStackBacking != null) _transformationStackBacking.MatrixDirty = true;

		List<UIBaseWindow> children = Children ?? EMPTY_CHILDREN_LIST;

		if (children.Count > 0)
		{
			var spaceForChildren = new Rectangle(Position, size);
			Rectangle scaledPadding = Paddings * scale;
			spaceForChildren.X += scaledPadding.X;
			spaceForChildren.Y += scaledPadding.Y;
			spaceForChildren.Width -= scaledPadding.X + scaledPadding.Width;
			spaceForChildren.Height -= scaledPadding.Y + scaledPadding.Height;

			switch (LayoutMode)
			{
				case LayoutMode.HorizontalList:
				case LayoutMode.HorizontalListWrap:
				case LayoutMode.VerticalList:
				case LayoutMode.VerticalListWrap:
					bool wrap = LayoutMode is LayoutMode.HorizontalListWrap or LayoutMode.VerticalListWrap;
					int mask = LayoutMode switch
					{
						LayoutMode.HorizontalList or LayoutMode.HorizontalListWrap => 0,
						LayoutMode.VerticalList or LayoutMode.VerticalListWrap => 1,

						_ => 0 // ???
					};

					Vector2 usedSpace = LayoutMode_ListLayout(children, spaceForChildren, wrap, mask);

					// todo: list in list will break stuff
					if (FillX && Anchor is not UIAnchor.TopCenter and not UIAnchor.CenterCenter and not UIAnchor.BottomCenter) usedSpace.X = Size.X;
					if (FillY && Anchor is not UIAnchor.CenterLeft and not UIAnchor.CenterCenter and not UIAnchor.CenterRight) usedSpace.Y = Size.Y;
					Assert(usedSpace.X <= Size.X, "Layout used space X is more than parent set size!");
					Assert(usedSpace.Y <= Size.Y, "Layout used space Y is more than parent set size!");

					usedSpace = Vector2.Clamp(usedSpace, MinSize * scale, MaxSize * scale).Ceiling();
					Size = usedSpace;
					break;
				case LayoutMode.Free:
					LayoutMode_FreeLayout(children, spaceForChildren);
					break;
			}
		}

		// Layout children relative to other windows. Might be a better idea to do this in a separate pass to
		// ensure that everything else layouted, but for now we'll do it here.
		for (var i = 0; i < children.Count; i++)
		{
			UIBaseWindow child = children[i];
			if (child.RelativeTo == null) continue;
		}

		// Construct input detecting boundary that includes this window's children.
		_inputBoundsWithChildren = Bounds;
		for (var i = 0; i < children.Count; i++)
		{
			UIBaseWindow child = children[i];
			_inputBoundsWithChildren = Rectangle.Union(child._inputBoundsWithChildren, _inputBoundsWithChildren);
		}

		AfterLayout();
	}

	protected static Vector2 GetChildPositionAnchorWise(UIAnchor parentAnchor, UIAnchor anchor, Rectangle parentContentRect, Vector2 contentSize)
	{
		Vector2 offset = Vector2.Zero;

		switch (parentAnchor)
		{
			case UIAnchor.TopLeft:
			case UIAnchor.CenterLeft:
			case UIAnchor.BottomLeft:
				offset.X += parentContentRect.X;
				break;
			case UIAnchor.TopCenter:
			case UIAnchor.CenterCenter:
			case UIAnchor.BottomCenter:
				offset.X += parentContentRect.Center.X;
				break;
			case UIAnchor.TopRight:
			case UIAnchor.CenterRight:
			case UIAnchor.BottomRight:
				offset.X += parentContentRect.Width;
				break;
		}

		switch (parentAnchor)
		{
			case UIAnchor.TopLeft:
			case UIAnchor.TopCenter:
			case UIAnchor.TopRight:
				offset.Y += parentContentRect.Y;
				break;
			case UIAnchor.CenterLeft:
			case UIAnchor.CenterCenter:
			case UIAnchor.CenterRight:
				offset.Y += parentContentRect.Center.Y;
				break;
			case UIAnchor.BottomLeft:
			case UIAnchor.BottomCenter:
			case UIAnchor.BottomRight:
				offset.Y += parentContentRect.Height;
				break;
		}

		switch (anchor)
		{
			case UIAnchor.TopCenter:
			case UIAnchor.CenterCenter:
			case UIAnchor.BottomCenter:
				offset.X -= contentSize.X / 2;
				break;
			case UIAnchor.TopRight:
			case UIAnchor.CenterRight:
			case UIAnchor.BottomRight:
				offset.X -= contentSize.X;
				break;
		}

		switch (anchor)
		{
			case UIAnchor.CenterLeft:
			case UIAnchor.CenterCenter:
			case UIAnchor.CenterRight:
				offset.Y -= contentSize.Y / 2;
				break;
			case UIAnchor.BottomLeft:
			case UIAnchor.BottomCenter:
			case UIAnchor.BottomRight:
				offset.Y -= contentSize.Y;
				break;
		}

		return offset;
	}
}

#endif