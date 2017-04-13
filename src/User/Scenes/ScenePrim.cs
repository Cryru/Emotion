using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Objects;
using SoulEngine.Objects.Components;
using SoulEngine.Events;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// The first scene to load.
    /// </summary>
    public class ScenePrim : Scene
    {
        #region "Declarations"

        #endregion
        public override void Start()
        {
            Networking.Connect("127.0.0.1", 1479, "test", "test");

            ESystem.Add(new Listen(EType.NETWORK_MESSAGE, PositionUpdate, "2"));

            GameObject nettest = GameObject.GenericDrawObject;

            nettest.Component<Transform>().Size = new Vector2(5, 5);
            nettest.Component<Transform>().CenterObject();

            nettest.Component<ActiveTexture>().Texture = AssetManager.BlankTexture;

            AddObject("obj", nettest);

            Ticker localUpdate = new Ticker(100, -1, true);

            ESystem.Add(new Listen(EType.TICKER_TICK, SendUpdate, localUpdate));
        }

        public override void Update()
        {
            int vert = 0;
            int horz = 0;

            if (Input.isKeyDown(Microsoft.Xna.Framework.Input.Keys.A)) horz -= 1;
            if (Input.isKeyDown(Microsoft.Xna.Framework.Input.Keys.D)) horz += 1;
            if (Input.isKeyDown(Microsoft.Xna.Framework.Input.Keys.W)) vert -= 1;
            if (Input.isKeyDown(Microsoft.Xna.Framework.Input.Keys.S)) vert += 1;

            GetObject("obj").Component<Transform>().X += (0.3f * Context.Core.frameTime) * horz;
            GetObject("obj").Component<Transform>().Y += (0.3f * Context.Core.frameTime) * vert;
        }

        public void PositionUpdate(Event e)
        {
            Dictionary<string, object> Data = Soul.JSON.fromJSON<Dictionary<string,object>>((string) e.Data);

            //TODO: Get data and create/reuse objects + assign own
            GetObject("obj").Component<Transform>().Position = new Vector2();
        }

        public void SendUpdate()
        {
            Networking.Send(new SoulServer.ServerMessage("2", Soul.JSON.toJSON<Vector2>(GetObject("obj").Component<Transform>().Position)));
        }
    }
}
