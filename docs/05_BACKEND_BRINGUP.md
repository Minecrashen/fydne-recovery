# Поднятие бэкенда FYDNE (Node.js + MongoDB)

Бэкенд — самая живая часть проекта (свежие зависимости, поддерживался до окт-2025).
Поднимается быстро. 14 сервисов под pm2 (в `cluster`).

## Порядок запуска (минимум для одного сервера)

Для старта достаточно: **MongoDB → scp-socket → loli-api → scp-web**.
Остальное (clans, connect, bots, cdn) подключать по мере роста.

## 1. MongoDB

Подними локально через Docker (`backend/docker-compose.yml`):
```bash
cd backend && docker compose up -d
# Mongo доступен на mongodb://localhost:27017
```
Если есть **дамп от основателя**:
```bash
mongorestore --uri="mongodb://localhost:27017" ./dump
```

## 2. Конфиги сервисов

Каждый сервис читает свой `config.js`. В публичном репо значения **затёрты** (пустые) —
их надо заполнить. ⚠️ **Все секреты — новые** (старые скомпрометированы публикацией).

Сводка обязательных полей (шаблоны — в `backend/config-templates/`):

| Сервис | Ключевые поля |
|---|---|
| `scp-socket/config.js` | `ip`, `webip`, `socketip`, `ApiToken` (новый!), `mongoDB` |
| `loli-api/config.js` | `ipinfoToken`, `steamApi` (новый Steam Web API key), `domain`, `mongoDB` |
| `scp-web/config.js` | `dashboard.{api,connect,baseURL,cdn,socketio}`, платёжки (`yookassa`, `yoomoney`, `paypalych`, `boosty`, `donationAlerts`), `mongoDB` |
| `web-connect/config.js` | `url`, `cdn`, `SteamAPI`, `token`, `mongoDB` |
| `web-socket/config.js` | `dashboard.socketIp`, `cdn`, `mongoDB` |
| `web-clans-socket/config.js` | `mongoDB` |

> `ApiToken` в `scp-socket/config.js` **должен совпадать** с `ApiToken` в плагине
> (`Loli-Merged/Loli/Core.cs`). Сгенерируй новый и пропиши в обоих местах.

## 3. Запуск под pm2

`pm2.config.js` в корне веб-репо ссылается на 14 приложений, **3 из которых отсутствуют**
в репозитории (`degraded.service`, `web-proxy`, `scpsl-mirror`). Перед запуском —
закомментировать их, иначе pm2 будет рестартить мёртвые сервисы.

```bash
npm i -g pm2
pm2 start pm2.config.js --only "scp socket,loli api,web"
pm2 logs
```

## 4. Проверка связки игра↔бэкенд

1. `scp-socket` слушает `:2467` (порт зашит в `socket.js`).
2. Плагин (`Core.cs`) подключается клиентом `QurreSocket.Client(2467, SocketIP)`.
3. После коннекта плагин шлёт `SCPServerInit` с `ApiToken` → сервер сверяет с `config.ApiToken`.
4. Если токены совпали — пойдёт обмен (`server.addip`, `database.get.*` и т.д.).

Признак успеха: в логе `scp-socket` появляется `SCPServerInit` от игрового сервера,
в логе плагина — `Connected to Socket`.

## 5. Зависимости Node

Стек (из `package.json`): `mongoose@7/8`, `express@4`, `socket.io@4`, `@sentry/node@9`,
`qurre-socket`. Менеджер — `pnpm` (видно по `pnpm-lock.yaml`).
```bash
cd scp-socket && pnpm i   # и так для каждого сервиса
```

## Чек-лист
- [ ] MongoDB поднят (+ дамп залит, если есть)
- [ ] Новые секреты сгенерированы (`ApiToken`, `steamApi`, платёжки)
- [ ] `config.js` заполнены
- [ ] 3 отсутствующих сервиса убраны из `pm2.config.js`
- [ ] `scp-socket` видит коннект игрового сервера
