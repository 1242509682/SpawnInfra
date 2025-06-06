using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using TShockAPI;

namespace SpawnInfra
{
    public class Map
    {
        //存储图格数据的目录
        internal static readonly string DataDir = Path.Combine(TShock.SavePath, "SPIData");

        #region 加载磁盘中的原始图格写到内存方法
        public static Stack<Dictionary<Point, Terraria.Tile>> LoadBack(string name)
        {
            string filePath = Path.Combine(DataDir, $"{name}_bk.dat");
            if (!File.Exists(filePath)) return new Stack<Dictionary<Point, Terraria.Tile>>();

            var stack = new Stack<Dictionary<Point, Terraria.Tile>>();
            using (var fs = new FileStream(filePath, FileMode.Open))
            using (var reader = new BinaryReader(fs))
            {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    int snapshotSize = reader.ReadInt32();
                    var snapshot = new Dictionary<Point, Terraria.Tile>(snapshotSize);

                    for (int j = 0; j < snapshotSize; j++)
                    {
                        int x = reader.ReadInt32();
                        int y = reader.ReadInt32();
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
                        snapshot[new Point(x, y)] = tile;
                    }
                    stack.Push(snapshot);
                }
            }
            return stack;
        }
        #endregion

        #region 保存原始图格从内存写到磁盘方法
        public static void SaveBack(string name, Stack<Dictionary<Point, Terraria.Tile>> stack)
        {
            Directory.CreateDirectory(DataDir);
            string filePath = Path.Combine(DataDir, $"{name}_bk.dat");

            using (var fs = new FileStream(filePath, FileMode.Create))
            using (var writer = new BinaryWriter(fs))
            {
                writer.Write(stack.Count);
                foreach (var snapshot in stack)
                {
                    writer.Write(snapshot.Count);
                    foreach (var pair in snapshot)
                    {
                        writer.Write(pair.Key.X);
                        writer.Write(pair.Key.Y);
                        var tile = pair.Value;
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
            }
        } 
        #endregion

        #region 剪贴板持久化方法
        internal static ClipboardData LoadClip(string name)
        {
            string filePath = Path.Combine(DataDir, $"{name}_cp.dat");
            if (!File.Exists(filePath)) return null!;

            using (var fs = new FileStream(filePath, FileMode.Open))
            using (var reader = new BinaryReader(fs))
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

                return new ClipboardData
                {
                    Origin = new Point(originX, originY),
                    Width = width,
                    Height = height,
                    Tiles = tiles
                };
            }
        }

        internal static void SaveClip(string name, ClipboardData clipboard)
        {
            Directory.CreateDirectory(DataDir);
            string filePath = Path.Combine(DataDir, $"{name}_cp.dat");

            using (var fs = new FileStream(filePath, FileMode.Create))
            using (var writer = new BinaryWriter(fs))
            {
                writer.Write(clipboard.Origin.X);
                writer.Write(clipboard.Origin.Y);
                writer.Write(clipboard.Width);
                writer.Write(clipboard.Height);

                for (int x = 0; x < clipboard.Width; x++)
                {
                    for (int y = 0; y < clipboard.Height; y++)
                    {
                        var tile = clipboard.Tiles![x, y];
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
            }
        }
        #endregion
    }
}