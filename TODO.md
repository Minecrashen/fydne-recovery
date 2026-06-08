# FYDNE Recovery — TODO

Живой список задач. Отмечай `[x]`, коммить, пушь. Подробности — в `docs/`.
Координация между агентами — `agent-exchange/SHARED_LOG.md`.

Статус на 2026-06-07: стенд развёрнут, shim компилируется, плагин — **887 ошибок** (перепись).

---

## 🔥 P0 — Миграция плагина Qurre→LabAPI (текущий фокус)

Цель: довести `scripts\build-plugin.ps1 -Census` до **0 ошибок**.
**Прогресс: 887 → 117 ошибок (−87%).** Остаток сконцентрирован в ОДНОЙ подсистеме (см. ниже).

### 🔴 ГЛАВНЫЙ ОСТАВШИЙСЯ БЛОКЕР — схематик/постройки (Builds/)
~100 из 117 ошибок — это подсистема кастомных построек: `Qurre.API.Addons.Models`
(`Model` 416×, `ModelPrimitive` 275×, `Primitive` 125×, `PrimitiveParams`, `SObject`, `Scheme`)
+ внешняя `SchematicUnity.API` (`SchematicManager`). Варианты:
- [ ] (A) Получить у основателя оригинальную **`SchematicUnity.dll`** (с namespace `.API`) +
      реализовать `Qurre.API.Addons.Models` — faithful.
- [ ] (B) **Временно исключить `Builds/`** из сборки → получить загружаемое ЯДРО плагина
      быстрее (постройки — фича, не core-геймплей), вернуть схематик позже.
- [ ] Аудио-аддон `Qurre.API.Addons.Audio` (`AudioPlayerBot`) — отдельный модуль.

### Event-структуры + обвязка (784× CS0246 — главный массив)
- [ ] Создать ~70 event-структур в `plugin/QurreShim/src/Structs/` (поля — по использованию в плагине)
- [ ] Наполнить `EventMap.PopulateEnumMap()` (enum → тип структуры)
- [ ] Наполнить `EventMap.WireLabApi()` — подписка на события LabAPI + трансляция в структуры
- [ ] Пачка 1 (раунд): `RoundEvents.Waiting/Start/End/Restart` → ServerEvents LabAPI
- [ ] Пачка 2 (игрок-жизнь): `Join/Leave/Spawn/ChangeRole/Dead/Dies` (самые частые)
- [ ] Пачка 3 (бой): `Damage/Attack` → `PlayerEvents.Hurting`
- [ ] Пачка 4 (двери/предметы/эскейп): `InteractDoor/PickupItem/Escape/UsedItem/ChangeItem`
- [ ] Пачка 5 (SCP): `Scp914/Scp173/Scp096/Scp079/Scp106` события
- [ ] Пачка 6 (сервер/админ): `RemoteAdminCommand/GameConsoleCommand/репорты`
- [ ] Пачка 7 (карта/варх): `OpenDoor/DamageDoor/TriggerTesla/CreatePickup/AlphaWarhead`
- [ ] Пачка 8 (эффекты): `EffectEnabled/EffectDisabled`

### Обёртки/контроллеры
- [ ] `Map` (Rooms/Doors/… как `List<T>` + extension `TryFind`/`Find`)
- [ ] `Room` полная (Type↔RoomName маппинг, Lights, Position)
- [ ] `Door` полная (Open/Name/Type/AddPart/GameObject)
- [ ] `Lift`/`Elevator`, `Tesla`, `Cassie`, `Generator`
- [ ] `Player.Effects` (EffectType enum + Enable/TryGet/DisableAll)
- [ ] `Player.RoleInformation.Scp*` (Scp079.Lvl и пр. — SCP-роль API)

### Прочее
- [ ] Публичайзер `Assembly-CSharp`→`_public` + `Mirror`→`_public` (закрыть 5× CS0122)
      — через Mono.Cecil-скрипт ИЛИ установить dotnet SDK + `BepInEx.AssemblyPublicizer.Cli`
