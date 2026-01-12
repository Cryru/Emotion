#nullable enable

using Emotion.Core.Utility.Coroutines;
using Emotion.Game.World.Components;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using static Emotion.Game.World.GameMap;

namespace Emotion.Game.World;

public enum GameObjectState
{
    Uninitialized,
    Initialized
}

public partial class GameObject
{
    public bool AlwaysRender;

    public const string DEFAULT_OBJECT_NAME = "Unnamed Object";

    /// <summary>
    /// User-friendly name of the object. Doesn't have to be unique.
    /// </summary>
    public string Name = DEFAULT_OBJECT_NAME;

    public GameMap? Map { get => _adapter?.Map; }

    [DontSerialize]
    public GameObjectState State { get; private set; } = GameObjectState.Uninitialized;

    private ObjectFriendAdapter? _adapter;

    /// <summary>
    /// Called by the map when the object is initialized.
    /// </summary>
    public IEnumerator InitRoutine(ObjectFriendAdapter adapter)
    {
        _adapter = adapter;

        Coroutine?[]? componentRoutines = null;
        int componentIdx = 0;
        foreach ((Type _, IGameObjectComponent component) in _components)
        {
            if (component is IGameObjectTransformProvider transformProvider)
                _transformProvider = transformProvider;

            Coroutine? coroutine = component.Init(this);
            if (coroutine != null)
            {
                componentRoutines ??= ArrayPool<Coroutine?>.Shared.Rent(_components.Count);
                componentRoutines[componentIdx] = coroutine;
            }
            componentIdx++;
        }

        // Wait for all routines
        if (componentRoutines != null)
        {
            for (int i = 0; i < _components.Count; i++)
            {
                Coroutine? routine = componentRoutines[i];
                if (routine != null)
                    yield return routine;
            }
            ArrayPool<Coroutine?>.Shared.Return(componentRoutines);
        }

        InternalOnInit();
        State = GameObjectState.Initialized;
    }

    protected virtual void InternalOnInit()
    {

    }

    /// <summary>
    /// Called by the map when the object is removed/destroyed
    /// todo: friend class adapter
    /// </summary>
    public virtual void Done()
    {
        foreach ((Type typ, IGameObjectComponent component) in _components)
        {
            component.Done(this);
        }
        _components.Clear();
    }

    public virtual void Update(float dt)
    {

    }

    public void RemoveFromMap()
    {
        if (Map == null) return;
        Map.RemoveObject(this);
    }

    #region Components

    private Dictionary<Type, IGameObjectComponent> _components = new Dictionary<Type, IGameObjectComponent>();

    public TComponent? GetComponent<TComponent>() where TComponent : class, IGameObjectComponent
    {
        if (_components.TryGetValue(typeof(TComponent), out IGameObjectComponent? val))
            return (TComponent)val;
        return null;
    }

    public bool GetComponent<TComponent>([NotNullWhen(true)] out TComponent? component) where TComponent : class, IGameObjectComponent
    {
        if (_components.TryGetValue(typeof(TComponent), out IGameObjectComponent? val))
        {
            component = (TComponent)val;
            return true;
        }

        component = null;
        return false;
    }

    public TComponent AddComponent<TComponent>(TComponent component, out Coroutine componentInitRoutine) where TComponent : class, IGameObjectComponent
    {
        componentInitRoutine = Coroutine.CompletedRoutine;

        if (_components.TryAdd(typeof(TComponent), component))
        {
            if (State == GameObjectState.Initialized) // Object is already initialized, we have to initialize the component now.
            {
                AssertNotNull(_adapter);
                _adapter.OnObjectComponentAdded<TComponent>(this);

                if (component is IGameObjectTransformProvider transformProvider)
                    _transformProvider = transformProvider;

                componentInitRoutine = component.Init(this) ?? Coroutine.CompletedRoutine;
            }
        }

        return component;
    }

    public TComponent AddComponent<TComponent>(TComponent component) where TComponent : class, IGameObjectComponent
    {
        return AddComponent(component, out _);
    }

    public bool RemoveComponent<TComponent>() where TComponent : class, IGameObjectComponent
    {
        if (_components.Remove(typeof(TComponent), out IGameObjectComponent? component))
        {
            component.Done(this);

            if (component == _transformProvider)
            {
                _transformProvider = null;
                InvalidateModelMatrix();
            }

            _adapter?.OnObjectComponentRemoved<TComponent>(this);
            return true;
        }

        return false;
    }

    public void ForEachComponentOfType<TComponent, TArg>(Action<TComponent, TArg> func, TArg arg1)
    {
        foreach (KeyValuePair<Type, IGameObjectComponent> component in _components)
        {
            IGameObjectComponent componentInstance = component.Value;
            if (componentInstance is TComponent asTComponent)
                func(asTComponent, arg1);
        }
    }

    #endregion

    #region Helper Constructors

    public static GameObject NewMeshObject(string entityFile)
    {
        var gameObject = new GameObject();
        gameObject.Name = entityFile;
        gameObject.AddComponent<MeshComponent>(new MeshComponent(entityFile));
        return gameObject;
    }

    public static GameObject NewSpriteObject(string entityFile)
    {
        var gameObject = new GameObject();
        gameObject.Name = entityFile;
        gameObject.AddComponent<SpriteComponent>(new SpriteComponent(entityFile));
        return gameObject;
    }

    public static GameObject CreateSkyBox(string textureFile)
    {
        var gameObject = new GameObject();
        gameObject.Name = "Skybox";
        gameObject.AlwaysRender = true;
        gameObject.AddComponent<SkyBoxComponent>(new SkyBoxComponent(textureFile));
        return gameObject;
    }

    #endregion
}
