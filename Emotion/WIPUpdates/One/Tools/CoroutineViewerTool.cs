using Emotion.Game.Time.Routines;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;
using System.Collections;

#nullable enable

namespace Emotion.WIPUpdates.One.Tools;

public class CoroutineViewerTool : EditorWindow
{
    private UIList _list = null!;
    private HashSet<Coroutine> _coroutinesShown = new();

    public CoroutineViewerTool() : base("Coroutines")
    {
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        UIBaseWindow contentParent = GetContentParent();

        _list = new UIList()
        {
            LayoutMode = LayoutMode.VerticalList,
            ListSpacing = new Vector2(0, 5)
        };
        contentParent.AddChild(_list);

        Engine.CoroutineManager.StartCoroutine(Testche());
        UpdateCoroutines();
    }

    protected override bool UpdateInternal()
    {
        UpdateCoroutines();
        return base.UpdateInternal();
    }

    public IEnumerator Testche()
    {
        while(true)
        {
            yield return null;
        }
    }

    public void UpdateCoroutines()
    {
        List<Coroutine>? newRoutines = null;

        var coroutines = Engine.CoroutineManager.DbgGetRunningRoutines();
        foreach (Coroutine coroutine in coroutines)
        {
            if (!_coroutinesShown.Contains(coroutine))
            {
                newRoutines ??= new List<Coroutine>();
                newRoutines.Add(coroutine);
            }
        }

        var asyncCoroutines = Engine.CoroutineManagerAsync.DbgGetRunningRoutines();
        foreach (Coroutine coroutine in asyncCoroutines)
        {
            if (!_coroutinesShown.Contains(coroutine))
            {
                newRoutines ??= new List<Coroutine>();
                newRoutines.Add(coroutine);
            }
        }

        if (newRoutines == null) return;

        for (int i = 0; i < newRoutines.Count; i++)
        {
            Coroutine newRoutine = newRoutines[i];
            _coroutinesShown.Add(newRoutine);
            _list.AddChild(new CoroutineViewerEntry(newRoutine));
        }
    }

    private class CoroutineViewerEntry : EditorButton
    {
        public Coroutine Routine;

        public CoroutineViewerEntry(Coroutine routine)
        {
            Routine = routine;
            Text = ExtractLabelFromStack();
        }

        public string ExtractLabelFromStack()
        {
            string creationStack = Routine.DebugCoroutineCreationStack ?? "No stack available";

            ReadOnlySpan<char> asSpan = creationStack.AsSpan();
            int firstNewLine = creationStack.IndexOf('\n');
            if (firstNewLine != -1)
            {
                asSpan = asSpan.Slice(0, firstNewLine);
            }

            // Get last file path slash "\"
            ReadOnlySpan<char> fileStartedIn;
            int lastSlash = asSpan.LastIndexOf('\\');
            if (lastSlash != -1)
            {
                lastSlash++;
                fileStartedIn = asSpan.Slice(lastSlash);
            }
            else
            {
                fileStartedIn = "Unknown File";
            }

            int functionStartedInStart = asSpan.IndexOf("at ");
            int functionStartedInEnd = asSpan.IndexOf(" in");
            if (functionStartedInEnd == -1) functionStartedInEnd = asSpan.Length - 1;

            ReadOnlySpan<char> functionStartedIn = new();
            if (functionStartedInStart != -1)
            {
                functionStartedInStart += 3;
                functionStartedIn = asSpan.Slice(functionStartedInStart, functionStartedInEnd - functionStartedInStart);
            }

            if (functionStartedIn.Length != 0) return $"{fileStartedIn}\n{functionStartedIn}";
            return asSpan.ToString();
        }
    }
}
