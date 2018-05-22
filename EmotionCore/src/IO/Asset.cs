namespace Emotion.IO
{
    public abstract class Asset
    {
        #region Properties

        /// <summary>
        /// The name and path of the asset.
        /// </summary>
        public string AssetName { get; internal set; } = "Non Managed Asset";

        /// <summary>
        /// The state the asset is in.
        /// </summary>
        public AssetState State { get; protected set; } = AssetState.Uninitialized;

        #endregion

        internal virtual void Process(byte[] data)
        {
            State = AssetState.Ready;
        }

        internal virtual void Destroy()
        {
            State = AssetState.Destroyed;
        }
    }

    public enum AssetState
    {
        Uninitialized,
        Ready,
        Destroyed
    }
}
