#region Using

using System.Threading.Tasks;
using Emotion.Graphics;

#endregion

namespace Emotion.Scenography
{
    public class LegacySceneWrapper : Scene
    {
        private IScene _innerScene;

        public LegacySceneWrapper(IScene scene)
        {
            _innerScene = scene;
        }

        public override Task LoadAsync()
        {
            return Task.Run(_innerScene.Load);
        }

        public override void Update()
        {
            _innerScene.Update();
        }

        public override void Draw(RenderComposer composer)
        {
            _innerScene.Draw(composer);
        }

        public override void Unload()
        {
            base.Unload();
            _innerScene.Unload();
        }

        public override string ToString()
        {
            return $"LegacyScene: {_innerScene}";
        }

        public override bool Equals(object obj)
        {
            return _innerScene.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _innerScene.GetHashCode();
        }
    }
}