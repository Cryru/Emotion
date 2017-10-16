// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;

#endregion

namespace Soul.Engine.AssetPacker
{
    public class AssetFile
    {
        public string Path = "";

        public string Name
        {
            get
            {
                string strippedPath = Path.Replace(Program.AssetsPath + System.IO.Path.DirectorySeparatorChar, "")
                    .Replace(Program.CachePath + System.IO.Path.DirectorySeparatorChar, "").Replace(".fragment", "");

                //string noExtension = strippedPath.Substring(0, strippedPath.IndexOf(".", StringComparison.Ordinal));

                return strippedPath;
            }
        }

        public string FullName
        {
            get
            {
                return Path.Substring(Path.LastIndexOf(System.IO.Path.DirectorySeparatorChar) + System.IO.Path.DirectorySeparatorChar.ToString().Length);
            }
        }

        public long TimeUpdated;

        public Status Status = Status.Unset;
    }

    public enum Status
    {
        Unset,
        Unchanged,
        Added,
        Updated,
        Deleted
    }
}