using Loli.Scps.Scp294.API.Interfaces;
using Qurre.API;
using Qurre.API.Controllers;
using Qurre.API.Objects;

namespace Loli.Scps.Scp294.Drinks
{
    public sealed class Rage : IDrink
    {
        public string Name { get; } = "Ярость";

        public string Description { get; } = "Жидкая ярость";

        public bool OnStartDrinking(Player _)
        {
            return true;
        }

        public void OnDrank(Player pl)
        {
            pl.Effects.Enable(EffectType.Invigorated, 30);
            pl.HealthInformation.Ahp = 150;
            int _hp = (int)pl.HealthInformation.Hp;
            pl.HealthInformation.Hp = _hp > 50 ? _hp - 50 : 1;
        }
    }
}