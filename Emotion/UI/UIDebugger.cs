#region Using

using System.Collections.Generic;
using System.Diagnostics;

#endregion

#nullable enable

namespace Emotion.UI
{
    public class UIDebugNode
    {
        public UIBaseWindow Window;

        public UIDebugNode(UIBaseWindow window)
        {
            Window = window;
        }

        public List<UIDebugNode> SubNodes = new List<UIDebugNode>();
        public Dictionary<string, object> Metrics = new Dictionary<string, object>();
    }

    public class UIDebugger
    {
        private UIDebugNode? _debugNodeRoot;
        private Dictionary<UIBaseWindow, UIDebugNode>? _windowToNode;

        public UIDebugNode? GetMetricsForWindow(UIBaseWindow window)
        {
            if (_windowToNode == null) return null;
            _windowToNode.TryGetValue(window, out UIDebugNode? node);
            return node;
        }

        public void RecordNewPass(UIBaseWindow root)
        {
            _debugNodeRoot = new UIDebugNode(root);
            _windowToNode ??= new Dictionary<UIBaseWindow, UIDebugNode>();
            _windowToNode.Clear();
        }

        public void RecordMetric(UIBaseWindow window, string name, object metric)
        {
            if (_windowToNode == null) return;

            if (!_windowToNode.TryGetValue(window, out UIDebugNode? node))
            {
                node = new UIDebugNode(window);
                _windowToNode.Add(window, node);

                UIBaseWindow? windowParent = window.Parent;
                if (windowParent != null)
                {
                    bool found = _windowToNode.TryGetValue(windowParent, out UIDebugNode? parentNode);
                    Debug.Assert(found && parentNode != null);
                    parentNode.SubNodes.Add(node);
                }
            }

            bool present = node.Metrics.ContainsKey(name);
            Debug.Assert(!present, "Window is present twice in hierarchy (by reference).");
            if(present) return;
            node.Metrics.Add(name, metric);
        }
    }
}