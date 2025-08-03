#nullable enable

using Emotion.Core.Platform.Implementation.CommonDesktop;

namespace Emotion.Core.Systems.Input;

public class InputManager
{
    /// <summary>
    /// Returns the current mouse position. Is preprocessed by the Renderer to scale to the window if possible.
    /// Therefore it is in window coordinates which change with the size of the Engine.Renderer.ScreenBuffer.
    /// </summary>
    public Vector2 MousePosition { get; private set; }

    /// <summary>
    /// Called when a key is pressed, let go, or a held event is triggered.
    /// </summary>
    public EmotionKeyEvent OnKey { get; } = new();

    /// <summary>
    /// Called when the mouse moves. The first vector is the old one, the second is the new position.
    /// </summary>
    public event Action<Vector2, Vector2>? OnMouseMove;

    private struct InputEvent
    {
        // todo: this would be cool to have as a union
        public Key Key;
        public KeyState State;
        public Vector2 MousePosition;
        public bool MouseMoved;
    }
    private const int MAX_EVENTS_PER_TICK = 256;

    private InputEvent[] _events = new InputEvent[MAX_EVENTS_PER_TICK];
    private int _eventsThisTick = 0;

    protected bool[] _keyState;

    public InputManager()
    {
        const int totalKeys = (int)Key.Last;
        _keyState = new bool[totalKeys];
    }

    public void ReportMouseMove(Vector2 movedTo)
    {
        if (_eventsThisTick == MAX_EVENTS_PER_TICK) return;

        movedTo = WindowPointToViewportPoint(movedTo);

        ref InputEvent nextEvent = ref _events[_eventsThisTick];
        nextEvent.MouseMoved = true;
        nextEvent.MousePosition = movedTo;
        _eventsThisTick++;
    }

    public void ReportKeyInput(Key key, KeyState state)
    {
        if (_eventsThisTick == MAX_EVENTS_PER_TICK) return;

        ref InputEvent nextEvent = ref _events[_eventsThisTick];
        nextEvent.MouseMoved = false;
        nextEvent.Key = key;
        nextEvent.State = state;
        _eventsThisTick++;
    }

    private void FlushEventsForTick()
    {
        for (int i = 0; i < _eventsThisTick; i++)
        {
            ref InputEvent ev = ref _events[i];

            if (ev.MouseMoved)
            {
                OnMouseMove?.Invoke(MousePosition, ev.MousePosition);
                MousePosition = ev.MousePosition;
            }
            else if (ev.Key == Key.MouseWheel)
            {
                OnKey.Invoke(ev.Key, ev.State);
            }
            else
            {
                Key key = ev.Key;
                var keyIndex = (short)key;
                if (keyIndex < 0 || keyIndex >= _keyState.Length)
                {
                    Engine.Log.Warning($"Got event for unknown key - {key}/{keyIndex}", MessageSource.Platform);
                    return;
                }

                bool down = ev.State == KeyState.Down;
                bool wasDown = _keyState[keyIndex];
                _keyState[keyIndex] = down;

                // Make sure we don't get double down/double up
                bool letGo = wasDown && !down;
                bool pressed = !wasDown && down;
                if (letGo || pressed)
                    OnKey.Invoke(ev.Key, ev.State);
            }
        }
        _eventsThisTick = 0;
    }

    private static Vector2 WindowPointToViewportPoint(Vector2 pos)
    {
        if (Engine.Renderer == null) return pos;

        FrameBuffer screenBuffer = Engine.Renderer.ScreenBuffer;
        FrameBuffer drawBuffer = Engine.Renderer.DrawBuffer;

        // Check for any viewport scaling
        if (screenBuffer.Viewport.Size == drawBuffer.Size && drawBuffer.Viewport.Size == screenBuffer.Size)
            return pos;

        // Get the difference in scale.
        float scaleX = screenBuffer.Viewport.Size.X / drawBuffer.Size.X;
        float scaleY = screenBuffer.Viewport.Size.Y / drawBuffer.Size.Y;

        // Calculate letterbox/pillarbox margins.
        float marginX = screenBuffer.Size.X / 2 - screenBuffer.Viewport.Size.X / 2;
        float marginY = screenBuffer.Size.Y / 2 - screenBuffer.Viewport.Size.Y / 2;

        return new Vector2((pos.X - marginX) / scaleX, (pos.Y - marginY) / scaleY);
    }

    public void Update()
    {
        FlushEventsForTick();
        UpdateFirstPersonMode();
    }

    #region First Person Mode

    private ReasonStack _firstPersonModeEnable = new ReasonStack();
    private ReasonStack _firstPersonModeDisable = new ReasonStack();

    public void SetMouseFirstPersonMode(bool on, string reason = "Default")
    {
        if (on)
            _firstPersonModeEnable.AddReason(reason);
        else
            _firstPersonModeEnable.RemoveReason(reason);
    }

    public void SuppressMouseFirstPersonMode(bool on, string reason = "Default")
    {
        if (on)
            _firstPersonModeDisable.AddReason(reason);
        else
            _firstPersonModeDisable.RemoveReason(reason);
    }

    public bool IsMouseFirstPersonMode()
    {
        if (!Engine.Host.IsFocused) return false;

        return !_firstPersonModeDisable.AnyReason() && _firstPersonModeEnable.AnyReason();
    }

    private void UpdateFirstPersonMode()
    {
        // todo: implement for other platforms
        DesktopPlatform? host = Engine.Host as DesktopPlatform;
        if (host == null) return;
        if (!host.IsFocused) return;

        if (!IsMouseFirstPersonMode())
        {
            host.SetHideCursor(false);
            return;
        }

        Vector2 center = host.Position + Engine.Renderer.ScreenBuffer.Viewport.Center;
        host.SetMousePos(center);
        host.SetHideCursor(true);
    }

    #endregion
}
