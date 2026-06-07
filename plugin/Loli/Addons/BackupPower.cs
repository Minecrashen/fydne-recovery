using Loli.Concepts.Hackers;
using Loli.Controllers;
using Loli.DataBase.Modules;
#if MRP
using Loli.Modules.Voices;
#endif
using MEC;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Collections.Generic;
using System.IO;
using Qurre.API.World;
using UnityEngine;

namespace Loli.Addons
{
    static class BackupPower
    {
        static string AudioPath { get; } = Path.Combine(Path.Combine(Paths.Plugins, "Audio"), "BackupPower.raw");

        static BackupPower()
        {
            (Core.CDNUrl + "/qurre/audio/BackupPower.raw").DownloadAudio(AudioPath);
        }

        static internal bool InProgress = false;
        static internal bool SystemsBreak = false;
        static internal void StartBackup(float dur)
        {
            if (ConceptsController.IsActivated)
                return;

            if (dur < 20)
                dur = 20;

            Timing.RunCoroutine(DoCor(), "HacksSystemsCoroutine");

            IEnumerator<float> DoCor()
            {
                InProgress = true;
                SystemsBreak = true;

                LostSignal(16);
                GlobalLights.TurnOff(16f);

                yield return Timing.WaitForSeconds(1);

                GlobalLights.ChangeColor(Color.black);

                yield return Timing.WaitForSeconds(13);

                GlobalLights.ChangeColor(Color.red);

                yield return Timing.WaitForSeconds(1);

                SystemsBreak = false;

                var str = $"<color=rainbow><b>Внимание всему персоналу</b></color>\n" +
                    $"<size=70%><color=#6f6f6f>Замечена хакерская атака на защитные системы комплекса</color></size>";
                var str2 = "<size=70%><color=#6f6f6f>,\nкомплекс переходит на резервное питание.</color></size>";

                var bc = Map.Broadcast(str.Replace("rainbow", "#ff0000"), 16, true);

#if MRP
                VoiceCore.PlayInIntercom(AudioPath, "C.A.S.S.I.E.");
#elif NR
                AudioExtensions.PlayInIntercom(AudioPath, "C.A.S.S.I.E.");
#endif

                Timing.RunCoroutine(BcChange(bc, str, str2), "HacksSystemsCoroutine");

                yield return Timing.WaitForSeconds(dur - 17);

                GlobalLights.TurnOff(3f);

                yield return Timing.WaitForSeconds(2);

                InProgress = false;
                GlobalLights.SetToDefault();

                yield break;
            }
        }

        static internal void Ra(RemoteAdminCommandEvent ev)
        {
            if (!Data.Users.TryGetValue(ev.Player.UserInformation.UserId, out var _d))
                return;

            if (_d.id != 1)
                return;

            ev.Allowed = false;
            ev.Reply = "Успешно";

            StartBackup(float.Parse(ev.Args[0]));
        }

        [EventMethod(RoundEvents.Waiting)]
        static void Waiting()
        {
            SystemsBreak = false;
            InProgress = false;
            Timing.KillCoroutines("HacksSystemsCoroutine");
        }

        [EventMethod(PlayerEvents.InteractDoor)]
        static void HackActivated(InteractDoorEvent ev)
        {
            if (!SystemsBreak)
                return;

            if (ev.Player.Tag.Contains(Hacker.Tag))
                return;

            ev.Allowed = false;
        }

        [EventMethod(PlayerEvents.InteractLift)]
        static void HackActivated(InteractLiftEvent ev)
        {
            if (!SystemsBreak)
                return;

            if (ev.Player.Tag.Contains(Hacker.Tag))
                return;

            ev.Allowed = false;
        }

        static void LostSignal(float dur)
        {
            foreach (var pl in Player.List)
            {
                try
                {
                    if (pl.RoleInformation.Role is PlayerRoles.RoleTypeId.Scp079)
                        pl.RoleInformation.Scp079.LostSignal(dur);
                }
                catch { }
            }
        }

        static IEnumerator<float> BcChange(MapBroadcast bc, string str, string str2)
        {
            bool red_color = false;
            for (int i = 0; i < 10; i++)
            {
                yield return Timing.WaitForSeconds(1f);
                var color = "#fdffbb";
                if (red_color)
                {
                    color = "#ff0000";
                    red_color = false;
                }
                else red_color = true;
                var msg = str.Replace("rainbow", color);
                if (i > 4) msg += str2;
                bc.Message = msg;
            }
        }
    }
}