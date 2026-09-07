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
            if (__instance.GetComponent<TropiCrawlers>() == null) __instance.gameObject.AddComponent<TropiCrawlers>();

            Handled.Clear();
        }
    }

    [HarmonyPatch(typeof(MountainProgressHandler), nameof(MountainProgressHandler.TriggerReached))]
    private static class OnBiomeReached
    {
        private static void Postfix(MountainProgressHandler.ProgressPoint progressPoint)
        {
            if (!Plugin.TropiCrawlers.Value || !PhotonNetwork.IsMasterClient || progressPoint == null || progressPoint.biome != Biome.BiomeType.Tropics || !Handled.Add(progressPoint)) return;

            Transform barrier = progressPoint.transform;
            if (barrier == null) return;

            Vector3 center = barrier.position + barrier.forward * ForwardOffset;

            for (int i = 0; i < Count; i++)
            {
                Vector2 offset = UnityEngine.Random.insideUnitCircle * HorizontalSpread;
                Vector3 spawn = center + new Vector3(offset.x, 30f + UnityEngine.Random.Range(-5f, 10f), offset.y);

                Utils.TrySpawnObject("Scorpion", spawn, Quaternion.Euler(0, UnityEngine.Random.Range(0, 360f), 0));
            }
        }
    }
}