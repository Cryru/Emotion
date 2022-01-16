#region Using

using System.Numerics;
using System.Runtime.InteropServices;
using Emotion.Utility;

#endregion

namespace Emotion.Game.Physics2D.Collision
{
    /// <summary>
    /// A manifold for two touching convex Shapes.
    /// The local point usage depends on the manifold type:
    /// - SeparationFunction.FaceA: the center of faceA
    /// - SeparationFunction.FaceB: the center of faceB
    /// Similarly the local normal usage:
    /// - SeparationFunction.FaceA: the normal on polygonA
    /// - SeparationFunction.FaceB: the normal on polygonB
    /// We store contacts in this way so that position correction can
    /// account for movement, which is critical for continuous physics.
    /// All contact scenarios must be expressed in one of these types.
    /// This structure is stored across time steps, so we keep it small.
    /// </summary>
    public struct Manifold
    {
        public Vector2 LocalNormal;
        public Vector2 LocalPoint;
        public int PointCount;
        public FixedArray2<ManifoldPoint> Points;
        public ManifoldType Type;
    }

    public enum ManifoldType : byte
    {
        FaceA,
        FaceB
    }

    /// <summary>
    /// A manifold point is a contact point belonging to a contact
    /// manifold. It holds details related to the geometry and dynamics
    /// of the contact points.
    /// The local point usage depends on the manifold type:
    /// -SeparationFunction.FaceA: the local center of cirlceB or the clip point of polygonB
    /// -SeparationFunction.FaceB: the clip point of polygonA
    /// This structure is stored across time steps, so we keep it small.
    /// Note: the impulses are used for internal caching and may not
    /// provide reliable contact forces, especially for high speed collisions.
    /// </summary>
    public struct ManifoldPoint
    {
        public ContactId Id;
        public Vector2 LocalPoint;
        public float NormalImpulse;
        public float TangentImpulse;
    }

    /// <summary>
    /// Contact ids to facilitate warm starting.
    /// Uniquely identifies a contact point between two Shapes
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct ContactId
    {
        /// <summary>The features that intersect to form the contact point</summary>
        [FieldOffset(0)] public ContactFeature ContactFeature;

        /// <summary>Used to quickly compare contact ids.</summary>
        [FieldOffset(0)] public uint Key;
    }

    /// <summary>
    /// The features that intersect to form the contact point This must be 4 bytes or less.
    /// </summary>
    public struct ContactFeature
    {
        /// <summary>Feature index on ShapeA</summary>
        public byte IndexA;

        /// <summary>Feature index on ShapeB</summary>
        public byte IndexB;

        /// <summary>The feature type on ShapeA</summary>
        public ContactFeatureType TypeA;

        /// <summary>The feature type on ShapeB</summary>
        public ContactFeatureType TypeB;
    }

    public enum ContactFeatureType : byte
    {
        Vertex = 0,
        Face = 1
    }

    /// <summary>
    /// Used for computing contact manifolds.
    /// </summary>
    public struct ClipVertex
    {
        public ContactId Id;
        public Vector2 V;
    }
}