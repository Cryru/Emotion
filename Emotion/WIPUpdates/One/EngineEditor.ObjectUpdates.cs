#nullable enable

namespace Emotion.WIPUpdates.One;

public partial class EngineEditor
{
    private struct ObjectChangeMonitor
    {
        public object? ListeningObject;
        public object ObjectListeningTo;
        public Action OnChange;
    }

    private static Dictionary<object, List<ObjectChangeMonitor>> _objModificationTracker = new Dictionary<object, List<ObjectChangeMonitor>>();

    public static void ObjectChanged(object obj, object? changedByListener = null)
    {
        if (!_objModificationTracker.TryGetValue(obj, out List<ObjectChangeMonitor>? list)) return;

        for (int i = 0; i < list.Count; i++)
        {
            ObjectChangeMonitor monitorItem = list[i];
            if (changedByListener != null && monitorItem.ListeningObject == changedByListener) continue;
            monitorItem.OnChange();
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

    public static void RegisterForObjectChanges(object obj, Action onChanged, object? listener = null)
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
}
