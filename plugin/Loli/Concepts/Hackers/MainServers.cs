using Loli.Addons;
using Loli.DataBase;
using MEC;
using Qurre.API;
using Qurre.API.Addons.Models;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.API.Objects;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Collections.Generic;
using Qurre.API.World;
using UnityEngine;

namespace Loli.Concepts.Hackers;

static class MainServers
{
    static readonly List<Primitive> Monitors = new();
    static Vector3 PanelPosition;
    const string TimeCoroutinesName = "TimeCoroutine_ServersManager_LoliPlugin";

    static readonly HashSet<Door> DoorsInteract;
    static (GameObject, GameObject) Doors;
    static byte DoorStatus; // 0 - closed; 1 - opened; 2 - opening;
    static internal GameObject DoorToRoom;

    static MainServers()
    {
        Monitors = new();
        PanelPosition = Vector3.zero;

        DoorsInteract = new();
        Doors = (null, null);
        DoorStatus = 0;

        Status = HackMode.Safe;
        Process = 0;
    }

    static internal HackMode Status { get; private set; }
    static internal byte Process { get; private set; }


    static void Interact(InteractWorkStationEvent ev)
    {
        ev.Allowed = false;

        if (Status is not HackMode.Safe)
            return;

        if (!Panel.AllHacked)
            return;

        if (Control.Status is not HackMode.Hacked)
            return;

        if (!ev.Player.ItsHacker() && !ev.Player.ItsSpyFacilityManager())
            return;

        ev.Station.Status = WorkstationStatus.PoweringUp;
        Status = HackMode.Hacking;

        HintsUi.UpdateProgressServers();

        Timing.RunCoroutine(UpdateColorMonitor(), TimeCoroutinesName);
        Timing.RunCoroutine(CheckDistance());

        static IEnumerator<float> UpdateColorMonitor()
        {
            yield return Timing.WaitForSeconds(0.3f);

            while (Status is HackMode.Hacking)
            {
                foreach (var monitor in Monitors)
                    try { monitor.Color = Color.yellow; } catch { }

                yield return Timing.WaitForSeconds(1.5f);

                foreach (var monitor in Monitors)
                    try { monitor.Color = Color.yellow / 2; } catch { }

                yield return Timing.WaitForSeconds(1.5f);
            }

            yield break;
        } // end coroutine

        IEnumerator<float> CheckDistance()
        {
            while (Status is HackMode.Hacking)
            {
                yield return Timing.WaitForSeconds(1f);

                if (Vector3.Distance(PanelPosition, ev.Player.MovementState.Position) > 15)
                {
                    ev.Station.Status = WorkstationStatus.Offline;

                    Process = 0;
                    Status = HackMode.Safe;

                    HintsUi.UpdateProgressServers();

                    Timing.KillCoroutines(TimeCoroutinesName);

                    foreach (var monitor in Monitors)
                        try { monitor.Color = Utils.GetRandomMonitorColor(); } catch { }

                    yield break;
                }

                Process++;

                if (Process % 10 == 0)
                    HintsUi.UpdateProgressServers();

                if (Process < 100)
                    continue;

                ev.Station.Status = WorkstationStatus.PoweringDown;

                Process = 100;
                Status = HackMode.Hacked;

                HintsUi.UpdateProgressServers();

                OmegaWarhead.Start();

                Timing.KillCoroutines(TimeCoroutinesName);

                foreach (var monitor in Monitors)
                    try { monitor.Color = Utils.GetRandomMonitorColor(); } catch { }

                var str = $"<color=rainbow><b>Внимание всему персоналу</b></color>\n" +
                    $"<size=70%><color=#6f6f6f><color=red>Хакеры</color> <color=green>Повстанцев Хаоса</color> успешно выкачали <color=red>все</color> данные комплекса.\n" +
                    $"Запущена <color=#0089c7>ОМЕГА Боеголовка</color></color></size>";
                var bc = Map.Broadcast(str.Replace("rainbow", "#ff0000"), 30, true);
                Timing.RunCoroutine(bc.WarnBc(str));

                ev.Player.AddStats(500, 50, "взлом Основных Серверов за Хакера");

                yield break;
            } // end while

            yield break;
        } // end coroutine

    }

