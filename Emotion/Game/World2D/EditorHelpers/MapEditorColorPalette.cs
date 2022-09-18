#region Using

using Emotion.Primitives;

#endregion

namespace Emotion.Game.World2D.EditorHelpers
{
    public static class MapEditorColorPalette
    {
        public static Color BarColor = new Color(25, 25, 25);
        public static Color ButtonColor = new Color("515983").SetAlpha(200);
        public static Color ActiveButtonColor = ButtonColor.Clone().SetAlpha(255);
        public static Color TextColor = Color.White;
    }
}