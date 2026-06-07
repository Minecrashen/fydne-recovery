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
using System.Linq;
using Qurre.API.World;
using UnityEngine;

namespace Loli.Concepts.Hackers;

internal class Panel
{
    static readonly Dictionary<WorkStation, Panel> Panels;
    const string TimeCoroutinesName = "TimeCoroutine_PanelModel_LoliPlugin";

    static internal bool AllHacked
        => Panels.All(x => x.Value.Status == HackMode.Hacked);
    static internal IEnumerable<Panel> ReadPanels
        => Panels.Select(x => x.Value);

    static Panel()
    {
        Panels = new();
    }

    internal HackMode Status { get; private set; }

    readonly Room Room;
    readonly Primitive Monitor;
    readonly Vector3 PanelPosition;
    byte Process;

    Panel(Vector3 pos, Vector3 rot, Room room = null)
    {
        Process = 0;
        Room = room;

        Model DoModel = null;
        if (room is not null)
            DoModel = new(string.Empty, room.Position, room.Rotation.eulerAngles, new Vector3(0.725f, 0.725f, 0.725f));

        Model model = new("Panel", pos, rot, DoModel);
        PanelPosition = model.GameObject.transform.position;

        Color32 bp = new(0, 57, 81, 255);

        model.AddPart(new ModelPrimitive(model, PrimitiveType.Cube, Color.gray, new(0, 0, -0.028f), Vector3.zero, new(0.9f, 0.7f, 0.07f)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.gray, new(0, -0.45f, -0.25f), new(56, 0), new(0.9f, 0.5f, 0.07f)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.gray, new(0, -0.62421f, -0.481f), Vector3.zero, new(0.298f, 0.127f, 0.02f)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.gray, new(0, -0.745f, -0.49f), Vector3.zero, new(0.298f, 0.129f, 0.04f)));

        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.green, new(-0.332f, -0.33f, -0.16f), new(56, 0), new(0.1f, 0.1f, 0.03f)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.blue, new(-0.199f, -0.33f, -0.16f), new(56, 0), new(0.1f, 0.1f, 0.03f)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.yellow, new(-0.066f, -0.33f, -0.16f), new(56, 0), new(0.1f, 0.1f, 0.03f)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, bp, new(0.073f, -0.33f, -0.16f), new(56, 0), new(0.1f, 0.1f, 0.03f)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.cyan, new(0.212f, -0.33f, -0.16f), new(56, 0), new(0.1f, 0.1f, 0.03f)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.green, new(0.343f, -0.33f, -0.16f), new(56, 0), new(0.1f, 0.1f, 0.03f)));

        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.red, new(-0.332f, -0.403f, -0.264f), new(56, 0), new(0.1f, 0.1f, 0.03f)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, bp, new(-0.199f, -0.403f, -0.264f), new(56, 0), new(0.1f, 0.1f, 0.03f)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.cyan, new(-0.066f, -0.403f, -0.264f), new(56, 0), new(0.1f, 0.1f, 0.03f)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.blue, new(0.073f, -0.403f, -0.264f), new(56, 0), new(0.1f, 0.1f, 0.03f)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.red, new(0.212f, -0.403f, -0.264f), new(56, 0), new(0.1f, 0.1f, 0.03f)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.yellow, new(0.343f, -0.403f, -0.264f), new(56, 0), new(0.1f, 0.1f, 0.03f)));

        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.blue, new(-0.332f, -0.476f, -0.369f), new(56, 0), new(0.1f, 0.1f, 0.03f)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.yellow, new(-0.199f, -0.476f, -0.369f), new(56, 0), new(0.1f, 0.1f, 0.03f)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.green, new(-0.066f, -0.476f, -0.369f), new(56, 0), new(0.1f, 0.1f, 0.03f)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, bp, new(0.073f, -0.476f, -0.369f), new(56, 0), new(0.1f, 0.1f, 0.03f)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.yellow, new(0.212f, -0.476f, -0.369f), new(56, 0), new(0.1f, 0.1f, 0.03f)));
        model.AddPart(prim: new(model, PrimitiveType.Cube, Color.cyan, new(0.343f, -0.476f, -0.369f), new(56, 0), new(0.1f, 0.1f, 0.03f)));

        ModelPrimitive monitor = new(model, PrimitiveType.Cube, Color.green, new(0, 0.032f, -0.063f), Vector3.zero, new(0.85f, 0.6f, 0.01f));
        model.AddPart(monitor, false);
        Monitor = monitor.Primitive;

        ModelWorkStation station = new(model, new(-0.014f, -0.8026f, -0.4905f), new(0, 180), new(0.1115f, 0.1215f, 0.015f));

        model.AddPart(station);
        Panels.Add(station.WorkStation, this);

        StationsManager.Register(station.WorkStation, Event);

        foreach (var prim in model.Primitives)
        {
            prim.Primitive.IsStatic = true;
        }

    }

    void Interact(Player pl, WorkStation station)
    {
        if (Status is not HackMode.Safe)
            return;

        if (!pl.ItsHacker() && !pl.ItsSpyFacilityManager())
            return;

        station.Status = WorkstationStatus.PoweringUp;
        Status = HackMode.Hacking;

        HintsUi.UpdateProgressPanels();

        UpdateRoomColor();

        Timing.RunCoroutine(UpdateColorMonitor(), TimeCoroutinesName + $"{Room?.Type}");
        Timing.RunCoroutine(CheckDistance());

        IEnumerator<float> UpdateColorMonitor()
        {
            yield return Timing.WaitForSeconds(0.3f);

            int round = Round.CurrentRound;

            while (Status is HackMode.Hacking && round == Round.CurrentRound)
            {
                try { Monitor.Color = Color.yellow; } catch { }
                yield return Timing.WaitForSeconds(1.5f);
                try { Monitor.Color = Color.yellow / 2; } catch { }
                yield return Timing.WaitForSeconds(1.5f);
            }

            yield break;
        } // end coroutine

        IEnumerator<float> CheckDistance()
        {
            while (Status is HackMode.Hacking)
            {
                yield return Timing.WaitForSeconds(1f);

                if (Vector3.Distance(PanelPosition, pl.MovementState.Position) > 3)
                {
                    station.Status = WorkstationStatus.Offline;

                    Process = 0;
                    Status = HackMode.Safe;

                    HintsUi.UpdateProgressPanels();

                    UpdateRoomColor();

                    Timing.KillCoroutines(TimeCoroutinesName + $"{Room?.Type}");

                    try { Monitor.Color = Color.green; } catch { }

                    yield break;
                }

                Process += 10;

                if (Process < 100)
                    continue;

                station.Status = WorkstationStatus.PoweringDown;

                Process = 100;
                Status = HackMode.Hacked;

                HintsUi.UpdateProgressPanels();
                HintsUi.UpdateProgressControl();

                UpdateRoomColor();

                Timing.KillCoroutines(TimeCoroutinesName + $"{Room?.Type}");

                try { Monitor.Color = Color.red; } catch { }

                pl.AddStats(100, 10, "активация Станции Безопасности за Хакера");

                yield break;
            } // end while

            yield break;
        } // end coroutine

    }

    void UpdateRoomColor()
    {
        if (Room is null)
            return;

        Color roomColor = Utils.GetRoomColor(Status);
        Room.LightsOff(1f);

        Timing.CallDelayed(1f, () =>
        {
            if (roomColor == Color.white)
            {
                Room.Lights.LockChange = false;
                Room.Lights.Override = false;
                Room.Lights.Color = roomColor;
            }
            else
            {
                Room.Lights.LockChange = false;
                Room.Lights.Color = roomColor;
                Room.Lights.LockChange = true;
            }
        });
    }

    [EventMethod(RoundEvents.Waiting, 2)]
    static void Load()
    {
        Panels.Clear();

        foreach (Room room in Map.Rooms)
        {
            switch (room.Type)
            {
                case RoomType.HczJunk:
                    {
                        new Panel(new(-4.943f, 1.531f, -3.13f), new(0, 90), room);
                        break;
                    }

                case RoomType.HczHid:
                    {
                        new Panel(new(1.331f / 0.725f, 1.331f / 0.725f, -2.429f / 0.725f), Vector3.zero, room);
                        break;
                    }

                case RoomType.HczNuke:
                    {
                        new Panel(new(-0.98f / 0.725f, -71 / 0.725f, 0.113f / 0.725f), new(0, 180), room);
                        break;
                    }

                case RoomType.Hcz079:
                    {
                        new Panel(new(-7.57f / 0.725f, -3.87f / 0.725f, -9.752f / 0.725f), new(0, 270), room);
                        break;
                    }
            }
        }
    }

    static void Event(InteractWorkStationEvent ev)
    {
        if (!Panels.TryGetValue(ev.Station, out Panel panel))
            return;

        ev.Allowed = false;
        panel.Interact(ev.Player, ev.Station);
    }
}