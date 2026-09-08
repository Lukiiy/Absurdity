using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace Absurdity;

[HarmonyPatch(typeof(MushroomZombieSpawner), "Spawn")]
public static class Zombeee
{
    [HarmonyPostfix]
    private static void Postfix(MushroomZombieSpawner __instance)
    {
        if (!PhotonNetwork.IsMasterClient || UnityEngine.Random.value >= .5f || __instance.mushroomZombiePrefab == null) return;

        Vector3 offset = UnityEngine.Random.insideUnitCircle.normalized;

        PhotonNetwork.Instantiate(__instance.mushroomZombiePrefab.gameObject.name, __instance.transform.position + new Vector3(offset.x, 0f, offset.y) * 1.5f, __instance.transform.rotation,0);
    }
}