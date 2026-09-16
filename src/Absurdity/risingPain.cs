using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using Zorro.Core;

namespace Absurdity;

[HarmonyPatch(typeof(LavaRising), "Update")]
public static class FastRise
{
    private static void Postfix(LavaRising __instance) => __instance.initialWaitTime *= .4f;
}