using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using Zorro.Core;

namespace Absurdity;

[HarmonyPatch(typeof(LavaRising), "Update")]
public static class FastRise
{
    [HarmonyPrefix]
    private static void Prefix(LavaRising __instance, ref float __state)
    {
        __state = __instance.travelTime;

        if (!PhotonNetwork.IsMasterClient || __instance.ended || Singleton<MapHandler>.Instance == null || Singleton<MapHandler>.Instance.GetCurrentSegment() != __instance.requiredSegment) return;

        float highestPlayerY = float.MinValue;

        foreach (Character character in Utils.GetActivePlayers())
        {
            if (!character.data.dead && character.data.fullyConscious) highestPlayerY = Mathf.Max(highestPlayerY, character.Center.y);
        }

        if (highestPlayerY == float.MinValue) return;

        float gap = highestPlayerY - __instance.lava.transform.position.y;

        if (!__instance.started && gap > 25f) __instance.secondsWaitedToStart = __instance.initialWaitTime + 1f;
        if (!__instance.started) return;

        float multiplier = gap switch
        {
            > 100f => 2.25f,
            > 70f => 2f,
            > 40f => 1.25f,
            _ => 1f
        };

        if (multiplier > 1f) __instance.travelTime = Mathf.Max(5f, __state / multiplier);
    }

    [HarmonyPostfix]
    private static void Postfix(LavaRising __instance, float __state) => __instance.travelTime = __state;
}