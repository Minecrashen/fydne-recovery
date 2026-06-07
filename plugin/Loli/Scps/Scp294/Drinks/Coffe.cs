using Loli.Scps.Scp294.API.Interfaces;
using Qurre.API;
using Qurre.API.Controllers;
using Qurre.API.Objects;

namespace Loli.Scps.Scp294.Drinks
{
    public sealed class Coffe : IDrink
    {
        public string Name { get; } = "Коффе";

        public string Description { get; } = "Свежемолотый";

        public bool OnStartDrinking(Player _)
        {
            return true;
        }

        public void OnDrank(Player pl)
        {
            pl.Effects.Enable(EffectType.Invigorated, 30);
        }
    }
}