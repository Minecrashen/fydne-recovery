#if MRP
using Loli.HintsCore.Utils;
using Loli.HintsCore;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Addons.Models;
using Qurre.API.Attributes;
using Qurre.API.Objects;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Collections.Generic;
using LabApi.Features.Wrappers;
using UnityEngine;
using JailbirdItem = InventorySystem.Items.Jailbird.JailbirdItem;
using Player = Qurre.API.Controllers.Player;

namespace Loli.Addons.RolePlay
{
    static class Scp096Rework
    {
        const string CellProcessPlayerTag = "Scp096Cell_PlayerHas_Process";
        const string CellModelPlayerTag = "Scp096Cell_PlayerHas_Model";
        const string CellOwnerPlayerTag = "Scp096Cell_PlayerHas_Owner";
        const string BlockTag = "Loli.Addons.RolePlay.Scp096Rework.Block";
        static readonly HashSet<Player> Scp096List = new();
        static readonly HashSet<ushort> RealCuffsWithCell = new();

        static internal void GetCell096(this Item item)
        {
            RealCuffsWithCell.Add(item.Serial);
        }

        static internal bool CheckString(ushort serial, out string str)
        {
            if (RealCuffsWithCell.Contains(serial))
            {
                str = "<color=#a02729>Мешок SCP-096 в наличии</color>";
                return true;
            }

            str = string.Empty;
            return false;
        }

        static internal void RemoveBlock(Player pl, DisplayBlock block)
        {
            if (!pl.Variables.TryGetAndParse(Constants.VariableTag, out PlayerDisplay display))
                return;

            display.RemoveBlock(block);
        }

        static internal void AddBlock(Player pl, DisplayBlock block)
        {
            if (!pl.Variables.TryGetAndParse(Constants.VariableTag, out PlayerDisplay display))
                return;

            display.AddBlock(block);
        }

        static internal void UpdateBlock(Player pl)
        {
            if (!pl.Variables.TryGetAndParse(BlockTag, out DisplayBlock block))
                return;

            if (block.Contents.Count < 1)
                return;

            block.Contents[0].Content = $"{pl.Variables[CellProcessPlayerTag]}/30";
        }

        static internal void ResetBlock(Player pl)
        {
            if (!pl.Variables.TryGetAndParse(BlockTag, out DisplayBlock block))
                return;

            pl.Variables.Remove(BlockTag);
            block.Contents.Clear();

            if (pl.Variables.TryGetAndParse(CellOwnerPlayerTag, out Player cuffer))
            {
                RemoveBlock(pl, block);
                RemoveBlock(cuffer, block);
            }
            else
            {
                foreach (Player obs in Player.List)
                    RemoveBlock(obs, block);
            }
        }

        static internal void CreateBlock(Player pl, Player cuffer)
        {
            if (pl.Variables.TryGetAndParse(BlockTag, out DisplayBlock block))
            {
                block.Contents.Clear();
                goto IL_001;
            }

            if (!pl.Variables.TryGetAndParse(Constants.VariableTag, out PlayerDisplay display))
                return;

            block = new(new(3000, -510), new(400, 200), padding: new(25), align: Align.Right, newFromTop: false);
            display.AddBlock(block);

            AddBlock(pl, block);

            goto IL_001;

        IL_001:
            {
                block.Contents.Add(new("0/30", new Color32(70, 185, 74, 255), "75%"));
                block.Contents.Add(new("Прогресс ВОУС SCP-096:", new Color32(70, 185, 74, 255), "75%"));
                pl.Variables[BlockTag] = block;
            }
        }

        static internal void Process(Player pl, Player target, JailbirdItem jail)
        {
            if (!RealCuffsWithCell.Contains(jail.ItemSerial))
            {
                pl.Client.ShowHint("<color=#ff9300>В этом связывающем устройстве нет мешка для SCP-096</color>", 7);
                return;
            }

            if (target.Variables.ContainsKey(CellProcessPlayerTag))
            {
                pl.Client.ShowHint("<color=#ff9300>Прогресс ВОУС SCP-096 уже начат</color>", 7);
                return;
            }

            if (target.Variables.ContainsKey(CellModelPlayerTag))
            {
                pl.Client.ShowHint("<color=#ff9300>Мешок уже надет</color>", 7);
                return;
            }

            target.Variables[CellProcessPlayerTag] = 30;
            target.Variables[CellOwnerPlayerTag] = pl;
            CreateBlock(target, pl);
        }

        static internal void PutCell(Player target)
        {
            if (target.Variables.TryGetAndParse(CellModelPlayerTag, out Model cell2))
                try { cell2.Destroy(); } catch { }

            Model cell = new("Cell_" + target.UserInformation.UserId, Vector3.zero, Vector3.zero);
            cell.GameObject.transform.parent = target.Transform;
            cell.GameObject.transform.localPosition = Vector3.down;
            cell.GameObject.transform.localRotation = Quaternion.identity;
            cell.GameObject.transform.localScale = new(0.725f, 0.725f, 0.725f);

            Color32 bottomColor = new(60, 62, 61, 255);
            cell.AddPart(prim: new(cell, PrimitiveType.Capsule, bottomColor, new(0.11f, 2.564f, 0.416f), new(50, 0), new(0.4f, 0.3f, 0.4f), false));

            cell.Primitives.ForEach(prim =>
            {
                prim.Primitive.MovementSmoothing = 64;
            });

            target.RoleInformation.Scp096.TargetsTracker.ClearAllTargets();
            target.RoleInformation.Scp096.RageManager.ServerEndEnrage();
            target.Variables[CellModelPlayerTag] = cell;
            target.Variables.Remove(CellOwnerPlayerTag);
        }

