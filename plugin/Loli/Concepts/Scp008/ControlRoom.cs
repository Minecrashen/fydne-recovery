using Interactables.Interobjects.DoorUtils;
using Loli.Controllers;
using Loli.DataBase;
using Loli.Modules.Voices;
using MEC;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Addons.Audio;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.API.Objects;
using Qurre.Events;
using Qurre.Events.Structs;
using SchematicUnity.API.Objects;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MapGeneration;
using Qurre.API.World;
using UnityEngine;

namespace Loli.Concepts.Scp008
{
    internal class ControlRoom
    {
        const string CoroutineName = "SCP008_ControlRoom_Coroutine";

        static string DirectoryPath { get; } = Path.Combine(Path.Combine(Paths.Plugins, "Audio"), "Scp008");
        static string AudioPath { get; } = Path.Combine(DirectoryPath, "Scp008.raw");
        static string AudioPathCancel { get; } = Path.Combine(DirectoryPath, "Scp008Cancel.raw");

        static bool _first = true;
        static int _damageType = 0;

        internal readonly Dictionary<TubeRoomType, Primitive> StatusMonitors = new();

        protected readonly Door _door;
        protected readonly Scheme _scheme;

        static AudioPlayerBot _audioPlayer = null;

        static internal bool Activated { get; private set; } = false;
        static internal bool Cancelled { get; private set; } = false;
        static internal bool AllowCancel { get; private set; } = true;

        internal ControlRoom()
        {
            try { _audioPlayer?.DestroySelf(); } catch { }
            _audioPlayer = null;
            _damageType = 0;

            Activated = false;
            Cancelled = false;
            AllowCancel = true;

            var room = Map.Rooms.Find(x => x.Type == RoomType.HczTest);
            _scheme = SchematicUnity.API.SchematicManager.LoadSchematic(
                Path.Combine(Paths.Plugins, "Schemes", "Scp008", "ControlRoom.json"),
                room.Position,
                room.Rotation);

            GameObject go = new();
            go.transform.parent = room.Transform;
            go.transform.localPosition = new(0.243f, 0.641f, -2.22f);
            go.transform.localRotation = Quaternion.Euler(new(90, 0, 90));

            _door = new Door(go.transform.position, DoorPrefabs.DoorHCZ, go.transform.rotation) { Scale = new(0.227f, 0.177f, 0.086f) };
            Object.Destroy(go);

            foreach (var _obj in _scheme.Objects)
                findObjects(_obj);
            void findObjects(SObject obj)
            {
                switch (obj.Name)
                {
                    case "Monitor1":
                        {
                            FindAndAdd(TubeRoomType.Lcz173);
                            break;
                        }
                    case "Monitor2":
                        {
                            FindAndAdd(TubeRoomType.Hcz049);
                            break;
                        }
                    case "Monitor3":
                        {
                            FindAndAdd(TubeRoomType.Hcz939);
                            break;
                        }
                    case "Monitor4":
                        {
                            FindAndAdd(TubeRoomType.EzVent);
                            break;
                        }
                }
                foreach (var _obj in obj.Childrens)
                    findObjects(_obj);

                void FindAndAdd(TubeRoomType type)
                {
                    if (obj.Childrens.TryFind(out SObject obj2, x => x.Name == "Display" && x.Primitive is not null))
                    {
                        if (StatusMonitors.ContainsKey(type))
                            StatusMonitors.Remove(type);

                        StatusMonitors.Add(type, ((PrimitiveParams)obj2.Primitive).Base);
                    }
                }
            }

            try { RoomsData.Control.Destroy(); } catch { }
            RoomsData.Control = this;

            GameObject go2 = new();
            go2.transform.parent = room.Transform;
            go2.transform.localPosition = new(0, -0.474f, 1.339f);
            go2.transform.localRotation = Quaternion.identity;
            new Door(go2.transform.position, DoorPrefabs.DoorHCZ, go2.transform.rotation);
            Object.Destroy(go2);

            if (_first)
            {
                _first = false;

                if (!Directory.Exists(DirectoryPath))
                    Directory.CreateDirectory(DirectoryPath);

                (Core.CDNUrl + "/qurre/audio/Scp008.raw").DownloadAudio(AudioPath);
                (Core.CDNUrl + "/qurre/audio/Scp008Cancel.raw").DownloadAudio(AudioPathCancel);
            }
        }

