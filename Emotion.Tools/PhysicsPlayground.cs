#region Using

using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.DebugTools;
using Emotion.Graphics;
using Emotion.Platform.Input;
using Emotion.Primitives;
using Emotion.Scenography;
using Emotion.Tools.OtherPhysics.Velcro;

#endregion


namespace Emotion.ExecTest.Examples
{
    public class PhysicsPlayground : Scene
    {
        private PhysicsTester _tester = new PhysicsTester(0.10f);

        public override Task LoadAsync()
        {
            _tester.AddAdapter(new EmotionPhysicsTestAdapter(new Rectangle(0, 0, 0, 0), new Vector2(0, 10)));
            //_tester.AddAdapter(new VelcroPhysicsAdapter(new Vector2(0, 10)));
            _tester.AddBody(new Vector2(0, 100), new Vector2(500, 5), 0, 0);
            _tester.AddBody(new Vector2(10, 50), new Vector2(10, 10), 0, 1);
            _tester.AddBody(new Vector2(15, 30), new Vector2(10, 10), 0, 1);
            //_tester.AddBody(new Vector2(5, 0), new Vector2(10, 10), 0, 1);

            Engine.Host.OnKey.AddListener((key, status) =>
            {
                if (key == Key.MouseKeyLeft && status == KeyStatus.Down) _tester.AddBody(Engine.Renderer.Camera.ScreenToWorld(Engine.Host.MousePosition), new Vector2(10, 10), 0, 1);

                return true;
            }, KeyListenerType.Game);

            return Task.CompletedTask;
        }

        public override void Update()
        {
            _tester.Step(Engine.DeltaTime / 1000f);
        }

        public override void Draw(RenderComposer composer)
        {
            composer.SetUseViewMatrix(false);
            composer.RenderSprite(new Vector3(0, 0, 0), Engine.Renderer.CurrentTarget.Size, Color.CornflowerBlue);
            composer.SetUseViewMatrix(true);

            _tester.Render(composer);
        }

        public override void Unload()
        {
        }
    }
}