using Loli.Scps.Scp294.API.Interfaces;
using Qurre.API;
using Qurre.API.Controllers;
using Qurre.API.Objects;

namespace Loli.Scps.Scp294.Drinks
{
    public sealed class Oil : IDrink
    {
        public string Name { get; } = "Нефть";

        public string Description { get; } = "";

        public bool OnStartDrinking(Player _)
        {
            return true;
        }

        public void OnDrank(Player pl)
        {
            pl.Effects.Enable(EffectType.Corroding);
        }
    }
}