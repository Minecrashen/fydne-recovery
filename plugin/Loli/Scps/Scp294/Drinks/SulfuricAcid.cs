using Loli.Scps.Scp294.API.Interfaces;
using Qurre.API;
using Qurre.API.Controllers;

namespace Loli.Scps.Scp294.Drinks
{
    public sealed class SulfuricAcid : IDrink
    {
        public string Name { get; } = "Серная кислота";

        public string Description { get; } = "Кислота она такая";

        public bool OnStartDrinking(Player _)
        {
            return true;
        }

        public void OnDrank(Player pl)
        {
            pl.HealthInformation.Kill("Решил попробовать серную кислоту");
        }
    }
}