#if MRP
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using Loli.Modules.Voices;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using Qurre.API;
using Qurre.API.Addons.Models;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.API.Objects;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Collections.Generic;
using System.IO;
using Qurre.API.World;
using UnityEngine;

namespace Loli.Addons.RolePlay
{
    static class Scp106Rework
    {
        const string CoroutineName = "Scp106Revork_Recontain_Coroutine";
        static string AudioPath { get; } = Path.Combine(Path.Combine(Paths.Plugins, "AudioStatic"), "Scp106Recontain.raw");

        static bool Clicked { get; set; }
        static bool Recontained { get; set; }
        static bool PeopleIn { get; set; }
        static ModelPrimitive Door { get; set; }
        static Model ModelRecontain { get; set; }

        static void LoadMap()
        {
            Room room = Map.Rooms.Find(x => x.RoomName == MapGeneration.RoomName.Hcz106);
            Model room_model = new("Room", room.Position, room.Rotation.eulerAngles);
            Model model = new("Scp106Recontain", new Vector3(22.039f, 2.613f, -10.172f), root: room_model);
            model.AddPart(new ModelPrimitive(model, PrimitiveType.Cube, new Color32(33, 27, 27, 255), new(0.1f, 2.1f), Vector3.zero, new(0.1f, 5.2f, 1.4f)));
            model.AddPart(new ModelPrimitive(model, PrimitiveType.Cube, new Color32(33, 27, 27, 255), new(0.549f, 1.0159f, 0.655f), new(0, 90), new(0.1f, 7.5f, 1)));
            model.AddPart(new ModelPrimitive(model, PrimitiveType.Cube, new Color32(33, 27, 27, 255), new(0.549f, 1.0159f, -0.655f), new(0, 90), new(0.1f, 7.5f, 1)));

            ModelPrimitive Checker = new(model, PrimitiveType.Cube, Color.yellow, new(0.621f, -2.723f), Vector3.zero, new(1, 0.1f, 1.3f));
            Door = new(model, PrimitiveType.Cube, Color.red, new(0.098f, 0.7f), Vector3.zero, new(0.08f, 2.3f, 1.3f)); // 0.7f -> -1.6f
            Door.Primitive.MovementSmoothing = 60;

            Model panel = new("Panel", new(-3.398f, -2.224f, 0.404f), Vector3.zero, model);
            model.AddPart(new ModelPrimitive(panel, PrimitiveType.Cube, Color.red, new(0, 0.74f), Vector3.zero, new(0.13f, 0.44f, 1.6f), false));

            ModelWorkStation Button = new(panel, new(0, 0.55f), new(0, 90), new(0.64f, 0.4f, 0.05f));

            StationsManager.Register(Button.WorkStation, Interact);

            Checker.GameObject.AddComponent<OnEnterScript>();

            model.AddPart(Checker);
            model.AddPart(Door);
            model.AddPart(Button);

            ModelRecontain = model;
        }

        static void SetupDoors()
        {
            foreach (Door door in Map.Doors)
            {
                if (door.Type is DoorType.Hcz106First or DoorType.Hcz106Second)
                {
                    door.Permissions = door.Permissions with
                    {
                        RequiredPermissions = DoorPermissionFlags.ContainmentLevelThree
                    };
                    if (door.DoorVariant is CheckpointDoor check)
                    {
                        foreach (var sub in check.SubDoors)
                        {
                            if (sub is BreakableDoor breakable)
                                breakable.IgnoredDamageSources = DoorDamageType.Grenade | DoorDamageType.Scp096 | DoorDamageType.Weapon;
                        }
                    }
                }
            }
        }

        static void Interact(InteractWorkStationEvent ev)
        {
            ev.Allowed = false;

            if (Clicked)
                return;

            Clicked = true;

            if (!PeopleIn)
            {
                Clicked = false;
                return;
            }

            if (ev.Player.RoleInformation.Team == Team.SCPs)
            {
                Clicked = false;
                return;
            }

            ev.Station.Status = WorkstationStatus.PoweringDown;

            Timing.RunCoroutine(Coroutine(), CoroutineName);

            static IEnumerator<float> Coroutine()
            {
                VoiceCore.PlayInIntercom(AudioPath, "??????");

                yield return Timing.WaitForSeconds(28f);

                Recontained = true;
                foreach (var pl in Player.List)
                {
                    if (pl.RoleInformation.Role is not RoleTypeId.Scp106)
                        continue;

                    pl.HealthInformation.Kill(DeathTranslations.Recontained);
                }

                yield break;
            }
        }

