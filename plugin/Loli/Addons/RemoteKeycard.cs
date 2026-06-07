#if NR
// using Interactables.Interobjects.DoorUtils;
// using PlayerRoles;
// using Qurre.API.Attributes;
// using Qurre.API.Objects;
// using Qurre.Events;
// using Qurre.Events.Structs;
//
// namespace Loli.Addons
// {
// 	static class RemoteKeycard
// 	{
// 		[EventMethod(PlayerEvents.InteractDoor)]
// 		static internal void Door(InteractDoorEvent ev)
// 		{
// 			try
// 			{
// 				if (ev.Allowed)
// 					return;
//
// 				if (ev.Door?.Permissions?.RequiredPermissions == KeycardPermissions.None)
// 					return;
//
// 				if (ev.Player.RoleInformation.Team == Team.SCPs)
// 					return;
//
// 				foreach (var item in ev.Player.Inventory.Base.UserInventory.Items)
// 				{
// 					if (item.Value == null)
// 						continue;
//
// 					if (ev.Door.Permissions.CheckPermissions(item.Value, ev.Player.ReferenceHub))
// 					{
// 						ev.Allowed = true;
// 						return;
// 					}
// 				}
// 			}
// 			catch { }
// 		}
//
// 		[EventMethod(PlayerEvents.InteractLocker)]
// 		static internal void Locker(InteractLockerEvent ev)
// 		{
// 			if (ev.Allowed)
// 				return;
//
// 			if (ev.Player.RoleInformation.Team == Team.SCPs)
// 				return;
//
// 			if (ev.Locker.Type == LockerType.Pedestal)
// 			{
// 				bool b1 = false;
// 				bool b2 = false;
// 				foreach (var item in ev.Player.Inventory.Base.UserInventory.Items)
// 				{
// 					try
// 					{
// 						if (item.Value == null) continue;
// 						if (item.Value is InventorySystem.Items.Keycards.KeycardItem keycard)
// 						{
// 							if (keycard.Permissions.HasFlagFast(KeycardPermissions.ContainmentLevelTwo)) b1 = true;
// 							if (keycard.Permissions.HasFlagFast(KeycardPermissions.Checkpoints)) b2 = true;
// 						}
// 					}
// 					catch { }
// 				}
// 				if (b1 && b2) ev.Allowed = true;
// 			}
// 			else
// 			{
// 				foreach (var item in ev.Player.Inventory.Base.UserInventory.Items)
// 				{
// 					try
// 					{
// 						if (item.Value == null) continue;
// 						if (item.Value is InventorySystem.Items.Keycards.KeycardItem keycard)
// 						{
// 							if (keycard.Permissions.HasFlagFast(ev.Chamber.Permissions))
// 							{
// 								ev.Allowed = true;
// 								return;
// 							}
// 						}
// 					}
// 					catch { }
// 				}
// 			}
// 		}
//
// 		[EventMethod(PlayerEvents.InteractGenerator)]
// 		static internal void Generator(InteractGeneratorEvent ev)
// 		{
// 			if (ev.Status != GeneratorStatus.Unlock)
// 				return;
//
// 			ev.Allowed = false;
//
// 			if (ev.Player.RoleInformation.Team == Team.SCPs)
// 				return;
//
// 			foreach (var item in ev.Player.Inventory.Base.UserInventory.Items)
// 			{
// 				if (item.Value == null)
// 					continue;
// 				try
// 				{
// 					if (item.Value.ItemTypeId == ItemType.KeycardContainmentEngineer)
// 					{
// 						ev.Allowed = true;
// 						return;
// 					}
// 					if (item.Value is InventorySystem.Items.Keycards.KeycardItem keycard &&
// 						keycard.Permissions.HasFlagFast(KeycardPermissions.ArmoryLevelOne))
// 					{
// 						ev.Allowed = true;
// 						return;
// 					}
// 				}
// 				catch { }
// 			}
// 		}
// 	}
// }
#endif