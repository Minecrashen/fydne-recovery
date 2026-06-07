using CustomPlayerEffects;
using Loli.Scps.Scp294.API.Interfaces;
using Qurre.API;
using Qurre.API.Controllers;
using Qurre.API.Objects;

namespace Loli.Scps.Scp294.Drinks
{
    public sealed class Juice : IDrink
    {
        public string Name { get; } = "Сок";

        public string Description { get; } = "Фруктовый сок";

        public bool OnStartDrinking(Player _)
        {
            return true;
        }

        public void OnDrank(Player pl)
        {
            pl.Client.ShowHint("оооо, такооой ~ вкусный", 5);
            pl.HealthInformation.Stamina = 100;
            if (pl.Effects.TryGet(EffectType.MovementBoost, out StatusEffectBase playerEffect))
            {
                playerEffect.Intensity = 7;
                pl.Effects.Enable(playerEffect, 10);
            }
        }
    }
}