#region Using

using System.Threading.Tasks;
using Emotion.Graphics;
using Emotion.Platform.Input;

#endregion

#nullable enable

namespace Emotion.UI
{
    public partial class UIController : UIBaseWindow
    {
        /// <summary>
        /// The priority for key events of this controller.
        /// </summary>
        public KeyListenerType KeyPriority { get; private set; }

        /// <summary>
        /// List of all controllers running. Non disposed controllers will still appear here.
        /// It is maintained in order for controllers to not steal events from each other.
        /// </summary>
        protected static List<UIController> _allControllers = new List<UIController>();

        /// <summary>
        /// The window that will receive keyboard key events.
        /// </summary>
        public UIBaseWindow? InputFocus { get; protected set; }

        /// <summary>
        /// Override for InputFocus due to internal logic such as focused text inputs.
        /// </summary>
        protected UIBaseWindow? _inputFocusManual;

        /// <summary>
        /// The currently open dropdown.
        /// </summary>
        public UIDropDown? DropDown { get; set; }

        protected bool[] _mouseFocusKeysHeld = new bool[Key.MouseKeyEnd - Key.MouseKeyStart];

        protected bool _updatePreload = true;
        protected bool _updateLayout = true;
        protected bool _updateInputFocus = true;
        protected bool _mouseUpdatedThisTick = false;

        protected HashSet<UIBaseWindow> _doubleAddChecker = new HashSet<UIBaseWindow>();

        public UIController(KeyListenerType inputPriority = KeyListenerType.UI)
        {
            HandleInput = true;
            KeyPriority = inputPriority;
            Engine.Host.OnResize += Host_OnResize;
            Engine.Host.OnMouseMove += Host_MouseMove;
            KeepTemplatePreloaded(this);

            _allControllers.Add(this);
            _allControllers.Sort(UIControllerInputSort);
        }

        private static int UIControllerInputSort(UIController a, UIController b)
        {
            return Math.Sign((byte) a.KeyPriority - (byte) b.KeyPriority);
        }

        public virtual void Dispose()
        {
            _allControllers.Remove(this);
            StopPreloadTemplate(this);
            Engine.Host.OnResize -= Host_OnResize;
            Engine.Host.OnMouseMove -= Host_MouseMove;
            if (InputFocus != null) Engine.Host.OnKey.RemoveListener(KeyboardFocusOnKey);
            if (_myMouseFocus != null) Engine.Host.OnKey.RemoveListener(MouseFocusOnKey);
            ClearChildren();
        }

        private void Host_OnResize(Vector2 obj)
        {
            InvalidateLayout();
            InvalidatePreload();
        }

        private void Host_MouseMove(Vector2 old, Vector2 nu)
        {
            UpdateMouseFocus();
            _mouseUpdatedThisTick = true;
        }

        public override void InvalidateLayout()
        {
            _updateLayout = true;
            _updateInputFocus = true;
        }

        public void InvalidatePreload()
        {
            _updatePreload = true;
        }

        protected override void AfterRenderChildren(RenderComposer c)
        {
#if false
            {
                if(_myMouseFocus != null) c.RenderOutline(_myMouseFocus.RenderBounds, Color.Red);
                c.RenderSprite(new Rectangle(Engine.Host.MousePosition.X, Engine.Host.MousePosition.Y, 1, 1), Color.Pink);
            }
#endif

            base.AfterRenderChildren(c);
        }

        protected override bool UpdateInternal()
        {
            if (!_loadingThread.IsCompleted) return false;
            if (_updatePreload) UpdateLoading();
            if (_updateLayout) UpdateLayout();
            if (_updateInputFocus) UpdateInputFocus();

            if (!_mouseUpdatedThisTick) UpdateMouseFocus();
            _mouseUpdatedThisTick = false;

            return true;
        }

#if NEW_UI
		protected override Vector2 InternalMeasure(Vector2 space)
		{
			return Engine.Renderer.DrawBuffer.Size;
		}
#else
        protected override Vector2 InternalMeasure(Vector2 space)
        {
            return Engine.Renderer.DrawBuffer.Size;
        }

        protected override Vector2 NEW_InternalMeasure(Vector2 space)
        {
            return Engine.Renderer.DrawBuffer.Size;
        }
#endif

