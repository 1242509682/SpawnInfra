using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using TShockAPI;
using static SpawnInfra.Plugin;
using static SpawnInfra.Utils;
using static Terraria.GameContent.Tile_Entities.TELogicSensor;

namespace SpawnInfra;

internal static class Commands
{
    public static async void command(CommandArgs args)
    {
        TSPlayer plr = args.Player;

        //随机颜色
        Random rand = new Random();
        int r = rand.Next(150, 200);
        int g = rand.Next(170, 200);
        int b = rand.Next(150, 200);
        var color = new Color(r, g, b);

        if (args.Parameters.Count == 0)
        {
            plr.SendInfoMessage("\n《生成基建》 [i:3456][C/F2F2C7:插件开发] [C/BFDFEA:by] [c/00FFFF:羽学][i:3459]");
            Help(args, plr, color, 1);
        }

        if (args.Parameters.Count >= 1)
        {
            switch (args.Parameters[0].ToLower())
            {
                case "help":
                case "h":
                    {
                        int Page = 1; // 默认第一页
                        if (args.Parameters.Count >= 2 && int.TryParse(args.Parameters[1], out int Pages))
                        {
                            Page = Pages;
                        }

                        plr.SendInfoMessage("\n《生成基建》 [i:3456][C/F2F2C7:插件开发] [C/BFDFEA:by] [c/00FFFF:羽学][i:3459]");
                        Page = Help(args, plr, color, Page);
                    }
                    break;

                case "s":
                case "set":
                case "选择":
                    {
                        if (NeedInGame()) return;
                        if (args.Parameters.Count < 2)
                        {
                            plr.SendInfoMessage("\n请确保s1和s2两点不再同一位置,两点之间可形成'线'或者'方形'");
                            plr.SendInfoMessage("使用方法: /spi s 1 放置或挖掘左上角方块");
                            plr.SendInfoMessage("使用方法: /spi s 2 放置或挖掘右下角方块");
                            plr.SendMessage("[c/F8F5B8:清理]区域: [c/AEEBE9:/spi c 1到8] (1方块,2墙,3液体,4喷漆,5不虚化,6电线,7制动器,8所有)", color);
                            plr.SendMessage("根据选区放[c/F8F5B8:手上方块]: [c/AEEBE9:/spi t 1到4] (1保留放置,2替换,3清完放置,4连锁放置)", color);
                            plr.SendMessage("根据选区放[c/F8F5B8:手上墙壁]: [c/AEEBE9:/spi w 1和2] (1保留放置,2清完放置)", color);
                            plr.SendMessage("根据选区放[c/F8F5B8:手上喷漆]: [c/AEEBE9:/spi p 1到4] (1喷方块,2喷墙,3所有,4切换方块虚化)", color);
                            plr.SendMessage("清选区并[c/F8F5B8:放液体]:[c/AEEBE9:/spi yt 1到4] (1水,2岩浆,3蜂蜜,微光)", color);
                            plr.SendMessage("将选区方块[c/F8F5B8:设为半砖]:[c/AEEBE9:/spi bz 1到5]（1右斜坡,2左斜坡,3右半砖,4左半砖,5全砖）", color);
                            plr.SendMessage("为选区方块[c/F8F5B8:添加电路]:[c/AEEBE9:/spi wr 1到4]（1只放电线,2清后再放,3清后再放并虚化,4清后再放加制动器）", color);
                            plr.SendMessage("*将选区[c/F8F5B8:还原上次]修改图格:[c/AEEBE9:/spi bk]", color);
                            plr.SendMessage("*[c/F8F5B8:复制]选区为自己名下建筑:[c/AEEBE9:/spi cp]", color);
                            plr.SendMessage("*[c/F8F5B8:复制]选区为保存指定建筑:[c/AEEBE9:/spi cp 名字]", color);
                            plr.SendMessage("*在头顶[c/F8F5B8:粘贴]自己名下建筑:[c/AEEBE9:/spi pt]", color);
                            plr.SendMessage("*在头顶位置[c/F8F5B8:粘贴]指定建筑:[c/AEEBE9:/spi pt 名字]", color);

                            if (plr.HasPermission("spawninfra.admin"))
                            {
                                plr.SendMessage("*粘贴还原箱子物品建筑:[c/AEEBE9:/spi pt -f] 或 [c/AEEBE9:/spi pt 名字 -f]", color);
                            }
                            break;
                        }

                        switch (args.Parameters[1].ToLower())
                        {
                            case "1":
                                plr.AwaitingTempPoint = 1;
                                plr.SendInfoMessage("请选择放置或挖掘左上角方块,画成: 线 或 方形");
                                break;
                            case "2":
                                plr.AwaitingTempPoint = 2;
                                plr.SendInfoMessage("请选择放置或挖掘右下角方块,画成: 线 或 方形");
                                break;
                            default:
                                plr.SendInfoMessage("\n请确保s1和s2两点不再同一位置,两点之间可形成'线'或者'方形'");
                                plr.SendInfoMessage("使用方法: /spi s 1 放置或挖掘左上角方块");
                                plr.SendInfoMessage("使用方法: /spi s 2 放置或挖掘右下角方块");
                                plr.SendMessage("[c/F8F5B8:清理]区域: [c/AEEBE9:/spi c 1到8] (1方块,2墙,3液体,4喷漆,5不虚化,6电线,7制动器,8所有)", color);
                                plr.SendMessage("根据选区放[c/F8F5B8:手上方块]: [c/AEEBE9:/spi t 1到4] (1保留放置,2替换,3清完放置,4连锁放置)", color);
                                plr.SendMessage("根据选区放[c/F8F5B8:手上墙壁]: [c/AEEBE9:/spi w 1和2] (1保留放置,2清完放置)", color);
                                plr.SendMessage("根据选区放[c/F8F5B8:手上喷漆]: [c/AEEBE9:/spi p 1到4] (1喷方块,2喷墙,3所有,4切换方块虚化)", color);
                                plr.SendMessage("清选区并[c/F8F5B8:放液体]:[c/AEEBE9:/spi yt 1到4] (1水,2岩浆,3蜂蜜,微光)", color);
                                plr.SendMessage("将选区方块[c/F8F5B8:设为半砖]:[c/AEEBE9:/spi bz 1到5]（1右斜坡,2左斜坡,3右半砖,4左半砖,5全砖）", color);
                                plr.SendMessage("为选区方块[c/F8F5B8:添加电路]:[c/AEEBE9:/spi wr 1到4]（1只放电线,2清后再放,3清后再放并虚化,4清后再放加制动器）", color);
                                plr.SendMessage("*将选区[c/F8F5B8:还原上次]修改图格:[c/AEEBE9:/spi bk]", color);
                                plr.SendMessage("*[c/F8F5B8:复制]选区为自己名下建筑:[c/AEEBE9:/spi cp]", color);
                                plr.SendMessage("*[c/F8F5B8:复制]选区为保存指定建筑:[c/AEEBE9:/spi cp 名字]", color);
                                plr.SendMessage("*在头顶[c/F8F5B8:粘贴]自己名下建筑:[c/AEEBE9:/spi pt]", color);
                                plr.SendMessage("*在头顶位置[c/F8F5B8:粘贴]指定建筑:[c/AEEBE9:/spi pt 名字]", color);

                                if (plr.HasPermission("spawninfra.admin"))
                                {
                                    plr.SendMessage("*粘贴还原箱子物品建筑:[c/AEEBE9:/spi pt -f] 或 [c/AEEBE9:/spi pt 名字 -f]", color);
                                }
                                break;
                        }
                    }
                    break;

                case "l":
                case "list":
                case "列表":
                    {
                        var clipNames = Map.GetAllClipNames();

                        if (clipNames.Count == 0)
                        {
                            plr.SendErrorMessage("当前[c/64E0DA:没有可用]的复制建筑。");
                            plr.SendInfoMessage($"如何复制自己的建筑:");
                            plr.SendInfoMessage($"使用选区指令:[c/64A1E0:/spi s]");
                            plr.SendInfoMessage($"使用复制指令:[c/F56C77:/spi cp] 或 [c/63E0D8:/spi cp 建筑名]");
                        }
                        else
                        {
                            plr.SendMessage($"\n当前可用的复制建筑 [c/64E0DA:{clipNames.Count}个]:", color);

                            // 加上索引编号（从 1 开始）
                            for (int i = 0; i < clipNames.Count; i++)
                            {
                                string msg = $"[c/D0AFEB:{i + 1}.] [c/FFFFFF:{clipNames[i]}]";
                                plr.SendMessage(msg, Color.AntiqueWhite);
                            }

                            plr.SendMessage($"可使用指定粘贴指令:[c/D0AFEB:/spi pt 名字]", color);
                        }
                    }
                    break;

                case "cp":
                case "copy":
                case "复制":
                    {
                        if (NeedInGame()) return;
                        if (plr.TempPoints[0].X == 0 || plr.TempPoints[1].X == 0)
                        {
                            plr.SendInfoMessage("您还没有选择区域！");
                            plr.SendMessage("使用方法: /spi s 1 选择左上角", color);
                            plr.SendMessage("使用方法: /spi s 2 选择右下角", color);
                            return;
                        }

                        string name = plr.Name; // 默认使用玩家自己的名字
                        if (args.Parameters.Count >= 2 && !string.IsNullOrWhiteSpace(args.Parameters[1]))
                        {
                            name = args.Parameters[1]; // 使用指定的名字
                        }

                        // 保存到剪贴板
                        var clipboard = CreateClipData(
                            plr.TempPoints[0].X, plr.TempPoints[0].Y,
                            plr.TempPoints[1].X, plr.TempPoints[1].Y);

                        Map.SaveClip(name, clipboard);
                        plr.SendSuccessMessage($"已复制区域 ({clipboard.Width}x{clipboard.Height})");
                    }
                    break;

                case "pt":
                case "粘贴":
                case "paste":
                    {
                        if (NeedInGame() || NeedWaitTask()) return;

                        string name = plr.Name; // 默认使用玩家自己的名字
                        bool fixChest = false;

                        // 解析参数
                        for (int i = 1; i < args.Parameters.Count; i++)
                        {
                            string param = args.Parameters[i];
                            if (param == "-f")
                            {
                                if (plr.HasPermission("spawninfra.admin"))
                                {
                                    plr.SendInfoMessage("已为你修复箱子物品");
                                    fixChest = true;
                                }
                                else
                                {
                                    plr.SendErrorMessage("你无权使用`-f`参数修复箱子物品");
                                    plr.SendInfoMessage("这需要权限: [c/AEEBE9:spawninfra.admin]");
                                    fixChest = false;
                                }
                            }
                            else if (!string.IsNullOrWhiteSpace(param))
                            {
                                name = param; // 使用指定的名字
                            }
                        }

                        var clip = Map.LoadClip(name);
                        if (clip == null)
                        {
                            plr.SendErrorMessage("剪贴板为空！");
                            return;
                        }

                        // 计算粘贴位置（玩家当前位置为头顶）
                        int startX = plr.TileX - clip.Width / 2;
                        int startY = plr.TileY - clip.Height;

                        await AsyncPaste(plr, startX, startY, clip, fixChest);
                    }
                    break;

                case "bk":
                case "back":
                case "还原":
                    {
                        if (NeedInGame() || NeedWaitTask()) return;

                        await AsyncBack(plr, plr.TempPoints[0].X, plr.TempPoints[0].Y, plr.TempPoints[1].X, plr.TempPoints[1].Y);
                    }
                    break;

                case "c":
                case "clear":
                case "清理":
                    {
                        if (NeedInGame() || NeedWaitTask()) return;

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
                                case "1": case "t": case "方块": case "block": type = 1; break;
                                case "2": case "w": case "墙": case "墙壁": case "will": type = 2; break;
                                case "3": case "yt": case "水": case "液体": case "linquid": type = 3; break;
                                case "4": case "p": case "paint": case "染料": type = 4; break;
                                case "5": case "虚化": type = 5; break;
                                case "6": case "wr": case "wire": case "电线": case "电路": type = 6; break;
                                case "7": case "ac": case "制动器": case "actuator": type = 7; break;
                                case "8": case "all": case "全部": type = 8; break;
                                default:
                                    plr.SendInfoMessage("正确格式为:/spi c 1到4(1方块/2墙/3液体/4所有)");
                                    return;
                            }
                        }
                        else
                        {
                            plr.SendInfoMessage("正确格式为:/spi c 1到4(1方块/2墙/3液体/4所有)");
                            return;
                        }

                        await AsyncClear(plr, plr.TempPoints[0].X, plr.TempPoints[0].Y, plr.TempPoints[1].X, plr.TempPoints[1].Y, type);
                    }
                    break;

                case "bz":
                case "半砖":
                case "Slope":
                    {
                        if (NeedInGame() || !plr.HasPermission("spawninfra.admin") || NeedWaitTask()) return;

                        var type = 0;
                        if (args.Parameters.Count > 1)
                        {
                            switch (args.Parameters[1].ToLowerInvariant())
                            {
                                case "1": case "右斜坡": type = 1; break;
                                case "2": case "左斜坡": type = 2; break;
                                case "3": case "右半砖": type = 3; break;
                                case "4": case "左半砖": type = 4; break;
                                case "5": case "全砖": type = 5; break;
                                default:
                                    plr.SendInfoMessage("正确格式为:/spi bz 1到4");
                                    plr.SendMessage("1 - 右斜坡", color);
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

                case "t":
                case "方块":
                case "block":
                    {
                        if (NeedInGame() || NeedWaitTask()) return;

                        int type = 0;
                        if (args.Parameters.Count > 1)
                        {
                            switch (args.Parameters[1].ToLowerInvariant())
                            {
                                case "1": case "保留": type = 1; break;
                                case "2": case "替换": type = 2; break;
                                case "3": case "清理": type = 3; break;
                                case "4": case "连锁": type = 4; break;
                                default:
                                    plr.SendInfoMessage("正确格式为:/spi t 1到4");
                                    plr.SendMessage("1 - 保留原有放置", color);
                                    plr.SendMessage("2 - 替换后放置", color);
                                    plr.SendMessage("3 - 清完所有后放置", color);
                                    plr.SendMessage("4 - 连锁放置", color);
                                    return;
                            }
                        }
                        else
                        {
                            plr.SendInfoMessage("正确格式为:/spi t 1到4");
                            plr.SendMessage("1 - 保留原有放置", color);
                            plr.SendMessage("2 - 替换后放置", color);
                            plr.SendMessage("3 - 清完所有后放置", color);
                            plr.SendMessage("4 - 连锁放置", color);
                            return;
                        }

                        var Sel = plr.SelectedItem; //获取玩家手上物品
                        if (Sel.createTile < 0)
                        {
                            plr.SendErrorMessage("请你手持需要放置的方块");
                            return;
                        }

                        if (type != 4)
                        {
                            // 只有在非连锁模式下才需要选区
                            if (plr.TempPoints[0].X == 0 || plr.TempPoints[1].X == 0)
                            {
                                plr.SendInfoMessage("您还没有选择区域！");
                                plr.SendMessage("使用方法: /spi s 1 选择左上角", color);
                                plr.SendMessage("使用方法: /spi s 2 选择右下角", color);
                                return;
                            }

                            await AsyncPlaceTile(plr, plr.TempPoints[0].X, plr.TempPoints[0].Y,
                                                 plr.TempPoints[1].X, plr.TempPoints[1].Y,
                                                 Sel.createTile, type, Sel.placeStyle);
                        }
                        else
                        {
                            // 连锁模式：记录图格类型，不处理选区
                            SweepReplaceMode[plr.Name] = Sel.createTile;
                            plr.SendSuccessMessage("已进入连锁替换模式，请点击任意图格进行替换。");
                        }
                    }
                    break;

                case "w":
                case "墙":
                case "墙壁":
                    {
                        if (NeedInGame() || NeedWaitTask()) return;

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
                                case "1": case "保留": type = 1; break;
                                case "2": case "清理": type = 2; break;
                                case "3": case "连锁": type = 3; break;
                                default:
                                    plr.SendInfoMessage("正确格式为:/spi w 1和2 (1保留原有放置/2清完所有后放置/3连锁放置)");
                                    return;
                            }
                        }
                        else
                        {
                            plr.SendInfoMessage("正确格式为:/spi w 1和3 (1保留原有放置/2清完所有后放置/3连锁放置)");
                            return;
                        }

                        var Sel = plr.SelectedItem; //获取玩家手上物品
                        if (Sel.createWall < 0)
                        {
                            plr.SendErrorMessage("请你手持需要放置的墙壁");
                            return;
                        }

                        await AsyncPlaceWall(plr, plr.TempPoints[0].X, plr.TempPoints[0].Y, plr.TempPoints[1].X, plr.TempPoints[1].Y, Sel.createWall, type);
                    }
                    break;

                case "wr":
                case "wire":
                case "电路":
                case "电线":
                    {
                        if (NeedInGame() || NeedWaitTask()) return;

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
                                case "1": case "保留": type = 1; break;
                                case "2": case "清理": type = 2; break;
                                case "3": case "虚化": type = 3; break;
                                case "4": case "制动器": type = 4; break;
                                default:
                                    plr.SendErrorMessage("正确格式为:/spi ct 1到4");
                                    plr.SendInfoMessage("1 - 保留原有电线后放置");
                                    plr.SendInfoMessage("2 - 清完电线后所有后放置)");
                                    plr.SendInfoMessage("3 - 清电线后放置并虚化方块)");
                                    plr.SendInfoMessage("4 - 清电线后放置再添加制动器)");
                                    return;
                            }
                        }
                        else
                        {
                            plr.SendErrorMessage("正确格式为:/spi ct 1到4");
                            plr.SendInfoMessage("1 - 保留原有电线后放置");
                            plr.SendInfoMessage("2 - 清完电线后所有后放置)");
                            plr.SendInfoMessage("3 - 清电线后放置并虚化方块)");
                            plr.SendInfoMessage("4 - 清电线后放置再添加制动器)");
                            return;
                        }

                        await AsyncPlaceWire(plr, plr.TempPoints[0].X, plr.TempPoints[0].Y, plr.TempPoints[1].X, plr.TempPoints[1].Y, type);
                    }
                    break;

