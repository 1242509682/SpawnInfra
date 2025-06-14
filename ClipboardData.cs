using Terraria;
using TShockAPI;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Tile_Entities;

namespace SpawnInfra;

//剪贴板数据
public class ClipboardData
{
    public int Width { get; set; }
    public int Height { get; set; }
    public Tile[,]? Tiles { get; set; }
    public Point Origin { get; set; }

    public List<ChestItems>? ChestItems { get; set; } // 箱子的物品
    public List<Sign>? Signs { get; set; } //标牌
    public List<ItemFrames> ItemFrames { get; set; } //物品框
    public List<WRacks> WeaponsRacks { get; set; } //武器架
    public List<FPlatters> FoodPlatters { get; set; } //盘子
    public List<DDolls> DisplayDolls { get; set; } //人偶
    public List<HatRacks> HatRacks { get; set; } //衣帽架
    public List<LogicSensors> LogicSensors { get; set; } //逻辑感应器
}

//箱子物品数据
public class ChestItems
{
    public Item? Item { get; set; }           // 物品本身（可空）
    public Point Position { get; set; }  // 箱子位置（世界坐标） 
    public int Slot { get; set; }             // 槽位编号（0 ~ 39）
}

//物品框的物品数据
public class ItemFrames
{
    public NetItem Item { get; set; }
    public Point Position { get; set; }
}

//武器架
public class WRacks
{
    public NetItem Item { get; set; }
    public Point Position { get; set; }
}

//盘子
public class FPlatters
{
    public NetItem Item { get; set; }
    public Point Position { get; set; }
}

//人偶
public class DDolls
{
    public NetItem[] Items;
    public NetItem[] Dyes;
    public Point Position { get; set; }
}

//帽架
public class HatRacks
{
    public NetItem[] Items;
    public NetItem[] Dyes;
    public Point Position { get; set; }
}

// 逻辑感应器
public class LogicSensors
{
    public Point Position { get; set; }
    public TELogicSensor.LogicCheckType type { get; set; }
}

