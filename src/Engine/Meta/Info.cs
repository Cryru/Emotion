namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Information about the engine.
    /// </summary>
    static class Info
    {
        /// <summary>
        /// The name of the engine.
        /// </summary>
        public const string Name = "SoulEngine";
        /// <summary>
        /// The version of the engine.
        /// </summary>
        public const string Version = "2017 5 dev 5/5/2017";
        /// <summary>
        /// The GUID of the application. Used on windows to prevent multi-instancing.
        /// The default SoulEngine GUID - 536F756C-456E-6769-6E65-203230313700
        /// </summary>
        public static string GUID = "536F756C-456E-6769-6E65-203230313700";

        /// <summary>
        /// Returns the engine information as a nicely formatted string.
        /// </summary>
        /// <returns></returns>
        public static string getInfo()
        {
            return "<color=#b642f4>" + Name + " " + Version + "</> {" + GUID + "}";
        }
    }
}
