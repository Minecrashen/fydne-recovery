using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Loli.Addons;
using Loli.Configs;
using Loli.DataBase.Modules;
using MEC;
using Qurre.API;
using Qurre.API.Addons;
using Qurre.API.Attributes;
using Qurre.API.Controllers;

namespace Loli
{
    [PluginInit("Loli", "fydne", "6.6.6")]
    static public class Core
    {
        #region peremens

        static internal string ServerName = "[data deleted]";
        static internal readonly SafeSocket Socket = new(2467, SocketIP);
        public static int MaxPlayers = GameCore.ConfigFile.ServerConfig.GetInt("max_players", 100);
        static internal string CDNUrl => System.Environment.GetEnvironmentVariable("FYDNE_CDN_URL") ?? string.Empty;

        static internal JsonConfig ConfigsCore { get; private set; }
        internal static WebHooks WebHooks { get; private set; }

        static internal int ServerID { get; private set; } = 0;
#if NR
        static internal int DonateID { get; private set; } = 1;
        static internal bool BlockStats { get; private set; }
#endif
        static internal short Ticks { get; set; } = 0;
        static internal int TicksMinutes { get; set; } = 0;
#if MRP
        static internal ushort Port => 7666; //Qurre.API.Server.Port;
#elif NR
        static internal ushort Port => 7779; // Qurre.API.Server.Port;
#endif
        static internal string ApiToken => System.Environment.GetEnvironmentVariable("FYDNE_API_TOKEN") ?? string.Empty;
        static internal string SteamToken => System.Environment.GetEnvironmentVariable("FYDNE_STEAM_WEB_API_KEY") ?? string.Empty;
        static internal string SocketIP => System.Environment.GetEnvironmentVariable("FYDNE_SOCKET_IP") ?? "127.0.0.1";
        static internal bool RecoveryMode
        {
            get
            {
                string value = System.Environment.GetEnvironmentVariable("FYDNE_RECOVERY_MODE") ?? "1";
                return value != "0" && !value.Equals("false", System.StringComparison.OrdinalIgnoreCase) && !value.Equals("no", System.StringComparison.OrdinalIgnoreCase);
            }
        }
        static internal bool SocketEnabled
        {
            get
            {
                string value = System.Environment.GetEnvironmentVariable("FYDNE_SOCKET_ENABLED") ?? string.Empty;
                return value == "1" || value.Equals("true", System.StringComparison.OrdinalIgnoreCase) || value.Equals("yes", System.StringComparison.OrdinalIgnoreCase);
            }
        }
        static internal bool TraceSocket
        {
            get
            {
                string value = System.Environment.GetEnvironmentVariable("FYDNE_TRACE_SOCKET") ?? string.Empty;
                if (value == "0" || value.Equals("false", System.StringComparison.OrdinalIgnoreCase) || value.Equals("no", System.StringComparison.OrdinalIgnoreCase) || value.Equals("off", System.StringComparison.OrdinalIgnoreCase))
                    return false;

                return RecoveryMode || value == "1" || value.Equals("true", System.StringComparison.OrdinalIgnoreCase) || value.Equals("yes", System.StringComparison.OrdinalIgnoreCase) || value.Equals("on", System.StringComparison.OrdinalIgnoreCase);
            }
        }
        static internal bool TraceSocketPayloads
        {
            get
            {
                string value = System.Environment.GetEnvironmentVariable("FYDNE_TRACE_SOCKET_PAYLOADS") ?? string.Empty;
                return value == "1" || value.Equals("true", System.StringComparison.OrdinalIgnoreCase) || value.Equals("yes", System.StringComparison.OrdinalIgnoreCase) || value.Equals("on", System.StringComparison.OrdinalIgnoreCase);
            }
        }
        static internal string APIUrl => System.Environment.GetEnvironmentVariable("FYDNE_API_URL") ?? string.Empty;

#if MRP
        static internal double PreMorningStatsCf => 1.5;
        static internal double MorningStatsCf => 1.25;
        static internal double DayStatsCf => 1.13;
#elif NR
        static internal double PreMorningStatsCf => 1.75;
        static internal double MorningStatsCf => 1.5;
        static internal double DayStatsCf => 1.2;
#endif

