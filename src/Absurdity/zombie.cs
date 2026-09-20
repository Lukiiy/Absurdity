using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace Absurdity;

[HarmonyPatch]
public static class Zombeee
{
    [HarmonyPatch(typeof(MushroomZombieSpawner), "Spawn")]
    [HarmonyPostfix]
    private static void SpawnPostfix(MushroomZombieSpawner __instance)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        MushroomZombie zombie = __instance.spawnedZombie;
        if (zombie == null) return;

        if (Random.value < .5f) zombie.currentState = MushroomZombie.State.Chasing;
        if (Random.value < .5) PhotonNetwork.Instantiate("MushroomZombie", zombie.character.refs.ragdoll.bodySpawnPoint.position, zombie.character.refs.ragdoll.bodySpawnPoint.rotation, 0);
    }
}