#region Using

using System.Collections;
using System.Threading.Tasks;
using Emotion.Common.Serialization;
using Emotion.Game.Time;
using Emotion.Game.Time.Routines;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.Standard.XML;
using Emotion.Utility;

#endregion

#nullable enable

namespace Emotion.UI
{
    [DontSerializeMembers("Position", "Size")]
    public partial class UIBaseWindow : IRenderable, IComparable<UIBaseWindow>, IEnumerable<UIBaseWindow>
    {
        public static float DefaultMaxSizeF = 9999;

        /// <summary>
        /// By default windows greedily take up all the size they can.
        /// </summary>
        public static Vector2 DefaultMaxSize = new(DefaultMaxSizeF, DefaultMaxSizeF);

        /// <summary>
        /// Unique identifier for this window to be used with GetWindowById. If two windows share an id the one closer
        /// to the parent GetWindowById is called from will be returned.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Whether to layout the window using the new layout system.
        /// Off by default due to legacy compatibility, will be on by default once the new system is stable.
        /// </summary>
        public bool UseNewLayoutSystem = false;

        #region Runtime State

        [DontSerialize]
        public UIBaseWindow? Parent { get; protected set; }

        [DontSerialize]
        public UIController? Controller { get; protected set; }

        #endregion

        #region Loading, Update, Render

        private Task _loadingTask = Task.CompletedTask;
        private bool _needsLoad = true;

        public bool IsLoading()
        {
            return !_loadingTask.IsCompleted || _needsLoad;
        }

        public void CheckLoadContent(UILoadingContext ctx)
        {
            // Add to loading only if not currently loading.
            lock (this)
            {
                if (_loadingTask.IsCompleted)
                {
                    _loadingTask = LoadContent();
                    // if (!_loadingTask.IsCompleted) Engine.Log.Trace(ToString(), "UI Loading");
                    ctx.AddLoadingTask(_loadingTask);
                    _needsLoad = false;
                }
            }

            if (Children == null) return;
            for (var i = 0; i < Children.Count; i++)
            {
                UIBaseWindow child = Children[i];
                child.CheckLoadContent(ctx);
            }
        }

        protected virtual void InvalidateLoaded()
        {
            _needsLoad = true;
            Controller?.InvalidatePreload();
        }

        protected virtual Task LoadContent()
        {
            return Task.CompletedTask;
        }

        public void Update()
        {
            if (!Visible) return;
            Assert(Controller != null || this is UIController);

            if (_updateColor)
            {
                CalculateColor();
                _updateColor = false;
            }

            bool updateChildren = UpdateInternal();
            if (!updateChildren || Children == null) return;

            for (var i = 0; i < Children.Count; i++)
            {
                UIBaseWindow child = Children[i];
                child.Update();
            }
        }

        protected virtual bool UpdateInternal()
        {
            // nop
            return true;
        }

        public void Render(RenderComposer c)
        {
            if (!Visible) return;
            if (IsLoading()) return;

            // Push displacements if any.
            var matrixPushed = false;
            if (_transformationStackBacking != null)
            {
                if (_transformationStackBacking.MatrixDirty)
                {
                    // Mark children matrices as dirty.
                    if (Children != null)
                    {
                        for (int i = 0; i < Children.Count; i++)
                        {
                            UIBaseWindow child = Children[i];
                            if (child.IgnoreParentDisplacement) continue;
                            if (child._transformationStackBacking != null)
                                child._transformationStackBacking.MatrixDirty = true;
                        }
                    }

                    Rectangle intermediateBounds;
                    if (IgnoreParentDisplacement)
                    {
                        intermediateBounds = Bounds;
                    }
                    else
                    {
                        intermediateBounds = Rectangle.Transform(Bounds, c.ModelMatrix);
                        intermediateBounds.Position = intermediateBounds.Position.Floor();
                        intermediateBounds.Size = intermediateBounds.Size.Ceiling();
                    }
                    _transformationStackBacking.RecalculateMatrix(GetScale(), intermediateBounds, Z);
                }

                c.PushModelMatrix(_transformationStackBacking.CurrentMatrix, !IgnoreParentDisplacement);
                matrixPushed = true;
            }
            else if (IgnoreParentDisplacement && !c.ModelMatrix.IsIdentity)
            {
                c.PushModelMatrix(Matrix4x4.Identity, false);
                matrixPushed = true;
            }

            // Cache displaced position.
            EnsureRenderBoundsCached(c);

            if (RenderInternal(c) && Children != null)
            {
                RenderChildren(c);
                // Pop displacements, if any were pushed.
                if (matrixPushed) c.PopModelMatrix();
                AfterRenderChildren(c);
            }
            else if (matrixPushed)
            {
                c.PopModelMatrix();
            }

            if (UIController.Debug_RenderLayoutEngine == this)
                _layoutEngine.RenderDebug(this, c);
        }

