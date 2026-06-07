using Loli.Scps.Scp294.API.Interfaces;
using Qurre.API;
using Qurre.API.Controllers;

namespace Loli.Scps.Scp294.Drinks
{
    public sealed class LiquidHidrogen : IDrink
    {
        public string Name { get; } = "Водород";

        public string Description { get; } = "Жидкий водород";

        public bool OnStartDrinking(Player _)
        {
            return true;
        }

        public void OnDrank(Player pl)
        {
            pl.HealthInformation.Kill("Повреждение слизистой оболочки");
        }
    }
}