        protected void UpdateLayout()
        {
            _updateLayout = false;

#if NEW_UI
			BuildRelativeToMapping();

			// 1. Measure the minimum size each window needs, which in turn
            // determines the minimum size of the parent. Children are measured in index order,
            // and layout rules and extra metrics (paddings, margins) are calculated too.
			Measure(Engine.Renderer.DrawBuffer.Size);

			// 2. Layout windows within their parents, starting with the controller taking up the full screen.
            // Sizes returned during measuring can be used, but larger sizes can be set. Positions are
            // absolute and not relative.
			Layout(Vector2.Zero, Size);

#else
            BuildRelativeToMapping();

            // 1. Measure the size of all windows.
            // Children are measured before parents in order for stretching to work.
            // Children are measured in index order. Layout rules are applied.
            Size = Engine.Renderer.DrawBuffer.Size;
            Measure(Size);

            // 2. Layout windows within their parents, starting with the controller taking up the full screen.
            // Sizes returned during measuring are used. Parents are positioned before children since
            // positions are absolute and not relative.
            Vector2 pos = CalculateContentPos(Vector2.Zero, Engine.Renderer.DrawBuffer.Size, Rectangle.Empty);
            Layout(pos);
#endif
        }

        public override void AddChild(UIBaseWindow? child)
        {
            if (child == null) return;
            base.AddChild(child);
            child.AttachedToController(this);
        }

        public override void RemoveChild(UIBaseWindow? child, bool evict = true)
        {
            if (child == null) return;
            base.RemoveChild(child, evict);
            child.DetachedFromController(this);
            InvalidateInputFocus();
        }

        #region Dedupe Hierarchy Checker

        public bool IsWindowPresentInHierarchy(UIBaseWindow win)
        {
            bool present = false;
            lock (_doubleAddChecker)
            {
                present = _doubleAddChecker.Contains(win);
            }

            return present;
        }

        public void RegisterWindowInController(UIBaseWindow win)
        {
            lock (_doubleAddChecker)
            {
                _doubleAddChecker.Add(win);
            }
        }

        public void RemoveWindowFromController(UIBaseWindow win)
        {
            lock (_doubleAddChecker)
            {
                _doubleAddChecker.Remove(win);
            }
        }

        #endregion

        #region RelativeTo Layout

        private Dictionary<UIBaseWindow, List<UIBaseWindow>>? _parentToRelatives;

        public List<UIBaseWindow>? GetWindowsRelativeToWindow(UIBaseWindow win)
        {
            if (_parentToRelatives == null) return null;
            _parentToRelatives.TryGetValue(win, out List<UIBaseWindow>? list);
            return list;
        }

        protected void BuildRelativeToMapping()
        {
            var toCheck = new Queue<UIBaseWindow>();
            Dictionary<UIBaseWindow, List<UIBaseWindow>>? parentToRelatives = null;

            if (Children != null)
                toCheck.Enqueue(this);

            while (toCheck.Count > 0)
            {
                UIBaseWindow checkChildren = toCheck.Dequeue();
                List<UIBaseWindow>? children = checkChildren.Children;
                AssertNotNull(children);

                for (var i = 0; i < children.Count; i++)
                {
                    UIBaseWindow child = children[i];
                    if (child.RelativeTo != null)
                    {
                        UIBaseWindow? parent = GetWindowById(child.RelativeTo) ?? Controller?.GetWindowById(child.RelativeTo);

                        if (parent != null)
                        {
                            parentToRelatives ??= new Dictionary<UIBaseWindow, List<UIBaseWindow>>();
                            if (!parentToRelatives.TryGetValue(parent, out List<UIBaseWindow>? list))
                            {
                                list = new List<UIBaseWindow>();
                                parentToRelatives.Add(parent, list);
                            }

                            list.Add(child);
                        }
                    }

                    if (child.Children != null) toCheck.Enqueue(child);
                }
            }

            _parentToRelatives = parentToRelatives;
        }

        #endregion

        #region Loading

        public Task PreloadUI()
        {
            if (!_loadingThread.IsCompleted) return _loadingThread;
            UpdateLoading();
            return _loadingThread;
        }

        private Task _loadingThread = Task.CompletedTask;
        private UILoadingContext _loadingContext = new UILoadingContext();

        protected void UpdateLoading()
        {
            if (!_loadingThread.IsCompleted) return;
            CheckLoadContent(_loadingContext);
            _loadingThread = Task.Run(_loadingContext.LoadWindows);
            _updatePreload = false;

            // If one controller is loading, check global.
            GlobalLoadUI();
        }