                case "p":
                case "paint":
                case "喷漆":
                    {
                        if (NeedInGame() || NeedWaitTask()) return;

                        if (plr.TempPoints[0].X == 0 || plr.TempPoints[1].X == 0)
                        {
                            plr.SendInfoMessage("您还没有选择区域！");
                            plr.SendMessage("使用方法: /spi s 1 选择左上角", color);
                            plr.SendMessage("使用方法: /spi s 2 选择右下角", color);
                            return;
                        }

                        var Sel = plr.SelectedItem; //获取玩家手上物品
                        byte paintID = 0;
                        bool hasPaint = false;
                        if (Sel.paint > 0)
                        {
                            paintID = Sel.paint;
                            hasPaint = true;
                        }
                        else if (Sel.paintCoating > 0)
                        {
                            paintID = Sel.paintCoating;
                            hasPaint = false;
                        }
                        else
                        {
                            plr.SendErrorMessage("请你手持涂料或喷漆再使用/spi p 指令");
                            return;
                        }

                        int type = 0;
                        if (args.Parameters.Count > 1)
                        {
                            switch (args.Parameters[1].ToLowerInvariant())
                            {
                                case "1": case "方块": case "block": type = 1; break;
                                case "2": case "墙": case "墙壁": case "wall": type = 2; break;
                                case "3": case "所有": case "all": type = 3; break;
                                case "4": case "虚化": type = 4; break;
                                default:
                                    plr.SendInfoMessage("正确格式为:/spi p 1到4 (1方块/2墙壁/3所有/4虚化切换)");
                                    plr.SendMessage("1 - 只喷方块", color);
                                    plr.SendMessage("2 - 只喷墙壁", color);
                                    plr.SendMessage("3 - 喷所有方块和墙壁", color);
                                    plr.SendMessage("4 - 切换方块虚化状态", color);
                                    return;
                            }
                        }
                        else
                        {
                            plr.SendInfoMessage("正确格式为:/spi p 1到4 (1方块/2墙壁/3所有/4虚化切换)");
                            plr.SendMessage("1 - 只喷方块", color);
                            plr.SendMessage("2 - 只喷墙壁", color);
                            plr.SendMessage("3 - 喷所有方块和墙壁", color);
                            plr.SendMessage("4 - 切换方块虚化状态", color);
                            return;
                        }

                        await AsyncPlacePaint(plr, plr.TempPoints[0].X, plr.TempPoints[0].Y, plr.TempPoints[1].X, plr.TempPoints[1].Y, paintID, type, hasPaint);
                    }
                    break;

