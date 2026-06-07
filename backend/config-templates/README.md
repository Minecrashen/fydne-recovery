# Шаблоны конфигов бэкенда

Скопируй нужный блок в `config.js` соответствующего сервиса в репо `fydne-prod-web`.
⚠️ Все секреты — **новые** (старые из публичного репо скомпрометированы).
`ApiToken` должен совпадать с тем, что в плагине (`Loli-Merged/Loli/Core.cs`).

---

### scp-socket/config.js
```js
module.exports = {
    ip: '0.0.0.0',          // на каком интерфейсе слушать сокет :2467
    webip: '',              // адрес web-сервиса
    socketip: '',           // адрес socket.io
    ApiToken: 'СГЕНЕРИРУЙ_НОВЫЙ', // == ApiToken в плагине Core.cs
    mongoDB: 'mongodb://localhost:27017/fydne'
}
```

### loli-api/config.js
```js
module.exports = {
    ipinfo: '',
    ipinfoToken: '',        // токен ipinfo.io
    discord: '',            // вебхук/токен discord
    steamApi: 'НОВЫЙ_STEAM_WEB_API_KEY', // https://steamcommunity.com/dev/apikey
    domain: 'твой-домен',
    mongoDB: 'mongodb://localhost:27017/fydne'
}
```

### web-connect/config.js
```js
module.exports = {
    url: 'твой-домен',
    cdn: 'https://cdn.твой-домен',
    SteamAPI: 'НОВЫЙ_STEAM_WEB_API_KEY',
    token: 'СГЕНЕРИРУЙ_НОВЫЙ',
    mongoDB: 'mongodb://localhost:27017/fydne'
}
```

### web-socket/config.js
```js
module.exports = {
    testing: true,
    dashboard: { socketIp: '', cdn: 'https://cdn.твой-домен' },
    mongoDB: 'mongodb://localhost:27017/fydne'
}
```

### web-clans-socket/config.js
```js
module.exports = { testing: true, mongoDB: 'mongodb://localhost:27017/fydne' }
```

### scp-web/config.js (фрагмент — самое важное)
```js
module.exports = {
    testing: false,
    dashboard: {
        ip: '',
        safe: 'https',
        api: 'api.твой-домен',
        connect: 'connect.твой-домен',
        baseURL: 'твой-домен',
        cdn: 'https://cdn.твой-домен',
        socketio: 'https://socket.твой-домен'
    },
    // Платёжки доната — заполнять по мере подключения (фаза 3):
    // yookassa / yoomoney / paypalych / boosty / donationAlerts / donationPay / qiwiP2P
    mongoDB: 'mongodb://localhost:27017/fydne'
}
```
