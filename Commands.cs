using TShockAPI;
using Terraria;
using Microsoft.Xna.Framework;
using static SpawnInfra.Plugin;
using static MonoMod.InlineRT.MonoModRule;
using System.ComponentModel;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;

namespace SpawnInfra;

internal class Commands
{
    public static async void command(CommandArgs args)
    {
        TSPlayer plr = args.Player;

        //随机颜色
        Random rand = new Random();
        int r = rand.Next(150, 256);
        int g = rand.Next(150, 256);
        int b = rand.Next(150, 256);
        var color = new Color(r, g, b);

        if (args.Parameters.Count >= 0)
        {
            plr.SendInfoMessage("\n《生成基建》 [i:3456][C/F2F2C7:插件开发] [C/BFDFEA:by] [c/00FFFF:羽学][i:3459]");

            int Page = 1; // 默认第一页
            if (args.Parameters.Count >= 1 && int.TryParse(args.Parameters[0], out int Pages))
            {
                Page = Pages;
            }

            var result = GetCommandsPage(plr, Page);
            var show = result.Item1;
            int all = result.Item2;

            foreach (var cmd in show)
            {
                plr.SendMessage(cmd, color);
            }

            if (Page < all)
            {
                plr.SendInfoMessage($"/spi {Page + 1} 下一页");
            }
            if (Page > 1)
            {
                plr.SendInfoMessage($"/spi {Page - 1} 上一页");
            }

            plr.SendInfoMessage($"当前 {Page} 页,共计 {all} 页\n");
        }

        if (args.Parameters.Count >= 1)
        {
            if (NeedWaitTask()) return;

            switch (args.Parameters[0].ToLower())
            {
                case "s":
                case "set":
                case "选择":
                    {
                        if (NeedInGame()) return;
                        if (args.Parameters.Count < 2)
                        {
                            plr.SendInfoMessage("使用方法: /spi s 1 放置或挖掘左上角方块");
                            plr.SendInfoMessage("使用方法: /spi s 2 放置或挖掘右下角方块");
                            plr.SendMessage("清理选区所有图格: /spi c", color);
                            plr.SendMessage("清选区并放手上方块: /spi t", color);
                            plr.SendMessage("清选区并放手上墙壁: /spi w", color);
                            plr.SendMessage("清选区并放液体:/spi yt (水/岩浆/蜂蜜/微光)", color);
                            plr.SendMessage("将选区方块设为半砖:/spi bz 1到4", color);
                            break;
                        }

                        switch (args.Parameters[1].ToLower())
                        {
                            case "1":
                                plr.AwaitingTempPoint = 1;
                                plr.SendInfoMessage("请选择放置或挖掘左上角方块,画出矩形");
                                break;
                            case "2":
                                plr.AwaitingTempPoint = 2;
                                plr.SendInfoMessage("请选择放置或挖掘右下角方块,画出矩形");
                                break;
                            default:
                                plr.SendInfoMessage("使用方法: /spi s 1 放置或挖掘左上角方块");
                                plr.SendInfoMessage("使用方法: /spi s 2 放置或挖掘右下角方块");
                                plr.SendMessage("清理区域所有: /spi c", color);
                                plr.SendMessage("清选区并放手上方块: /spi t", color);
                                plr.SendMessage("清选区并放手上墙壁: /spi w", color);
                                plr.SendMessage("清理选区并放液体:/spi yt 水 岩浆 蜂蜜 微光", color);
                                plr.SendMessage("将选区方块设为半砖:/spi bz 1到4", color);
                                break;
                        }
                    }
                    break;

                case "c":
                case "clear":
                case "清理":
                    {
                        if (NeedInGame()) return;
                        if (plr.TempPoints[0].X == 0 || plr.TempPoints[1].X == 0)
                        {
                            plr.SendInfoMessage("您还没有选择区域！");
                            plr.SendMessage("使用方法: /spi s 1 选择左上角", color);
                            plr.SendMessage("使用方法: /spi s 2 选择右下角", color);
                            return;
                        }

                        await AsyncClear(plr, plr.TempPoints[0].X, plr.TempPoints[0].Y, plr.TempPoints[1].X, plr.TempPoints[1].Y);
                    }
                    break;

                case "t":
                case "方块":
                    {
                        if (NeedInGame()) return;

                        if (plr.TempPoints[0].X == 0 || plr.TempPoints[1].X == 0)
                        {
                            plr.SendInfoMessage("您还没有选择区域！");
                            plr.SendMessage("使用方法: /spi s 1 选择左上角", color);
                            plr.SendMessage("使用方法: /spi s 2 选择右下角", color);
                            return;
                        }

                        var Sel = plr.SelectedItem; //获取玩家手上方块
                        if (Sel.createTile < 0)
                        {
                            plr.SendErrorMessage("请你手持需要放置的方块");
                            return;
                        }
                        await AsyncPlaceTile(plr, plr.TempPoints[0].X, plr.TempPoints[0].Y, plr.TempPoints[1].X, plr.TempPoints[1].Y, Sel.createTile);
                    }
                    break;

                case "bz":
                case "半砖":
                case "Slope":
                    {
                        if (NeedInGame() || !plr.HasPermission("spawninfra.admin")) return;
                        var type = 0;

                        if (args.Parameters.Count > 1)
                        {
                            switch (args.Parameters[1].ToLowerInvariant())
                            {
                                case "1":
                                case "右斜坡":
                                    type = 1;
                                    break;
                                case "2":
                                case "左斜坡":
                                    type = 2;
                                    break;
                                case "3":
                                case "右半砖":
                                    type = 3;
                                    break;
                                case "4":
                                case "左半砖":
                                    type = 4;
                                    break;
                                default:
                                    plr.SendInfoMessage("正确格式为:/spi bz 1到4");
                                    plr.SendMessage("1 - 右斜坡",color);
                                    plr.SendMessage("2 - 左斜坡", color);
                                    plr.SendMessage("3 - 右半砖", color);
                                    plr.SendMessage("4 - 左半砖", color);
                                    plr.SendInfoMessage("电梯常用1/2 推怪常用3/4");
                                    return;
                            }
                        }
                        else
                        {
                            plr.SendInfoMessage("正确格式为:/spi bz 1到4");
                            plr.SendMessage("1 - 右斜坡", color);
                            plr.SendMessage("2 - 左斜坡", color);
                            plr.SendMessage("3 - 右半砖", color);
                            plr.SendMessage("4 - 左半砖", color);
                            plr.SendInfoMessage("电梯常用1/2 推怪常用3/4");
                            return;
                        }

                        await AsyncSetSlope(plr, plr.TempPoints[0].X, plr.TempPoints[0].Y, plr.TempPoints[1].X, plr.TempPoints[1].Y, type);
                    }
                    break;

                case "w":
                case "墙":
                case "墙壁":
                    {
                        if (NeedInGame()) return;
                        if (plr.TempPoints[0].X == 0 || plr.TempPoints[1].X == 0)
                        {
                            plr.SendInfoMessage("您还没有选择区域！");
                            plr.SendMessage("使用方法: /spi s 1 选择左上角", color);
                            plr.SendMessage("使用方法: /spi s 2 选择右下角", color);
                            return;
                        }

                        var Sel = plr.SelectedItem; //获取玩家手上方块
                        if (Sel.createWall < 0)
                        {
                            plr.SendErrorMessage("请你手持需要放置的墙壁");
                            return;
                        }

                        await AsyncPlaceWall(plr, plr.TempPoints[0].X, plr.TempPoints[0].Y, plr.TempPoints[1].X, plr.TempPoints[1].Y, Sel.createWall);
                    }
                    break;

                case "yt":
                case "液体":
                    {
                        if (NeedInGame()) return;
                        if (plr.TempPoints[0].X == 0 || plr.TempPoints[1].X == 0)
                        {
                            plr.SendInfoMessage("您还没有选择区域！");
                            plr.SendMessage("使用方法: /spi s 1 选择左上角", color);
                            plr.SendMessage("使用方法: /spi s 2 选择右下角", color);
                            return;
                        }

                        int type = 0;
                        if (args.Parameters.Count > 1)
                        {
                            switch (args.Parameters[1].ToLowerInvariant())
                            {
                                case "水":
                                case "water":
                                    type = 0;
                                    break;
                                case "岩浆":
                                case "lava":
                                    type = 1;
                                    break;
                                case "蜂蜜":
                                case "honey":
                                    type = 2;
                                    break;
                                case "微光":
                                case "shimmer":
                                    type = 3;
                                    break;
                                default:
                                    plr.SendErrorMessage("鱼池风格不对正确格式为:/spi yt 水、岩浆、蜂蜜、微光");
                                    return;
                            }
                        }

                        await Asynclinquid(plr, plr.TempPoints[0].X, plr.TempPoints[0].Y, plr.TempPoints[1].X, plr.TempPoints[1].Y, type);
                    }
                    break;

                case "r":
                case "房子":
                case "屋子":
                case "房屋":
                case "room":
                    {
                        if (NeedInGame()) return;

                        int total;
                        // 检查是否提供了数量参数
                        if (args.Parameters.Count < 2)
                        {
                            plr.SendErrorMessage("用法: /spi room 数量");
                            return;
                        }

                        if (!int.TryParse(args.Parameters[1], out total))
                        {
                            plr.SendErrorMessage("输入的数量不合法");
                            return;
                        }

                        if (total < 1 || total > 1000)
                        {
                            total = 3;
                        }

                        await AsyncGenRoom(plr, plr.TileX, plr.TileY + 3, total, needCenter: true);
                    }
                    break;

                case "hs":
                case "监狱":
                case "别墅":
                case "house":
                    {
                        if (NeedInGame() || !plr.HasPermission("spawninfra.admin")) return;
                        await AsyncGenLargeHouse(plr, plr.TileX, plr.TileY);
                    }
                    break;

                case "仓库":
                case "ck":
                case "Chest":
                    {
                        if (NeedInGame() || !plr.HasPermission("spawninfra.admin")) return;
                        await AsyncSpawnChest(plr, plr.TileX, plr.TileY);
                    }
                    break;

                case "yc":
                case "水池":
                case "鱼池":
                case "pond":
                    {
                        if (NeedInGame()) return;
                        int type = 0;
                        if (args.Parameters.Count > 1)
                        {
                            switch (args.Parameters[1].ToLowerInvariant())
                            {
                                case "水":
                                case "water":
                                    type = 0;
                                    break;
                                case "岩浆":
                                case "lava":
                                    type = 1;
                                    break;
                                case "蜂蜜":
                                case "honey":
                                    type = 2;
                                    break;
                                case "微光":
                                case "shimmer":
                                    type = 3;
                                    break;
                                default:
                                    plr.SendErrorMessage("鱼池风格不对正确格式为:/spi 鱼池 水、岩浆、蜂蜜、微光");
                                    return;
                            }
                        }
                        await AsyncGenPond(plr, plr.TileX, plr.TileY + 3, type);
                        break;
                    }

                case "sm":
                case "神庙":
                case "Temple":
                    {
                        if (NeedInGame() || !plr.HasPermission("spawninfra.admin")) return;
                        await AsyncTemple(plr, plr.TileX, plr.TileY);
                    }
                    break;

                case "d":
                case "dl":
                case "地牢":
                case "Dungeon":
                    {
                        if (NeedInGame() || !plr.HasPermission("spawninfra.admin")) return;
                        if ((Main.zenithWorld || Main.remixWorld) && plr.TileY < Main.worldSurface)
                        {
                            plr.SendErrorMessage("当前为颠倒种子,请前往地表生成地牢");
                            return;
                        }

                        await AsyncDungeon(plr, plr.TileX, plr.TileY);
                    }
                    break;

                case "v":
                case "wg":
                case "wgh":
                case "微光":
                case "微光湖":
                case "Shimmer":
                    {
                        if (NeedInGame() || !plr.HasPermission("spawninfra.admin")) return;
                        await AsyncShimmer(plr, plr.TileX, plr.TileY);
                    }
                    break;

                case "sg":
                case "刷怪":
                case "刷怪场":
                case "Brush":
                    {
                        if (NeedInGame()) return;

                        int height = 0;
                        int width = 0;
                        if (args.Parameters.Count < 3)
                        {
                            plr.SendErrorMessage("用法: /spi 刷怪场 高度 宽度");
                            plr.SendErrorMessage("高度必须至少70格，宽度必须至少84格");
                            return;
                        }

                        if (!int.TryParse(args.Parameters[1], out height) ||
                            !int.TryParse(args.Parameters[2], out width) ||
                            height < 70 || width < 84)
                        {
                            plr.SendErrorMessage("高度必须至少70格，宽度必须至少84格");
                            return;
                        }

                        await AsyncRockTrialField(plr, plr.TileX, plr.TileY, height, width);
                    }
                    break;

                case "h":
                case "zt":
                case "直通车":
                case "hell":
                    {
                        if (NeedInGame()) return;
                        await AsyncHellTunnel(plr, plr.TileX, plr.TileY);
                    }
                    break;

                case "wp":
                case "世平":
                case "世界平台":
                case "WorldPlatform":
                    {
                        if (NeedInGame()) return;

                        int clear;
                        if (args.Parameters.Count < 2)
                        {
                            plr.SendErrorMessage("用法: /spi 世界平台 清理高度");
                            return;
                        }

                        if (!int.TryParse(args.Parameters[1], out clear))
                        {
                            plr.SendErrorMessage("输入的高度不合法 必须超过4格");
                            return;
                        }

                        var Sel = plr.SelectedItem; //获取玩家手上方块
                        if (Sel.createTile < 0)
                        {
                            plr.SendErrorMessage("请你手持需要放置的方块或平台");
                            return;
                        }

                        await AsyncWorldPlatform(plr, plr.TileY, clear, Sel.createTile);
                    }
                    break;

                case "fp":
                case "战平":
                case "战斗平台":
                case "FightPlatforms":
                    {
                        if (NeedInGame()) return;

                        int w;
                        int h;
                        int i;

                        if (args.Parameters.Count < 4)
                        {
                            plr.SendErrorMessage("用法: /spi 战斗平台 宽度 高度 间隔");
                            return;
                        }

                        if (!int.TryParse(args.Parameters[1], out w) ||
                            !int.TryParse(args.Parameters[2], out h) ||
                            !int.TryParse(args.Parameters[3], out i))
                        {
                            plr.SendErrorMessage("用法: /spi 战斗平台 宽度 高度 间隔");
                            return;
                        }

                        var Sel = plr.SelectedItem; //获取玩家手上方块
                        if (Sel.createTile < 0)
                        {
                            plr.SendErrorMessage("请你手持需要放置的方块或平台");
                            return;
                        }
                        await AsyncFightPlatforms(plr, plr.TileX, plr.TileY, w, h, i, Sel.createTile);
                    }
                    break;

                case "bx":
                case "宝箱":
                case "箱子":
                case "NewChest":
                    {
                        if (NeedInGame() || !plr.HasPermission("spawninfra.admin")) return;

                        await AsyncBuriedChest(plr, plr.TileX, plr.TileY);
                    }
                    break;

                case "gz":
                case "pot":
                case "罐子":
                    {
                        if (NeedInGame() || !plr.HasPermission("spawninfra.admin")) return;

                        await AsyncPlacePot(plr, plr.TileX, plr.TileY);
                    }
                    break;

                case "sj":
                case "水晶":
                case "生命水晶":
                case "LifeCrystal":
                    {
                        if (NeedInGame() || !plr.HasPermission("spawninfra.admin")) return;

                        await AsyncLifeCrystal(plr, plr.TileX, plr.TileY);
                    }
                    break;

                case "jt":
                case "祭坛":
                case "Altar":
                    {
                        if (NeedInGame()) return;

                        await AsyncAltar(plr, plr.TileX, plr.TileY);
                    }
                    break;

                case "jz":
                case "剑冢":
                case "附魔剑":
                case "sword":
                    {
                        if (NeedInGame() || !plr.HasPermission("spawninfra.admin")) return;

                        await AsyncSword(plr, plr.TileX, plr.TileY);
                    }
                    break;

                case "jzt":
                case "金字塔":
                case "Pyramid":
                    {
                        if (NeedInGame() || !plr.HasPermission("spawninfra.admin")) return;

                        await AsyncPyramid(plr, plr.TileX, plr.TileY);
                    }
                    break;

                case "xj":
                case "陷阱":
                case "Trap":
                    {
                        if (NeedInGame() || !plr.HasPermission("spawninfra.admin")) return;

                        await AsyncTrap(plr, plr.TileX, plr.TileY);
                    }
                    break;

                case "重置":
                case "rs":
                case "reset":
                    {
                        if (plr.HasPermission("spawninfra.admin"))
                        {
                            Config.Enabled = true;
                            plr.SendMessage("【生成基础建设】开服自动基建已启用，请重启服务器。", 250, 247, 105);
                            Config.Write();
                        }
                    }
                    break;
            }
        }

        bool NeedInGame() => Utils.NeedInGame(plr);
        bool NeedWaitTask() => TileHelper.NeedWaitTask(plr);
    }

