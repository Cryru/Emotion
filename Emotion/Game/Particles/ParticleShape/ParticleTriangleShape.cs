using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emotion.Game.Particles;
using Emotion.Utility;

namespace Emotion.Game.Particles.ParticleShape;

public class ParticleTriangleShape : IParticleDirectionShape
{
    public bool FromApex = true;

    private Triangle _triangle;

    public ParticleTriangleShape(Triangle triangle)
    {
        _triangle = triangle;
    }

    public void SetParticleDirection(Particle particle)
    {
        Vector3 pointOnLine = _triangle.Base.PointOnLineAtDistance(_triangle.Base.Length() * Helpers.GenerateRandomFloat()).ToVec3();
        particle.TargetDirection = Vector3.Normalize(pointOnLine - particle.Position);
    }
}
