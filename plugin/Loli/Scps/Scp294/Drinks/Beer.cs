using CustomPlayerEffects;
using Loli.Scps.Scp294.API.Interfaces;
using Qurre.API;
using Qurre.API.Controllers;

namespace Loli.Scps.Scp294.Drinks
{
    public sealed class Beer : IDrink
    {
        public string Name { get; } = "Пиво";

        public string Description { get; } = "";

        public bool OnStartDrinking(Player _)
        {
            return true;
        }

        public void OnDrank(Player pl)
        {
            if (pl.Effects.Controller.TryGetEffect(out Bleeding blend))
            {
                blend.minDamage = 0;
                blend.maxDamage = 3;
            }
            pl.Effects.Enable<Bleeding>(5);
            pl.Client.ShowHint("Сейчас бы рыбки...", 5);
        }
    }
}