        static internal void Cycle1s()
        {
            foreach (Player scp in Scp096List)
            {
                if (!scp.Variables.TryGetAndParse(CellProcessPlayerTag, out int time))
                    continue;

                if (scp.Variables.TryGetAndParse(CellOwnerPlayerTag, out Player pl) &&
                    !pl.Disconnected && scp.DistanceTo(pl) < 3f)
                    time--;
                else
                    time++;

                if (time >= 30)
                {
                    ResetBlock(scp);
                    scp.Variables.Remove(CellProcessPlayerTag);
                    scp.Variables.Remove(CellOwnerPlayerTag);
                    continue;
                }

                if (time < 1)
                {
                    ResetBlock(scp);
                    scp.Variables.Remove(CellProcessPlayerTag);
                    try { PutCell(scp); } catch (System.Exception ex) { Log.Error(ex); }
                    continue;
                }

                scp.Variables[CellProcessPlayerTag] = time;
                UpdateBlock(scp);
            }
        }

        static internal void Cycle5s()
        {
            foreach (Player pl in Player.List)
            {
                if (pl.RoleInformation.Role is RoleTypeId.Scp096)
                {
                    Scp096List.Add(pl);
                }
                else
                {
                    ResetBlock(pl);
                    Scp096List.Remove(pl);
                    pl.Variables.Remove(CellProcessPlayerTag);
                }
            }
        }


        [EventMethod(RoundEvents.Waiting)]
        static void ClearCache()
        {
            RealCuffsWithCell.Clear();
            Scp096List.Clear();
        }

        [EventMethod(PlayerEvents.InteractDoor, -10)]
        static void InteractDoor(InteractDoorEvent ev)
        {
            if (!ev.Allowed)
                return;

            if (ev.Player.RoleInformation.Role is not RoleTypeId.Scp096)
                return;

            if (!ev.Player.Variables.ContainsKey(CellModelPlayerTag))
                return;

            ev.Allowed = false;
        }

        [EventMethod(ScpEvents.Scp096SetState)]
        static void AddTarget(Scp096SetStateEvent ev)
        {
            if (ev.State is Scp096State.Calming)
            {
                if (ev.Player.RoleInformation.Scp096.TargetsTracker.Targets.Count == 0)
                    return;

                ev.Allowed = false;
                return;
            }

            if (!ev.Player.Variables.ContainsKey(CellModelPlayerTag))
                return;

            if (ev.State is not Scp096State.Enraging and not Scp096State.Distressed)
                return;

            ev.Allowed = false;
        }

        [EventMethod(ScpEvents.Scp096AddTarget)]
        static void AddTarget(Scp096AddTargetEvent ev)
        {
            if (!ev.Scp.Variables.ContainsKey(CellModelPlayerTag))
                return;

            ev.Allowed = false;
        }

        [EventMethod(PlayerEvents.Spawn)]
        [EventMethod(PlayerEvents.ChangeRole)]
        static void Spawn(IBaseEvent ev)
        {
            Player pl = ev switch
            {
                SpawnEvent ev1 => ev1.Player,
                ChangeRoleEvent ev2 => ev2.Player,
                _ => null,
            };

            if (pl is null)
                return;

            if (pl.Variables.TryGetAndParse(CellModelPlayerTag, out Model cell))
                cell.Destroy();

            pl.Variables.Remove(CellModelPlayerTag);
        }

        [EventMethod(PlayerEvents.Damage, 1)]
        static void Damage(DamageEvent ev)
        {
            if (!ev.Allowed)
                return;

            Scp096Damage(ev);
            By096Damaged(ev);
        }

        static void By096Damaged(DamageEvent ev)
        {
            if (ev.Attacker.RoleInformation.Role != RoleTypeId.Scp096)
                return;

            if (ev.Attacker.Variables.ContainsKey(CellModelPlayerTag))
                goto IL_001;

            if (!ev.Attacker.RoleInformation.Scp096.TargetsTracker.HasTarget(ev.Target.ReferenceHub))
                goto IL_001;

            return;

        IL_001:
            {
                ev.Damage = 0;
                ev.Allowed = false;
            }
        }

        static void Scp096Damage(DamageEvent ev)
        {
            if (ev.Target.RoleInformation.Role != RoleTypeId.Scp096)
                return;

            if (ev.LiteType is LiteDamageTypes.Disruptor
                or LiteDamageTypes.Recontainment
                or LiteDamageTypes.Custom)
                return;

            if (ev.LiteType is LiteDamageTypes.Universal)
            {
                ev.Damage = 0;
                return;
            }

            if (ev.Damage == -1)
                return;

            ev.Damage = Random.Range(0, 10) % 2;
        }
    }
}
#endif