using CustomPlayerEffects;
using Loli.Scps.Scp294.API.Interfaces;
using Qurre.API;
using Qurre.API.Controllers;
using Qurre.API.Objects;

namespace Loli.Scps.Scp294.Drinks
{
    public sealed class Hot : IDrink
    {
        public string Name { get; } = "Жар";

        public string Description { get; } = "Горячее...";

        public bool OnStartDrinking(Player _)
        {
            return true;
        }

        public void OnDrank(Player pl)
        {
            pl.Client.ShowHint("Ай, ай, ай, горячо то как", 5);
            if (pl.Effects.Controller.TryGetEffect(out Bleeding blend))
            {
                blend.minDamage = 0;
                blend.maxDamage = 3;
            }
            pl.Effects.Enable<Bleeding>(10);
            pl.Effects.Enable(EffectType.Burned, 30);
        }
    }
}