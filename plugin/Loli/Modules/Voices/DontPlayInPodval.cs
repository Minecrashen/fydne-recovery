using Loli.Builds.Models.Rooms;
using Qurre.API;
using Qurre.API.Addons.Audio.Objects;
using Qurre.API.Attributes;
using Qurre.Events;
using System.Collections.Generic;
using Qurre.API.Controllers;
using Qurre.API.World;
using UnityEngine;

namespace Loli.Modules.Voices
{
    internal class DontPlayInPodval : IAccessConditions
    {
        static readonly HashSet<string> Bypass = new();

        public virtual bool CheckRequirements(ReferenceHub hub)
        {
            return Bypass.Contains(hub.authManager.UserId);
        }

        static internal void CheckPlayers()
        {
            if (!Round.Started)
                return;

            Vector3 podval = AdminRoom.GetSpawnPoint();
            foreach (var pl in Player.List)
            {
                string userId = pl.UserInformation.UserId;

                if (Vector3.Distance(pl.MovementState.Position, podval) < 40f ||
                    pl.InPocket())
                {
                    Bypass.Add(userId);
                    continue;
                }

                Bypass.Remove(userId);
            }
        }

        [EventMethod(RoundEvents.Waiting)]
        static void ClearCache()
        {
            Bypass.Clear();
        }
    }
}