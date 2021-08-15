#region Using

using System.Collections.Generic;
using Emotion.Game.Pathing;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Utility;
using System.Numerics;
using Emotion.Common;
using Emotion.Primitives;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows
{
    public class PathingDebugger : PresetGenericEditor<PathingWorld>
    {
        public PathingSystem System;
        public bool Valid;
        private PathingActor _selectedActor;
        private List<Vector2> _previewPath;

        public PathingDebugger() : base("Pathing Debugger")
        {
        }

        public override void Update()
        {
            Helpers.CameraWASDUpdate();

            if(System == null) return;

            if (Engine.Host.IsKeyDown(Platform.Input.Key.MouseKeyRight) && _selectedActor != null)
            {
                _previewPath = System.CalculatePathTo(_selectedActor, Engine.Renderer.Camera.ScreenToWorld(Engine.Host.MousePosition));
            }
        }

        protected override void RenderContent(RenderComposer c)
        {
            base.RenderContent(c);

            if (System == null) return;
            
                c.SetUseViewMatrix(true);
                foreach (PathingActor actor in System.Actors)
                {
                    Color color = (actor.Type == PathingActorType.Dynamic ? Color.Green : Color.Red) * 0.3f;
                    if (actor == _selectedActor) color = Color.Yellow;
                    c.RenderSprite(actor, color);
                }

                if (_previewPath != null)
                {
                    for (var i = 0; i < _previewPath.Count - 1; i++)
                    {
                        c.RenderLine(_previewPath[i], _previewPath[i + 1], Color.White);
                    }
                }

                c.SetUseViewMatrix(false);
            

            ImGui.Text($"World: {_currentAsset?.Name}, Valid: {Valid}");

            ImGui.BeginListBox("Dynamic Actors", new Vector2(100, 200));
            var idx = 1;
            foreach (PathingActor actor in System.DynamicActors)
            {
                if (_selectedActor == actor)
                    ImGui.Text($"{idx++} {actor.Identifier}");
                else if (ImGui.Button($"{idx++} {actor.Identifier}"))
                {
                    _selectedActor = actor;
                }
            }
            ImGui.EndListBox();
        }

        protected override bool OnFileLoaded(XMLAsset<PathingWorld> file)
        {
            System = new PathingSystem(file.Content);
            Valid = System.ValidateWorld();
            _selectedActor = null;
            return true;
        }

        protected override bool OnFileSaving()
        {
            return true;
        }
    }
}