#nullable enable

#region Using

using Emotion.Common.Serialization;
using Emotion.Game.Time.Routines;

#endregion

namespace Emotion.IO
{
    /// <summary>
    /// A handle to an asset.
    /// </summary>
    [DontSerialize]
    public abstract class Asset : IRoutineWaiter
    {
        /// <summary>
        /// The name of the asset. If loaded from the AssetLoader this is the path of the asset.
        /// </summary>
        public string Name { get; set; } = "Unknown";

        /// <summary>
        /// The byte size of the asset when loaded from the asset source.
        /// Subsequent operations may cause the asset to take more space/less space.
        /// Such as decompression etc.
        /// </summary>
        public int ByteSize { get; set; }

        /// <summary>
        /// Whether the asset is loaded. If this is false it means the
        /// AssetLoader is still processing it.
        /// </summary>
        public bool Loaded { get; protected set; }

        /// <summary>
        /// Whether the asset has been freed.
        /// </summary>
        public bool Disposed { get; protected set; }

        /// <summary>
        /// Fires when the asset is loaded or hot reloaded.
        /// </summary>
        public event Action<Asset>? OnLoaded;

        /// <summary>
        /// Called by the asset loader on the asset loading thread(s),
        /// which performs the IO and asset creation and/or hot reloading if already loaded.
        /// </summary>
        public bool AssetLoader_LoadAsset(AssetSource source)
        {
            if (source == null) return true; // ???

            ReadOnlyMemory<byte> data;

            // Due to sharing violations we should try to hot reload this in a try-catch.
            Engine.SuppressLogExceptions(true);
            try
            {
                data = source.GetAsset(Name);
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                Engine.SuppressLogExceptions(false);
            }
            ByteSize = data.Length;

            // Hot reload
            if (Loaded)
            {
                ReloadInternal(data);
                Engine.Log.Info($"Reloaded asset '{Name}'", MessageSource.AssetLoader);
                Engine.CoroutineManager.StartCoroutine(ExecuteAssetLoadedEventsRoutine());
                return true;
            }

            CreateInternal(data);
            Loaded = true;
            Engine.CoroutineManager.StartCoroutine(ExecuteAssetLoadedEventsRoutine());
            return true;
        }

        public void AssetLoader_CreateLegacy(ReadOnlyMemory<byte> data)
        {
            Loaded = true;
            CreateInternal(data);
            Engine.CoroutineManager.StartCoroutine(ExecuteAssetLoadedEventsRoutine());
        }

        // Loaded event is executed as a coroutine as assets are loaded in another thread
        // and we don't want threading issues.
        private IEnumerator ExecuteAssetLoadedEventsRoutine()
        {
            OnLoaded?.Invoke(this);
            yield break;
        }

        protected abstract void CreateInternal(ReadOnlyMemory<byte> data);

        protected virtual void ReloadInternal(ReadOnlyMemory<byte> data)
        {

        }

        /// <summary>
        /// Dispose of the asset clearing any external resources it used.
        /// </summary>
        public void Dispose()
        {
            if (Disposed) return;
            DisposeInternal();
            Disposed = true;
        }

        protected abstract void DisposeInternal();

        /// <summary>
        /// The hashcode of the asset. Derived from the name.
        /// </summary>
        /// <returns>The hashcode of the asset.</returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        #region Routine Waiter

        public bool Finished => Loaded || !Engine.AssetLoader.IsAssetQueuedForLoading(this); // if not queued we consider it non-existing at this point

        public void Update()
        {
            // nop
        }

        #endregion

        #region Dependencies

        /// <summary>
        /// Called by the AssetLoader and intended to be overridden.
        /// Call Get to load dependencies here.
        /// </summary>
        public virtual void AssetLoader_LoadDependencyAssets()
        {

        }

        private List<Asset>? _dependencies;

        public void AssetLoader_AttachDependency(Asset dependantAsset)
        {
            _dependencies ??= new List<Asset>();
            _dependencies.Add(dependantAsset);
            Engine.AssetLoader.AddReferenceToAsset(dependantAsset, this);
        }

        public bool AssetLoader_AllDependenciesLoaded()
        {
            if (_dependencies == null) return true;
            for (int i = 0; i < _dependencies.Count; i++)
            {
                Asset dependant = _dependencies[i];
                if (!dependant.Loaded) return false;
            }

            return true;
        }

        #endregion
    }
}