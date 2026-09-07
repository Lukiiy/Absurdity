using HarmonyLib;

namespace Absurdity;

[HarmonyPatch(typeof(Beetle), "InflictAttack")]
internal static class BeetleTweaks
{
    private const float Multi = 2f;

    private static void Prefix(Beetle __instance)
    {
        if (!Plugin.BeetleTweaks.Value) return;

        __instance.bonkForce *= Multi;
        __instance.bonkForceUp *= Multi;
    }
}