using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using Zorro.Core;

namespace Absurdity;

[HarmonyPatch(typeof(LavaRising), "Update")]
public static class FastRise
{
    [HarmonyPrefix]
    private static void Prefix(LavaRising __instance)
    {
        if (!PhotonNetwork.IsMasterClient || __instance.ended || Singleton<MapHandler>.Instance == null || Singleton<MapHandler>.Instance.GetCurrentSegment() != __instance.requiredSegment) return;

        float highestPlayerY = float.MinValue;

        foreach (Character character in Utils.GetActivePlayers()) if (!character.data.dead && character.data.fullyConscious) highestPlayerY = Mathf.Max(highestPlayerY, character.Center.y);

        if (highestPlayerY == float.MinValue) return;

        if (!__instance.started && highestPlayerY - __instance.lava.transform.position.y > 25f) __instance.secondsWaitedToStart = __instance.initialWaitTime + 1f;
    }
}