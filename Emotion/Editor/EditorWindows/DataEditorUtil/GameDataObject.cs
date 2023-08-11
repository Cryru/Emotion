using Emotion.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.Editor.EditorWindows.DataEditorUtil
{
    public abstract class GameDataObject
    {
        public string Id = "Untitled";
        public string AssetPath;

        public bool Save()
        {
            if (string.IsNullOrEmpty(AssetPath)) AssetPath = GameDataDatabase.GetAssetPath(this);
            return XMLAsset<GameDataObject>.CreateFromContent(this, AssetPath).Save();
        }
    }
}
