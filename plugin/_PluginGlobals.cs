// Build-time мост для плагина (НЕ часть оригинала): глобальные алиасы там, где Qurre
// держал типы в namespace, отличном от импортов конкретных файлов плагина.
// В Qurre класс Player жил в Qurre.API.Controllers; часть файлов импортирует только
// Qurre.API. Глобальный алиас делает `Player` видимым во всех файлах компиляции.
global using Player = Qurre.API.Player;
