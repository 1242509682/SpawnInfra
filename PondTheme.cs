using Terraria.ID;

namespace SpawnInfra;

internal class PondTheme
{
    public ushort tile = 0;

    public ushort wall = 16;

    public TileInfo platform = new TileInfo(19, 0);

    public TileInfo chair = new TileInfo(15, 0);

    public TileInfo bench = new TileInfo(18, 0);

    public TileInfo torch = new TileInfo(4, 0);

    public void SetGlass()
    {
        //默认54 玻璃 → 267 钻石晶莹宝石块
        tile = TileID.WoodBlock;
        //默认 21 玻璃墙 → 155 钻石晶莹宝石墙
        wall = WallID.Wood;
        platform.style = 0; //14
        chair.style = 0; //18
        bench.style = 0; //25
        torch.style = 0; //5
    }

    public void SetGray()
    {
        tile = 38;
        wall = 5;
        platform.style = 43;
    }

    public void SetWood()
    {
        tile = 30;
        wall = 4;
    }

    public void SetHoney()
    {
        tile = 225;
        wall = 108;
        platform.style = 24;
    }

    public void SetObsidian()
    {
        tile = 75;
        wall = 20;
        platform.style = 13;
    }
}