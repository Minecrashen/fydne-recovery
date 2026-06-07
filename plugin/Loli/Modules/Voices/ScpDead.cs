using PlayerRoles;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Objects;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Collections.Generic;
using System.IO;

namespace Loli.Modules.Voices
{
    static class ScpDead
    {
        static string DirectoryPath { get; } = Path.Combine(Path.Combine(Paths.Plugins, "Audio"), "ScpDead");

        static internal string OpenPlay { get; } = Path.Combine(DirectoryPath, "_open.raw");
        static internal string ClosePlay { get; } = Path.Combine(DirectoryPath, "_close.raw");

        static internal string ByMtf { get; } = Path.Combine(DirectoryPath, "by_mtf.raw");
        static internal string ByUnknow { get; } = Path.Combine(DirectoryPath, "by_unknow.raw");

        static internal string Scp049 { get; } = Path.Combine(DirectoryPath, "scp049.raw");
        static internal string Scp096 { get; } = Path.Combine(DirectoryPath, "scp096.raw");
        static internal string Scp106 { get; } = Path.Combine(DirectoryPath, "scp106.raw");
        static internal string Scp173 { get; } = Path.Combine(DirectoryPath, "scp173.raw");
        static internal string Scp939 { get; } = Path.Combine(DirectoryPath, "scp939.raw");
        static internal string Scp3114 { get; } = Path.Combine(DirectoryPath, "scp3114.raw");


        [EventMethod(PlayerEvents.Dies, int.MinValue)]
        static void Dead(DiesEvent ev)
        {
            if (!ev.Allowed)
                return;

            if (ev.Target.RoleInformation.Team is not Team.SCPs)
                return;

#if MRP
            if (ev.LiteType is LiteDamageTypes.Warhead or LiteDamageTypes.ScpDamage or LiteDamageTypes.Custom)
                return;

            if (ev.Target.Disconnected)
                return;
#elif NR
            if (ev.LiteType is LiteDamageTypes.Warhead or LiteDamageTypes.ScpDamage or LiteDamageTypes.Recontainment)
                return;

            if (ev.Attacker == ev.Target)
                return;

            if (ev.Attacker == Server.Host)
                return;

            if (ev.Target.HealthInformation.MaxHp - ev.Target.HealthInformation.Hp < 100)
                return;
#endif

            string scpfile = ev.Target.RoleInformation.Role switch
            {
                RoleTypeId.Scp049 => Scp049,
                RoleTypeId.Scp096 => Scp096,
                RoleTypeId.Scp106 => Scp106,
                RoleTypeId.Scp173 => Scp173,
                RoleTypeId.Scp939 => Scp939,
                RoleTypeId.Scp3114 => Scp3114,
                _ => string.Empty,
            };

            if (string.IsNullOrEmpty(scpfile))
                return;

            string byfile = (ev.Attacker?.RoleInformation.Team is Team.FoundationForces) ? ByMtf : ByUnknow;

            List<string> pathes = new()
            {
                OpenPlay,
                scpfile,
                byfile,
                ClosePlay,
            };

            VoiceCore.PlayAudio(pathes);
        }
    }
}