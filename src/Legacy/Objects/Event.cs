using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Legacy.Objects
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    // This code is part of the SoulEngine backwards compatibility layer.       //
    // Original Repository: https://github.com/Cryru/SoulEngine-2016            //
    //////////////////////////////////////////////////////////////////////////////
    public class Event
    {
        public int Count()
        {
            return 0;
        }

        public void Trigger(object data)
        {

        }

        public void Add(Action a)
        {

        }
    }
    public class Event<T>
    {
        public int Count()
        {
            return 0;
        }

        public void Trigger(object data)
        {

        }

        public void Add(Action a)
        {

        }
    }
}
