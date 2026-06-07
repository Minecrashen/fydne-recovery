using Loli.Addons;
using Loli.DataBase;
using Loli.Modules.Voices;
using MEC;
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
using Qurre.API.World;
using UnityEngine;

namespace Loli.Concepts.Hackers;

static class Control
{
    static readonly List<Primitive> Monitors;
    static Vector3 PanelPosition;
    static bool Alerted;
    const string TimeCoroutinesName = "TimeCoroutine_ControlRoom_LoliPlugin";
    static readonly string AudioPath;

    static Control()
    {
        Monitors = new();
        PanelPosition = Vector3.zero;
        Alerted = false;

        Status = HackMode.Safe;
        Process = 0;

        AudioPath = Path.Combine(Path.Combine(Paths.Plugins, "Audio"), "OmegaWarhead", "ControlRoom.raw");
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

        if (!ev.Player.ItsHacker() && !ev.Player.ItsSpyFacilityManager())
            return;

        ev.Station.Status = WorkstationStatus.PoweringUp;
        Status = HackMode.Hacking;

        HintsUi.UpdateProgressControl();

        UpdateRoomsColor();

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

                if (Vector3.Distance(PanelPosition, ev.Player.MovementState.Position) > 7)
                {
                    ev.Station.Status = WorkstationStatus.Offline;

                    Process = 0;
                    Status = HackMode.Safe;

                    HintsUi.UpdateProgressControl();

                    UpdateRoomsColor();

                    Timing.KillCoroutines(TimeCoroutinesName);

                    foreach (var monitor in Monitors)
                        try { monitor.Color = Utils.GetRandomMonitorColor(); } catch { }

                    yield break;
                }

                Process++;

                if (Process % 10 == 0)
                    HintsUi.UpdateProgressControl();

                if (Process < 100)
                    continue;

                ev.Station.Status = WorkstationStatus.PoweringDown;

                Process = 100;
                Status = HackMode.Hacked;

                HintsUi.UpdateProgressControl();
                HintsUi.UpdateProgressServers();

                UpdateRoomsColor();

                Timing.KillCoroutines(TimeCoroutinesName);

                foreach (var monitor in Monitors)
                    try { monitor.Color = Utils.GetRandomMonitorColor(); } catch { }


                if (MainServers.DoorToRoom is not null)
                {
                    Transform transform = MainServers.DoorToRoom.transform;
                    Vector3 pos = transform.position;

                    Timing.RunCoroutine(OpenDoorThis());

                    IEnumerator<float> OpenDoorThis()
                    {
                        for (float i = 0; 45 >= i; i++)
                        {
                            try { transform.localPosition = new(pos.x, pos.y + 0.1f * i, pos.z); } catch { }
                            yield return Timing.WaitForSeconds(0.05f);
                        }

                        try { transform.localPosition = new(pos.x, pos.y + 4.5f, pos.z); } catch { }
                        yield break;
                    }
                } // end if

                ev.Player.AddStats(300, 30, "взлом Комнаты Управления за Хакера");

                yield break;
            } // end while

            yield break;
        } // end coroutine

        if (!Alerted)
        {
            var str = $"<color=rainbow><b>Внимание всему персоналу</b></color>\n" +
                $"<size=70%><color=#6f6f6f>Замечено хакерское вторжение в системы комплекса на уровне</color></size>\n" +
                $"<size=70%><color=#6f6f6f>пункта управления, требуется реакция средств самообороны</color></size>";

            var bc = Map.Broadcast(str.Replace("rainbow", "#ff0000"), 40, true);
            Timing.RunCoroutine(bc.WarnBc(str));

            VoiceCore.PlayInIntercom(AudioPath, "C.A.S.S.I.E.");

            Alerted = true;
        }

    }

    static void UpdateRoomsColor()
    {
        GlobalLights.TurnOff(1f);

        Timing.CallDelayed(1f, () =>
        {
            if (Status is HackMode.Safe)
                GlobalLights.SetToDefault();
            else
            {
                Color roomColor = Utils.GetRoomColor(Status);
                GlobalLights.ChangeColor(roomColor);
            }
        });
    }

    [EventMethod(RoundEvents.Waiting, int.MaxValue)]
    [EventMethod(RoundEvents.Restart)]
    static void Refresh()
    {
        Timing.KillCoroutines(TimeCoroutinesName);

        Monitors.Clear();
        Alerted = false;

        Status = HackMode.Safe;
        Process = 0;
    }

    [EventMethod(RoundEvents.Waiting, int.MinValue)]
    static void Load()
    {
        var scheme = SchematicUnity.API.SchematicManager.LoadSchematic(Path.Combine(Paths.Plugins, "Schemes", "Waiting_Room.json"), new(0, -700), default);

        Model button = new("Button", new(-129.17f, 291.77f, -72.85f));
        button.AddPart(prim: new(button, PrimitiveType.Cube, Color.red, Vector3.zero, new(90, 0), new(0.01f, 0.316f, 0.1f)));

        ModelWorkStation station = new(button, new(-0.0043f, -0.1606f, 0.0239f), new(0, -90), new(0.2224778f, 0.113811f, 0.015f));
        button.AddPart(station);

        StationsManager.Register(station.WorkStation, Interact);

        PanelPosition = station.WorkStation.Position;

        foreach (var _obj in scheme.Objects)
            findObjects(_obj);

        static void findObjects(SObject obj)
        {
            if (obj.Primitive != null)
            {
                PrimitiveParams prm = (PrimitiveParams)obj.Primitive;
                prm.Base.IsStatic = true;
            }

            switch (obj.Name)
            {
                case "Steklo":
                    {
                        if (obj.Primitive != null)
                        {
                            PrimitiveParams prm = (PrimitiveParams)obj.Primitive;
                            prm.Color = new Color32(0, 133, 155, 150);
                        }
                        break;
                    }
                case "Monitor":
                    {
                        if (obj.Primitive != null)
                        {
                            PrimitiveParams prm = (PrimitiveParams)obj.Primitive;
                            prm.Color = Utils.GetRandomMonitorColor();
                            Monitors.Add(prm.Base);
                        }
                        break;
                    }
                case "ProjectorLight":
                    {
                        if (obj.Primitive != null)
                        {
                            PrimitiveParams prm = (PrimitiveParams)obj.Primitive;
                            prm.Color = new Color(20, 20, 20);
                        }
                        break;
                    }
                case "Polotno_":
                    {
                        if (obj.Primitive != null)
                        {
                            PrimitiveParams prm = (PrimitiveParams)obj.Primitive;
                            prm.Color = new Color(3, 3, 3);
                        }
                        break;
                    }
                case "ButtonClick":
                    {
                        if (obj.Primitive != null)
                        {
                            PrimitiveParams prm = (PrimitiveParams)obj.Primitive;
                            prm.Color = new Color(5, 0, 0);
                        }
                        break;
                    }
            }

            foreach (var _obj in obj.Childrens)
                try { findObjects(_obj); } catch { }

        } // end void findObjects

    } // end load
}