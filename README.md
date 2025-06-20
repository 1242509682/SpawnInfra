# SpawnInfra 生成基础建设（开服自动基建）

- 作者: 羽学
- 出处: [SpawnInfra](https://github.com/1242509682/SpawnInfra)
- 在新世界出生点右侧脚下建一个监狱集群
- 出生点上方生成世界平台与世界轨道
- 出生点脚下生成地狱直通车,
- 直通车洞穴层下方生成简易刷怪场（200 * 200) * 2边
- 在世界轨道能找到微光湖直通车(底下放了雨云块防摔)
- 在出生点左下角生成8*14个箱子
- 从地狱层往下深40格生成地狱平台与轨道
- 允许使用指令生成基础建筑

## 更新日志
```
v1.7.2
将监狱集群的生成指令改为:/spi jy
加入野生洞穴小屋生成指令:/spi hs
当人物处于地表时，箱子物品为地表物品反之则为洞穴物品（沙漠箱洞穴物品独立）
房子类型：木屋(1)、沙漠(2)、雪原(3)、丛林(4)、花岗岩(5)、大理石(6)、蘑菇(7)
洞穴小屋需要脚下有实体块才会生成
该指令需要权限：spawninfra.admin

v1.7.1
修复复制一次建筑，导致该晶塔即使挖掉也无法放置的BUG
还原建筑时不再残留实体在图格上，移除修复晶塔配置项
修复了复制和还原可恢复【物品框、武器架、帽架、盘子、人偶】等家具的物品
这些都可以通过/spi pt -f参数或《复制建筑修复家具物品》配置项进行工作

v1.7.0
修复标牌、广播盒、墓碑等可阅读家具无法粘贴信息的BUG
/spi bk还原指令支持还原箱子物品与标牌等家具信息
使用了GZipStream极大降低了对建筑文件的空间占用（意味着之前的.dat文件都会作废）
建议配合少司命的CreateSpawn《复制建筑》插件进行互相复制来创建建筑文件
注意：《复制建筑》插件的.map后缀改为.dat即可在新版《生成基建》使用
两者采用一样的读写代码，《复制建筑》为独立版，《生成基建》为集成版

v1.6.9
优化了连锁放置与还原图格速度
加入了异步线程保护，避免同时多个任务一起执行
使用/spi reset不再备份bk.dat还原文件，只在有cp.dat时才会生成压缩包
修复世界平台与战斗平台不会获取手上物品类型放置的问题
修复关闭世界平台/直通车/仓库/监狱配置项时，指令无法生成对应建筑问题
修复神庙、地牢、金字塔会使人物卡在里面的问题（默认生成在脚下）

v1.6.8
移除了对/spi list需要进入服务器才能使用的条件检查（方便控制台查看）
给放置方块加入了：“连锁放置”模式（无需画选区）
/spi t 4 ——连锁替换方块
手持物品输入指令,然后破坏图格即刻完成替换（每次替换都需要输1次）
加入了【连锁替换图格上限】配置项

v1.6.7
加入了【复制建筑修复晶塔】配置项
不建议开,有BUG：放置的晶塔会在地图一直显示。
当关闭【复制建筑修复箱子物品】配置项时:
管理可使用【/spi pt -f】 或 【/spi pt 名字 -f】来强制修复箱子
非管理会提示缺少权限，开启配置项则无需-f参数和管理权来强制修复。

v1.6.6
加入了新开关配置：【复制建筑修复箱子物品】、【重置是否备份建筑】、【重置是否清理建筑】、【重置是否自动基建】
复制建筑修复箱子物品：仅对复制和粘贴的箱子有效，使用/spi back还原的则无效（还未处理这部分逻辑）

v1.6.5
修复粘贴的建筑家具无法互动的问题，包含：
箱子、物品框、武器架、标牌、墓碑、广播盒、逻辑感应器、人偶模特、盘子、晶塔、稻草人、衣帽架

v1.6.4
减少使用“复制粘贴撤销”时对内存的消耗
将其复制和还原数据存储到Tshock文件夹下的SPIData文件夹中：
**.bk.dat是玩家自己粘贴或修改选区后的还原图格数据
**.cp.dat是剪贴板的图格数据
以此解决重启服务器时丢失“复制数据”的问题，支持跨地图复制粘贴
现在“复制”支持使用指定名字来保存建筑：/spi cp 名字
如果没有指定名字则使用玩家“自己名下”的剪贴板来复制建筑（粘贴也是如此）
加入了/spi list指令，支持列出可复制的建筑
/spi reset指令不仅会出生点自动基建,还会备份所有建筑数据后再清除

v1.6.3
每次对选区进行操作都会记录一次图格缓存，使用指令可还原到上次操作前的状态
还原指令:/spi bk
复制指令:/spi cp
粘贴指令:/spi pt
粘贴也支持还原（粘贴是以玩家头顶）
以上指令都需要使用/spi s 设置好选区才有用
暂时不支持非选区指令的范围还原

v1.6.2
/spi p 区域喷漆和涂料无法识别正确ID导致染色错误的BUG

v1.6.1
修复/spi t与/spi w的介绍写反问题（1保留、2不保留）
修复/spi sg ——生成刷怪场指令,还是有方块会覆盖到地牢砖与丛林蜥蜴砖的问题
修复/spi w ——区域墙壁指令，参数2不应该清理方块，而是清理墙壁再放置。
修复/spi t ——区域方块指令，参数3不应该清理墙壁，而是清理方块再放置。
给/spi t ——区域方块指令，加入了‘替换’参数，‘替换’会保留半砖原有特性：
1保留放置,2替换,3清完放置
给/spi c ——区域清理指令加入了更多可选项目,并修复参数1清方块，只清理边界点的BUG：
1方块,2墙,3液体,4喷漆,5不虚化,6电线,7制动器,8清除所有
给/spi p ——区域喷漆指令加入了切换方块虚化的功能：1喷方块,2喷墙,3所有,4切换虚化
给/spi bz ——区域半砖指令加入了恢复全砖的功能：1右斜坡,2左斜坡,3右半砖,4左半砖,5恢复全砖
修改/spi wr ——区域电路指令，参数4不会再虚化方块，而是放完电线再放制动器：
1只放电线,2清后再放,3清后再放并虚化,4清后再放加制动器
将/spi yc ——指令中的参数从0到3改为：1水,2岩浆,3蜂蜜,4微光

v1.6.0
加入了【放置时禁止覆盖方块表】配置项，该表作用于使用指令生成：
刷怪场/世界平台/战斗平台/直通车时遇到表中图格ID就会停止放置（默认为地牢3砖和丛林蜥蜴砖块图格ID）
加入了/spi p指令 用于给选区涂抹喷漆用，并对选区相关修改操作指令进一步优化：
修复/spi c指令在放置时会生成自然土墙bug
使用/spi s空值输入可提示以下教学：
请确保s1和s2两点不再同一位置,两点之间可形成'线'或者'方形'
使用方法: /spi s 1 放置或挖掘左上角方块
使用方法: /spi s 2 放置或挖掘右下角方块
清理区域: /spi c 1到4 (1方块/2墙/3液体/4所有)
根据选区放手上方块: /spi t 1和2 (1保留放置/2清完放置)
根据选区放手上墙壁: /spi w 1和2 (1保留放置/2清完放置)
根据选区放手上喷漆: /spi p 1和2 (1喷方块/2喷墙/3所有)
清选区并放液体:/spi yt 1到4 (1水,2岩浆,3蜂蜜.4微光)
将选区方块设为半砖:/spi bz 1到4（1右斜坡,2左斜坡,3右半砖,4左半砖）[12适合做电梯/34做推怪平台]
为选区方块添加电路:/spi wr 1到4（1只放电线,2清后再放,3清后再放并虚化,4清后再放并虚化加制动器）

v1.5.9
指令加入了生成宝物箱、陷阱、罐子、生命水晶、附魔剑冢、金字塔、恶魔祭坛、腐化地功能
添加了/spi s [1/2]指令，可用来构建选区提供给以下指令使用：
- /spi c 清理选区所有图格
- /spi t 清选区并放置手上方块
- /spi w 清选区并放置手上墙壁
- /spi yt 清选区并放置液体(水/蜂蜜/岩浆/微光)
- /spi bz [1/2/3/4] 将选区内的方块设为半砖
spawninfra.use权限只能使用：
选区、清理、放置方块、放置液体、放置墙壁、设置半砖
房屋、鱼池、刷怪场、世界平台、直通车、战斗平台、恶魔祭坛指令
spawninfra.admin权限独享生成指令：
监狱、仓库、地牢、神庙、陷阱、罐子、微光湖、宝物箱、腐化地、金字塔、生命水晶、附魔剑冢

v1.5.8
完善指令系统，允许玩家在游戏中使用指令生成指定建筑
指令权限改为：spawninfra.use与spawninfra.admin
指令名改为：/spi
支持生成：
小房子、监狱集群、箱子集群、地牢、神庙、微光湖
刷怪场、鱼池、直通车、世界轨道平台、战斗平台

v1.5.7
当前版本已适配TShock 5.2.4
给箱子集群加入了【启用墙壁与物品框】配置项
当它开启时每层箱子会放墙壁与物品框并使间隔额外加3格(适配西江的AutoClassificationQuickStack插件)
将rm指令修改为/rms(避免与少司命的RolesModifying插件指令冲突）

v1.5.6
移除左右实体块是否对齐上下配置项
给世界平台加了判断阻拦，如果进入太空层则不生成
重新排列了一下世界平台的生成顺序
（放最后，避免建了小地图导致平台放置，导致索引越界妨碍其他建筑的放置）
声明此插件不适用于小地图，如果是中地图请自己修改相关配置参数进行调试

v1.5.5
完善卸载函数
配置项加入开服指令表，用于配合【奖励箱】插件，
它会先执行指令再放置建筑物
（仅在开启【开服自动基建】时会执行1次）

v1.5.4
给刷怪场加入提高刷怪率方法
需击败肉山后所有在线玩家在刷怪场中心才会生效
把刷怪层改成传送带，尖球机关改成虚化（取消制动器）

v1.5.3
左海平台不会再建到太空层
优化天顶颠倒种子的世界平台与十周年种子的微光直通车
加入了实体块包围左海平台配置项（天顶颠倒必开）
左侧实体块在x轴0格位置(少于41格只能从地图编辑器看),与右侧实体块等高
左右两侧对齐建议别开，加载速度很慢
修复了刷怪场的双向无限飞镖，
只要开启配置项无论直通车是否覆盖刷怪场都会有边界飞镖
给核心区加入了半砖平台让飞镖能正常穿墙进岩浆区

v1.5.2
加入了自建微光湖（开启则关闭微光湖直通车）
优化了左海/世界平台对十周年种子距离控制
优化了左海清理方块逻辑（保留液体），加入对天顶的人工造海逻辑
给刷怪场左右边界封闭防液体流入，
开启直通车贯穿时会在底部放一层岩浆禁止刷怪
关闭直通车贯穿则在刷怪场右边放一列飞镖机关
刷怪场可自定义是否放岩浆/飞镖/尖球
开启`只清刷怪区域`会关闭`自动建刷怪场`

v1.5.1
修复十周年种子没有按天顶微光直通车逻辑运行问题
修复颠倒与天顶世界直通车不能到达世界平台的问题
给刷怪场加入了一些基础物品放置，匹配了缩放比例
给刷怪场加入了无限飞镖机关/完善了“定刷怪区”
注意：此插件预设的配置参数仅适用于大地图，小地图需要自己调整

v1.5.0
支持天顶/颠倒/十周年世界种子

v1.4.0
修复了左海平台离地太近导致穿插沙滩的问题
世界平台与左海平台不再以出生点为起点定高度
改为从太空层往下数，主要适配小世界
左海平台默认会清理每层间隔内的所有方块
加入了世界平台禁入左海的配置项
加入了直通车贯穿刷怪场的开关
给地狱直通车通道加了发光墙
修正了刷怪场中心点为出生点X轴
修正了刷怪场比例缩放
给刷怪场放个花园侏儒
提高优先级避免覆盖CreateSpawn插件

v1.3.0
加入了微光湖直通车
地狱直通车洞穴层下方加入了刷怪场（无特殊环境,简易电路）
将出生点箱子的垫脚石更换为了灰砖平台

v1.2.0
加入自动基建后自动清理一遍全地图掉落物（替换树木引起的图格错误）
将具备开关的配置项放入了它们自己的数据组里
加入了生成箱子的设定

v1.1.0
插件更名为：生成基础建设
加入地表/左海/地狱平台（轨道）、地狱直通车

v1.0.0
将世界修改器插件中的监狱小房子方法独立成一个功能
支持配置项自定义监狱细节样式
建完监狱后会自动关掉`开服自动基建`配置项，
需要重置请输入/rm reset 然后重启服务器即可。

```

