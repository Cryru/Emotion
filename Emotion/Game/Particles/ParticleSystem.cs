using Emotion.Common.Serialization;
using Emotion.Game.Particles.ParticleShape;
using Emotion.IO;
using Emotion.Utility;

namespace Emotion.Game.Particles
{
    [DontSerialize]
    public class ParticleSystem
    {
        public Vector3 Position;

        public List<ColorAtTime> ColorAtTime = new();
        public List<SizeAtTime> SizeAtTime = new();

        public List<Particle> Particles = new();
        public float Periodicity = 70;
        public float LifeTime = 1000;
        //public ENUM MovementMode
        public float Speed = 32;
        public Circle SpawnShape = new Circle(new Vector2(0, 0), 200);
        public IParticleDirectionShape DirectionShape = new ParticleConstantDirection(new Vector3(0, -1, 0));

        private float _timer = 0;

        private static ObjectPool<Particle> _particlePool = new ObjectPool<Particle>(null, 132);

        public ParticleSystem()
        {

        }

        public void Init()
        {
            ColorAtTime.Sort((x, y) => x.PercentOfLifetime > y.PercentOfLifetime ? 1 : -1);
        }

        public void Update(float dt)
        {
            _timer += dt;

            while (_timer > Periodicity)
            {
                Particle particle = _particlePool.Get();
                particle.AliveTime = 0;
                Vector2 initialPos = SpawnShape.GetRandomPointInsideCircle();
                particle.Position = initialPos.ToVec3();
                DirectionShape.SetParticleDirection(particle);

                particle.Position += Position;
                Particles.Add(particle);
                _timer -= Periodicity;
            }

            for (int i = Particles.Count - 1; i >= 0; i--)
            {
                Particle particle = Particles[i];
                particle.AliveTime += dt;
                if (particle.AliveTime > LifeTime)
                {
                    Particles.Remove(particle);
                    _particlePool.Return(particle);
                }
            }

            var speedPerMS = Speed / 1000f;
            var speedPerDt = speedPerMS * dt;
            foreach (var particle in Particles)
            {
                Vector3 change = particle.TargetDirection * speedPerDt;
                particle.Position += change;
            }
        }

        public void Render(RenderComposer c)
        {
            //var particleTexture = Engine.AssetLoader.Get<TextureAsset>("Particle.png");
            var particleTexture = Engine.AssetLoader.ONE_Get<TextureAsset>("Particle.png");

            //c.RenderCircleOutline(SpawnShape, Color.PrettyYellow, 1, 30);

            //float particleSize = 20;
            foreach (var particle in Particles)
            {
                Color particleColor = GetColorAtCurrentLifetime(particle.AliveTime);
                Vector2 particleSize = GetSizeAtCurrentLifetime(particle.AliveTime);
                c.RenderSprite(particle.Position - new Vector3(particleSize.X / 2f, particleSize.Y / 2f, 0), particleSize, particleColor, particleTexture);
            }
        }

        private Color GetColorAtCurrentLifetime(float currentTime)
        {
            float timePercentage = currentTime / LifeTime;

            if (ColorAtTime.Count == 0)
                return new Color(255, 255, 255, 255);

            ColorAtTime first = ColorAtTime[0];
            if (timePercentage < first.PercentOfLifetime)
                return first.Color;

            ColorAtTime last = ColorAtTime[^1];
            if (timePercentage > last.PercentOfLifetime)
                return last.Color;

            Color color1 = new Color(255, 255, 255, 255);
            Color color2 = new Color(255, 255, 255, 255);
            float amount = 0f;
            for (int i = 0; i < ColorAtTime.Count; i++)
            {
                ColorAtTime current = ColorAtTime[i];
                if (timePercentage < current.PercentOfLifetime)
                {
                    ColorAtTime previous = ColorAtTime[i - 1];

                    color1 = previous.Color;
                    color2 = current.Color;
                    amount = (timePercentage - previous.PercentOfLifetime) / (current.PercentOfLifetime - previous.PercentOfLifetime);
                    break;
                }
            }

            return Color.Lerp(color1, color2, amount);
        }

        private Vector2 GetSizeAtCurrentLifetime(float currentTime)
        {
            float timePercentage = currentTime / LifeTime;

            if (SizeAtTime.Count == 0)
                return new Vector2(10, 10);
 
            SizeAtTime first = SizeAtTime[0];
            if (timePercentage < first.PercentOfLifetime)
                return first.Size;

            SizeAtTime last = SizeAtTime[^1];
            if (timePercentage > last.PercentOfLifetime)
                return last.Size;

            Vector2 size1 = new Vector2(0, 0);
            Vector2 size2 = new Vector2(0, 0);
            float amount = 0f;
            for (int i = 0; i < SizeAtTime.Count; i++)
            {
                SizeAtTime current = SizeAtTime[i];
                if (timePercentage < current.PercentOfLifetime)
                {
                    SizeAtTime previous = SizeAtTime[i - 1];

                    size1 = previous.Size;
                    size2 = current.Size;
                    amount = (timePercentage - previous.PercentOfLifetime) / (current.PercentOfLifetime - previous.PercentOfLifetime);
                    break;
                }
            }

            return Vector2.Lerp(size1, size2, amount);
        }
    }
}
