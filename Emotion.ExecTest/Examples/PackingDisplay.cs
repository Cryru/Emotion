#region Using

using System.Collections.Generic;
using Emotion.Common;
using Emotion.Game;
using Emotion.Game.Time;
using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.Scenography;
using Emotion.Utility;

#endregion

namespace Emotion.ExecTest.Examples
{
	public class PackingDisplay : IScene
	{
		private static int _showing;
		private static Rectangle[] _rects;
		private static Rectangle _boundingRect;

		private static Every _timer = new Every(50, () =>
		{
			_showing++;
			if (_showing <= _rects.Length - 1) return;
			_showing = 0;
			Regenerate();
		});

		private static void Regenerate()
		{
			var r = new List<Rectangle>();
			for (var i = 0; i < 20; i++)
			{
				int width = Helpers.GenerateRandomNumber(1, 80);
				int height = Helpers.GenerateRandomNumber(1, 180);
				r.Add(new Rectangle(0, 0, width, height));
			}

			_rects = r.ToArray();
			_boundingRect = new Rectangle(0, 0, Packing.FitRectangles(_rects));
		}

		public void Load()
		{
			Helpers.SetRandomGenSeed(100);
			Regenerate();
		}

		public void Update()
		{
			_timer.Update(Engine.DeltaTime);
		}

		public void Draw(RenderComposer composer)
		{
			composer.SetUseViewMatrix(false);

			composer.RenderOutline(_boundingRect, Color.White);

			for (int i = 0; i < _rects.Length; i++)
			{
				if (i > _showing) break;

				var col = (uint) _rects[i].GetHashCode();
				composer.RenderSprite(_rects[i], new Color(col).SetAlpha(255));
			}

			composer.SetUseViewMatrix(true);
		}

		public void Unload()
		{
		}
	}
}