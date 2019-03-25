namespace SharpFont.TrueType
{
    /// <summary>
    /// A list of values describing which kind of TrueType bytecode engine is implemented in a given
    /// <see cref="Library" /> instance. It is used by the <see cref="Library.GetTrueTypeEngineType" /> function.
    /// </summary>
    public enum EngineType
    {
        /// <summary>
        /// The library doesn't implement any kind of bytecode interpreter.
        /// </summary>
        None = 0,

        /// <summary>
        ///     <para>
        ///     The library implements a bytecode interpreter that doesn't support the patented operations of the TrueType
        ///     virtual machine.
        ///     </para>
        ///     <para>
        ///     Its main use is to load certain Asian fonts which position and scale glyph components with bytecode
        ///     instructions. It produces bad output for most other fonts.
        ///     </para>
        /// </summary>
        Unpatented,

        /// <summary>
        /// The library implements a bytecode interpreter that covers the full instruction set of the TrueType virtual
        /// machine (this was governed by patents until May 2010, hence the name).
        /// </summary>
        Patented
    }
}