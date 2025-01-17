using Emotion.Game.Data;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;
using static Emotion.Game.Data.GameDataDatabase;

namespace Emotion.WIPUpdates.One.Tools;

public class GameDataEditor<T> : EditorWindowFileSupport where T : GameDataObject, new()
{
    public GameDataEditor() : base($"{typeof(T).Name} Editor")
    {
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        UIBaseWindow contentParent = GetContentParent();

        UISolidColor content = new UISolidColor
        {
            IgnoreParentColor = true,
            MinSize = new Vector2(100),
            WindowColor = new Color(0, 0, 0, 50),
            Paddings = new Primitives.Rectangle(5, 5, 5, 5),
            LayoutMode = LayoutMode.HorizontalEditorPanel
        };
        contentParent.AddChild(content);

        UISolidColor list = new UISolidColor
        {
            IgnoreParentColor = true,
            Id = "List",
            MinSize = new Vector2(50),
            WindowColor = Color.Black * 0.5f,
            LayoutMode = LayoutMode.VerticalList
        };
        content.AddChild(list);

        content.AddChild(new HorizontalPanelSeparator());

        UIBaseWindow contentRight = new()
        {
            IgnoreParentColor = true,
            Id = "SelectedInfo",
            LayoutMode = LayoutMode.VerticalList,
            FillX = false,
            MinSize = new Vector2(50)
        };
        content.AddChild(contentRight);

        SpawnList();
    }

    protected override void CreateTopBarButtons(UIBaseWindow topBar)
    {
        {
            EditorButton button = new EditorButton("New");
            button.OnClickedProxy = (_) => CreateNewItem();
            topBar.AddChild(button);
        }

        //EditorButton fileButton = new EditorButton("File");
        //fileButton.OnClickedProxy = (me) =>
        //{
        //    UIDropDown dropDown = OpenDropdown(me);

        //    {
        //        EditorButton button = new EditorButton("New");
        //        button.FillX = true;
        //        button.OnClickedProxy = (_) => NewFile();
        //        dropDown.AddChild(button);
        //    }

        //    {
        //        EditorButton button = new EditorButton("Open");
        //        button.FillX = true;
        //        button.OnClickedProxy = (_) => OpenFile();
        //        dropDown.AddChild(button);
        //    }

        //    {
        //        EditorButton button = new EditorButton("Save");
        //        button.FillX = true;
        //        button.OnClickedProxy = (_) => SaveFile();
        //        dropDown.AddChild(button);
        //    }

        //    //{
        //    //    EditorButton button = new EditorButton("Save As");
        //    //    button.FillX = true;
        //    //    dropDown.AddChild(button);
        //    //}

        //    Controller?.AddChild(dropDown);
        //};
        //topBar.AddChild(fileButton);
    }

    protected void CreateNewItem()
    {
        EditorAdapter.EditorAddObject(typeof(T), new T());
        UpdateUI();
    }

    protected void UpdateUI()
    {
        SpawnList();
    }

    protected void SpawnList()
    {
        UIBaseWindow list = GetWindowById("List");
        if (list == null) return;

        list.ClearChildren();

        string[] gameDataIds = EditorAdapter.GetObjectIdsOfType(typeof(T));
        foreach (string item in gameDataIds)
        {
            EditorLabel lbl = new EditorLabel();
            lbl.Text = item;
            list.AddChild(lbl);
        }
    }
}
