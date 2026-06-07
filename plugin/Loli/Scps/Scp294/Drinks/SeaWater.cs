using Loli.Scps.Scp294.API.Interfaces;
using Qurre.API;
using Qurre.API.Controllers;

namespace Loli.Scps.Scp294.Drinks
{
    public sealed class SeaWater : IDrink
    {
        public string Name { get; } = "Морская вода";

        public string Description { get; } = "Просто морская вода";

        public bool OnStartDrinking(Player _)
        {
            return true;
        }

        public void OnDrank(Player pl)
        {
            pl.Client.ShowHint("Как же захотелось пить..", 5);
            pl.HealthInformation.Stamina = 0;
        }
    }
}