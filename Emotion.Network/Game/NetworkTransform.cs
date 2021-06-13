#region Using

using System;
using System.Diagnostics;
using System.Numerics;
using Emotion.Network.Infrastructure;
using Emotion.Primitives;
using Emotion.Utility;

#endregion

namespace Emotion.Network.Game
{
    public class NetworkTransform : Transform
    {
        public NetworkScene Parent;
        public string ObjectId { get; set; }
        public NetworkActorHandle Owner = NetworkActorHandle.ServerHandle;

        public NetworkTransform(string uniqueId)
        {
            ObjectId = uniqueId;
        }

        // Serialization constructor.
        protected NetworkTransform()
        {
        }

        #region Temporal State

        public class StatePoint
        {
            public StatePoint Previous;

            public Vector3 Position;
            public float Time;
        }

        private StatePoint[] _states = new StatePoint[NetworkScene.MAX_TICK_BACK + 1]; // Ring array
        private int _nextStateIdx;

        public Vector3 DebugLatestPosition;

        public void ApplyFullData(float timestamp, NetworkTransform data)
        {
            Position = data.Position;
            Size = data.Size;
            AddStatePoint(timestamp, data);
        }

        public void AddStatePoint(float timestamp, NetworkTransform data)
        {
            StatePoint statePoint = GetNextStatePointForWriting();
            statePoint.Position = data.Position;
            statePoint.Time = timestamp;
            DebugLatestPosition = data.Position;
        }

        private StatePoint GetNextStatePointForWriting()
        {
            StatePoint nextState = _states[_nextStateIdx];
            if (nextState == null)
            {
                nextState = new StatePoint();
                _states[_nextStateIdx] = nextState;

                // Establish links.
                if (_nextStateIdx != 0) nextState.Previous = _states[_nextStateIdx - 1];
                if (_nextStateIdx == _states.Length - 1) _states[0].Previous = nextState;
            }

            _nextStateIdx++;
            if (_nextStateIdx == _states.Length) _nextStateIdx = 0;
            return nextState;
        }

        private float GetStatePointsAtTime(float time, out StatePoint previous, out StatePoint next)
        {
            // Next state will be the oldest state, so start looking from there.
            StatePoint latestPoint = null;
            int s = _nextStateIdx;
            int i = _nextStateIdx;
            do
            {
                StatePoint point = _states[i];
                if (point == null) goto cont;

                float pointTime = point.Time;
                if (pointTime > time)
                {
                    next = point;
                    previous = next.Previous ?? next; // If null - no data for point before this one.
                    float timeBetweenPoints = next.Time - previous.Time; // next should be after previous, unless experiencing extreme lag.
                    float timeSinceLastPoint = time - previous.Time;
                    if (timeSinceLastPoint == 0.0f) return 1.0f; // Next and current are the at the same time. Go to next.
                    return Maths.Clamp01(timeSinceLastPoint / timeBetweenPoints);
                }

                if (latestPoint == null || pointTime > latestPoint.Time) latestPoint = point;

                cont:
                i++;
                if (i == _states.Length) i = 0;
            } while (s != i);

            Debug.Assert(latestPoint != null); // At least one point will be always present/initialized.

            // All data points are before the provided point in time.
            next = latestPoint;
            previous = latestPoint.Previous ?? latestPoint;

            // Extrapolate future state if the last movement is extrapolation friendly.
            if (next != previous)
            {
                bool extrapolationFriendly = previous.Previous == null;
                if (!extrapolationFriendly)
                {
                    Vector3 normalBeforeThat = Vector3.Normalize(previous.Position - previous.Previous.Position);
                    Vector3 normal = Vector3.Normalize(next.Position - previous.Position);
                    extrapolationFriendly = Vector3.Dot(normal, normalBeforeThat) > 0.9f;
                }

                if (extrapolationFriendly)
                {
                    float timeAfterNext = time - next.Time;
                    float timeBetweenPoints = next.Time - previous.Time;
                    float extrapolationFactor = timeAfterNext / timeBetweenPoints;
                    return 1.0f + extrapolationFactor;
                }
            }

            // Otherwise wait at last state point.
            return 1.0f;
        }

        public void ApplyStateAtTime(float time)
        {
            float progress = GetStatePointsAtTime(time, out StatePoint previousPoint, out StatePoint nextPoint);
            Position = Vector3.Lerp(previousPoint.Position, nextPoint.Position, progress);
        }

        #endregion
    }
}