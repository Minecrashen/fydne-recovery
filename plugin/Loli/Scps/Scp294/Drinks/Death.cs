using Loli.Scps.Scp294.API.Interfaces;
using Qurre.API;
using Qurre.API.Controllers;

namespace Loli.Scps.Scp294.Drinks
{
    public sealed class Death : IDrink
    {
        public string Name { get; } = "Смерть";

        public string Description { get; } = "Жидкая смерть";

        public bool OnStartDrinking(Player _)
        {
            return true;
        }

        public void OnDrank(Player pl)
        {
            pl.HealthInformation.Kill("Что?");
        }
    }
}