    #region 获取指令页数
    private static Tuple<List<string>, int> GetCommandsPage(TSPlayer plr, int page)
    {
        // 每页显示的命令数量
        int PerPage = 7;
        // 根据权限准备命令列表
        var commands = new List<string>
        {
            "/spi s [1/2] —— 修改指定选择区域(画矩形)",
            "/spi r 数量 —— 建小房子",
            "/spi hs —— 脚下生成监狱",
            "/spi ck —— 脚下生成仓库",
            "/spi dl —— 生成地牢",
            "/spi sm —— 生成神庙",
            "/spi wg —— 生成微光湖",
            "/spi sg 70 85 —— 生成刷怪场",
            "/spi yc 水 —— 生成鱼池(水/蜂蜜/岩浆/微光)",
            "/spi zt —— 脚下生成直通车(天顶为头上)",
            "/spi wp 清理高度 —— 以手上方块生成世界平台",
            "/spi fp 宽 高 间隔 —— 以手上方块生成战斗平台",
            "/spi bx —— 生成宝物箱",
            "/spi xj —— 生成陷阱",
            "/spi gz —— 生成罐子",
            "/spi sj —— 生成生命水晶",
            "/spi jt —— 生成恶魔祭坛",
            "/spi jz —— 生成附魔剑冢",
            "/spi jzt —— 生成金字塔",
            "/spi rs —— 开服自动在出生点基建"
        };

        // 如果没有管理员权限，则移除一些命令
        if (!plr.HasPermission("spawninfra.admin"))
        {
            commands.RemoveAll(cmd => cmd.Contains("hs") || cmd.Contains("ck") || cmd.Contains("dl") || cmd.Contains("sm") || cmd.Contains("wg") || cmd.Contains("bx") || cmd.Contains("xj") || cmd.Contains("gz") || cmd.Contains("sj") || cmd.Contains("jt") || cmd.Contains("jz") || cmd.Contains("jzt") || cmd.Contains("rs"));
        }

        // 计算总页数
        int total = (int)Math.Ceiling(commands.Count / (double)PerPage);
        // 确保请求的页码有效
        if (page < 1) page = 1;
        if (page > total) page = total;

        // 获取当前页应该显示的命令
        int startIndex = (page - 1) * PerPage;
        return Tuple.Create(commands.GetRange(startIndex, Math.Min(PerPage, commands.Count - startIndex)), total);
    } 
    #endregion