        protected virtual bool RenderInternal(RenderComposer c)
        {
            return true;
        }

        protected virtual void RenderChildren(RenderComposer c)
        {
            for (var i = 0; i < Children!.Count; i++)
            {
                UIBaseWindow child = Children[i];
                if (!child.Visible) continue;
                child.Render(c);
            }
        }

        protected virtual void AfterRenderChildren(RenderComposer c)
        {
        }

        #endregion

        #region Hierarchy

        /// <summary>
        /// Children of this window.
        /// </summary>
        [SerializeNonPublicGetSet]
        public List<UIBaseWindow>? Children { get; protected set; }

        public virtual void AddChild(UIBaseWindow? child)
        {
            Assert(child != null || child == this);
            if (child == this || child == null) return;

            if (Engine.Configuration.DebugMode && Children != null && !string.IsNullOrEmpty(child.Id))
                for (var i = 0; i < Children.Count; i++)
                {
                    UIBaseWindow c = Children[i];
                    if (c.Id == child.Id)
                    {
                        Engine.Log.Warning($"Child with duplicate id was added - {child.Id}", "UI");
                        break;
                    }
                }

            Children ??= new List<UIBaseWindow>();
            Children.Add(child);

            child.Parent = this;
            child.InvalidateLayout();
            child.InvalidateColor();
            child.EnsureParentLinks();
            if (Controller != null) child.AttachedToController(Controller);
            SortChildren();
        }

