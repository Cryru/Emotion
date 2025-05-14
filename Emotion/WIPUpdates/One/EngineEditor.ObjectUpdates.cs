#nullable enable

namespace Emotion.WIPUpdates.One;

public enum ObjectChangeType
{
    ValueChanged, // Most generic, might not be needed
    ComplexObject_PropertyChanged, // Complex object property has changed. todo: property name
    List_NewObj, // A new item has been added to the list. todo: pass the item/index (currently it's always the last item)
    List_ObjectRemoved, // An item has been removed from the list. todo: pass the item
    List_Reodered, // The list has been reordered.
    List_ObjectValueChanged // The list itself has changed, as in one of the object references/values has changed. todo: index
}

public partial class EngineEditor
{
    private struct ObjectChangeMonitor
    {
        public object? ListeningObject;
        public object ObjectListeningTo;
        public Action<ObjectChangeType> OnChange;
    }

    private static Dictionary<object, List<ObjectChangeMonitor>> _objModificationTracker = new Dictionary<object, List<ObjectChangeMonitor>>();

    public static void ObjectChanged(object obj, ObjectChangeType changeType, object? changedByListener = null)
    {
        if (!_objModificationTracker.TryGetValue(obj, out List<ObjectChangeMonitor>? list)) return;

        for (int i = 0; i < list.Count; i++)
        {
            ObjectChangeMonitor monitorItem = list[i];
            if (changedByListener != null && monitorItem.ListeningObject == changedByListener) continue;
            monitorItem.OnChange(changeType);
        }
    }

    public static void UnregisterForObjectChanges(object listener)
    {
        bool compact = false;
        foreach (KeyValuePair<object, List<ObjectChangeMonitor>> pair in _objModificationTracker)
        {
            List<ObjectChangeMonitor> list = pair.Value;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var item = list[i];
                if (item.ListeningObject == listener)
                    list.RemoveAt(i);
            }

            if (list.Count == 0) compact = true;
        }

        if (compact)
            _objModificationTracker.RemoveAll((_, val) => val.Count == 0);
    }

    public static void RegisterForObjectChanges(object obj, Action<ObjectChangeType> onChanged, object? listener = null)
    {
        ObjectChangeMonitor listenInstance = new ObjectChangeMonitor()
        {
            ListeningObject = listener,
            ObjectListeningTo = obj,
            OnChange = onChanged
        };

        if (!_objModificationTracker.TryGetValue(obj, out List<ObjectChangeMonitor>? list))
        {
            list = new List<ObjectChangeMonitor>();
            _objModificationTracker.Add(obj, list);
        }

        list.Add(listenInstance);
    }

    public static void RegisterForObjectChangesList(IEnumerable obj, Action<ObjectChangeType> onChanged, object? listener = null)
    {
        foreach (object? item in obj)
        {
            RegisterForObjectChanges(item, onChanged, listener);
        }
    }
}
