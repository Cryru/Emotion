#region Using

using Assimp;
using Emotion.Common;
using Emotion.IO;
using Emotion.Utility;

#endregion

namespace Emotion.PostBuildTool
{
	public class AssimpAssetIOSystem : IOSystem
	{
		public override IOStream OpenFile(string pathToFile, FileIOMode fileMode)
		{
			var otherAsset = Engine.AssetLoader.Get<OtherAsset>(pathToFile);
			var str = new ReadOnlyLinkedMemoryStream();
			str.AddMemory(otherAsset.Content);
			return new AssimpStream(str, pathToFile, fileMode);
		}
	}
}