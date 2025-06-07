using Newtonsoft.Json;
using TShockAPI;
using SpawnInfra;
using Terraria;
using Terraria.ID;

namespace SpawnInfra;

internal class Configuration
{
    [JsonProperty("使用说明", Order = -15)]
    public string Text { get; set; } = "[微光直通车]的高度是从[世界平台]到湖中心,[自建微光湖]天顶从地狱往上算，其他以出生点往下算,开启则关闭[微光直通]";
    [JsonProperty("使用说明2", Order = -14)]
    public string Text2 { get; set; } = "[世界平台]普通世界从出生点往上算，天顶从太空层往下算，[刷怪场比例缩放]为倍数放大，建议不要调太高";
    [JsonProperty("使用说明3", Order = -13)]
    public string Text3 { get; set; } = "改[箱子数量]要同时改[出生点偏移X]，改[箱子层数]要同时改[出生点偏移Y]，[层数间隔]和[箱子宽度]不建议动,[启用墙壁与物品框]会使间隔额外加3格";
    [JsonProperty("使用说明4", Order = -12)]
    public string Text4 { get; set; } = "给玩家用的建晶塔房指令:/rms 数量 权限名:room.use (千万别在出生点用，有炸图风险)";
    [JsonProperty("使用说明5", Order = -11)]
    public string Text5 { get; set; } = "每次重置服务器前使用一遍：/rms 重置，或者直接把这个指令写进重置插件里";
    [JsonProperty("使用说明6", Order = -11)]
    public string Text6 { get; set; } = "以下参数仅适用于大地图，中地图需自己调试适配";
    [JsonProperty("使用说明7", Order = -11)]
    public string Text7 { get; set; } = "[设置刷怪率]需所有在线玩家在刷怪场中心且击败[肉山]后生效";

    [JsonProperty("插件开关", Order = -11)]
    public bool Enabled { get; set; } = true;

    [JsonProperty("重置是否自动基建", Order = -10)]
    public bool SpawnInfra { get; set; } = false;

    [JsonProperty("重置是否清理建筑", Order = -9)]
    public bool DeleteAllDataFiles { get; set; } = true;

    [JsonProperty("重置是否备份建筑", Order = -8)]
    public bool BackerAllDataFiles { get; set; } = true;

    [JsonProperty("复制建筑修复箱子物品", Order = -7)]
    public bool FixCopyChestItem { get; set; } = true;

    [JsonProperty("开服指令表", Order = -2)]
    public string[] CommandList { get; set; } = new string[] { };

    [JsonProperty("放置时禁止覆盖方块表", Order = -1)]
    public HashSet<int> DisableBlock = new HashSet<int>();

    [JsonProperty("自建微光湖", Order = 0)]
    public List<ItemData4> SpawnShimmerBiome { get; set; } = new List<ItemData4>();

    [JsonProperty("监狱集群", Order = 1)]
    public List<ItemData> Prison { get; set; } = new List<ItemData>();

    [JsonProperty("箱子集群", Order = 2)]
    public List<ItemData1> Chests { get; set; } = new List<ItemData1>();

    [JsonProperty("世界/左海平台", Order = 3)]
    public List<ItemData2> WorldPlatform { get; set; } = new List<ItemData2>();

    [JsonProperty("直通车/刷怪场", Order = 4)]
    public List<ItemData3> HellTunnel { get; set; } = new List<ItemData3>();

    #region 监狱数据
    public class ItemData
    {
        [JsonProperty("是否建监狱", Order = -9)]
        public bool BigHouseEnabled { get; set; } = false;

        [JsonProperty("出生点偏移X", Order = -2)]
        public int spawnTileX { get; set; }

        [JsonProperty("出生点偏移Y", Order = -2)]
        public int spawnTileY { get; set; }

        [JsonProperty("监狱集群宽度", Order = -2)]
        public int BigHouseWidth { get; set; }

        [JsonProperty("监狱集群高度", Order = -1)]
        public int BigHouseHeight { get; set; }

        [JsonProperty("图格ID", Order = 1)]
        public ushort TileID { get; set; }