    [EventMethod(PlayerEvents.InteractDoor)]
    static void InteractDoor(InteractDoorEvent ev)
    {
        if (!DoorsInteract.Contains(ev.Door))
            return;

        ev.Allowed = false;

        if (DoorStatus == 2)
            return;

        if (DoorStatus == 1)
            Timing.RunCoroutine(CloseDoor(), "DoorOpenCloseInServersRoom");
        else
            Timing.RunCoroutine(OpenDoor(), "DoorOpenCloseInServersRoom");

        IEnumerator<float> OpenDoor()
        {
            DoorStatus = 2;

            (GameObject Door1, GameObject Door2) = Doors;

            var pos1 = Door1.transform.localPosition;
            var pos2 = Door2.transform.localPosition;

            Door1.transform.localPosition = new(pos1.x, pos1.y, 1);
            Door2.transform.localPosition = new(pos2.x, pos2.y, -1);

            for (float i = 0; 100 >= i; i++)
            {
                try { Door1.transform.localPosition = new(pos1.x, pos1.y, 1 + 0.02f * i); } catch { }
                try { Door2.transform.localPosition = new(pos2.x, pos2.y, -1 - 0.02f * i); } catch { }
                yield return Timing.WaitForSeconds(0.05f);
            }

            DoorStatus = 1;
        }

        IEnumerator<float> CloseDoor()
        {
            DoorStatus = 2;

            (GameObject Door1, GameObject Door2) = Doors;

            var pos1 = Door1.transform.localPosition;
            var pos2 = Door2.transform.localPosition;

            Door1.transform.localPosition = new Vector3(pos1.x, pos1.y, 3);
            Door2.transform.localPosition = new Vector3(pos2.x, pos2.y, -3);

            for (float i = 0; 100 >= i; i++)
            {
                try { Door1.transform.localPosition = new Vector3(pos1.x, pos1.y, 3 - 0.02f * i); } catch { }
                try { Door2.transform.localPosition = new Vector3(pos2.x, pos2.y, -3 + 0.02f * i); } catch { }
                yield return Timing.WaitForSeconds(0.05f);
            }

            DoorStatus = 0;
        } // end coroutine
    } // end void


    [EventMethod(RoundEvents.Waiting, int.MaxValue)]
    [EventMethod(RoundEvents.Restart)]
    static void Refresh()
    {
        Monitors.Clear();
        PanelPosition = Vector3.zero;

        DoorsInteract.Clear();
        DoorStatus = 0;
        Timing.KillCoroutines("DoorOpenCloseInServersRoom");

        Status = HackMode.Safe;
        Process = 0;
    }

