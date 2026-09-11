using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace Absurdity;

[HarmonyPatch(typeof(EruptionSpawner), "RPCA_SpawnEruption")]
internal static class ChainedEruptions
{
    volatile static bool chaining;

    private static void Postfix(EruptionSpawner __instance, Vector3 position)
    {
        if (chaining || !PhotonNetwork.IsMasterClient) return;

        PhotonView view = __instance.GetComponent<PhotonView>();
        if (view == null) return;

        chaining = true;

        try {
            view.RPC("RPCA_SpawnEruption", RpcTarget.All, RandomNearby(position));

            if (Random.value < .3f) view.RPC("RPCA_SpawnEruption", RpcTarget.All, RandomNearby(position)); // 30% chance to spawn a 3rd one :)
        } finally {
            chaining = false;
        }
    }

    private static Vector3 RandomNearby(Vector3 position)
    {
        Vector2 offset = Random.insideUnitCircle * 100f;

        return position + new Vector3(offset.x, 0f, offset.y);
    }
}
