using System.Collections.Generic;
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
        static internal readonly QurreSocket.Client Socket = new(2467, SocketIP);
        public static int MaxPlayers = GameCore.ConfigFile.ServerConfig.GetInt("max_players", 100);
        internal const string CDNUrl = "http://cdn.loliscp.ru";

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
        static internal string ApiToken => "uhRZieRHg3ZYfH5VQULb0Do94Vs3RNNp0RxXTJ4Onr";
        static internal string SteamToken => "89A9ED2C55CB1BEEFE27EF4E6A4977C6";
        static internal string SocketIP => "185.236.24.190";
        static internal string APIUrl => "https://api.loliscp.ru";

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

            new Harmony("fydne.loli").PatchAll();
        }

        [PluginDisable]
        static internal void Disable()
            => Server.Restart();

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