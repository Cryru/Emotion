#nullable enable

using Emotion.Editor.EditorUI.Components;
using Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;
using static Emotion.Game.Systems.GameData.GameDatabase;

namespace Emotion.Editor.Tools.GameDataTool;

public class GameDataListEditor : ListEditor<GameDataObject>
{
    private GameDataEditor _editor;

    public GameDataListEditor(GameDataEditor editor, Type typ) : base(typ)
    {
        _editor = editor;
    }

    protected override bool CanCreateItems()
    {
        return true;
    }

    protected override object? CreateNewItem()
    {
        GameDataObject? newItem = EditorAdapter.CreateNew(_editor.GameDataType);
        if (newItem == null) return null;

        newItem.Id = EditorAdapter.EnsureNonDuplicatedId(newItem.Id, _editor.EmulatedEditList);
        newItem.Index = _editor.EmulatedEditList.Count;

        return newItem;
    }

    public void UpdatePendingChangesForItems()
    {
        UpdateUI();
    }

    protected override void UpdateListItemUI(int idx, EditorListItem<GameDataObject> itemUI)
    {
        base.UpdateListItemUI(idx, itemUI);
        if (itemUI.Item == null) return;
        itemUI.LabelSuffix = _editor.IsObjectModified(itemUI.Item) ? "*" : string.Empty;
    }
}
