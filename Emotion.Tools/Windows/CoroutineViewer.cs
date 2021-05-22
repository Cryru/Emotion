#region Using

using System;
using Emotion.Common;
using Emotion.Game.Time.Routines;
using Emotion.Graphics;
using Emotion.Plugins.ImGuiNet.Windowing;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows
{
    public class CoroutineViewer : ImGuiWindow
    {
        public CoroutineViewer() : base("Coroutine Viewer")
        {
        }

        protected override void RenderContent(RenderComposer composer)
        {
            CoroutineManager manager = Engine.CoroutineManager;
            for (var i = 0; i < manager.Count; i++)
            {
                Coroutine routine = manager.DbgGetCoroutine(i);
                string creationStackStr = routine.DebugStackTrace;
                int stackLineBeforeStartCoroutine = creationStackStr.IndexOf("StartCoroutine");
                int firstLine = creationStackStr.IndexOf('\n', stackLineBeforeStartCoroutine) + 1;
                int firstLineEnd = creationStackStr.IndexOf('\n', firstLine);
                string cutOff = creationStackStr.Substring(firstLine, firstLineEnd - firstLine);

                ImGui.PushID(i);
                if (ImGui.TreeNode((i + 1) + ". " + cutOff))
                {
                    ImGui.TextWrapped(cutOff);
                    ImGui.TreePop();
                }

                ImGui.PopID();
            }
        }

        public override void Update()
        {
        }
    }
}