using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace Absurdity;

internal sealed class Shornado : MonoBehaviour
{
    private const int Limit = 4;
    private static readonly Queue<GameObject> Active = new();

    [HarmonyPatch(typeof(MountainProgressHandler), nameof(MountainProgressHandler.TriggerReached))]
    private static class BiomeReach
    {
        private static void Postfix(MountainProgressHandler.ProgressPoint progressPoint)
        {
            if (!Plugin.ShoreTornadoes.Value || !PhotonNetwork.IsMasterClient || progressPoint == null || progressPoint.biome != Biome.BiomeType.Shore) return;

            Active.Clear();
            Plugin.Instance.StartCoroutine(SpawnLoop());
        }
    }

    private const float NearPlayerTargetRoll = .25f;
    private const float TotallyNotArbitraryRadius = 100f;

    private static void SpawnTornado()
    {
        if (!Utils.TryGetGround(GetRandomShoreProbe(), out Vector3 spawnPosition)) return;

        GameObject? tornadoObj = Utils.TrySpawnObject("Tornado", spawnPosition, Quaternion.identity);
        if (tornadoObj == null) return;

        NullCleanup();
        while (Active.Count >= Limit)
        {
            GameObject oldest = Active.Dequeue();

            if (oldest != null) Destroy(oldest);
        }

        Active.Enqueue(tornadoObj);

        Tornado tornado = tornadoObj.GetComponent<Tornado>();
        Vector3 targetProbe = Random.value < NearPlayerTargetRoll ? GetRandomPlayerProbe() : GetRandomShoreProbe();

        Utils.TryGetGround(targetProbe, out Vector3 targetPosition); // probe fallback

        GameObject targetRoot = new("ShornadoTargets");
        GameObject target = new("Target");

        target.transform.SetParent(targetRoot.transform);

        target.transform.position = targetPosition;
        tornado.targetParent = targetRoot.transform;
        tornado.target = target.transform;

        Plugin.Instance?.StartCoroutine(DestroyThings(tornadoObj, targetRoot));
    }

    private static void NullCleanup()
    {
        while (Active.Count > 0 && Active.Peek() == null) Active.Dequeue();
    }

    private static IEnumerator DestroyThings(GameObject tornado, GameObject root)
    {
        yield return new WaitUntil(() => tornado == null);

        if (root != null) Destroy(root);
    }

    private static Vector3 GetRandomShoreProbe()
    {
        Vector3 center = Utils.GetPlayerCenter(Utils.GetActivePlayers());

        return center + new Vector3(Random.insideUnitCircle.x, 0f, Random.insideUnitCircle.y) * TotallyNotArbitraryRadius;
    }

    private static Vector3 GetRandomPlayerProbe()
    {
        List<Character> players = Utils.GetActivePlayers();
        if (players.Count == 0) return GetRandomShoreProbe();

        Vector2 offset = 30f * Random.insideUnitCircle;

        return players[Random.Range(0, players.Count)].Center + new Vector3(offset.x, 0f, offset.y);
    }

    private static IEnumerator SpawnLoop()
    {
        SpawnTornado();

        while (PhotonNetwork.InRoom)
        {
            yield return new WaitForSeconds(Random.Range(0f, 35f));

            if (!Utils.InRoomAndReady() || Utils.AllPlayersOnMountain() || !Utils.IsBiome(Biome.BiomeType.Shore))
            {
                Plugin.Log.LogDebug("shornado spawn disabled!");
                yield break;
            }

            if (!Utils.IsBiome(Biome.BiomeType.Shore)) continue;

            SpawnTornado();
        }
    }
}