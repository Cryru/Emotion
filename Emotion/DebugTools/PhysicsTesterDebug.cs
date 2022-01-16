#region Using

using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace Emotion
{
    public static class PhysicsTesterDebug
    {
        public static bool Enabled { get; private set; }

        private static Dictionary<string, object> _reference = new Dictionary<string, object>();
        private static bool _recordMode = true;

        public static void Enable()
        {
            Enabled = true;
        }

        private static List<string> _id = new List<string>();

        public static void PushId(string name)
        {
            if (!Enabled) return;
            _id.Add(name);
        }

        public static void PopId()
        {
            if (!Enabled) return;
            _id.RemoveAt(_id.Count - 1);
        }

        public static void RecordData(string name, object value)
        {
            if (!Enabled) return;

            var id = "";
            for (var i = 0; i < _id.Count; i++)
            {
                id += _id[i];
            }
            name = id + "_" + name;

            if (_recordMode)
            {
                _reference.Add(name, value);
            }
            else
            {
                object valRecorded = _reference[name];
                Debug.Assert(valRecorded.Equals(value));
            }
        }

        public static void SwitchMode()
        {
            if (!Enabled) return;

            _recordMode = !_recordMode;
        }
    }
}