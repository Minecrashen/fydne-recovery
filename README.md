# FYDNE Recovery

Рабочее пространство по воскрешению проекта **FYDNE** (сервера SCP: Secret Laboratory).
Синхронизируется через GitHub — редактируй с любого устройства команды.

> **Текущая фаза:** 0 — разведка и доступы. Главный блокер — связаться с основателем
> (дамп MongoDB + бренд). См. `docs/02_RECOVERY_ROADMAP.md`.

## 📁 Структура

```
fydne recovery/
├── docs/                      Документация и планы
│   ├── 01_TECHNICAL_AUDIT.md     Полный технический аудит проекта
│   ├── 02_RECOVERY_ROADMAP.md    Дорожная карта по фазам (чек-листы)
│   ├── 03_DEPENDENCIES.md        Манифест DLL-зависимостей плагина (блокер сборки)
│   ├── 04_HARMONY_PATCHES_AUDIT.md  Все 51 Harmony-патч: хрупкость + как чинить
│   ├── 05_BACKEND_BRINGUP.md     Как поднять бэкенд (Node + MongoDB)
│   ├── 06_GAME_VERSION.md        Привязка к версии игры (v14.x, доказательства)
│   └── 07_FOUNDER_PITCH.md       Питч основателю + что просить
├── scripts/
│   ├── clone-sources.ps1         Клонировать 3 исходных репо в ./sources
│   └── gather-dependencies.ps1   Собрать игровые DLL из установки сервера
├── backend/
│   ├── docker-compose.yml        MongoDB для локальной разработки
│   └── config-templates/         Шаблоны config.js всех сервисов
├── patches/
│   └── transpiler-to-prefix.md   Паттерн фикса хрупких патчей
├── sources/                   (gitignored) клоны исходников FYDNE
└── dependencies/              (gitignored) игровые DLL для сборки
```

## 🚀 Быстрый старт для нового участника

```powershell
# 1. Склонировать это рабочее пространство (GitHub Desktop или git clone)
# 2. Подтянуть исходники FYDNE:
powershell -ExecutionPolicy Bypass -File scripts\clone-sources.ps1
# 3. Прочитать docs/01..07 по порядку
# 4. Взять задачу из docs/02_RECOVERY_ROADMAP.md
```

## 🔗 Исходные репозитории FYDNE (org: `uwu-loli`)

- Плагин (C#): https://github.com/uwu-loli/Loli-Merged
- Архив плагинов: https://github.com/uwu-loli/plugins-scpsl-2020-2024
- Бэкенд (Node.js): https://github.com/uwu-loli/fydne-prod-web-2020-2023

## 🔄 Синхронизация (правило команды)

> **Начал работу → `Pull`. Закончил → `Commit` + `Push`.**

`.gitignore` блокирует заливку секретов, DLL и дампов БД — токены и бинарники в репо не попадут.

## 🎯 Краткий вывод аудита

- **Бэкенд (Node)** — ~80% жив, поднимается за дни. Восстанавливать.
- **Плагин (C#)** — таргетит свежую v14.x, но не собирается из-за отсутствующих зависимостей;
  игровой слой завязан на single-maintainer Qurre + 51 Harmony-патч. Гибрид: каркас на LabAPI,
  контент-логику портировать.
- **Данные игроков** — в репозиториях НЕТ. Только у основателя. Это решает судьбу «возврата».