                case "yt":
                case "水":
                case "液体":
                case "linquid":
                    {
                        if (NeedInGame() || NeedWaitTask()) return;


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
                                case "1": case "水": case "water": type = 0; break;
                                case "2": case "岩浆": case "lava": type = 1; break;
                                case "3": case "蜂蜜": case "honey": type = 2; break;
                                case "4": case "微光": case "shimmer": type = 3; break;
                                default:
                                    plr.SendInfoMessage("正确格式为:/spi yt 1到4(水/岩浆/蜂蜜/微光)");
                                    return;
                            }
                        }
                        else
                        {
                            plr.SendInfoMessage("正确格式为:/spi yt 1到4(水/岩浆/蜂蜜/微光)");
                            return;
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
                        if (NeedInGame() || NeedWaitTask()) return;

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
                        if (NeedInGame() || !plr.HasPermission("spawninfra.admin") || NeedWaitTask()) return;

                        await AsyncGenLargeHouse(plr, plr.TileX, plr.TileY);
                    }
                    break;

                case "仓库":
                case "ck":
                case "Chest":
                    {
                        if (NeedInGame() || !plr.HasPermission("spawninfra.admin") || NeedWaitTask()) return;

                        await AsyncSpawnChest(plr, plr.TileX, plr.TileY);
                    }
                    break;

                case "yc":
                case "水池":
                case "鱼池":
                case "pond":
                    {
                        if (NeedInGame() || NeedWaitTask()) return;

                        int type = 0;
                        if (args.Parameters.Count > 1)
                        {
                            switch (args.Parameters[1].ToLowerInvariant())
                            {
                                case "1": case "水": case "water": type = 0; break;
                                case "2": case "岩浆": case "lava": type = 1; break;
                                case "3": case "蜂蜜": case "honey": type = 2; break;
                                case "4": case "微光": case "shimmer": type = 3; break;
                                default:
                                    plr.SendErrorMessage("正确格式为:/spi yc 1到4 (1水,2岩浆,3蜂蜜,4微光");
                                    return;
                            }
                        }
                        else
                        {
                            plr.SendErrorMessage("正确格式为:/spi yc 1到4 (1水,2岩浆,3蜂蜜,4微光");
                            return;
                        }

                        await AsyncGenPond(plr, plr.TileX, plr.TileY + 3, type);
                        break;
                    }

                case "sm":
                case "神庙":
                case "Temple":
                    {
                        if (NeedInGame() || !plr.HasPermission("spawninfra.admin") || NeedWaitTask()) return;

                        await AsyncTemple(plr, plr.TileX, plr.TileY);
                    }
                    break;

                case "d":
                case "dl":
                case "地牢":
                case "Dungeon":
                    {
                        if (NeedInGame() || !plr.HasPermission("spawninfra.admin") || NeedWaitTask()) return;

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
                        if (NeedInGame() || !plr.HasPermission("spawninfra.admin") || NeedWaitTask()) return;

                        await AsyncShimmer(plr, plr.TileX, plr.TileY);
                    }
                    break;

                case "sg":
                case "刷怪":
                case "刷怪场":
                case "Brush":
                    {
                        if (NeedInGame() || NeedWaitTask()) return;

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

                case "zt":
                case "直通车":
                case "hell":
                    {
                        if (NeedInGame() || NeedWaitTask()) return;

                        await AsyncHellTunnel(plr, plr.TileX, plr.TileY);
                    }
                    break;

                case "wp":
                case "世平":
                case "世界平台":
                case "WorldPlatform":
                    {
                        if (NeedInGame() || NeedWaitTask()) return;


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

                        await AsyncWorldPlatform(plr, plr.TileY, clear, Sel.createTile, Sel.placeStyle);
                    }
                    break;

                case "fp":
                case "战平":
                case "战斗平台":
                case "FightPlatforms":
                    {
                        if (NeedInGame() || NeedWaitTask()) return;

                        int w; int h; int i;

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
                        await AsyncFightPlatforms(plr, plr.TileX, plr.TileY, w, h, i, Sel.createTile, Sel.placeStyle);
                    }
                    break;

                case "bx":
                case "宝箱":
                case "箱子":
                case "NewChest":
                    {
                        if (NeedInGame() || !plr.HasPermission("spawninfra.admin") || NeedWaitTask()) return;

                        await AsyncBuriedChest(plr, plr.TileX, plr.TileY);
                    }
                    break;

                case "gz":
                case "pot":
                case "罐子":
                    {
                        if (NeedInGame() || !plr.HasPermission("spawninfra.admin") || NeedWaitTask()) return;

                        await AsyncPlacePot(plr, plr.TileX, plr.TileY);
                    }
                    break;

                case "sj":
                case "水晶":
                case "生命水晶":
                case "LifeCrystal":
                    {
                        if (NeedInGame() || !plr.HasPermission("spawninfra.admin") || NeedWaitTask()) return;

                        await AsyncLifeCrystal(plr, plr.TileX, plr.TileY);
                    }
                    break;

                case "jt":
                case "祭坛":
                case "Altar":
                    {
                        if (NeedInGame() || NeedWaitTask()) return;

                        await AsyncAltar(plr, plr.TileX, plr.TileY);
                    }
                    break;

                case "jz":
                case "剑冢":
                case "附魔剑":
                case "sword":
                    {
                        if (NeedInGame() || !plr.HasPermission("spawninfra.admin") || NeedWaitTask()) return;

                        await AsyncSword(plr, plr.TileX, plr.TileY);
                    }
                    break;

                case "jzt":
                case "金字塔":
                case "Pyramid":
                    {
                        if (NeedInGame() || !plr.HasPermission("spawninfra.admin") || NeedWaitTask()) return;

                        await AsyncPyramid(plr, plr.TileX, plr.TileY);
                    }
                    break;

                case "xj":
                case "陷阱":
                case "Trap":
                    {
                        if (NeedInGame() || !plr.HasPermission("spawninfra.admin") || NeedWaitTask()) return;

                        await AsyncTrap(plr, plr.TileX, plr.TileY);
                    }
                    break;

                case "fh":
                case "腐化":
                case "腐化地":
                    {
                        if (NeedInGame() || !plr.HasPermission("spawninfra.admin") || NeedWaitTask()) return;

                        await AsyncChasmRunner(plr, plr.TileX, plr.TileY);
                    }
                    break;

                case "重置":
                case "rs":
                case "reset":
                    {
                        if (plr.HasPermission("spawninfra.admin"))
                        {
                            //重置是否自动基建
                            if (!Config.SpawnInfra)
                            {
                                Config.SpawnInfra = true;
                                plr.SendMessage("【生成基础建设】开服自动基建已启用，请重启服务器。", 250, 247, 105);
                                Config.Write();
                            }
                            else
                            {
                                Config.SpawnInfra = false;
                                plr.SendMessage("【生成基础建设】开服自动基建已关闭", 250, 247, 105);
                                Config.Write();
                            }

                            if (Config.DeleteAllDataFiles || Config.BackerAllDataFiles)
                            {
                                if (!Directory.Exists(Map.DataDir))
                                {
                                    Directory.CreateDirectory(Map.DataDir);
                                    plr.SendMessage("tshok文件夹:未检测到SPIData文件夹,已创建", 250, 247, 105);
                                    break;
                                }

                                var datFiles = Directory.GetFiles(Map.DataDir, "*.dat");
                                if (datFiles.Length == 0)
                                {
                                    plr.SendMessage("SPIData文件夹:未找到任何.dat建筑文件,已返回", 250, 247, 105);
                                    return;
                                }
                                else
                                {
                                    Map.BackupAndDeleteAllDataFiles();  //删除所有剪切板数据文件并备份压缩包
                                    plr.SendMessage("已备份或清理:tshck文件夹下的SPIData里所有建筑数据", 250, 247, 105);
                                }
                                break;
                            }
                        }
                    }
                    break;
            }
        }

        bool NeedInGame() => Utils.NeedInGame(plr);
        bool NeedWaitTask() => TileHelper.NeedWaitTask(plr);
    }

    #region 帮助菜单
    private static int Help(CommandArgs args, TSPlayer plr, Color color, int Page)
    {
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
            plr.SendInfoMessage($"/spi help {Page + 1} 下一页");
        }
        if (Page > 1)
        {
            plr.SendInfoMessage($"/spi help {Page - 1} 上一页");
        }

        plr.SendInfoMessage($"当前 {Page} 页,共计 {all} 页\n");
        return Page;
    }
    #endregion

    #region 获取指令页数
    private static Tuple<List<string>, int> GetCommandsPage(TSPlayer plr, int page)
    {
        // 每页显示的命令数量
        int PerPage = 7;
        // 根据权限准备命令列表
        var commands = new List<string>
        {
            "/spi s [1/2] —— 修改指定选择区域(画矩形)",
            "/spi L —— 列出所有复制建筑",
            "/spi r 数量 —— 建小房子",
            "/spi hs —— 脚下生成监狱",
            "/spi ck —— 脚下生成仓库",
            "/spi dl —— 生成地牢",
            "/spi sm —— 生成神庙",
            "/spi wg —— 生成微光湖",
            "/spi yc —— 生成鱼池",
            "/spi sg 70 85 —— 生成刷怪场",
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
            "/spi fh —— 生成腐化地",
            "/spi rs —— 出生点自动基建/清除所有建筑数据"
        };

        // 如果没有管理员权限，则移除一些命令
        if (!plr.HasPermission("spawninfra.admin"))
        {
            commands.RemoveAll(cmd => cmd.Contains("hs") || cmd.Contains("ck") || cmd.Contains("dl") || cmd.Contains("sm") || cmd.Contains("wg") || cmd.Contains("bx") || cmd.Contains("xj") || cmd.Contains("gz") || cmd.Contains("sj") || cmd.Contains("jt") || cmd.Contains("jz") || cmd.Contains("jzt") || cmd.Contains("rs") || cmd.Contains("fh"));
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
        TileHelper.StartGen();
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(() =>
        {
            int num = 5;
            int num2 = 1 + total * num;
            int num3 = (needCenter ? (posX - num2 / 2) : posX);
            for (int i = 0; i < total; i++)
            {
                GenRoom(num3, posY, isRight);
                num3 += num;
            }
        }).ContinueWith(_ =>
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
        TileHelper.StartGen();
        int secondLast = Utils.GetUnixTimestamp;
        var item = Config.Prison[0];
        int centerX = (posX - 6) - item.BigHouseWidth / 2;
        return Task.Run(() =>
        {
            GenLargeHouse(true, centerX, posY + item.spawnTileY + 3, item.BigHouseWidth, item.BigHouseHeight);

        }).ContinueWith(_ =>
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
        TileHelper.StartGen();
        int secondLast = Utils.GetUnixTimestamp;
        var item = Config.Chests[0];
        int centerX = (posX - 16) - item.ChestWidth / 2;
        return Task.Run(() =>
        {
            SpawnChest(centerX, posY + item.spawnTileY + 3, item.ClearHeight, item.ChestWidth, item.ChestCount, item.ChestLayers, true);

        }).ContinueWith(_ =>
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
        TileHelper.StartGen();
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(() =>
        {
            GenPond(posX, posY, style);
        }).ContinueWith(_ =>
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
        TileHelper.StartGen();
        int secondLast = Utils.GetUnixTimestamp;
        var item = Config.HellTunnel[0];
        var sky = Main.worldSurface * 0.3499999940395355;
        return Task.Run(() =>
        {
            if (Main.zenithWorld || Main.remixWorld) //是颠倒种子
                HellTunnel(posX, (int)sky - Config.WorldPlatform[0].WorldPlatformY, item.HellTrunnelWidth);
            else
                HellTunnel(posX, posY + item.SpawnTileY + 3, item.HellTrunnelWidth);

        }).ContinueWith(_ =>
        {
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已生成直通车，用时{value}秒。");
        });
    }
    #endregion

    #region 世界平台
    public static Task AsyncWorldPlatform(TSPlayer plr, int posY, int clear, int tile, int style)
    {
        TileHelper.StartGen();
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(() =>
        {
            WorldPlatform(posY + 3, clear, tile, style);

        }).ContinueWith(_ =>
        {
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已生成世界平台，用时{value}秒。");
        });
    }
    #endregion

    #region 战斗平台
    public static Task AsyncFightPlatforms(TSPlayer plr, int posX, int posY, int w, int h, int i, int tile, int style)
    {
        TileHelper.StartGen();
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(() =>
        {
            FightPlatforms(posX, posY + 3, w, h, i, tile, style);

        }).ContinueWith(_ =>
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
        TileHelper.StartGen();
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(() =>
        {
            WorldGen.ShimmerMakeBiome(posX, posY + 4);
            //脚下放个方块避免掉下去
            WorldGen.PlaceTile(posX, posY + 3, 38, false, true, -1, 0);
            WorldGen.PlaceTile(posX + 1, posY + 3, 38, false, true, -1, 0);
            WorldGen.PlaceTile(posX - 1, posY + 3, 38, false, true, -1, 0);

        }).ContinueWith(_ =>
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
        TileHelper.StartGen();
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(() =>
        {
            WorldGen.makeTemple(posX, posY + 16);

        }).ContinueWith(_ =>
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
        TileHelper.StartGen();
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(() =>
        {
            WorldGen.MakeDungeon(posX, posY + 52);

        }).ContinueWith(_ =>
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
        TileHelper.StartGen();
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(() =>
        {
            RockTrialField2(posY - 9, posX, Height, Width, 2);

        }).ContinueWith(_ =>
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
        // 直接使用 startY 作为基准点
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
                if (Config.DisableBlock.Contains(Main.tile[x, y].type))
                {
                    TSPlayer.All.SendInfoMessage($"[刷怪场] {x}, {y} 位置遇到禁止方块，已停止放置。");
                    return;
                }

                // 清除当前位置方块
                ClearEverything(x, y);

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
                    }
                }
                else //不在中心 左右生成半砖推怪平台
                {
                    if (Config.DisableBlock.Contains(Main.tile[x, middle - 1].type))
                    {
                        TSPlayer.All.SendInfoMessage($"[刷怪场] {x}, {middle - 1} 位置遇到禁止方块，已停止放置。");
                        return;
                    }

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
            var leftTile = Main.tile[left - 1, y2];
            var rightTile = Main.tile[right, y2];
            if (Config.DisableBlock.Contains(leftTile.type) || Config.DisableBlock.Contains(rightTile.type))
            {
                int stopX = Config.DisableBlock.Contains(leftTile.type) ? left - 1 : right;
                TSPlayer.All.SendInfoMessage($"[刷怪场] {stopX}, {y2} 位置遇到禁止方块，已停止放置。");
                return;
            }

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
        TileHelper.StartGen();
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(delegate
        {
            WorldGen.Pyramid(posX, posY + 7);

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
        TileHelper.StartGen();
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
            ClearEverything(posX, posY + 2);
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
        TileHelper.StartGen();
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(delegate
        {
            ClearEverything(posX, posY + 2);
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
        TileHelper.StartGen();
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(delegate
        {
            ClearEverything(posX, posY + 2);
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
        TileHelper.StartGen();
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(delegate
        {
            ClearEverything(posX, posY + 2);
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
        TileHelper.StartGen();
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(delegate
        {
            ClearEverything(posX, posY + 2);
            WorldGen.PlaceTile(posX, posY + 2, 187, true, false, -1, 17);

        }).ContinueWith(delegate
        {
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已生成附魔剑冢，用时{value}秒。");
        });
    }
    #endregion

    #region 腐化地
    public static Task AsyncChasmRunner(TSPlayer plr, int posX, int posY)
    {
        TileHelper.StartGen();
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(delegate
        {
            ClearEverything(posX, posY + 2);
            WorldGen.ChasmRunner(posX, posY + 7, Random.Shared.Next(10), true);

        }).ContinueWith(delegate
        {
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已生成腐化地，用时{value}秒。");
        });
    }
    #endregion

    #region 邪恶祭坛随机生成0或1（0是恶魔祭坛 1是猩红祭坛）
    public static Task AsyncAltar(TSPlayer plr, int posX, int posY)
    {
        TileHelper.StartGen();
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(delegate
        {
            ClearEverything(posX, posY + 2);
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
    public static Task AsyncClear(TSPlayer plr, int startX, int startY, int endX, int endY, int type)
    {
        TileHelper.StartGen();
        CacheArea(plr, startX, startY, endX, endY); //缓存
        int secondLast = Utils.GetUnixTimestamp;

        return Task.Run(() =>
        {
            for (int x = Math.Min(startX, endX); x <= Math.Max(startX, endX); x++)
            {
                for (int y = Math.Min(startY, endY); y <= Math.Max(startY, endY); y++)
                {
                    if (type == 1)
                    {
                        Main.tile[x, y].Clear(Terraria.DataStructures.TileDataType.Tile); //清理方块
                        NetMessage.SendTileSquare(-1, x, y, TileChangeType.None);
                    }
                    else if (type == 2)
                    {
                        WorldGen.KillWall(x, y, false); // 清除墙壁
                        NetMessage.SendTileSquare(-1, x, y, TileChangeType.None);
                    }
                    else if (type == 3)
                    {
                        WorldGen.EmptyLiquid(x, y); // 清除液体
                        NetMessage.SendTileSquare(-1, x, y, TileChangeType.None);
                    }
                    else if (type == 4)
                    {
                        Main.tile[x, y].ClearBlockPaintAndCoating(); // 清除方块的涂装和涂层
                        Main.tile[x, y].ClearWallPaintAndCoating(); // 清除墙壁的涂装和涂层
                        NetMessage.SendTileSquare(-1, x, y, TileChangeType.None);
                    }
                    else if (type == 5)
                    {
                        Main.tile[x, y].inActive(true); //清除虚化
                        NetMessage.SendTileSquare(-1, x, y, TileChangeType.None);
                    }
                    else if (type == 6)
                    {
                        WorldGen.KillWire(x, y); //清除电线
                        NetMessage.SendTileSquare(-1, x, y, TileChangeType.None);
                    }
                    else if (type == 7)
                    {
                        WorldGen.KillActuator(x, y); //清除制动器
                        NetMessage.SendTileSquare(-1, x, y, TileChangeType.None);
                    }
                    else if (type == 8)
                    {
                        ClearEverything(x, y); //清除所有
                        NetMessage.SendTileSquare(-1, x, y, TileChangeType.None);
                    }
                    else
                    {
                        plr.SendInfoMessage("请指定清理类型: \n1.清除方块 \n2.清除墙壁 \n3.清除液体 \n4.清除涂装和涂层 \n5.清除虚化 \n6.清除电线 \n7.清除制动器 \n8.清除所有");
                        return;
                    }
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
    public static Task AsyncPlaceTile(TSPlayer plr, int startX, int startY, int endX, int endY, int TileID, int type, int sy)
    {
        TileHelper.StartGen();
        CacheArea(plr, startX, startY, endX, endY); //缓存
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(() =>
        {
            for (int x = Math.Min(startX, endX); x <= Math.Max(startX, endX); x++)
            {
                for (int y = Math.Min(startY, endY); y <= Math.Max(startY, endY); y++)
                {
                    if (type == 1)
                    {
                        WorldGen.PlaceTile(x, y, TileID);
                    }
                    else if (type == 2)
                    {
                        WorldGen.ReplaceTile(x, y, (ushort)TileID, sy);
                    }
                    else if (type == 3)
                    {
                        Main.tile[x, y].Clear(Terraria.DataStructures.TileDataType.Tile); //清理方块
                        WorldGen.PlaceTile(x, y, TileID);
                    }
                    else
                    {
                        plr.SendErrorMessage("正确格式为:/spi t 1到4");
                        plr.SendInfoMessage("1 - 保留原有放置");
                        plr.SendInfoMessage("2 - 替换方块(会保留半砖属性)");
                        plr.SendInfoMessage("3 - 清完所有后放置");
                        plr.SendInfoMessage("4 - 连锁放置模式");
                        return;
                    }
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

    #region 生成墙壁
    public static Task AsyncPlaceWall(TSPlayer plr, int startX, int startY, int endX, int endY, int wallID, int type)
    {
        TileHelper.StartGen();
        CacheArea(plr, startX, startY, endX, endY); //缓存
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(() =>
        {
            for (int x = Math.Min(startX, endX); x <= Math.Max(startX, endX); x++)
            {
                for (int y = Math.Min(startY, endY); y <= Math.Max(startY, endY); y++)
                {
                    if (type == 1)
                    {
                        WorldGen.PlaceWall(x, y, wallID);
                    }
                    else if (type == 2)
                    {
                        WorldGen.KillWall(x, y, false); // 清除墙壁
                        WorldGen.PlaceWall(x, y, wallID);

                    }
                    else
                    {
                        plr.SendInfoMessage("请指定类型: 1保留原有墙壁后放置,2清理原有墙壁后放置");
                        return;
                    }
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

    #region 生成电线、虚化、制动器
    public static Task AsyncPlaceWire(TSPlayer plr, int startX, int startY, int endX, int endY, int type)
    {
        TileHelper.StartGen();
        CacheArea(plr, startX, startY, endX, endY); //缓存
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(() =>
        {
            for (int x = Math.Min(startX, endX); x <= Math.Max(startX, endX); x++)
            {
                for (int y = Math.Min(startY, endY); y <= Math.Max(startY, endY); y++)
                {
                    if (type == 1)
                    {
                        WorldGen.PlaceWire(x, y);
                    }
                    else if (type == 2)
                    {
                        WorldGen.KillWire(x, y);
                        WorldGen.PlaceWire(x, y);
                    }
                    else if (type == 3)
                    {
                        WorldGen.KillWire(x, y);
                        WorldGen.PlaceWire(x, y);
                        Main.tile[x, y].inActive(true);
                    }
                    else if (type == 4)
                    {
                        WorldGen.KillWire(x, y);
                        WorldGen.PlaceWire(x, y);
                        WorldGen.PlaceActuator(x, y);
                    }
                    else
                    {
                        plr.SendInfoMessage("请指定电路类型: \n1.只放置电线 \n2.清理电线后放置 \n3.清电线后放置并虚化方块 \n4.清电线后放置再添加制动器");
                        return;
                    }
                }
            }
        }).ContinueWith(_ =>
        {
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已生成电路，用时{value}秒。");
        });
    }
    #endregion

    #region 生成喷漆
    public static Task AsyncPlacePaint(TSPlayer plr, int startX, int startY, int endX, int endY, byte paintID, int TileOrWall, bool hasPaint)
    {
        TileHelper.StartGen();
        CacheArea(plr, startX, startY, endX, endY); //缓存
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(() =>
        {
            for (int x = Math.Min(startX, endX); x <= Math.Max(startX, endX); x++)
            {
                for (int y = Math.Min(startY, endY); y <= Math.Max(startY, endY); y++)
                {
                    if (TileOrWall == 4)
                    {
                        // 虚化切换单独处理
                        bool Inactive = Main.tile[x, y].inActive();
                        Main.tile[x, y].inActive(!Inactive);
                        continue;
                    }

                    // 根据物品类型决定使用 paint 还是 coating
                    if (hasPaint)
                    {
                        // 使用的是 paint（喷漆）
                        if (TileOrWall == 1 || TileOrWall == 3)
                        {
                            Main.tile[x, y].Clear(Terraria.DataStructures.TileDataType.TilePaint);
                            WorldGen.paintTile(x, y, paintID);
                        }
                        if (TileOrWall == 2 || TileOrWall == 3)
                        {
                            Main.tile[x, y].Clear(Terraria.DataStructures.TileDataType.WallPaint);
                            WorldGen.paintWall(x, y, paintID);
                        }
                    }
                    else
                    {
                        // 使用的是 paintCoating（涂层）
                        if (paintID > 2)
                        {
                            plr.SendErrorMessage("涂层仅支持 ID 为 1 或 2 的涂料！");
                            return;
                        }

                        if (TileOrWall == 1 || TileOrWall == 3)
                        {
                            Main.tile[x, y].Clear(Terraria.DataStructures.TileDataType.TilePaint);
                            WorldGen.paintCoatTile(x, y, paintID);
                        }
                        if (TileOrWall == 2 || TileOrWall == 3)
                        {
                            Main.tile[x, y].Clear(Terraria.DataStructures.TileDataType.WallPaint);
                            WorldGen.paintCoatWall(x, y, paintID);
                        }
                    }
                }
            }
        }).ContinueWith(_ =>
        {
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已生成喷漆，用时{value}秒。");
        });
    }
    #endregion

    #region 设为半砖
    public static Task AsyncSetSlope(TSPlayer plr, int startX, int startY, int endX, int endY, int SlopeID)
    {
        TileHelper.StartGen();
        CacheArea(plr, startX, startY, endX, endY); //缓存
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(delegate
        {
            for (int x = Math.Min(startX, endX); x <= Math.Max(startX, endX); x++)
            {
                for (int y = Math.Min(startY, endY); y <= Math.Max(startY, endY); y++)
                {
                    if (SlopeID != 5)
                        WorldGen.SlopeTile(x, y, SlopeID);
                    else
                        Main.tile[x, y].Clear(Terraria.DataStructures.TileDataType.Slope);
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

    #region 生成液体
    public static Task Asynclinquid(TSPlayer plr, int startX, int startY, int endX, int endY, int type)
    {
        TileHelper.StartGen();
        CacheArea(plr, startX, startY, endX, endY); //缓存
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(() =>
        {
            for (int x = Math.Min(startX, endX); x <= Math.Max(startX, endX); x++)
            {
                for (int y = Math.Min(startY, endY); y <= Math.Max(startY, endY); y++)
                {
                    ClearEverything(x, y);
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

    #region 还原选区指令方法
    public static Task AsyncBack(TSPlayer plr, int startX, int startY, int endX, int endY)
    {
        TileHelper.StartGen();
        int secondLast = Utils.GetUnixTimestamp;
        return Task.Run(delegate
        {
            RestoreArea(plr);

        }).ContinueWith(delegate
        {
            //还原也修一遍 避免互动家具失效
            FixAll(startX, endX, startY, endY);
            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已将选区还原，用时{value}秒。");
        });
    }
    #endregion

    #region 缓存选区原始图格方法
    private static void CacheArea(TSPlayer plr, int startX, int startY, int endX, int endY)
    {
        var snapshot = new Dictionary<Point, Terraria.Tile>();

        for (int x = Math.Min(startX, endX); x <= Math.Max(startX, endX); x++)
            for (int y = Math.Min(startY, endY); y <= Math.Max(startY, endY); y++)
                snapshot[new Point(x, y)] = (Terraria.Tile)Main.tile[x, y].Clone();

        var stack = Map.LoadBack(plr.Name);
        stack.Push(snapshot);
        Map.SaveBack(plr.Name, stack);
    }
    #endregion

    #region 还原图格方法
    private static void RestoreArea(TSPlayer plr)
    {
        var stack = Map.LoadBack(plr.Name);
        if (stack.Count == 0)
        {
            plr.SendErrorMessage("没有可还原的图格");
            return;
        }

        var snapshot = stack.Pop();
        Map.SaveBack(plr.Name, stack);

        foreach (var t in snapshot)
        {
            var pos = t.Key;
            var orig = t.Value;

            Main.tile[pos.X, pos.Y].CopyFrom(orig);
            TSPlayer.All.SendTileSquareCentered(pos.X, pos.Y, 1);
        }
    }
    #endregion

    #region 创建剪贴板数据
    private static ClipboardData CreateClipData(int startX, int startY, int endX, int endY)
    {
        int minX = Math.Min(startX, endX);
        int maxX = Math.Max(startX, endX);
        int minY = Math.Min(startY, endY);
        int maxY = Math.Max(startY, endY);

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;

        Terraria.Tile[,] tiles = new Terraria.Tile[width, height];

        // 定义箱子物品及其位置
        var chestItems = new List<ChestItemData>();
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                int indexX = x - minX;
                int indexY = y - minY;
                tiles[indexX, indexY] = (Terraria.Tile)Main.tile[x, y].Clone();

                GetChestItems(chestItems, x, y); //获取箱子物品
            }
        }

        return new ClipboardData
        {
            Width = width,
            Height = height,
            Tiles = tiles,
            Origin = new Point(minX, minY),
            ChestItems = chestItems,
        };
    }
    #endregion

    #region 异步粘贴实现
    public static Task AsyncPaste(TSPlayer plr, int startX, int startY, ClipboardData clip, bool fixChest)
    {
        TileHelper.StartGen();
        //缓存 方便粘贴错了还原
        CacheArea(plr, startX, startY, startX + clip.Width - 1, startY + clip.Height - 1);
        int secondLast = Utils.GetUnixTimestamp;

        return Task.Run(() =>
        {
            for (int x = 0; x < clip.Width; x++)
            {
                for (int y = 0; y < clip.Height; y++)
                {
                    int worldX = startX + x;
                    int worldY = startY + y;

                    // 边界检查
                    if (worldX < 0 || worldX >= Main.maxTilesX ||
                        worldY < 0 || worldY >= Main.maxTilesY) continue;

                    // 完全复制图格数据
                    Main.tile[worldX, worldY] = (Terraria.Tile)clip.Tiles![x, y].Clone();
                }
            }
        }).ContinueWith(_ =>
        {
            // 修复箱子、物品框、武器架、标牌、墓碑、广播盒、逻辑感应器、人偶模特、盘子、晶塔、稻草人、衣帽架
            FixAll(startX, startX + clip.Width - 1, startY, startY + clip.Height - 1);

            // 定义偏移坐标（从原始世界坐标到玩家头顶）
            int baseX = startX - clip.Origin.X;
            int baseY = startY - clip.Origin.Y;

            //启动配置项和使用-f都可以还原箱子物品
            if (Config.FixCopyItem || fixChest)
                RestoreChestItems(clip.ChestItems!, new Point(baseX, baseY));

            TileHelper.GenAfter();
            int value = Utils.GetUnixTimestamp - secondLast;
            plr.SendSuccessMessage($"已粘贴区域 ({clip.Width}x{clip.Height})，用时{value}秒。");
        });
    }
    #endregion

    #region 获取箱子物品方法
    private static void GetChestItems(List<ChestItemData> chestItems, int x, int y)
    {
        // 判断是否是箱子图格
        if (!TileID.Sets.BasicChest[Main.tile[x, y].type]) return;

        // 查找对应的 Chest 对象
        int index = Chest.FindChest(x, y);
        if (index < 0) return;

        Chest chest = Main.chest[index];
        if (chest == null) return;

        // 只处理箱子的左上角图格
        if (x != chest.x || y != chest.y) return;

        //定义箱子内的物品格子位置
        for (int slot = 0; slot < 40; slot++)
        {
            Item item = chest.item[slot];
            if (item?.active == true)
            {
                // 克隆物品并记录其原来所在箱子的位置
                chestItems.Add(new ChestItemData
                {
                    Item = (Item)item.DeepCopy(), // 克隆物品
                    Position = new Point(x, y), // 记录箱子位置
                    Slot = slot
                });
            }
        }
    }
    #endregion

    #region 还原箱子物品方法
    private static void RestoreChestItems(IEnumerable<ChestItemData> Chests, Point offset)
    {
        if (Chests == null || !Chests.Any()) return;

        foreach (var data in Chests)
        {
            try
            {
                if (data.Item == null || data.Item.IsAir) continue;

                // 计算新的箱子位置
                int newX = data.Position.X + offset.X;
                int newY = data.Position.Y + offset.Y;

                // 查找箱子
                int index = Chest.FindChest(newX, newY);
                if (index == -1) continue;

                //获取箱子对象
                Chest chest = Main.chest[index];

                // 检查槽位是否合法
                if (data.Slot < 0 || data.Slot >= 40) continue;

                // 克隆物品并设置到箱子槽位
                chest.item[data.Slot] = (Item)data.Item.DeepCopy();
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError($"还原箱子物品出错 @ {data.Position}: {ex}");
            }
        }
    }
    #endregion

    #region 深度复制物品数据
    public static Item DeepCopy(this Item item)
    {
        if (item == null || item.IsAir) return new Item();

        var copy = new Item();
        copy.SetDefaults(item.type);
        copy.netID = item.netID;
        copy.stack = item.stack;
        copy.prefix = item.prefix;
        copy.damage = item.damage;
        copy.useTime = item.useTime;
        copy.useAnimation = item.useAnimation;
        copy.knockBack = item.knockBack;
        copy.useStyle = item.useStyle;
        copy.value = item.value;
        copy.rare = item.rare;
        copy.expert = item.expert;
        copy.color = item.color;
        return copy;
    }
    #endregion

    #region 修复粘贴后家具无法互动：箱子、物品框、武器架、标牌、墓碑、广播盒、逻辑感应器、人偶模特、盘子、晶塔、稻草人、衣帽架
    private static void FixAll(int startX, int endX, int startY, int endY)
    {
        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                var tile = Main.tile[x, y];
                if (tile == null || !tile.active()) continue;

                //如果查找图格里是箱子
                if (TileID.Sets.BasicChest[tile.type] && Chest.FindChest(x, y) == -1)
                {
                    // 创建新的 Chest  
                    int newChest = Chest.CreateChest(x, y);
                    if (newChest == -1) continue;
                }

                // 同步箱子图格到主位置
                if (tile.type == TileID.Containers || tile.type == TileID.Containers2)
                {
                    WorldGen.SquareTileFrame(x, y, true);
                }

                //物品框
                if (tile.type == TileID.ItemFrame)
                {
                    var ItemFrame = Terraria.GameContent.Tile_Entities.TEItemFrame.Place(x, y);
                    WorldGen.SquareTileFrame(x, y, true);
                    if (ItemFrame == -1) continue;
                }

                //武器架
                if (tile.type == TileID.WeaponsRack || tile.type == TileID.WeaponsRack2)
                {
                    var WeaponsRack = Terraria.GameContent.Tile_Entities.TEWeaponsRack.Place(x, y);
                    WorldGen.SquareTileFrame(x, y, true);
                    if (WeaponsRack == -1) continue;
                }

                //标牌 墓碑  广播盒
                if ((tile.type == TileID.Signs ||
                    tile.type == TileID.Tombstones ||
                    tile.type == TileID.AnnouncementBox) &&
                    tile.frameX % 36 == 0 && tile.frameY == 0 &&
                    Sign.ReadSign(x, y, false) == -1)
                {
                    var sign = Sign.ReadSign(x, y, true);
                    if (sign == -1) continue;
                }

                //逻辑感应器
                if (tile.type == TileID.LogicSensor &&
                    Terraria.GameContent.Tile_Entities.TELogicSensor.Find(x, y) == -1)
                {
                    int LogicSensor = Terraria.GameContent.Tile_Entities.TELogicSensor.Place(x, y);
                    if (LogicSensor == -1) continue;

                    ((Terraria.GameContent.Tile_Entities.TELogicSensor)TileEntity.ByID[LogicSensor]).logicCheck = (LogicCheckType)(tile.frameY / 18 + 1);
                }

                //人体模型
                if (tile.type == TileID.DisplayDoll)
                {
                    var DisplayDoll = Terraria.GameContent.Tile_Entities.TEDisplayDoll.Place(x, y);
                    if (DisplayDoll == -1) continue;
                }

                //盘子
                if (tile.type == TileID.FoodPlatter)
                {
                    var FoodPlatter = Terraria.GameContent.Tile_Entities.TEFoodPlatter.Place(x, y);
                    WorldGen.SquareTileFrame(x, y, true);
                    if (FoodPlatter == -1) continue;
                }

                // 是否修复晶塔
                if (Config.FixCopyPylon)
                {
                    if (tile.type == TileID.TeleportationPylon)
                    {
                        var TeleportationPylon = Terraria.GameContent.Tile_Entities.TETeleportationPylon.Place(x, y);
                        if (TeleportationPylon == -1) continue;
                    }
                }

                //训练假人（稻草人）
                if (tile.type == TileID.TargetDummy)
                {
                    var TrainingDummy = Terraria.GameContent.Tile_Entities.TETrainingDummy.Place(x, y);
                    if (TrainingDummy == -1) continue;
                }

                //衣帽架
                if (tile.type == TileID.HatRack)
                {
                    var HatRack = Terraria.GameContent.Tile_Entities.TEHatRack.Place(x, y);
                    WorldGen.SquareTileFrame(x, y, true);
                    if (HatRack == -1) continue;
                }
            }
        }
    }
    #endregion
}
