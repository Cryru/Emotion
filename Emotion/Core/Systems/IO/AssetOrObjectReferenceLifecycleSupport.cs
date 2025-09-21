#nullable enable

using Emotion.Core.Utility.Coroutines;

namespace Emotion.Core.Systems.IO;

[DontSerialize]
public partial class AssetOrObjectReferenceLifecycleSupport<TAsset, TObject>
    where TAsset : Asset, IAssetContainingObject<TObject>, new()
{
    public Coroutine CurrentSetRoutine = Coroutine.CompletedRoutine;

    private AssetOrObjectReference<TAsset, TObject> _initialRef = AssetOrObjectReference<TAsset, TObject>.Invalid;
    private AssetOrObjectReference<TAsset, TObject> _currentRef = AssetOrObjectReference<TAsset, TObject>.Invalid;

    private Action<TObject?>? _onObjectChanged;

    public AssetOrObjectReferenceLifecycleSupport(AssetOrObjectReference<TAsset, TObject> initial, Action<TObject?>? onChanged)
    {
        _initialRef = initial;
        _onObjectChanged = onChanged;
    }

    public Coroutine? Init()
    {
        return Set(_initialRef);
    }

    public Coroutine? Set(AssetOrObjectReference<TAsset, TObject> objRef)
    {
        if (objRef.ReadyToUse() || !objRef.IsValid())
        {
            AssetOrObjectReference<TAsset, TObject> oldAsset = _currentRef;
            _currentRef = objRef;
            _onObjectChanged?.Invoke(objRef.GetObject());
            oldAsset.Cleanup();

            return null;
        }

        // Stop old
        if (!CurrentSetRoutine.Finished) CurrentSetRoutine.RequestStop();

        CurrentSetRoutine = Engine.CoroutineManager.StartCoroutine(SwapRoutine(this, objRef));
        return CurrentSetRoutine;
    }

    public void Done()
    {
        _currentRef.Cleanup();
    }

    private static IEnumerator SwapRoutine(AssetOrObjectReferenceLifecycleSupport<TAsset, TObject> owner, AssetOrObjectReference<TAsset, TObject> objRef)
    {
        yield return objRef.PerformLoading(owner, static (obj) =>
        {
            var component = (AssetOrObjectReferenceLifecycleSupport<TAsset, TObject>)obj;
            component._onObjectChanged?.Invoke(component._currentRef.GetObject());
        });

        AssetOrObjectReference<TAsset, TObject> oldRef = owner._currentRef;
        owner._currentRef = objRef;
        owner._onObjectChanged?.Invoke(objRef.GetObject());
        oldRef.Cleanup();
    }
}