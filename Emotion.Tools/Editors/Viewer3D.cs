#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Game.Animation3D;
using Emotion.Game.ThreeDee;
using Emotion.Graphics;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Data;
using Emotion.Graphics.ThreeDee;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Tools.DevUI;
using Emotion.Tools.Windows.HelpWindows;
using Emotion.UI;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Editors
{
    public class Viewer3D : ImGuiBaseWindow
    {
        public Object3D DisplayObject;

        private CameraBase _oldCamera;
        private Terrain3D _terrain;
        private bool _showTerrain;
        private static ShaderAsset _skeletalShader;
        private static RenderStreamBatch<VertexDataWithBones> _boneVerticesStream;

        public Viewer3D() : base("3D Viewer")
        {
            DisplayObject = new Object3D();
            _oldCamera = Engine.Renderer.Camera;
            Engine.Renderer.Camera = new Camera3D(new Vector3(20, 50, 200));
            Engine.Renderer.Camera.LookAt = Vector3.Normalize(Vector3.Zero - Engine.Renderer.Camera.Position);

            _skeletalShader ??= Engine.AssetLoader.Get<ShaderAsset>("Shaders/SkeletalAnim.xml");
            if (_boneVerticesStream == null)
                GLThread.ExecuteGLThreadAsync(() => { _boneVerticesStream = new RenderStreamBatch<VertexDataWithBones>(0, 1, false); });

            _terrain = new Terrain3D(32, 32, 16);

            _windowFlags |= ImGuiWindowFlags.MenuBar;
        }

        public override void DetachedFromController(UIController controller)
        {
            Engine.Renderer.Camera = _oldCamera;
            base.DetachedFromController(controller);
        }

        protected override bool UpdateInternal()
        {
            DisplayObject.Update(Engine.DeltaTime);

            // Prevent dragging on controls from moving the camera.
            if (Controller?.InputFocus == this) return true;

            var camera = (Camera3D) Engine.Renderer.Camera;
            camera.DefaultMovementLogicUpdate();

            return true;
        }

        protected override bool RenderInternal(RenderComposer c)
        {
            base.RenderInternal(c);

            RenderState oldState = c.CurrentState.Clone();
            c.SetState(RenderState.Default);
            c.SetUseViewMatrix(false);
            c.RenderSprite(new Vector3(0, 0, 0), Engine.Renderer.CurrentTarget.Size, Color.CornflowerBlue);
            c.SetUseViewMatrix(true);
            c.ClearDepth();

            //c.RenderLine(new Vector3(short.MinValue, 0, 0), new Vector3(short.MaxValue, 0, 0), Color.Red, snapToPixel: false);
            //c.RenderLine(new Vector3(0, short.MinValue, 0), new Vector3(0, short.MaxValue, 0), Color.Green, snapToPixel: false);
            //c.RenderLine(new Vector3(0, 0, short.MinValue), new Vector3(0, 0, short.MaxValue), Color.Blue, snapToPixel: false);

            c.RenderLine(new Vector3(0, 0, 0), new Vector3(short.MaxValue, 0, 0), Color.Red, snapToPixel: false);
            c.RenderLine(new Vector3(0, 0, 0), new Vector3(0, short.MaxValue, 0), Color.Green, snapToPixel: false);
            c.RenderLine(new Vector3(0, 0, 0), new Vector3(0, 0, short.MaxValue), Color.Blue, snapToPixel: false);

            if (_showTerrain) _terrain.Render(c);

            if (DisplayObject.Entity?.AnimationRig != null)
            {
                c.SetShader(_skeletalShader.Shader);
                DisplayObject.RenderAnimated(c, _boneVerticesStream);
                c.SetShader();
            }
            else
            {
                DisplayObject.Render(c);
            }

            c.SetState(oldState);

            _boneVerticesStream?.DoTasks(c);

            return true;
        }

        protected override void RenderImGui()
        {
            if (ImGui.BeginMenuBar())
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.3f, 0.3f, 0.3f, 1f));
                if (ImGui.Button("Open OBJ"))
                {
                    var explorer = new FileExplorer<ObjMeshAsset>(asset => { DisplayObject.Entity = asset.Entity; });
                    _toolsRoot.AddLegacyWindow(explorer);
                }

                if (ImGui.Button("Open EM3"))
                {
                    var explorer = new FileExplorer<EmotionMeshAsset>(asset => { DisplayObject.Entity = asset.Entity; });
                    _toolsRoot.AddLegacyWindow(explorer);
                }

                if (ImGui.Button("Open Sprite Stack Texture"))
                {
                    var explorer = new FileExplorer<SpriteStackTexture>(asset =>
                    {
                        _toolsRoot.AddLegacyWindow(new Vec2Modal(size =>
                        {
                            MeshEntity entity = asset.GetSpriteStackEntity(size);
                            DisplayObject.Entity = entity;
                        }, "Sprite Stack Settings", "Individual Frame Size", new Vector2(32, 32)));
                    });
                    _toolsRoot.AddLegacyWindow(explorer);
                }

                ImGui.EndMenuBar();
            }

            ImGui.Checkbox("Show Terrain", ref _showTerrain);

            Vector3 pos = DisplayObject.Position;
            if (ImGui.DragFloat3("Position", ref pos)) DisplayObject.Position = pos;

            Vector3 scale = DisplayObject.Size;
            if (ImGui.DragFloat3("Scale", ref scale)) DisplayObject.Size = scale;

            Vector3 rot = DisplayObject.RotationDeg;
            if (ImGui.DragFloat3("Rotation", ref rot)) DisplayObject.RotationDeg = rot;

            if (DisplayObject.Entity != null && DisplayObject.Entity.Animations != null && ImGui.BeginCombo("Animation", DisplayObject.CurrentAnimation))
            {
                if (ImGui.Button("None")) DisplayObject.SetAnimation(null);

                for (var i = 0; i < DisplayObject.Entity.Animations.Length; i++)
                {
                    SkeletalAnimation anim = DisplayObject.Entity.Animations[i];
                    if (ImGui.Button($"{anim.Name}")) DisplayObject.SetAnimation(anim.Name);
                }

                ImGui.EndCombo();
            }
        }
    }
}