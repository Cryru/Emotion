#nullable enable

using Emotion.Game.World.Grids;

namespace Emotion.Game.World;

public class GameMapFactory
{
    public string MapPath { get; set; } = "Unknown";
    public IMapGrid[] Grids { get; set; } = Array.Empty<IMapGrid>();

    public GameMap CreateMapInstance()
    {
        var inst = new GameMap()
        {
            FactoryCreatedFrom = this
        };

        IMapGrid[] grids = new IMapGrid[Grids.Length];
        for (int i = 0; i < grids.Length; i++)
        {
            grids[i] = Grids[i];
        }
        inst.Grids = grids;

        return inst;
    }

    public static GameMapFactory CreateFromMap(GameMap map)
    {
        IMapGrid[] grids = new IMapGrid[map.Grids.Length];
        for (int i = 0; i < map.Grids.Length; i++)
        {
            IMapGrid grid = map.Grids[i];
            grids[i] = grid.Copy();
        }

        var factory = new GameMapFactory()
        {
            Grids = grids
        };

        return factory;
    }
}