- [ ] `SCPLogs.dll`: заглушить зависимость в `plugin/Loli/Logs/` ИЛИ получить у основателя
- [ ] `Qurre.API.Addons.Audio` (аудио-плеер) — отдельный модуль
- [ ] Закрыть 89× CS0234 (под-namespace'ы Qurre: `Objects`, `World.Map`, `Addons.Models` и т.д.)

### Финал P0
- [ ] `build-plugin.ps1` → 0 ошибок, `Loli.dll` собран
- [ ] Разложить `Qurre.dll` + `Loli.dll` в plugins-папку LabAPI, проверить загрузку на сервере
- [ ] Прогнать Harmony-патчи (`PatchAll` обернуть в try/catch — см. `docs/04`)

---

## 🟧 P1 — Бэкенд (Node + MongoDB)

- [ ] Поднять Mongo (`backend/docker-compose.yml`)
- [ ] Заполнить `config.js` сервисов новыми секретами (`backend/config-templates/`)
- [ ] Убрать 3 отсутствующих сервиса из `pm2.config.js` (degraded/web-proxy/scpsl-mirror)
- [ ] Запустить `scp-socket` + `loli-api`, проверить коннект `:2467` с игрой
- [ ] Сгенерировать новый `ApiToken` (синхронно в плагине и `scp-socket`)

---

## 🟨 P2 — Инфраструктура запуска

> 📌 **Временный хостинг на первое время:** французская нода **Hostkey `секретно`**,
> `vm.nano` = **2 vCore / 2 GB RAM / 60 GB SSD** (из проекта Зоркофф). **Уже занята** —
> там REALITY-выход (Xray) для Telegram-трафика Зоркофф. ⚠️ НЕ ронять `xray-fr`/REALITY.
> Реальный потолок на 2 GB (после Xray+ОС свободно ~1.3–1.5 ГБ):
> - ✅ **лёгкий Node-бэкенд** (`scp-socket` + `loli-api`) для теста связки игра↔БД — влезет;
> - 🟡 **мини тест-сервер SCP:SL** (10–15 слотов, флавор `NR`) — влезет ВПРИТЫК, только если
>   Mongo вынести наружу и Xray под низкой нагрузкой; для теста/ностальгии с друзьями — ок,
>   для реального онлайна — нет (2 vCore burst — слабое single-thread ядро, будет лагать);
> - 🔴 **полный стек (4 сервера + Mongo + боты)** — не влезет. Нужно отдельное железо.

- [ ] (старт) Поднять на фр-ноде `секретно` лёгкий бэкенд для теста — НЕ трогая Xray
- [ ] Нормальное железо (НЕ 1 ГБ): игровой узел — быстрое ядро + 2–4 ГБ/инстанс
- [ ] Новый Steam server token + verkey → попасть в список серверов
- [ ] Домен + SSL, подключить сайт
- [ ] Один публичный NoRules-сервер (флавор `NR`)

---

## 🟦 P3 — Организация / не-техническое

- [ ] Разговор с основателем (питч — `docs/07_FOUNDER_PITCH.md`): дамп БД + бренд
- [ ] Прощупать старую аудиторию через YouTube-канал

---

## ✅ Сделано
- [x] Аудит трёх репозиториев + 9 документов
- [x] Версия игры зафиксирована (v14.x)
- [x] Стенд развёрнут (сервер SCP:SL, LabApi 1.1.7, 147 DLL, компилятор)
- [x] Полная карта API LabApi выгружена
- [x] Shim-фундамент компилируется (Log/Server/Round/Player+подобъекты/контроллеры-мин)
- [x] Каркас диспетчера + точка входа LabAPI
- [x] Перепись ошибок плагина (887 → план выше)
- [x] agent-exchange протокол + общий лог
