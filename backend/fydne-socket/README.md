# FYDNE socket recovery backend

Minimal QurreSocket-compatible TCP backend for the recovered FYDNE plugin.

It replaces the missing original `scp-socket` enough for local gameplay tests:

- listens on TCP `2467`;
- uses the legacy frame format: `JSON.stringify({ ev, args }) + U+22E0`;
- persists users, XP, money, admins, online players, and TPS into `data/store.json`;
- returns safe empty donate/customization/clan/patrol data until those systems are rebuilt.

## Start

```powershell
cd "C:\Users\Admin\Downloads\fydne recovery\backend\fydne-socket"
npm start
```

Then start the SCP:SL server with:

```powershell
$env:FYDNE_SOCKET_ENABLED="1"
$env:FYDNE_SOCKET_IP="127.0.0.1"
$env:FYDNE_RECOVERY_MODE="1"
```

For a future public server, set `FYDNE_RECOVERY_MODE=0` after solo-test behavior and replacement systems are stable.

## Store

The backend creates:

```text
backend/fydne-socket/data/store.json
```

This file is local runtime data and should not be committed.
