using HarmonyLib;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace Absurdity;

[HarmonyPatch(typeof(Campfire), "Light_Rpc")]
internal static class CampfireMobs
{
    private const float SpawnRadius = 8f;

    private static void Postfix(bool updateSegment)
    {
        if (!updateSegment || !PhotonNetwork.IsMasterClient) return;

        List<Character> players = Utils.GetActivePlayers();
        if (players.Count == 0) return;

        foreach (Character player in players)
        {
            Vector3 probe = player.Center + new Vector3(UnityEngine.Random.Range(-SpawnRadius, SpawnRadius), 0f, UnityEngine.Random.Range(-SpawnRadius, SpawnRadius));
            if (!Utils.TryGetGround(probe, out Vector3 ground)) continue;

            string prefab = UnityEngine.Random.value < .5f ? "Beetle" : "Scorpion";

            Utils.TrySpawnObject(prefab, ground, Quaternion.identity)?.GetComponent<Mob>()?.SetForcedTarget(player);
        }

        if (players.Count > 2 && UnityEngine.Random.value < .2f)
        {
            Vector3 probe = Utils.GetPlayerCenter(players) + new Vector3(UnityEngine.Random.Range(-SpawnRadius, SpawnRadius), 0f, UnityEngine.Random.Range(-SpawnRadius, SpawnRadius));

            if (Utils.TryGetGround(probe, out Vector3 ground)) Utils.TrySpawnObject("Character_Scoutmaster", ground, Quaternion.identity);
        }
    }
}
