using CustomPlayerEffects;
using Loli.Scps.Scp294.API.Interfaces;
using Qurre.API;
using Qurre.API.Controllers;
using Qurre.API.Objects;

namespace Loli.Scps.Scp294.Drinks
{
    public sealed class Vodka : IDrink
    {
        public string Name { get; } = "Водка";

        public string Description { get; } = "";

        public bool OnStartDrinking(Player _)
        {
            return true;
        }

        public void OnDrank(Player pl)
        {
            pl.Client.ShowHint("Кааааак х-хорош-шо", 5);
            if (pl.Effects.Controller.TryGetEffect(out Bleeding effect))
            {
                effect.minDamage = 0;
                effect.maxDamage = 3;
            }
            pl.Effects.Enable<Bleeding>(10);
            pl.Effects.Enable(EffectType.Burned, 30);
            pl.Effects.Enable(EffectType.Invigorated, 30);
            if (pl.Effects.TryGet(EffectType.MovementBoost, out StatusEffectBase playerEffect))
            {
                playerEffect.Intensity = 7;
                pl.Effects.Enable(playerEffect, 10);
            }
        }
    }
}