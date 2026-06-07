#if MRP
using PlayerRoles;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.API.World;
using Qurre.Events;
using Qurre.Events.Structs;

namespace Loli.Addons.RolePlay
{
    static class SafeSystem
    {
        [EventMethod(RoundEvents.Waiting)]
        static void NullCall() { }

        internal const string Tag = "SafeSystem";

        static SafeSystem()
        {
            CommandsSystem.RegisterConsole("ssa", Com);
            static void Com(GameConsoleCommandEvent ev)
            {
                ev.Allowed = false;
                if (ev.Player.RoleInformation.Role != RoleTypeId.Scp079)
                {
                    ev.Reply = "Вы не SCP-079";
                    return;
                }
                if (ev.Player.Variables.ContainsKey(Tag))
                {
                    ev.Reply = "Вы уже являетесь Системой Безопасности";
                    return;
                }
                ev.Reply = "Успешно";
                ev.Player.Variables[Tag] = true;
                ev.Player.UserInformation.CustomInfo = "Система Безопасности";
                ev.Player.UserInformation.InfoToShow = PlayerInfoArea.Nickname | PlayerInfoArea.Badge | PlayerInfoArea.CustomInfo;
                Map.Broadcast("<size=70%><color=#6f6f6f>Активирована</color> <color=red>Система Безопасности</color></size>", 15);
                Cassie.Send("safety system .g4 activated");
            }
        }

        [EventMethod(PlayerEvents.Spawn)]
        static void FixTags(SpawnEvent ev)
        {
            ev.Player.Variables.Remove(Tag);
        }

        [EventMethod(PlayerEvents.ChangeRole)]
        static void FixTags(ChangeRoleEvent ev)
        {
            if (!ev.Allowed)
                return;

            ev.Player.Variables.Remove(Tag);
        }

        [EventMethod(PlayerEvents.Dead)]
        static void FixTags(DeadEvent ev)
        {
            ev.Target.Variables.Remove(Tag);
        }
    }
}
#endif