    [EventMethod(RoundEvents.Waiting, int.MinValue)]
    static void Load()
    {
        Color32 steklo = new(0, 133, 155, 150);
        Color32 steklo_black = new(0, 0, 0, 150);

        Model model = new("Servers_Room", new(-88.47f, -810, -69.76f), Vector3.zero, new Vector3(0.725f, 0.725f, 0.725f));

        new Builds.Models.Lift(new(null, new(-59.108f, 292.9f, -53.641f), Vector3.zero, true),
            new(model.GameObject.transform, new(7.22f, 2.5f, -4.14f), Vector3.zero, true), Color.white, true);

        model.AddPart(prim: new(model, PrimitiveType.Quad, Color.white, new(0, -3f), new(90, 0), new(20, 20, 1)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.white, Vector3.zero, new(90, 0), new(20, 20, 0.1f)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.white, new(0, 4.87f), new(270, 0), new(20, 20, 0.1f)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.white, new(0, 2.447021f, 10), Vector3.zero, new(20, 4.9f, 0.1f)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.white, new(0, 2.447021f, -10), new(0, 180), new(20, 4.9f, 0.1f)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.white, new(10, 2.447021f, 0), new(0, 90), new(20, 4.9f, 0.1f)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.white, new(-10, 2.447021f, 0), new(0, 270), new(20, 4.9f, 0.1f)));

        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.white, new(4.25f, 2.447f, -5), new(0, 270), new(10, 4.9f, 0.4f)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.white, new(4.25f, 2.447f, 6.89f), new(0, 270), new(6, 4.9f, 0.4f)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.white, new(4.25f, 4.07f, 1.95f), new(0, 90), new(3.9f, 1.6f, 0.4f)));

        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.white, new(-3.911f, 0.74f, -6.224f), Vector3.zero, new(12, 1.5f, 0.1f)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, steklo_black, new(-3.911f, 2.99f, -6.224f), Vector3.zero, new(12, 3, 0.09f)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.white, new(-3.911f, 4.6761f, -6.224f), Vector3.zero, new(12, 0.37f, 0.1f)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.white, new(2.133f, 2.44f, -6.224f), Vector3.zero, new(0.1f, 4.9f, 0.1f)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.white, new(-9.952f, 2.44f, -6.224f), Vector3.zero, new(0.1f, 4.9f, 0.1f)));

        model.AddPart(light: new(model, new Color32(33, 46, 82, 255), new(-2, 3.5f, 2), 40, 10));
        model.AddPart(light: new(model, new Color32(33, 46, 82, 255), new(6.891f, 3.539f, 6.686f), 20, 10));
        model.AddPart(light: new(model, new Color32(33, 46, 82, 255), new(-8.9f, 3.4f, -8.15f), 2, 5));

        model.AddPart(prim: new(model, PrimitiveType.Cylinder, Color.red, new(4.391f, 1.689f, 4.857f), new(0, 0, 90), new(0.3f, 0.1f, 0.3f)));
        model.AddPart(prim: new(model, PrimitiveType.Cylinder, Color.red, new(4.122f, 1.689f, 4.857f), new(0, 0, 90), new(0.3f, 0.1f, 0.3f)));

        CreateMonitor(new(2.414f, 3.82f, -9.94f), new(0, 90), Vector3.one);
        CreateMonitor(new(-0.086f, 3.82f, -9.94f), new(0, 90), Vector3.one);
        CreateMonitor(new(-2.59f, 3.82f, -9.94f), new(0, 90), Vector3.one);
        CreateMonitor(new(-5.09f, 3.82f, -9.94f), new(0, 90), Vector3.one);
        CreateMonitor(new(-7.59f, 3.82f, -9.94f), new(0, 90), Vector3.one);

        CreateMonitor(new(-6.304f, 2.716f, -9.94f), new(0, 90), Vector3.one);
        CreateMonitor(new(-3.8f, 2.716f, -9.94f), new(0, 90), Vector3.one);
        CreateMonitor(new(-1.3f, 2.716f, -9.94f), new(0, 90), Vector3.one);
        CreateMonitor(new(1.1f, 2.716f, -9.94f), new(0, 90), Vector3.one);

        CreateMonitor(new(-9.941f, 2.94f, -8.093f), new(0, 180), new(1, 5, 3.5f));

        CreatePanel(new(-9.435f, 0.491f, -8.163f), new(0, 180));

        {
            Model Door = new("Door", new(4.25f, 1.635f, 1.95f), Vector3.zero, model);
            ModelPrimitive obj1 = new(Door, PrimitiveType.Cube, steklo, new(0, 0, 1), new(0, 90), new(2, 3.265f, 0.35f));
            Door.AddPart(obj1);

            ModelPrimitive obj2 = new(Door, PrimitiveType.Cube, steklo, new(0, 0, -1), new(0, 90), new(2, 3.265f, 0.35f));
            Door.AddPart(obj2);

            Doors = (obj1.GameObject, obj2.GameObject);
        }

        {
            ModelPrimitive _obj = new(model, PrimitiveType.Cube, new Color32(33, 37, 37, 255),
                new(3.106f, 2.44f, -6.224f), Vector3.zero, new(1.9f, 4.9f, 0.08f));
            model.AddPart(_obj, false);

            DoorToRoom = _obj.GameObject;
        }

        foreach (var prim in model.Primitives)
        {
            prim.Primitive.IsStatic = true;
        }

        {
            GameObject zero = new();
            zero.transform.parent = model.GameObject.transform;
            zero.transform.localPosition = new(4.351722f, 3.434469f, 6.121384f);
            zero.transform.localRotation = Quaternion.Euler(new(180, 90));

            Door door = new(zero.transform.position, DoorPrefabs.DoorHCZ, zero.transform.rotation);
            DoorsInteract.Add(door);
            door.Name = Builds.Load.StaticDoorName;
        }
        {
            GameObject zero = new();
            zero.transform.parent = model.GameObject.transform;
            zero.transform.localPosition = new(4.146107f, -0.09378367f, 6.121384f);
            zero.transform.localRotation = Quaternion.Euler(new(0, 90));

            Door door = new(zero.transform.position, DoorPrefabs.DoorHCZ, zero.transform.rotation);
            DoorsInteract.Add(door);
            door.Name = Builds.Load.StaticDoorName;
        }

        void CreateMonitor(Vector3 _pos, Vector3 _rot, Vector3 scale)
        {
            Model Monitor = new("Monitor", _pos, _rot, model);

            Monitor.AddPart(new ModelPrimitive(Monitor, PrimitiveType.Cube, new Color32(48, 48, 48, 255), Vector3.zero, Vector3.zero,
                new Vector3(0.02f * scale.x, 0.7f * scale.y, 1 * scale.z)));

            var mon = new ModelPrimitive(Monitor, PrimitiveType.Quad, Utils.GetRandomMonitorColor(), new(-0.015f, 0), new(0, 90),
                new Vector3(0.97f * scale.z, 0.67f * scale.y, 1));

            Monitor.AddPart(mon);
            Monitors.Add(mon.Primitive);

            foreach (var prim in Monitor.Primitives)
            {
                prim.Primitive.IsStatic = true;
            }
        }

        void CreatePanel(Vector3 _pos, Vector3 _rot)
        {
            Model Panel = new("Panel", _pos, _rot, model);
            Panel.AddPart(prim: new(Panel, PrimitiveType.Cube, Color.gray, Vector3.zero, new(90, 90), new Vector3(1, 0.7f, 1)));
            Panel.AddPart(prim: new(Panel, PrimitiveType.Quad, Utils.GetRandomMonitorColor(), new(0.173f, 0.506f, 0.34f), new(90, 0), new Vector3(0.1f, 0.1f, 1)));
            Panel.AddPart(prim: new(Panel, PrimitiveType.Quad, Utils.GetRandomMonitorColor(), new(0.173f, 0.506f, 0.112f), new(90, 0), new Vector3(0.1f, 0.1f, 1)));
            Panel.AddPart(prim: new(Panel, PrimitiveType.Quad, Utils.GetRandomMonitorColor(), new(0.173f, 0.506f, -0.13f), new(90, 0), new Vector3(0.1f, 0.1f, 1)));
            Panel.AddPart(prim: new(Panel, PrimitiveType.Quad, Utils.GetRandomMonitorColor(), new(0.173f, 0.506f, -0.339f), new(90, 0), new Vector3(0.1f, 0.1f, 1)));
            Panel.AddPart(prim: new(Panel, PrimitiveType.Quad, Utils.GetRandomMonitorColor(), new(-0.029f, 0.506f, 0.34f), new(90, 0), new Vector3(0.1f, 0.1f, 1)));
            Panel.AddPart(prim: new(Panel, PrimitiveType.Quad, Utils.GetRandomMonitorColor(), new(-0.029f, 0.506f, -0.339f), new(90, 0), new Vector3(0.1f, 0.1f, 1)));

            Model button = new("Button", new(-9.342f, 1.046f, -8.16f), new(0, 180), new(1.37931f, 1.37931f, 1.37931f), model);
            button.AddPart(prim: new(button, PrimitiveType.Cube, Color.red, Vector3.zero, new(90, 0), new(0.01f, 0.316f, 0.1f)));

            ModelWorkStation station = new(button, new(-0.0043f, -0.1606f, 0.0239f), new(0, -90), new(0.2224778f, 0.113811f, 0.015f));
            button.AddPart(station);

            StationsManager.Register(station.WorkStation, Interact);

            PanelPosition = station.WorkStation.Position;

            foreach (var prim in Panel.Primitives)
            {
                prim.Primitive.IsStatic = true;
            }
        }

        Timing.RunCoroutine(SpawnServers());
        IEnumerator<float> SpawnServers()
        {
            yield return Timing.WaitForSeconds(1);
            new Builds.Models.Server(new Vector3(0, 0.84f, 7), new Vector3(0, 220), model);
            new Builds.Models.Server(new Vector3(-4, 0.84f, 7), new Vector3(0, 220), model);
            new Builds.Models.Server(new Vector3(-7.51f, 0.84f, 7), new Vector3(0, 220), model);

            new Builds.Models.Server(new Vector3(0, 0.84f, -3.69f), new Vector3(0, 120), model);
            new Builds.Models.Server(new Vector3(-4, 0.84f, -3.69f), new Vector3(0, 120), model);
            new Builds.Models.Server(new Vector3(-7.51f, 0.84f, -3.69f), new Vector3(0, 120), model);
            yield break;
        } // end coroutine

    } // end void
}