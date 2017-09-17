// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using Soul.Engine.AssetManager;
using Soul.Engine.ECS;

#endregion

namespace Soul.Engine
{
    /// <summary>
    /// A game scene.
    /// </summary>
    public abstract class Scene : Actor, IDisposable
    {
        /// <summary>
        /// Whether the scene has physics.
        /// </summary>
        public bool HasPhysics = false;

        #region Disposing
        /// <summary>
        /// Disposing flag to detect redundant calls.
        /// </summary>
        private bool _disposedValue;
        protected void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                // Set disposing flag.
                _disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}