## 指令
| 语法                             | 别名  |       权限       |                   说明                   |
| -------------------------------- | :---: | :--------------: | :--------------------------------------: |
| /spi | /基建 |  spawninfra.use    |    指令菜单    |
| /spi s [1/2] | /基建 选择 |  spawninfra.use    |    修改指定选择区域(画矩形)    |
| /spi L | /基建 列表 |  spawninfra.use    |    列出可复制的建筑名    |
| /spi c | /基建 清理 |  spawninfra.use    |    清理选区所有图格    |
| /spi t | /基建 方块 |  spawninfra.use    |    放置手上方块    |
| /spi w | /基建 墙壁 |  spawninfra.use    |    放置手上墙壁    |
| /spi p | /基建 喷漆 |  spawninfra.use    |    放置手上喷漆    |
| /spi yt | /基建 液体 |  spawninfra.use    |    清选区并放置液体(水/蜂蜜/岩浆/微光)    |
| /spi bz | /基建 半砖 |  spawninfra.use    |    将选区内的方块设为半砖    |
| /spi wr | /基建 电路 |  spawninfra.use    |    为选区内的方块添加电路    |
| /spi cp | /基建 复制 |  spawninfra.use    |    复制选区为自己名下建筑    |
| /spi cp 名字 | /基建 复制 |  spawninfra.use    |    复制选区为保存指定建筑    |
| /spi pt | /基建 粘贴 |  spawninfra.use    |    在头顶粘贴自己名下建筑    |
| /spi pt 名字 | /基建 粘贴 |  spawninfra.use    |    在头顶粘贴指定建筑    |
| /spi pt 名字 -f | /基建 粘贴 |  spawninfra.admin    |    粘贴还原箱子物品的指定建筑    |
| /spi r 数量 | /基建 房子 |  spawninfra.use    |    在当前位置建小房子    |
| /spi jy | /基建 监狱 |  spawninfra.admin    |    脚下生成监狱    |
| /spi ck | /基建 仓库 |  spawninfra.admin    |    脚下生成仓库    |
| /spi dl | /基建 地牢 |  spawninfra.admin    |    生成地牢(天顶需去地表使用)    |
| /spi sm | /基建 神庙 |  spawninfra.admin    |    生成神庙    |
| /spi wg | /基建 微光 |  spawninfra.admin    |    生成微光湖    |
| /spi bx | /基建 宝箱 |  spawninfra.admin    |    生成宝物箱    |
| /spi xj | /基建 陷阱 |  spawninfra.admin    |    生成陷阱    |
| /spi gz | /基建 罐子 |  spawninfra.admin    |    生成罐子    |
| /spi sj | /基建 水晶 |  spawninfra.admin    |    生成生命水晶    |
| /spi jt | /基建 祭坛 |  spawninfra.use    |    生成恶魔祭坛    |
| /spi jz | /基建 剑冢 |  spawninfra.admin    |    生成附魔剑冢    |
| /spi jzt | /基建 金字塔 |  spawninfra.admin    |    生成金字塔    |
| /spi fh | /基建 腐化地 |  spawninfra.admin    |    生成腐化地    |
| /spi hs | /基建 洞穴屋 |  spawninfra.admin    |    生成洞穴小屋    |
| /spi sg 高度 宽度 | /基建 刷怪场 |  spawninfra.use    |    生成刷怪场    |
| /spi yc 水 | /基建 鱼池 |  spawninfra.use    |    生成鱼池(水/蜂蜜/岩浆/微光)    |
| /spi zt | /基建 直通车 |  spawninfra.use    |    生成直通车(天顶为头上)    |
| /spi wp 清理高度 | /基建 世界平台 |  spawninfra.use    |    以手上方块生成世界平台    |
| /spi fp 宽 高 间隔 | /基建 战斗平台 |  spawninfra.use    |    以手上方块生成战斗平台    |
| /spi rs | /基建 重置 |  spawninfra.admin    |    出生点自动基建/清除所有建筑数据   |