        public void SortChildren()
        {
            if (Children == null) return;

            // Custom insertion sort as Array.Sort is unstable
            // Isn't too problematic performance wise since adding children shouldn't happen often.
            for (var i = 1; i < Children.Count; i++)
            {
                UIBaseWindow thisC = Children[i];
                for (int j = i - 1; j >= 0;)
                {
                    UIBaseWindow otherC = Children[j];
                    if (thisC.CompareTo(otherC) < 0)
                    {
                        Children[j + 1] = otherC;
                        Children[j] = thisC;
                        j--;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Parents aren't serialized so the links need to be reestablished once the UI is loaded.
        /// </summary>
        protected void EnsureParentLinks()
        {
            if (Children == null) return;
            for (var i = 0; i < Children.Count; i++)
            {
                UIBaseWindow child = Children[i];
                child.Parent = this;
                child.EnsureParentLinks();
            }
        }

        public virtual void RemoveChild(UIBaseWindow child, bool evict = true)
        {
            if (Children == null) return;
            Assert(child.Parent == this);

            if (evict) Children.Remove(child);
            if (Controller != null) child.DetachedFromController(Controller);
            InvalidateLayout();
        }

        public virtual void ClearChildren()
        {
            if (Children == null) return;
            for (var i = 0; i < Children.Count; i++)
            {
                UIBaseWindow child = Children[i];
                RemoveChild(child, false);
            }

            Children = null;
        }

        public void RecalculateZValue()
        {
            Z = ZOffset + (Parent?.Z + 0.01f ?? 0);
        }

        public virtual void AttachedToController(UIController controller)
        {
            bool isPresent = controller.IsWindowPresentInHierarchy(this);
            if (isPresent)
            {
                Assert(false, "Window is present in hierarchy twice!");
                return;
            }

            controller.RegisterWindowInController(this);

            Controller = controller;
            Controller?.InvalidatePreload();
            RecalculateZValue();
            if (Children == null) return;
            for (var i = 0; i < Children.Count; i++)
            {
                UIBaseWindow child = Children[i];
                child.AttachedToController(controller);
            }
        }

        public virtual void DetachedFromController(UIController controller)
        {
            controller.RemoveWindowFromController(this);

            Controller = null;
            if (Children == null) return;
            for (var i = 0; i < Children.Count; i++)
            {
                UIBaseWindow child = Children[i];
                child.DetachedFromController(controller);
            }
        }

        public void Close()
        {
            Assert(Parent != null);
            Parent?.RemoveChild(this);
        }

        /// <summary>
        /// Marks this window as generated by code. Such windows will not be serialized by the UI editor.
        /// </summary>
        [DontSerialize]
        public bool CodeGenerated { get; set; }

        public virtual void RemoveCodeGeneratedChildren()
        {
            if (Children == null) return;
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                UIBaseWindow child = Children[i];
                if (child.CodeGenerated)
                    RemoveChild(child);
                else
                    child.RemoveCodeGeneratedChildren();
            }
        }

        public bool IsWithin(UIBaseWindow? within)
        {
            if (within == null) return false;
            if (within == this) return true;

            UIBaseWindow? parent = Parent;
            while (parent != null)
            {
                if (parent == within) return true;
                parent = parent.Parent;
            }

            return false;
        }

        #endregion

        #region Layout

        /// <summary>
        /// The point in the parent to anchor the window to.
        /// </summary>
        public UIAnchor ParentAnchor
        {
            get => _parentAnchor;
            set
            {
                if (value == _parentAnchor) return;
                _parentAnchor = value;
                InvalidateLayout();
            }
        }

        private UIAnchor _parentAnchor { get; set; } = UIAnchor.TopLeft;

        /// <summary>
        /// Where the window should anchor to relative to the alignment in its parent.
        /// </summary>
        public UIAnchor Anchor
        {
            get => _anchor;
            set
            {
                if (value == _anchor) return;
                _anchor = value;
                InvalidateLayout();
            }
        }

        private UIAnchor _anchor { get; set; } = UIAnchor.TopLeft;

        /// <summary>
        /// How to layout the children of this window.
        /// </summary>
        public LayoutMode LayoutMode { get; set; } = LayoutMode.Free;

        /// <summary>
        /// Spacing if the LayoutMode is a list.
        /// </summary>
        public Vector2 ListSpacing
        {
            get => _listSpacing;
            set
            {
                if (value == _listSpacing) return;
                _listSpacing = value;
                InvalidateLayout();
            }
        }

        private Vector2 _listSpacing;

        /// <summary>
        /// Whether the window is visible.
        /// If not, the RenderInternal function will not be called and
        /// children will not be drawn either.
        /// </summary>
        public bool Visible
        {
            get => _visible;
            set
            {
                if (value == _visible) return;
                _visible = value;
                Controller?.InvalidateInputFocus();
                if (DontTakeSpaceWhenHidden)
                    Controller?.InvalidateLayout();
            }
        }

        private bool _visible = true;

        /// <summary>
        /// Whether to consider this window as part of the layout when invisible.
        /// Matters only within lists.
        /// </summary>
        public bool DontTakeSpaceWhenHidden { get; set; }

        /// <summary>
        /// The Z axis is combined with that of the parent, whose is combined with that of their parent, and so forth.
        /// This is the Z offset for this window, added to this window and its children.
        /// </summary>
        public float ZOffset
        {
            get => _zOffset;
            set
            {
                if (_zOffset == value) return;
                _zOffset = value;
                RecalculateZValue();
                InvalidateLayout();
            }
        }

        protected float _zOffset;

        /// <summary>
        /// Margins push the window in one of the four directions, only if it is against another window.
        /// This is applied after alignment, but before the anchor.
        /// </summary>
        public Rectangle Margins
        {
            get => _margins;
            set
            {
                if (_margins == value) return;
                _margins = value;
                InvalidateLayout();
            }
        }

        private Rectangle _margins;

        /// <summary>
        /// Paddings push children windows if they are inside the parent.
        /// </summary>
        public Rectangle Paddings
        {
            get => _paddings;
            set
            {
                if (_paddings == value) return;
                _paddings = value;
                InvalidateLayout();
            }
        }

        private Rectangle _paddings;

        /// <summary>
        /// The minimum size the window can be.
        /// </summary>
        public Vector2 MinSize
        {
            get => new(MinSizeX, MinSizeY);
            set
            {
                MinSizeX = value.X;
                MinSizeY = value.Y;
            }
        }

        [DontSerialize] // will be saved via the Vector2 prop
        public float MinSizeX;

        [DontSerialize] //
        public float MinSizeY;

        /// <summary>
        /// The maximum size the window can be.
        /// </summary>
        public Vector2 MaxSize
        {
            get => new(MaxSizeX, MaxSizeY);
            set
            {
                MaxSizeX = value.X;
                MaxSizeY = value.Y;
            }
        }

        [DontSerialize] // will be saved via the Vector2 prop
        public float MaxSizeX = DefaultMaxSizeF;

        [DontSerialize] //
        public float MaxSizeY = DefaultMaxSizeF;

        /// <summary>
        /// Always added to the position after all other checks.
        /// Mostly used to offset from an anchor position.
        /// </summary>
        public Vector2 Offset
        {
            get => _offsetBacking;
            set
            {
                if (value == _offsetBacking) return;
                _offsetBacking = value;
                InvalidateLayout();
            }
        }

        private Vector2 _offsetBacking;

        /// <summary>
        /// Position and size is multiplied by Renderer.Scale, Renderer.IntScale or neither.
        /// </summary>
        public UIScaleMode ScaleMode { get; set; } = UIScaleMode.FloatScale;

        /// <summary>
        /// Position relative to another window in the same controller.
        /// </summary>
        public string? RelativeTo
        {
            get => _relativeTo;
            set
            {
                if (value == _relativeTo) return;
                _relativeTo = value;
                InvalidateLayout();
            }
        }

        private string? _relativeTo;

        public virtual void InvalidateLayout()
        {
            Parent?.InvalidateLayout();
        }

        protected virtual Vector2 BeforeLayout(Vector2 position)
        {
            SortChildren();
            return position;
        }

        protected virtual void AfterMeasure(Vector2 contentSize)
        {
        }

        protected virtual void AfterLayout()
        {
            // nop - to be overriden
        }

        #endregion

        #region Color

        /// <summary>
        /// This color should be mixed in with the rendering of the window somehow.
        /// Used to control opacity as well.
        /// </summary>
        public Color WindowColor
        {
            get => _windowColor;
            set
            {
                _windowColor = value;
                InvalidateColor();
            }
        }

        private Color _windowColor = Color.White;

        /// <summary>
        /// If set to true then this window will not mix its color with its parent's.
        /// </summary>
        public bool IgnoreParentColor = false;

        protected Color _calculatedColor;
        protected bool _updateColor = true;

        public void InvalidateColor()
        {
            _updateColor = true;
            if (Children == null) return;
            for (var i = 0; i < Children.Count; i++)
            {
                UIBaseWindow child = Children[i];
                child.InvalidateColor();
            }
        }

        protected Coroutine? _alphaTweenRoutine;
        private ITimer? _alphaTweenTimer;

        private IEnumerator AlphaTweenRoutine(ITimer alphaTween, byte startingAlpha, byte targetAlpha, bool? setVisible)
        {
            while (true)
            {
                alphaTween.Update(Engine.DeltaTime);
                var current = (byte) Maths.Lerp(startingAlpha, targetAlpha, alphaTween.Progress);
                WindowColor = WindowColor.SetAlpha(current);

                // If fading in we need to set visible from the get go.
                if (setVisible != null && setVisible.Value && !Visible) Visible = true;

                if (alphaTween.Finished)
                {
                    Assert(WindowColor.A == targetAlpha);
                    if (setVisible != null) Visible = setVisible.Value;
                    yield break;
                }

                InvalidateColor();
                yield return null;
            }
        }

        public void SetAlpha(byte value, ITimer? tween = null)
        {
            Engine.CoroutineManager.StopCoroutine(_alphaTweenRoutine);
            _alphaTweenTimer?.End();
            _alphaTweenTimer = null;

            if (tween == null)
            {
                WindowColor = WindowColor.SetAlpha(value);
                return;
            }

            if (WindowColor.A == value)
            {
                tween.End();
                _alphaTweenRoutine = null;
                return;
            }

            _alphaTweenTimer = tween;
            _alphaTweenRoutine = Engine.CoroutineManager.StartCoroutine(AlphaTweenRoutine(tween, WindowColor.A, value, null));
        }

        public void SetVisible(bool val, ITimer? tween = null)
        {
            SetVisibleFade(val, tween);
        }

        public IRoutineWaiter SetVisibleFade(bool val, ITimer? tween = null)
        {
            Engine.CoroutineManager.StopCoroutine(_alphaTweenRoutine);
            _alphaTweenTimer?.End();
            _alphaTweenTimer = null;

            var targetAlpha = (byte) (val ? 255 : 0);
            if (tween == null)
            {
                WindowColor = WindowColor.SetAlpha(targetAlpha);
                Visible = val;
                return Coroutine.CompletedRoutine;
            }

            if (Visible == val && WindowColor.A == targetAlpha)
            {
                tween.End();
                _alphaTweenRoutine = null;
                return Coroutine.CompletedRoutine;
            }

            _alphaTweenTimer = tween;
            _alphaTweenRoutine = Engine.CoroutineManager.StartCoroutine(AlphaTweenRoutine(tween, WindowColor.A, targetAlpha, Visible == val ? null : val));
            return new PassiveRoutineObserver(_alphaTweenRoutine);
        }

        protected void CalculateColor()
        {
            if (Parent == null || IgnoreParentColor)
                _calculatedColor = WindowColor;
            else
                _calculatedColor = WindowColor * Parent._calculatedColor.A;

            if (Children == null) return;
            for (var i = 0; i < Children.Count; i++)
            {
                UIBaseWindow child = Children[i];
                child.CalculateColor();
            }
        }

        #endregion

        #region Input

        public bool ChildrenHandleInput
        {
            get => _childrenHandleInput;
            set
            {
                if (value == _childrenHandleInput) return;
                _childrenHandleInput = value;
                Controller?.InvalidateInputFocus();
            }
        }

        private bool _childrenHandleInput = true;

        public bool HandleInput
        {
            get => _handleInput;
            set
            {
                if (value == _handleInput) return;
                _handleInput = value;
                Controller?.InvalidateInputFocus();
            }
        }

        private bool _handleInput;

        public virtual bool OnKey(Key key, KeyStatus status, Vector2 mousePos)
        {
            return true;
        }

        /// <summary>
        /// Whether the mouse is currently inside this window.
        /// </summary>
        [DontSerialize]
        public bool MouseInside { get; protected set; }

        protected Rectangle _renderBoundsCalculatedFrom; // .Bounds at time of caching.
        private Matrix4x4? _renderBoundsCachedMatrix; // The matrix _renderBounds was generated from.
        protected Rectangle _renderBounds; // Bounds but with any displacements active on the window applied 
        protected Rectangle _renderBoundsWithChildren; // _inputBoundsWithChildren but with any displacements active on the window applied
        private Rectangle _inputBoundsWithChildren; // Bounds unioned with all children bounds.

        public Rectangle RenderBounds
        {
            get => _renderBoundsWithChildren;
        }

        public void EnsureRenderBoundsCached(RenderComposer c)
        {
            if (c.ModelMatrix == _renderBoundsCachedMatrix && _renderBoundsCalculatedFrom == _inputBoundsWithChildren) return;
            _renderBoundsWithChildren = Rectangle.Transform(_inputBoundsWithChildren, c.ModelMatrix);
            _renderBoundsWithChildren.Position = _renderBoundsWithChildren.Position.Floor();
            _renderBoundsWithChildren.Size = _renderBoundsWithChildren.Size.Ceiling();

            _renderBounds = Rectangle.Transform(Bounds, c.ModelMatrix);
            _renderBounds.Position = _renderBounds.Position.Floor();
            _renderBounds.Size = _renderBounds.Size.Ceiling();

            _renderBoundsCachedMatrix = c.ModelMatrix;
            _renderBoundsCalculatedFrom = _inputBoundsWithChildren;
        }

        public virtual bool IsPointInside(Vector2 pt)
        {
            return _renderBoundsCalculatedFrom != Rectangle.Empty ? _renderBoundsWithChildren.Contains(pt) : _inputBoundsWithChildren.Contains(pt);
        }

        public virtual bool IsInsideRect(Rectangle rect)
        {
            return _renderBoundsCalculatedFrom != Rectangle.Empty ? rect.ContainsInclusive(_renderBoundsWithChildren) : rect.ContainsInclusive(_inputBoundsWithChildren);
        }

        public virtual bool IsInsideOrIntersectRect(Rectangle rect, out bool inside)
        {
            Rectangle checkAgainst = _renderBoundsCalculatedFrom != Rectangle.Empty ? _renderBoundsWithChildren : _inputBoundsWithChildren;
            if (rect.ContainsInclusive(checkAgainst))
            {
                inside = true;
                return true;
            }

            if (rect.IntersectsInclusive(checkAgainst))
            {
                inside = false;
                return true;
            }

            inside = false;
            return false;
        }

        /// <summary>
        /// Find the window under the mouse cursor in this parent.
        /// This could be either a child window or the parent itself.
        /// </summary>
        public virtual UIBaseWindow? FindMouseInput(Vector2 pos)
        {
            if (!Visible) return null;

            if (Children != null && ChildrenHandleInput)
                for (int i = Children.Count - 1; i >= 0; i--) // Top to bottom
                {
                    UIBaseWindow win = Children[i];
                    if (win.Visible && win.IsPointInside(pos))
                    {
                        UIBaseWindow? inChild = win.FindMouseInput(pos);
                        if (inChild != null) return inChild;
                    }
                }

            if (HandleInput && (_renderBoundsCalculatedFrom != Rectangle.Empty ? _renderBounds.Contains(pos) : Bounds.Contains(pos)))
                return this;

            return null;
        }

        /// <summary>
        /// Find a window that handles input in this parent.
        /// Could be either a child window or the parent itself.
        /// </summary>
        public UIBaseWindow? FindInputFocusable()
        {
            if (!Visible) return null;

            if (Children != null && ChildrenHandleInput)
                for (int i = Children.Count - 1; i >= 0; i--)
                {
                    UIBaseWindow win = Children[i];
                    if (win.Visible)
                    {
                        UIBaseWindow? found = win.FindInputFocusable();
                        if (found != null) return found;
                    }
                }

            return HandleInput ? this : null;
        }

        public virtual void OnMouseEnter(Vector2 mousePos)
        {
            MouseInside = true;
        }

        public virtual void OnMouseLeft(Vector2 mousePos)
        {
            MouseInside = false;
        }

        public virtual void OnMouseMove(Vector2 mousePos)
        {
        }

        public virtual void InputFocusChanged(bool haveFocus)
        {
        }

        #endregion

        #region Animations

        /// <summary>
        /// Ignore the displacements on the parent, otherwise the displacements of this window are multiplied by the parent's.
        /// </summary>
        public bool IgnoreParentDisplacement { get; set; }

        /// <summary>
        /// List of affine transformations to apply to this window and its children.
        /// </summary>
        [DontSerialize]
        public NamedTransformationStack TransformationStack
        {
            get => _transformationStackBacking ??= new NamedTransformationStack();
        }

        protected NamedTransformationStack? _transformationStackBacking;

        /// <summary>
        /// Displace the position of a UI window over time.
        /// At the end of the tween the window will remain displaced.
        /// </summary>
        public IEnumerator TranslationDisplacement(Vector3 position, ITimer tween, string id = "translation")
        {
            while (true)
            {
                tween.Update(Engine.DeltaTime);
                Vector3 current = Vector3.Lerp(Vector3.Zero, position, tween.Progress);
                TransformationStack.AddOrUpdate(id, Matrix4x4.CreateTranslation(current.X, current.Y, 0));
                if (tween.Finished) yield break;

                yield return null;
            }
        }

        public IEnumerator PositionDisplacement(Vector3 position, ITimer tween, string id = "translation")
        {
            while (true)
            {
                tween.Update(Engine.DeltaTime);
                Vector3 current = Vector3.Lerp(Position / GetScale(), position, tween.Progress);
                TransformationStack.AddOrUpdate(id, Matrix4x4.CreateTranslation(current.X, current.Y, 0), true, MatrixSpecialFlag.TranslationPositionReplace);
                if (tween.Finished) break;

                yield return null;
            }
        }

        public IEnumerator ScaleDisplacement(float scaleStart, float scaleTarget, ITimer tween, string id = "scale")
        {
            while (true)
            {
                tween.Update(Engine.DeltaTime);
                float current = Maths.Lerp(scaleStart, scaleTarget, tween.Progress);
                TransformationStack.AddOrUpdate(id,
                    Matrix4x4.CreateScale(current, current, 1f), true, MatrixSpecialFlag.ScaleBoundsCenter);
                if (tween.Finished) break;

                yield return null;
            }
        }


        /// <summary>
        /// The window's rotation around its center, in degrees.
        /// </summary>
        public float Rotation
        {
            get => _rotation;
            set
            {
                if (_rotation == value) return;
                SetRotation(value);
            }
        }

        private float _rotation;

        private Coroutine? _rotationRoutineCurrent;
        private ITimer? _rotationTweenCurrent;

        /// <summary>
        /// Set a rotation (in degrees) for this window around its center.
        /// </summary>
        public void SetRotation(float degrees, ITimer? tween = null)
        {
            if (_rotationRoutineCurrent != null && _rotationRoutineCurrent.Active)
                Engine.CoroutineManager.StopCoroutine(_rotationRoutineCurrent);
            _rotationRoutineCurrent = Engine.CoroutineManager.StartCoroutine(RotationDisplacement(_rotation, degrees, tween));
        }

        /// <summary>
        /// Rotate a UI window around its center. Optionally over time.
        /// </summary>
        private IEnumerator RotationDisplacement(float fromDegrees, float toDegrees, ITimer? tween = null, string id = "rotation")
        {
            if (tween == null)
            {
                _rotation = toDegrees;
                TransformationStack.AddOrUpdate(id, Matrix4x4.CreateRotationZ(Maths.DegreesToRadians(toDegrees)), true, MatrixSpecialFlag.RotateBoundsCenter);
                yield break;
            }

            while (true)
            {
                tween.Update(Engine.DeltaTime);
                float current = Maths.LerpAngle(fromDegrees, toDegrees, tween.Progress);
                _rotation = current;
                TransformationStack.AddOrUpdate(id, Matrix4x4.CreateRotationZ(Maths.DegreesToRadians(current)), true, MatrixSpecialFlag.RotateBoundsCenter);
                if (tween.Finished) yield break;

                yield return null;
            }
        }

        #endregion

        #region Layout Helpers

        public void SetExactRequestedSize(Vector2 size)
        {
            size = size.Ceiling();
            MinSize = size;
            MaxSize = size;
        }

        /// <summary>
        /// A very simple check for whether the anchors will land the window inside or outside the parent.
        /// This decides whether the window will layout within the parent or outside it.
        /// </summary>
        /// <returns></returns>
        protected static bool AnchorsInsideParent(UIAnchor parentAnchor, UIAnchor anchor)
        {
            bool parentIsTop = parentAnchor is UIAnchor.TopLeft or UIAnchor.TopCenter or UIAnchor.TopRight;
            bool parentIsVCenter = parentAnchor is UIAnchor.CenterLeft or UIAnchor.CenterCenter or UIAnchor.CenterRight;
            bool parentIsBottom = parentAnchor is UIAnchor.BottomLeft or UIAnchor.BottomCenter or UIAnchor.BottomRight;

            bool parentIsLeft = parentAnchor is UIAnchor.TopLeft or UIAnchor.CenterLeft or UIAnchor.BottomLeft;
            bool parentIsHCenter = parentAnchor is UIAnchor.TopCenter or UIAnchor.CenterCenter or UIAnchor.BottomCenter;
            bool parentIsRight = parentAnchor is UIAnchor.TopRight or UIAnchor.CenterRight or UIAnchor.BottomRight;

            if (parentIsTop)
            {
                if (parentIsLeft && anchor == UIAnchor.TopLeft) return true;

                if (parentIsHCenter && anchor is UIAnchor.TopLeft or UIAnchor.TopCenter or UIAnchor.TopRight) return true;

                if (parentIsRight && anchor == UIAnchor.TopRight) return true;
            }
            else if (parentIsVCenter)
            {
                if (parentIsLeft && anchor is UIAnchor.TopLeft or UIAnchor.CenterLeft or UIAnchor.BottomLeft) return true;
                if (parentIsHCenter) return true;
                if (parentIsRight && anchor is UIAnchor.TopRight or UIAnchor.CenterRight or UIAnchor.BottomRight) return true;
            }
            else if (parentIsBottom)
            {
                if (parentIsLeft && anchor == UIAnchor.BottomLeft) return true;
                if (parentIsHCenter && anchor is UIAnchor.BottomLeft or UIAnchor.BottomCenter or UIAnchor.BottomRight) return true;
                if (parentIsRight && anchor == UIAnchor.BottomRight) return true;
            }

            return false;
        }

        protected static Vector2 GetUIAnchorPosition(UIAnchor parentAnchor, Vector2 parentSize, Rectangle parentContentRect, UIAnchor anchor, Vector2 contentSize)
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
                    offset.X += parentSize.X / 2;
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
                    offset.Y += parentSize.Y / 2;
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

        /// <summary>
        /// The scale factor applied on the UI.
        /// </summary>
        /// <returns></returns>
        public float GetScale()
        {
            return ScaleMode switch
            {
                UIScaleMode.FloatScale => Engine.Renderer.Scale,
                UIScaleMode.IntScale => Engine.Renderer.IntScale,
                _ => 1.0f
            };
        }

        #endregion

        #region Helpers

        public const string SPECIAL_WIN_ID_MOUSE_FOCUS = "MouseFocus";
        public const string SPECIAL_WIN_ID_CONTROLLER = "Controller";


        /// <summary>
        /// Get a window with the specified id which is either a child of this window,
        /// or below it on the tree.
        /// </summary>
        /// <param name="id">The id of the window to look for.</param>
        /// <returns>The instance of the window.</returns>
        public virtual UIBaseWindow? GetWindowById(string id)
        {
            if (id == SPECIAL_WIN_ID_MOUSE_FOCUS)
                return UIController.MouseFocus;

            if (id == SPECIAL_WIN_ID_CONTROLLER)
                return Controller;

            if (id == Id) return this;
            if (Children == null) return null;

            for (var i = 0; i < Children.Count; i++)
            {
                if (Children[i].Id == id) return Children[i];
            }

            for (var i = 0; i < Children.Count; i++)
            {
                UIBaseWindow? win = Children[i].GetWindowById(id);
                if (win != null) return win;
            }

            return null;
        }

        public bool VisibleAlongTree()
        {
            if (Controller == null) return false;

            UIBaseWindow? parent = Parent;
            while (parent != null)
            {
                if (!parent.Visible) return false;
                parent = parent.Parent;
            }

            return Visible;
        }

        /// <summary>
        /// Clone the window. By default this performs a serialization clone.
        /// </summary>
        /// <returns></returns>
        public UIBaseWindow Clone()
        {
            string? xml = XMLFormat.To(this);
            AssertNotNull(xml); // Serialization fail?
            return XMLFormat.From<UIBaseWindow>(xml)!;
        }

        /// <summary>
        /// Compares the windows based on their Z order.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(UIBaseWindow? other)
        {
            if (other == null) return MathF.Sign(ZOffset);
            return MathF.Sign(ZOffset - other.ZOffset);
        }

        public override string ToString()
        {
            return $"{GetType().ToString().Replace("Emotion.UI.", "")} {Id}";
        }

        public static T? CreateFromAsset<T>(string assetPath) where T : UIBaseWindow
        {
            var asset = Engine.AssetLoader.Get<XMLAsset<UIBaseWindow>>(assetPath);
            if (asset == null) return null;
            var content = asset.Content;
            if (content == null) return null;
            if (content is not T) return null;

            // Clone as to prevent modifying the cached asset data.
            // We could alternatively load the asset as non-cached but that
            // will mean it is read from the disk every time this window is
            // initialized.

            string? xml = XMLFormat.To(content);
            AssertNotNull(xml); // Serialization failed, how did it deserialize then?
            return XMLFormat.From<T>(xml)!;
        }

        #endregion

        #region Enum

        public IEnumerator<UIBaseWindow> GetEnumerator()
        {
            for (var i = 0; i < Children?.Count; i++)
            {
                UIBaseWindow cur = Children[i];

                // Get children.
                var childrenLists = new Queue<List<UIBaseWindow>>();
                if (cur.Children != null) childrenLists.Enqueue(cur.Children);
                yield return cur;

                while (childrenLists.Count > 0)
                {
                    List<UIBaseWindow> list = childrenLists.Dequeue();
                    for (var j = 0; j < list.Count; j++)
                    {
                        UIBaseWindow child = list[j];
                        // Get grandchildren if any.
                        if (child.Children?.Count > 0) childrenLists.Enqueue(child.Children);
                        yield return child;
                    }
                }
            }
        }

        public IEnumerable<UIBaseWindow> WindowChildren()
        {
            for (var i = 0; i < Children?.Count; i++)
            {
                UIBaseWindow cur = Children[i];
                yield return cur;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Rollover Functionality

        public virtual UIRollover? GetRollover()
        {
            return null;
        }

        #endregion
    }
}