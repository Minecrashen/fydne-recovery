#if MRP
using Loli.HintsCore;
using Loli.HintsCore.Utils;
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
    static class Scp173Rework
    {
        const string CellProcessPlayerTag = "Scp173Cell_PlayerHas_Process";
        const string CellModelPlayerTag = "Scp173Cell_PlayerHas_Model";
        const string BlockTag = "Loli.Addons.RolePlay.Scp173Rework.Block";
        static readonly HashSet<Player> Scp173List = new();
        static readonly HashSet<ushort> RealCuffsWithCell = new();

        static internal void GetCell173(this Item item)
        {
            RealCuffsWithCell.Add(item.Serial);
        }

        static internal bool CheckString(ushort serial, out string str)
        {
            if (RealCuffsWithCell.Contains(serial))
            {
                str = "<color=#a02729>Клетка SCP-173 в наличии</color>";
                return true;
            }

            str = string.Empty;
            return false;
        }

        static internal void RemoveBlock(Player pl, Player scp)
        {
            if (!scp.Variables.TryGetAndParse(BlockTag, out DisplayBlock block))
                return;

            RemoveBlock(pl, block);
        }

        static internal void AddBlock(Player pl, Player scp)
        {
            if (!scp.Variables.TryGetAndParse(BlockTag, out DisplayBlock block))
                return;

            AddBlock(pl, block);
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

            if (block.Contents.Count < 2)
                return;

            block.Contents[0].Content = $"{pl.RoleInformation.Scp173.Observers.CurrentObservers}/4";
            block.Contents[1].Content = $"{pl.Variables[CellProcessPlayerTag]}/30";
        }

        static internal void ResetBlock(Player pl)
        {
            if (!pl.Variables.TryGetAndParse(BlockTag, out DisplayBlock block))
                return;

            pl.Variables.Remove(BlockTag);
            block.Contents.Clear();

            foreach (Player obs in Player.List)
                RemoveBlock(obs, block);

            /*
            if (pl.RoleInformation.Role is RoleTypeId.Scp173)
            {
                RemoveBlock(pl, block);

                foreach (ReferenceHub obs in pl.RoleInformation.Scp173.Observers.Observers)
                    RemoveBlock(obs.GetPlayer(), block);
            }
            else
            {
                foreach (Player obs in Player.List)
                    RemoveBlock(obs, block);
            }
            */
        }

        static internal void CreateBlock(Player pl)
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

            goto IL_001;

        IL_001:
            {
                block.Contents.Add(new("0/4", new Color32(70, 185, 74, 255), "75%"));
                block.Contents.Add(new("0/30", new Color32(70, 185, 74, 255), "75%"));
                block.Contents.Add(new("Прогресс ВОУС SCP-173:", new Color32(70, 185, 74, 255), "75%"));
                pl.Variables[BlockTag] = block;
            }
        }

        static internal void Process(Player pl, Player target, JailbirdItem jail)
        {
            if (!RealCuffsWithCell.Contains(jail.ItemSerial))
            {
                pl.Client.ShowHint("<color=#ff9300>В этом связывающем устройстве нет клетки для SCP-173</color>", 7);
                return;
            }
            if (target.RoleInformation.Scp173.Observers.CurrentObservers < 4)
            {
                pl.Client.ShowHint("<color=#ff9300>Необходимо 4 человека, чтобы совершить ВОУС SCP-173</color>", 7);
                return;
            }

            if (target.Variables.ContainsKey(CellProcessPlayerTag))
            {
                pl.Client.ShowHint("<color=#ff9300>Прогресс ВОУС SCP-173 уже начат</color>", 7);
                return;
            }

            if (target.Variables.ContainsKey(CellModelPlayerTag))
            {
                pl.Client.ShowHint("<color=#ff9300>Клетка уже активирована</color>", 7);
                return;
            }

            target.Variables[CellProcessPlayerTag] = 30;
            CreateBlock(target);
        }

        static internal void PutCell(Player target)
        {
            if (target.Variables.TryGetAndParse(CellModelPlayerTag, out Model cell2))
                try { cell2.Destroy(); } catch { }

            Model cell = new("Cell_" + target.UserInformation.UserId, Vector3.zero, Vector3.zero);
            cell.GameObject.transform.parent = target.Transform;
            cell.GameObject.transform.localPosition = Vector3.down;
            cell.GameObject.transform.localRotation = Quaternion.identity;

            Color32 bottomColor = new(60, 62, 61, 255);
            cell.AddPart(prim: new(cell, PrimitiveType.Cylinder, bottomColor, Vector3.zero, Vector3.zero, new(2, 0.05f, 2), false));
            cell.AddPart(prim: new(cell, PrimitiveType.Cylinder, bottomColor, Vector3.up * 2.192f, Vector3.zero, new(2, 0.05f, 2), false));

            cell.AddPart(prim: new(cell, PrimitiveType.Cylinder, Color.red, new(0, 1.08f, 0.91f), Vector3.zero, new(0.1f, 1.1f, 0.1f), false));
            cell.AddPart(prim: new(cell, PrimitiveType.Cylinder, Color.red, new(0, 1.08f, -0.91f), Vector3.zero, new(0.1f, 1.1f, 0.1f), false));
            cell.AddPart(prim: new(cell, PrimitiveType.Cylinder, Color.red, new(-0.91f, 1.08f, 0), Vector3.zero, new(0.1f, 1.1f, 0.1f), false));
            cell.AddPart(prim: new(cell, PrimitiveType.Cylinder, Color.red, new(0.91f, 1.08f, 0), Vector3.zero, new(0.1f, 1.1f, 0.1f), false));

            cell.AddPart(prim: new(cell, PrimitiveType.Cylinder, Color.red, new(0.637f, 1.08f, 0.622f), Vector3.zero, new(0.1f, 1.1f, 0.1f), false));
            cell.AddPart(prim: new(cell, PrimitiveType.Cylinder, Color.red, new(-0.637f, 1.08f, 0.622f), Vector3.zero, new(0.1f, 1.1f, 0.1f), false));
            cell.AddPart(prim: new(cell, PrimitiveType.Cylinder, Color.red, new(-0.637f, 1.08f, -0.622f), Vector3.zero, new(0.1f, 1.1f, 0.1f), false));
            cell.AddPart(prim: new(cell, PrimitiveType.Cylinder, Color.red, new(0.637f, 1.08f, -0.622f), Vector3.zero, new(0.1f, 1.1f, 0.1f), false));

            cell.Primitives.ForEach(prim =>
            {
                prim.Primitive.MovementSmoothing = 64;
            });

            target.Variables[CellModelPlayerTag] = cell;
            target.RoleInformation.Scp173.Observers.UpdateObservers();
        }

        static internal void Cycle1s()
        {
            foreach (Player scp in Scp173List)
            {
                if (!scp.Variables.TryGetAndParse(CellProcessPlayerTag, out int time))
                    continue;

                if (scp.RoleInformation.Scp173.Observers.CurrentObservers < 4)
                    time++;
                else
                    time--;

                if (time >= 30)
                {
                    scp.Variables.Remove(CellProcessPlayerTag);
                    ResetBlock(scp);
                    continue;
                }

                if (time < 1)
                {
                    scp.Variables.Remove(CellProcessPlayerTag);
                    ResetBlock(scp);
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
                if (pl.RoleInformation.Role is RoleTypeId.Scp173)
                {
                    Scp173List.Add(pl);
                }
                else
                {
                    Scp173List.Remove(pl);
                    pl.Variables.Remove(CellProcessPlayerTag);
                    ResetBlock(pl);
                }
            }
        }


        [EventMethod(ScpEvents.Scp173AddObserver, int.MinValue)]
        static void HintsAddObserver(Scp173AddObserverEvent ev)
        {
            if (!ev.Allowed)
                return;

            if (!ev.Scp.Variables.ContainsKey(CellProcessPlayerTag))
                return;

            AddBlock(ev.Target, ev.Scp);
        }

        [EventMethod(ScpEvents.Scp173RemovedObserver)]
        static void HintsRemoveObserver(Scp173RemovedObserverEvent ev)
        {
            if (!ev.Scp.Variables.ContainsKey(CellProcessPlayerTag))
                return;

            RemoveBlock(ev.Target, ev.Scp);
        }

        [EventMethod(RoundEvents.Waiting)]
        static void ClearCache()
        {
            RealCuffsWithCell.Clear();
            Scp173List.Clear();
        }

        [EventMethod(PlayerEvents.InteractDoor, -10)]
        static void InteractDoor(InteractDoorEvent ev)
        {
            if (!ev.Allowed)
                return;

            if (ev.Player.RoleInformation.Role is not RoleTypeId.Scp173)
                return;

            if (!ev.Player.Variables.ContainsKey(CellModelPlayerTag))
                return;

            ev.Allowed = false;
        }

        [EventMethod(ScpEvents.Scp173EnableSpeed)]
        static void EnableSpeed(Scp173EnableSpeedEvent ev)
        {
            if (!ev.Active)
                return;

            ev.Allowed = false;
        }

        [EventMethod(ScpEvents.Scp173AddObserver)]
        static void AddObserver(Scp173AddObserverEvent ev)
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

            Scp173Damage(ev);
            By173Damaged(ev);
        }

        static void By173Damaged(DamageEvent ev)
        {
            if (ev.Attacker.RoleInformation.Role != RoleTypeId.Scp173)
                return;

            if (!ev.Attacker.Variables.ContainsKey(CellModelPlayerTag))
                return;

            ev.Damage = 0;
            ev.Allowed = false;
        }

        static void Scp173Damage(DamageEvent ev)
        {
            if (ev.Target.RoleInformation.Role != RoleTypeId.Scp173)
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