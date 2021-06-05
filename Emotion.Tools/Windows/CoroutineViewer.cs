#region Using

using System.Collections.Generic;
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
            List<Coroutine> routines = manager.DbgGetRunningRoutines();
            for (var i = 0; i < routines.Count; i++)
            {
                Coroutine routine = routines[i];
                ImGui.PushID(i);
                RenderCoroutineData(routine, $"{i + 1}.");
                ImGui.PopID();
            }
        }

        private static void RenderCoroutineData(Coroutine r, string header = "")
        {
            string creationStackStr = r.DebugCoroutineCreationStack;
            int firstLineEnd = creationStackStr.IndexOf('\n');
            string cutOff = firstLineEnd == -1 ? creationStackStr : creationStackStr.Substring(0, firstLineEnd);

            ImGui.PushID(header);
            if (ImGui.TreeNode($"{header} {cutOff}"))
            {
                string afterCutOff = firstLineEnd == -1 ? "" : creationStackStr.Substring(firstLineEnd + 1);
                if (afterCutOff != "") ImGui.TextWrapped(afterCutOff);

                if (r.CurrentWaiter is Coroutine nested)
                    RenderCoroutineData(nested, "Subroutine");
                else
                    ImGui.Text($"Waiting On: {(r.CurrentWaiter == null ? "self" : r.CurrentWaiter.ToString())}");
                ImGui.TreePop();
            }

            ImGui.PopID();
        }

        public override void Update()
        {
        }
    }
}