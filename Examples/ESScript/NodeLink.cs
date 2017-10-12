using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examples.ESScripts
{
    public class NodeLink
    {
        public Emotion destinationEmotion;
        public string destinationScript;

        public NodeLink(string destinationScript, Emotion destinationEmotion)
        {
            this.destinationScript = destinationScript;
            this.destinationEmotion = destinationEmotion;
        }
    }
}