    #region 小房子
    public static Task AsyncGenRoom(TSPlayer plr, int posX, int posY, int total = 1, bool isRight = true, bool needCenter = false)
    {
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(delegate
        {
            int num = 5;
            int num2 = 1 + total * num;
            int num3 = (needCenter ? (posX - num2 / 2) : posX);
            for (int i = 0; i < total; i++)
            {
                GenRoom(num3, posY, isRight);
                num3 += num;
            }
        }).ContinueWith(delegate
        {
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已生成{total}个小房间，用时{value}秒。");
        });
    }
    #endregion

    #region 监狱集群
    public static Task AsyncGenLargeHouse(TSPlayer plr, int posX, int posY)
    {
        int secondLast = Utils.GetUnixTimestamp;
        var item = Config.Prison[0];
        int centerX = (posX - 6) - item.BigHouseWidth / 2;
        return Task.Run(delegate
        {
            GenLargeHouse(centerX, posY + item.spawnTileY + 3, item.BigHouseWidth, item.BigHouseHeight);

        }).ContinueWith(delegate
        {
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已生成监狱集群，用时{value}秒。");
        });
    }
    #endregion

    #region 箱子集群
    public static Task AsyncSpawnChest(TSPlayer plr, int posX, int posY)
    {
        int secondLast = Utils.GetUnixTimestamp;
        var item = Config.Chests[0];
        int centerX = (posX - 16) - item.ChestWidth / 2;
        return Task.Run(delegate
        {
            SpawnChest(centerX, posY + item.spawnTileY + 3, item.ClearHeight, item.ChestWidth, item.ChestCount, item.ChestLayers);

        }).ContinueWith(delegate
        {
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已生成箱子集群，用时{value}秒。");
        });
    }
    #endregion

