using TShockAPI;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace SpawnInfra
{
    internal class Utils
    {
        public static int GetUnixTimestamp => (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        public static bool NeedInGame(TSPlayer plr)
        {
            if (!plr.RealPlayer)
            {
                plr.SendErrorMessage("请进入游戏后再操作！");
            }
            return !plr.RealPlayer;
        }

        public static void ClearEverything(int x, int y)
        {
            Main.tile[x, y].ClearEverything();
            NetMessage.SendTileSquare(-1, x, y, TileChangeType.None);
        }
    }
}
