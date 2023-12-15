#region Using

using Emotion.UI;

#endregion

namespace Emotion.Game.World.Editor
{
    public class MapEditorInfoWorldAttach : UIWorldAttachedWindow
    {
        protected override Vector3 VerifyWorldPos(Vector3 pos)
        {
            var uiWillBe = new Rectangle(pos.X, pos.Y, Width, Height);
            var screenBound = new Rectangle(0, 0, Engine.Renderer.CurrentTarget.Size);
            if (!screenBound.ContainsInclusive(uiWillBe))
            {
                Vector2 mousePos = Engine.Host.MousePosition;
                return new Vector3(mousePos.X, mousePos.Y, pos.Z);
            }

            return pos;
        }
    }
}