        static internal double PreNightCf => 1.13;
        static internal double NightCf => 1.25;

#if NR
        static internal double AverageCf => 1.1;
#endif

        #endregion

        #region Enable / Disable

        [PluginEnable]
        static internal void Enable()
        {
            Log.Custom($"Enable recovery={RecoveryMode} socketEnabled={SocketEnabled} traceSocket={TraceSocket} traceSocketPayloads={TraceSocketPayloads}", "FYDNE-BOOT", System.ConsoleColor.Green);

            ConfigsCore ??= new JsonConfig("Loli");
            WebHooks = ConfigsCore.SafeGetValue("WebHooks", new WebHooks());

            JsonConfig.UpdateFile();

            Socket.On("token.required", data => SocketConnected());
            Socket.On("connect", data =>
            {
                Log.Custom("Connected to Socket", "Connect", System.ConsoleColor.Blue);
                SocketConnected();
            });

            static void SocketConnected()
            {
                Socket.Emit("SCPServerInit", new string[] { ApiToken });
                Timing.CallDelayed(1f, () => Socket.Emit("server.clearips", new object[] { ServerID }));
                Timing.CallDelayed(2f, () =>
                {
                    try
                    {
                        foreach (var pl in Player.List)
                        {
                            Socket.Emit("server.addip", new object[]
                            {
                                ServerID,
                                pl.UserInformation.Ip,
                                pl.UserInformation.UserId,
                                pl.UserInformation.Nickname
                            });
                        }
                    }
                    catch
                    {
                    }
                });
            }

            UpdateServers();
            Timing.RunCoroutine(UpdateVerkey());

            CommandsSystem.RegisterRemoteAdmin("bp", BackupPower.Ra);
            CommandsSystem.RegisterRemoteAdmin("backup_power", BackupPower.Ra);

            Patrol.Init();
            Admins.Call();

            Scps.Scp294.Events.Init();

            PatchAllSafely();
        }

        [PluginDisable]
        static internal void Disable()
        {
            Log.Custom("Disable requested; restarting server", "FYDNE-BOOT", System.ConsoleColor.Yellow);
            Server.Restart();
        }

        static void PatchAllSafely()
        {
            Harmony harmony = new("fydne.loli");
            int patched = 0;
            int skipped = 0;

            foreach (System.Type type in typeof(Core).Assembly.GetTypes())
            {
                try
                {
                    harmony.CreateClassProcessor(type).Patch();
                    patched++;
                }
                catch (System.Exception ex)
                {
                    skipped++;
                    Log.Error($"Harmony patch skipped: {type.FullName}: {ex.GetType().Name}: {ex.Message}");
                }
            }

            Log.Custom($"Harmony patch pass finished patched={patched} skipped={skipped}", "FYDNE-BOOT", System.ConsoleColor.Green);
        }

        internal sealed class SafeSocket
        {
            readonly QurreSocket.Client _inner;
            readonly bool _enabled;
            int _noopSubscriptionId;

            public SafeSocket(int port, string ip)
            {
                _enabled = SocketEnabled;
                if (!_enabled)
                {
                    Trace($"disabled target={ip}:{port}");
                    return;
                }

                try
                {
                    _inner = new QurreSocket.Client(port, ip);
                    Trace($"created target={ip}:{port}");
                }
                catch (System.Exception ex)
                {
                    Log.Error($"QurreSocket disabled: {ex.GetType().Name}: {ex.Message}");
                    _enabled = false;
                }
            }

            public string On(string eventName, System.Action<object[]> action)
            {
                if (!_enabled || _inner == null)
                    return "disabled:" + ++_noopSubscriptionId;

                try
                {
                    Trace($"on {eventName}");
                    return _inner.On(eventName, data =>
                    {
                        Trace($"<= {eventName} {FormatArgs(eventName, data)}");
                        try
                        {
                            action(data);
                        }
                        catch (System.Exception ex)
                        {
                            Log.Error($"QurreSocket handler failed ({eventName}): {ex.GetType().Name}: {ex.Message}");
                        }
                    });
                }
                catch (System.Exception ex)
                {
                    Log.Error($"QurreSocket.On skipped ({eventName}): {ex.GetType().Name}: {ex.Message}");
                    return "disabled:" + ++_noopSubscriptionId;
                }
            }

