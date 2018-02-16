using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Soul.Engine;
using Soul.Engine.Scenography;
using Soul.Engine.Modules;

namespace Soul.Examples
{
    public class SceneLoading : Scene
    {
        public static void Main()
        {
            Core.Setup(new SceneLoading());
        }

        protected override void Setup()
        {
            SceneManager.LoadScene("scenetoload", new SceneToLoad(), true);
            SceneManager.LoadScene("eternalLoad", new EternalLoad());
        }

        protected override void Update()
        {
            Console.WriteLine("Main Scene :)");
        }
    }

    public class SceneToLoad : Scene
    {
        protected override void Setup()
        {
            Thread.Sleep(3000);
        }

        protected override void Update()
        {
            Console.WriteLine("Scene loaded!");
        }
    }

    public class EternalLoad : Scene
    {
        protected override void Setup()
        {
           while(true) { }
        }

        protected override void Update()
        {
            Console.WriteLine("Error: This scene loaded somehow.");
        }
    }
}
