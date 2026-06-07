using Loli.HintsCore;
using Loli.HintsCore.Utils;
using PlayerRoles;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;

namespace Loli.Addons;

static class CustomUnits
{
    static readonly DisplayBlock Block;

    static CustomUnits()
    {
        Block = new(new(3000, 470), new(600, 1250), padding: new(50), align: Align.Right);
    }

    static internal void AddUnit(string unit, string color)
    {
        Block.Contents.Add(new(unit, color.ColorFromHex(), "70%", prepare: (data) => RenderHint(data, unit)));
    }

    static void RenderHint(MessageEvent data, string unit)
    {
        if ($"{(data.Player.GetSpectatingPlayer() ?? data.Player).Variables["UNIT"]}" == unit)
            data.MessageBlock.Content = $"> >u/<{unit}>u<";
        else
            data.MessageBlock.Content = unit;
    }


    [EventMethod(RoundEvents.Waiting)]
    static void Refresh()
    {
        Block.Contents.Clear();
    }

    [EventMethod(RoundEvents.Start)]
    static void Start()
    {
        AddUnit("Охрана", "#afafa1");
    }

    [EventMethod(PlayerEvents.Spawn, int.MinValue)]
    static void Spawn(SpawnEvent ev)
    {
        if (ev.Role is RoleTypeId.FacilityGuard)
        {
            ev.Player.Variables["UNIT"] = "Охрана";
        }

        if (ev.Role is RoleTypeId.Spectator or RoleTypeId.Overwatch)
        {
            ev.Player.Variables.Remove("UNIT");
        }
    }

    [EventMethod(PlayerEvents.Spawn)]
    static void SpawnBlock(SpawnEvent ev)
    {
        if (!ev.Player.Variables.TryGetAndParse(Constants.VariableTag, out PlayerDisplay display))
            return;

        if (ev.Role is RoleTypeId.FacilityGuard or RoleTypeId.NtfPrivate or
            RoleTypeId.NtfSergeant or RoleTypeId.NtfSpecialist or
            RoleTypeId.NtfCaptain or RoleTypeId.Overwatch)
            display.AddBlock(Block);
        else
            display.RemoveBlock(Block);
    }

}