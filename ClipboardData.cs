using Terraria;
using Microsoft.Xna.Framework;

namespace SpawnInfra;

internal class ClipboardData
{
    public int Width { get; set; }
    public int Height { get; set; }
    public Tile[,] Tiles { get; set; }
    public Point Origin { get; set; } // 原始选区位置
}