    #region 鱼池
    private static Task AsyncGenPond(TSPlayer op, int posX, int posY, int style)
    {
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(delegate
        {
            GenPond(posX, posY, style);
        }).ContinueWith(delegate
        {
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            string value2 = style switch
            {
                1 => "岩浆",
                2 => "蜂蜜",
                3 => "微光",
                _ => "普通",
            };
            op.SendSuccessMessage($"已生成{value2}鱼池，用时{value}秒。");
        });
    }
    #endregion

    #region 直通车
    public static Task AsyncHellTunnel(TSPlayer plr, int posX, int posY)
    {
        int secondLast = Utils.GetUnixTimestamp;
        var item = Config.HellTunnel[0];
        var sky = Main.worldSurface * 0.3499999940395355;
        return Task.Run(delegate
        {
            if (Main.zenithWorld || Main.remixWorld) //是颠倒种子
                HellTunnel(posX, (int)sky - Config.WorldPlatform[0].WorldPlatformY, item.HellTrunnelWidth);
            else
                HellTunnel(posX, posY + item.SpawnTileY + 3, item.HellTrunnelWidth);

        }).ContinueWith(delegate
        {
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已生成直通车，用时{value}秒。");
        });
    }
    #endregion

    #region 世界平台
    public static Task AsyncWorldPlatform(TSPlayer plr, int posY, int clear, int tile)
    {
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(delegate
        {
            WorldPlatform(posY + 3, clear, tile);

        }).ContinueWith(delegate
        {
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已生成世界平台，用时{value}秒。");
        });
    }
    #endregion

