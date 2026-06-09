using Mirror;
using UnityEngine;

namespace Loli.Builds.Models
{
    internal static class SafeNetwork
    {
        internal static bool Spawn(GameObject gameObject)
        {
            if (gameObject == null || !gameObject.TryGetComponent<NetworkIdentity>(out _))
                return false;

            try
            {
                NetworkServer.Spawn(gameObject);
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static bool UnSpawn(GameObject gameObject)
        {
            if (gameObject == null || !gameObject.TryGetComponent<NetworkIdentity>(out _))
                return false;

            try
            {
                NetworkServer.UnSpawn(gameObject);
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static void Destroy(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            if (gameObject.TryGetComponent<NetworkIdentity>(out _))
            {
                try
                {
                    NetworkServer.Destroy(gameObject);
                    return;
                }
                catch
                {
                }
            }

            try
            {
                Object.Destroy(gameObject);
            }
            catch
            {
            }
        }
    }
}