        [JsonProperty("墙壁ID", Order = 2)]
        public ushort WallID { get; set; }

        [JsonProperty("平台样式", Order = 3)]
        public int PlatformStyle { get; set; }

        [JsonProperty("椅子样式", Order = 4)]
        public int ChairStyle { get; set; }

        [JsonProperty("工作台样式", Order = 5)]
        public int BenchStyle { get; set; }

        [JsonProperty("火把样式", Order = 6)]
        public int TorchStyle { get; set; }

        public ItemData(ushort tileID, ushort wallID, int tileX, int tileY, int width, int height, int platformstyle, int chairstyle, int benchstyle, int torchstyle)
        {
            this.TileID = tileID;
            this.WallID = wallID;
            this.spawnTileX = tileX;
            this.spawnTileY = tileY;
            this.BigHouseWidth = width;
            this.BigHouseHeight = height;
            this.PlatformStyle = platformstyle;
            this.ChairStyle = chairstyle;
            this.BenchStyle = benchstyle;
            this.TorchStyle = torchstyle;
        }
    }
    #endregion

    #region 箱子数据
    public class ItemData1
    {
        [JsonProperty("是否建箱子", Order = -9)]
        public bool SpawnChestEnabled { get; set; } = false;

        [JsonProperty("启用墙壁与物品框", Order = -8)]
        public bool WallAndItemFrame { get; set; } = false;

        [JsonProperty("出生点偏移X", Order = -3)]
        public int spawnTileX { get; set; }

        [JsonProperty("出生点偏移Y", Order = -3)]
        public int spawnTileY { get; set; }

        [JsonProperty("箱子数量", Order = -2)]
        public int ChestCount { get; set; }

        [JsonProperty("箱子层数", Order = -2)]
        public int ChestLayers { get; set; }

        [JsonProperty("层数间隔", Order = -2)]
        public int LayerHeight { get; set; }

        [JsonProperty("清理高度", Order = -1)]
        public int ClearHeight { get; set; }

        [JsonProperty("箱子宽度", Order = -1)]
        public int ChestWidth { get; set; }

        [JsonProperty("箱子ID", Order = 0)]
        public ushort ChestTileID { get; set; }

        [JsonProperty("箱子样式", Order = 0)]
        public int ChestStyle { get; set; }

        [JsonProperty("平台ID", Order = 1)]
        public ushort ChestPlatformID { get; set; }

        [JsonProperty("平台样式", Order = 2)]
        public int ChestPlatformStyle { get; set; }

        [JsonProperty("墙壁ID", Order = 3)]
        public ushort WallID { get; set; }

        [JsonProperty("物品框ID", Order = 4)]
        public ushort ItemFrameID { get; set; }

        public ItemData1(int tileX, int tileY, int width, int count, int layers, int layerheight, int height, ushort chestID, int chestStyle, ushort platformID, int platformstyle, ushort wallID, ushort itemFrameID)
        {
            this.spawnTileX = tileX;
            this.spawnTileY = tileY;
            this.ChestWidth = width;
            this.ChestLayers = layers;
            this.LayerHeight = layerheight;
            this.ChestCount = count;
            this.ClearHeight = height;
            this.ChestTileID = chestID;
            this.ChestStyle = chestStyle;
            this.ChestPlatformID = platformID;
            this.ChestPlatformStyle = platformstyle;
            this.WallID = wallID;
            this.ItemFrameID = itemFrameID;
        }
    }
    #endregion

    #region 世界/左海平台数据
    public class ItemData2
    {
        [JsonProperty("是否建世界平台", Order = -8)]
        public bool WorldPlatformEnabled { get; set; } = false;
        [JsonProperty("是否建世界轨道", Order = -7)]
        public bool WorldTrackEnabled { get; set; } = false;
        [JsonProperty("是否建左海平台", Order = -6)]
        public bool OceanPlatformEnabled { get; set; } = false;

