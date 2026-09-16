using HarmonyLib;

namespace Absurdity;

[HarmonyPatch(typeof(GhostFire), "RPC_SetLit")]
internal static class BellTower
{
    private static void Postfix(GhostFire __instance, bool lit)
    {
        if (!lit || __instance.photonView == null) return;

        __instance.statusProtectionRadius = 0f;
    }
}
