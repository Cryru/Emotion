#region Using

using System.Collections.Generic;
using Emotion.Common;
using Emotion.IO;
using Emotion.Utility;

#endregion

namespace Emotion.PostBuildTool
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			var config = new Configurator
			{
				DebugMode = true,
				Logger = new ToolLogger(false, "Logs"),
				HiddenWindow = true
			};
			Engine.Setup(config);

			if (args == null || args.Length == 0)
			{
				ShowHelp();
				Engine.Quit();
				return;
			}

			if (CommandLineParser.FindArgument(args, "PackageAssets", out string arg))
				EmotionPackageAssets.PackageAssets();
			else if (CommandLineParser.FindArgument(args, "BuildObj=", out string objPath))
				ObjToEm3(objPath);
			else if (CommandLineParser.FindArgument(args, "BuildEntity=", out string filePath))
				AssimpToEm3(args, filePath);
			else if (CommandLineParser.FindArgument(args, "Help", out string _))
				ShowHelp();

			Engine.Quit();
		}

		public static void ShowHelp()
		{
			Engine.Log.Info("==================", ToolLogger.Help);
			Engine.Log.Info("Emotion Build Tool", ToolLogger.Help);
			Engine.Log.Info("All file paths are relative to ./Assets", ToolLogger.Help);
			Engine.Log.Info("==================", ToolLogger.Help);
			Engine.Log.Info("", ToolLogger.Help);
			Engine.Log.Info("Possible arguments:", ToolLogger.Help);
			Engine.Log.Info("[PackageAssets] Will package all assets in the ./Assets folder into blobs in the ./www/AssetBlobs folder.", ToolLogger.Help);
			Engine.Log.Info("[BuildEntity=<filePath> (AddAnim=<filePath>...)] Will produce a .em3 file from an Assimp compatible file.", ToolLogger.Help);
		}

		public static void ObjToEm3(string objPath)
		{
			var loadedEntity = Engine.AssetLoader.Get<ObjMeshAsset>(objPath);

			Engine.Log.Info($"Saving entity {loadedEntity.Entity.Name}.", ToolLogger.Tool);
			string nameWithoutExtension = AssetLoader.GetFilePathNoExtension(loadedEntity.Name);
			Engine.AssetLoader.Save(EmotionMeshAsset.EntityToByteArray(loadedEntity.Entity), $"AssetsOutput/{nameWithoutExtension}.em3", false);
		}

		public static void AssimpToEm3(string[] args, string filePath)
		{
			var animations = new List<string>();
			var offset = 0;
			while (CommandLineParser.FindArgument(args, "AddAnim=", out string animPath, offset))
			{
				animations.Add(animPath);
				offset++;
			}

			var loadedEntity = Engine.AssetLoader.Get<AssimpAsset>(filePath);
			if (loadedEntity == null) return;
			for (var i = 0; i < animations.Count; i++)
			{
				var animation = Engine.AssetLoader.Get<AssimpAsset>(animations[i]);
				loadedEntity.Entity.Animations = Extensions.JoinArrays(loadedEntity.Entity.Animations, animation.Entity.Animations);
			}

			Engine.Log.Info($"Saving entity {loadedEntity.Entity.Name} with {loadedEntity.Entity.Animations.Length} animations.", ToolLogger.Tool);
			string nameWithoutExtension = AssetLoader.GetFilePathNoExtension(loadedEntity.Name);
			Engine.AssetLoader.Save(EmotionMeshAsset.EntityToByteArray(loadedEntity.Entity), $"AssetsOutput/{nameWithoutExtension}.em3", false);
		}
	}
}