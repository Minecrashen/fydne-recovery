using Loli.HintsCore;
using Loli.HintsCore.Utils;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.Events;
using Qurre.Events.Structs;
using UnityEngine;

namespace Loli.Addons.Hints;

static class ScpHint
{
    static readonly DisplayBlock Block;
    static readonly Color BlockColor = new(0.65f, 0, 0);

    static ScpHint()
    {
        Block = new(new(3000, 430), new(350, 600), align: Align.Right);
    }

    [EventMethod(PlayerEvents.Spawn)]
    static void Spawn(SpawnEvent ev)
    {
        if (!ev.Player.Variables.TryGetAndParse(Constants.VariableTag, out PlayerDisplay display))
            return;

        if (ev.Role.GetTeam() is Team.SCPs)
            display.AddBlock(Block);
        else
            display.RemoveBlock(Block);

    }

    static internal void UpdateScps()
    {
        Block.Contents.Clear();

        foreach (Player pl in Player.List)
        {
            if (pl.RoleInformation.Team != Team.SCPs)
                continue;

            RoleTypeId role = pl.RoleInformation.Role;
            MessageBlock message = new($"{pl.UserInformation.Nickname.OptimizeNick()} - {role} [{GetEmojiByRole(role)}]", BlockColor, "80%");
            Block.Contents.Add(message);
        }

        static string GetEmojiByRole(RoleTypeId role)
        {
            return role switch
            {
                RoleTypeId.Scp049 => "⚕️",
                RoleTypeId.Scp0492 => "♰",
                RoleTypeId.Scp079 => "💻",
                RoleTypeId.Scp096 => " 🐇",
                RoleTypeId.Scp106 => "👴",
                RoleTypeId.Scp173 => "🍪",
                RoleTypeId.Scp939 => "🐶",
                RoleTypeId.Scp3114 => "💀",
                _ => "⚠️"
            };
        } // end string
    } // end cycle void
}