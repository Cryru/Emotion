using Emotion.Editor.EditorHelpers;
using Emotion.Editor.EditorWindows.DataEditorUtil;
using Emotion.Game.World2D;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.IO;
using Emotion.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Emotion.Editor.EditorWindows
{
    public class DataEditorGeneric : EditorPanel
    {
        protected Type _type;
        protected UICallbackListNavigator _list = null!;
        protected UIBaseWindow _rightSide = null!;
        protected GameDataObject? _selectedObject;
        
        public DataEditorGeneric(Type type) : base($"{type.Name} Editor")
        {
            _type = type;
        }

        public override void AttachedToController(UIController controller)
        {
            base.AttachedToController(controller);

            var leftPart = new UIBaseWindow();
            leftPart.StretchX = true;
            leftPart.StretchY = true;
            leftPart.LayoutMode = LayoutMode.VerticalList;
            _contentParent.AddChild(leftPart);

            {
                var buttonList = new UIBaseWindow();
                buttonList.StretchX = true;
                buttonList.StretchY = true;
                buttonList.LayoutMode = LayoutMode.HorizontalList;
                buttonList.Margins = new Rectangle(0, 0, 0, 5);
                buttonList.ListSpacing = new Vector2(5, 0);
                leftPart.AddChild(buttonList);

                var newButton = new MapEditorTopBarButton();
                newButton.StretchY = true;
                newButton.Text = "New";
                newButton.OnClickedProxy = (_) =>
                {
                    CreateNew();
                };
                buttonList.AddChild(newButton);

                var saveButton = new MapEditorTopBarButton();
                saveButton.StretchY = true;
                saveButton.Text = "Save";
                saveButton.OnClickedProxy = (_) =>
                {
                    SaveToFile();
                };
                buttonList.AddChild(saveButton);

                var listContainer = new UIBaseWindow();
                listContainer.StretchX = true;
                listContainer.StretchY = true;
                listContainer.LayoutMode = LayoutMode.HorizontalList;
                listContainer.ZOffset = 10;
                leftPart.AddChild(listContainer);

                var listNav = new UICallbackListNavigator();
                listNav.LayoutMode = LayoutMode.VerticalList;
                listNav.StretchX = true;
                listNav.ListSpacing = new Vector2(0, 1);
                listNav.Margins = new Rectangle(0, 0, 5, 0);
                listNav.ChildrenAllSameWidth = true;
                listNav.MinSizeX = 100;
                listNav.OnChoiceConfirmed += (wnd, idx) =>
                {
                    MapEditorTopBarButton? nuSelButton = wnd as MapEditorTopBarButton;
                    if (nuSelButton != null)
                    {
                        object? userData = nuSelButton.UserData;
                        if (userData != null && userData.GetType().IsAssignableTo(_type))
                        {
                            _selectedObject = (GameDataObject) userData;
                            RegenerateSelection();
                        }
                    }
                };
                listContainer.AddChild(listNav);

                var scrollBar = new EditorScrollBar();
                listNav.SetScrollbar(scrollBar);
                listContainer.AddChild(scrollBar);

                _list = listNav;
            }

            var rightPart = new UIBaseWindow();
            rightPart.StretchX = true;
            rightPart.StretchY = true;
            rightPart.LayoutMode = LayoutMode.VerticalList;
            _rightSide = rightPart;
            _contentParent.AddChild(rightPart);

            _contentParent.LayoutMode = LayoutMode.HorizontalList;
            RegenerateList();
        }

        private void CreateNew()
        {
            ConstructorInfo constructor = _type.GetConstructor(Type.EmptyTypes)!;
            var newObj = (GameDataObject)constructor.Invoke(null);
            newObj.Id = "Untitled";
            
            GameDataDatabase.EditorAddObject(_type, newObj);
            RegenerateList();
        }

        private void SaveToFile()
        {
            if (_selectedObject == null) return;
            var asset = XMLAsset<GameDataObject>.CreateFromContent(_selectedObject, _selectedObject.AssetPath);
            asset.Save();
        }

        private void RegenerateList()
        {
            _list.ClearChildren();

            Dictionary<string, GameDataObject>? data = GameDataDatabase.GetObjectsOfType(_type);
            if (data == null) return;

            foreach (var item in data)
            {
                var UIForItem = new MapEditorTopBarButton();
                UIForItem.StretchY = true;
                UIForItem.Text = item.Key;
                UIForItem.UserData = item.Value;
                _list.AddChild(UIForItem);
            }
            _list.SetupMouseSelection();
        }

        private void RegenerateSelection()
        {
            _rightSide.ClearChildren();
            if (_selectedObject == null) return;
            GenericPropertiesEditorPanel properties = new GenericPropertiesEditorPanel(_selectedObject);
            properties.PanelMode = PanelMode.Embedded;
            properties.OnPropertyEdited = (propertyName, oldValue) =>
            {
                if (propertyName == "Id")
                {
                    var newId = GameDataDatabase.EnsureNonDuplicatedId(_selectedObject.Id, _type);
                    _selectedObject.Id = newId;

                    var newPath = GameDataDatabase.GetAssetPath(_selectedObject);
                    var oldAssetPath = _selectedObject.AssetPath;
                    _selectedObject.AssetPath = newPath;
                    SaveToFile();

                    // Remove the old file.
                    // Keep in mind that if it was already loaded (by the game or such)
                    // as an asset it will stay loaded.
                    var path = DebugAssetStore.AssetDevPath;
                    var oldFileSystemPath = System.IO.Path.Join(path, oldAssetPath);
                    if (System.IO.File.Exists(oldFileSystemPath))
                    {
                        System.IO.File.Delete(oldFileSystemPath);
                    }
                    var assetPath = System.IO.Path.Join("Assets", oldAssetPath);
                    if (System.IO.File.Exists(assetPath))
                    {
                        System.IO.File.Delete(assetPath);
                    }

                    GameDataDatabase.EditorReIndex(_type);
                    RegenerateList();
                }
            };

            _rightSide.AddChild(properties);
        }
    }
}
