using System.Collections.Generic;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using Zorro.Core;

namespace Absurdity;

[HarmonyPatch(typeof(Luggage), nameof(Luggage.OpenLuggageRPC))]
internal static class Beansticks
{
    private const float Radius = 10f;
    private const int MaxExtra = 4;
    private static readonly HashSet<Biome.BiomeType> Handled = [];

    [HarmonyPostfix]
    private static void Postfix()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Biome.BiomeType biome = Singleton<MapHandler>.Instance.GetCurrentBiome();
        if (biome != Biome.BiomeType.Shore && biome != Biome.BiomeType.Alpine || !Handled.Add(biome)) return;

        Transform root = MapHandler.CurrentMapSegment.segmentParent.transform;
        if (root == null) return;

        foreach (Luggage luggage in Object.FindObjectsByType<Luggage>(FindObjectsSortMode.None))
        {
            if (luggage == null || !luggage.transform.IsChildOf(root) || biome == Biome.BiomeType.Shore && IgnoreFirst(luggage)) continue;

            SpawnNear(luggage.transform.position);

            for (int i = 0; i < MaxExtra; i++)
            {
                if (Random.value > .5) break;

                SpawnNear(luggage.transform.position);
            }
        }
    }

    [HarmonyPatch(typeof(RunManager), nameof(RunManager.StartRun))]
    private static class RunReset
    {
        private static void Postfix() => Handled.Clear();
    }

    private static bool IgnoreFirst(Luggage luggage)
    {
        List<Character> players = Utils.GetActivePlayers();
        if (players == null || players.Count == 0) return false;

        return Vector3.Distance(luggage.transform.position, Utils.GetPlayerCenter(players)) <= 50f;
    }

    private static void SpawnNear(Vector3 center) // TODO???
    {
        Vector3 pos = center + Random.insideUnitSphere * Radius;
        Vector3 ray = pos + Random.onUnitSphere * Radius;

        if (!Physics.Raycast(ray, (pos - ray).normalized, out RaycastHit hit, Radius * 3f, HelperFunctions.terrainMapMask)) return; // inside, ig.

        SpawnAt(hit.point, hit.normal);
    }

    private static void SpawnAt(Vector3 point, Vector3 normal)
    {
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, normal);
        GameObject? beanObj = Utils.TrySpawnObject("MagicBean", point, rot);
        if (beanObj == null) return;

        MagicBean bean = beanObj.GetComponentInChildren<MagicBean>();
        if (bean == null || bean.plantPrefab == null)
        {
            PhotonNetwork.Destroy(beanObj);

            return;
        }

        PhotonNetwork.InstantiateRoomObject(bean.plantPrefab.name, point, rot, 0)?.GetComponent<PhotonView>()?.RPC("RPC_GrowVine", RpcTarget.All, bean.plantPrefab.maxLength, float.MinValue);
        PhotonNetwork.Destroy(beanObj);
    }
}