#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Standard.XML;
using Emotion.Standard.XML.TypeHandlers;
using Emotion.Utility;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows
{
    public class MemoryViewer : ImGuiWindow
    {
        private Process _p;

        public MemoryViewer() : base("Memory")
        {
            _p = Process.GetCurrentProcess();
        }

        protected override void RenderContent(RenderComposer composer)
        {
            long usedRAM = _p.WorkingSet64;
            ImGui.Text($"Working Memory: {Helpers.FormatByteAmountAsString(usedRAM)}");
            ImGui.Text(UnmanagedMemoryAllocator.GetDebugInformation());

            ImGui.BeginChild("LoadedAssets", new Vector2(400, 400), true, ImGuiWindowFlags.HorizontalScrollbar);
            ImGui.Text("Loaded Assets: ");
            ImGui.Text(" ");
            Asset[] loadedAssets = Engine.AssetLoader.LoadedAssets;
            IOrderedEnumerable<Asset> orderedEnum = loadedAssets.OrderByDescending(x => x.Size);
            foreach (Asset asset in orderedEnum)
            {
                float percent = (float) asset.Size / usedRAM;
                ImGui.Text($"{asset.Name} {Helpers.FormatByteAmountAsString(asset.Size)} {percent * 100:0}%%");
                ImGui.Text($"\t{asset.GetType()}");
            }

            ImGui.EndChild();

            ImGui.SameLine();
            ImGui.BeginChild("XMLType", new Vector2(400, 400), true, ImGuiWindowFlags.HorizontalScrollbar);
            ImGui.Text("XML Cache: ");
            ImGui.Text(" ");
            LazyConcurrentDictionary<Type, XMLTypeHandler?> xmlCachedHandlers = XMLHelpers.Handlers;

            var counter = 0;
            foreach ((Type type, Lazy<XMLTypeHandler?> typeHandlerLazy) in xmlCachedHandlers)
            {
                if (!typeHandlerLazy.IsValueCreated) continue;

                counter++;
                ImGui.PushID(counter);

                XMLTypeHandler typeHandler = typeHandlerLazy.Value!;
                if (ImGui.TreeNode($"{typeHandler.TypeName} ({typeHandler.GetType().ToString().Replace("Emotion.Standard.XML.TypeHandlers.", "")})"))
                {
                    ImGui.Text($"\t Full TypeName: {type}");
                    if (typeHandler is XMLComplexTypeHandler complexHandler)
                        ImGui.Text($"\t Fields: {complexHandler.FieldCount()}, Recursive: {complexHandler.RecursiveType}");

                    ImGui.TreePop();
                }

                ImGui.PopID();
            }

            ImGui.EndChild();
        }

        public override void Update()
        {
        }
    }
}