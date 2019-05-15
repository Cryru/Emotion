using Adfectus.IO;

namespace Rationale.Interop
{
    public class DebugMessage
    {
        public MessageType Type { get; set; }
        public object Data { get; set; }
        public string[] StringArrayData { get; set; }
    }
}