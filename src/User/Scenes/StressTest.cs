using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Objects;
using SoulEngine.Objects.Components;
using Microsoft.Xna.Framework;
using SoulEngine.Modules;

namespace SoulEngine
{
    class StressTest : Scene
    {
        public override void Start()
        {
            AddCluster("testCluster", new List<GameObject>());

            int detail = 10;

            for (int x = 0; x <= Settings.Width; x += detail)
            {
                for (int y = 0; y <= Settings.Height; y += detail)
                {
                    GameObject temp = new GameObject();

                    int C = Functions.generateRandomNumber(1, 255);

                    temp.AddComponent(new ActiveTexture(Enums.TextureMode.Stretch, AssetManager.BlankTexture));
                    temp.Component<ActiveTexture>().Tint = new Color(C,
                    C, C);

                    temp.Size = new Vector2(detail, detail);
                    temp.X = x;
                    temp.Y = y;

                    GetCluster("testCluster").Add(temp);
                }
            }
        }

        public override void Update()
        {
            for (int i = 0; i < GetCluster("testCluster").Count; i++)
            {
                int C = Functions.generateRandomNumber(1, 255);

                GetCluster("testCluster")[i].Component<ActiveTexture>().Tint = new Color(C,
                    C, C);
            }
        }
    }
}
