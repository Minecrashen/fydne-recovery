# Миграция Qurre → LabAPI через shim

Решение команды: уйти от Qurre (single-maintainer, доступен только у основателя).
Вместо переписывания ~196 файлов плагина — **строим тонкий shim, реализующий API Qurre
поверх LabAPI**. Плагин компилируется почти без правок.

## Ключевой трюк: drop-in замена

Проект shim'а собирает сборку с **`AssemblyName = Qurre`**. Плагин ссылается на `Qurre`
(`<Reference Include="Qurre">`) — и получает наш shim вместо оригинала. Правок в `.csproj`
плагина и в коде — минимум.

```
plugin/
├── Loli/              ← плагин FYDNE (почти не трогаем)
└── QurreShim/         ← наш shim, собирает Qurre.dll поверх LabAPI
```

## Спецификация поверхности (извлечена из кода плагина)

### Атрибуты (`Qurre.API.Attributes`)
| Атрибут | Сигнатура | Использование |
|---|---|---|
| `PluginInit` | `(string name, string author, string version)` | 1× на классе `Core` |
| `PluginEnable` | маркер на методе | 1× |
| `PluginDisable` | маркер на методе | 1× |
| `EventMethod` | `(object eventType, int priority = 0)` | 376× |
| `EventsIgnore` | маркер | 2× |

> `EventMethod` принимает `object`, т.к. используется с 7 разными enum'ами (boxed enum constant).

### Enum'ы событий (`Qurre.Events`) — ~75 членов
`RoundEvents` (6), `PlayerEvents` (33), `ScpEvents` (16), `ServerEvents` (5),
`MapEvents` (8), `EffectEvents` (2), `AlphaEvents` (3). Полный список — в `Events/Enums.cs`.

### Event-структуры (`Qurre.Events.Structs`) — ~70 типов
Самые частые: `SpawnEvent` (39), `GameConsoleCommandEvent` (29), `RemoteAdminCommandEvent` (21),
`ChangeRoleEvent` (19), `DamageEvent` (18), `JoinEvent` (17), `DeadEvent` (13)…
Каждая несёт поля, к которым обращается плагин (`ev.Player`, `ev.Position`, `ev.Role`,
`ev.Allowed` и т.д.) — строятся из args соответствующего события LabAPI.

### API-обёртки (`Qurre.API`, `.Controllers`, `.World`, `.Objects`)
Топ по использованию (число обращений):
- `Player.UserInformation` (146), `Player.Inventory` (131), `Player.RoleInformation` (105),
  `Player.Tag` (100), `Player.List` (93), `Player.Client` (78), `Player.MovementState` (39),
  `Player.Variables` (38), `Player.HealthInformation` (16)…
- `Round.CurrentRound` (55), `Round.ElapsedTime` (32), `Round.Waiting` (14)…
- `Map.Rooms` (15), `Map.Doors` (15), `Map.Broadcast` (14)…
- `Server.Port` (10), `Server.Ip` (8), `Server.FriendlyFire` (7)…
- `Log.Info/Error/Debug/Custom`
- Контроллеры: `Door`, `Room`, `Lift`, `Tesla`, `Cassie`, `Warhead`

## Карта событий Qurre → LabAPI (черновик)

| Qurre event | LabAPI событие (примерно) |
|---|---|
| `PlayerEvents.Join` | `PlayerEvents.Joined` |
| `PlayerEvents.Leave` | `PlayerEvents.Left` |
| `PlayerEvents.Spawn` | `PlayerEvents.Spawned` |
| `PlayerEvents.ChangeRole` | `PlayerEvents.ChangingRole` |
| `PlayerEvents.Damage` | `PlayerEvents.Hurting` |
| `PlayerEvents.Dead/Dies` | `PlayerEvents.Death/Dying` |
| `PlayerEvents.Escape` | `PlayerEvents.Escaping` |
| `PlayerEvents.InteractDoor` | `PlayerEvents.InteractingDoor` |
| `RoundEvents.Waiting` | `ServerEvents.WaitingForPlayers` |
| `RoundEvents.Start` | `ServerEvents.RoundStarted` |
| `RoundEvents.End` | `ServerEvents.RoundEnded` |
| `RoundEvents.Restart` | `ServerEvents.RoundRestarted` |
| `MapEvents.OpenDoor` | `PlayerEvents.InteractingDoor` / Map door API |
| `AlphaEvents.Detonate` | `WarheadEvents.Detonated` |
| `ServerEvents.RemoteAdminCommand` | `CommandEvents` / RA hook |
| `ScpEvents.Scp914UpgradePlayer` | `Scp914Events.ProcessingPlayer` |
| … | (дополняется при реализации) |

Точные имена сверяются по реальному `LabApi.dll` (рефлексия/ildasm) после установки сервера.

## Движок диспетчеризации (`Core/EventDispatcher.cs`)

1. На `PluginEnable` сканируем сборку плагина на методы с `[EventMethod]`.
2. Группируем по типу события, сортируем по `priority`.
3. Подписываемся на соответствующие события LabAPI.
4. В обработчике LabAPI: строим Qurre-структуру из args, вызываем методы плагина по порядку,
   прокидываем `ev.Allowed`/отмену обратно в LabAPI.

## Статус реализации

- [x] Спецификация поверхности извлечена из кода
- [x] Enum'ы событий (`src/Enums.cs`) — **компилируются**
- [x] Атрибуты (`src/Attributes.cs`) — **компилируются**
- [x] Build-харнесс `scripts/build-shim.ps1` (csc против dependencies/) — **Qurre.dll собирается** ✅
- [x] Окружение развёрнуто: сервер + LabApi 1.1.7 + 147 DLL (см. `09_BUILD_ENV_STATUS.md`)
- [ ] Диспетчер событий `EventDispatcher` (следующий шаг — против LabApi.Events.Handlers)
- [ ] Event-структуры (~70, по приоритету частоты)
- [ ] Обёртка `Player` (самая используемая — 146× UserInformation и т.д.)
- [ ] Обёртки `Map`/`Round`/`Server`/`Log`
- [ ] Контроллеры `Door`/`Room`/`Lift`/`Tesla`/`Cassie`
- [ ] Аудио-аддон (`Qurre.API.Addons.Audio`) — отдельная задача
- [ ] Публичайзер Assembly-CSharp/Mirror (для прямых патчей плагина)
- [ ] Первая успешная компиляция плагина против shim'а
```
```
