#nullable enable

using Emotion.Core.Systems.Scenography;
using GameData.MonsterDefs;
using Emotion.Editor.EditorUI.Base;
using Emotion.Editor.EditorUI.Components.One;
using Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.Game.Systems.UI2;
using Emotion.Primitives.DataStructures;
using System.Collections;
using Emotion.Editor;

namespace Emotion.ExecTest.ExamplesOne;

public class TestTool : SplitEditorWindow
{
    NTree<string, string> _tree = new NTree<string, string>();

    public TestTool() : base(3, "Hello!")
    {
        _tree.Add(["Group 1"], "Item 1");
        _tree.Add(["Group 1"], "Item 2");
        _tree.Add(["Group 1", "Subgroup"], "Item 1");
        _tree.Add(["Group 2", "Subgroup"], "Item 1");
        _tree.AddLeaf("TopLevel Item 1");
        _tree.AddGetBranch("Group No Items");


        {
            var contentParent = GetSplitContentParent(0);
            contentParent.Layout.LayoutMethod = UILayoutMethod.VerticalList(2);
            contentParent.AddChild(new UIText()
            {
                Text = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum."
            });

            var objEdit = new ObjectPropertyWindow();
            objEdit.SetEditor(new TestData());
            contentParent.AddChild(objEdit);
        }

        {
            var contentParent = GetSplitContentParent(1);
            contentParent.Layout.LayoutMethod = UILayoutMethod.VerticalList(2);
            contentParent.AddChild(new UIText()
            {
                Text = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum."
            });

            var objEdit = new ObjectPropertyWindow();
            objEdit.SetEditor(new TestData());
            contentParent.AddChild(objEdit);
        }

        {
            var contentParent = GetSplitContentParent(2);
            var treeView = new OneTreeView<string, string>(_tree);
            contentParent.AddChild(treeView);
        }
    }

    protected override void OnOpen()
    {
        base.OnOpen();


    }
}

public class ExampleEmpty : SceneWithMap
{
    protected override IEnumerator InternalLoadSceneRoutineAsync()
    {
        EngineEditor.OpenEditor();
        SceneUI.AddChild(new TestTool());
        yield break;
    }
}
