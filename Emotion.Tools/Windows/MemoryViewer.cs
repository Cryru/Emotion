#region Using

using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Emotion.Audio;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.Graphics.Objects;
using Emotion.IO;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Standard.XML;
using Emotion.Standard.XML.TypeHandlers;
using Emotion.Utility;
using ImGuiNET;
using OpenGL;

#endregion

#nullable enable

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
            ImGui.Text(UnmanagedMemoryAllocator.GetDebugInformation());

            var assetBytes = 0;
            foreach (Asset asset in Engine.AssetLoader.LoadedAssets)
            {
                assetBytes += asset.Size;
            }

            ImGui.Text($"Assets Rough Estimate: {Helpers.FormatByteAmountAsString(assetBytes)}");
            var totalBytes = 0;
            for (var i = 0; i < Texture.AllTextures.Count; i++)
            {
                Texture texture = Texture.AllTextures[i];
                int byteSize = (int) (texture.Size.X * texture.Size.Y) * Gl.PixelTypeToByteCount(texture.PixelType) *
                               Gl.PixelFormatToComponentCount(texture.PixelFormat);
                totalBytes += byteSize;
            }

            ImGui.Text($"Texture Memory Estimate: {Helpers.FormatByteAmountAsString(totalBytes)}");
            ImGui.Text($"Audio Buffer Memory: {Helpers.FormatByteAmountAsString(AudioLayer.MetricAllocatedDataBlocks)}");
            ImGui.Text($"Managed Memory (Game): {Helpers.FormatByteAmountAsString(GC.GetTotalMemory(false))}");

            long usedRam = _p.WorkingSet64;
            long usedRamMost = _p.PrivateMemorySize64;
            ImGui.Text($"Total Memory Used: {Helpers.FormatByteAmountAsString(usedRam)} | Allocated: {Helpers.FormatByteAmountAsString(usedRamMost)}");

            ImGui.NewLine();
            ImGui.BeginGroup();
            ImGui.BeginTabBar("TabBar");
            if (ImGui.BeginTabItem("Assets"))
            {
                ImGui.BeginChild("Assets", new Vector2(450, 500), true, ImGuiWindowFlags.HorizontalScrollbar);

                Asset[] loadedAssets = Engine.AssetLoader.LoadedAssets;
                IOrderedEnumerable<Asset> orderedEnum = loadedAssets.OrderByDescending(x => x.Size);
                foreach (Asset asset in orderedEnum)
                {
                    float percent = (float) asset.Size / assetBytes;
                    ImGui.Text($"{asset.Name} {Helpers.FormatByteAmountAsString(asset.Size)} {percent * 100:0}%%");
                    ImGui.Text($"\t{asset.GetType()}");
                }

                ImGui.EndChild();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("XML Cache"))
            {
                ImGui.BeginChild("XMLType", new Vector2(450, 500), true, ImGuiWindowFlags.HorizontalScrollbar);

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
                            ImGui.Text($"\t Fields: {complexHandler.FieldCount()}");

                        ImGui.TreePop();
                    }

                    ImGui.PopID();
                }

                ImGui.EndChild();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
            ImGui.EndGroup();
        }

        public override void Update()
        {
        }
    }
}