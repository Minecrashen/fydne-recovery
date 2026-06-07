using MEC;
using PlayerRoles;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;

#if MRP
using LabApi.Features.Wrappers;
using Loli.Addons.RolePlay;
using UnityEngine;
#endif

namespace Loli.Addons
{
    static class Spawn
    {
        [EventMethod(PlayerEvents.Spawn)]
        static void Update(SpawnEvent ev)
        {
            if (ev.Player.Disconnected)
                return;

            if (ev.Role is not RoleTypeId.Scp939)
                ev.Player.Effects.DisableAll();

            ev.Player.UserInformation.CustomInfo = "";
            ev.Player.UserInformation.InfoToShow = PlayerInfoArea.Nickname | PlayerInfoArea.Badge | PlayerInfoArea.CustomInfo |
                PlayerInfoArea.Role | PlayerInfoArea.UnitName | PlayerInfoArea.PowerStatus;

            Timing.CallDelayed(0.2f, () =>
            {
#if NR
                var hp = ev.Player.GetMaxHp();
                if (hp > 0)
                {
                    ev.Player.HealthInformation.MaxHp = (int)hp;
                    ev.Player.HealthInformation.Hp = hp;
                }
#endif

                switch (ev.Player.RoleInformation.Role)
                {
                    case RoleTypeId.Tutorial: break;
                    case RoleTypeId.ClassD:
                        {
                            ev.Player.Inventory.Clear();
                            ev.Player.GetAmmo();

#if NR
                            ev.Player.Inventory.AddItem(ItemType.KeycardJanitor);
#endif

                            ev.Player.Inventory.AddItem(ItemType.Lantern);

#if MRP
							float scale = Random.Range(0.8f, 1.1f);
							ev.Player.MovementState.Scale = new(scale, scale, scale);
#endif
                            break;
                        }
                    case RoleTypeId.Scientist:
                        {
                            ev.Player.Inventory.Clear();
                            ev.Player.GetAmmo();

#if NR
                            ev.Player.Inventory.AddItem(ItemType.KeycardScientist);
#endif

                            ev.Player.Inventory.AddItem(ItemType.Medkit);
                            ev.Player.Inventory.AddItem(ItemType.Radio);
                            ev.Player.Inventory.AddItem(ItemType.Flashlight);

#if MRP
							float scale = Random.Range(0.88f, 1.1f);
							ev.Player.MovementState.Scale = new(scale, scale, scale);
#endif
                            break;
                        }
                    case RoleTypeId.FacilityGuard:
                        {
                            ev.Player.Inventory.Clear();
                            ev.Player.GetAmmo();

#if MRP
							ev.Player.Inventory.AddItem(ItemType.KeycardGuard);
#elif NR
                            ev.Player.Inventory.AddItem(ItemType.KeycardMTFPrivate);
#endif

                            ev.Player.Inventory.AddItem(ItemType.GunFSP9);
                            ev.Player.Inventory.AddItem(ItemType.Medkit);

#if NR
                            ev.Player.Inventory.AddItem(ItemType.Medkit);
#endif

                            ev.Player.Inventory.AddItem(ItemType.Radio);

#if MRP
							ev.Player.Inventory.AddItem(ItemType.GrenadeFlash);
#elif NR
                            ev.Player.Inventory.AddItem(ItemType.GrenadeHE);
#endif

                            ev.Player.Inventory.AddItem(ItemType.Flashlight);
                            ev.Player.Inventory.AddItem(ItemType.ArmorLight);

#if MRP
							if (Random.Range(0, 100) < 50)
								ev.Player.Inventory.AddItem(ItemType.Medkit);
							else
								ev.Player.AddCuff();


							float scale = Random.Range(0.88f, 1.1f);
							ev.Player.MovementState.Scale = new(scale, scale, scale);
#endif
                            break;
                        }
                    case RoleTypeId.ChaosConscript:
                        {
                            ev.Player.Inventory.Clear();
                            ev.Player.GetAmmo();
                            ev.Player.Inventory.AddItem(ItemType.KeycardChaosInsurgency);
                            ev.Player.Inventory.AddItem(ItemType.GunAK);
                            ev.Player.Inventory.AddItem(ItemType.GunA7);
                            ev.Player.Inventory.AddItem(ItemType.Medkit);
                            ev.Player.Inventory.AddItem(ItemType.Painkillers);
                            ev.Player.Inventory.AddItem(ItemType.GrenadeFlash);
                            ev.Player.Inventory.AddItem(ItemType.Lantern);
                            ev.Player.Inventory.AddItem(ItemType.ArmorCombat);

#if MRP
							float scale = Random.Range(0.88f, 1.1f);
							ev.Player.MovementState.Scale = new(scale, scale, scale);
#endif
                            break;
                        }
                    case RoleTypeId.ChaosMarauder:
                        {
                            ev.Player.Inventory.Clear();
                            ev.Player.GetAmmo();
                            ev.Player.Inventory.AddItem(ItemType.KeycardChaosInsurgency);
                            ev.Player.Inventory.AddItem(ItemType.GunLogicer);
                            ev.Player.Inventory.AddItem(ItemType.GunA7);
                            ev.Player.Inventory.AddItem(ItemType.Medkit);
                            ev.Player.Inventory.AddItem(ItemType.Adrenaline);
                            ev.Player.Inventory.AddItem(ItemType.GrenadeHE);
                            ev.Player.Inventory.AddItem(ItemType.Lantern);
                            ev.Player.Inventory.AddItem(ItemType.ArmorHeavy);

#if MRP
							float scale = Random.Range(0.88f, 1.1f);
							ev.Player.MovementState.Scale = new(scale, scale, scale);
#endif
                            break;
                        }
                    case RoleTypeId.ChaosRepressor:
                        {
                            ev.Player.Inventory.Clear();
                            ev.Player.GetAmmo();
                            ev.Player.Inventory.AddItem(ItemType.KeycardChaosInsurgency);
                            ev.Player.Inventory.AddItem(ItemType.GunShotgun);
                            ev.Player.Inventory.AddItem(ItemType.GunRevolver);
                            ev.Player.Inventory.AddItem(ItemType.Medkit);

#if NR
                            ev.Player.Inventory.AddItem(ItemType.Painkillers);
#endif

                            ev.Player.Inventory.AddItem(ItemType.GrenadeFlash);
                            ev.Player.Inventory.AddItem(ItemType.Lantern);
                            ev.Player.Inventory.AddItem(ItemType.ArmorCombat);

#if MRP
							if (Random.Range(0, 100) < 50)
								ev.Player.Inventory.AddItem(ItemType.Painkillers);
							else
							{
								Item cuff = ev.Player.AddCuff();

								if (Random.Range(0, 100) % 2 == 0)
									cuff.AddRandomCuffAbility();
							}

							float scale = Random.Range(0.88f, 1.1f);
							ev.Player.MovementState.Scale = new(scale, scale, scale);
#endif
                            break;
                        }
                    case RoleTypeId.ChaosRifleman:
                        {
                            ev.Player.Inventory.Clear();
                            ev.Player.GetAmmo();
                            ev.Player.Inventory.AddItem(ItemType.KeycardChaosInsurgency);
                            ev.Player.Inventory.AddItem(ItemType.GunAK);
                            ev.Player.Inventory.AddItem(ItemType.GunCOM18);
                            ev.Player.Inventory.AddItem(ItemType.Medkit);

#if NR
                            ev.Player.Inventory.AddItem(ItemType.Painkillers);
#endif

                            ev.Player.Inventory.AddItem(ItemType.GrenadeHE);
                            ev.Player.Inventory.AddItem(ItemType.Lantern);
                            ev.Player.Inventory.AddItem(ItemType.ArmorCombat);

#if MRP
							if (Random.Range(0, 100) < 50)
								ev.Player.Inventory.AddItem(ItemType.Painkillers);
							else
							{
								Item cuff = ev.Player.AddCuff();

								if (Random.Range(0, 100) % 2 == 0)
									cuff.AddRandomCuffAbility();
							}

							float scale = Random.Range(0.88f, 1.1f);
							ev.Player.MovementState.Scale = new(scale, scale, scale);
#endif
                            break;
                        }
                    case RoleTypeId.NtfPrivate:
                        {
                            ev.Player.Inventory.Clear();
                            ev.Player.GetAmmo();
                            ev.Player.Inventory.AddItem(ItemType.KeycardMTFOperative);
                            ev.Player.Inventory.AddItem(ItemType.GunCrossvec);
                            ev.Player.Inventory.AddItem(ItemType.Radio);
                            ev.Player.Inventory.AddItem(ItemType.Medkit);

#if NR
                            ev.Player.Inventory.AddItem(ItemType.Painkillers);
#endif

                            ev.Player.Inventory.AddItem(ItemType.GrenadeFlash);
                            ev.Player.Inventory.AddItem(ItemType.Flashlight);
                            ev.Player.Inventory.AddItem(ItemType.ArmorCombat);

#if MRP
							ev.Player.AddCuff();

							float scale = Random.Range(0.88f, 1.1f);
							ev.Player.MovementState.Scale = new(scale, scale, scale);
#endif
                            break;
                        }
                    case RoleTypeId.NtfCaptain:
                        {
                            ev.Player.Inventory.Clear();
                            ev.Player.GetAmmo();
                            ev.Player.Inventory.AddItem(ItemType.KeycardMTFCaptain);
                            ev.Player.Inventory.AddItem(ItemType.GunE11SR);

#if NR
                            ev.Player.Inventory.AddItem(ItemType.GunRevolver);
#endif

                            ev.Player.Inventory.AddItem(ItemType.GunCOM18);
                            ev.Player.Inventory.AddItem(ItemType.Radio);
                            ev.Player.Inventory.AddItem(ItemType.GrenadeHE);
                            ev.Player.Inventory.AddItem(ItemType.Medkit);
                            ev.Player.Inventory.AddItem(ItemType.Flashlight);
                            ev.Player.Inventory.AddItem(ItemType.ArmorCombat);

#if MRP
							float scale = Random.Range(0.88f, 1.1f);
							ev.Player.MovementState.Scale = new(scale, scale, scale);
#endif
                            break;
                        }
                    case RoleTypeId.NtfSergeant:
                        {
                            ev.Player.Inventory.Clear();
                            ev.Player.GetAmmo();
                            ev.Player.Inventory.AddItem(ItemType.KeycardMTFOperative);
                            ev.Player.Inventory.AddItem(ItemType.GunE11SR);

#if NR
                            ev.Player.Inventory.AddItem(ItemType.GunCOM18);
#endif

                            ev.Player.Inventory.AddItem(ItemType.Radio);
                            ev.Player.Inventory.AddItem(ItemType.GrenadeHE);
                            ev.Player.Inventory.AddItem(ItemType.Medkit);
                            ev.Player.Inventory.AddItem(ItemType.Flashlight);
                            ev.Player.Inventory.AddItem(ItemType.ArmorCombat);

#if MRP
							Item cuff = ev.Player.AddCuff();

							if (Random.Range(0, 100) % 2 == 0)
								cuff.AddRandomCuffAbility();

							float scale = Random.Range(0.88f, 1.1f);
							ev.Player.MovementState.Scale = new(scale, scale, scale);
#endif
                            break;
                        }
                    case RoleTypeId.NtfSpecialist:
                        {
                            ev.Player.Inventory.Clear();
                            ev.Player.GetAmmo();
                            ev.Player.Inventory.AddItem(ItemType.KeycardMTFCaptain);
                            ev.Player.Inventory.AddItem(ItemType.GunE11SR);

#if MRP
							ev.Player.Inventory.AddItem(ItemType.Jailbird);
#elif NR
                            ev.Player.Inventory.AddItem(ItemType.SCP330);
#endif

                            ev.Player.Inventory.AddItem(ItemType.Radio);
                            ev.Player.Inventory.AddItem(ItemType.GrenadeHE);
                            ev.Player.Inventory.AddItem(ItemType.Medkit);
                            ev.Player.Inventory.AddItem(ItemType.Flashlight);
                            ev.Player.Inventory.AddItem(ItemType.ArmorHeavy);

#if MRP
							float scale = Random.Range(0.88f, 1.1f);
							ev.Player.MovementState.Scale = new(scale, scale, scale);
#endif
                            break;
                        }
                }
            });
        }
    }
}