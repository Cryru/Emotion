// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Collections.Generic;
using Emotion;
using Emotion.Engine;
using Emotion.Game.Layering;
using Emotion.Game.Text;
using Emotion.GLES;
using Emotion.IO;
using Emotion.Primitives;

#endregion

namespace EmotionSandbox.Examples
{
    public class Input : Layer
    {
        private static Color _bgColor = new Color(59, 62, 66);
        private static Color _letterBgColor = new Color(74, 79, 89, 200);
        private static Color _outlineColor = new Color(255, 255, 255, 100);
        private static Color _textColor = new Color(220, 220, 220, 240);
        private static Color _textColorHeld = new Color(92, 142, 224, 240);
        private static Color _textColorPressed = new Color(193, 81, 50, 240);

        private Font _displayFont;
        private List<string> _eventList;
        private RichText _textArea;

        public static void Main()
        {
            Context context = Starter.GetEmotionContext(settings => { settings.ClearColor = _bgColor; });
            context.LayerManager.Add(new Input(), "Input Example", 0);
            context.Start();
        }

        public override void Load()
        {
            _displayFont = Context.AssetLoader.Get<Font>("ExampleFont.ttf");
            _eventList = new List<string>();

            _textArea = new RichText(new Rectangle(0, 0, 0, 0), _displayFont, "", 15, true);
        }

        public override void Draw(Renderer renderer)
        {
            for (short i = 0; i < 255; i++)
            {
                if (Context.Input.IsKeyDown(i)) _eventList.Add("isKeyDown(\"" + Context.Input.GetKeyNameFromCode(i) + "\")");
                if (Context.Input.IsKeyUp(i)) _eventList.Add("IsKeyUp(\"" + Context.Input.GetKeyNameFromCode(i) + "\")");
            }

            _textArea.Text = string.Join("\n", _eventList);
            _textArea.Draw(renderer);

            // This is not the most optimized/smart way to do the following operation. It is however the most readable, and this is an example.
            Vector2 center = renderer.ScreenCenter;

            Vector2 charSize = _displayFont.MeasureString("S", 40);
            Vector2 sLocation = new Vector2(center.X - charSize.X, center.Y - charSize.Y);
            Rectangle sBounds = new Rectangle(sLocation, charSize);
            sBounds = RenderKey("S", sBounds, renderer);

            charSize = _displayFont.MeasureString("A", 40);
            Vector2 aLocation = new Vector2(sBounds.X - charSize.X - 15, sBounds.Y + 10);
            Rectangle aBounds = new Rectangle(aLocation, charSize);
            RenderKey("A", aBounds, renderer);

            charSize = _displayFont.MeasureString("D", 40);
            Vector2 dLocation = new Vector2(sBounds.X + sBounds.Width + 15, sBounds.Y + 10);
            Rectangle dBounds = new Rectangle(dLocation, charSize);
            RenderKey("D", dBounds, renderer);

            charSize = _displayFont.MeasureString("W", 40);
            Vector2 wLocation = new Vector2(sBounds.X + 5, sBounds.Y - charSize.Y - 15);
            Rectangle wBounds = new Rectangle(wLocation, charSize);
            RenderKey("W", wBounds, renderer);
        }

        public override void Update(float frameTime)
        {
            _textArea.Update(frameTime);
        }

        public override void Unload()
        {
            Context.AssetLoader.Free("ExampleFont.ttf");
        }

        #region Helpers

        private Rectangle RenderKey(string key, Rectangle loc, Renderer renderer)
        {
            Color color = _textColor;

            if (Context.Input.IsKeyDown(key))
                color = _textColorPressed;
            else if (Context.Input.IsKeyHeld(key))
                color = _textColorHeld;

            Rectangle inflated = loc;
            inflated.Inflate(10, 10);

            renderer.DrawRectangle(inflated, _letterBgColor);
            renderer.DrawRectangleOutline(inflated, _outlineColor);
            renderer.DrawText(_displayFont, 40, key, color, loc.Location);

            return inflated;
        }

        #endregion
    }
}