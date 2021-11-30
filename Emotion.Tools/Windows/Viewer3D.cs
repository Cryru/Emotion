#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.Game.SpriteStack;
using Emotion.Game.ThreeDee;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.Graphics.ThreeDee;
using Emotion.IO;
using Emotion.Plugins.ImGuiNet;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Primitives;
using Emotion.Tools.Windows.HelpWindows;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows
{
    public class Viewer3D : ImGuiWindow
    {
        public Object3D DisplayObject;

        private CameraBase _oldCamera;

        public Viewer3D() : base("3D Viewer")
        {
            MenuBar = true;

            DisplayObject = new Object3D();
            Engine.Renderer.FarZ = 10_000;
            _oldCamera = Engine.Renderer.Camera;
            Engine.Renderer.Camera = new Camera3D(new Vector3(20, 50, 200));
            Engine.Renderer.Camera.LookAt = Vector3.Normalize(Vector3.Zero - Engine.Renderer.Camera.Position);
        }

        public override void Dispose()
        {
            Engine.Renderer.Camera = _oldCamera;
            base.Dispose();
        }

        public override void Update()
        {
            var camera = (Camera3D) Engine.Renderer.Camera;
            if (ImGuiNetPlugin.Focused) return;
            camera.DefaultMovementLogicUpdate();
        }

        protected override void RenderContent(RenderComposer c)
        {
            if (ImGui.BeginMenuBar())
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.3f, 0.3f, 0.3f, 1f));
                if (ImGui.Button("Open OBJ"))
                {
                    var explorer = new FileExplorer<ObjMeshAsset>(asset => { DisplayObject.Entity = asset.Entity; });
                    Parent.AddWindow(explorer);
                }

                if (ImGui.Button("Open Sprite Stack Texture"))
                {
                    var explorer = new FileExplorer<SpriteStackTexture>(asset =>
                    {
                        Parent.AddWindow(new Vec2Modal(size =>
                        {
                            MeshEntity entity = asset.GetSpriteStackEntity(size);
                            DisplayObject.Entity = entity;
                        }, "Sprite Stack Settings", "Individual Frame Size", new Vector2(32, 32)));
                    });
                    Parent.AddWindow(explorer);
                }

                ImGui.EndMenuBar();
            }

            float scale = DisplayObject.Scale;
            if (ImGui.DragFloat("Scale", ref scale)) DisplayObject.Scale = scale;

            Vector3 rot = DisplayObject.RotationDeg;
            if (ImGui.DragFloat3("Rotation", ref rot)) DisplayObject.RotationDeg = rot;

            c.SetUseViewMatrix(false);
            c.RenderSprite(new Vector3(0, 0, 0), Engine.Renderer.CurrentTarget.Size, Color.CornflowerBlue);
            c.SetUseViewMatrix(true);
            c.ClearDepth();

            c.RenderLineScreenSpace(new Vector3(short.MinValue, 0, 0), new Vector3(short.MaxValue, 0, 0), Color.Red, snapToPixel: false);
            c.RenderLineScreenSpace(new Vector3(0, short.MinValue, 0), new Vector3(0, short.MaxValue, 0), Color.Green, snapToPixel: false);
            c.RenderLineScreenSpace(new Vector3(0, 0, short.MinValue), new Vector3(0, 0, short.MaxValue), Color.Blue, snapToPixel: false);
            c.FlushRenderStream();

            DisplayObject.Render(c);
        }
    }
}