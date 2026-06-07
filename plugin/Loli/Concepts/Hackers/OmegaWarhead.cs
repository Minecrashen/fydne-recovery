using Interactables.Interobjects.DoorUtils;
using Loli.Addons;
using Loli.Controllers;
using Loli.DataBase.Modules;
using Loli.Modules.Voices;
using MEC;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Qurre.API.World;
using UnityEngine;

namespace Loli.Concepts.Hackers;

static class OmegaWarhead
{
    [EventMethod(RoundEvents.Waiting)]
    static void NullCall() { }

    static OmegaWarhead()
    {
        CommandsSystem.RegisterRemoteAdmin("ow", RaActivate);
        CommandsSystem.RegisterRemoteAdmin("audio", RaMusic);
    }

    public static bool InProgress { get; private set; } = false;
    public static bool Detonated { get; private set; } = false;
    public static int RoundThis { get; private set; } = 0;
    static string AudioPath => Path.Combine(Path.Combine(Paths.Plugins, "Audio"), "OmegaWarhead", "OmegaWarhead.raw");

    public static void Start()
    {
        if (InProgress)
            return;

        if (ConceptsController.IsActivated)
            return;

        try { Alpha.Stop(); } catch { }

        InProgress = true;
        AlphaController.ChangeState(true, true);
        ConceptsController.Activate();

        GlobalLights.ChangeColor(Color.blue, true, true, true);
        DoorEventOpenerExtension.TriggerAction(DoorEventOpenerExtension.OpenerEventType.WarheadStart);

        RoundThis = Round.CurrentRound;

        VoiceCore.PlayInIntercom(AudioPath, "Омега Боеголовка");

        Timing.RunCoroutine(CallDelayed(Round.CurrentRound), "OmegaWarheadDelayed");
    }

    static IEnumerator<float> CallDelayed(int round)
    {
        yield return Timing.WaitForSeconds(160f);

        if (Round.CurrentRound != round)
            yield break;

        foreach (var door in Map.Doors.Where(x => x.IsLift))
            door.Lock = true;

        yield return Timing.WaitForSeconds(40f);

        if (Round.CurrentRound != round)
            yield break;

        Alpha.Detonate();
        Detonated = true;

        yield break;
    }

    [EventMethod(RoundEvents.Waiting)]
    static void Refresh()
    {
        InProgress = false;
        Detonated = false;
        Timing.KillCoroutines("OmegaWarheadDelayed");
    }

    [EventMethod(AlphaEvents.Stop)]
    static void NotDisable(AlphaStopEvent ev)
    {
        if (InProgress)
            ev.Allowed = false;
    }

    [EventMethod(AlphaEvents.Detonate)]
    static void AllKill()
    {
        if (!InProgress)
            return;

        foreach (Player pl in Player.List)
        {
            if (!pl.RoleInformation.IsAlive)
                continue;

            pl.HealthInformation.Kill("Взрыв Омега-Боеголовки");
        }

        Timing.CallDelayed(1f, () => Round.End());
    }

    static void RaActivate(RemoteAdminCommandEvent ev)
    {
        if (ev.Sender.SenderId == "SERVER CONSOLE")
            Activate();

        if (Data.Users.TryGetValue(ev.Sender.SenderId, out var data) && data.id == 1)
            Activate();

        void Activate()
        {
            ev.Allowed = false;
            ev.Reply = "Успешно";
            Map.Broadcast("<size=65%><color=#6f6f6f>Руководство объекта согласилось на <color=red>взрыв</color> <color=#0089c7>ОМЕГА Боеголовки</color></color></size>", 10, true);
            Start();
        }
    }
    static void RaMusic(RemoteAdminCommandEvent ev)
    {
        if (!Data.Users.TryGetValue(ev.Sender.SenderId, out var data))
            return;

        if (data.id != 1)
            return;

        ev.Allowed = false;
        ev.Reply = "Успешно";
        VoiceCore.PlayInIntercom(AudioPath, "Омега Боеголовка");
    }
}