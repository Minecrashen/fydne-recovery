using Loli.Scps.Scp294.API.Interfaces;
using Qurre.API;
using Qurre.API.Controllers;

namespace Loli.Scps.Scp294.Drinks
{
    public sealed class AloeVera : IDrink
    {
        public string Name { get; } = "Алоэ вера";

        public string Description { get; } = "Жидая алоэ вера";

        public bool OnStartDrinking(Player _)
        {
            return true;
        }

        public void OnDrank(Player pl)
        {
            pl.HealthInformation.Heal(5, false);
        }
    }
}