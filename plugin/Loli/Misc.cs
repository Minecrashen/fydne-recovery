using Loli.DataBase;
using Newtonsoft.Json;
using Qurre.API.Addons.Models;
using Qurre.API.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using Loli.HintsCore;
using UnityEngine;

namespace Loli
{
    public class CustomDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public CustomDictionary() : base() { }

        new public bool TryGetValue(TKey key, out TValue value)
        {
            try
            {
                return base.TryGetValue(key, out value);
            }
            catch
            {
                value = default;
                return false;
            }
        }

        new public bool ContainsKey(TKey key)
        {
            try
            {
                return base.ContainsKey(key);
            }
            catch
            {
                return false;
            }
        }

        new public void Add(TKey key, TValue value)
        {
            try
            {
                base.Add(key, value);
            }
            catch { }
        }

        new public bool Remove(TKey key)
        {
            try
            {
                return base.Remove(key);
            }
            catch
            {
                return false;
            }
        }
    }
    internal class BansCounts
    {
        readonly List<DateTime> _dates = new();
        internal int Counts => _dates.Count(x => (DateTime.Now - x).TotalSeconds < 30);
        internal void Add() => _dates.Add(DateTime.Now);
        internal void Clear() => _dates.Clear();
    }
    public class FixPrimitiveSmoothing : MonoBehaviour
    {
        readonly float _interval = 0.1f;
        float _nextCycle = 0f;

        internal Model Model;

        void Start()
        {
            _nextCycle = Time.time;
        }
        void Update()
        {
            if (Model is null)
                return;

            if (Time.time < _nextCycle)
                return;

            _nextCycle += _interval;

            for (int i = 0; i < Model.Primitives.Count; i++)
            {
                var prim = Model.Primitives[i];
                prim.Primitive.Base.NetworkMovementSmoothing = prim.Primitive.MovementSmoothing;
                prim.Primitive.Base.NetworkRotation = prim.GameObject.transform.rotation;
                prim.Primitive.Base.NetworkPosition = prim.GameObject.transform.position;
            }
        }
    }
    public class FixOnePrimitiveSmoothing : MonoBehaviour
    {
        readonly float _interval = 0.1f;
        float _nextCycle = 0f;

        internal Primitive Primitive;

        void Start()
        {
            _nextCycle = Time.time;
        }
        void Update()
        {
            if (Primitive is null)
                return;

            if (Time.time < _nextCycle)
                return;

            _nextCycle += _interval;

            Primitive.Base.NetworkMovementSmoothing = Primitive.MovementSmoothing;
            Primitive.Base.NetworkRotation = Primitive.Rotation;
            Primitive.Base.NetworkPosition = Primitive.Position;
        }
    }
    public class RainbowTagController : MonoBehaviour
    {
        private ServerRoles _roles;
        private string _originalColor;

        private int _position = 0;
        private float _nextCycle = 0f;

        public static string[] Colors =
        {
            "pink",
            "red",
            "brown",
            "silver",
            "light_green",
            "crimson",
            "cyan",
            "aqua",
            "deep_pink",
            "tomato",
            "yellow",
            "magenta",
            "blue_green",
            "orange",
            "lime",
            "green",
            "emerald",
            "carmine",
            "nickel",
            "mint",
            "army_green",
            "pumpkin"
        };

        internal static float Interval { get; set; } = 0.5f;


        private void Start()
        {
            _roles = GetComponent<ServerRoles>();
            _nextCycle = Time.time;
            _originalColor = _roles.Network_myColor;
        }


        private void OnDisable()
        {
            _roles.Network_myColor = _originalColor;
        }


        private void Update()
        {
            if (Time.time < _nextCycle) return;
            _nextCycle += Interval;

            _roles.Network_myColor = Colors[_position];

            if (++_position >= Colors.Length)
                _position = 0;
        }
    }

    public class GradientColorTag : MonoBehaviour
    {
        internal Player _player;
        internal UserData _data;

        List<Color> _colorA = new();
        List<Color> _colorB = new();

        string _prefix = string.Empty;

        bool setuped = false;

        private float _nextCycle = 0f;

        internal static float Interval { get; set; } = 0.1f;


        public static GradientColorTag Create(Player pl, UserData data)
        {
            var component = pl.ReferenceHub.GetComponent<GradientColorTag>() ?? pl.GameObject.AddComponent<GradientColorTag>();
            component._player = pl;
            component._data = data;
            component.Setup();

            data.prefix = (data.prefix.Replace("%gradient%", "") + " %gradient%").Trim();

            return component;
        }

        internal void Setup()
        {
            _prefix = Constants.ReplaceUnAllowedRegex.Replace(_data.gradient.prefix, "");
            _colorA = GenerateGradient(ColorFromHex(_data.gradient.fromA), ColorFromHex(_data.gradient.toA), _prefix.Replace(" ", "").Length);
            _colorB = GenerateGradient(ColorFromHex(_data.gradient.fromB), ColorFromHex(_data.gradient.toB), _prefix.Replace(" ", "").Length);
            setuped = true;
        }


        private void Update()
        {
            if (!setuped)
                return;

            if (Time.time < _nextCycle)
                return;

            _nextCycle += Interval;

            string gradient = string.Empty;
            string pref = $"{_prefix}";
            for (int i = 0; pref.Length != 0; i++)
            {
                char symbol = pref.First();
                while (symbol == ' ' && pref.Length != 0)
                {
                    gradient += symbol;
                    pref = pref.Substring(1);
                    symbol = pref.First();
                }

                Color colorA = _colorA.Last();
                Color colorB = _colorB.Last();

                try { colorA = _colorA.ElementAt(i); } catch { }
                try { colorB = _colorB.ElementAt(i); } catch { }

                Color color = Color.Lerp(colorA, colorB, Mathf.PingPong(Time.time, 1));
                gradient += $"<color={ColorToHex(color)}>{symbol}</color>".Replace("<", "\\u003C").Replace(">", "\\u003E");
                pref = pref.Substring(1);
            }

            _player.Administrative.RoleName = _data.cached_role.Replace("%gradient%", gradient);

            if (_player.UserInformation.InfoToShow.HasFlag(PlayerInfoArea.Badge))
            {
                _player.UserInformation.InfoToShow &= ~PlayerInfoArea.Badge;
                RefreshTag(_player.ReferenceHub.serverRoles);
            }
        }


        static void RefreshTag(ServerRoles sr)
        {
            string color = sr._myColor;
            sr.NetworkGlobalBadge = null;
            sr.NetworkGlobalBadgeSignature = null;
            sr.HiddenBadge = null;
            sr.GlobalHidden = false;
            sr.RpcResetFixed();
            sr.RefreshPermissions(true);
            sr.Network_myColor = color;
        }

        static readonly Dictionary<string, string> _gradientTextCache = new();
        public static string GradientText(string original, Color colorA, Color colorB, bool cache = true)
        {
            if (original.Length < 3)
                return $"<color={colorA.ToHex()}>{original}</color>";

            string key = original + colorA.ToHex() + colorB.ToHex();

            List<Color> gradients = GenerateGradient(colorA, colorB, original.Replace(" ", "").Length);

            if (gradients.Count < 2)
                return $"<color={colorA.ToHex()}>{original}</color>";

            if (cache && _gradientTextCache.TryGetValue(key, out string value))
                return value;

            string gradient = string.Empty;

            for (int i = 0; original.Length != 0; i++)
            {
                char symbol = original.First();
                while (symbol == ' ' && original.Length != 0)
                {
                    gradient += symbol;
                    original = original.Substring(1);
                    symbol = original.First();
                }

                Color color = gradients.Last();
                try { color = gradients.ElementAt(i); } catch { }

                gradient += $"<color={ColorToHex(color)}>{symbol}</color>";
                original = original.Substring(1);
            }

            if (cache)
                try { _gradientTextCache.Add(key, gradient); } catch { }

            return gradient;
        }

        public static Color ColorFromHex(string hex)
            => new Color32(
                Convert.ToByte(hex.Substring(1, 2), 16),
                Convert.ToByte(hex.Substring(3, 2), 16),
                Convert.ToByte(hex.Substring(5, 2), 16),
                255);

        public static string ColorToHex(Color32 color)
            => "#" + color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");


        public static List<Color> GenerateGradient(Color32 start, Color32 end, int steps)
        {
            List<Color> colors = new();

            if (steps <= 0)
                return colors;

            double aStep = (end.a - start.a) / steps;
            double rStep = (end.r - start.r) / steps;
            double gStep = (end.g - start.g) / steps;
            double bStep = (end.b - start.b) / steps;

            for (int i = 0; i < steps; i++)
            {
                int a = start.a + (int)(aStep * i);
                int r = start.r + (int)(rStep * i);
                int g = start.g + (int)(gStep * i);
                int b = start.b + (int)(bStep * i);

                Color intermediateColor = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
                colors.Add(intermediateColor);
            }

            return colors;
        }
    }


    [Serializable]
    class SteamMainInfoApi
    {
        [JsonProperty("personaname")]
        public string Name { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        [JsonProperty("avatarfull")]
        public string AvatarFull { get; set; }
    }


    [Serializable]
    class DoubfulData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("setuped")]
        public bool IsSetup { get; set; }

        [JsonProperty("banned")]
        public bool Banned { get; set; }

        [JsonProperty("days_since_last_ban")]
        public int DaysSinceLastBan { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("game_hours")]
        public int GameHours { get; set; }

        [JsonProperty("created")]
        public long Created { get; set; }

        [JsonProperty("created_formatted")]
        public string CreatedFormatted { get; set; }
    }

    [Serializable]
    class GeoIP
    {
        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("loc")]
        public string Loc { get; set; }

        [JsonProperty("org")]
        public string Org { get; set; }

        [JsonProperty("postal")]
        public string Postal { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }
    }


    [Serializable]
    class GetCheaterByUserid
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("ipAddress")]
        public string[] IpArray { get; set; }

        [JsonProperty("reports")]
        public CheaterReport[] Reports { get; set; }
    }

    [Serializable]
    class GetCheaterByIp
    {
        [JsonProperty("ipAddress")]
        public string IpAddress { get; set; }

        [JsonProperty("userIds")]
        public string[] UserIds { get; set; }

        [JsonProperty("reports")]
        public CheaterReport[] Reports { get; set; }
    }

    [Serializable]
    class CheaterReport
    {
        [JsonProperty("projectName")]
        public string Project { get; set; }

        [JsonProperty("isConfirmed")]
        public bool Verified { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("evidence")]
        public string Evidence { get; set; }
    }
}