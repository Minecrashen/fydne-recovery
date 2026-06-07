using Loli.Scps.Scp294.API.Interfaces;
using PlayerStatsSystem;
using Qurre.API;
using Qurre.API.Controllers;

namespace Loli.Scps.Scp294.Drinks
{
    public sealed class Perfume : IDrink
    {
        public string Name { get; } = "Яд";

        public string Description { get; } = "Самый настоящий";

        public bool OnStartDrinking(Player _)
        {
            return true;
        }

        public void OnDrank(Player pl)
        {
            pl.HealthInformation.Kill(DeathTranslations.Poisoned);
        }
    }
}