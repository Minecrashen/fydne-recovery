"use strict";

const fs = require("fs");
const net = require("net");
const path = require("path");

const PORT = Number(process.env.FYDNE_SOCKET_PORT || 2467);
const HOST = process.env.FYDNE_SOCKET_HOST || "0.0.0.0";
const DATA_DIR = process.env.FYDNE_SOCKET_DATA || path.join(__dirname, "data");
const STORE_PATH = path.join(DATA_DIR, "store.json");
const SEP = "⋠";
let store = null;
let saveTimer = null;

function defaultStore() {
  return {
    nextUserId: 1,
    users: {},
    admins: [],
    clans: [],
    online: {},
    tps: {}
  };
}

function loadStore() {
  fs.mkdirSync(DATA_DIR, { recursive: true });
  if (!fs.existsSync(STORE_PATH)) {
    const store = defaultStore();
    saveStore(store);
    return store;
  }

  try {
    return { ...defaultStore(), ...JSON.parse(fs.readFileSync(STORE_PATH, "utf8")) };
  } catch (error) {
    const broken = STORE_PATH + ".broken-" + Date.now();
    fs.copyFileSync(STORE_PATH, broken);
    console.error(`[fydne-socket] store parse failed, backed up to ${broken}:`, error.message);
    const store = defaultStore();
    saveStore(store);
    return store;
  }
}

store = loadStore();

function saveStore(next = store) {
  store = next;
  fs.mkdirSync(DATA_DIR, { recursive: true });
  fs.writeFileSync(STORE_PATH, JSON.stringify(store, null, 2), "utf8");
}

function scheduleSave() {
  if (saveTimer) return;
  saveTimer = setTimeout(() => {
    saveTimer = null;
    saveStore();
  }, 250);
}

function normalizeUserId(id, isDiscord = false) {
  const raw = String(id || "").trim();
  if (!raw) return "";
  if (raw.includes("@")) return raw;
  return raw + (isDiscord === true || String(isDiscord).toLowerCase() === "true" ? "@discord" : "@steam");
}

function strippedUserId(fullUserId) {
  return String(fullUserId || "").replace("@steam", "").replace("@discord", "");
}

function nextLevelTo(level) {
  return Math.max(100, (Number(level) + 1) * 1000);
}

function recalcLevel(user) {
  user.xp = Number(user.xp || 0);
  user.lvl = Number(user.lvl || 0);
  user.to = Number(user.to || nextLevelTo(user.lvl));
  while (user.xp >= user.to) {
    user.lvl += 1;
    user.to = nextLevelTo(user.lvl);
  }
}

function createUser(fullUserId) {
  const id = store.nextUserId++;
  const login = strippedUserId(fullUserId);
  return {
    money: 0,
    xp: 0,
    lvl: 0,
    to: nextLevelTo(0),
    donater: false,
    trainee: false,
    helper: false,
    mainhelper: false,
    admin: false,
    mainadmin: false,
    selection: false,
    control: false,
    maincontrol: false,
    it: false,
    warnings: 0,
    prefix: "",
    clan: "",
    clanColor: "",
    found: true,
    name: "[recovered]",
    id,
    discord: "",
    login,
    gradient: {
      fromA: "",
      toA: "",
      fromB: "",
      toB: "",
      prefix: ""
    }
  };
}

function ensureUser(fullUserId) {
  const key = normalizeUserId(fullUserId);
  if (!key) return createUser("");
  if (!store.users[key]) {
    store.users[key] = createUser(key);
    scheduleSave();
  }
  recalcLevel(store.users[key]);
  return store.users[key];
}

function findUserByNumericId(id) {
  const numeric = Number(id);
  return Object.values(store.users).find((user) => Number(user.id) === numeric) || null;
}

function statsOf(user) {
  recalcLevel(user);
  return {
    xp: Number(user.xp || 0),
    lvl: Number(user.lvl || 0),
    to: Number(user.to || nextLevelTo(user.lvl || 0)),
    money: Number(user.money || 0)
  };
}

function defaultCustomize() {
  const gensMod = {
    adrenaline_compatible: false,
    adrenaline_rush: false,
    native_armor: false
  };
  return {
    genetics: {
      ClassD: { ...gensMod },
      Scientist: { ...gensMod },
      Guard: { ...gensMod },
      Mtf: { ...gensMod },
      Chaos: { ...gensMod },
      Serpents: { ...gensMod }
    },
    scales: {
      ClassD: 100,
      Scientist: 100,
      Guard: 100,
      Mtf: 100,
      Chaos: 100,
      Serpents: 100
    }
  };
}

function pack(ev, args = []) {
  return JSON.stringify({ ev, args }) + SEP;
}

function send(socket, ev, args = []) {
  socket.write(pack(ev, args), "utf8");
}

function parseFrames(socket, chunk, onMessage) {
  socket._fydneBuffer = (socket._fydneBuffer || "") + chunk.toString("utf8");
  const frames = socket._fydneBuffer.split(SEP);
  socket._fydneBuffer = frames.pop();

  for (const frame of frames) {
    if (!frame.trim()) continue;
    try {
      onMessage(JSON.parse(frame));
    } catch (error) {
      console.error("[fydne-socket] bad frame:", error.message, frame.slice(0, 200));
    }
  }
}

