using Loli.Scps.Scp294.API.Interfaces;
using Qurre.API;
using Qurre.API.Controllers;

namespace Loli.Scps.Scp294.Drinks
{
    public sealed class Gold : IDrink
    {
        public string Name { get; } = "Золото";

        public string Description { get; } = "Жидкое золото";

        public bool OnStartDrinking(Player _)
        {
            return true;
        }

        public void OnDrank(Player pl)
        {
            pl.Client.ShowHint("Что это?", 5);
        }
    }
}