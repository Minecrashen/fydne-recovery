// Qurre.API.JsonConfig — простой JSON-конфиг на игрока/плагин (Newtonsoft).
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Qurre.API
{
    public class JsonConfig
    {
        static readonly List<JsonConfig> _all = new List<JsonConfig>();
        readonly string _path;
        Dictionary<string, object> _data = new Dictionary<string, object>();

        public JsonConfig(string name)
        {
            var dir = Path.Combine(LabApi.Loader.Features.Paths.PathManager.Configs.FullName, "Qurre");
            Directory.CreateDirectory(dir);
            _path = Path.Combine(dir, name + ".json");
            if (File.Exists(_path))
                try { _data = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(_path)) ?? new Dictionary<string, object>(); }
                catch { }
            _all.Add(this);
        }

        public T SafeGetValue<T>(string key, T defaultValue)
        {
            if (_data.TryGetValue(key, out var v) && v != null)
            {
                try
                {
                    if (v is T tv) return tv;
                    return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(v));
                }
                catch { }
            }
            _data[key] = defaultValue;
            return defaultValue;
        }

        public void Set(string key, object value) => _data[key] = value;
        public void Save() => File.WriteAllText(_path, JsonConvert.SerializeObject(_data, Formatting.Indented));

        public static void UpdateFile()
        {
            foreach (var c in _all) c.Save();
        }
    }
}
