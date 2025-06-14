using TShockAPI;
using Terraria;
using Terraria.ID;
using static SpawnInfra.Plugin;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.GameContent.Tile_Entities;
using Terraria.GameContent.Biomes.CaveHouse;

namespace SpawnInfra;

internal static class Utils
{
    #region 判断是否在游戏方法（用于控制台）
    public static bool NeedInGame(TSPlayer plr)
    {
        if (!plr.RealPlayer)
        {
            plr.SendErrorMessage("请进入游戏后再操作！");
        }
        return !plr.RealPlayer;
    }
    #endregion

    #region 清理一切方法
    public static void ClearEverything(int x, int y)
    {
        Main.tile[x, y].ClearEverything();
        NetMessage.SendTileSquare(-1, x, y, TileChangeType.None);
    }
    #endregion

    #region 缓存原始图格
    public static void SaveOrigTile(TSPlayer plr, int startX, int startY, int endX, int endY)
    {
        var building = CopyBuilding(startX, startY, endX, endY);
        var stack = Map.LoadBack(plr.Name);
        stack.Push(building);
        Map.SaveBack(plr.Name, stack);
    }
    #endregion

    #region 还原图格实体方法
    public static void RollbackBuilding(TSPlayer plr)
    {
        var stack = Map.LoadBack(plr.Name);
        if (stack.Count == 0)
        {
            plr.SendErrorMessage("没有可还原的图格");
            return;
        }

        var building = stack.Pop();
        Map.SaveBack(plr.Name, stack);

        // 计算选区边界
        int startX = building.Origin.X;
        int startY = building.Origin.Y;
        int endX = startX + building.Width - 1;
        int endY = startY + building.Height - 1;

        if (building.Tiles == null) return;

        // 0. 还原前先销毁当前区域的互动家具实体
        KillAll(startX, endX, startY, endY);

        // 1. 先还原图格数据
        for (int x = 0; x < building.Width; x++)
        {
            for (int y = 0; y < building.Height; y++)
            {
                int worldX = startX + x;
                int worldY = startY + y;

                if (worldX < 0 || worldX >= Main.maxTilesX ||
                    worldY < 0 || worldY >= Main.maxTilesY) continue;

                Main.tile[worldX, worldY].CopyFrom(building.Tiles[x, y]);
                TSPlayer.All.SendTileSquareCentered(worldX, worldY, 1);
            }
        }

        // 2. 修复实体
        FixAll(startX, endX, startY, endY);

        // 3. 恢复箱子物品（依赖已存在的箱子实体）
        if (building.ChestItems != null)
        {
            RestoreChestItems(building.ChestItems, new Point(0, 0));
        }

        // 4. 恢复标牌信息
        RestoreSignText(building, 0, 0);

        // 5. 物品框、盘子、武器架、人偶、衣帽架、逻辑感应器
        RestoreItemFrames(building.ItemFrames, new Point(0, 0));
        RestorefoodPlatter(building.FoodPlatters, new Point(0, 0));
        RestoreWeaponsRack(building.WeaponsRacks, new Point(0, 0));
        RestoreDisplayDoll(building.DisplayDolls, new Point(0, 0));
        RestoreHatRack(building.HatRacks, new Point(0, 0));
        RestoreLogicSensor(building.LogicSensors, new Point(0, 0));
    }
    #endregion

