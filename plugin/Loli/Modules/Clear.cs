using MEC;
using Qurre.API.Attributes;
using Qurre.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using Qurre.API.World;
using Map = Qurre.API.World.Map;

namespace Loli.Modules
{
    static class Clear
    {
        static internal readonly List<ushort> Pickups = new();

        [EventMethod(RoundEvents.Start)]
        static void StartRefresh()
        {
            Timing.RunCoroutine(ClearWarhead(), "ClearWarheadThread");
            Timing.RunCoroutine(ClearManyItems(), "ClearManyItemsThread");
            Pickups.Clear();
            Timing.CallDelayed(5f, () => { foreach (var pick in Pickup.List) if (!Pickups.Contains(pick.Serial)) Pickups.Add(pick.Serial); });
        }

        [EventMethod(RoundEvents.End)]
        static void ClearEndRefresh()
        {
            Timing.KillCoroutines("ClearWarheadThread");
            Timing.KillCoroutines("ClearManyItemsThread");
            Pickups.Clear();
        }

        static IEnumerator<float> ClearWarhead()
        {
            for (; ; )
            {
                yield return Timing.WaitForSeconds(10);

                if (Alpha.Detonated)
                    Timing.RunCoroutine(_clear());
            }

            static IEnumerator<float> _clear()
            {
                var items = Pickup.List.Where(x => x.Position.y.Difference(980) > 50);
                while (items.Any())
                {
                    var item = items.First();
                    items = items.Skip(1);
                    try { item.Destroy(); } catch { }
                    yield return Timing.WaitForSeconds(0.01f);
                }
                items = null;
                var dolls = Map.Corpses.Where(x => x.Position.y.Difference(980) > 50);
                while (dolls.Any())
                {
                    var doll = dolls.First();
                    dolls = dolls.Skip(1);
                    try { doll.Destroy(); } catch { }
                    yield return Timing.WaitForSeconds(0.01f);
                }
                dolls = null;
                yield break;
            }
        }

        static DateTime ClearManyItemsLast = DateTime.Now;
        static IEnumerator<float> ClearManyItems()
        {
            for (; ; )
            {
                yield return Timing.WaitForSeconds(15);
                var picks = Pickup.List.Where(x => !Pickups.Contains(x.Serial));
                bool b1 = picks.Count() > 300;
                bool b2 = Map.Corpses.Count > 100;
                if (b1)
                {
                    picks = picks.OrderBy(x => x.Serial).Take(200);
                    Timing.RunCoroutine(ClearItems(picks));
                }
                if (b2) Timing.RunCoroutine(ClearDolls());
                if ((b1 || b2) && (DateTime.Now - ClearManyItemsLast).TotalSeconds > 60)
                {
                    ClearManyItemsLast = DateTime.Now;
                    string t = "неизвестно чего";
                    if (b1) t = "вещей";
                    else if (b2) t = "всех трупов";
                    if (b1 && b2) t = "вещей, а также трупов";
                    Map.Broadcast($"<size=65%><color=#6f6f6f>Cовет О5 активировал <color=red>молекулярное уничтожение</color> <color=#0089c7>{t}</color>," +
                     "\n<color=#00ff22>ввиду сохранности секретности комплекса</color>.</color></size>", 10);
                }
            }

            static IEnumerator<float> ClearItems(IEnumerable<Pickup> picks)
            {
                foreach (var item in picks)
                {
                    try { item.Destroy(); } catch { }
                    yield return Timing.WaitForSeconds(0.005f);
                }
                picks = null;
                yield break;
            }

            static IEnumerator<float> ClearDolls()
            {
                var dolls = Map.Corpses.ToArray();
                foreach (var doll in dolls)
                {
                    try { doll.Destroy(); } catch { }
                    yield return Timing.WaitForSeconds(0.005f);
                }
                dolls = null;
                yield break;
            }
        }
    }
}