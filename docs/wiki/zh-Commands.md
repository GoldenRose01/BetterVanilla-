# 命令

语言: [Italiano](Comandi.md) | [English](en-Commands.md) | [Español](es-Commands.md) | [简体中文](zh-Commands.md) | [हिन्दी](hi-Commands.md) | [العربية](ar-Commands.md) | [Português](pt-Commands.md) | [বাংলা](bn-Commands.md) | [Русский](ru-Commands.md) | [اردو](ur-Commands.md)

`/gameset` 命令需要 Minecraft 的 `COMMANDS_GAMEMASTER` 管理权限。KeepInventory 的目标玩家必须在线。

## 个人 KeepInventory

```text
/gameset player keepinventory on <player>
/gameset player keepinventory off <player>
```

开启或关闭指定玩家死亡后保留物品栏及经验的功能。此设置独立于全局 `keepInventory` 游戏规则，并随玩家数据保存。

## 村民交易编辑器

```text
/gameset villagertrade
```

打开水平方向约 16 格内最近村民的编辑器。可以创建、修改、删除交易，并设置：

- 主要费用的物品和数量。
- 可选的第二项费用的物品和数量。
- 获得的物品和数量。
- 最大交易次数。

在物品栏中拿起物品，再点击编辑器中的对应槽位。菜单只复制物品类型，不消耗光标上的物品。找不到村民时会显示错误。

保存的修改会应用于村民并持久保存在世界中。菜单打开期间，玩家必须保持在村民附近。

---

## 目录

- [首页](zh-Home.md)
- [安装](zh-Installation.md)
- [电梯](zh-Elevators.md)
- [合成配方](zh-Recipes.md)
- [炼药锅](zh-Cauldrons.md)
- [信标加成](zh-Beacon.md)
- [命令](zh-Commands.md)
- [1.1.0 版本](zh-Release-1.1.0.md)
- [兼容性](zh-Compatibility.md)
- [常见问题](zh-FAQ.md)

[代码仓库](https://github.com/GoldenRose01/BetterVanilla-) | [报告问题](https://github.com/GoldenRose01/BetterVanilla-/issues)