        [JsonProperty("世界平台图格", Order = 2)]
        public int WorldPlatformID { get; set; }
        [JsonProperty("世界平台样式", Order = 3)]
        public int WorldPlatformStyle { get; set; }
        [JsonProperty("世界平台高度", Order = 5)]
        public int WorldPlatformY { get; set; }
        [JsonProperty("平台清理高度", Order = 6)]
        public int WorldPlatformClearY { get; set; }

        [JsonProperty("世界平台禁入左海距离", Order = 7)]
        public int WorldPlatformFromOceanLimit { get; set; }
        [JsonProperty("左海平台禁入出生点距离", Order = 7)]
        public int OceanPlatformFormSpwanXLimit { get; set; }
        [JsonProperty("左海平台长度", Order = 8)]
        public int OceanPlatformWide { get; set; }
        [JsonProperty("左海平台高度", Order = 9)]
        public int OceanPlatformHeight { get; set; }
        [JsonProperty("左海平台间隔", Order = 10)]
        public int OceanPlatformInterval { get; set; }

        [JsonProperty("实体块包围左海平台(天顶必开)", Order = 11)]
        public bool Enclose { get; set; } = false;
        [JsonProperty("左海平台产生液体(非空岛不需要开)", Order = 12)]
        public bool PlaceLiquid { get; set; } = false;
        [JsonProperty("世界平台不覆盖地牢砖与丛林蜥蜴砖", Order = 13)]
        public bool DisableBlock { get; set; } = true;

        public ItemData2(int id, int style, int tileY, int height, int limit, int limit2, int wide, int height2, int interval,bool disableBlock)
        {
            this.WorldPlatformID = id;
            this.WorldPlatformStyle = style;
            this.WorldPlatformY = tileY;
            this.WorldPlatformClearY = height;
            this.WorldPlatformFromOceanLimit = limit;
            this.OceanPlatformFormSpwanXLimit = limit2;
            this.OceanPlatformWide = wide;
            this.OceanPlatformHeight = height2;
            this.OceanPlatformInterval = interval;
            this.DisableBlock = disableBlock;
        }
    }
    #endregion

    #region 直通车与地狱平台数据
    public class ItemData3
    {
        [JsonProperty("是否建地狱直通车", Order = -6)]
        public bool HellTunnelEnabled { get; set; } = false;
        [JsonProperty("是否建地狱平台", Order = -5)]
        public bool HellPlatformEnabled { get; set; } = false;
        [JsonProperty("是否建地狱轨道", Order = -4)]
        public bool HellTrackEnabled { get; set; } = false;
        [JsonProperty("是否建微光直通车", Order = -3)]
        public bool ShimmerBiomeTunnelEnabled { get; set; } = false;
        [JsonProperty("是否建刷怪场", Order = -2)]
        public bool BrushMonstEnabled { get; set; } = false;
        [JsonProperty("只清刷怪区域", Order = -1)]
        public bool ClearRegionEnabled { get; set; } = false;
        [JsonProperty("直通车贯穿刷怪场", Order = 0)]
        public bool HellTrunnelCoverBrushMonst { get; set; } = true;

        [JsonProperty("方块图格", Order = 1)]
        public ushort Hell_BM_TileID { get; set; }
        [JsonProperty("绳子图格", Order = 2)]
        public ushort Cord_TileID { get; set; }
        [JsonProperty("平台图格", Order = 3)]
        public ushort PlatformID { get; set; }
        [JsonProperty("平台样式", Order = 3)]
        public int PlatformStyle { get; set; }

        [JsonProperty("直通车偏移X", Order = 4)]
        public int SpawnTileX { get; set; }
        [JsonProperty("直通车偏移Y", Order = 5)]
        public int SpawnTileY { get; set; }
        [JsonProperty("地狱直通车宽度", Order = 6)]
        public int HellTrunnelWidth { get; set; }
        [JsonProperty("地狱平台深度", Order = 7)]
        public int HellPlatformY { get; set; }

        [JsonProperty("微光直通车宽度", Order = 8)]
        public int ShimmerBiomeTunnelWidth { get; set; }

