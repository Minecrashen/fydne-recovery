using Loli.Scps.Scp294.API.Interfaces;
using Qurre.API;
using Qurre.API.Controllers;

namespace Loli.Scps.Scp294.Drinks
{
    public sealed class Pepsi : IDrink
    {
        public string Name { get; } = "Пепси";

        public string Description { get; } = "Обычная газировка";

        public bool OnStartDrinking(Player _)
        {
            return true;
        }

        public void OnDrank(Player pl)
        {
            pl.Client.ShowHint("А что не кола?)", 5);
            pl.HealthInformation.Stamina = 100;
        }
    }
}