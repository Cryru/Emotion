#region Using

using System.Numerics;
using ImGuiNET;

#endregion

namespace Rationale.Windows
{
    public abstract class Window
    {
        public bool Open
        {
            get => _open;
        }

        public string Title { get; set; }

        private bool _open = true;
        private bool _setStartingSize;
        private Vector2? _startingSize;
        private ImGuiWindowFlags _flags;

        protected Window(string title, Vector2? startingSize = null, ImGuiWindowFlags flags = ImGuiWindowFlags.None)
        {
            Title = title;
            _startingSize = startingSize;
            _flags = flags;
        }

        public void Draw()
        {
            if (!_setStartingSize)
            {
                if (_startingSize != null) ImGui.SetNextWindowSize((Vector2) _startingSize);

                _setStartingSize = true;
            }

            ImGui.Begin(Title, ref _open, _flags);
            DrawContent();
            ImGui.End();
        }

        protected abstract void DrawContent();

        public void Reopen()
        {
            _open = true;
        }

        public virtual void Close()
        {
            _open = false;
        }
    }
}