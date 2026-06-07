using Loli.Scps.Scp294.API.Interfaces;
using Qurre.API;
using Qurre.API.Controllers;
using Qurre.API.Objects;

namespace Loli.Scps.Scp294.Drinks
{
    public sealed class Plague : IDrink
    {
        public string Name { get; } = "Чума";

        public string Description { get; } = "";

        public bool OnStartDrinking(Player _)
        {
            return true;
        }

        public void OnDrank(Player pl)
        {
            pl.Effects.Enable(EffectType.Burned);
#if MRP
            pl.Effects.Enable(EffectType.Blindness, 60);
#elif NR
            pl.Effects.Enable(EffectType.Bleeding, 60);
#endif
            pl.Effects.Enable(EffectType.Corroding, 20);
        }
    }
}