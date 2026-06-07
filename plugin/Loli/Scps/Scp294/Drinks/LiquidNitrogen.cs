using Loli.Scps.Scp294.API.Interfaces;
using Qurre.API;
using Qurre.API.Controllers;

namespace Loli.Scps.Scp294.Drinks
{
    public sealed class LiquidNitrogen : IDrink
    {
        public string Name { get; } = "Жидкий азот";

        public string Description { get; } = "";

        public bool OnStartDrinking(Player _)
        {
            return true;
        }

        public void OnDrank(Player pl)
        {
            pl.Client.ShowHint("З-зачем я э-эт-то с-сделал", 5);
            pl.Effects.SetFogType(CustomRendering.FogType.Scp244);
        }
    }
}