        internal void Destroy()
        {
            _door.Destroy();
            _scheme.Unload();
            Timing.KillCoroutines(CoroutineName);
        }


        [EventMethod(RoundEvents.Restart, int.MinValue)]
        static internal void RestartDestroy()
        {
            try { RoomsData.Control.Destroy(); } catch { }
            RoomsData.Control = null;
        }

        [EventMethod(MapEvents.OpenDoor, int.MinValue)]
        [EventMethod(MapEvents.LockDoor, int.MinValue)]
        [EventMethod(MapEvents.DamageDoor, int.MinValue)]
        static internal void LockInteract(IBaseEvent @base)
        {
            if (RoomsData.Control is null)
                return;

            switch (@base)
            {
                case OpenDoorEvent ev:
                    {
                        if (RoomsData.Control._door == ev.Door)
                            ev.Allowed = false;
                        break;
                    }
                case LockDoorEvent ev:
                    {
                        if (RoomsData.Control._door == ev.Door)
                            ev.Allowed = false;
                        break;
                    }
                case DamageDoorEvent ev:
                    {
                        if (RoomsData.Control._door == ev.Door)
                            ev.Allowed = false;
                        break;
                    }
            }
        }

        static void CheckCancel(Player pl)
        {
            if (!Activated)
                return;

            if (Cancelled)
                return;

            if (!AllowCancel)
                return;

            int activated = 0;
            foreach (var room in TubeRoom.DoorToRoom)
            {
                if (room.Value.Type == TubeRoomType.Lcz173 && Decontamination.InProgress)
                    activated++;
                else if (!room.Value.Activated)
                    activated++;
            }

            if (activated < 4)
            {
                string status = $"{activated}/4";
                if (Decontamination.InProgress)
                    status = $"{activated - 1}/3";
                pl.Client.ShowHint($"<b><color=red>Не все вентили SCP-008 были деактивированы ({status})</color></b>", 10);
                return;
            }

            GlobalLights.TurnOff(5);
            Timing.CallDelayed(2f, () =>
            {
                GlobalLights.SetToDefault(true, true);
                DoorEventOpenerExtension.TriggerAction(DoorEventOpenerExtension.OpenerEventType.WarheadCancel);
            });

            AlphaController.DisableLock();
            AlphaController.ChangeState(false);
            ConceptsController.Disable();

            _audioPlayer?.DestroySelf();
            _audioPlayer = VoiceCore.PlayInIntercom(AudioPathCancel, "SCP-008");
            Cassie.Lock = false;

            Cancelled = true;
            Activated = false;
            HintsUi.UpdateProgress();

            RoomsData.Control._door.Open = false;
        }

        [EventMethod(PlayerEvents.InteractDoor, int.MinValue)]
        static internal void Interact(InteractDoorEvent ev)
        {
            if (RoomsData.Control._door != ev.Door)
                return;

            ev.Allowed = false;

            if (ev.Player.RoleInformation.Team == Team.SCPs)
                return;

            if (Cancelled)
            {
                ev.Player.Client.ShowHint("<b><color=red>Процедура выброса SCP-008 была прервана</color></b>", 10);
                return;
            }

            if (!ev.Player.Tag.Contains(SerpentsHand.Tag))
            {
                if (Activated)
                {
                    CheckCancel(ev.Player);
                    return;
                }
                ev.Player.Client.ShowHint("<b><color=red>Только Длань Змея может активировать SCP-008</color></b>", 10);
                return;
            }

            if (Activated)
                return;

            if (ConceptsController.IsActivated)
                return;

            int activated = 0;
            foreach (var room in TubeRoom.DoorToRoom)
            {
                if (room.Value.Type == TubeRoomType.Lcz173 && Decontamination.InProgress)
                    activated++;
                else if (room.Value.Activated)
                    activated++;
            }

            if (activated < 4)
            {
                ev.Player.Client.ShowHint($"<b><color=red>Не все вентили SCP-008 были активированы ({activated}/4)</color></b>" +
                    (Decontamination.InProgress ? "\n<b><color=yellow>В Легкой Зоне Содержания вентиль уже активирован</color></b>" : ""), 10);
                return;
            }

            ev.Player.Client.ShowHint("<b><color=green>SCP-008 активирован</color></b>", 10);
            ev.Player.AddStats(500, 50, "активация SCP-008 за Длань Змея");

            Activate();
            Activated = true;
            HintsUi.UpdateProgress();

            ev.Door.Open = true;
        }

