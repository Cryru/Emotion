#region Using

using System;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Game;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Tools.Windows.HelpWindows;
using Emotion.UI;

#endregion

#nullable enable

namespace Emotion.Tools.EmUI
{
    public class FileChoiceModal<T> : ModalWindow where T : Asset
    {
        private Action<T> _fileSelected;
        private string _customFile = "";
        private Task _loadingTask;

        private Tree<string, string> _fileSystem;

        /// <summary>
        /// Create a file explorer dialog.
        /// </summary>
        /// <param name="fileSelected">The callback to receive the selected file.</param>
        public FileChoiceModal(Action<T> fileSelected) : base($"Pick a [{typeof(T)}]", true)
        {
            _fileSelected = fileSelected;
            _fileSystem = FileExplorer.FilesToTree(Engine.AssetLoader.AllAssets);

            var listContainer = new UISolidColor();
            listContainer.InputTransparent = false;
            listContainer.StretchY = true;
            listContainer.StretchX = true;
            listContainer.WindowColor = IMBaseWindow.MainColorInner;
            listContainer.Margins = new Rectangle(3, 0, 3, 3);
            listContainer.Paddings = new Rectangle(3, 3, 3, 3);
            ModalContent.AddChild(listContainer);

            var scrollBar = new UIScrollbar();
            scrollBar.MaxSize = new Vector2(6, DefaultMaxSize.Y);
            scrollBar.WindowColor = IMBaseWindow.HeaderColor;
            scrollBar.DefaultSelectorColor = IMBaseWindow.MainColorLightMouseIn;
            scrollBar.Anchor = UIAnchor.TopRight;
            scrollBar.ParentAnchor = UIAnchor.TopRight;
            listContainer.AddChild(scrollBar);

            var list = new UICallbackListNavigator();
            list.Id = "List";
            list.LayoutMode = LayoutMode.VerticalList;
            list.ListSpacing = new Vector2(0, 3);
            list.MinSize = new Vector2(0, 200);
            list.MaxSize = new Vector2(300, 210);
            list.SetScrollbar(scrollBar);
            listContainer.AddChild(list);

            ModalContent.MaxSize = new Vector2(300, DefaultMaxSize.Y);

            LayoutTree(_fileSystem);
        }

        private void LayoutTree(Tree<string, string> tree)
        {
            var list = (UICallbackListNavigator?) GetWindowById("List");
            if (list == null) return;
            list.ClearChildren();

            var currentNodeText = (EditorTextWindow?) GetWindowById("CurrentFolderText");
            if (currentNodeText == null)
            {
                currentNodeText = new EditorTextWindow();
                currentNodeText.Id = "CurrentFolderText";
                currentNodeText.Margins = new Rectangle(3, 0, 0, 0);
                currentNodeText.ZOffset = -1;
                ModalContent.AddChild(currentNodeText);
            }

            currentNodeText.Text = tree.Name ?? "Path: ./Assets";

            for (int i = 0; i < tree.Branches.Count; i++)
            {
                Tree<string, string> branch = tree.Branches[i];
                var folder = new TextCallbackButton(branch.Name);
                folder.OnClickedProxy = _ => { LayoutTree(branch); };
                list.AddChild(folder);
            }
        }

        //public static void RenderTree(Tree<string, string> tree, int depth, Action<string> onClick, bool skipFold = false)
        //{
        //    // Add branches
        //    Tree<string, string> current = tree;

        //    bool open = skipFold || ImGui.TreeNode(string.IsNullOrEmpty(current.Name) ? "/" : current.Name);
        //    if (!open) return;

        //    // Render branches.
        //    foreach (Tree<string, string> b in current.Branches)
        //    {
        //        ImGui.PushID(depth);
        //        RenderTree(b, depth++, onClick);
        //        ImGui.PopID();
        //    }

        //    // Render leaves. (Some LINQ magic here to not render past the clicked button)
        //    foreach (string name in current.Leaves.Where(name => ImGui.Button(Path.GetFileName(name))))
        //    {
        //        // Load the asset custom so the asset loader's caching doesn't get in the way.
        //        onClick(name);
        //    }

        //    if (!skipFold) ImGui.TreePop();
        //}
    }
}