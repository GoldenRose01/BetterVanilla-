# Команды

Языки: [Italiano](Comandi.md) | [English](en-Commands.md) | [Español](es-Commands.md) | [简体中文](zh-Commands.md) | [हिन्दी](hi-Commands.md) | [العربية](ar-Commands.md) | [Português](pt-Commands.md) | [বাংলা](bn-Commands.md) | [Русский](ru-Commands.md) | [اردو](ur-Commands.md)

Команды `/gameset` требуют административного права `COMMANDS_GAMEMASTER`. Игрок, указанный для KeepInventory, должен быть онлайн.

## Индивидуальный KeepInventory

```text
/gameset player keepinventory on <player>
/gameset player keepinventory off <player>
```

Включает или выключает сохранение инвентаря и опыта указанного игрока после смерти. Настройка индивидуальна, не зависит от глобального правила `keepInventory` и сохраняется в данных игрока.

## Редактор торговли

```text
/gameset villagertrade
```

Открывает редактор ближайшего жителя примерно в пределах 16 блоков по горизонтали. Можно создавать, изменять и удалять предложения, задавая:

- Предмет и количество основной оплаты.
- Предмет и количество необязательной второй оплаты.
- Получаемый предмет и количество.
- Максимальное число использований.

Возьмите предмет в инвентаре и нажмите соответствующую ячейку редактора. Меню копирует тип, не расходуя предмет на курсоре. Если житель не найден, выводится ошибка.

Сохранённые изменения назначаются жителю и остаются в мире. Пока меню открыто, оставайтесь рядом с жителем.

---

## Содержание

- [Главная](ru-Home.md)
- [Установка](ru-Installation.md)
- [Лифты](ru-Elevators.md)
- [Рецепты](ru-Recipes.md)
- [Котлы](ru-Cauldrons.md)
- [Бонус маяка](ru-Beacon.md)
- [Команды](ru-Commands.md)
- [Версия 1.1.0](ru-Release-1.1.0.md)
- [Совместимость](ru-Compatibility.md)
- [Частые вопросы](ru-FAQ.md)

[Репозиторий](https://github.com/GoldenRose01/BetterVanilla-) | [Сообщить о проблеме](https://github.com/GoldenRose01/BetterVanilla-/issues)