        /// <summary>
        /// You can keep windows globally loaded.
        /// </summary>
        private class PreloadWindowStorage : UIBaseWindow
        {
            public override void AddChild(UIBaseWindow? child)
            {
                if (child == null) return;
                Children ??= new List<UIBaseWindow>();
                Children.Add(child);
            }

            public override void RemoveChild(UIBaseWindow child, bool evict = true)
            {
                if (Children == null) return;
                if (evict) Children.Remove(child);
            }
        }

        private static object _globalLoadingLock = new object();
        private static Task _globalLoadingThread = Task.CompletedTask;
        private static PreloadWindowStorage _keepWindowsLoaded = new();
        private static UILoadingContext _globalLoadingContext = new UILoadingContext();

        public static Task GlobalLoadUI()
        {
            lock (_globalLoadingLock)
            {
                if (!_globalLoadingThread.IsCompleted) return _globalLoadingThread;
                _keepWindowsLoaded.CheckLoadContent(_globalLoadingContext);
                _globalLoadingThread = Task.Run(_globalLoadingContext.LoadWindows);
            }

            return _globalLoadingThread;
        }

        public static void KeepTemplatePreloaded(UIBaseWindow window)
        {
            _keepWindowsLoaded.AddChild(window);
        }

        public static void StopPreloadTemplate(UIBaseWindow window)
        {
            _keepWindowsLoaded.RemoveChild(window);
        }

        #endregion

        #region Input

        private bool KeyboardFocusOnKey(Key key, KeyStatus status)
        {
            // It is possible to receive an input even while a recalculating is pending.
            if (_updateInputFocus && status == KeyStatus.Down)
            {
                UpdateInputFocus();
                UpdateMouseFocus();
            }

            if (!Visible) return true;
            if (key > Key.MouseKeyStart && key < Key.MouseKeyEnd) return true;
            if (InputFocus != null && InputFocus.VisibleAlongTree())
            {
                Vector2 mousePos = Engine.Host.MousePosition;
                var current = InputFocus;
                while(current != null)
                {
                    bool propagate = current.OnKey(key, status, mousePos);
                    if (!propagate) return false;
                    current = current.Parent;
                }
            }

            return true;
        }

        protected virtual bool MouseFocusOnKey(Key key, KeyStatus status)
        {
            if (_updateInputFocus && status == KeyStatus.Down)
            {
                UpdateInputFocus();
                UpdateMouseFocus();
            }

            if (!Visible) return true;

            if (key > Key.MouseKeyStart && key < Key.MouseKeyEnd && _myMouseFocus != null)
            {
                _mouseFocusKeysHeld[key - Key.MouseKeyStart] = status == KeyStatus.Down;

                if (key == Key.MouseKeyLeft && status == KeyStatus.Down)
                {
                    // todo: there must be a better way of consuming clicks outside yourself? (SetInputFocus param?)
                    UIBaseWindow? oldFocus = _inputFocusManual;
                    bool oldFocusDropDown = oldFocus is UIDropDown;
                    if (oldFocusDropDown && !_myMouseFocus.IsWithin(_inputFocusManual))
                    {
                        SetInputFocus(null);
                        return false;
                    }

                    // todo: there also must be a way to consume clicks inside yourself that cause you to focus
                    // as it is possible for another key handler to then change the focus due to propagation.
                    // careful - since we dont want buttons to have to be double clicked xd
                    if (_myMouseFocus.HandleInput)
                        SetInputFocus(_myMouseFocus);
                }

                // Testing if this is fixed.
                // Theoretically if two clicks occur within one tick
                // the first one removing the window, this can happen.
                // However I don't believe Emotion will send out both click events?
                // Not sure how the platform code handles this, in any case we don't want to
                // call event handlers of destroyed windows, so lets return out.
                if (status == KeyStatus.Down && _myMouseFocus is not UIController)
                {
                    Assert(_myMouseFocus.Controller != null);
                    if (_myMouseFocus.Controller == null) return true;
                }

                Vector2 mousePos = Engine.Host.MousePosition;
                var current = _myMouseFocus;
                while (current != null)
                {
                    bool propagate = current.OnKey(key, status, mousePos);
                    if (!propagate) return false;
                    current = current.Parent;
                }
            }

            return true;
        }

        public void InvalidateInputFocus()
        {
            _updateInputFocus = true;
        }

