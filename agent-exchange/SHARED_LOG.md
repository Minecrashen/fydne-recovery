# FYDNE Recovery — Shared Agent Log

Координационный лог между агентами/устройствами. Читай перед работой, обновляй после.
Протокол — в `AGENT_PROTOCOL.md`. Новые записи — В НАЧАЛО секции LOG ENTRIES.

---

## 📋 CURRENT STATUS

| Agent | Status | Working On |
|-------|--------|------------|
| Claude Code (Opus) | 🟢 ACTIVE | Миграция Qurre→LabAPI (shim). Каркас готов, идёт event-структурный слой |
| (другие) | — | — |

## 🔴 ACTIVE BLOCKERS

- **Стратегический (не-техн.):** возврат старой аудитории невозможен без **дампа MongoDB** —
  он только у основателя. «Без баз» решено стартовать с нуля (см. DECISION 2026-06-07).
- **Железо:** доступен 1 ГБ — недостаточно для игрового инстанса (нужно 2–4 ГБ/сервер + быстрое ядро).
- **Публичайзер:** нет `Assembly-CSharp_public.dll` (нет dotnet SDK для tool). 5 ошибок CS0122
  (protection level) в патчах ждут публичайзинга. Обход — Mono.Cecil-скрипт или ставить SDK.

---

## 0. TL;DR текущего состояния

- **Цель:** воскресить FYDNE (SCP:SL). Бэкенд (Node) — восстанавливать; плагин (C#) — мигрировать
  с фреймворка **Qurre** (доступен только у основателя) на официальный **LabAPI**.
- **Подход к миграции:** не переписывать 196 файлов плагина, а собрать **shim-сборку с именем
  `Qurre.dll`** поверх LabAPI — плагин подхватит её без правок в себе.
- **Окружение развёрнуто** на машине разработчика: сервер SCP:SL (SteamCMD), **147 DLL**,
  **LabApi 1.1.7**, компилятор VS2022/csc. Полная карта API LabApi выгружена в
  `docs/labapi_1.1.7_api_reference.txt`.
- **Shim компилируется** (`plugin/QurreShim` → `Qurre.dll`, 23 КБ): Log, Server, Round,
  Player + 8 под-объектов, мин. контроллеры, диспетчер событий (загрузчик/реестр/Inject),
  точка входа LabAPI `QurreBootstrap`.
- **Перепись плагина:** `build-plugin.ps1 -Census` → **887 ошибок** (784× CS0246 = отсутствующие
  event-структуры и типы, 89× CS0234 = под-namespace'ы, 5× CS0122 = публичайзинг). Это и есть
  карта оставшейся механической работы.

---

## 1. Что уже в репозитории

```
docs/01..09 + labapi_1.1.7_api_reference.txt   — аудит, дорожная карта, зависимости, патчи,
                                                  бэкенд, версия, миграция, статус окружения
scripts/   clone-sources, gather-dependencies, build-shim, build-plugin
backend/   docker-compose (Mongo) + config-templates
patches/   transpiler-to-prefix паттерн
plugin/Loli       — исходник плагина (196 .cs, MIT)
plugin/QurreShim  — shim Qurre→LabAPI (компилируется)
```

## 2. Окружение на машине разработчика (НЕ в git)

| Что | Путь |
|---|---|
| Сервер SCP:SL | `C:\Users\Admin\fydne_build\scpsl-server` |
| Игровые DLL | `…\SCPSL_Data\Managed` (144 шт) |
| SteamCMD | `C:\Users\Admin\fydne_build\steamcmd` |
| Инспектор API (MetadataLoadContext) | `C:\Users\Admin\fydne_build\tools\Inspect.exe` |
| dependencies/ | в репо-папке, gitignored, 147 DLL |

## 3. Что осталось (кратко — детально в TODO.md)

1. Event-структуры (~70) + обвязка LabAPI в `EventMap.cs` (784 CS0246).
2. `Map` + полные контроллеры `Room/Door/Lift/Tesla/Cassie/Generator`.
3. `Player.Effects` + `RoleInformation.Scp*` (SCP-роль API).
4. Публичайзер `Assembly-CSharp` → закрыть CS0122 в патчах.
5. `SCPLogs.dll` — заглушить или получить у основателя.
6. Довести `build-plugin.ps1` до 0 ошибок → собрать плагин.
7. Бэкенд: поднять Mongo + сервисы (новые секреты).

---

## 📝 LOG ENTRIES

### 2026-06-07 ✅ DONE — Agent: Claude Code (Opus), session 2026-06-07

**Status**: IN_PROGRESS (миграция продолжается)
**Files Changed**: весь репозиторий `fydne-recovery` (init → текущее состояние)
**Related To**: аудит + развёртывание стенда + старт миграции Qurre→LabAPI

Сделано за сессию:
- **Аудит** трёх репозиториев FYDNE (Loli-Merged, plugins-scpsl-2020-2024, fydne-prod-web).
  Документы `docs/01..09`. Версия игры зафиксирована — **v14.x** (маркер `Scp956Pinata`).
- **Развернул стенд** автономно: SteamCMD → сервер SCP:SL → 147 DLL + LabApi 1.1.7.
  Собрал инспектор на MetadataLoadContext, выгрузил **полную карту API LabApi**.
- **Старт миграции** через shim (`AssemblyName=Qurre`): Log/Server/Round/Player(+8 под-объектов)/
  контроллеры(мин)/диспетчер(loader+registry+Inject)/Bootstrap — **всё компилируется** против
  реального LabApi.
- **Перепись плагина**: 887 ошибок (карта оставшегося, см. TODO.md).

⚠️ DECISION: мигрируем на LabAPI (не ждём Qurre у основателя). Старт «без баз данных».
📌 INFO: `Qurre.dll`-shim — drop-in замена, плагин править почти не нужно.
🐛 BUG (в самом плагине, на будущее): `VoiceTransceiver.ServerReceiveMessage` патчится дважды
   (`ScpSpeaks.cs` + `FixSpoiled.cs`) — проверить конфликт при сборке.

Следующий шаг: наполнять `EventMap.cs` + создавать event-структуры пачками, гоняя
`build-plugin.ps1 -Census` до нуля.

---
