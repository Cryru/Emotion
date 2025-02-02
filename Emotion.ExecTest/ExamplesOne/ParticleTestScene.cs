using System.Collections;
using System.Numerics;
using Emotion.Common;
using Emotion.Game.Particles;
using Emotion.Game.Particles.ParticleShape;
using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.Scenography;

namespace Emotion.ExecTest.ExamplesOne;

public class ParticlesTestScene : SceneWithMap
{
    public ParticleSystem ParticleSystem = null!;
    public ParticleSystem FireParticleSystem = null!;
    public Triangle triangle = new Triangle(new Vector3(0, 0, 0), new Vector3(-15, -100, 0), new Vector3(15, -100, 0));
    public Circle circle = new Circle(new Vector2(0, 0), 200f);

    protected override IEnumerator InternalLoadSceneRoutineAsync()
    {
        //ParticleSystem = new ParticleSystem();
        //ParticleSystem.ColorAtTime.Add(new ColorAtTime(0f, new Color(255, 255, 255, 0)));
        //ParticleSystem.ColorAtTime.Add(new ColorAtTime(0.2f, new Color(255, 255, 255, 255)));
        //ParticleSystem.ColorAtTime.Add(new ColorAtTime(0.8f, new Color(255, 255, 255, 255)));
        //ParticleSystem.ColorAtTime.Add(new ColorAtTime(1f, new Color(255, 255, 255, 0)));
        //ParticleSystem.Init();

        //FireParticleSystem = new ParticleSystem();
        //FireParticleSystem.ColorAtTime.Add(new ColorAtTime(0f, new Color(255, 255, 255, 0)));
        //FireParticleSystem.ColorAtTime.Add(new ColorAtTime(0.13f, new Color("FFD563").SetAlpha(125)));
        //FireParticleSystem.ColorAtTime.Add(new ColorAtTime(0.22f, new Color("FFD563")));
        //FireParticleSystem.ColorAtTime.Add(new ColorAtTime(0.31f, new Color("FF5D15")));
        //FireParticleSystem.ColorAtTime.Add(new ColorAtTime(0.9f, new Color("FF1A00").SetAlpha(0)));
        //FireParticleSystem.Speed = 50;
        //FireParticleSystem.Periodicity = 30;
        //FireParticleSystem.LifeTime = 5000;
        //FireParticleSystem.SpawnShape = new Circle(new Vector2(0, 0), 5);
        //FireParticleSystem.DirectionShape = new ParticleTriangleShape(triangle);
        //FireParticleSystem.Init();

        FireParticleSystem = new ParticleSystem();
        FireParticleSystem.ColorAtTime.Add(new ColorAtTime(0f, new Color(255, 255, 255, 0)));
        FireParticleSystem.ColorAtTime.Add(new ColorAtTime(0.13f, new Color("FFD563").SetAlpha(125)));
        FireParticleSystem.ColorAtTime.Add(new ColorAtTime(0.22f, new Color("FFD563")));
        FireParticleSystem.ColorAtTime.Add(new ColorAtTime(0.31f, new Color("FF5D15")));
        FireParticleSystem.ColorAtTime.Add(new ColorAtTime(0.9f, new Color("FF1A00").SetAlpha(0)));
        FireParticleSystem.SizeAtTime.Add(new SizeAtTime(0f, new Vector2(0, 0)));
        FireParticleSystem.SizeAtTime.Add(new SizeAtTime(0.1f, new Vector2(10, 10)));
        FireParticleSystem.SizeAtTime.Add(new SizeAtTime(0.7f, new Vector2(20, 20)));
        FireParticleSystem.SizeAtTime.Add(new SizeAtTime(0.9f, new Vector2(40, 40)));
        FireParticleSystem.Speed = 50;
        FireParticleSystem.Periodicity = 30;
        FireParticleSystem.LifeTime = 5000;
        FireParticleSystem.SpawnShape = new Circle(new Vector2(0, 0), 5);
        FireParticleSystem.DirectionShape = new ParticleCircleShape(circle);
        FireParticleSystem.Init();

        yield break;
    }

    public override void UpdateScene(float dt)
    {
        base.UpdateScene(dt);

        //ParticleSystem.Update(dt);
        FireParticleSystem.Update(dt);

        FireParticleSystem.Position = Engine.Renderer.Camera.ScreenToWorld(Engine.Host.MousePosition);
    }

    public override void RenderScene(RenderComposer c)
    {
        base.RenderScene(c);

        //ParticleSystem.Render(c);
        FireParticleSystem.Render(c);

        triangle.RenderOutline(c);
        c.RenderCircleOutline(circle, Color.PrettyYellow, 1, 30);
    }
}
