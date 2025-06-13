using System.IO.Compression;
using Microsoft.Xna.Framework;
using Terraria;
using TShockAPI;
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

        // 箱子物品
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

        // 标牌
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
    }
    #endregion

    #region 从内存加载建筑方法
    private static ClipboardData LoadBuilding(BinaryReader reader)
    {
        int originX = reader.ReadInt32();
        int originY = reader.ReadInt32();
        int width = reader.ReadInt32();
        int height = reader.ReadInt32();

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

        // 箱子物品
        int chestItemCount = reader.ReadInt32();
        var chestItems = new List<ChestItemData>(chestItemCount);
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

            chestItems.Add(new ChestItemData
            {
                Position = new Point(posX, posY),
                Slot = slot,
                Item = item
            });
        }

        // 标牌
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

        return new ClipboardData
        {
            Origin = new Point(originX, originY),
            Width = width,
            Height = height,
            Tiles = tiles,
            ChestItems = chestItems,
            Signs = signs
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