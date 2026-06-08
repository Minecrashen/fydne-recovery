// Qurre.API.Server → LabApi.Features.Wrappers.Server
using Qurre.API.Controllers;
using Lab = LabApi.Features.Wrappers;

namespace Qurre.API
{
    public static class Server
    {
        public static ushort Port => Lab.Server.Port;
        public static string Ip => Lab.Server.IpAddress;
        public static bool FriendlyFire
        {
            get => Lab.Server.FriendlyFire;
            set => Lab.Server.FriendlyFire = value;
        }
        public static int MaxPlayers => Lab.Server.MaxPlayers;
        public static int PlayersCount => Lab.Server.PlayerCount;
        public static double Tps => Lab.Server.Tps;

        // Qurre Server.Host — игрок-хост; оборачиваем LabApi host'а.
        public static Player Host => Player.Get(Lab.Server.Host);

        public static void Restart() => Lab.Server.Restart();
        public static void Shutdown() => Lab.Server.Shutdown();
    }
}
