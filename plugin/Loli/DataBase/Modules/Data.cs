using System.Collections.Generic;
using Qurre.API.Attributes;
using Qurre.Events;

#if NR
using System;
#endif

namespace Loli.DataBase.Modules
{
    static class Data
    {
        internal static readonly Dictionary<string, Clan> Clans = new Dictionary<string, Clan>();
        internal static readonly CustomDictionary<string, UserData> Users = new CustomDictionary<string, UserData>();
        internal static readonly Dictionary<string, DonateRoles> Roles = new Dictionary<string, DonateRoles>();

#if NR
        internal static readonly Dictionary<string, DonateRA> Donates = new Dictionary<string, DonateRA>();
        internal static readonly Dictionary<string, int> force = new Dictionary<string, int>();
        internal static readonly Dictionary<string, int> giveway = new Dictionary<string, int>();
        internal static readonly Dictionary<string, DateTime> effect = new Dictionary<string, DateTime>();
        internal static readonly Dictionary<string, DateTime> gives = new Dictionary<string, DateTime>();
        internal static readonly Dictionary<string, DateTime> forces = new Dictionary<string, DateTime>();
        internal static readonly Dictionary<string, bool> giver = new Dictionary<string, bool>();
        internal static readonly Dictionary<string, bool> forcer = new Dictionary<string, bool>();
        internal static readonly Dictionary<string, bool> effecter = new Dictionary<string, bool>();
        internal static readonly Dictionary<string, bool> scp_play = new Dictionary<string, bool>();
#endif

        [EventMethod(RoundEvents.Waiting)]
        static void Waiting()
        {
			Users.Clear();
            Roles.Clear();

#if NR
            Donates.Clear();
            giveway.Clear();
            force.Clear();
            effect.Clear();
            gives.Clear();
            giver.Clear();
            forces.Clear();
            forcer.Clear();
            effecter.Clear();
            scp_play.Clear();
            try { Module.Prefixs.Clear(); } catch { }
#endif

            Loader.LoadClans();
        }

#if NR
        [EventMethod(RoundEvents.End)]
        static void RoundEnd()
        {
            giveway.Clear();
            force.Clear();
            effect.Clear();
            gives.Clear();
            forces.Clear();
        }
#endif
    }
}