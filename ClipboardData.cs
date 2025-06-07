using Terraria;
using Microsoft.Xna.Framework;

namespace SpawnInfra;

//箱子物品数据
public class ChestItemData
{
    public Item? Item { get; set; }           // 物品本身（可空）
    public Point ChestPosition { get; set; }  // 箱子位置（世界坐标） 
    public int Slot { get; set; }             // 槽位编号（0 ~ 39）
}

//剪贴板数据
public class ClipboardData
{
    public int Width { get; set; }
    public int Height { get; set; }
    public Tile[,]? Tiles { get; set; }
    public Point Origin { get; set; }
    public List<ChestItemData>? ChestItems { get; set; }
}