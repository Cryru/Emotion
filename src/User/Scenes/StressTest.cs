using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Objects;
using SoulEngine.Objects.Components;
using Microsoft.Xna.Framework;

namespace SoulEngine
{
    class StressTest : Scene
    {
        public override void Start()
        {
            AddCluster("testCluster", new List<GameObject>());

            int detail = 5;

            for (int x = 0; x <= Settings.Width; x += detail)
            {
                for (int y = 0; y <= Settings.Height; y += detail)
                {
                    GameObject temp = new GameObject();

                    int R = Functions.generateRandomNumber(1, 255);
                    int G = Functions.generateRandomNumber(1, 255);
                    int B = Functions.generateRandomNumber(1, 255);

                    temp.AddComponent(new ActiveTexture(Enums.TextureMode.Stretch, AssetManager.BlankTexture));
                    temp.Component<ActiveTexture>().Tint = new Color(Functions.generateRandomNumber(1, 255),
                    Functions.generateRandomNumber(1, 255), Functions.generateRandomNumber(1, 255));

                    temp.Size = new Vector2(detail, detail);
                    temp.X = x;
                    temp.Y = y;

                    GetCluster("testCluster").Add(temp);
                }
            }
        }

        public override void Update()
        {
            return;
            for (int i = 0; i < GetCluster("testCluster").Count; i++)
            {
                GetCluster("testCluster")[i].Component<ActiveTexture>().Tint = new Color(Functions.generateRandomNumber(1, 255),
                    Functions.generateRandomNumber(1, 255), Functions.generateRandomNumber(1, 255));
            }
        }
    }
}
