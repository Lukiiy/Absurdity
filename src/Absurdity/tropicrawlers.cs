using System.Collections.Generic;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace Absurdity;

internal sealed class TropiCrawlers : MonoBehaviour
{
    private static readonly HashSet<MountainProgressHandler.ProgressPoint> Handled = [];

    private const int Count = 125;
    private const float ForwardOffset = 80f;
    private const float HorizontalSpread = 100f;

    [HarmonyPatch(typeof(RunManager), nameof(RunManager.StartRun))]
    private static class Apply
    {
        private static void Postfix(RunManager __instance)
        {
            if (__instance == null || !Plugin.TropiCrawlers.Value) return;
            if (!__instance.TryGetComponent<TropiCrawlers>(out _)) __instance.gameObject.AddComponent<TropiCrawlers>();

            Handled.Clear();
        }
    }

    [HarmonyPatch(typeof(MountainProgressHandler), nameof(MountainProgressHandler.TriggerReached))]
    private static class OnBiomeReached
    {
        private static void Postfix(MountainProgressHandler.ProgressPoint progressPoint)
        {
            if (!Plugin.TropiCrawlers.Value || !PhotonNetwork.IsMasterClient || progressPoint == null) return;

            (string? prefab, int spawnCount) = progressPoint.biome switch
            {
                Biome.BiomeType.Tropics => ("Scorpion", Count),
                Biome.BiomeType.Roots => ("Beetle", Mathf.RoundToInt(Count * .25f)),
                _ => (null, 0)
            };

            if (prefab == null || !Handled.Add(progressPoint)) return;

            Transform barrier = progressPoint.transform;
            if (barrier == null) return;

            Vector3 center = barrier.position + barrier.forward * ForwardOffset;
            float baseY = center.y + 30f;

            for (int i = 0; i < spawnCount; i++)
            {
                Vector2 offset = UnityEngine.Random.insideUnitCircle * HorizontalSpread;
                Vector3 spawn = new(center.x + offset.x, baseY + UnityEngine.Random.Range(-5f, 10f), center.z + offset.y);

                Utils.TrySpawnObject(prefab, spawn, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f));
            }
        }
    }
}