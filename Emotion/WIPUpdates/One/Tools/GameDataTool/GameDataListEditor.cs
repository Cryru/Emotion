﻿#nullable enable

using Emotion.Game.Data;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

using static Emotion.Game.Data.GameDatabase;

namespace Emotion.WIPUpdates.One.Tools.GameDataTool;

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
