using Loli.Scps.Scp294.API.Interfaces;
using Qurre.API;
using Qurre.API.Controllers;

namespace Loli.Scps.Scp294.Drinks
{
    public sealed class Life : IDrink
    {
        public string Name { get; } = "Жизнь";

        public string Description { get; } = "Жидкая жизнь";

        public bool OnStartDrinking(Player _)
        {
            return true;
        }

        public void OnDrank(Player pl)
        {
            pl.HealthInformation.Hp = pl.HealthInformation.MaxHp;
        }
    }
}