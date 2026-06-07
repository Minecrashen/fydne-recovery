using Interactables.Interobjects.DoorUtils;
using Loli.Addons;
using Loli.Controllers;
using Loli.DataBase;
using Loli.Modules.Voices;
using MEC;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Addons.Models;
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

namespace Loli.Concepts.NuclearAttack
{
    static class Builds
    {
        static SObject LightHcz { get; set; }
        static SObject LightLcz { get; set; }

        static List<WorkStation> HczStations { get; } = new();
        static WorkStation LczStation { get; set; }
        static WorkStation RadarStation { get; set; }

        static internal bool ActivatedHcz { get; private set; }
        static internal bool ActivatedLcz { get; private set; }
        static internal bool Activated { get; private set; }

        static string AudioPath { get; } = Path.Combine(Path.Combine(Paths.Plugins, "Audio"), "NuclearAttack.raw");

        static Builds()
        {
            (Core.CDNUrl + "/qurre/audio/NuclearAttack.raw").DownloadAudio(AudioPath);
        }

        [EventMethod(RoundEvents.Waiting)]
        static void Load()
        {
            ActivatedHcz = false;
            ActivatedLcz = false;
            Activated = false;

            {
                var room = Map.Rooms.Find(x => x.Type == RoomType.HczCornerDeep);
                var _scheme = SchematicUnity.API.SchematicManager.LoadSchematic(
                    Path.Combine(Paths.Plugins, "Schemes", "Nuclear", "HCZ.json"),
                    room.Position,
                    room.Rotation);
                foreach (var _obj in _scheme.Objects)
                    findObjects(_obj, 1);

                HczStations.Clear();

                Model model = new("Nuclear-HCZ", room.Position, room.Rotation.eulerAngles);
                HczStations.Add(new ModelWorkStation(model, new(-1.5305f, 1.0496f, 2.042f), new(0, 113.158f), new(0.0665f, 0.1003f, 0.1f)).WorkStation);

                foreach (var station in HczStations)
                    StationsManager.Register(station, Interact);

                new ModelDoor(model, DoorPrefabs.DoorHCZ, new(-0.082f, 0, 1.408f), new(0, -70), Vector3.one);
            }

            {
                var room = Map.Rooms.Find(x => x.Type == RoomType.LczArmory);
                var _scheme = SchematicUnity.API.SchematicManager.LoadSchematic(
                    Path.Combine(Paths.Plugins, "Schemes", "Nuclear", "LCZ.json"),
                    room.Position,
                    room.Rotation);
                foreach (var _obj in _scheme.Objects)
                    findObjects(_obj, 2);

                Model model = new("Nuclear-Lcz", room.Position, room.Rotation.eulerAngles);
                LczStation = new ModelWorkStation(model, new(5.1796f, 1.029f, -0.0651f), new(0, -90), new(0.0665f, 0.1003f, 0.1f)).WorkStation;

                StationsManager.Register(LczStation, Interact);
            }

            RadarStation = new(new(21.56f, 301.73f, -41.72f), new(0, 90), new(0.165f, 0.173f, 0.1f));
            StationsManager.Register(RadarStation, Interact);

            static void findObjects(SObject obj, int type)
            {
                switch (obj.Name)
                {
                    case "ColorLight":
                        {
                            if (obj.Light is not null)
                            {
                                if (type == 1)
                                    LightHcz = obj;
                                else if (type == 2)
                                    LightLcz = obj;
                            }
                            break;
                        }
                }
                foreach (var _obj in obj.Childrens)
                    findObjects(_obj, type);
            }
        }


