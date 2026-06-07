#if MRP
using Loli.Addons.RolePlay;
#endif
using Loli.Concepts.NuclearAttack;
using Loli.Concepts.Scp008;
using Loli.HintsCore;
using Loli.HintsCore.Utils;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using UnityEngine;

namespace Loli.Addons.Hints;

static class OverwatchHelp
{
    const string HintBlockTag = "Loli.Addons.Hints.OverwatchHelp.HintBlock";

    [EventMethod(PlayerEvents.Spawn)]
    static void Spawn(SpawnEvent ev)
    {
        if (!ev.Player.Variables.TryGetAndParse(Constants.VariableTag, out PlayerDisplay display))
            return;

        bool isOw = ev.Role is RoleTypeId.Overwatch;

        if (ev.Player.Variables.TryGetAndParse(HintBlockTag, out DisplayBlock block))
        {
            if (!isOw)
            {
                display.RemoveBlock(block);
                ev.Player.Variables.Remove(HintBlockTag);
            }

            return;
        }

        if (!isOw)
            return;

        block = new(new(-3000, -510), new(400 + 420, 700), padding: new(0, 0, 0, 420), align: Align.Left, newFromTop: false);
        display.AddBlock(block);

        ev.Player.Variables[HintBlockTag] = block;
    }

    [EventMethod(PlayerEvents.ChangeSpectate)]
    static void Change(ChangeSpectateEvent ev)
    {
        if (ev.New is null || ev.New == Server.Host)
            return;

        if (ev.Player.RoleInformation.Role is not RoleTypeId.Overwatch)
            return;

        if (!ev.Player.Variables.TryGetAndParse(HintBlockTag, out DisplayBlock block))
            return;

        string name = string.Empty;
        string cuff = string.Empty;
        (string role, string roleColor) = ev.New.RoleInformation.Role.GetInfoRole();

        if (ev.New.Variables.TryGetAndParse(MainInfo.NameBlockTag, out MessageBlock messageName))
            name = messageName.Content;

        if (ev.New.Variables.TryGetAndParse(MainInfo.CuffBlockTag, out MessageBlock messageCuff))
            cuff = messageCuff.Content;

        if (ev.New.ItsSpyFacilityManager())
        {
            role = "Менеджер Комплекса [Шпион]";
            roleColor = "#008f1c";
        }
        else if (ev.New.ItsHacker())
        {
            role = "Хакер";
        }
        else if (ev.New.Tag.Contains(SerpentsHand.Tag))
        {
            role = "Длань Змея";
        }
        else if (ev.New.Tag.Contains(CPIR.Tag))
        {
            role = "Шпион КСИР";
            roleColor = "red";
        }
#if MRP
        else if (ev.New.Tag.Contains(FacilityManager.Tag))
        {
            role = "Менеджер Комплекса";
            roleColor = "red";
        }
#endif
        else if (ev.New.UserInformation.CustomInfo.Contains("Начальник СБ"))
        {
            role = "Начальник СБ";
        }

        block.Contents.Clear();
        block.Contents.Add(new(name, new Color32(255, 112, 115, 255), "60%"));
        block.Contents.Add(new(cuff, new Color32(255, 112, 115, 255), "60%"));
        block.Contents.Add(new($"🔑 <b>Роль: <color={roleColor}>{role}</color></b>", new Color32(255, 112, 115, 255), "60%"));

        if (ev.New.Variables.ContainsKey("UNIT"))
            block.Contents.Add(new($"🔪 <b>Отряд: <color={roleColor}>{ev.New.Variables["UNIT"]}</color></b>", new Color32(255, 112, 115, 255), "60%"));
    }
}