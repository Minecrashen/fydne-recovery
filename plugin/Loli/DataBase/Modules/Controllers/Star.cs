#if NR
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using System.Collections.Generic;
using System.Linq;
using Qurre.API.Controllers;
using UnityEngine;

namespace Loli.DataBase.Modules.Controllers
{
    internal class Star
    {
        internal static readonly List<Star> List = new();
        private readonly Glow Glow;
        internal Player pl;
        internal Star(Player pl)
        {
            List.Add(this);
            this.pl = pl;
            var color = new Color32(247, 0, 255, 255);
            Glow = new Glow(pl, color);
        }
        internal void Break()
        {
            try { List.Remove(this); } catch { }
            try { Glow.Destroy(); } catch { }
        }
        internal static Star Get(Player pl)
        {
            var _list = List.Where(x => x.pl == pl);
            if (_list.Count() > 0) return _list.First();
            return null;
        }

        [EventMethod(RoundEvents.Waiting)]
        static void Waiting() => List.Clear();
    }
}
#endif