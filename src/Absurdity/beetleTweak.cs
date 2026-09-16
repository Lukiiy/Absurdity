using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace Absurdity;

[HarmonyPatch(typeof(Beetle), "InflictAttack")]
internal static class BeetleTweaks
{
    private const float Multi = 2f;

    private static bool Prefix(Beetle __instance, Character character)
    {
        if (!Plugin.BeetleTweaks.Value) return true; // vanilla

        Vector3 force = __instance.transform.forward * (__instance.bonkForce * Multi) + __instance.transform.up * (__instance.bonkForceUp * Multi);

        character.Fall(__instance.ragdollTime);
        character.AddForceAtPosition(force, __instance.bonkPoint.position, __instance.bonkRange);

        float ignore = UnityEngine.Random.Range(__instance.allBeetlesIgnoreAfterAttackTime.x, __instance.allBeetlesIgnoreAfterAttackTime.y);

        __instance.photonView.RPC("RPC_IgnorePlayerForTime", RpcTarget.All, character.photonView.Owner, ignore);

        return false;
    }
}