function handle(socket, message) {
  const ev = message.ev;
  const args = Array.isArray(message.args) ? message.args : [];

  switch (ev) {
    case "SCPServerInit":
      send(socket, "SCPServerInit", []);
      break;

    case "server.database.clans":
      send(socket, "socket.database.clans", [store.clans || []]);
      break;

    case "database.get.adm.steams":
      send(socket, "database.get.adm.steams", [JSON.stringify(store.admins || [])]);
      break;

    case "database.get.data": {
      const fullUserId = normalizeUserId(args[3] || args[0], args[1]);
      const user = ensureUser(fullUserId);
      send(socket, "database.get.data", [JSON.stringify(user), fullUserId]);
      break;
    }

    case "database.get.stats": {
      const responseUserId = String(args[2] || normalizeUserId(args[0], args[1]));
      const fullUserId = normalizeUserId(String(responseUserId).replace("+updating", ""), args[1]);
      const user = ensureUser(fullUserId);
      send(socket, "database.get.stats", [JSON.stringify(statsOf(user)), responseUserId]);
      break;
    }

    case "database.add.stats": {
      const fullUserId = normalizeUserId(args[4] || args[0], args[1]);
      const user = ensureUser(fullUserId);
      user.xp = Number(user.xp || 0) + Number(args[2] || 0);
      user.money = Number(user.money || 0) + Number(args[3] || 0);
      if (user.money < 0) user.money = 0;
      recalcLevel(user);
      scheduleSave();
      send(socket, "database.get.stats", [JSON.stringify(statsOf(user)), fullUserId]);
      break;
    }

    case "database.internal.unsafe.set_level": {
      const fullUserId = normalizeUserId(args[0]);
      const user = ensureUser(fullUserId);
      user.lvl = Math.max(0, Number(args[1] || 0));
      user.to = nextLevelTo(user.lvl);
      scheduleSave();
      send(socket, "database.get.stats", [JSON.stringify(statsOf(user)), fullUserId]);
      break;
    }

    case "database.get.donate.roles":
      send(socket, "database.get.donate.roles", [JSON.stringify([]), String(args[2] || "")]);
      break;

    case "database.get.donate.customize":
      send(socket, "database.get.donate.customize", [JSON.stringify(defaultCustomize()), String(args[1] || "")]);
      break;

    case "database.get.donate.ra":
      send(socket, "database.get.donate.ra", [JSON.stringify([]), String(args[1] || "")]);
      break;

    case "database.get.nitro":
      send(socket, "database.get.nitro", [String(args[0] || "")]);
      break;

    case "database.get.patrol":
      send(socket, "database.get.patrol", [JSON.stringify({}), String(args[0] || ""), false]);
      break;

    case "server.clearips":
      store.online = {};
      scheduleSave();
      break;

    case "server.addip": {
      const userId = String(args[2] || "");
      if (userId) {
        store.online[userId] = {
          serverId: args[0],
          ip: args[1],
          nickname: args[3],
          updatedAt: new Date().toISOString()
        };
        scheduleSave();
      }
      break;
    }

    case "server.join":
      break;

    case "server.leave":
      delete store.online[String(args[1] || "")];
      scheduleSave();
      break;

    case "server.tps":
      store.tps[String(args[0] || "0")] = {
        serverName: args[1],
        tps: args[2],
        players: args[3],
        updatedAt: new Date().toISOString()
      };
      scheduleSave();
      break;

    case "database.add.time":
      // args: user database id, server id, seconds/minutes-like total from legacy plugin.
      break;

    case "database.remove.admin":
    case "database.admin.ban":
    case "database.admin.kick":
      // Admin moderation audit is intentionally accepted but not enforced in the recovery store yet.
      break;

    default:
      console.log("[fydne-socket] unhandled event", ev, JSON.stringify(args).slice(0, 300));
      break;
  }
}

const server = net.createServer((socket) => {
  const remote = `${socket.remoteAddress}:${socket.remotePort}`;
  console.log(`[fydne-socket] client connected ${remote}`);
  socket.setEncoding("utf8");

  send(socket, "connect", []);
  send(socket, "token.required", []);

  socket.on("data", (chunk) => parseFrames(socket, chunk, (message) => handle(socket, message)));
  socket.on("error", (error) => console.error(`[fydne-socket] client error ${remote}:`, error.message));
  socket.on("close", () => console.log(`[fydne-socket] client disconnected ${remote}`));
});

server.listen(PORT, HOST, () => {
  console.log(`[fydne-socket] listening on ${HOST}:${PORT}`);
  console.log(`[fydne-socket] store ${STORE_PATH}`);
});

process.on("SIGINT", () => {
  saveStore();
  process.exit(0);
});

process.on("SIGTERM", () => {
  saveStore();
  process.exit(0);
});
