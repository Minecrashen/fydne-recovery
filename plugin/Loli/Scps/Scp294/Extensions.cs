using InventorySystem.Items;
using InventorySystem.Items.Usables;
using Loli.Scps.Scp294.API;
using Loli.Scps.Scp294.API.Interfaces;

namespace Loli.Scps.Scp294
{
    internal static class Extensions
    {
        internal static bool TryGetDrink(this ItemIdentifier itemIdentifier, out IDrink drink)
        {
            return DrinksManager.Drinks.TryGetValue(itemIdentifier.SerialNumber, out drink);
        }

        internal static bool TryGetDrink(this ItemBase item, out IDrink drink)
        {
            if (item != null)
            {
                return DrinksManager.Drinks.TryGetValue(item.ItemSerial, out drink);
            }

            drink = null;
            return false;
        }

        internal static bool TryGetDrink(this Scp207 scp207, out IDrink drink)
        {
            if (scp207 != null)
            {
                return DrinksManager.Drinks.TryGetValue(scp207.ItemSerial, out drink);
            }

            drink = null;
            return false;
        }
    }
}