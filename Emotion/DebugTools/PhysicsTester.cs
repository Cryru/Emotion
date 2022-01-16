#region Using

using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.Utility;

#endregion

namespace Emotion.DebugTools
{
    public abstract class PhysicsTesterAdapter
    {
        public Stopwatch Profiler = new Stopwatch();

        public abstract int AddBody(Vector2 position, Vector2 size, float rotation, int type);
        public abstract void Simulate(float time);
        public abstract void GetBodyData(int bodyIndex, out Vector2 position, out float rotation);
        public abstract void Render(RenderComposer c, Color color);
    }

    public class PhysicsTester
    {
        private List<PhysicsTesterAdapter> _adapters = new List<PhysicsTesterAdapter>();
        private float _physicsScale;
        private int _lastBodyIdx;
        private int _step;
        private bool _exact;

        private Color[] _colors =
        {
            Color.White,
            Color.Yellow,
            Color.Magenta
        };

        public PhysicsTester(float physicsScale, bool exact = true)
        {
            _physicsScale = physicsScale;
            _exact = exact;
        }

        public void AddAdapter(PhysicsTesterAdapter adapter)
        {
            _adapters.Add(adapter);
        }

        public void AddBody(Vector2 position, Vector2 size, float rotation, int type)
        {
            for (var i = 0; i < _adapters.Count; i++)
            {
                PhysicsTesterAdapter adapter = _adapters[i];
                int idx = adapter.AddBody(position * _physicsScale, size * _physicsScale, rotation, type);
                _lastBodyIdx = idx;
            }
        }

        public void Step(float time)
        {
            //if (_step == 61)
            //    PhysicsTesterDebug.Enable();

            CompareAllBodies();
            for (var i = 0; i < _adapters.Count; i++)
            {
                PhysicsTesterAdapter adapter = _adapters[i];
                if (i == 1) PhysicsTesterDebug.SwitchMode();

                adapter.Profiler.Restart();
                adapter.Simulate(time);
                adapter.Profiler.Stop();
            }

            CompareAllBodies();
            _step++;
        }

        private void CompareAllBodies()
        {
            var positions = new Vector2[_adapters.Count];
            var rotations = new float[_adapters.Count];

            for (var i = 0; i < _lastBodyIdx; i++)
            {
                for (var j = 0; j < _adapters.Count; j++)
                {
                    PhysicsTesterAdapter adapter = _adapters[j];
                    adapter.GetBodyData(i, out Vector2 pos, out float rot);
                    positions[j] = pos;
                    rotations[j] = rot;
                }

                for (var j = 0; j < _adapters.Count - 1; j++)
                {
                    //if (_exact)
                    //{
                    //    Debug.Assert(positions[j] == positions[j + 1]);
                    //    Debug.Assert(rotations[j] == rotations[j + 1]);
                    //}
                    //else
                    //{
                    //    Vector2 difference = Vector2.Abs(positions[j] - positions[j + 1]);
                    //    Debug.Assert(difference.X < Maths.EPSILON);
                    //    Debug.Assert(difference.Y < Maths.EPSILON);
                    //    Debug.Assert(rotations[j] - rotations[j + 1] < Maths.EPSILON);
                    //}
                }
            }
        }

        public void Render(RenderComposer c)
        {
            c.PushModelMatrix(Matrix4x4.CreateScale(1f / _physicsScale, 1f / _physicsScale, 1f));
            for (var i = 0; i < _adapters.Count; i++)
            {
                PhysicsTesterAdapter adapter = _adapters[i];
                adapter.Render(c, _colors[i].SetAlpha(100));
            }

            c.PopModelMatrix();
        }
    }
}