            public void Off(string id)
            {
                if (!_enabled || _inner == null || string.IsNullOrEmpty(id) || id.StartsWith("disabled:"))
                    return;

                try
                {
                    _inner.Off(id);
                }
                catch (System.Exception ex)
                {
                    Log.Error($"QurreSocket.Off skipped ({id}): {ex.GetType().Name}: {ex.Message}");
                }
            }

            public void Emit(string eventName, object[] data)
            {
                if (!_enabled || _inner == null)
                    return;

                try
                {
                    Trace($"=> {eventName} {FormatArgs(eventName, data)}");
                    _inner.Emit(eventName, data);
                }
                catch (System.Exception ex)
                {
                    Log.Error($"QurreSocket.Emit skipped ({eventName}): {ex.GetType().Name}: {ex.Message}");
                }
            }

            static void Trace(string message)
            {
                if (TraceSocket)
                    Log.Custom(message, "FYDNE-SOCKET", System.ConsoleColor.DarkCyan);
            }

            static string FormatArgs(string eventName, object[] data)
            {
                if (data == null) return "args=null";
                if (!TraceSocketPayloads) return $"args={data.Length}";

                bool sensitiveEvent = IsSensitive(eventName);
                return "args=[" + string.Join(", ", data.Take(8).Select((value, index) => FormatValue(value, sensitiveEvent || index == 0 && eventName == "SCPServerInit"))) + (data.Length > 8 ? ", ..." : "") + "]";
            }

            static bool IsSensitive(string eventName)
            {
                if (string.IsNullOrEmpty(eventName)) return false;
                return eventName.IndexOf("token", System.StringComparison.OrdinalIgnoreCase) >= 0
                    || eventName.IndexOf("auth", System.StringComparison.OrdinalIgnoreCase) >= 0
                    || eventName.IndexOf("password", System.StringComparison.OrdinalIgnoreCase) >= 0
                    || eventName.IndexOf("SCPServerInit", System.StringComparison.OrdinalIgnoreCase) >= 0;
            }

            static string FormatValue(object value, bool redact)
            {
                if (redact) return "<redacted>";
                if (value == null) return "null";
                if (value is string text) return Quote(text);
                if (value is System.Array array)
                    return "[" + string.Join(",", array.Cast<object>().Take(8).Select(x => FormatValue(x, false))) + (array.Length > 8 ? ",..." : "") + "]";

                return Truncate(value.ToString(), 96);
            }

            static string Quote(string value)
            {
                return "\"" + Truncate((value ?? string.Empty).Replace("\r", "\\r").Replace("\n", "\\n"), 96) + "\"";
            }

            static string Truncate(string value, int max)
            {
                if (string.IsNullOrEmpty(value) || value.Length <= max) return value ?? string.Empty;
                return value.Substring(0, System.Math.Max(0, max - 3)) + "...";
            }
        }

        #endregion

        #region Updater

        static void UpdateServers()
        {
#if MRP
            ServerID = 1;
            ServerName = "Medium RP";
#elif NR
            switch (Server.Port)
            {
                case 7779:
                    {
                        ServerID = 2;
                        ServerName = "NoRules";
                        DonateID = 1;
                        break;
                    }
                case 6666:
                    {
                        ServerID = 3;
                        ServerName = "Friendly NoRules";
                        DonateID = 1;
                        break;
                    }
                case 7888:
                    {
                        ServerID = 4;
                        ServerName = "AdmZone";
                        BlockStats = true;
                        DonateID = 1;
                        break;
                    }
            }
#endif
        }

        static IEnumerator<float> UpdateVerkey()
        {
            string token = ConfigsCore.SafeGetValue("verkey", "default");

            if (token == "default")
                yield break;

            while (true)
            {
                ServerConsole.Password = token;
                yield return Timing.WaitForSeconds(2f);
            }
        }

        #endregion
    }
}
