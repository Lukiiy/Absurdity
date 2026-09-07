using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using Zorro.Core;

namespace Absurdity;

internal sealed class TropicsLookerHazard : MonoBehaviour
{
    [HarmonyPatch(typeof(Looker), "ToggleLookers")]
    internal static class TropicsLookerDisablePatch
    {
        private static bool Prefix()
        {
            if (!Plugin.EveryLooker.Value || !PhotonNetwork.IsMasterClient || !MapHandler.ExistsAndInitialized || Singleton<MapHandler>.Instance.GetCurrentBiome() != Biome.BiomeType.Tropics) return true;

            return false;
        }
    }

    [HarmonyPatch(typeof(RunManager), nameof(RunManager.StartRun))]
    private static class Apply
    {
        private static void Postfix(RunManager __instance)
        {
            if (__instance == null || !Plugin.EveryLooker.Value) return;

            Looker[] lookers = FindObjectsByType<Looker>(FindObjectsInactive.Include, FindObjectsSortMode.None); // ooo parameterssss

            int activated = 0;
            foreach (Looker looker in lookers)
            {
                if (looker == null || looker.gameObject.activeSelf) continue;

                PhotonView view = looker.GetComponent<PhotonView>();
                if (view == null) continue;

                looker.gameObject.SetActive(true);
                activated++;
            }

            if (activated > 0) Plugin.Log.LogDebug($"reactivated {activated} lookers.");
        }
    }
}