        static IEnumerator<float> Сycle()
        {
            for (; ; )
            {
                foreach (var pl in Player.List)
                {
                    if (pl.RoleInformation.Role != RoleTypeId.Scp106)
                        continue;

                    pl.Effects.Enable(EffectType.Sinkhole);
                }

                yield return Timing.WaitForSeconds(5f);
            }
        }


        [EventMethod(RoundEvents.Waiting)]
        static void Waiting()
        {
            Clicked = false;
            Recontained = false;
            PeopleIn = false;
            Door = null;
            ModelRecontain = null;
            LoadMap();
            SetupDoors();
            Timing.KillCoroutines(CoroutineName);
            Timing.RunCoroutine(Сycle(), CoroutineName);
        }

        [EventMethod(RoundEvents.Restart)]
        static void Restart()
        {
            Timing.KillCoroutines(CoroutineName);
        }

        [EventMethod(PlayerEvents.ChangeRole)]
        static void Spawn(ChangeRoleEvent ev)
        {
            if (ev.Role == RoleTypeId.Scp106 && Recontained)
            {
                ev.Allowed = false;
            }
        }

        [EventMethod(MapEvents.DamageDoor)]
        static void DamageDoor(DamageDoorEvent ev)
        {
            if (ev.Door.Type is DoorType.Hcz106First or DoorType.Hcz106Second)
            {
                ev.Damage = 0;
                ev.Allowed = false;
            }
        }

        [EventMethod(MapEvents.OpenDoor)]
        static void OpenDoor(OpenDoorEvent ev)
        {
            if (ev.Door.Type is DoorType.Hcz106First or DoorType.Hcz106Second)
            {
                ev.Allowed = false;
            }
        }

        [EventMethod(PlayerEvents.Damage, 1)]
        static void Damage(DamageEvent ev)
        {
            if (!ev.Allowed)
                return;

            Scp106Damage(ev);
            By106Damaged(ev);
        }

        static void Scp106Damage(DamageEvent ev)
        {
            if (ev.Target.RoleInformation.Role != RoleTypeId.Scp106)
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

        static void By106Damaged(DamageEvent ev)
        {
            if (ev.Attacker.RoleInformation.Role != RoleTypeId.Scp106)
                return;

            ev.Target.Effects.Enable(EffectType.Sinkhole, 30);
        }


        public class OnEnterScript : MonoBehaviour
        {
            private void Start()
            {
                var collider = gameObject.AddComponent<BoxCollider>();
                collider.isTrigger = true;
                collider.size = new(1, 22, 1);
                collider.center = Vector3.up * 11;
            }

            private void OnTriggerEnter(Collider other)
            {
                Log.Info(other.gameObject.name);
                if (!other.gameObject.name.Contains("Player"))
                    return;

                Player pl = other.gameObject.GetPlayer();

                if (pl.RoleInformation.Team == Team.SCPs)
                    return;

                pl.Inventory.Clear();
                Timing.CallDelayed(0.1f, () => pl.HealthInformation.Kill(DeathTranslations.Crushed));

                if (PeopleIn)
                    return;

                PeopleIn = true;

                Timing.RunCoroutine(DoorAnim(), CoroutineName);
                ModelRecontain.AddPart(new ModelPrimitive(ModelRecontain, PrimitiveType.Cube, new Color32(0, 0, 0, 0), new(0.098f, -1.6f), Vector3.zero, new(0.08f, 2.3f, 1.3f)));

                static IEnumerator<float> DoorAnim()
                {
                    Door.GameObject.transform.localPosition = new(0.098f, 0.7f);
                    for (float i = 0.7f; i > -1.6f; i -= 0.05f)
                    {
                        Door.GameObject.transform.localPosition = new(0.098f, i);
                        yield return Timing.WaitForSeconds(0.05f);
                    }
                    Door.GameObject.transform.localPosition = new(0.098f, -1.6f);

                    yield break;
                }
            }
        }
    }
}
#endif