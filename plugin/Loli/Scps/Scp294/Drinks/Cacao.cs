using Loli.Scps.Scp294.API.Interfaces;
using Qurre.API;
using Qurre.API.Controllers;

namespace Loli.Scps.Scp294.Drinks
{
    public sealed class Cacao : IDrink
    {
        public string Name { get; } = "Какао";

        public string Description { get; } = "Горячее...";

        public bool OnStartDrinking(Player _)
        {
            return true;
        }

        public void OnDrank(Player pl)
        {
            if (UnityEngine.Random.Range(0, 100) > 95)
            {
                pl.Client.ShowHint("аааай", 5);
                pl.HealthInformation.Damage(10, "Ожоги в районе полости рта");
                return;
            }
            pl.Client.ShowHint("ммм, вкусно", 5);
        }
    }
}