#if MRP
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Keycards;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Objects;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Collections.Generic;
using InventorySystem.Items;
using Qurre.API.Controllers;
using Qurre.API.World;

namespace Loli.Addons.RolePlay
{
    static class OpenSCPs
    {
        static readonly List<RoleTypeId> Scps = [];

        [EventMethod(RoundEvents.Waiting)]
        static void ClearJanitors() => Scps.Clear();

        [EventMethod(PlayerEvents.Spawn)]
        [EventMethod(PlayerEvents.ChangeRole)]
        static void Spawn(IBaseEvent ev)
        {
            RoleTypeId role;

            if (ev is SpawnEvent se)
                role = se.Role;
            else if (ev is ChangeRoleEvent ce)
                role = ce.Role;
            else
                return;

            if (role == RoleTypeId.Scp0492 ||
                role.GetFaction() != Faction.SCP)
                return;

            if (!Scps.Contains(role))
                Scps.Add(role);

            switch (role)
            {
                case RoleTypeId.Scp049:
                    {
                        if (Map.Doors.TryFind(out var door, x => x.Type == DoorType.Hcz049Gate))
                        {
                            door.Open = true;
                            door.Lock = true;
                        }
                        break;
                    }

                case RoleTypeId.Scp096:
                    {
                        if (Map.Doors.TryFind(out var door, x => x.Type == DoorType.Hcz096))
                        {
                            door.Open = true;
                            door.Lock = true;
                        }
                        break;
                    }

                case RoleTypeId.Scp173:
                    {
                        if (Map.Doors.TryFind(out var door, x => x.Type == DoorType.Hcz173Gate))
                        {
                            door.Open = true;
                            door.Lock = true;
                        }
                        break;
                    }
            }

        }

        [EventMethod(PlayerEvents.InteractDoor, int.MinValue)]
        static void Door(InteractDoorEvent ev)
        {
            switch (ev.Door.Type)
            {
                case DoorType.Hcz049Gate:
                    {
                        Call(RoleTypeId.Scp049);
                        break;
                    }
                case DoorType.Hcz096:
                    {
                        Call(RoleTypeId.Scp096);
                        break;
                    }
                case DoorType.Hcz173Gate:
                    {
                        Call(RoleTypeId.Scp173);
                        break;
                    }
            }

            void Call(RoleTypeId role)
            {
                if (ev.Door.Lock)
                {
                    if (!ev.Door.Open)
                        ev.Door.Open = true;

                    return;
                }

                if (!ev.Allowed)
                    return;

                ev.Allowed = false;

                if (ev.Player.RoleInformation.Role == role)
                {
                    ev.Door.Open = true;
                    ev.Door.Lock = true;
                    return;
                }

                if (Scps.Contains(role))
                    return;

                if (!CheckPerms(ev.Player))
                {
                    ev.Player.Client.ShowHint("<b><color=red>Недостаточно доступа</color></b>", 5);
                    return;
                }

                if (!TryFindPlayer(out Player pl))
                {
                    ev.Player.Client.ShowHint("<b><color=red>Не найден подходящий игрок для спавна данного SCP</color></b>", 5);
                    return;
                }

                pl.RoleInformation.SetNew(role, RoleChangeReason.Respawn);
                ev.Door.Open = true;
            }

            static bool TryFindPlayer(out Player pl)
            {
                if (Player.List.TryFind(out var pl2, x => x.RoleInformation.Role == RoleTypeId.Spectator))
                {
                    pl = pl2;
                    return true;
                }
                pl = null;
                return false;
            }

            static bool CheckPerms(Player pl)
            {
                ItemBase cur = pl?.Inventory.Base.CurInstance;
                if (cur is null)
                    return false;

                return cur is KeycardItem card && card.GetPermissions(null).HasFlagAny(DoorPermissionFlags.ContainmentLevelTwo);
            }
        }
    }
}
#endif