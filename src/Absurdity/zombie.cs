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
    }

    [HarmonyPatch(typeof(MushroomZombie), "WakeUpFromSleep")]
    [HarmonyPostfix]
    private static void SleepPost(MushroomZombie __instance)
    {
        if (Random.value < .5f) __instance.currentState = MushroomZombie.State.Chasing;
    }
}