using Loli.Scps.Scp294.API.Interfaces;
using MEC;
using Qurre.API;
using Qurre.API.Controllers;

namespace Loli.Scps.Scp294.Drinks
{
    public sealed class Iron : IDrink
    {
        public string Name { get; } = "Железо";

        public string Description { get; } = "Жидкое железо";

        public bool OnStartDrinking(Player _)
        {
            return true;
        }

        public void OnDrank(Player pl)
        {
            pl.Client.ShowHint("Ура, железо ^^", 5);
            Timing.CallDelayed(10f, () =>
            {
                pl.Client.ShowHint("ай", 5);
                pl.HealthInformation.Damage(25, "Окисление желудка");
            });
        }
    }
}