#region Using

using SharpFont.Internal;

#endregion

namespace SharpFont
{
    /// <summary>
    /// The data exchange structure for the increase-x-height property.
    /// </summary>
    public class IncreaseXHeightProperty
    {
        private IncreaseXHeightPropertyRec rec;
        private Face face;

        /// <summary>
        /// Initializes a new instance of the <see cref="IncreaseXHeightProperty" /> class.
        /// </summary>
        /// <param name="face">The face to increase the X height of.</param>
        public IncreaseXHeightProperty(Face face)
        {
            rec.face = face.Reference;
            this.face = face;
        }

        internal IncreaseXHeightProperty(IncreaseXHeightPropertyRec rec, Face face)
        {
            this.rec = rec;
            this.face = face;
        }

        /// <summary>
        /// Gets or sets the associated face.
        /// </summary>
        public Face Face
        {
            get => face;

            set
            {
                face = value;
                rec.face = face.Reference;
            }
        }

        /// <summary>
        /// Gets or sets the limit property.
        /// </summary>

        public uint Limit
        {
            get => rec.limit;

            set => rec.limit = value;
        }

        internal IncreaseXHeightPropertyRec Rec
        {
            get => rec;
        }
    }
}