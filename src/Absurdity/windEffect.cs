using System.Collections.Generic;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace Absurdity;

[HarmonyPatch(typeof(WindChillZone))]
internal static class WindEffect
{
    [HarmonyPatch(nameof(WindChillZone.GetNextWindTime))]
    [HarmonyPostfix]
    private static void TimePost(bool windActive, ref float __result)
    {
        if (windActive) __result *= 1.25f;

        if (!Utils.IsBiome(Biome.BiomeType.Alpine)) return; // leave alpine alone

        if (!windActive && UnityEngine.Random.value < .25f) __result *= UnityEngine.Random.Range(1.05f, 1.25f);// 25% chance to extend timer
        if (Utils.GetActivePlayers().Count <= 1) __result *= 1.15f;
    }

    [HarmonyPatch(nameof(WindChillZone.RPCA_ToggleWind))]
    [HarmonyPostfix]
    private static void TogglePost(WindChillZone __instance, bool set)
    {
        if (!set || !PhotonNetwork.IsMasterClient || !Utils.IsBiome(Biome.BiomeType.Tropics) || UnityEngine.Random.value >= .3f) return; // first one means only when it starts

        List<Character> players = Utils.GetActivePlayers();
        if (players.Count == 0) return;

        int hitCount = Mathf.Min(players.Count, UnityEngine.Random.Range(1, 3));
        for (int i = 0; i < hitCount; i++)
        {
            Character player = players[UnityEngine.Random.Range(0, players.Count)];
            bool already = false;

            foreach (KeyValuePair<Bugfix, Character> attached in Bugfix.AllAttachedBugs)
            {
                if (attached.Value == player) {
                    already = true;

                    break;
                }
            }

            if (already) continue;

            PhotonNetwork.Instantiate("BugfixOnYou", Vector3.zero, Quaternion.identity, 0).GetComponent<Bugfix>().AttachBug(player.photonView.ViewID);
        }
    }
}