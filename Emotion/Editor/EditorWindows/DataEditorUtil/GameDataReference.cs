﻿namespace Emotion.Editor.EditorWindows.DataEditorUtil
{
	public interface IGameDataReferenceEditorMarker
	{

	}

	public class GameDataReference<T> : IGameDataReferenceEditorMarker where T : GameDataObject
	{
		public string Id;

		public bool IsValid()
		{
			return GameDataDatabase.GetDataObject<T>(Id) != null;
		}

		public static T[] GetOptions()
		{
			return GameDataDatabase.GetObjectsOfType<T>();
		}

		public override string ToString()
		{
			if (string.IsNullOrEmpty(Id)) return "Empty Reference";
			return IsValid() ? $"Ref: {Id}" : $"Invalid Ref: {Id}";
		}
	}
}