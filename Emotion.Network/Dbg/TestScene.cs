#region Using

using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.IO;
using Emotion.Network.Game;
using Emotion.Network.Infrastructure;
using Emotion.Primitives;
using Emotion.Tools.Windows;

#endregion

namespace Emotion.Network.Dbg
{
    public class TestScene : NetworkScene
    {
        private FontAsset _font;

        public TestScene(GameClient player) : base(player)
        {
        }

        public TestScene(GameServer server, int msBetweenTicks) : base(server, msBetweenTicks)
        {
        }

        public override async Task LoadAsync()
        {
            Engine.Renderer.Camera = new TrueScaleCamera((Engine.Configuration.RenderSize / 2.0f).ToVec3());
            _font = await Engine.AssetLoader.GetAsync<FontAsset>("calibrib.ttf");
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Draw(RenderComposer composer)
        {
            for (var i = 0; i < SyncedObjects.Count; i++)
            {
                NetworkTransform obj = SyncedObjects[i];
                //composer.RenderSprite(obj.DebugLatestPosition, obj.Size, Color.Green);
                composer.RenderSprite(BallServerSideLocation.ToVec3(), obj.Size, Color.Red);
                composer.RenderSprite(obj.Position, obj.Size, Color.White);
            }

            for (var i = 0; i < _points.Length; i++)
            {
                composer.RenderSprite(_points[i].ToVec3(), new Vector2(20), Color.Blue);
            }

            composer.RenderString(new Vector3(0, 20, 0), Color.White, $"Ping: {Client.Ping.TotalMilliseconds:0}", _font.GetAtlas(15));

            composer.RenderToolsMenu();
        }

        public override void Unload()
        {
        }

        public override void LoadServer()
        {
            Vector2 worldSize = Engine.Configuration.RenderSize;
            NetworkActor p1 = Server.ClientsList[0];

            //var p = new NetworkTransform("P1Paddle")
            //{
            //    Owner = p1.Handle,
            //    Position = new Vector3(30, worldSize.Y / 2 - 70 / 2, 0),
            //    Size = new Vector2(18, 70)
            //};
            //AddObject(p);

            var ballSize = 18;
            var ball = new NetworkTransform("Ball")
            {
                Position = new Vector3(worldSize.X / 2 - ballSize / 2, worldSize.Y / 2 - ballSize / 2, 0),
                Size = new Vector2(ballSize)
            };
            AddObject(ball);
        }

        private Vector2[] _points = new Vector2[4]
        {
            new Vector2(900, 260),
            new Vector2(450, 20),
            new Vector2(30, 260),
            new Vector2(450, 500)
        };
        private int _nextPoint = 0;
        private static Vector2 BallServerSideLocation;

        public override void UpdateServer(float timePassed)
        {
            Time += timePassed;

            NetworkTransform ball = IdToObject["Ball"];
            ref Vector2 targetPoint = ref _points[_nextPoint];
            Vector2 normal = Vector2.Normalize(targetPoint - ball.Position2);

            float step = 0.09f * timePassed;
            ball.Position2 += new Vector2(step) * normal;
            if (Vector2.Distance(ball.Position2, targetPoint) < step)
            {
                _nextPoint++;
                if (_nextPoint == _points.Length) _nextPoint = 0;
            }

            BallServerSideLocation = ball.Position2;
        }
    }
}