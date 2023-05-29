#region Using

using System.Threading.Tasks;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Game.World2D.SceneControl;
using Emotion.UI;

#endregion

namespace Emotion.Game.World2D
{
	public class MapEditorCreateMapPanel : MapEditorPanel
	{
		private World2DEditor _editor;
		private Type _mapType;

		private MapEditorString _nameInput;
		private MapEditorString _pathInput;

		public MapEditorCreateMapPanel(World2DEditor editor, Type mapType) : base("New Map")
		{
			_editor = editor;
			_mapType = mapType;
		}

		public override void AttachedToController(UIController controller)
		{
			base.AttachedToController(controller);

			_contentParent.LayoutMode = LayoutMode.VerticalList;
			_contentParent.ListSpacing = new Vector2(0, 5);

			_contentParent.AddChild(MapEditorString.CreateStringEditorWithLabel("Name:", true, out _nameInput));
			_contentParent.AddChild(MapEditorString.CreateStringEditorWithLabel("File Path:", true, out _pathInput));

			_nameInput.MinSize = new Vector2(100, 0);
			_pathInput.MinSize = new Vector2(100, 0);

			MapEditorCreateMapPanel creationDlg = this;
			Task creationTask = null;

			var confirmButton = new MapEditorTopBarButton();
			confirmButton.Text = "Create";
			confirmButton.Id = "CreateButton";
			confirmButton.Anchor = UIAnchor.TopCenter;
			confirmButton.ParentAnchor = UIAnchor.TopCenter;
			confirmButton.StretchY = true;
			confirmButton.Enabled = false;
			confirmButton.OnClickedProxy = _ =>
			{
				if (creationTask != null && !creationTask.IsCompleted) return;

				creationTask = Task.Run(() =>
				{
					var newMap = (Map2D) Activator.CreateInstance(_mapType, true)!;

					string fileName = _pathInput.Text;
					if (!fileName.EndsWith(".xml")) fileName += ".xml";

					// We need to do some manual init as the serialization constructor will expect these to be present.
					newMap.MapName = _nameInput.Text;
					newMap.FileName = fileName;
					newMap.MapSize = new Vector2(1, 1);
					newMap.PersistentObjects = new List<GameObject2D>();
					newMap.InitAsync().Wait();

					_editor.EditorSaveMap(newMap);
					_editor.ChangeSceneMap(newMap);
					creationDlg.Close();
				});
			};
			_contentParent.AddChild(confirmButton);
		}

		protected override bool UpdateInternal()
		{
			var createButton = (MapEditorTopBarButton) GetWindowById("CreateButton");

			if (createButton != null && _nameInput != null && _pathInput != null)
				createButton.Enabled = !string.IsNullOrEmpty(_nameInput.Text) && !string.IsNullOrEmpty(_pathInput.Text);

			return base.UpdateInternal();
		}
	}
}