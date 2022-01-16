#region Using

using Emotion.Utility;

#endregion

namespace Emotion.Game.Physics2D
{
    public static class PhysicsConfig
    {
        /// <summary>
        /// A small length used as a collision and constraint tolerance.
        /// Usually it is chosen to be numerically significant, but visually insignificant.
        /// </summary>
        public static float LinearSlop = 0.005f;

        /// <summary>
        /// The maximum number of vertices on a convex polygon.
        /// </summary>
        public static int MaxPolygonVertices = 8;

        /// <summary>
        /// The number of velocity iterations to do.
        /// Less means more performance, but also more inaccurate position calculations.
        /// </summary>
        public static int VelocityIterations = 8;

        /// <summary>
        /// The number of position iterations to do.
        /// Less means more performance, but also more inaccurate position calculations.
        /// </summary>
        public static int PositionIterations = 3;

        /// <summary>
        /// A body cannot sleep if its linear velocity is above this tolerance.
        /// </summary>
        public static float LinearSleepTolerance = 0.01f;

        /// <summary>
        /// A body cannot sleep if its angular velocity is above this tolerance.
        /// </summary>
        public static float AngularSleepTolerance = 2.0f / 180.0f * Maths.PI;

        /// <summary>
        /// The time that a body must be still before it will go to sleep.
        /// </summary>
        public static float TimeToSleep = 0.5f;

        /// <summary>
        /// Maximum number of contacts to be handled to solve a TOI impact.
        /// </summary>
        public static int MaxTimeOfImpactContacts = 32;

        /// <summary>
        /// Maximum number of sub-steps per contact in continuous physics simulation.
        /// </summary>
        public static int MaxSubSteps = 8;

        /// <summary>
        /// This scale factor controls how fast overlap is resolved. Ideally this would be 1 so that overlap is removed in
        /// one time step. However using values close to 1 often lead to overshoot.
        /// </summary>
        public static float Baumgarte = 0.2f;

        public static float TimeOfImpactBaumgarte = 0.75f;

        /// <summary>
        /// The maximum linear position correction used when solving constraints. This helps to prevent overshoot.
        /// </summary>
        public static float MaxLinearCorrection = 0.2f;

        /// <summary>
        /// The maximum angular position correction used when solving constraints. This helps to prevent overshoot.
        /// </summary>
        public static float MaxAngularCorrection = 8.0f / 180.0f * Maths.PI;
    }
}