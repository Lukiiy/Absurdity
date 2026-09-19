using HarmonyLib;
using Peak;
using Photon.Pun;
using UnityEngine;

namespace Absurdity;

internal static class ResourcesDebuggin
{
    [HarmonyPatch(typeof(RunManager), nameof(RunManager.StartRun))]
    private static class Trigger
    {
        private static void Postfix(RunManager __instance)
        {
            if (__instance == null || !PhotonNetwork.IsMasterClient) return;

            GameObject[] allPrefabs = Resources.LoadAll<GameObject>("");

            foreach (GameObject prefab in allPrefabs)
            {
                if (prefab == null || prefab.GetComponent<PhotonView>() == null) continue;

                Plugin.Log.LogInfo($"[networkable prefab] {prefab.name}");
            }
        }
    }
}
