using HarmonyLib;
using Photon.Pun;

namespace Absurdity;

[HarmonyPatch(typeof(FrogTongue))]
internal static class Frog
{
    [HarmonyPatch(nameof(FrogTongue.CheckAllCharacters))]
    [HarmonyPrefix]
    private static void CheckPre(FrogTongue __instance, ref float __state)
    {
        __state = __instance.maxAngle;

        if (!PhotonNetwork.IsMasterClient) return;

        if (UnityEngine.Random.value < .45f) __instance.maxAngle = 180f;
    }

    [HarmonyPatch(nameof(FrogTongue.CheckAllCharacters))]
    [HarmonyPostfix]
    private static void CheckPost(FrogTongue __instance, float __state)
    {
        __instance.maxAngle = __state;

        if (!PhotonNetwork.IsMasterClient) return;

        __instance._attackCheckTimer *= .5f;
    }
}