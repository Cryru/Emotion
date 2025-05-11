#nullable enable

#region Using

using Emotion.Common.Serialization;
using Emotion.Game.Time.Routines;
using System.Xml.Linq;

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
        /// The name of the asset.
        /// If loaded from the AssetLoader this is the engine path of the asset.
        /// </summary>
        public string Name { get; set; } = "Unknown";

        /// <summary>
        /// Whether this asset was loaded as a dependency. It could still be a
        /// dependency if it was loaded in another way first.
        /// </summary>
        public bool LoadedAsDependency { get; set; }

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

        protected bool _useNewLoading = false;

        /// <summary>
        /// Called by the asset loader on the asset loading thread(s),
        /// which performs the IO and asset creation and/or hot reloading if already loaded.
        /// </summary>
        public IEnumerator AssetLoader_LoadAsset()
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            // Asset not found in any source.
            AssetSource? source = Engine.AssetLoader.GetSource(Name);
            if (source == null)
            {
                Engine.Log.Warning($"Tried to load asset {Name} which doesn't exist in any loaded source.", MessageSource.AssetLoader, true);
                yield break;
            }

            ReadOnlyMemory<byte> data = ReadOnlyMemory<byte>.Empty;

            // Due to sharing violations we should try to hot reload this in a try-catch.
            //Engine.SuppressLogExceptions(true);
            int attempts = 0;
            while (attempts < 10)
            {
                FileReadRoutineResult fileRead = source.GetAssetRoutine(Name);
                yield return fileRead;

                if (fileRead.Finished && !fileRead.Errored)
                {
                    data = fileRead.FileBytes;
                    break;
                }

                //try
                //{
                //    data = source.GetAsset(Name);
                //    break;
                //}
                //catch (Exception)
                //{
                //    // Error, try again :(
                //}
                //finally
                //{
                //    Engine.SuppressLogExceptions(false);
                //}

                attempts++;
            }
           
            ByteSize = data.Length;

            if (_useNewLoading)
            {
                yield return Internal_LoadAssetRoutine(data);
                if (!LoadedAsDependency)
                {
                    if (Loaded)
                        Engine.Log.Info($"Reloaded asset '{Name}'{(_dependencies == null ? "" : $" ({_dependencies.Count} Dependencies)")}", MessageSource.AssetLoader);
                    else
                        Engine.Log.Info($"Loaded asset '{Name}'{(_dependencies == null ? "" : $" ({_dependencies.Count} Dependencies)")}", MessageSource.AssetLoader);
                }
                Loaded = true;
                if (OnLoaded != null)
                    Engine.CoroutineManager.StartCoroutine(ExecuteAssetLoadedEventsRoutine());

                Engine.Log.Trace($"Loaded in {timer.ElapsedMilliseconds}", "Profiler");
            }
            else
            {
                // Hot reload
                if (Loaded)
                {
                    ReloadInternal(data);
                    if (!LoadedAsDependency) Engine.Log.Info($"Reloaded asset '{Name}'", MessageSource.AssetLoader);
                    if (OnLoaded != null)
                        Engine.CoroutineManager.StartCoroutine(ExecuteAssetLoadedEventsRoutine());
                }
                else
                {
                    CreateInternal(data);
                    if (!LoadedAsDependency) Engine.Log.Info($"Loaded asset '{Name}'", MessageSource.AssetLoader);
                    Loaded = true;
                    if (OnLoaded != null)
                        Engine.CoroutineManager.StartCoroutine(ExecuteAssetLoadedEventsRoutine());
                }
            }
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

        protected virtual IEnumerator Internal_LoadAssetRoutine(ReadOnlyMemory<byte> data)
        {
            yield break;
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

        #region File Extension Support

        public static string[] GetFileExtensionsSupported<T>() where T : Asset
        {
            var typ = typeof(T);
            if (_assetTypeToExtensions.TryGetValue(typ, out string[]? extensions))
                return extensions;

            return Array.Empty<string>();
        }

        protected static Dictionary<Type, string[]> _assetTypeToExtensions = new();

        protected static void RegisterFileExtensionSupport<T>(string[] extensions) where T : Asset
        {
            _assetTypeToExtensions.Add(typeof(T), extensions);
        }

        #endregion

        #region Routine Waiter

        public bool Finished => Loaded;

        public void Update()
        {
            // nop
        }

        #endregion

        #region Dependencies

        private List<Asset>? _dependencies;

        protected T LoadAssetDependency<T>(string name) where T : Asset, new()
        {
            T dependantAsset = Engine.AssetLoader.ONE_Get<T>(name, this, false, true);

            _dependencies ??= new List<Asset>();
            _dependencies.Add(dependantAsset);

            return dependantAsset;
        }

        protected IEnumerator WaitAllDependenciesToLoad()
        {
            if (_dependencies == null) yield break;

            while (true)
            {
                bool anyLoading = false;
                for (int i = 0; i < _dependencies.Count; i++)
                {
                    Asset dependent = _dependencies[i];
                    if (!dependent.Loaded)
                    {
                        anyLoading = true;
                        yield return null;
                    }
                }

                if (!anyLoading)
                    break;
            }
        }

        #endregion
    }
}