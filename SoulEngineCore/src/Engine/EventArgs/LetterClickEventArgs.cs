using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace SoulEngine.Events
{
    public class LetterClickEventArgs : EventArgs
    {
        public string clickData;

        public LetterClickEventArgs(string clickData)
        {
            this.clickData = clickData;
        }
    }
}
