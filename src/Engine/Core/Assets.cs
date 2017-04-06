using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine
{
    public class Assets
    {
        public ContentManager Content;

        public Assets()
        {
             Content = new ContentManager(Context.Core.Content.ServiceProvider, Context.Core.Content.RootDirectory);
        }
    }
}
