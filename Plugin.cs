using System;
using Terraria;
using Terraria.Map;
using Terraria.Utilities;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using TShockAPI.Net;
using static SpawnInfra.Utils;

namespace SpawnInfra
{
    [ApiVersion(2, 1)]
    public class Plugin : TerrariaPlugin
    {
        #region 插件信息
        public override string Name => "生成基础建设";
        public override string Author => "羽学";
        public override Version Version => new Version(1, 6, 3);
        public override string Description => "给新世界创建NPC住房、箱子集群、洞穴刷怪场、地狱/微光直通车、地表和地狱世界级平台（轨道）";
        #endregion

        #region 注册与释放
        public Plugin(Main game) : base(game) { }
        private GeneralHooks.ReloadEventD _reloadHandler;
        public override void Initialize()
        {
            LoadConfig();
            this._reloadHandler = (_) => LoadConfig();
            GeneralHooks.ReloadEvent += this._reloadHandler;
            //提高优先级避免覆盖CreateSpawn插件
            ServerApi.Hooks.GamePostInitialize.Register(this, OnGamePostInitialize, 20);
            GetDataHandlers.PlayerUpdate.Register(this.PlayerUpdate);
            TShockAPI.Commands.ChatCommands.Add(new Command("spawninfra.use", Commands.command, "spi", "基建")
            {
                HelpText = "生成基础建设"
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeneralHooks.ReloadEvent -= this._reloadHandler;
                ServerApi.Hooks.GamePostInitialize.Deregister(this, OnGamePostInitialize);
                GetDataHandlers.PlayerUpdate.UnRegister(this.PlayerUpdate);
                TShockAPI.Commands.ChatCommands.RemoveAll(x => x.CommandDelegate == Commands.command);
            }
            base.Dispose(disposing);
        }
        #endregion

        #region 配置重载读取与写入方法
        internal static Configuration Config = new();
        private static void LoadConfig()
        {
            Config = Configuration.Read();
            Config.Write();
            TShock.Log.ConsoleInfo("[生成基础建设]重新加载配置完毕。");
        }
        #endregion

        #region  游戏初始化建筑设施（世界平台/轨道+地狱平台/轨道+出生点监狱+直通车、刷怪场、仓库、微光湖）
        private static void OnGamePostInitialize(EventArgs args)
        {
            if (args == null)
            {
                return;
            }

            //太空层
            var sky = Main.worldSurface * 0.3499999940395355;
            //临近地下层
            var surface = Main.worldSurface * 0.7;

            //当"开服自动基建"开启时
            if (Config.Enabled)
            {
                //查找“开服指令表”的命令
                foreach (var cmd in Config.CommandList)
                {
                    //用服务器执行这些命令
                    TShockAPI.Commands.HandleCommand(TSPlayer.Server, cmd);
                }

                //监狱
                foreach (var item in Config.Prison)
                {
                    GenLargeHouse(Main.spawnTileX + item.spawnTileX, Main.spawnTileY + item.spawnTileY, item.BigHouseWidth, item.BigHouseHeight);
                }


                //箱子
                foreach (var item in Config.Chests)
                {
                    SpawnChest(Main.spawnTileX + item.spawnTileX, Main.spawnTileY + item.spawnTileY, item.ClearHeight, item.ChestWidth, item.ChestCount, item.ChestLayers);
                }

                //自建微光湖
                foreach (var item in Config.SpawnShimmerBiome)
                {
                    if (item.SpawnShimmerBiome)
                    {
                        //天顶、颠倒以地狱为起点往上建微光湖
                        if (Main.zenithWorld || Main.remixWorld || !Main.tenthAnniversaryWorld)
                        {
                            WorldGen.ShimmerMakeBiome(Main.spawnTileX + item.TileX, Main.UnderworldLayer - item.TileY);
                        }

                        //普通地图按出生点为起点往下建
                        else
                        {
                            WorldGen.ShimmerMakeBiome(Main.spawnTileX + item.TileX, Main.spawnTileY + item.TileY); //坐标为出生点
                        }
                    }
                }

                //地狱直通车与刷怪场
                foreach (var item in Config.HellTunnel)
                {
                    //微光湖直通车 天顶
                    if (Main.zenithWorld)
                    {
                        ZenithShimmerBiome(item.ShimmerBiomeTunnelWidth, (int)sky - Config.WorldPlatform[0].WorldPlatformY);
                    }
                    //十周年
                    else if (Main.tenthAnniversaryWorld)
                    {
                        ZenithShimmerBiome(item.ShimmerBiomeTunnelWidth, Main.spawnTileY + Config.WorldPlatform[0].WorldPlatformY + 1);
                    }
                    //其他世界
                    else
                    {
                        ShimmerBiome(Main.spawnTileY + Config.WorldPlatform[0].WorldPlatformY + 1, item.ShimmerBiomeTunnelWidth);
                    }

                    if (Main.zenithWorld || Main.remixWorld) //是颠倒种子
                    {
                        if (Config.HellTunnel[0].HellTrunnelCoverBrushMonst)//直通车是否贯穿刷怪场
                        {
                            //刷怪场 = 地狱层 除一半 补上自定义高度 
                            RockTrialField((int)Main.rockLayer / 2 + item.BrushMonstHeight, Main.spawnTileX, item.BrushMonstHeight, item.BrushMonstWidth, item.BrushMonstCenter, item.ClearRegionEnabled);
                            //直通车 = 地狱 - 世界平台高度
                            HellTunnel(Main.spawnTileX + item.SpawnTileX, (int)sky - Config.WorldPlatform[0].WorldPlatformY, item.HellTrunnelWidth);
                        }
                        else //没啥不同就换个先后顺序
                        {
                            HellTunnel(Main.spawnTileX + item.SpawnTileX, (int)sky - Config.WorldPlatform[0].WorldPlatformY, item.HellTrunnelWidth);
                            RockTrialField((int)Main.rockLayer / 2 + item.BrushMonstHeight, Main.spawnTileX, item.BrushMonstHeight, item.BrushMonstWidth, item.BrushMonstCenter, item.ClearRegionEnabled);
                        }

                        //左海平台挖到地下层为止 避免失重不好打猪鲨
                        OceanPlatforms((int)surface,
                            Config.WorldPlatform[0].OceanPlatformWide,
                            Config.WorldPlatform[0].OceanPlatformHeight,
                            Config.WorldPlatform[0].OceanPlatformInterval,
                            Config.WorldPlatform[0].OceanPlatformInterval - 1,
                            Config.WorldPlatform[0].OceanPlatformFormSpwanXLimit);
                    }
                    else //普通世界
                    {
                        if (Config.HellTunnel[0].HellTrunnelCoverBrushMonst)
                        {
                            RockTrialField((int)Main.rockLayer, Main.spawnTileX, item.BrushMonstHeight, item.BrushMonstWidth, item.BrushMonstCenter, item.ClearRegionEnabled);
                            HellTunnel(Main.spawnTileX + item.SpawnTileX, Main.spawnTileY + item.SpawnTileY, item.HellTrunnelWidth);
                        }
                        else
                        {
                            HellTunnel(Main.spawnTileX + item.SpawnTileX, Main.spawnTileY + item.SpawnTileY, item.HellTrunnelWidth);
                            RockTrialField((int)Main.rockLayer, Main.spawnTileX, item.BrushMonstHeight, item.BrushMonstWidth, item.BrushMonstCenter, item.ClearRegionEnabled);
                        }

                        //不超过海平面就行
                        OceanPlatforms((int)sky,
                            Config.WorldPlatform[0].OceanPlatformWide,
                            Config.WorldPlatform[0].OceanPlatformHeight,
                            Config.WorldPlatform[0].OceanPlatformInterval,
                            Config.WorldPlatform[0].OceanPlatformInterval - 1,
                            Config.WorldPlatform[0].OceanPlatformFormSpwanXLimit);
                    }
                    //地狱平台
                    UnderworldPlatform(Main.UnderworldLayer + item.HellPlatformY, item.HellPlatformY);
                }

                //世界平台
                foreach (var item in Config.WorldPlatform)
                {
                    if (Main.zenithWorld || Main.remixWorld) //天顶 颠倒以天空层往下算
                    {
                        WorldPlatform((int)sky - item.WorldPlatformY, item.WorldPlatformClearY, item.WorldPlatformID);
                    }
                    else //正常世界从出生点往上算
                    {
                        WorldPlatform(Main.spawnTileY + item.WorldPlatformY, item.WorldPlatformClearY, item.WorldPlatformID);
                    }
                }

                TShock.Utils.Broadcast(
                    "基础建设已完成，如需重置布局\n" +
                    "请输入指令后重启服务器：/rm reset", 250, 247, 105);

                TShockAPI.Commands.HandleCommand(TSPlayer.Server, "/clear i 9999");
                TShock.Utils.SaveWorld();
                Configuration.Read();

                Config.Enabled = false;
                Config.Write();
            }
        }
        #endregion

        #region 提高刷怪场中的刷怪率方法
        private void PlayerUpdate(object? sender, GetDataHandlers.PlayerUpdateEventArgs e)
        {
            if (e == null || !Config.Enabled || !Config.HellTunnel[0].SpawnRate ||
                !Config.HellTunnel[0].BrushMonstEnabled || !Main.hardMode)
            {
                return;
            }

            if (Main.zenithWorld || Main.remixWorld)//颠倒种子
            {
                MonsterRegion(Main.rockLayer / 2 + Config.HellTunnel[0].BrushMonstHeight,
                    Config.HellTunnel[0].BrushMonstHeight, Config.HellTunnel[0].BrushMonstWidth, Config.HellTunnel[0].BrushMonstCenter);
            }

            else //普通世界
            {
                MonsterRegion(Main.rockLayer, Config.HellTunnel[0].BrushMonstHeight,
                    Config.HellTunnel[0].BrushMonstWidth, Config.HellTunnel[0].BrushMonstCenter);
            }
        }

        private static void MonsterRegion(double posY, int height, int width, int centerVal)
        {
            if (InRegion(posY, height, width, centerVal))
            {
                NPC.spawnRate = Config.HellTunnel[0].defaultSpawnRate;
                NPC.maxSpawns = Config.HellTunnel[0].defaultMaxSpawns;
            }
            else
            {
                NPC.spawnRate = TShock.Config.Settings.DefaultSpawnRate;
                NPC.maxSpawns = TShock.Config.Settings.DefaultMaximumSpawns;
            }
        }
        #endregion

        #region 判断所有玩家在刷怪区方法
        public static bool InRegion(double posY, int height, int width, int centerVal)
        {
            var region = (int)posY - height;
            var top = region + height * 2;
            var bottom = (int)posY + height * 2;
            var middle = (top + bottom) / 2 + centerVal;

            var centerTop = middle + 8 + centerVal - 11;
            var centerBottom = middle + 8 + centerVal + 3;
            var centerLeft = Main.spawnTileX - 8 - centerVal;
            var centerRight = Main.spawnTileX + 8 + centerVal;

            for (var i = 0; i < Main.maxPlayers; i++) //所有在线玩家
            {
                var plr = Main.player[i];
                if (plr.active)
                {
                    var plrX = (int)(plr.position.X / 16);
                    var plrY = (int)(plr.position.Y / 16);
                    if (!(plrX >= centerLeft && plrX <= centerRight
                        && plrY >= centerTop && plrY <= centerBottom))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        #endregion

        #region 刷怪场
        public static void RockTrialField(int posY, int posX, int Height, int Width, int CenterVal, bool ClearRegion)
        {
            if (!Config.HellTunnel[0].BrushMonstEnabled) return;

            var clear = posY - Height;
            // 计算顶部、底部和中间位置
            var top = clear + Height * 2;
            var bottom = posY + Height * 2;
            var middle = (top + bottom) / 2 + CenterVal;

            var left = Math.Max(posX - Width, 0);
            var right = Math.Min(posX + Width, Main.maxTilesX);

            var CenterLeft = posX - 8 - CenterVal;
            var CenterRight = posX + 8 + CenterVal;

            if (!ClearRegion)
            {
                for (var y = posY; y > clear; y--)
                {
                    for (var x = left; x < right; x++)
                    {
                        ClearEverything(x, y + Height * 2); // 清除方块

                        WorldGen.PlaceTile(x, top, Config.HellTunnel[0].Hell_BM_TileID, false, true, -1, 0); // 在清理顶部放1层（防液体流进刷怪场）

                        WorldGen.PlaceTile(x, bottom, Config.HellTunnel[0].Hell_BM_TileID, false, true, -1, 0); //刷怪场底部放1层

                        //放岩浆在底部禁止刷怪
                        if (Config.HellTunnel[0].Lava)
                        {
                            WorldGen.PlaceLiquid(x, bottom - 1, 1, 1); //岩浆高于底部一格
                        }

                        WorldGen.PlaceTile(x, middle, Config.HellTunnel[0].Hell_BM_TileID, false, true, -1, 0); //刷怪场中间放1层（刷怪用）

                        WorldGen.PlaceTile(x, middle + 8 + CenterVal, Config.HellTunnel[0].PlatformID, false, true, -1, Config.HellTunnel[0].PlatformStyle);//中间下8格放一层方便站人

                        for (var wallY = middle + 3; wallY <= middle + 7 + CenterVal; wallY++)
                        {
                            Main.tile[x, wallY].wall = 155; // 放置墙壁
                        }
                        //定刷怪区
                        for (var i = 1; i <= 3; i++)
                        {
                            for (var j = 61; j <= 83; j++)
                            {
                                WorldGen.PlaceWall(CenterLeft - j + 8 + CenterVal, middle + i, 164, false); // 左 62 - 84格刷怪区 放红玉晶莹墙警示
                                WorldGen.PlaceWall(CenterRight + j - 8 - CenterVal, middle + i, 164, false); // 右 62 - 84格刷怪区 放红玉晶莹墙警示
                            }
                        }
                        if (Main.tile[x, y].wall != Terraria.ID.WallID.None)
                        {
                            continue;
                        }
                        WorldGen.PlaceTile(x, middle + 11 + CenterVal, Config.HellTunnel[0].Hell_BM_TileID, false, true, -1, 0); //中间下11格放箱子的实体块

                        //如果箱子样式为-1则用随机样式
                        if (Config.Chests[0].ChestStyle == -1)
                        {
                            // 定义随机样式范围
                            List<int> valSty = new List<int>();
                            var LockSty = new List<int> { 2, 4, 36, 38, 40 }.Concat(Enumerable.Range(18, 10)).ToHashSet();
                            for (int j = 0; j <= 51; j++)
                            {
                                // 排除掉上锁的 或 环境6神器的宝箱
                                if (LockSty.Contains(j)) continue;
                                valSty.Add(j);
                            }

                            Random rand = new Random(); // 创建一个随机数生成器
                            int randStyle = valSty[rand.Next(valSty.Count)]; // 随机选择一个样式

                            // 放置随机样式的箱子
                            WorldGen.PlaceTile(x, middle + 10 + CenterVal, Config.Chests[0].ChestTileID, false, true, -1, randStyle);
                        }
                        else
                        {
                            // 放置指定样式的箱子
                            WorldGen.PlaceTile(x, middle + 10 + CenterVal, Config.Chests[0].ChestTileID, false, true, -1, Config.Chests[0].ChestStyle); //中间下10格放箱子
                        }

                        WorldGen.PlaceTile(x, middle + 2, Config.HellTunnel[0].Hell_BM_TileID, false, true, -1, 0); //放计时器的平台

                        WorldGen.PlaceTile(x, middle + 4, Config.HellTunnel[0].Hell_BM_TileID, false, true, -1, 0); //防掉怪下来

                        // 如果x值在中心范围内，放置10格高的方块
                        if (x >= CenterLeft && x <= CenterRight)
                        {
                            for (var wallY = middle - 10 - CenterVal; wallY <= middle - 1; wallY++)
                            {
                                // 创建矩形判断
                                if (wallY >= middle - 10 - CenterVal && wallY <= middle - 1 && x >= CenterLeft + 1 && x <= CenterRight - 1)
                                {
                                    // 挖空方块
                                    ClearEverything(x, wallY);
                                }
                                else
                                {
                                    // 在矩形范围外放置方块
                                    WorldGen.PlaceTile(x, wallY, Config.HellTunnel[0].Hell_BM_TileID, false, true, -1, 0);

                                    //放一层半砖平台让怪穿进中心区
                                    WorldGen.PlaceTile(CenterLeft - 1, wallY, 19, false, true, -1, 43);
                                    Main.tile[CenterLeft - 1, wallY].slope(2); // 设置为右斜坡
                                    WorldGen.PlaceTile(CenterRight + 1, wallY, 19, false, true, -1, 43);
                                    Main.tile[CenterRight + 1, wallY].slope(1); // 设置为左斜坡
                                }

                                // 检查是否在中间位置，如果是则放置岩浆
                                if (wallY == middle - 10 - CenterVal)
                                {
                                    //是否放岩浆
                                    if (Config.HellTunnel[0].Lava)
                                    {
                                        Main.tile[x - 1, wallY + 2].liquid = 60;  //设置1格液体
                                        Main.tile[x - 1, wallY + 2].liquidType(1); // 设置为岩浆
                                        WorldGen.SquareTileFrame(x - 1, wallY + 2, false);
                                    }

                                    //是否放尖球
                                    if (Config.HellTunnel[0].Trap)
                                    {
                                        //加一排尖球
                                        for (var j = 1; j <= 5 + CenterVal; j++)
                                        {
                                            //电线
                                            WorldGen.PlaceWire(x - j, wallY - j);
                                            WorldGen.PlaceWire(x + j, wallY - j);
                                            //尖球
                                            WorldGen.PlaceTile(x - j, wallY - j, 137, true, false, -1, 3);
                                            WorldGen.PlaceTile(x + j, wallY - j, 137, true, false, -1, 3);
                                            //给尖球虚化
                                            Main.tile[x - j, wallY - j].inActive(true);
                                            Main.tile[x + j, wallY - j].inActive(true);
                                        }
                                        //给尖球加电线
                                        for (var j = 2; j <= 10 + CenterVal; j++)
                                        {
                                            WorldGen.PlaceWire(CenterLeft - 1, middle - j);
                                            WorldGen.PlaceWire(CenterRight + 1, middle - j);
                                        }
                                    }
                                }
                            }
                        }
                        else //不在中心 左右生成半砖推怪平台
                        {
                            Main.tile[x, middle - 1].type = Config.HellTunnel[0].Hell_BM_TileID;
                            Main.tile[x, middle - 1].active(true);
                            Main.tile[x, middle - 1].halfBrick(false);

                            // 根据x值确定斜坡方向
                            if (x < Main.spawnTileX)
                            {
                                Main.tile[x, middle - 1].slope(3); // 设置为右斜坡
                                WorldGen.PlaceTile(x, middle, 421, false, true, -1, 0); //刷怪场中间左边放1层传送带（刷怪取物品用）
                            }
                            else
                            {
                                Main.tile[x, middle - 1].slope(4); // 设置为左斜坡
                                WorldGen.PlaceTile(x, middle, 422, false, true, -1, 0); //刷怪场中间右边放1层传送带（刷怪取物品用） 
                            }

                            // 把半砖替换成推怪平台
                            WorldGen.PlaceTile(x, middle - 1, Config.HellTunnel[0].PlatformID, false, true, -1, Config.HellTunnel[0].PlatformStyle);
                            //平台加电线+制动器
                            WorldGen.PlaceWire(x, middle - 1);
                            WorldGen.PlaceActuator(x, middle - 1);

                            // 在电线的末端放置1/4秒计时器并连接
                            WorldGen.PlaceWire(CenterLeft - 1, middle + 1);
                            WorldGen.PlaceWire(CenterRight + 1, middle + 1);
                            WorldGen.PlaceWire(CenterLeft - 1, middle);
                            WorldGen.PlaceWire(CenterRight + 1, middle);
                            WorldGen.PlaceTile(CenterLeft - 1, middle + 1, 144, false, true, -1, 4);
                            WorldGen.PlaceTile(CenterRight + 1, middle + 1, 144, false, true, -1, 4);

                            //在1/4秒计时器下面连接开关
                            for (int i = 2; i <= 5; i++)
                            {
                                WorldGen.PlaceWire(CenterLeft - 1, middle + i);
                                WorldGen.PlaceWire(CenterRight + 1, middle + i);
                                WorldGen.PlaceTile(CenterLeft - 1, middle + 5, 136, false, true, -1, 0);
                                WorldGen.PlaceTile(CenterRight + 1, middle + 5, 136, false, true, -1, 0);
                            }

                            //左边 放篝火、影烛、水蜡烛、花园侏儒、心灯、巴斯特雕像
                            WorldGen.PlaceObject(CenterLeft - 2, middle + 7 + CenterVal, 215, false, 0, 0, -1, -1);
                            WorldGen.PlaceObject(CenterLeft - 4, middle + 7 + CenterVal, 646, false, 0, 0, -1, -1);
                            WorldGen.PlaceObject(CenterLeft - 5, middle + 7 + CenterVal, 49, false, 0, 0, -1, -1);
                            WorldGen.PlaceGnome(CenterLeft - 6, middle + 7 + CenterVal, 0);
                            WorldGen.PlaceObject(CenterLeft - 6, middle + 3 + CenterVal, 42, false, 9, 0, -1, -1);
                            WorldGen.PlaceObject(CenterLeft - 8, middle + 7 + CenterVal, 506, false, 0, 0, -1, -1);

                            //右边 放环境火把
                            for (int j = 5; j <= 23; j++)
                                WorldGen.PlaceObject(CenterRight + j, middle + 3 + CenterVal, 4, false, j, -1, -1);

                            //是否放飞镖
                            if (Config.HellTunnel[0].Dart)
                            {
                                //每30格 放一个 一共6个
                                for (int k = 1; k <= 6; k++)
                                {
                                    //给电线从中心y0拉线到下位3
                                    for (int l = 0; l <= 3; l++)
                                    {
                                        //输出1 3 忽略偶数（刷怪层为0 ，下1和3格放飞镖）
                                        for (int m = 1; m <= 3; m += 2)
                                        {
                                            //右飞镖
                                            WorldGen.PlaceTile(CenterRight + 30 * k, middle + m, 137, false, true, -1, 5);
                                            //电线
                                            WorldGen.PlaceWire(CenterRight + 30 * k, middle + l);
                                            //把飞镖机关虚化
                                            Main.tile[CenterRight + 30 * k, middle + m].inActive(true);

                                            //左飞镖
                                            WorldGen.PlaceTile(CenterLeft - 30 * k, middle + m, 137, false, true, -1, 5);
                                            //给飞镖换个方向
                                            ITile obj = Main.tile[CenterLeft - 30 * k, middle + m];
                                            obj.frameX = 18;
                                            //放电线
                                            WorldGen.PlaceWire(CenterLeft - 30 * k, middle + l);
                                            //把飞镖机关虚化
                                            Main.tile[CenterLeft - 30 * k, middle + m].inActive(true);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // 左右各放一列把刷怪场封闭起来
                for (int y2 = top; y2 < bottom + 1; y2++)
                {
                    WorldGen.PlaceTile(left - 1, y2, Config.HellTunnel[0].Hell_BM_TileID, false, true, -1, 0);
                    WorldGen.PlaceTile(right, y2, Config.HellTunnel[0].Hell_BM_TileID, false, true, -1, 0);

                    //是否放置飞镖
                    if (Config.HellTunnel[0].Dart)
                    {
                        // 避免核心区放置边界飞镖
                        var placeTop = middle + 8 + CenterVal - 11; // 平台上方11格开始
                        var placeBottom = middle + 8 + CenterVal + 3; // 平台下方3格结束
                        if (y2 < placeTop || y2 > placeBottom)
                        {
                            //右飞镖
                            WorldGen.PlaceTile(right - 1, y2, 137, false, true, -1, 5);
                            //左飞镖
                            WorldGen.PlaceTile(left, y2, 137, false, true, -1, 5);
                            //给左飞镖换朝向
                            ITile obj = Main.tile[left, y2];
                            obj.frameX = 18;
                            //把飞镖机关虚化
                            Main.tile[right - 1, y2].inActive(true);
                            Main.tile[left, y2].inActive(true);
                        }
                        //放电线
                        WorldGen.PlaceWire(right - 1, y2);
                        WorldGen.PlaceWire(left, y2);
                    }
                }
            }
            //只清场地不建刷怪场 帮定中心点与刷怪范围
            else
            {
                for (int y = posY; y > clear; y--)
                {
                    for (int x = left; x < right; x++)
                    {
                        ClearEverything(x, y + Height * 2); // 清除方块

                        WorldGen.PlaceTile(x, top, Config.HellTunnel[0].Hell_BM_TileID, false, true, -1, 0); // 在清理顶部放1层（防液体流进刷怪场）

                        WorldGen.PlaceTile(x, bottom, Config.HellTunnel[0].Hell_BM_TileID, false, true, -1, 0); //刷怪场底部放1层

                        //放岩浆禁止刷怪
                        if (Config.HellTunnel[0].Lava)
                        {
                            WorldGen.PlaceLiquid(x, bottom - 1, 1, 1);  //岩浆高于底部一格
                        }

                        //定中心
                        WorldGen.PlaceTile(CenterLeft, middle, 267, false, true, -1, 0);
                        WorldGen.PlaceTile(CenterRight, middle, 267, false, true, -1, 0);

                        //定刷怪区
                        for (int i = 1; i <= 3; i++)
                        {
                            for (int j = 61; j <= 83; j++)
                            {
                                WorldGen.PlaceWall(CenterLeft - 60 + 8 + CenterVal, middle + i, 155, false); // 左 61格 放钻石晶莹墙
                                WorldGen.PlaceWall(CenterLeft - j + 8 + CenterVal, middle + i, 164, false); // 左 62 - 84格刷怪区 放红玉晶莹墙警示
                                WorldGen.PlaceWall(CenterLeft - 84 + 8 + CenterVal, middle + i, 155, false); // 左 85格 放钻石晶莹墙

                                WorldGen.PlaceWall(CenterRight + 60 - 8 - CenterVal, middle + i, 155, false);  //  右 61格 放钻石晶莹墙
                                WorldGen.PlaceWall(CenterRight + j - 8 - CenterVal, middle + i, 164, false); // 右 62 - 84格刷怪区 放红玉晶莹墙警示
                                WorldGen.PlaceWall(CenterRight + 84 - 8 - CenterVal, middle + i, 155, false); // 右 85格 放钻石晶莹墙
                            }
                        }
                    }
                }

                // 左右各放一列把刷怪场封闭起来
                for (int y2 = top; y2 < bottom + 1; y2++)
                {
                    WorldGen.PlaceTile(left - 1, y2, Config.HellTunnel[0].Hell_BM_TileID, false, true, -1, 0);
                    WorldGen.PlaceTile(right, y2, Config.HellTunnel[0].Hell_BM_TileID, false, true, -1, 0);
                }
            }
        }
        #endregion

        #region 构造监狱集群方法
        public static void GenLargeHouse(int startX, int startY, int width, int height)
        {
            if (!Config.Prison[0].BigHouseEnabled) return;

            int roomsAcross = width / 5;
            int roomsDown = height / 9;

            for (int row = 0; row < roomsDown; row++)
            {
                for (int col = 0; col < roomsAcross; col++)
                {
                    bool isRight = col == roomsAcross;
                    GenRoom(startX + col * 5, startY + row * 9, isRight);
                }
            }
        }
        #endregion

        #region 创建小房间方法
        public static void GenRoom(int posX, int posY, bool isRight = true)
        {
            RoomTheme roomTheme = new RoomTheme();
            roomTheme.SetGlass();
            ushort tile = roomTheme.tile;
            ushort wall = roomTheme.wall;
            TileInfo platform = roomTheme.platform;
            TileInfo chair = roomTheme.chair;
            TileInfo bench = roomTheme.bench;
            TileInfo torch = roomTheme.torch;

            int num = posX;
            int num2 = 6;
            int num3 = 10;
            if (!isRight)
            {
                num += 6;
            }

            for (int i = num; i < num + num2; i++)
            {
                for (int j = posY - num3; j < posY; j++)
                {
                    ITile val = Main.tile[i, j];
                    ClearEverything(i, j);
                    if (i > num && j < posY - 5 && i < num + num2 - 1 && j > posY - num3)
                    {
                        val.wall = wall;
                    }
                    if (i == num && j > posY - 5 || i == num + num2 - 1 && j > posY - 5 || j == posY - 1)
                    {
                        WorldGen.PlaceTile(i, j, platform.id, false, true, -1, platform.style);
                    }
                    else if (i == num || i == num + num2 - 1 || j == posY - num3 || j == posY - 5)
                    {
                        val.type = tile;
                        val.active(true);
                        val.slope(0);
                        val.halfBrick(false);
                    }
                }
            }
            if (isRight)
            {
                WorldGen.PlaceTile(num + 1, posY - 6, chair.id, false, true, 0, chair.style);
                ITile obj = Main.tile[num + 1, posY - 6];
                obj.frameX += 18;
                ITile obj2 = Main.tile[num + 1, posY - 7];
                obj2.frameX += 18;
                WorldGen.PlaceTile(num + 2, posY - 6, bench.id, false, true, -1, bench.style);
                WorldGen.PlaceTile(num + 4, posY - 5, torch.id, false, true, -1, torch.style);
            }
            else
            {
                WorldGen.PlaceTile(num + 4, posY - 6, chair.id, false, true, 0, chair.style);
                WorldGen.PlaceTile(num + 2, posY - 6, bench.id, false, true, -1, bench.style);
                WorldGen.PlaceTile(num + 1, posY - 5, torch.id, false, true, -1, torch.style);
            }

            // 通知玩家更新地图并结束任务锁
            // TileHelper.GenAfter();
        }
        #endregion

        #region 箱子集群+物品框方法
        public static void SpawnChest(int posX, int posY, int hight, int width, int count, int layers)
        {
            if (!Config.Chests[0].SpawnChestEnabled) return;

            // 如果启用了墙壁，则每层多预留3格空间
            int spacing = Config.Chests[0].WallAndItemFrame ? 3 : 0;
            int totalLayerHeight = width + Config.Chests[0].LayerHeight + spacing;

            int ClearHeight = hight + (layers - 1) * totalLayerHeight;

            for (int x = posX; x < posX + width * count; x++)
                for (int y = posY - ClearHeight; y <= posY; y++)
                    ClearEverything(x, y);


            for (int layer = 0; layer < layers; layer++)
            {
                for (int i = 0; i < count; i++)
                {
                    int currentXPos = posX + i * width;
                    int currentYPos = posY - (layer * totalLayerHeight);

                    // 先放置平台
                    for (int wx = currentXPos; wx < currentXPos + width; wx++)
                    {
                        WorldGen.PlaceTile(wx, currentYPos + 1, Config.Chests[0].ChestPlatformID, false, true, -1, Config.Chests[0].ChestPlatformStyle);
                    }

                    //如果箱子样式为-1则用随机样式
                    if (Config.Chests[0].ChestStyle == -1)
                    {
                        // 定义随机样式范围
                        List<int> valSty = new List<int>();
                        var LockSty = new List<int> { 2, 4, 36, 38, 40 }.Concat(Enumerable.Range(18, 10)).ToHashSet();
                        for (int j = 0; j <= 51; j++)
                        {
                            // 排除掉上锁的 或 环境6神器的宝箱
                            if (LockSty.Contains(j)) continue;
                            valSty.Add(j);
                        }

                        Random rand = new Random(); // 创建一个随机数生成器
                        int randStyle = valSty[rand.Next(valSty.Count)]; // 随机选择一个样式

                        // 放置随机样式的箱子
                        WorldGen.PlaceTile(currentXPos, currentYPos, Config.Chests[0].ChestTileID, false, true, -1, randStyle);
                    }
                    else
                    {
                        // 放置指定样式的箱子
                        WorldGen.PlaceTile(currentXPos, currentYPos, Config.Chests[0].ChestTileID, false, true, -1, Config.Chests[0].ChestStyle);
                    }

                    // 如果启用了墙壁和物品框，则在上方添加
                    if (Config.Chests[0].WallAndItemFrame)
                    {
                        // 在箱子正上方 3 格空间中全部放满墙（仅当前箱子占用的 X 坐标）
                        for (int wx = currentXPos; wx < currentXPos + width; wx++)
                        {
                            for (int wy = currentYPos; wy >= currentYPos - spacing; wy--)
                            {
                                //再清一遍树
                                ClearEverything(wx, currentYPos - spacing);
                                // 铺满墙
                                WorldGen.PlaceWall(wx, wy, Config.Chests[0].WallID, false);
                            }
                        }

                        // 物品框（顶部）
                        for (int wx = currentXPos; wx < currentXPos + width; wx++)
                        {
                            //检查上面一层是否是墙
                            if (Main.tile[wx, currentYPos - spacing].wall == Config.Chests[0].WallID)
                            {
                                //放置物品框
                                WorldGen.PlaceObject(wx, currentYPos - spacing, Config.Chests[0].ItemFrameID, false, 4, -1, -1);

                                // 如果是物品框则激活它
                                if (Main.tile[wx, currentYPos - spacing].type == Config.Chests[0].ItemFrameID)
                                {
                                    Main.tile[wx, currentYPos - spacing].active(true);
                                    Terraria.GameContent.Tile_Entities.TEItemFrame.Place(wx, currentYPos - spacing);
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region 左海平台
        private static void OceanPlatforms(int sky, int wide, int height, int interval, int IntlClear, int Radius)
        {
            if (!Config.WorldPlatform[0].OceanPlatformEnabled) return;

            int clear = Math.Max(0, height);

            for (int y = Main.oceanBG; y < sky + clear; y += interval)
            {
                for (int top = y - IntlClear; top < y; top++)
                {
                    for (int x = 0; x < wide; x++)
                    {
                        // 停止放置平台的条件
                        if (x >= Main.maxTilesX || x < 0 || y < sky) continue;
                        if (Math.Abs(x - Main.spawnTileX) <= Radius) continue;

                        // 清理方块和墙（保留液体方法）
                        WorldGen.KillTile(x, y);
                        WorldGen.KillWall(x, y);
                        WorldGen.KillTile(x, top);
                        WorldGen.KillWall(x, top);

                        // 正常放置平台
                        WorldGen.PlaceTile(x, y, Config.WorldPlatform[0].WorldPlatformID, false, true, -1, Config.WorldPlatform[0].WorldPlatformStyle);

                        //是否让平台生成液体（怕海水太多把平台阉了）
                        if (Config.WorldPlatform[0].PlaceLiquid)
                        {
                            //从每层平台的位置生成水 避免原来的海水下落体积过大游戏引擎过载（液体碰撞就会流动了）
                            WorldGen.PlaceLiquid(x, y, 0, 255);
                        }

                        // 开启实体块包围左海平台
                        if (Config.WorldPlatform[0].Enclose)
                        {
                            // 清理顶部上面一层
                            for (int j = 1; j < clear; j++)
                            {
                                if (sky - clear + height - j >= 0 && sky - clear + height - j < Main.maxTilesY)
                                {
                                    Main.tile[x, sky - clear + height - j].ClearTile();
                                }
                            }

                            // 放置左海平台顶部格栅
                            if (sky - clear + height >= 0 && sky - clear + height < Main.maxTilesY)
                            {
                                WorldGen.PlaceTile(x, sky - clear + height, 546, false, false, -1, 0);
                            }

                            // 清理底部上面一层
                            for (int j = 1; j < IntlClear; j++)
                            {
                                if (sky + clear - j >= 0 && sky + clear - j < Main.maxTilesY)
                                {
                                    Main.tile[x, sky + clear - j].ClearEverything();
                                }
                            }

                            // 放置左海平台底部
                            if (sky + clear >= 0 && sky + clear < Main.maxTilesY)
                            {
                                WorldGen.PlaceTile(x, sky + clear, 38, false, true, -1, 0);
                            }

                            // 放置左边一列特定方块
                            int leftX = 0;
                            int leftY = x + y - IntlClear;
                            if (leftY >= 0 && leftY < Main.maxTilesY)
                            {
                                WorldGen.PlaceTile(leftX, leftY, Config.HellTunnel[0].Hell_BM_TileID, false, true, -1, 0);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region 战斗平台（指令方法）
        public static void FightPlatforms(int posX, int posY, int wide, int height, int interval, int type)
        {
            var sky = Main.worldSurface * 0.3499999940395355;

            // 计算整个矩形区域的边界
            int top = posY - height; // 矩形顶部
            int bottom = posY;        // 矩形底部
            int left = posX - wide / 2; // 矩形左边界
            int right = posX + wide / 2; // 矩形右边界

            // 1. 先清理整个矩形区域
            for (int y = top; y <= bottom; y++)
            {
                // 顶部禁止进入太空层
                if (y < sky) continue;

                for (int x = left; x <= right; x++)
                {
                    // 禁止放置在某些方块上
                    if (Config.WorldPlatform[0].DisableBlock)
                    {
                        if (Config.DisableBlock.Contains(Main.tile[x, y].type))
                        {
                            TSPlayer.All.SendInfoMessage($"[战斗平台] {x}, {y} 位置遇到禁止方块，已停止放置平台。");
                            return;
                        }
                    }

                    // 清除当前位置方块
                    ClearEverything(x, y);
                }
            }

            // 2. 在矩形区域内放置平台（每隔interval高度）
            for (int y = top; y <= bottom; y += interval)
            {
                // 顶部禁止进入太空层
                if (y < sky) continue;

                // 在水平范围内生成平台
                for (int x = left; x <= right; x++)
                {
                    // 禁止放置在某些方块上
                    if (Config.WorldPlatform[0].DisableBlock)
                    {
                        if (Config.DisableBlock.Contains(Main.tile[x, y].type))
                        {
                            TSPlayer.All.SendInfoMessage($"[战斗平台] {x}, {y} 位置遇到禁止方块，已停止放置平台。");
                            return;
                        }
                    }

                    // 放置平台
                    WorldGen.PlaceTile(x, y, type, false, true, -1);
                }

                // 3. 在平台上方一格放置篝火（每隔30格）
                int campfireY = y - 1; // 平台上一格
                if (campfireY < sky) continue; // 篝火也不能在太空层
                // 计算起始位置（确保左右对称）
                int startX = left + 30; // 从左侧起第30格开始放置
                for (int x = startX; x <= right; x += 60) // 每60格放一个
                {
                    // 确保在平台范围内
                    if (x < left || x > right) continue;

                    // 禁止放置在某些方块上
                    if (Config.WorldPlatform[0].DisableBlock)
                    {
                        if (Config.DisableBlock.Contains(Main.tile[x, campfireY].type))
                        {
                            TSPlayer.All.SendInfoMessage($"[战斗平台] {x}, {campfireY} 位置遇到禁止方块，已停止放置篝火。");
                            return;
                        }
                    }

                    // 清除篝火位置原有方块
                    ClearEverything(x, campfireY);
                    // 放置篝火
                    WorldGen.PlaceTile(x, campfireY, 215, false, true, -1);
                    // 放置猫雕 
                    WorldGen.PlaceTile(x - 3, campfireY, 506, false, true, -1);
                }
            }
        }
        #endregion

        #region 世界平台
        public static void WorldPlatform(int posY, int hight, int type)
        {
            int clear = Math.Max(3, hight);

            for (int x = 0; x < Main.maxTilesX; x++)
            {
                if (Config.WorldPlatform[0].OceanPlatformEnabled)
                {
                    if (x - clear <= Main.oceanBG + Config.WorldPlatform[0].WorldPlatformFromOceanLimit) continue;
                }

                for (int y = posY - clear; y <= posY; y++)
                {
                    if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY) continue;
                    if (Config.WorldPlatform[0].DisableBlock)
                    {
                        if (Config.DisableBlock.Contains(Main.tile[x, y].type))
                        {
                            TSPlayer.All.SendInfoMessage($"[世界平台] {x}, {y} 位置遇到禁止方块，已停止放置平台。");
                            return;
                        }
                    }
                    ClearEverything(x, y); // 清除方块和墙壁
                }

                if (Config.WorldPlatform[0].WorldPlatformEnabled)
                    WorldGen.PlaceTile(x, posY, type, false, true, -1, Config.WorldPlatform[0].WorldPlatformStyle);
                if (Config.WorldPlatform[0].WorldTrackEnabled)
                    WorldGen.PlaceTile(x, posY - 1, 314, false, true, -1, 0);
            }
        }
        #endregion

        #region 地狱直通车
        public static int HellTunnel(int posX, int posY, int Width)
        {
            var hell = 0;

            if (Config.HellTunnel[0].HellTunnelEnabled)
            {
                int xtile;

                for (hell = Main.UnderworldLayer + 10; hell <= Main.maxTilesY - 100; hell++)
                {
                    xtile = posX;
                    Parallel.For(posX, posX + 8, delegate (int cwidth, ParallelLoopState state)
                    {
                        if (Main.tile[cwidth, hell].active() && !Main.tile[cwidth, hell].lava())
                        {
                            state.Stop();
                            xtile = cwidth;
                        }
                    });
                    if (!Main.tile[xtile, hell].active())
                    {
                        break;
                    }
                }
                var num = hell;
                var Xstart = posX - 2;
                Parallel.For(Xstart, Xstart + Width, delegate (int cx)
                {
                    Parallel.For(posY, hell, delegate (int cy)
                    {
                        var tile = Main.tile[cx, cy];
                        if (Config.HellTunnel[0].DisableBlock)
                        {
                            if (Config.DisableBlock.Contains(tile.type))
                            {
                                TSPlayer.All.SendInfoMessage($"[地狱直通车] {cx}, {cy} 位置遇到禁止方块，已停止放置平台。");
                                return;
                            }
                        }
                        ClearEverything(cx, cy);
                        if (cx == Xstart + Width / 2)
                        {
                            tile.type = Config.HellTunnel[0].Cord_TileID; //绳子
                            tile.active(true);
                            tile.slope(0);
                            tile.halfBrick(false);
                        }
                        else if (cx == Xstart || cx == Xstart + Width - 1)
                        {
                            tile.type = Config.HellTunnel[0].Hell_BM_TileID; //边界方块
                            tile.active(true);
                            tile.slope(0);
                            tile.halfBrick(false);
                        }
                    });
                });

                var platformStart = Xstart + 1;
                var platformEnd = Xstart + Width - 2;
                //确保平台与直通车等宽
                for (var px = platformStart; px <= platformEnd; px++)
                {
                    if (Config.HellTunnel[0].DisableBlock)
                    {
                        if (Config.DisableBlock.Contains(Main.tile[px, posY].type))
                        {
                            TSPlayer.All.SendInfoMessage($"[地狱直通车] {px}, {posY} 位置遇到禁止方块，已停止放置平台。");
                            continue;
                        }
                    }
                    WorldGen.PlaceTile(px, posY, Config.HellTunnel[0].PlatformID, false, true, -1, Config.HellTunnel[0].PlatformStyle);
                    for (var cy = posY + 1; cy <= hell; cy++)
                    {
                        Main.tile[px, cy].wall = 155; // 放置墙壁
                    }
                }
            }
            return hell;
        }
        #endregion

        #region 地狱平台
        private static void UnderworldPlatform(int posY, int hight)
        {
            var Clear = posY - hight;
            for (var y = posY; y > Clear; y--)
            {
                for (var x = 0; x < Main.maxTilesX; x++)
                {
                    ClearEverything(x, y); // 清除方块

                    if (Config.HellTunnel[0].HellPlatformEnabled)
                        WorldGen.PlaceTile(x, posY, Config.HellTunnel[0].PlatformID, false, true, -1, Config.HellTunnel[0].PlatformStyle); //地狱平台

                    if (Config.HellTunnel[0].HellTrackEnabled)
                        WorldGen.PlaceTile(x, posY - 1, 314, false, true, -1, 0); //地狱轨道
                }
            }
        }
        #endregion

        #region 普通世界微光湖直通车
        private static void ShimmerBiome(int spawnY, int Width)
        {
            //开启出生点微光湖则返回
            if (!Config.HellTunnel[0].ShimmerBiomeTunnelEnabled || Config.SpawnShimmerBiome[0].SpawnShimmerBiome) { return; }

            //西江的判断微光湖位置方法
            var skipTile = new bool[Main.maxTilesX, Main.maxTilesY];
            for (var x = 0; x < Main.maxTilesX; x++)
            {
                for (var y = 0; y < Main.maxTilesY; y++)
                {
                    var tile = Main.tile[x, y];
                    if (tile is null || skipTile[x, y])
                    {
                        continue;
                    }
                    if (tile.shimmer())
                    {
                        var Right = x;
                        var Bottom = y;
                        for (var right = x; right < Main.maxTilesX; right++)
                        {
                            if (!Main.tile[right, y].shimmer())
                            {
                                Right = right;
                                break;
                            }
                        }

                        for (var right = x; right < Right; right++)
                        {
                            for (var bottom = y; bottom < Main.maxTilesY; bottom++)
                            {
                                if (!Main.tile[right, bottom].shimmer())
                                {
                                    if (bottom > Bottom)
                                    {
                                        Bottom = bottom;
                                    }
                                    break;
                                }
                            }
                        }

                        for (var start = x - 2; start < Right + 2; start++)
                        {
                            for (var end = y; end < Bottom + 2; end++)
                            {
                                skipTile[start, end] = true;
                            }
                        }

                        #region 微光湖直通车
                        // 找到微光湖的中心点
                        var CenterX = (x + Right) / 2;
                        //深度到为中心湖面
                        var CenterY = (y + Bottom) / 2 - 8;

                        // 从微光湖中心点向上挖通道直至地表
                        for (var TunnelY = CenterY; TunnelY >= spawnY; TunnelY--)
                        {
                            for (var TunnelX = CenterX - Width; TunnelX <= CenterX + Width; TunnelX++)
                            {
                                if (TunnelX >= 0 && TunnelX < Main.maxTilesX)
                                {
                                    Main.tile[TunnelX, TunnelY].ClearEverything();
                                    //直通车的两侧的方块
                                    if (TunnelX == CenterX - Width || TunnelX == CenterX + Width)
                                    {
                                        WorldGen.PlaceTile(TunnelX, TunnelY, Config.HellTunnel[0].Hell_BM_TileID, false, true, -1, 0);
                                    }

                                    //微光湖底部放一层雨云 防摔
                                    else if (TunnelY == CenterY)
                                    {
                                        WorldGen.PlaceTile(TunnelX, TunnelY, 460, false, true, -1, 0);
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                }
            }
        }
        #endregion

        #region 天顶微光直通车
        private static int MaxTunnels = 1; // 全局最大隧道数
        private static int CurrentTunnels = 0; // 当前已创建的隧道数
        private static void ZenithShimmerBiome(int Width, int Height)
        {
            //开启出生点微光湖则返回
            if (!Config.HellTunnel[0].ShimmerBiomeTunnelEnabled || CurrentTunnels >= MaxTunnels || Config.SpawnShimmerBiome[0].SpawnShimmerBiome) return;

            const int MinLakeSize = 200; // 只为大于等于200个瓷砖的微光湖创建隧道
            var skipTile = new bool[Main.maxTilesX, Main.maxTilesY];
            List<Tuple<int, int>> ConedLakes = new List<Tuple<int, int>>();
            int[] labelMap = new int[Main.maxTilesX * Main.maxTilesY]; // 用于标记连通组件

            // 使用连通组件分析检测微光湖
            for (var x = 0; x < Main.maxTilesX; x++)
                for (var y = 0; y < Main.maxTilesY; y++)
                {
                    if (Main.tile[x, y]?.shimmer() == true && !skipTile[x, y])
                    {
                        // 发现新的微光湖，开始标记
                        var label = ConedLakes.Count + 1;
                        FloodFill(x, y, label, ref labelMap, ref skipTile);
                        ConedLakes.Add(Tuple.Create(x, y));
                    }
                }

            // 从连通组件中选择最大的微光湖
            Tuple<int, int> SCenter = null!;
            foreach (var lake in ConedLakes)
            {
                var label = ConedLakes.IndexOf(lake) + 1;
                var size = CountSize(labelMap, label);
                if (size >= MinLakeSize)
                {
                    // 更新选定的微光湖中心点
                    if (SCenter == null || size > CountSize(labelMap, ConedLakes.FindIndex(l => l.Equals(SCenter)) + 1))
                    {
                        SCenter = lake;
                    }
                }
            }

            // 创建隧道
            if (SCenter != null)
            {
                CreateTunnel(SCenter.Item1, SCenter.Item2, Width, Height);
                CurrentTunnels++;
            }
        }



        //大量填充，根据邻近点合并，统计连通区域的大小（主打一个扫雷）
        private static void FloodFill(int x, int y, int label, ref int[] labelMap, ref bool[,] skipTile)
        {
            if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY || !Main.tile[x, y].shimmer() || skipTile[x, y])
                return;

            var index = y * Main.maxTilesX + x;
            labelMap[index] = label;
            skipTile[x, y] = true;

            FloodFill(x + 1, y, label, ref labelMap, ref skipTile);
            FloodFill(x - 1, y, label, ref labelMap, ref skipTile);
            FloodFill(x, y + 1, label, ref labelMap, ref skipTile);
            FloodFill(x, y - 1, label, ref labelMap, ref skipTile);
        }

        //计数大小
        private static int CountSize(int[] labelMap, int label)
        {
            var count = 0;
            for (int i = 0; i < labelMap.Length; i++)
            {
                if (labelMap[i] == label)
                    count++;
            }
            return count;
        }

        //创建隧道
        private static void CreateTunnel(int CenterX, int CenterY, int Width, int Height)
        {
            var CenterYOffset = -8; // 调整到微光湖中心偏上的位置

            // 开始挖掘隧道
            for (var y = CenterY + CenterYOffset; y >= Height; y--)
            {
                for (var x = CenterX - Width; x <= CenterX + Width; x++)
                {
                    if (x >= 0 && x < Main.maxTilesX)
                    {
                        Main.tile[x, y].ClearEverything();
                        if (x == CenterX - Width || x == CenterX + Width)
                            WorldGen.PlaceTile(x, y, Config.HellTunnel[0].Hell_BM_TileID, false, true, -1, 0); // 两侧放置特殊方块
                        else if (y == CenterY + CenterYOffset)
                            WorldGen.PlaceTile(x, y, 460, false, true, -1, 0); // 底部放置雨云
                    }
                }
            }
        }
        #endregion

        #region 鱼池（指令方法）
        public static void GenPond(int posX, int posY, int style)
        {
            PondTheme pondTheme = new PondTheme();
            switch (style)
            {
                case 1:
                    pondTheme.SetObsidian();
                    break;
                case 2:
                    pondTheme.SetHoney();
                    break;
                case 3:
                    pondTheme.SetGray();
                    break;
                default:
                    pondTheme.SetGlass();
                    break;
            }

            ushort tile = pondTheme.tile;
            TileInfo platform = pondTheme.platform;
            int num = posX - 6;
            int num2 = 13;
            int num3 = 32;

            for (int i = num; i < num + num2; i++)
            {
                for (int j = posY; j < posY + num3; j++)
                {
                    ITile val = Main.tile[i, j];
                    val.ClearEverything();
                    if (i == num || i == num + num2 - 1 || j == posY + num3 - 1)
                    {
                        val.type = tile;
                        val.active(true);
                        val.slope(0);
                        val.halfBrick(false);
                    }
                }
                WorldGen.PlaceTile(i, posY, platform.id, false, true, -1, platform.style);
            }

            for (int k = num + 1; k < num + num2 - 1; k++)
            {
                for (int l = posY + 1; l < posY + num3 - 1; l++)
                {
                    ITile val2 = Main.tile[k, l];
                    val2.active(false);
                    val2.liquid = byte.MaxValue;
                    switch (style)
                    {
                        case 1:
                            val2.lava(true);
                            break;
                        case 2:
                            val2.honey(true);
                            break;
                        case 3:
                            val2.shimmer(true);
                            break;
                    }
                }
            }
        }
        #endregion

    }
}