    #region 战斗平台
    public static Task AsyncFightPlatforms(TSPlayer plr, int posX, int posY, int w, int h, int i, int tile)
    {
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(delegate
        {
            FightPlatforms(posX, posY + 3, w, h, i, tile);

        }).ContinueWith(delegate
        {
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已生成战斗平台，用时{value}秒。");
        });
    }
    #endregion

    #region 微光湖
    public static Task AsyncShimmer(TSPlayer plr, int posX, int posY)
    {
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(delegate
        {
            WorldGen.ShimmerMakeBiome(posX, posY + 3);
            //脚下放个方块避免掉下去
            WorldGen.PlaceTile(posX, posY + 4, 38, false, true, -1, 0);

        }).ContinueWith(delegate
        {
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已生成微光湖，用时{value}秒。");
        });
    }
    #endregion

    #region 神庙
    public static Task AsyncTemple(TSPlayer plr, int posX, int posY)
    {
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(delegate
        {
            WorldGen.makeTemple(posX, posY + 3);

        }).ContinueWith(delegate
        {
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已生成神庙，用时{value}秒。");
        });
    }
    #endregion

    #region 地牢
    public static Task AsyncDungeon(TSPlayer plr, int posX, int posY)
    {
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(delegate
        {
            WorldGen.MakeDungeon(posX, posY + 3);

        }).ContinueWith(delegate
        {
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已生成地牢，用时{value}秒。");
        });
    }
    #endregion

