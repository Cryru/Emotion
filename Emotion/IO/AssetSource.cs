﻿#region Using

#endregion

namespace Emotion.IO
{
    /// <summary>
    /// A source of assets.
    /// </summary>
    public abstract class AssetSource
    {
        /// <summary>
        /// Returns an asset by engine path.
        /// </summary>
        /// <param name="enginePath">The engine path to the asset.</param>
        /// <returns>A byte array of the asset.</returns>
        public abstract ReadOnlyMemory<byte> GetAsset(string enginePath);

        public abstract FileReadRoutineResult GetAssetRoutine(string enginePath);

        /// <summary>
        /// Returns the time the asset was last modified.
        /// </summary>
        /// <param name="enginePath">The engine path to the asset.</param>
        /// <returns>The time the asset was last modified.</returns>
        public virtual DateTime GetAssetModified(string enginePath)
        {
            return DateTime.Now;
        }

        /// <summary>
        /// Returns all assets this manifest manages.
        /// </summary>
        /// <returns>A list of engine paths to assets this source can provide.</returns>
        public abstract string[] GetManifest();

        #region ONE

        public virtual bool HasAsset(string enginePath)
        {
            return false;
        }

        #endregion
    }
}