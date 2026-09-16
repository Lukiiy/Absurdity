using HarmonyLib;

namespace Absurdity;

[HarmonyPatch(typeof(WindChillZone), "GetNextWindTime")]
internal static class WindEffect
{
    private static void Postfix(bool windActive, ref float __result)
    {
        if (windActive) __result *= 1.75f;

        if (!Utils.IsBiome(Biome.BiomeType.Alpine)) return; // leave alpine alone

        if (!windActive && UnityEngine.Random.value < .25f) __result *= UnityEngine.Random.Range(1.05f, 1.25f);// 25% chance to extend timer
        if (Utils.GetActivePlayers().Count <= 1) __result *= 1.15f;
    }
}