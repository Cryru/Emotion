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
            ESystem.Add(new Listen(EType.NETWORK_MESSAGE, PositionUpdate, "2"));
            ESystem.Add(new Listen(EType.NETWORK_STATUSCHANGED, ConnectServer, Enums.NetworkStatus.Disconnected));

            ConnectServer();

            GameObject nettest = GameObject.GenericDrawObject;

            nettest.Component<Transform>().Size = new Vector2(5, 5);
            nettest.Component<Transform>().CenterObject();

            nettest.Component<ActiveTexture>().Texture = AssetManager.BlankTexture;

            nettest.Drawing = false;

            AddObject("me", nettest);
            AddCluster("others", new List<GameObject>());

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

            GetObject("me").Component<Transform>().X += (0.3f * Context.Core.frameTime) * horz;
            GetObject("me").Component<Transform>().Y += (0.3f * Context.Core.frameTime) * vert;
        }

        public void PositionUpdate(Event e)
        {
           dynamic Data = Soul.JSON.fromJSON<dynamic>((string)e.Data);

            bool b = true;
            //Dictionary<string, object> Others = ((Newtonsoft.Json.Linq.JObject)Data["others"]).ToObject<Dictionary<string, object>>();
            //Dictionary<string, object> Me = ((Newtonsoft.Json.Linq.JObject)Data["me"]).ToObject<Dictionary<string, object>>();

            //List<GameObject> OthersLocal = GetCluster("others");

            //foreach (var item in Others)
            //{
            //    Dictionary<string, object> thisguy = ((Newtonsoft.Json.Linq.JObject) item.Value).ToObject<Dictionary<string, object>>();

            //    if (OthersLocal.Count < int.Parse(item.Key))
            //    {
            //        OthersLocal[int.Parse(item.Key)].Component<Transform>().Position = new Vector2((float)((double)thisguy["X"]), (float)((double)thisguy["Y"]));
            //    }
            //    else
            //    {
            //        GameObject newGuy = GameObject.GenericDrawObject;

            //        newGuy.Component<Transform>().Size = new Vector2(5, 5);
            //        newGuy.Component<ActiveTexture>().Texture = AssetManager.BlankTexture;
            //        newGuy.Component<Transform>().Position = new Vector2((float)((double)thisguy["X"]), (float)((double)thisguy["Y"]));

            //        OthersLocal.Add(newGuy);
            //    }
            //}

            //GetObject("me").Component<Transform>().Position = new Vector2((float)((double)Me["X"]), (float) ((double) Me["Y"]));
            //GetObject("me").Drawing = true;
        }

        public void SendUpdate()
        {
            if (Networking.Status != Enums.NetworkStatus.Connected) return;
            Networking.Send(new SoulServer.ServerMessage("2", Soul.JSON.toJSON<Vector2>(GetObject("me").Component<Transform>().Position)));
        }

        public void ConnectServer()
        {
            bool connectionSuccessful = Networking.Connect("127.0.0.1", 1479, "test", "test");
            if (!connectionSuccessful)
            {
                Ticker recon = new Ticker(3000, 1, true);
                ESystem.Add(new Listen(EType.TICKER_DONE, ConnectServer, recon, 1));
            }
        }
    }
}
