#region Using

using System.Collections.Generic;
using System.Numerics;
using Emotion.Game;
using Emotion.Graphics;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Primitives;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows
{
    public class CollisionViewer : ImGuiWindow
    {
        public int SelectedCollision = -1;

        public CollisionViewer() : base("Collision Viewer")
        {
        }

        public override void Update()
        {
        }

        protected override void RenderContent(RenderComposer composer)
        {
            List<Collision.CollisionDebugData> lastCollision = Collision.LastCollision;
            if (SelectedCollision > lastCollision.Count - 1 || SelectedCollision < 0) SelectedCollision = 0;

            composer.SetUseViewMatrix(true);

            int bestWeightInClusterIdx = -1;
            var bestWeight = float.MinValue;

            for (var i = 0; i < lastCollision.Count; i++)
            {
                Collision.CollisionDebugData current = lastCollision[i];
                if (current == null)
                {
                    if (bestWeightInClusterIdx != -1) ImGui.Text($"    Best: {bestWeightInClusterIdx}");
                    bestWeightInClusterIdx = -1;
                    bestWeight = int.MinValue;

                    ImGui.NewLine();
                    continue;
                }

                if (current.Weight > bestWeight)
                {
                    bestWeightInClusterIdx = i;
                    bestWeight = current.Weight;
                }

                if (SelectedCollision == i)
                {
                    ImGui.Text($"Collision {i}");

                    composer.RenderLine(ref current.Line, Color.Red);
                    Vector2 middleOfLine = current.Line.PointOnLineAtDistance(current.Line.Length() / 2);
                    composer.RenderLine(current.Line.Start, current.Line.Start + current.LineNormal * 10f, Color.Yellow);
                    composer.RenderLine(middleOfLine, middleOfLine + current.LineNormal * 10f, Color.Yellow);
                    composer.RenderLine(current.Line.End, current.Line.End + current.LineNormal * 10f, Color.Yellow);
                }
                else
                {
                    if (ImGui.Button($"Collision  {i}")) SelectedCollision = i;
                }
            }

            if (bestWeightInClusterIdx != -1) ImGui.Text($"    Best: {bestWeightInClusterIdx}");

            composer.SetUseViewMatrix(false);

            if (lastCollision.Count == 0) return;
            Collision.CollisionDebugData selected = lastCollision[SelectedCollision];
            if (selected == null) return;

            ImGui.NewLine();
            ImGui.Text($"Surface {selected.Line.GetHashCode()} {selected.Line.Start}-{selected.Line.End}");
            ImGui.Text($"Weight {selected.Weight}");
            ImGui.Text($"Movement {selected.Movement}");
            ImGui.Text(selected.CollisionType);
        }
    }
}