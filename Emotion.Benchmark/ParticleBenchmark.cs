using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Emotion.Common;
using Emotion.Game.Particles;
using Emotion.Game.Particles.ParticleShape;
using Emotion.IO;
using Emotion.Primitives;

namespace Emotion.Benchmark;

[MemoryDiagnoser]
public class ParticleBenchmark
{
    ParticleSystem FireParticleSystem = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        Circle circle = new Circle(new Vector2(0, 0), 200f);

        FireParticleSystem = new ParticleSystem();
        FireParticleSystem.ColorAtTime = [
            new ColorAtTime(0f, new Color(255, 255, 255, 0)),
            new ColorAtTime(0.13f, new Color("FFD563").SetAlpha(125)),
            new ColorAtTime(0.22f, new Color("FFD563")),
            new ColorAtTime(0.31f, new Color("FF5D15")),
            new ColorAtTime(0.9f, new Color("FF1A00").SetAlpha(0))
        ];

        FireParticleSystem.SizeAtTime = [
            new SizeAtTime(0f, new Vector2(0, 0)),
            new SizeAtTime(0.1f, new Vector2(10, 10)),
            new SizeAtTime(0.7f, new Vector2(20, 20)),
            new SizeAtTime(0.9f, new Vector2(40, 40))
        ];

        FireParticleSystem.Speed = 50;
        FireParticleSystem.Periodicity = 1;
        FireParticleSystem.LifeTime = 5000;
        FireParticleSystem.SpawnShape = new Circle(new Vector2(0, 0), 5);
        FireParticleSystem.DirectionShape = new ParticleCircleShape(circle);
        FireParticleSystem.Init();
    }

    [Benchmark]
    public void BenchmarkLotsOfUpdates()
    {
        for (int i = 0; i < 10; i++)
        {
            FireParticleSystem.Update(16);
        }
    }

}