    #region 刷怪场生成(异步方法)
    public static Task AsyncRockTrialField(TSPlayer plr, int posX, int posY, int Height, int Width)
    {
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(delegate
        {
            RockTrialField2(posY - 9, posX, Height, Width, 2);

        }).ContinueWith(delegate
        {
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已生成刷怪场，用时{value}秒。");
        });
    }
    #endregion

    #region 刷怪场2
    public static void RockTrialField2(int posY, int posX, int Height, int Width, int Center)
    {
        // 直接使用 posY 作为基准点
        var top = posY - Height;   // 顶部位置
        var bottom = posY + Height; // 底部位置
        var middle = (top + bottom) / 2 + Center; // 中心位置

        var left = Math.Max(posX - Width, 0);
        var right = Math.Min(posX + Width, Main.maxTilesX);

        var CenterLeft = posX - 8 - Center;
        var CenterRight = posX + 8 + Center;

        // 遍历整个刷怪场高度范围 [top, bottom]
        for (var y = top; y <= bottom; y++)
        {
            for (var x = left; x < right; x++)
            {
                // 清除当前位置方块
                Main.tile[x, y].ClearEverything();

                // 顶部防液体层
                WorldGen.PlaceTile(x, top, Config.HellTunnel[0].Hell_BM_TileID, false, true, -1, 0);

                // 底部层
                WorldGen.PlaceTile(x, bottom, Config.HellTunnel[0].Hell_BM_TileID, false, true, -1, 0);

                // 底部岩浆
                if (Config.HellTunnel[0].Lava && y == bottom - 1)
                {
                    WorldGen.PlaceLiquid(x, bottom - 1, 1, 1);
                }

                // 中间刷怪层
                WorldGen.PlaceTile(x, middle, Config.HellTunnel[0].Hell_BM_TileID, false, true, -1, 0);

                // 平台层
                WorldGen.PlaceTile(x, middle + 8 + Center, Config.HellTunnel[0].PlatformID, false, true, -1, Config.HellTunnel[0].PlatformStyle);

                // 墙壁（从 middle+3 到 middle+7+Center）
                for (var wallY = middle + 3; wallY <= middle + 7 + Center; wallY++)
                {
                    Main.tile[x, wallY].wall = 155;
                }

                // 刷怪区警示墙
                if (y >= middle + 1 && y <= middle + 3)
                {
                    for (var j = 61; j <= 83; j++)
                    {
                        WorldGen.PlaceWall(CenterLeft - j + 8 + Center, y, 164, false);
                        WorldGen.PlaceWall(CenterRight + j - 8 - Center, y, 164, false);
                    }
                }

                // 实体块
                WorldGen.PlaceTile(x, middle + 11 + Center, Config.HellTunnel[0].Hell_BM_TileID, false, true, -1, 0);
                // 放计时器的平台
                WorldGen.PlaceTile(x, middle + 2, Config.HellTunnel[0].Hell_BM_TileID, false, true, -1, 0);
                // 防掉怪下来
                WorldGen.PlaceTile(x, middle + 4, Config.HellTunnel[0].Hell_BM_TileID, false, true, -1, 0);

                // 中心区域处理
                if (x >= CenterLeft && x <= CenterRight)
                {
                    for (var wallY = middle - 10 - Center; wallY <= middle - 1; wallY++)
                    {
                        // 创建矩形判断
                        if (wallY >= middle - 10 - Center && wallY <= middle - 1 && x >= CenterLeft + 1 && x <= CenterRight - 1)
                        {
                            // 挖空方块
                            Main.tile[x, wallY].ClearEverything();
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
                    PlaceWires(middle, CenterLeft, CenterRight);

                    //在1/4秒计时器下面连接开关
                    for (int i = 2; i <= 5; i++)
                    {
                        WorldGen.PlaceWire(CenterLeft - 1, middle + i);
                        WorldGen.PlaceWire(CenterRight + 1, middle + i);
                        WorldGen.PlaceTile(CenterLeft - 1, middle + 5, 136, false, true, -1, 0);
                        WorldGen.PlaceTile(CenterRight + 1, middle + 5, 136, false, true, -1, 0);
                    }
                }
            }
        }

        // 左右各放一列把刷怪场封闭起来
        for (int y2 = top; y2 <= bottom; y2++)
        {
            WorldGen.PlaceTile(left - 1, y2, Config.HellTunnel[0].Hell_BM_TileID, false, true, -1, 0);
            WorldGen.PlaceTile(right, y2, Config.HellTunnel[0].Hell_BM_TileID, false, true, -1, 0);
        }
    }

    private static void PlaceWires(int middle, int CenterLeft, int CenterRight)
    {
        WorldGen.PlaceWire(CenterLeft - 1, middle + 1);
        WorldGen.PlaceWire(CenterRight + 1, middle + 1);
        WorldGen.PlaceWire(CenterLeft - 1, middle);
        WorldGen.PlaceWire(CenterRight + 1, middle);
        WorldGen.PlaceTile(CenterLeft - 1, middle + 1, 144, false, true, -1, 4);
        WorldGen.PlaceTile(CenterRight + 1, middle + 1, 144, false, true, -1, 4);
    }
    #endregion

    #region 金字塔
    public static Task AsyncPyramid(TSPlayer plr, int posX, int posY)
    {
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(delegate
        {
            WorldGen.Pyramid(posX, posY + 3);

        }).ContinueWith(delegate
        {
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已生成金字塔，用时{value}秒。");
        });
    }
    #endregion

    #region 宝物箱子
    public static Task AsyncBuriedChest(TSPlayer plr, int posX, int posY)
    {
        int secondLast = Utils.GetUnixTimestamp;
        ushort type = (ushort)(Random.Shared.Next(2) == 0 ? 21 : 467);
        int Style = -1; // 默认样式为-1
        if (type == 21)
        {
            var range = new List<int>();
            var exclude = new List<int> { 2, 4, 36, 38, 40 }.Concat(Enumerable.Range(18, 10)).ToHashSet();
            for (int j = 0; j <= 51; j++)
            {
                // 排除掉上锁的 或 环境6神器的宝箱
                if (exclude.Contains(j)) continue;
                range.Add(j);
            }
            Style = range[Random.Shared.Next(range.Count)];
        }

        if (type == 467)
        {
            var range = new List<int>();
            var exclude = new int[] { 6, 7, 8, 9 };
            for (int j = 1; j <= 14; j++)
            {
                if (exclude.Contains(j)) continue;
                range.Add(j);
            }
            Style = range[Random.Shared.Next(range.Count)];
        }

        return Task.Run(delegate
        {
            Main.tile[posX, posY + 2].ClearEverything();
            WorldGen.AddBuriedChest(posX, posY + 3, contain: 0, false, Style, false, chestTileType: type);

        }).ContinueWith(delegate
        {
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已生成自然箱子，用时{value}秒。");
        });
    }
    #endregion

    #region 生命水晶生成
    public static Task AsyncLifeCrystal(TSPlayer plr, int posX, int posY)
    {
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(delegate
        {
            Main.tile[posX, posY + 2].ClearEverything();
            WorldGen.AddLifeCrystal(posX, posY + 3);

        }).ContinueWith(delegate
        {
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已生成生命水晶，用时{value}秒。");
        });
    }
    #endregion

    #region 罐子随机生成（样式0到36）
    public static Task AsyncPlacePot(TSPlayer plr, int posX, int posY)
    {
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(delegate
        {
            Main.tile[posX, posY + 2].ClearEverything();
            WorldGen.PlacePot(posX, posY + 2, 28, Random.Shared.Next(0, 37));

        }).ContinueWith(delegate
        {
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已生成罐子，用时{value}秒。");
        });
    }
    #endregion

    #region 陷阱
    public static Task AsyncTrap(TSPlayer plr, int posX, int posY)
    {
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(delegate
        {
            Main.tile[posX, posY + 2].ClearEverything();
            WorldGen.placeTrap(posX, posY + 3);

        }).ContinueWith(delegate
        {
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已生成陷阱，用时{value}秒。");
        });
    }
    #endregion

    #region 附魔剑冢
    public static Task AsyncSword(TSPlayer plr, int posX, int posY)
    {
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(delegate
        {
            Main.tile[posX, posY + 2].ClearEverything();
            WorldGen.PlaceTile(posX, posY + 2, 187, true, false, -1, 17);

        }).ContinueWith(delegate
        {
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已生成附魔剑冢，用时{value}秒。");
        });
    }
    #endregion

    #region 邪恶祭坛随机生成0或1（0是恶魔祭坛 1是猩红祭坛）
    public static Task AsyncAltar(TSPlayer plr, int posX, int posY)
    {
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(delegate
        {
            Main.tile[posX, posY + 2].ClearEverything();
            WorldGen.Place3x2(posX, posY + 2, 26, Random.Shared.Next(2));

        }).ContinueWith(delegate
        {
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已生成邪恶祭坛，用时{value}秒。");
        });
    }
    #endregion

    #region 清理选区
    public static Task AsyncClear(TSPlayer plr, int startX, int startY, int endX, int endY)
    {
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(() =>
        {
            for (int x = Math.Min(startX, endX); x <= Math.Max(startX, endX); x++)
            {
                for (int y = Math.Min(startY, endY); y <= Math.Max(startY, endY); y++)
                {
                    Main.tile[x, y].ClearEverything();
                    WorldGen.PlaceWall(x, y, 2);
                }
            }
        }).ContinueWith(_ =>
        {
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已清理区域，用时{value}秒。");
        });
    }
    #endregion

    #region 生成方块
    public static Task AsyncPlaceTile(TSPlayer plr, int startX, int startY, int endX, int endY, int TileID)
    {
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(() =>
        {
            for (int x = Math.Min(startX, endX); x <= Math.Max(startX, endX); x++)
            {
                for (int y = Math.Min(startY, endY); y <= Math.Max(startY, endY); y++)
                {
                    Main.tile[x, y].ClearEverything();
                    WorldGen.PlaceTile(x, y, TileID);
                }
            }
        }).ContinueWith(_ =>
        {
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已生成方块，用时{value}秒。");
        });
    }
    #endregion

    #region 生成液体
    public static Task Asynclinquid(TSPlayer plr, int startX, int startY, int endX, int endY, int type)
    {
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(() =>
        {
            for (int x = Math.Min(startX, endX); x <= Math.Max(startX, endX); x++)
            {
                for (int y = Math.Min(startY, endY); y <= Math.Max(startY, endY); y++)
                {
                    Main.tile[x, y].ClearEverything();
                    Main.tile[x, y].liquid = byte.MaxValue;
                    Main.tile[x, y].liquidType(type);
                }
            }
        }).ContinueWith(_ =>
        {
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已生成液体，用时{value}秒。");
        });
    }
    #endregion

    #region 生成墙壁
    public static Task AsyncPlaceWall(TSPlayer plr, int startX, int startY, int endX, int endY, int wallID)
    {
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(() =>
        {
            for (int x = Math.Min(startX, endX); x <= Math.Max(startX, endX); x++)
            {
                for (int y = Math.Min(startY, endY); y <= Math.Max(startY, endY); y++)
                {
                    Main.tile[x, y].ClearEverything();
                    WorldGen.PlaceWall(x, y, wallID);
                }
            }
        }).ContinueWith(_ =>
        {
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已生成墙壁，用时{value}秒。");
        });
    }
    #endregion

    #region 设为半砖
    public static Task AsyncSetSlope(TSPlayer plr, int startX, int startY, int endX, int endY, int SlopeID)
    {
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(delegate
        {
            for (int x = Math.Min(startX, endX); x <= Math.Max(startX, endX); x++)
            {
                for (int y = Math.Min(startY, endY); y <= Math.Max(startY, endY); y++)
                {
                    WorldGen.SlopeTile(x, y, SlopeID);
                }
            }

        }).ContinueWith(delegate
        {
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已将方块设为半砖，用时{value}秒。");
        });
    }
    #endregion

}