        public void SetInputFocus(UIBaseWindow? win)
        {
            // todo: old code, remove?
            //, bool searchTree = false
            //UIBaseWindow? focusable = searchTree && win != null ? FindInputFocusable(win) : win;
            //_inputFocusManual = focusable;

            // If focus is being removed (set to null) then we explicitly don't want to
            // focus the same window as before (or their tree). So we temporary remove their focus.
            var removedHandleInput = false;
            UIBaseWindow? oldFocus = InputFocus;
            if (win == null && oldFocus != null && oldFocus.ChildrenHandleInput)
            {
                oldFocus.ChildrenHandleInput = false;
                removedHandleInput = true;
            }

            _inputFocusManual = win;
            UpdateInputFocus();

            if (removedHandleInput) oldFocus!.ChildrenHandleInput = true;
        }

        private void UpdateInputFocus()
        {
            _updateInputFocus = false;

            UIBaseWindow? newFocus;
            if (!ChildrenHandleInput || !Visible)
            {
                newFocus = null;
            }
            else if (_inputFocusManual != null && _inputFocusManual.VisibleAlongTree() && _inputFocusManual.HandleInput && _inputFocusManual.Controller == this)
            {
                newFocus = _inputFocusManual;
            }
            else
            {
                _inputFocusManual = null;
                newFocus = FindInputFocusable(this);
            }

            if (newFocus == this) newFocus = null;

            if (InputFocus != newFocus)
            {
                UIBaseWindow? commonParentWithOldFocus = null;

                // Re-hook event to get KeyUp events on keys that are pressed down.
                if (InputFocus != null)
                {
                    Engine.Host.OnKey.RemoveListener(KeyboardFocusOnKey);

                    // Send focus remove events only on the part of the tree that will be unfocused.
                    commonParentWithOldFocus = FindCommonParent(InputFocus, newFocus);
                    SetFocusUpTree(InputFocus, false, commonParentWithOldFocus);
                }

                InputFocus = newFocus;

                if (InputFocus != null)
                {
                    Engine.Host.OnKey.AddListener(KeyboardFocusOnKey, KeyPriority);

                    // Send focus add events down to the child that will be focused.
                    SetFocusUpTree(InputFocus, true, commonParentWithOldFocus);
                }

                // Kinda spammy.
                // Engine.Log.Info($"New input focus {InputFocus}", "UI");
            }
        }

        protected void SetFocusUpTree(UIBaseWindow startFrom, bool focus, UIBaseWindow? stopAt)
        {
            if (stopAt == startFrom) return;
            startFrom.InputFocusChanged(focus);

            UIBaseWindow? p = startFrom.Parent;
            while (p != null)
            {
                if (p == stopAt) break;
                p.InputFocusChanged(focus);
                p = p.Parent;
            }
        }

        protected static UIBaseWindow? FindCommonParent(UIBaseWindow one, UIBaseWindow? two)
        {
            if (two == null) return null;
            if (two.IsWithin(one)) return one;
            if (one.IsWithin(two)) return two;

            UIBaseWindow? p = one.Parent;
            while (p != null)
            {
                if (two.IsWithin(p)) return p;
                p = p.Parent;
            }

            return null;
        }

        protected static UIBaseWindow? FindInputFocusable(UIBaseWindow wnd)
        {
            if (!wnd.Visible) return null;

            if (wnd.Children != null && wnd.ChildrenHandleInput)
                for (int i = wnd.Children.Count - 1; i >= 0; i--)
                {
                    UIBaseWindow win = wnd.Children[i];
                    if (win.ChildrenHandleInput && win.Visible)
                    {
                        UIBaseWindow? found = FindInputFocusable(win);
                        if (found != null) return found;
                    }
                }

            return wnd.HandleInput ? wnd : null;
        }

        #endregion

        public static List<UIController> GetControllersLesserPriorityThan(KeyListenerType priority)
        {
            // Note: Lesser priority means these levels will be suppressed, the actual
            // numeric value is actually higher.
            var found = new List<UIController>();
            for (var i = 0; i < _allControllers.Count; i++)
            {
                UIController controller = _allControllers[i];
                if (controller.Visible && (byte) controller.KeyPriority > (byte) priority) found.Add(controller);
            }

            return found;
        }

        /// <summary>
        /// Get a list if window id and bounds. Used for debugging.
        /// </summary>
        public (string?, Rectangle)[] GetLayoutDescription()
        {
            var layout = new List<(string?, Rectangle)>
            {
                (Id, new Rectangle(Position, Size))
            };

            foreach (UIBaseWindow child in this)
            {
                layout.Add((child.Id, new Rectangle(child.Position, child.Size)));
            }

            return layout.ToArray();
        }
    }
}