using Loli.Scps.Scp294.API.Interfaces;
using Qurre.API;
using Qurre.API.Controllers;

namespace Loli.Scps.Scp294.Drinks
{
    public sealed class Carbon : IDrink
    {
        public string Name { get; } = "Углерод";

        public string Description { get; } = "Жидкий углерод";

        public bool OnStartDrinking(Player _)
        {
            return true;
        }

        public void OnDrank(Player pl)
        {
            pl.HealthInformation.Kill("Ожоги третьей степени");
        }
    }
}