        static void Interact(InteractWorkStationEvent ev)
        {
            if (HczStations.Contains(ev.Station))
            {
                ev.Allowed = false;

                if (ActivatedHcz)
                    return;

                if (!ev.Player.Tag.Contains(CPIR.Tag))
                    return;

                foreach (var station in HczStations)
                {
                    try
                    {
                        station.Status = WorkstationStatus.PoweringDown;
                    }
                    catch { }
                }

                CPIR.AllowAttack = true;
                ActivatedHcz = true;
                HintsUi.UpdateProgress();
                if (LightHcz.Light is not null)
                {
                    LightParams light = (LightParams)LightHcz.Light;
                    light.Color = Color.red;
                }

                ev.Player.Client.ShowHint("<color=#0dff15><b>Вы выкачали данные в Тяжелой Зоне</b></color>", 10);
                ev.Player.AddStats(100, 10, "саботаж комплекса за КСИР");
                return;
            }

            if (ev.Station == LczStation)
            {
                ev.Allowed = false;

                if (ActivatedLcz)
                    return;

                if (!ev.Player.Tag.Contains(CPIR.Tag))
                    return;

                ev.Station.Status = WorkstationStatus.PoweringDown;

                CPIR.AllowAttack = true;
                ActivatedLcz = true;
                HintsUi.UpdateProgress();
                if (LightLcz.Light is not null)
                {
                    LightParams light = (LightParams)LightLcz.Light;
                    light.Color = Color.red;
                }

                ev.Player.Client.ShowHint("<color=#0dff15><b>Вы выкачали данные в Легкой Зоне</b></color>", 10);
                ev.Player.AddStats(100, 10, "саботаж комплекса за КСИР");
                return;
            }

            if (ev.Station == RadarStation)
            {
                ev.Allowed = false;

                if (Activated)
                    return;

                if (!ev.Player.Tag.Contains(CPIR.Tag))
                    return;

                if (!ActivatedHcz)
                {
                    ev.Player.Client.ShowHint("<color=#ff0d0d><b>Необходимо выкачать данные в Тяжелой Зоне</b></color>", 10);
                    return;
                }
                if (!ActivatedLcz)
                {
                    ev.Player.Client.ShowHint("<color=#ff0d0d><b>Необходимо выкачать данные в Легкой Зоне</b></color>", 10);
                    return;
                }

                ev.Station.Status = WorkstationStatus.PoweringDown;

                Activate();

                ev.Player.AddStats(500, 50, "выполнение своей задачи за КСИР");
                return;
            }
        }

        static void Activate()
        {
            if (ConceptsController.IsActivated)
                return;

            Activated = true;
            HintsUi.UpdateProgress();

            try { Alpha.Stop(); } catch { }
            AlphaController.ChangeState(true, true);
            ConceptsController.Activate();

            int roundId = Round.CurrentRound;

            VoiceCore.PlayInIntercom(AudioPath, "Атака КСИР");
            Cassie.Lock = true;

            Timing.RunCoroutine(Process());

            IEnumerator<float> Process()
            {
                yield return Timing.WaitForSeconds(38f);

                if (!Activated || Round.CurrentRound != roundId)
                    yield break;

                GlobalLights.TurnOff(5);

                yield return Timing.WaitForSeconds(2f);

                GlobalLights.ChangeColor(new Color32(255, 151, 0, 255), true, true, true);
                DoorEventOpenerExtension.TriggerAction(DoorEventOpenerExtension.OpenerEventType.WarheadStart);


                yield return Timing.WaitForSeconds(130f);

                if (!Activated || Round.CurrentRound != roundId)
                    yield break;

                Loli.Builds.Models.Lift.Locked = true;
                foreach (var door in Map.Doors.Where(
                    x => x.Type is DoorType.ElevatorLczChkpA or DoorType.ElevatorLczChkpB
                    or DoorType.ElevatorHczChkpA or DoorType.ElevatorHczChkpB
                    or DoorType.EzCheckpointA or DoorType.EzCheckpointB
                    or DoorType.EzCheckpointArmoryA or DoorType.EzCheckpointArmoryB
                    or DoorType.ElevatorGateA or DoorType.ElevatorGateB))
                {
                    door.Open = false;
                    door.Lock = true;
                }


                yield return Timing.WaitForSeconds(32f);

                if (!Activated || Round.CurrentRound != roundId)
                    yield break;

                Alpha.Shake();
                Timing.RunCoroutine(KillSurface());


                yield return Timing.WaitForSeconds(30f);

                if (!Activated || Round.CurrentRound != roundId)
                    yield break;

                Timing.RunCoroutine(KillEveryone());


                yield return Timing.WaitForSeconds(110f);

                if (!Activated || Round.CurrentRound != roundId)
                    yield break;

                Round.End();
            }

            IEnumerator<float> KillSurface()
            {
                while (Activated && Round.CurrentRound == roundId)
                {
                    try
                    {
                        foreach (var pl in Player.List)
                        {

                            try
                            {
                                if (pl.RoleInformation.Role is RoleTypeId.Spectator or RoleTypeId.Overwatch)
                                    continue;

                                if (pl.GamePlay.CurrentZone is FacilityZone.LightContainment)
                                    continue;

                                pl.HealthInformation.Kill("Взрыв Ядерной Боеголовки");
                            }
                            catch { }
                        }
                    }
                    catch { }
                    yield return Timing.WaitForSeconds(1);
                }
                yield break;
            }

            IEnumerator<float> KillEveryone()
            {
                while (Activated && Round.CurrentRound == roundId)
                {
                    try
                    {
                        foreach (var pl in Player.List)
                        {

                            try
                            {
                                pl.HealthInformation.Damage(5, "Поражение радиацией");
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