    #region 创建剪贴板数据
    public static ClipboardData CopyBuilding(int startX, int startY, int endX, int endY)
    {
        int minX = Math.Min(startX, endX);
        int maxX = Math.Max(startX, endX);
        int minY = Math.Min(startY, endY);
        int maxY = Math.Max(startY, endY);

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;

        Terraria.Tile[,] tiles = new Terraria.Tile[width, height];

        var chestItems = new List<ChestItems>(); // 用于存储箱子数据
        var signs = new List<Sign>(); // 用于存储标牌数据
        var itemFrames = new List<ItemFrames>(); // 用于存储物品框数据
        var weaponsRack = new List<WRacks>(); //武器架物品
        var foodPlatter = new List<FPlatters>(); //盘子物品
        var displayDolls = new List<DDolls>(); //人偶物品
        var hatRacks = new List<HatRacks>();  //衣帽架物品
        var logicSensor = new List<LogicSensors>(); //逻辑感应器
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                int indexX = x - minX;
                int indexY = y - minY;
                tiles[indexX, indexY] = (Terraria.Tile)Main.tile[x, y].Clone();

                GetChestItems(chestItems, x, y); //获取箱子物品
                GetSign(signs, x, y); //获取标牌、广播盒、墓碑上等可阅读家具的信息
                GetItemFrames(itemFrames, x, y); //获取物品框的物品
                GetWeaponsRack(weaponsRack, x, y);  //获取武器架的物品
                GetfoodPlatter(foodPlatter, x, y);  //获取盘子的物品
                GetDisplayDoll(displayDolls, x, y); //获取人偶的物品
                GetHatRack(hatRacks, x, y); //获取衣帽架物品
                GetLogicSensor(logicSensor, x, y); //获取逻辑感应器开关状态
            }
        }

        return new ClipboardData
        {
            Width = width,
            Height = height,
            Tiles = tiles,
            Origin = new Point(minX, minY),
            ChestItems = chestItems,
            Signs = signs,
            ItemFrames = itemFrames,
            WeaponsRacks = weaponsRack,
            FoodPlatters = foodPlatter,
            DisplayDolls = displayDolls,
            HatRacks = hatRacks,
            LogicSensors = logicSensor,
        };
    }
    #endregion

    #region 获取 与 还原箱子物品方法
    public static void GetChestItems(List<ChestItems> chestItems, int x, int y)
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
                chestItems.Add(new ChestItems
                {
                    Item = (Item)item.Clone(), // 克隆物品
                    Position = new Point(x, y), // 记录箱子位置
                    Slot = slot
                });
            }
        }
    }

    public static void RestoreChestItems(IEnumerable<ChestItems> Chests, Point offset)
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
                chest.item[data.Slot] = (Item)data.Item.Clone();
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError($"还原箱子物品出错 @ {data.Position}: {ex}");
            }
        }
    }
    #endregion

    #region 获取 与 还原标牌内容方法
    public static void GetSign(List<Sign> signs, int x, int y)
    {
        if (Main.tile[x, y]?.active() == true && Main.tileSign[Main.tile[x, y].type])
        {
            // 获取 Sign ID
            int signId = Sign.ReadSign(x, y);
            if (signId >= 0)
            {
                Sign origSign = Main.sign[signId];
                if (origSign != null)
                {
                    signs.Add(new Sign
                    {
                        x = x,
                        y = y,
                        text = origSign.text ?? ""
                    });
                }
            }
        }
    }

    private static int CreateNewSign(int x, int y)
    {
        // 查找是否有重复的 Sign
        for (int i = 0; i < Sign.maxSigns; i++)
        {
            if (Main.sign[i] != null && Main.sign[i].x == x && Main.sign[i].y == y)
            {
                return i;
            }
        }

        // 找一个空槽位插入新的 Sign
        for (int i = 0; i < Sign.maxSigns; i++)
        {
            if (Main.sign[i] == null)
            {
                Main.sign[i] = new Sign
                {
                    x = x,
                    y = y,
                    text = ""
                };
                return i;
            }
        }

        return -1; // 没有可用槽位
    }

    public static void RestoreSignText(ClipboardData clip, int baseX, int baseY)
    {
        if (clip.Signs != null)
        {
            foreach (var sign in clip.Signs)
            {
                int newX = sign.x + baseX;
                int newY = sign.y + baseY;

                if (newX < 0 || newY < 0 || newX >= Main.maxTilesX || newY >= Main.maxTilesY)
                    continue;

                var tile = Main.tile[newX, newY];
                if (tile == null || !tile.active() || !Main.tileSign[tile.type]) continue;

                // 创建新的 Sign 或复用已有的
                int signId = CreateNewSign(newX, newY);
                if (signId >= 0)
                {
                    Main.sign[signId].text = sign.text;
                }
            }
        }
    }
    #endregion

    #region 获取 与 还原物品框物品方法
    public static void GetItemFrames(List<ItemFrames> itemFrames, int x, int y)
    {
        var tile = Main.tile[x, y];
        if (tile.type != TileID.ItemFrame) return;
        if (tile.frameX % 36 != 0 || tile.frameY != 0) return;

        var id = TEItemFrame.Find(x, y);
        if (id != -1)
        {
            var frame = (TEItemFrame)TileEntity.ByID[id];
            itemFrames.Add(new ItemFrames()
            {
                Item = new NetItem(frame.item.netID, frame.item.stack, frame.item.prefix),
                Position = new Point(x, y),
            });
        }
    }

    public static void RestoreItemFrames(List<ItemFrames> itemFrames, Point offset)
    {
        if (itemFrames == null || !itemFrames.Any()) return;

        foreach (var data in itemFrames)
        {
            // 计算新的物品框位置
            int newX = data.Position.X + offset.X;
            int newY = data.Position.Y + offset.Y;

            // 获取图格信息
            var tile = Main.tile[newX, newY];
            if (tile == null || !tile.active() || tile.type != TileID.ItemFrame)
                continue;

            // 查找或创建 TEItemFrame（TileEntity）
            int id = TEItemFrame.Find(newX, newY);
            if (id == -1)
            {
                id = TEItemFrame.Place(newX, newY);
                if (id == -1)
                    continue; // 创建失败
            }

            // 获取物品框实体
            var frame = (TEItemFrame)TileEntity.ByID[id];

            // 创建一个实际的物品对象并复制数据
            Item item = new Item();
            item.netDefaults(data.Item.NetId); // 设置物品 ID
            item.stack = data.Item.Stack;       // 设置数量
            item.prefix = data.Item.PrefixId;     // 设置前缀

            // 更新物品框内的物品
            frame.item = item;
        }
    }
    #endregion

    #region 获取 与 还原武器架物品方法
    public static void GetWeaponsRack(List<WRacks> WRack, int x, int y)
    {
        var tile = Main.tile[x, y];
        if (tile.frameX % 54 != 0 || tile.frameY != 0) return;
        if (tile.type == TileID.WeaponsRack2)
        {
            var id = TEWeaponsRack.Find(x, y);
            if (id != -1)
            {
                var rack = (TEWeaponsRack)TileEntity.ByID[id];
                WRack.Add(new WRacks()
                {
                    Item = new NetItem(rack.item.netID, rack.item.stack, rack.item.prefix),
                    Position = new Point(x, y),
                });
            }
        }
    }

    public static void RestoreWeaponsRack(List<WRacks> WRack, Point offset)
    {
        if (WRack == null || !WRack.Any()) return;

        foreach (var data in WRack)
        {
            int newX = data.Position.X + offset.X;
            int newY = data.Position.Y + offset.Y;

            var tile = Main.tile[newX, newY];
            if (tile == null || !tile.active() || tile.type != TileID.WeaponsRack2) continue;

            int id = TEWeaponsRack.Find(newX, newY);
            if (id == -1)
            {
                id = TEWeaponsRack.Place(newX, newY);
                if (id == -1)
                    continue; // 创建失败
            }

            var rack = (TEWeaponsRack)TileEntity.ByID[id];

            // 创建一个实际的物品对象并复制数据
            Item item = new Item();
            item.netDefaults(data.Item.NetId); // 设置物品 ID
            item.stack = data.Item.Stack;       // 设置数量
            item.prefix = data.Item.PrefixId;     // 设置前缀

            rack.item = item;
        }
    }
    #endregion

    #region 获取 与 还原盘子物品方法
    public static void GetfoodPlatter(List<FPlatters> FPlatter, int x, int y)
    {
        var tile = Main.tile[x, y];
        if (tile.type == TileID.FoodPlatter)
        {
            var id = TEFoodPlatter.Find(x, y);
            if (id != -1)
            {
                var platter = (TEFoodPlatter)TileEntity.ByID[id];
                FPlatter.Add(new FPlatters()
                {
                    Item = new NetItem(platter.item.netID, platter.item.stack, platter.item.prefix),
                    Position = new Point(x, y),
                });
            }
        }
    }

    public static void RestorefoodPlatter(List<FPlatters> FPlatter, Point offset)
    {
        if (FPlatter == null || !FPlatter.Any()) return;

        foreach (var data in FPlatter)
        {
            int newX = data.Position.X + offset.X;
            int newY = data.Position.Y + offset.Y;

            var tile = Main.tile[newX, newY];
            if (tile == null || !tile.active() ||
                tile.type != TileID.FoodPlatter) continue;

            int id = TEFoodPlatter.Find(newX, newY);
            if (id == -1)
            {
                id = TEFoodPlatter.Place(newX, newY);
                if (id == -1)
                    continue; // 创建失败
            }

            var food = (TEFoodPlatter)TileEntity.ByID[id];
            // 创建一个实际的物品对象并复制数据
            Item item = new Item();
            item.netDefaults(data.Item.NetId); // 设置物品 ID
            item.stack = data.Item.Stack;       // 设置数量
            item.prefix = data.Item.PrefixId;     // 设置前缀

            food.item = item;
        }
    }
    #endregion

    #region 获取 与 还原人偶物品方法
    public static void GetDisplayDoll(List<DDolls> Dolls, int x, int y)
    {
        var tile = Main.tile[x, y];
        if (tile.type == TileID.DisplayDoll)
        {
            var id = TEDisplayDoll.Find(x, y);
            if (id != -1)
            {
                var doll = (TEDisplayDoll)TileEntity.ByID[id];
                Dolls.Add(new DDolls()
                {
                    Items = doll._items.Select(i => new NetItem(i.netID, i.stack, i.prefix)).ToArray(),
                    Dyes = doll._dyes.Select(i => new NetItem(i.netID, i.stack, i.prefix)).ToArray(),
                    Position = new Point(x, y),
                });
            }
        }
    }

    public static void RestoreDisplayDoll(List<DDolls> Dolls, Point offset)
    {
        if (Dolls == null || !Dolls.Any()) return;

        foreach (var data in Dolls)
        {
            int newX = data.Position.X + offset.X;
            int newY = data.Position.Y + offset.Y;

            var tile = Main.tile[newX, newY];
            if (tile == null || !tile.active() ||
                tile.type != TileID.DisplayDoll) continue;

            int id = TEDisplayDoll.Find(newX, newY);
            if (id == -1)
            {
                id = TEDisplayDoll.Place(newX, newY);
                if (id == -1)
                    continue; // 创建失败
            }

            var doll = (TEDisplayDoll)TileEntity.ByID[id];
            doll._items = new Item[data.Items.Length];
            for (int i = 0; i < data.Items.Length; i++)
            {
                var netItem = data.Items[i];
                var item = new Item();
                item.netDefaults(netItem.NetId);
                item.stack = netItem.Stack;
                item.prefix = netItem.PrefixId;
                doll._items[i] = item;
            }

            doll._dyes = new Item[data.Dyes.Length];
            for (int i = 0; i < data.Dyes.Length; i++)
            {
                var netItem = data.Dyes[i];
                var item = new Item();
                item.netDefaults(netItem.NetId);
                item.stack = netItem.Stack;
                item.prefix = netItem.PrefixId;
                doll._dyes[i] = item;
            }
        }
    }
    #endregion

    #region 获取 与 还原衣帽架物品方法
    public static void GetHatRack(List<HatRacks> Racks, int x, int y)
    {
        var tile = Main.tile[x, y];
        if (tile.type == TileID.HatRack)
        {
            var id = TEHatRack.Find(x, y);
            if (id != -1)
            {
                var doll = (TEHatRack)TileEntity.ByID[id];
                Racks.Add(new HatRacks()
                {
                    Items = doll._items.Select(i => new NetItem(i.netID, i.stack, i.prefix)).ToArray(),
                    Dyes = doll._dyes.Select(i => new NetItem(i.netID, i.stack, i.prefix)).ToArray(),
                    Position = new Point(x, y),
                });
            }
        }
    }

    public static void RestoreHatRack(List<HatRacks> Racks, Point offset)
    {
        if (Racks == null || !Racks.Any()) return;

        foreach (var data in Racks)
        {
            int newX = data.Position.X + offset.X;
            int newY = data.Position.Y + offset.Y;

            var tile = Main.tile[newX, newY];
            if (tile == null || !tile.active() ||
                tile.type != TileID.HatRack) continue;

            int id = TEHatRack.Find(newX, newY);
            if (id == -1)
            {
                id = TEHatRack.Place(newX, newY);
                if (id == -1)
                    continue; // 创建失败
            }

            var Rack = (TEHatRack)TileEntity.ByID[id];
            Rack._items = new Item[data.Items.Length];
            for (int i = 0; i < data.Items.Length; i++)
            {
                var netItem = data.Items[i];
                var item = new Item();
                item.netDefaults(netItem.NetId);
                item.stack = netItem.Stack;
                item.prefix = netItem.PrefixId;
                Rack._items[i] = item;
            }

            Rack._dyes = new Item[data.Dyes.Length];
            for (int i = 0; i < data.Dyes.Length; i++)
            {
                var netItem = data.Dyes[i];
                var item = new Item();
                item.netDefaults(netItem.NetId);
                item.stack = netItem.Stack;
                item.prefix = netItem.PrefixId;
                Rack._dyes[i] = item;
            }
        }
    }
    #endregion

    #region 获取 与 还原逻辑感应器方法
    public static void GetLogicSensor(List<LogicSensors> sensors, int x, int y)
    {
        var tile = Main.tile[x, y];
        if (tile.type == TileID.LogicSensor)
        {
            var id = TEFoodPlatter.Find(x, y);
            if (id != -1)
            {
                var sensor = (TELogicSensor)TileEntity.ByID[id];
                sensors.Add(new LogicSensors()
                {
                    type = sensor.logicCheck,
                    Position = new Point(x, y),
                });
            }
        }
    }

    public static void RestoreLogicSensor(List<LogicSensors> Sensors, Point offset)
    {
        if (Sensors == null || !Sensors.Any()) return;

        foreach (var data in Sensors)
        {
            int newX = data.Position.X + offset.X;
            int newY = data.Position.Y + offset.Y;

            var tile = Main.tile[newX, newY];
            if (tile == null || !tile.active() ||
                tile.type != TileID.LogicSensor) continue;

            int id = TELogicSensor.Find(newX, newY);
            if (id == -1)
            {
                id = TELogicSensor.Place(newX, newY);
                if (id == -1)
                    continue; // 创建失败
            }

            var Sensor = (TELogicSensor)TileEntity.ByID[id];
            Sensor.logicCheck = data.type;
        }
    }
    #endregion

    #region 连锁替换方法
    public static void VeinMiner(TSPlayer plr, int x, int y, int KillType, int id)
    {
        var Tile = Main.tile[x, y];
        var vein = GetVein(new HashSet<Point>(), x, y, KillType);
        if (Tile == null || !Tile.active() || Tile.type != KillType || vein.Count == 0) return;

        //默认500格
        if (vein.Count > Config.ReplaceCount)
        {
            plr.SendInfoMessage($"【[c/F66E78:失败]】替换区域超过[c/64A1E0:{Config.ReplaceCount}]格");
            return;
        }

        // 缓存原始图格的物品名称
        var origItem = GetItemFromTile(x, y);
        string name = $"[i/s{vein.Count}:{origItem.netID}]";

        // 替换图格
        foreach (var point in vein)
        {
            WorldGen.KillTile(point.X, point.Y, false, false, true);
            WorldGen.PlaceTile(point.X, point.Y, id);
        }

        plr.SendInfoMessage($"【[c/79E365:成功]】将 {name} 替换 [i/s{vein.Count}:{GetItemFromTile(x, y).netID}]");
    }
    #endregion

    #region 连锁区域的8个方向（上下左右+斜4向）
    public static HashSet<Point> GetVein(HashSet<Point> list, int x, int y, int type)
    {
        if (list == null)
            list = new HashSet<Point>(Config.ReplaceCount);

        var stack = new Stack<(int X, int Y)>(Config.ReplaceCount);
        stack.Push((x, y));

        while (stack.Count > 0 && list.Count < Config.ReplaceCount)
        {
            var (curX, curY) = stack.Pop();

            if (!list.Contains(new Point(curX, curY)) &&
                Main.tile[curX, curY] is { } tile &&
                tile.active() && tile.type == type)
            {
                list.Add(new Point(curX, curY));
                var directions = new[] { (1, 0), (-1, 0), (0, 1), (0, -1), (1, 1), (1, -1), (-1, 1), (-1, -1) };
                foreach (var (dx, dy) in directions)
                {
                    var newX = curX + dx;
                    var newY = curY + dy;
                    if (newX >= 0 && newX < Main.maxTilesX && newY >= 0 && newY < Main.maxTilesY)
                    {
                        stack.Push((newX, newY));
                    }
                }
            }
        }
        return list;
    }
    #endregion

    #region 获取连锁破坏图格的物品属性
    public static Item GetItemFromTile(int x, int y)
    {
        WorldGen.KillTile_GetItemDrops(x, y, Main.tile[x, y], out int id, out int stack, out _, out _);
        Item item = new();
        item.SetDefaults(id);
        item.stack = stack;
        return item;
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

        ushort Tile2 = pondTheme.tile;
        TileInfo platform = pondTheme.platform;
        int num = posX - 6;
        int num2 = 13;
        int num3 = 32;

        for (int i = num; i < num + num2; i++)
        {
            for (int j = posY; j < posY + num3; j++)
            {
                var Tile = Main.tile[i, j];
                Tile.ClearEverything();
                if (i == num || i == num + num2 - 1 || j == posY + num3 - 1)
                {
                    Tile.type = Tile2;
                    Tile.active(true);
                    Tile.slope(0);
                    Tile.halfBrick(false);
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

    #region 刷怪场指令版 构建方法
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

    #region 修复粘贴后家具无法互动：箱子、物品框、武器架、标牌、墓碑、广播盒、逻辑感应器、人偶模特、盘子、晶塔、稻草人、衣帽架
    public static void FixAll(int startX, int endX, int startY, int endY)
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
                    WorldGen.SquareTileFrame(x, y, true);
                    if (newChest == -1) continue;
                }

                //物品框
                if (tile.type == TileID.ItemFrame && tile.frameX % 36 == 0 && tile.frameY == 0)
                {
                    var ItemFrame = TEItemFrame.Place(x, y);
                    WorldGen.SquareTileFrame(x, y, true);
                    if (ItemFrame == -1) continue;
                }

                //武器架
                if ((tile.type == TileID.WeaponsRack || tile.type == TileID.WeaponsRack2) && tile.frameX % 54 == 0 && tile.frameY == 0)
                {
                    var WeaponsRack = TEWeaponsRack.Place(x, y);
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
                if (tile.type == TileID.LogicSensor && TELogicSensor.Find(x, y) == -1)
                {
                    int LogicSensor = TELogicSensor.Place(x, y);
                    if (LogicSensor == -1) continue;

                    ((TELogicSensor)TileEntity.ByID[LogicSensor]).logicCheck = (TELogicSensor.LogicCheckType)(tile.frameY / 18 + 1);
                }

                //人体模型
                if (tile.type == TileID.DisplayDoll && tile.frameX % 36 == 0 && tile.frameY == 0)
                {
                    var DisplayDoll = TEDisplayDoll.Place(x, y);
                    if (DisplayDoll == -1) continue;
                }

                //盘子
                if (tile.type == TileID.FoodPlatter)
                {
                    var FoodPlatter = TEFoodPlatter.Place(x, y);
                    WorldGen.SquareTileFrame(x, y, true);
                    if (FoodPlatter == -1) continue;
                }

                // 晶塔
                if (tile.type == TileID.TeleportationPylon && tile.frameX % 54 == 0 && tile.frameY == 0)
                {
                    var TeleportationPylon = TETeleportationPylon.Place(x, y);
                    if (TeleportationPylon == -1) continue;
                }

                //训练假人（稻草人）
                if (tile.type == TileID.TargetDummy && tile.frameX % 36 == 0 && tile.frameY == 0)
                {
                    var TrainingDummy = TETrainingDummy.Place(x, y);
                    if (TrainingDummy == -1) continue;
                }

                //衣帽架
                if (tile.type == TileID.HatRack && tile.frameX == 0 && tile.frameY == 0)
                {
                    var HatRack = TEHatRack.Place(x, y);
                    WorldGen.SquareTileFrame(x, y, true);
                    if (HatRack == -1) continue;
                }
            }
        }
    }
    #endregion

    #region 销毁所有互动家具实体
    public static void KillAll(int startX, int endX, int startY, int endY)
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
                    Chest.DestroyChest(x, y);
                }

                //物品框
                if (tile.type == TileID.ItemFrame)
                {
                    TEItemFrame.Kill(x, y);
                }

                //武器架
                if (tile.type == TileID.WeaponsRack || tile.type == TileID.WeaponsRack2)
                {
                    TEWeaponsRack.Kill(x, y);
                }

                //标牌 墓碑  广播盒
                if ((tile.type == TileID.Signs ||
                    tile.type == TileID.Tombstones ||
                    tile.type == TileID.AnnouncementBox))
                {
                    Sign.KillSign(x, y);
                }

                //逻辑感应器
                if (tile.type == TileID.LogicSensor)
                {
                    TELogicSensor.Kill(x, y);
                }

                //人体模型
                if (tile.type == TileID.DisplayDoll)
                {
                    TEDisplayDoll.Kill(x, y);
                }

                //盘子
                if (tile.type == TileID.FoodPlatter)
                {
                    TEFoodPlatter.Kill(x, y);
                }

                //晶塔
                if (tile.type == TileID.TeleportationPylon)
                {
                    TETeleportationPylon.Kill(x, y);
                }

                //训练假人（稻草人）
                if (tile.type == TileID.TargetDummy)
                {
                    TETrainingDummy.Kill(x, y);
                }

                //衣帽架
                if (tile.type == TileID.HatRack)
                {
                    TEHatRack.Kill(x, y);
                }
            }
        }
    }
    #endregion

    #region 获取野生房屋类型
    public static HouseBuilder HouseTypeToBuilder(HouseType type, List<Rectangle> rooms)
    {
        return type switch
        {
            HouseType.Wood => new WoodHouseBuilder(rooms),
            HouseType.Desert => new DesertHouseBuilder(rooms),
            HouseType.Granite => new GraniteHouseBuilder(rooms),
            HouseType.Ice => new IceHouseBuilder(rooms),
            HouseType.Jungle => new JungleHouseBuilder(rooms),
            HouseType.Marble => new MarbleHouseBuilder(rooms),
            HouseType.Mushroom => new MushroomHouseBuilder(rooms),
            _ => HouseBuilder.Invalid,
        };
    }
    #endregion
}
