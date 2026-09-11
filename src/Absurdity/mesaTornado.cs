using System.Collections.Generic;
using HarmonyLib;

namespace Absurdity;

[HarmonyPatch(typeof(Tornado), "Start")]
internal static class MesaTornado
{
    private static void Postfix(Tornado __instance)
    {
        if (!Utils.IsBiome(Biome.BiomeType.Mesa) || UnityEngine.Random.value >= .25f) return;

        List<Character> players = Utils.GetActivePlayers();
        if (players.Count == 0) return;

        __instance.target = players[UnityEngine.Random.Range(0, players.Count)].transform;
    }
}
