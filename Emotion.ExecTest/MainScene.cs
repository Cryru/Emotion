// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Numerics;
using System.Threading.Tasks;
using Emotion.Engine;
using Emotion.Engine.Hosting.Desktop;
using Emotion.Engine.Scenography;
using Emotion.Graphics;
using Emotion.Primitives;

#endregion

namespace Emotion.ExecTest
{
    public class MainScene : Scene
    {
        public static void Main(string[] args)
        {
            Context.Setup();
            Context.SceneManager.SetScene(new MainScene());
            Context.Run();
        }

        public override void Load()
        {
        }

        public override void Update(float frameTime)
        {
            Rectangle mouseRect = new Rectangle(Context.InputManager.GetMousePosition(), new Vector2(1, 1));
            if (new Rectangle(10, 10, 10, 10).Intersects(mouseRect) && Context.InputManager.IsMouseKeyDown(Input.MouseKeys.Left))
            {
                Task.Run(() =>
                {
                    if (Context.Settings.HostSettings.WindowMode == WindowMode.Windowed)
                    {
                        Context.Settings.HostSettings.WindowMode = Engine.Hosting.Desktop.WindowMode.Borderless;
                        Context.Host.ApplySettings(Context.Settings.HostSettings);
                    }
                    else if (Context.Settings.HostSettings.WindowMode == Engine.Hosting.Desktop.WindowMode.Borderless)
                    {
                        Context.Settings.HostSettings.WindowMode = WindowMode.Windowed;
                        Context.Host.ApplySettings(Context.Settings.HostSettings);
                    }
                    Context.Host.ApplySettings(Context.Settings.HostSettings);
                });
            }

        }

        public override void Draw(Renderer renderer)
        {
            Context.Renderer.Render(new Vector3(0, 0, 0), new Vector2(960, 540), Color.CornflowerBlue);
            Context.Renderer.Render(new Vector3(10, 10, 0), new Vector2(10, 10), Color.White);
        }

        public override void Unload()
        {
        }
    }
}