namespace Emotion.Common
{
    public enum EngineStatus
    {
        /// <summary>
        /// The initial engine status.
        /// </summary>
        Initial = 0,

        /// <summary>
        /// The engine light setup was initiated.
        /// </summary>
        LightSetup = 1,

        /// <summary>
        /// The engine was setup.
        /// </summary>
        Setup = 2,

        /// <summary>
        /// The engine is running.
        /// </summary>
        Running = 3,

        /// <summary>
        /// The engine cannot recover from this state, and the application must restart.
        /// </summary>
        Stopped = 4
    }
}