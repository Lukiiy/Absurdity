using System;
using HarmonyLib;
using Zorro.Core;

namespace Absurdity;

[HarmonyPatch(typeof(Dynamite), nameof(Dynamite.OnInstanceDataSet))]
public static class Dynabuff
{
    private static readonly Random random = new();

    [HarmonyPostfix]
    private static void Postfix(Dynamite __instance)
    {
        if (Singleton<MapHandler>.Instance.GetCurrentBiome() != Biome.BiomeType.Mesa || random.NextDouble() > .75f) return;

        __instance.startingFuseTime *= .8f;
    }
}