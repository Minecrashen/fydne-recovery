#if MRP
using Loli.Modules.Voices;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.API.Objects;
using Qurre.Events;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Qurre.API.World;
using UnityEngine;

namespace Loli.Addons.RolePlay
{
    static class Cutscene
    {
        static string DirectoryPath { get; } = Path.Combine(Path.Combine(Paths.Plugins, "Audio"), "RoundStart");
        static internal string CutSceneFile { get; } = Path.Combine(DirectoryPath, "cutscene.raw");

        [EventMethod(RoundEvents.Start, -1)]
        static void Init()
        {
            int round = Round.CurrentRound;
            var door = DoorType.Lcz173Connector.GetDoor();
            var rot = door.Rotation.eulerAngles;

            if (rot.y.Difference(0) < 1)
            {
                DoSpawn(new(door.Position.x - 10, door.Position.y + 1, door.Position.z + 4), rot, new(0, 0, 0.5f));
                DoSpawn(new(door.Position.x - 12.6875f, door.Position.y + 4, door.Position.z + 7.6f), new(rot.x, rot.y + 60, rot.z));
            }
            else if (rot.y.Difference(90) < 1)
            {
                DoSpawn(new(door.Position.x + 4, door.Position.y + 1, door.Position.z + 10), rot, new(0.5f, 0, 0));
                DoSpawn(new(door.Position.x + 7.6f, door.Position.y + 4, door.Position.z + 12.6875f), new(rot.x, rot.y + 60, rot.z));
            }
            else if (rot.y.Difference(180) < 1)
            {
                DoSpawn(new(door.Position.x + 10, door.Position.y + 1, door.Position.z - 4), rot, new Vector3(0, 0, -0.5f));
                DoSpawn(new(door.Position.x + 12.6875f, door.Position.y + 4, door.Position.z - 7.6f), new Vector3(rot.x, rot.y + 60, rot.z));
            }
            else if (rot.y.Difference(270) < 1)
            {
                DoSpawn(new(door.Position.x - 4, door.Position.y + 1, door.Position.z - 10), rot, new(-0.5f, 0, 0));
                DoSpawn(new(door.Position.x - 7.6f, door.Position.y + 4, door.Position.z - 12.6875f), new(rot.x, 0, rot.z));
            }

            void DoSpawn(Vector3 pos, Vector3 rot, Vector3 cof = default)
            {
                new Corpse(RoleTypeId.FacilityGuard, pos + Vector3.up, Quaternion.Euler(rot),
                    new UniversalDamageHandler(-1, DeathTranslations.Scp173), "Facility Guard");
            }

            try
            {
                foreach (Door _door in Map.Doors.Where(x => x.Type is DoorType.LczPrison))
                {
                    _door.Lock = true;
                    _door.Open = false;
                }
            }
            catch { }

            Timing.RunCoroutine(Coroutine());
            IEnumerator<float> Coroutine()
            {
                VoiceCore.PlayInIntercom(CutSceneFile, "C.A.S.S.I.E.");

                yield return Timing.WaitForSeconds(15f);
                if (round != Round.CurrentRound)
                    yield break;

                GlobalLights.TurnOff(17f);

                yield return Timing.WaitForSeconds(5f);
                if (round != Round.CurrentRound)
                    yield break;

                var room = RoomType.LczClassDSpawn.GetRoom();
                new Corpse(RoleTypeId.FacilityGuard, room.Position + Vector3.up, Quaternion.identity,
                    new UniversalDamageHandler(-1, DeathTranslations.Scp173), "Facility Guard");
                //Scp173.PlaceTantrum(room.Position + Vector3.up);

                yield return Timing.WaitForSeconds(5f);
                if (round != Round.CurrentRound)
                    yield break;

                try
                {
                    foreach (Door door in Map.Doors.Where(x => x.Type is DoorType.LczPrison))
                    {
                        door.Lock = true;
                        door.Open = true;
                    }
                }
                catch { }

                yield return Timing.WaitForSeconds(5f);
                if (round != Round.CurrentRound)
                    yield break;

                GlobalLights.ChangeColor(Color.red, false);

                yield return Timing.WaitForSeconds(50f);
                if (round != Round.CurrentRound)
                    yield break;

                GlobalLights.TurnOff(2f);

                yield return Timing.WaitForSeconds(1f);
                if (round != Round.CurrentRound)
                    yield break;

                foreach (var room2 in Map.Rooms)
                    room2.Lights.Override = false;

                while (round == Round.CurrentRound)
                {
                    UpdateLight(RoomType.LczClassDSpawn.GetRoom().Lights);
                    UpdateLight(RoomType.Lcz173.GetRoom().Lights);
                    UpdateLight(RoomType.Hcz106.GetRoom().Lights);
                    yield return Timing.WaitForSeconds(60f);
                }

            }

            void UpdateLight(Lights lg)
            {
                lg.Color = Color.red;
                lg.LockChange = true;
            }
        }
    }
}
#endif