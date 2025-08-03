#nullable enable

using Emotion.Game.Systems.Particles.ParticleShape;
using Emotion.Graphics.Assets;
using Emotion.Standard.DataStructures;

namespace Emotion.Game.Systems.Particles;

[DontSerialize]
public class ParticleSystem
{
    public Vector3 Position;

    public ColorAtTime[] ColorAtTime = Array.Empty<ColorAtTime>();
    public SizeAtTime[] SizeAtTime = Array.Empty<SizeAtTime>();
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
        Array.Sort(ColorAtTime, (x, y) => x.PercentOfLifetime > y.PercentOfLifetime ? 1 : -1);
        Array.Sort(SizeAtTime, (x, y) => x.PercentOfLifetime > y.PercentOfLifetime ? 1 : -1);
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

    public void Render(Renderer c)
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

        if (ColorAtTime.Length == 0)
            return new Color(255, 255, 255, 255);

        ref ColorAtTime first = ref ColorAtTime[0];
        if (timePercentage < first.PercentOfLifetime)
            return first.Color;

        ref ColorAtTime last = ref ColorAtTime[^1];
        if (timePercentage > last.PercentOfLifetime)
            return last.Color;

        Color color1 = new Color(255, 255, 255, 255);
        Color color2 = new Color(255, 255, 255, 255);
        float amount = 0f;
        for (int i = 0; i < ColorAtTime.Length; i++)
        {
            ref ColorAtTime current = ref ColorAtTime[i];
            if (timePercentage < current.PercentOfLifetime)
            {
                ref ColorAtTime previous = ref ColorAtTime[i - 1];
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

        if (SizeAtTime.Length == 0)
            return new Vector2(10, 10);

        ref SizeAtTime first = ref SizeAtTime[0];
        if (timePercentage < first.PercentOfLifetime)
            return first.Size;

        ref SizeAtTime last = ref SizeAtTime[^1];
        if (timePercentage > last.PercentOfLifetime)
            return last.Size;

        Vector2 size1 = new Vector2(0, 0);
        Vector2 size2 = new Vector2(0, 0);
        float amount = 0f;
        for (int i = 0; i < SizeAtTime.Length; i++)
        {
            ref SizeAtTime current = ref SizeAtTime[i];
            if (timePercentage < current.PercentOfLifetime)
            {
                ref SizeAtTime previous = ref SizeAtTime[i - 1];

                size1 = previous.Size;
                size2 = current.Size;
                amount = (timePercentage - previous.PercentOfLifetime) / (current.PercentOfLifetime - previous.PercentOfLifetime);
                break;
            }
        }

        return Vector2.Lerp(size1, size2, amount);
    }
}
