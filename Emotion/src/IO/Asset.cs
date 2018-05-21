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

        protected object _processedData;

        internal virtual void Process(byte[] data)
        {
            State = AssetState.Processing;
        }

        internal virtual void ProcessNative()
        {
            _processedData = null;
            State = AssetState.Processed;
        }

        internal virtual void Destroy()
        {
            State = AssetState.Destroying;
        }

        internal virtual void DestroyNative()
        {
            State = AssetState.Destroyed;
        }
    }

    public enum AssetState
    {
        Uninitialized,
        Processing,
        Processed,
        Destroying,
        Destroyed
    }
}
