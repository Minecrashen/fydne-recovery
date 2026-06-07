# FYDNE Recovery

Рабочее пространство по восстановлению и портированию open-source плагина **FYDNE**
для **SCP: Secret Laboratory** на актуальную версию игры.

Исходники FYDNE были опубликованы автором под открытыми лицензиями. Здесь — технический
аудит этого кода, план восстановления и слой совместимости для сборки под текущую игру.

> **Статус:** активная разработка. Развёрнут сборочный стенд, идёт миграция плагина
> с фреймворка Qurre на официальный **LabAPI** через shim-сборку. См. `TODO.md`.

## 📁 Структура

```
docs/        технический аудит, дорожная карта, манифест зависимостей,
             аудит Harmony-патчей, гайд по бэкенду, привязка к версии игры,
             план миграции на LabAPI, статус окружения
scripts/     build-харнессы (PowerShell): сборка shim'а и плагина, сбор зависимостей
backend/     docker-compose (MongoDB) + шаблоны конфигов сервисов
patches/     паттерны починки хрупких Harmony-патчей
plugin/Loli       исходник плагина (как опубликован автором)
plugin/QurreShim  слой совместимости Qurre → LabAPI (компилируется)
```

## 🚀 Быстрый старт

```powershell
# 1. Склонировать это рабочее пространство
# 2. Подтянуть исходники FYDNE в ./sources:
powershell -ExecutionPolicy Bypass -File scripts\clone-sources.ps1
# 3. Поставить выделенный сервер SCP:SL и собрать зависимости:
powershell -ExecutionPolicy Bypass -File scripts\gather-dependencies.ps1 -ServerPath <путь>
# 4. Собрать shim:
powershell -ExecutionPolicy Bypass -File scripts\build-shim.ps1
```

Дальше — по `docs/02_RECOVERY_ROADMAP.md` и `TODO.md`.

## 🔗 Исходные репозитории FYDNE

- Плагин (C#): https://github.com/uwu-loli/Loli-Merged
- Архив плагинов: https://github.com/uwu-loli/plugins-scpsl-2020-2024
- Бэкенд (Node.js): https://github.com/uwu-loli/fydne-prod-web-2020-2023

## 🧩 Подход к миграции

Вместо переписывания плагина целиком — тонкий **shim** с именем сборки `Qurre.dll`,
реализующий используемую поверхность Qurre поверх LabAPI. Плагин подхватывает его
как drop-in замену почти без правок. Детали — `docs/08_LABAPI_MIGRATION.md`.

## 🔄 Синхронизация (для команды)

Репозиторий синхронизируется через GitHub. Перед работой — `pull`, после — `commit` + `push`.
`.gitignore` исключает бинарники, игровые DLL и любые секреты — в репозиторий они не попадают.

## ⚖️ Лицензии

Исходный код FYDNE распространяется под лицензиями его автора (MIT / GPLv3 в разных репо).
Игровые сборки Northwood (`Assembly-CSharp` и пр.) **не входят** в репозиторий — каждый
берёт их из собственной легальной установки сервера. Слой `plugin/QurreShim` — оригинальный.
