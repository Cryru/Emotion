﻿using Emotion.Game.Time.Routines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

#nullable enable

namespace Emotion.IO;

public abstract class AssetHandleBase : IRoutineWaiter
{
    public string Name { get; init; }

    public AssetSource? AssetSource;

    public bool AssetExists => AssetSource != null;

    public bool AssetLoaded;

    protected AssetHandleBase(string name)
    {
        Name = name;
    }

    public abstract bool LoadAsset();

    #region Routine Waiter

    public bool Finished => !AssetExists || AssetLoaded;

    public void Update()
    {
        // nop
    }

    #endregion
}

public class AssetHandle<T> : AssetHandleBase where T : Asset, new()
{
    public static AssetHandle<T> Empty = new(string.Empty);

    public T? Asset;

    public event Action<T>? OnAssetLoaded;

    public AssetHandle(string name) : base(name)
    {
    }

    public override bool LoadAsset()
    {
        AssetSource? source = AssetSource;
        AssertNotNull(source);
        if (source == null) return true; // ???

        ReadOnlyMemory<byte> data;

        // Hot reload
        if (Asset != null)
        {
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

            if (Asset is not IHotReloadableAsset reloadableAsset) return true;

            reloadableAsset.Reload(data);
            Engine.Log.Info($"Reloaded asset '{Name}'", MessageSource.AssetLoader);
            Engine.CoroutineManager.StartCoroutine(ExecuteAssetLoadedEventsRoutine(Asset));
            return true;
        }

        data = source.GetAsset(Name);
        Asset = new T { Name = Name };
        Asset.Create(data);
        AssetLoaded = true;
        Engine.CoroutineManager.StartCoroutine(ExecuteAssetLoadedEventsRoutine(Asset));
        return true;
    }

    // Loaded event is executed as a coroutine as assets are loaded in another thread
    // and we don't want threading issues.
    private IEnumerator ExecuteAssetLoadedEventsRoutine(T asset)
    {
        OnAssetLoaded?.Invoke(asset);
        yield break;
    }
}