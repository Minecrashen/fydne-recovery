using Loli.Scps.Scp294.API.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;
using Random = UnityEngine.Random;
namespace Loli.Scps.Scp294.API
{
    internal static class DrinksManager
    {
        internal static Dictionary<ushort, IDrink> Drinks;

        private static List<IDrink> _drinks;

        internal static void Init()
        {
            Drinks = new Dictionary<ushort, IDrink>();
            _drinks = new();

            foreach (var type in Assembly.GetCallingAssembly().GetTypes())
            {
                try
                {
                    if (type.GetInterface("IDrink") != typeof(IDrink))
                        continue;

                    var drink = Activator.CreateInstance(type) as IDrink;

                    _drinks.Add(drink);
                }
                catch { }
            }
        }

        internal static void Reset()
        {
            Drinks = null;
            _drinks = null;
        }

        internal static bool TryGetRandomDrink(out IDrink drink)
        {
            if (_drinks.Count > 0)
            {
                drink = _drinks[Random.Range(0, _drinks.Count - 1)];
                return true;
            }

            drink = null;
            return false;
        }
    }
}