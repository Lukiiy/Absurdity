using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Zorro.Core;
using Peak.Afflictions;

namespace Absurdity;

internal sealed class CampfireGhost
{
    [HarmonyPatch(typeof(MountainProgressHandler), nameof(MountainProgressHandler.TriggerReached))]
    private static class OnBiomeReached
    {
        private static void Postfix(MountainProgressHandler.ProgressPoint progressPoint)
        {
            if (!Plugin.CampGhost.Value || !PhotonNetwork.IsMasterClient || progressPoint == null || progressPoint.biome == Biome.BiomeType.Shore || progressPoint.biome == Biome.BiomeType.Peak) return;

            Plugin.Instance.StartCoroutine(SpawnGhostWhenVulnerable());
        }
    }

    private static IEnumerator<object?> SpawnGhostWhenVulnerable()
    {
        List<Character> players = Utils.GetActivePlayers();
        while (players == null || players.Count == 0)
        {
            yield return new WaitForSeconds(.5f);

            players = Utils.GetActivePlayers();
        }

        bool isProtected = true;
        while (isProtected)
        {
            players = Utils.GetActivePlayers();

            bool iseveryoneresting = Singleton<OrbFogHandler>.Instance != null && Singleton<OrbFogHandler>.Instance.PlayersAreResting;
            bool isCampfireProt = players.Any(player => player != null && player.refs != null && player.refs.afflictions != null && player.refs.afflictions.HasAfflictionType(Peak.Afflictions.Affliction.AfflictionType.NoHunger, out Affliction a));

            isProtected = iseveryoneresting || isCampfireProt;

            if (isProtected) yield return new WaitForSeconds(.5f);
        }

        PhotonNetwork.Instantiate("GhostBall", Utils.GetPlayerCenter(players) + Vector3.up * 8f, Quaternion.identity);
    }
}