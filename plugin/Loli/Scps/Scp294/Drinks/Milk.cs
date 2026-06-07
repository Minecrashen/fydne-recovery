using Loli.Scps.Scp294.API.Interfaces;
using Qurre.API;
using Qurre.API.Controllers;

namespace Loli.Scps.Scp294.Drinks
{
    public sealed class Milk : IDrink
    {
        public string Name { get; } = "Молоко";

        public string Description { get; } = "Обычное молоко";

        public bool OnStartDrinking(Player _)
        {
            return true;
        }

        public void OnDrank(Player pl)
        {
            pl.Client.ShowHint("Тааак ~ освежает", 5);
            pl.HealthInformation.Heal(20, false);
        }
    }
}