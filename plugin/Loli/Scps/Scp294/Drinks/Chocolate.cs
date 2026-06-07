using Loli.Scps.Scp294.API.Interfaces;
using Qurre.API;
using Qurre.API.Controllers;

namespace Loli.Scps.Scp294.Drinks
{
    public sealed class Chocolate : IDrink
    {
        public string Name { get; } = "Шоколад";

        public string Description { get; } = "Горячий шоколад";

        public bool OnStartDrinking(Player _)
        {
            return true;
        }

        public void OnDrank(Player pl)
        {
            pl.Client.ShowHint("ммм, вкусно", 5);
        }
    }
}