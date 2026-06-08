// Build-time мост для плагина (НЕ часть оригинала). Глобальные алиасы.
// Player вынесен в Qurre.API.Controllers (как в оригинальном Qurre) — алиас не нужен.
// Primitive: ControlRoom.cs импортит SchematicUnity.API.Objects, остальные — Qurre.API.Addons.Models;
// глобальный алиас делает тип видимым везде без правок плагина.
global using Primitive = Qurre.API.Addons.Models.Primitive;
global using RoomType = Qurre.API.Objects.RoomType;
global using DoorType = Qurre.API.Objects.DoorType;
global using GeneratorStatus = Qurre.API.Objects.GeneratorStatus;
global using WorkstationStatus = Qurre.API.Objects.WorkstationStatus;