## 配置

```json
{
  "使用说明": "[微光直通车]的高度是从[世界平台]到湖中心,[自建微光湖]天顶从地狱往上算，其他以出生点往下算,开启则关闭[微光直通]",
  "使用说明2": "[世界平台]普通世界从出生点往上算，天顶从太空层往下算，[刷怪场比例缩放]为倍数放大，建议不要调太高",
  "使用说明3": "改[箱子数量]要同时改[出生点偏移X]，改[箱子层数]要同时改[出生点偏移Y]，[层数间隔]和[箱子宽度]不建议动,[启用墙壁与物品框]会使间隔额外加3格",
  "使用说明4": "给玩家用的建晶塔房指令:/rms 数量 权限名:room.use (千万别在出生点用，有炸图风险)",
  "使用说明5": "每次重置服务器前使用一遍：/rms 重置，或者直接把这个指令写进重置插件里",
  "使用说明6": "以下参数仅适用于大地图，中地图需自己调试适配",
  "使用说明7": "[设置刷怪率]需所有在线玩家在刷怪场中心且击败[肉山]后生效",
  "插件开关": true,
  "重置是否自动基建": false,
  "重置是否清理建筑": true,
  "重置是否备份建筑": true,
  "复制建筑修复家具物品": true,
  "连锁替换图格上限": 500,
  "开服指令表": [],
  "放置时禁止覆盖方块表": [],
  "自建微光湖": [
    {
      "生成微光湖": false,
      "出生点偏移X": 0,
      "出生点偏移Y": 200
    }
  ],
  "监狱集群": [
    {
      "是否建监狱": false,
      "出生点偏移X": -4,
      "出生点偏移Y": 10,
      "监狱集群宽度": 36,
      "监狱集群高度": 80,
      "图格ID": 38,
      "墙壁ID": 147,
      "平台样式": 43,
      "椅子样式": 27,
      "工作台样式": 18,
      "火把样式": 13
    }
  ],
  "箱子集群": [
    {
      "是否建箱子": false,
      "启用墙壁与物品框": false,
      "出生点偏移X": -38,
      "出生点偏移Y": 34,
      "箱子数量": 18,
      "箱子层数": 8,
      "层数间隔": 0,
      "清理高度": 2,
      "箱子宽度": 2,
      "箱子ID": 21,
      "箱子样式": -1,
      "平台ID": 19,
      "平台样式": 43,
      "墙壁ID": 318,
      "物品框ID": 395
    }
  ],
  "世界/左海平台": [
    {
      "是否建世界平台": false,
      "是否建世界轨道": false,
      "是否建左海平台": false,
      "世界平台图格": 19,
      "世界平台样式": 43,
      "世界平台高度": -100,
      "平台清理高度": 35,
      "世界平台禁入左海距离": 150,
      "左海平台禁入出生点距离": 50,
      "左海平台长度": 270,
      "左海平台高度": 200,
      "左海平台间隔": 30,
      "实体块包围左海平台(天顶必开)": false,
      "左海平台产生液体(非空岛不需要开)": false,
      "世界平台不覆盖地牢砖与丛林蜥蜴砖": false
    }
  ],
  "直通车/刷怪场": [
    {
      "是否建地狱直通车": false,
      "是否建地狱平台": false,
      "是否建地狱轨道": false,
      "是否建微光直通车": false,
      "是否建刷怪场": false,
      "只清刷怪区域": false,
      "直通车贯穿刷怪场": true,
      "方块图格": 38,
      "绳子图格": 214,
      "平台图格": 19,
      "平台样式": 43,
      "直通车偏移X": 0,
      "直通车偏移Y": 0,
      "地狱直通车宽度": 5,
      "地狱平台深度": 25,
      "微光直通车宽度": 2,
      "刷怪场清理深度": 200,
      "刷怪场清理宽度": 200,
      "刷怪场比例缩放": 2,
      "是否放岩浆": true,
      "是否放尖球": true,
      "是否放飞镖": true,
      "设置刷怪率": false,
      "刷怪间隔/帧": 60,
      "刷怪上限/个": 100,
      "直通车不覆盖地牢砖与丛林蜥蜴砖": false
    }
  ]
}
```
## 反馈
- 优先发issued -> 共同维护的插件库：https://github.com/UnrealMultiple/TShockPlugin
- 次优先：TShock官方群：816771079
- 大概率看不到但是也可以：国内社区trhub.cn ，bbstr.net , tr.monika.love
