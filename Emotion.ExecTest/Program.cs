#region Using

using System.Collections.Generic;
using System.Numerics;
using Emotion.Common;
using Emotion.Game.ThreeDee.Editor;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Data;
using Emotion.Graphics.ThreeDee;
using Emotion.Primitives;
using Emotion.Scenography;

#endregion

namespace Emotion.ExecTest
{
    public class Program : IScene
    {
        private static void Main()
        {
            var config = new Configurator
            {
                DebugMode = true
            };

            Engine.Setup(config);
            Engine.SceneManager.SetScene(new Program());
            Engine.Run();
        }

        private List<Plane3D> _planes = new List<Plane3D>();
        private TranslationGizmo _gizmo;

        public void Load()
        {
            var cam3d = new Camera3D(new Vector3(-90, 50, 60));
            cam3d.LookAtPoint(new Vector3(0, 0, 0));
            Engine.Renderer.Camera = cam3d;

            var ground = new Plane3D();
            ground.Position = new Vector3(0, 0, 0);
            ground.Size = new Vector3(30, 30, 1);
            ground.Tint = Color.Black;
            _planes.Add(ground);

            var wall = new Plane3D();
            wall.Position = new Vector3(0, 0, 5);
            wall.Size = new Vector3(10, 10, 1);
            wall.RotationDeg = new Vector3(0, -90, 0);
            _planes.Add(wall);

            var transGizmo = new TranslationGizmo();
            transGizmo.Position = new Vector3(0, 0, 0);
            _gizmo = transGizmo;
        }

        public void Update()
        {
	        var cam = (Camera3D)Engine.Renderer.Camera;
	        cam.DefaultMovementLogicUpdate();
        }

        public void Draw(RenderComposer composer)
        {
            composer.SetUseViewMatrix(false);
            composer.RenderSprite(Vector3.Zero, composer.CurrentTarget.Size, Color.CornflowerBlue);
            composer.ClearDepth();
            composer.SetUseViewMatrix(true);

            //composer.RenderLine(new Vector3(0, 0, 0), new Vector3(short.MaxValue, 0, 0), Color.Red, snapToPixel: false);
            //composer.RenderLine(new Vector3(0, 0, 0), new Vector3(0, short.MaxValue, 0), Color.Green, snapToPixel: false);
            //composer.RenderLine(new Vector3(0, 0, 0), new Vector3(0, 0, short.MaxValue), Color.Blue, snapToPixel: false);

            _gizmo.Render(composer);

            Ray3D ray = ((Camera3D) Engine.Renderer.Camera).GetCameraMouseRay();
            if (ray.IntersectWithObject(_gizmo, out Mesh col, out Vector3 colPoint, out Vector3 norm, out int triangleIdx))
            {
	            //col.GetTriangleAtIndex(triangleIdx, out Vector3 p1, out Vector3 p2, out Vector3 p3);
	            //composer.DbgClear();
	            //composer.DbgAddTriangle(p1, p2, p3);
            }
        }

        public void Unload()
        {
        }
    }
}