        [JsonProperty("刷怪场清理深度", Order = 10)]
        public int BrushMonstHeight { get; set; }
        [JsonProperty("刷怪场清理宽度", Order = 11)]
        public int BrushMonstWidth { get; set; }
        [JsonProperty("刷怪场比例缩放", Order = 12)]
        public int BrushMonstCenter { get; set; }
        [JsonProperty("是否放岩浆", Order = 13)]
        public bool Lava { get; set; } = true;
        [JsonProperty("是否放尖球", Order = 13)]
        public bool Trap { get; set; } = true;
        [JsonProperty("是否放飞镖", Order = 13)]
        public bool Dart { get; set; } = true;

        [JsonProperty("设置刷怪率", Order = 15)]
        public bool SpawnRate { get; set; } = false;
        [JsonProperty("刷怪间隔/帧", Order = 15)]
        public int defaultSpawnRate { get; set; }
        [JsonProperty("刷怪上限/个", Order = 15)]
        public int defaultMaxSpawns { get; set; }
        [JsonProperty("直通车不覆盖地牢砖与丛林蜥蜴砖", Order = 16)]
        public bool DisableBlock { get; set; } = true;

        public ItemData3(ushort id, ushort id2, ushort platformID, int platformstyle, int tileX, int tileY, int width, int platformY, int width2, int height, int width3, int center, int deSpawn, int MaxSpawn, bool disableBlock)
        {
            this.Hell_BM_TileID = id;
            this.Cord_TileID = id2;
            this.PlatformID = platformID;
            this.PlatformStyle = platformstyle;
            this.SpawnTileX = tileX;
            this.SpawnTileY = tileY;
            this.HellTrunnelWidth = width;
            this.HellPlatformY = platformY;
            this.ShimmerBiomeTunnelWidth = width2;
            this.BrushMonstHeight = height;
            this.BrushMonstWidth = width3;
            this.BrushMonstCenter = center;
            this.defaultSpawnRate = deSpawn;
            this.defaultMaxSpawns = MaxSpawn;
            this.DisableBlock = disableBlock;
        }
    }
    #endregion

    #region 自建微光湖
    public class ItemData4
    {
        [JsonProperty("生成微光湖", Order = -9)]
        public bool SpawnShimmerBiome { get; set; } = false;

        [JsonProperty("出生点偏移X", Order = -3)]
        public int TileX { get; set; }

        [JsonProperty("出生点偏移Y", Order = -3)]
        public int TileY { get; set; }


        public ItemData4(int tileX, int tileY)
        {
            this.TileX = tileX;
            this.TileY = tileY;
        }
    }
    #endregion

    #region 预设参数方法
    public void Ints()
    {
        this.DisableBlock = new HashSet<int>{ 41, 43, 44,226, };

        this.Prison = new List<ItemData>
        {
            new ItemData(38,147,-4,10,36,80,43,27,18,13)
        };

        this.Chests = new List<ItemData1>
        {
            new ItemData1(-38,34,2,18,8,0,2,21,-1,19,43,318,395)
        };

        this.WorldPlatform = new List<ItemData2>
        {
            new ItemData2(19, 43, -100 ,35,150,50,270,200,30,true),
        };

        this.HellTunnel = new List<ItemData3>
        { 
            new ItemData3(38, 214, 19, 43, 0, 0,5,25,2,200,200,2,60,100,true),
        };

        this.SpawnShimmerBiome = new List<ItemData4>
        {
            new ItemData4(0, 200),
        };
    }
    #endregion

    #region 读取与创建配置文件方法
    public static readonly string FilePath = Path.Combine(TShock.SavePath, "生成基础建设.json");

    public void Write()
    {
        var json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(FilePath, json);
    }

    public static Configuration Read()
    {
        if (!File.Exists(FilePath))
        {
            var NewConfig = new Configuration();
            NewConfig.Ints();
            NewConfig.Write();
            return NewConfig;
        }
        else
        {
            var jsonContent = File.ReadAllText(FilePath);
            return JsonConvert.DeserializeObject<Configuration>(jsonContent)!;
        }
    }
    #endregion

}