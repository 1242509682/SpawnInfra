using System.IO.Compression;
using Microsoft.Xna.Framework;
using Terraria;
using TShockAPI;
using Terraria.GameContent.Tile_Entities;
using static SpawnInfra.Plugin;

namespace SpawnInfra;

public class Map
{
    //存储图格数据的目录
    internal static readonly string DataDir = Path.Combine(TShock.SavePath, "SPIData");

    #region GZip 压缩辅助方法
    private static Stream GZipWrite(string filePath)
    {
        var fileStream = new FileStream(filePath, FileMode.Create);
        return new GZipStream(fileStream, CompressionLevel.Optimal);
    }

    private static Stream GZipRead(string filePath)
    {
        var fileStream = new FileStream(filePath, FileMode.Open);
        return new GZipStream(fileStream, CompressionMode.Decompress);
    }
    #endregion

    #region 加载原始图格方法
    public static Stack<ClipboardData> LoadBack(string name)
    {
        string filePath = Path.Combine(DataDir, $"{name}_bk.dat");
        if (!File.Exists(filePath)) return new Stack<ClipboardData>();

        var stack = new Stack<ClipboardData>();
        using (var fs = GZipRead(filePath))
        using (var reader = new BinaryReader(fs))
        {
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                stack.Push(LoadBuilding(reader));
            }
        }
        return stack;
    }
    #endregion

    #region 保存原始图格方法
    public static void SaveBack(string name, Stack<ClipboardData> stack)
    {
        Directory.CreateDirectory(DataDir);
        string filePath = Path.Combine(DataDir, $"{name}_bk.dat");

        using (var fs = GZipWrite(filePath))
        using (var writer = new BinaryWriter(fs))
        {
            writer.Write(stack.Count);
            foreach (var building in stack)
            {
                SaveBuilding(writer, building);
            }
        }
    }
    #endregion

    #region 读取剪贴板方法
    internal static ClipboardData LoadClip(string name)
    {
        string filePath = Path.Combine(DataDir, $"{name}_cp.dat");
        if (!File.Exists(filePath)) return null!;

        using (var fs = GZipRead(filePath))
        using (var reader = new BinaryReader(fs))
        {
            return LoadBuilding(reader);
        }
    }
    #endregion

    #region 保存剪贴板方法
    internal static void SaveClip(string name, ClipboardData clip)
    {
        Directory.CreateDirectory(DataDir);
        string filePath = Path.Combine(DataDir, $"{name}_cp.dat");

        using (var fs = GZipWrite(filePath))
        using (var writer = new BinaryWriter(fs))
        {
            SaveBuilding(writer, clip);
        }
    }
    #endregion

    #region 把建筑写入到内存方法
    private static void SaveBuilding(BinaryWriter writer, ClipboardData clip)
    {
        writer.Write(clip.Origin.X);
        writer.Write(clip.Origin.Y);
        writer.Write(clip.Width);
        writer.Write(clip.Height);

        if (clip.Tiles == null) return;

        #region 写入图格
        for (int x = 0; x < clip.Width; x++)
        {
            for (int y = 0; y < clip.Height; y++)
            {
                var tile = clip.Tiles[x, y];
                writer.Write(tile.bTileHeader);
                writer.Write(tile.bTileHeader2);
                writer.Write(tile.bTileHeader3);
                writer.Write(tile.frameX);
                writer.Write(tile.frameY);
                writer.Write(tile.liquid);
                writer.Write(tile.sTileHeader);
                writer.Write(tile.type);
                writer.Write(tile.wall);
            }
        }
        #endregion

        #region 写入箱子物品
        writer.Write(clip.ChestItems?.Count ?? 0);
        if (clip.ChestItems != null)
        {
            foreach (var data in clip.ChestItems)
            {
                writer.Write(data.Position.X);
                writer.Write(data.Position.Y);
                writer.Write(data.Slot);
                writer.Write(data.Item?.type ?? 0);
                writer.Write(data.Item?.netID ?? 0);
                writer.Write(data.Item?.stack ?? 0);
                writer.Write(data.Item?.prefix ?? 0);
            }
        }
        #endregion

        #region 写入标牌信息
        writer.Write(clip.Signs?.Count ?? 0);
        if (clip.Signs != null)
        {
            foreach (var sign in clip.Signs)
            {
                writer.Write(sign.x - clip.Origin.X); // 相对坐标
                writer.Write(sign.y - clip.Origin.Y);
                writer.Write(sign.text ?? "");
            }
        }
        #endregion

        #region 写入物品框物品
        writer.Write(clip.ItemFrames?.Count ?? 0);
        if (clip.ItemFrames != null)
        {
            foreach (var data in clip.ItemFrames)
            {
                writer.Write(data.Position.X - clip.Origin.X); // 存储相对坐标
                writer.Write(data.Position.Y - clip.Origin.Y);
                writer.Write(data.Item.NetId);
                writer.Write(data.Item.Stack);
                writer.Write(data.Item.PrefixId);
            }
        }
        #endregion

        #region 写入武器架物品
        writer.Write(clip.WeaponsRacks?.Count ?? 0);
        if (clip.WeaponsRacks != null)
        {
            foreach (var data in clip.WeaponsRacks)
            {
                writer.Write(data.Position.X - clip.Origin.X);
                writer.Write(data.Position.Y - clip.Origin.Y);
                writer.Write(data.Item.NetId);
                writer.Write(data.Item.Stack);
                writer.Write(data.Item.PrefixId);
            }
        }
        #endregion

        #region 写入盘子物品
        writer.Write(clip.FoodPlatters?.Count ?? 0);
        if (clip.FoodPlatters != null)
        {
            foreach (var data in clip.FoodPlatters)
            {
                writer.Write(data.Position.X - clip.Origin.X);
                writer.Write(data.Position.Y - clip.Origin.Y);
                writer.Write(data.Item.NetId);
                writer.Write(data.Item.Stack);
                writer.Write(data.Item.PrefixId);
            }
        }
        #endregion

        #region 写入人偶物品
        writer.Write(clip.DisplayDolls?.Count ?? 0);
        if (clip.DisplayDolls != null)
        {
            foreach (var doll in clip.DisplayDolls)
            {
                // 保存相对坐标
                writer.Write(doll.Position.X - clip.Origin.X);
                writer.Write(doll.Position.Y - clip.Origin.Y);

                // 保存物品数据 (8个槽位)
                writer.Write(doll.Items.Length);
                foreach (var item in doll.Items)
                {
                    writer.Write(item.NetId);
                    writer.Write(item.Stack);
                    writer.Write(item.PrefixId);
                }

                // 保存染料数据 (8个槽位)
                writer.Write(doll.Dyes.Length);
                foreach (var dye in doll.Dyes)
                {
                    writer.Write(dye.NetId);
                    writer.Write(dye.Stack);
                    writer.Write(dye.PrefixId);
                }
            }
        }
        #endregion

        #region 写入衣帽架物品
        writer.Write(clip.HatRacks?.Count ?? 0);
        if (clip.HatRacks != null)
        {
            foreach (var Rack in clip.HatRacks)
            {
                // 保存相对坐标
                writer.Write(Rack.Position.X - clip.Origin.X);
                writer.Write(Rack.Position.Y - clip.Origin.Y);

                // 保存物品数据 (2个槽位)
                writer.Write(Rack.Items.Length);
                foreach (var item in Rack.Items)
                {
                    writer.Write(item.NetId);
                    writer.Write(item.Stack);
                    writer.Write(item.PrefixId);
                }

                // 保存染料数据 (2个槽位)
                writer.Write(Rack.Dyes.Length);
                foreach (var dye in Rack.Dyes)
                {
                    writer.Write(dye.NetId);
                    writer.Write(dye.Stack);
                    writer.Write(dye.PrefixId);
                }
            }
        }
        #endregion

        #region 写入逻辑感应器检查类型
        writer.Write(clip.LogicSensors?.Count ?? 0);
        if (clip.LogicSensors != null)
        {
            foreach (var data in clip.LogicSensors)
            {
                writer.Write(data.Position.X - clip.Origin.X);
                writer.Write(data.Position.Y - clip.Origin.Y);
                writer.Write((int)data.type);
            }
        }
        #endregion

    }
    #endregion

    #region 从内存加载建筑方法
    private static ClipboardData LoadBuilding(BinaryReader reader)
    {
        int originX = reader.ReadInt32();
        int originY = reader.ReadInt32();
        int width = reader.ReadInt32();
        int height = reader.ReadInt32();

        #region 读取图格物品数据
        var tiles = new Terraria.Tile[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var tile = new Terraria.Tile
                {
                    bTileHeader = reader.ReadByte(),
                    bTileHeader2 = reader.ReadByte(),
                    bTileHeader3 = reader.ReadByte(),
                    frameX = reader.ReadInt16(),
                    frameY = reader.ReadInt16(),
                    liquid = reader.ReadByte(),
                    sTileHeader = reader.ReadUInt16(),
                    type = reader.ReadUInt16(),
                    wall = reader.ReadUInt16()
                };
                tiles[x, y] = tile;
            }
        }
        #endregion

        #region 读取箱子物品数据
        int chestItemCount = reader.ReadInt32();
        var chestItems = new List<ChestItems>(chestItemCount);
        for (int i = 0; i < chestItemCount; i++)
        {
            int posX = reader.ReadInt32();
            int posY = reader.ReadInt32();
            int slot = reader.ReadInt32();
            int type = reader.ReadInt32();
            int netId = reader.ReadInt32();
            int stack = reader.ReadInt32();
            byte prefix = reader.ReadByte();

            var item = new Item();
            item.SetDefaults(type);
            item.netID = netId;
            item.stack = stack;
            item.prefix = prefix;

            chestItems.Add(new ChestItems
            {
                Position = new Point(posX, posY),
                Slot = slot,
                Item = item
            });
        }
        #endregion

        #region 读取标牌信息内容
        int signCount = reader.ReadInt32();
        var signs = new List<Sign>(signCount);
        for (int i = 0; i < signCount; i++)
        {
            int relX = reader.ReadInt32();
            int relY = reader.ReadInt32();
            string text = reader.ReadString();

            signs.Add(new Sign
            {
                x = originX + relX,
                y = originY + relY,
                text = text
            });
        }
        #endregion

        #region 读取物品框物品数据
        int itemFrameCount = reader.ReadInt32();
        var itemFrames = new List<ItemFrames>(itemFrameCount);
        for (int i = 0; i < itemFrameCount; i++)
        {
            int relX = reader.ReadInt32();
            int relY = reader.ReadInt32();
            int netId = reader.ReadInt32();
            int stack = reader.ReadInt32();
            byte prefix = reader.ReadByte();

            itemFrames.Add(new ItemFrames
            {
                Position = new Point(originX + relX, originY + relY),
                Item = new NetItem(netId, stack, prefix)
            });
        }
        #endregion

        #region 读取武器架物品数据
        int weaponRackCount = reader.ReadInt32();
        var weaponRacks = new List<WRacks>(weaponRackCount);
        for (int i = 0; i < weaponRackCount; i++)
        {
            int relX = reader.ReadInt32();
            int relY = reader.ReadInt32();
            int netId = reader.ReadInt32();
            int stack = reader.ReadInt32();
            byte prefix = reader.ReadByte();

            weaponRacks.Add(new WRacks
            {
                Position = new Point(originX + relX, originY + relY),
                Item = new NetItem(netId, stack, prefix)
            });
        }
        #endregion

        #region 读取盘子物品数据
        int foodPlatterCount = reader.ReadInt32();
        var foodPlatters = new List<FPlatters>(foodPlatterCount);
        for (int i = 0; i < foodPlatterCount; i++)
        {
            int relX = reader.ReadInt32();
            int relY = reader.ReadInt32();
            int netId = reader.ReadInt32();
            int stack = reader.ReadInt32();
            byte prefix = reader.ReadByte();

            foodPlatters.Add(new FPlatters
            {
                Position = new Point(originX + relX, originY + relY),
                Item = new NetItem(netId, stack, prefix)
            });
        }
        #endregion

        #region 读取人偶物品数据
        int dollCount = reader.ReadInt32();
        var displayDolls = new List<DDolls>(dollCount);
        for (int i = 0; i < dollCount; i++)
        {
            int relX = reader.ReadInt32();
            int relY = reader.ReadInt32();

            // 读取物品
            int itemCount = reader.ReadInt32();
            var items = new NetItem[itemCount];
            for (int j = 0; j < itemCount; j++)
            {
                items[j] = new NetItem(
                    reader.ReadInt32(),
                    reader.ReadInt32(),
                    reader.ReadByte()
                );
            }

            // 读取染料
            int dyeCount = reader.ReadInt32();
            var dyes = new NetItem[dyeCount];
            for (int j = 0; j < dyeCount; j++)
            {
                dyes[j] = new NetItem(
                    reader.ReadInt32(),
                    reader.ReadInt32(),
                    reader.ReadByte()
                );
            }

            displayDolls.Add(new DDolls
            {
                Position = new Point(originX + relX, originY + relY),
                Items = items,
                Dyes = dyes
            });
        }
        #endregion

        #region 读取衣帽架物品数据
        int RackCount = reader.ReadInt32();
        var hatRacks = new List<HatRacks>(RackCount);
        for (int i = 0; i < RackCount; i++)
        {
            int relX = reader.ReadInt32();
            int relY = reader.ReadInt32();

            // 读取物品
            int itemCount = reader.ReadInt32();
            var items = new NetItem[itemCount];
            for (int j = 0; j < itemCount; j++)
            {
                items[j] = new NetItem(
                    reader.ReadInt32(),
                    reader.ReadInt32(),
                    reader.ReadByte()
                );
            }

            // 读取染料
            int dyeCount = reader.ReadInt32();
            var dyes = new NetItem[dyeCount];
            for (int j = 0; j < dyeCount; j++)
            {
                dyes[j] = new NetItem(
                    reader.ReadInt32(),
                    reader.ReadInt32(),
                    reader.ReadByte()
                );
            }

            hatRacks.Add(new HatRacks
            {
                Position = new Point(originX + relX, originY + relY),
                Items = items,
                Dyes = dyes
            });
        }
        #endregion

        #region 读取逻辑灯开关数据
        int LogicSensorsCount = reader.ReadInt32();
        var logicSensors = new List<LogicSensors>(LogicSensorsCount);
        for (int i = 0; i < LogicSensorsCount; i++)
        {
            int relX = reader.ReadInt32();
            int relY = reader.ReadInt32();
            int type = reader.ReadInt32();

            logicSensors.Add(new LogicSensors
            {
                Position = new Point(originX + relX, originY + relY),
                type = (TELogicSensor.LogicCheckType)reader.ReadInt32()
            });
        }
        #endregion

        return new ClipboardData
        {
            Origin = new Point(originX, originY),
            Width = width,
            Height = height,
            Tiles = tiles,
            ChestItems = chestItems,
            Signs = signs,
            ItemFrames = itemFrames,
            WeaponsRacks = weaponRacks,
            FoodPlatters = foodPlatters,
            DisplayDolls = displayDolls,
            HatRacks = hatRacks,
            LogicSensors = logicSensors
        };
    }
    #endregion

    #region 获取所有已存在的剪贴板名称
    public static List<string> GetAllClipNames()
    {
        if (!Directory.Exists(Map.DataDir))
            return new List<string>();

        return Directory.GetFiles(Map.DataDir, "*_cp.dat")
                        .Select(f => Path.GetFileNameWithoutExtension(f).Replace("_cp", ""))
                        .ToList();
    }
    #endregion

    #region 备份并压缩所有 .dat 文件后删除
    public static void BackupAndDeleteAllDataFiles()
    {
        if (!Directory.Exists(Map.DataDir)) return;

        // 构建压缩包保存路径
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string backupFolder = Path.Combine(Map.DataDir, $"{timestamp}");
        string zipFilePath = Path.Combine(Map.DataDir, $"{timestamp}.zip");

        try
        {
            //重置是否备份建筑
            if (Config.BackerAllDataFiles)
            {
                // 创建临时备份文件夹
                Directory.CreateDirectory(backupFolder);

                // 将所有 .dat 文件复制到备份文件夹
                foreach (var file in Directory.GetFiles(Map.DataDir, "*_cp.dat"))
                {
                    string destFile = Path.Combine(backupFolder, Path.GetFileName(file));
                    File.Copy(file, destFile, overwrite: true);
                }

                // 压缩文件夹为 .zip
                ZipFile.CreateFromDirectory(backupFolder, zipFilePath, CompressionLevel.SmallestSize, false);

                // 删除临时文件夹（不再需要了）
                Directory.Delete(backupFolder, recursive: true);

                TShock.Log.ConsoleInfo($"已成功备份所有 .dat 文件，压缩包保存于:\n {zipFilePath}");
            }

            //重置是否清理建筑
            if (Config.DeleteAllDataFiles)
            {
                // 删除原始 .dat 文件
                foreach (var file in Directory.GetFiles(Map.DataDir, "*.dat"))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        TShock.Log.ConsoleInfo($"删除文件失败: {file}, 错误: {ex.Message}");
                    }
                }

                TShock.Log.ConsoleInfo($"已成功删除所有 .dat 文件");
            }
        }
        catch (Exception ex)
        {
            TShock.Log.ConsoleInfo($"备份和删除过程中出错: {ex.Message}");
        }
    }

    #endregion
}