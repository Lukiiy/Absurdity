using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace Absurdity;

internal sealed class Shornado : MonoBehaviour
{
    [HarmonyPatch(typeof(RunManager), "StartRun")]
    private static class Patch
    {
        private static void Postfix(RunManager __instance)
        {
            if (__instance == null || !Plugin.ShoreTornadoes.Value) return;
            if (__instance.GetComponent<Shornado>() == null) __instance.gameObject.AddComponent<Shornado>();

            Plugin.Instance?.StartCoroutine(SpawnLoop());
        }
    }

    private const float NearPlayerTargetRoll = .25f;
    private const float TotallyNotArbitraryRadius = 100f;

    private static void SpawnTornado()
    {
        if (!Utils.TryGetGround(GetRandomShoreProbe(), out Vector3 spawnPosition)) return;

        GameObject? tornadoObj = Utils.TrySpawnObject("Tornado", spawnPosition, Quaternion.identity);
        if (tornadoObj == null) return;

        Tornado tornado = tornadoObj.GetComponent<Tornado>();
        Vector3 targetProbe = UnityEngine.Random.value < NearPlayerTargetRoll ? GetRandomPlayerProbe() : GetRandomShoreProbe();

        Utils.TryGetGround(targetProbe, out Vector3 targetPosition); // probe fallback

        GameObject targetRoot = new("ShornadoTargets");
        GameObject target = new("Target");
        target.transform.SetParent(targetRoot.transform);
        target.transform.position = targetPosition;

        // ihatethis
        typeof(Tornado).GetField("targetParent", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(tornado, targetRoot.transform);
        typeof(Tornado).GetField("target", BindingFlags.Instance | BindingFlags.Public)?.SetValue(tornado, target.transform);

        Plugin.Instance?.StartCoroutine(DestroyThings(tornadoObj, targetRoot));
    }

    private static IEnumerator DestroyThings(GameObject tornado, GameObject root)
    {
        yield return new WaitUntil(() => tornado == null);

        if (root != null) Destroy(root);
    }

    private static Vector3 GetRandomShoreProbe()
    {
        Vector3 center = Utils.GetPlayerCenter(Utils.GetActivePlayers());

        return center + new Vector3(UnityEngine.Random.insideUnitCircle.x, 0f, UnityEngine.Random.insideUnitCircle.y) * TotallyNotArbitraryRadius;
    }

    private static Vector3 GetRandomPlayerProbe()
    {
        List<Character> players = Utils.GetActivePlayers();
        if (players.Count == 0) return GetRandomShoreProbe();

        Vector2 offset = 30f * UnityEngine.Random.insideUnitCircle;

        return players[UnityEngine.Random.Range(0, players.Count)].Center + new Vector3(offset.x, 0f, offset.y);
    }

    private static IEnumerator SpawnLoop()
    {
        while (PhotonNetwork.InRoom)
        {
            SpawnTornado();

            yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 35f));

            if (!Utils.InRoomAndReady() || Utils.AllPlayersOnMountain())
            {
                Plugin.Log.LogDebug("shornado spawn disabled!");
                yield break;
            }

            if (!Utils.IsBiome(Biome.BiomeType.Shore)) continue;

            SpawnTornado();
        }
    }
}