        static void Activate()
        {
            int roundId = Round.CurrentRound;
            _damageType = 0;

            Timing.RunCoroutine(Process(), CoroutineName);
            Timing.RunCoroutine(KillEveryone(), CoroutineName);

            IEnumerator<float> Process()
            {
                GlobalLights.TurnOff(5);

                try { Alpha.Stop(); } catch { }
                AlphaController.ChangeState(true, true);
                ConceptsController.Activate();

                _audioPlayer?.DestroySelf();
                _audioPlayer = VoiceCore.PlayInIntercom(AudioPath, "SCP-008");
                Cassie.Lock = true;

                yield return Timing.WaitForSeconds(2f);

                if (Cancelled || Round.CurrentRound != roundId)
                    yield break;

                GlobalLights.ChangeColor(Color.green, true, true, true);
                DoorEventOpenerExtension.TriggerAction(DoorEventOpenerExtension.OpenerEventType.WarheadStart);

                yield return Timing.WaitForSeconds(190f);

                if (Cancelled || Round.CurrentRound != roundId)
                    yield break;

                _damageType = 1;

                foreach (var door in Map.Doors.Where(
                    x => x.Type is DoorType.ElevatorLczChkpA or DoorType.ElevatorLczChkpB
                    or DoorType.ElevatorHczChkpA or DoorType.ElevatorHczChkpB))
                {
                    door.Open = false;
                    door.Lock = true;
                }

                yield return Timing.WaitForSeconds(55f);

                if (Cancelled || Round.CurrentRound != roundId)
                    yield break;

                AllowCancel = false;
                _damageType = 2;

                foreach (var door in Map.Doors.Where(
                    x => x.Type is DoorType.EzCheckpointA or DoorType.EzCheckpointB
                    or DoorType.EzCheckpointArmoryA or DoorType.EzCheckpointArmoryB))
                {
                    door.Open = false;
                    door.Lock = true;
                }

                yield return Timing.WaitForSeconds(40f);

                if (Cancelled || Round.CurrentRound != roundId)
                    yield break;

                Builds.Models.Lift.Locked = true;
                foreach (var door in Map.Doors.Where(
                    x => x.Type is DoorType.ElevatorGateA or DoorType.ElevatorGateB))
                {
                    door.Open = false;
                    door.Lock = true;
                }

                yield return Timing.WaitForSeconds(30f);

                if (Cancelled || Round.CurrentRound != roundId)
                    yield break;

                _damageType = 3;

                yield return Timing.WaitForSeconds(30f);

                if (Cancelled || Round.CurrentRound != roundId)
                    yield break;

                _damageType = 4;

                yield return Timing.WaitForSeconds(50f);

                if (Cancelled || Round.CurrentRound != roundId)
                    yield break;

                Round.End();

                yield break;
            }

            IEnumerator<float> KillEveryone()
            {
                while (Round.CurrentRound == roundId)
                {
                    try
                    {
                        foreach (var pl in Player.List)
                        {

                            try
                            {
                                if (_damageType == 0)
                                    continue;

                                if (_damageType == 1 &&
                                pl.GamePlay.CurrentZone is not FacilityZone.LightContainment)
                                    continue;

                                if (_damageType == 2 &&
                                pl.GamePlay.CurrentZone is not FacilityZone.LightContainment
                                and not FacilityZone.HeavyContainment)
                                    continue;

                                if (_damageType == 3 &&
                                pl.GamePlay.CurrentZone is not FacilityZone.LightContainment
                                and not FacilityZone.HeavyContainment and not FacilityZone.Entrance)
                                    continue;

                                if (pl.RoleInformation.IsScp)
                                    continue;

                                if (7 >= pl.HealthInformation.Hp)
                                {
                                    Timing.CallDelayed(1f, () => pl.RoleInformation.SetNew(RoleTypeId.Scp0492, RoleChangeReason.Respawn));
                                }
                                pl.HealthInformation.Damage(7, "Заражение SCP-008");
                            }
                            catch { }
                        }
                    }
                    catch { }
                    yield return Timing.WaitForSeconds(1);
                }
                yield break;
            }
        }
    }
}