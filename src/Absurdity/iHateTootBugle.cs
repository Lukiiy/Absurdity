using HarmonyLib;

namespace Absurdity;

[HarmonyPatch(typeof(BugleEventProc), "ThrowBugleEvent")]
public static class iHateTootBugle
{
    [HarmonyPostfix]
    private static void Postfix(BugleEventProc __instance)
    {
        Item bugle = __instance.GetComponent<Item>();
        if (bugle == null) return;

        bugle.holderCharacter?.Fall(2f, 6f);
    }
}