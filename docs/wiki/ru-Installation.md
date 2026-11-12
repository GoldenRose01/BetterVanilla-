# Установка

Языки: [Italiano](Installazione.md) | [English](en-Installation.md) | [Español](es-Installation.md) | [简体中文](zh-Installation.md) | [हिन्दी](hi-Installation.md) | [العربية](ar-Installation.md) | [Português](pt-Installation.md) | [বাংলা](bn-Installation.md) | [Русский](ru-Installation.md) | [اردو](ur-Installation.md)

## Требования

- Minecraft Java **26.3** или **26.4 Snapshot 7**, в зависимости от JAR.
- Fabric Loader, совместимый с игрой.
- Fabric API для той же версии Minecraft.
- Java 25 или новее.

## Установка

1. Установите Fabric Loader для выбранной версии.
2. Добавьте совместимый Fabric API в `mods`.
3. Скопируйте **только один** JAR BetterVanilla+ 1.1.0 в `mods`.
4. Запустите Minecraft с соответствующим профилем Fabric.

Выберите одну загрузку ниже. Не устанавливайте обе сборки в один профиль.

## Сервер и клиенты

Для лифтов и пользовательских предметов мод нужен на сервере и всех клиентах. В одиночной игре встроенный сервер использует установку локального профиля. Fabric API должен присутствовать в тех же средах.

Команды и изменения жителей выполняются на сервере. Частицы активности лифтов — клиентский эффект, видимый при установленном моде.

## Сборка

С JDK 25 выполните `gradlew.bat build` в Windows. JAR разработки появится в `build/libs/`; распространяемые сборки находятся в `jar/`.

- [Minecraft 26.3 — JAR](../../jar/BetterVanilla+-1.1.0-26.3.jar)
- [Minecraft 26.4 Snapshot 7 — JAR](../../jar/BetterVanilla+-1.1.0-26.4-snapshot-7.jar)

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
