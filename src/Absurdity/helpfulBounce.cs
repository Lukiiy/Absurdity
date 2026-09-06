using HarmonyLib;

namespace Absurdity;

[HarmonyPatch(typeof(CollisionModifier), "Awake")]
internal static class HelpfulBounces
{
    private const float Multi = 1.75f;
 
    private static void Postfix(CollisionModifier __instance)
    {
        if (!Plugin.helpfulBounces.Value) return;
        if (__instance.GetComponentInParent<Character>() == null) return;
 
        __instance.knockback *= Multi;
        __instance.ragdolledKnockback *= Multi;
    }
}
