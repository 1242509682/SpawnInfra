# SpawnInfra 生成基础建设（开服自动基建）

- 作者: 羽学
- 出处: [TShockWorldModify](https://github.com/hufang360/TShockWorldModify)
- 在新世界出生点右侧脚下建一个监狱集群
- 出生点上方生成世界平台与世界轨道
- 出生点脚下生成地狱直通车,
- 直通车洞穴层下方生成简易刷怪场（200 * 200) * 2边
- 在世界轨道能找到微光湖直通车(底下放了雨云块防摔)
- 在出生点左下角生成8*14个箱子
- 从地狱层往下深40格生成地狱平台与轨道
- 或使用指令在当前位置创建个监狱（/rm 数量）

## 更新日志
```
v1.5.4
给刷怪场加入提高刷怪率方法
需击败肉山后所有在线玩家在刷怪场中心才会生效

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
| /rm 数量 | /基建 |  room.use    |    在当前位置建小房子（玩家用）    |
| /rm reset | /基建 重置 |  room.admin    |    重置配置开关:开服自动建监狱   |


## 配置

```json
{
  "使用说明": "[微光直通车]的高度是从[世界平台]到湖中心,[自建微光湖]天顶从地狱往上算，其他以出生点往下算,开启则关闭[微光直通]",
  "使用说明2": "[世界平台]普通世界从出生点往上算，天顶从太空层往下算，[刷怪场比例缩放]为倍数放大，建议不要调太高",
  "使用说明3": "改[箱子数量]要同时改[出生点偏移X]，改[箱子层数]要同时改[出生点偏移Y]，[层数间隔]和[箱子宽度]不建议动",
  "使用说明4": "给玩家用的建晶塔房指令:/rm 数量 权限名:room.use (千万别在出生点用，有炸图风险)",
  "使用说明5": "每次重置服务器前使用一遍：/rm 重置，或者直接把这个指令写进重置插件里",
  "使用说明6": "以下参数仅适用于大地图，小地图需自己调试适配",
  "使用说明7": "[设置刷怪率]需所有在线玩家在刷怪场中心且击败[肉山]后生效",
  "开服自动基建": true,
  "自建微光湖": [
    {
      "生成微光湖": false,
      "出生点偏移X": 0,
      "出生点偏移Y": 200
    }
  ],
  "监狱集群": [
    {
      "是否建监狱": true,
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
      "是否建箱子": true,
      "出生点偏移X": -38,
      "出生点偏移Y": 27,
      "箱子数量": 18,
      "箱子层数": 8,
      "层数间隔": 2,
      "清理高度": 2,
      "箱子宽度": 2,
      "箱子ID": 21,
      "箱子样式": 1,
      "平台ID": 19,
      "平台样式": 43
    }
  ],
  "世界/左海平台": [
    {
      "是否建世界平台": true,
      "是否建世界轨道": true,
      "是否建左海平台": true,
      "世界平台图格": 19,
      "世界平台样式": 43,
      "世界平台高度": -200,
      "平台清理高度": 35,
      "世界平台禁入左海距离": 150,
      "左海平台禁入出生点距离": 50,
      "左海平台长度": 270,
      "左海平台高度": 200,
      "左海平台间隔": 30,
      "实体块包围左海平台(天顶必开)": true,
      "左右实体块是否对齐上下(加载很慢)": false,
      "左海平台产生液(非空岛不需要开)": false
    }
  ],
  "直通车/刷怪场": [
    {
      "是否建地狱直通车": true,
      "是否建地狱平台": true,
      "是否建地狱轨道": true,
      "是否建微光直通车": true,
      "是否建刷怪场": true,
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
      "设置刷怪率": true,
      "刷怪间隔/帧": 60,
      "刷怪上限/个": 100
    }
  ]
}
```
## 反馈
- 优先发issued -> 共同维护的插件库：https://github.com/Controllerdestiny/TShockPlugin
- 次优先：TShock官方群：816771079
- 大概率看不到但是也可以：国内社区trhub.cn ，bbstr.net , tr.monika.love