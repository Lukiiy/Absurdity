using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace Absurdity;

internal sealed class TumbleRampage : MonoBehaviour
{
    [HarmonyPatch(typeof(RunManager), nameof(RunManager.StartRun))]
    private static class Apply
    {
        private static void Postfix(RunManager __instance)
        {
            if (__instance == null) return;

            if (__instance.GetComponent<TumbleRampage>() == null) __instance.gameObject.AddComponent<TumbleRampage>();

            Originals.Clear();
            Duplicates.Clear();
            ForcedTargets.Clear();
        }
    }

    private static readonly HashSet<TumbleWeed> Originals = [];
    private static readonly HashSet<TumbleWeed> Duplicates = [];
    private static readonly Dictionary<TumbleWeed, Character> ForcedTargets = [];

    [HarmonyPatch(typeof(TumbleWeed), "FixedUpdate")]
    private static class OnTumbleWeedPhysicsUpdate
    {
        private static void Postfix(TumbleWeed __instance)
        {
            if (__instance == null || !PhotonNetwork.IsMasterClient || !Utils.IsBiome(Biome.BiomeType.Mesa) || Duplicates.Contains(__instance) || !Originals.Add(__instance)) return;

            __instance.gameObject.AddComponent<AbsurdityTumble>().Apply(__instance, false);

            Dupe(__instance);
        }
    }

    [HarmonyPatch(typeof(TumbleWeed), "GetTarget")]
    private static class ForceDuplicateTarget
    {
        private static void Postfix(TumbleWeed __instance, ref Character __result)
        {
            if (ForcedTargets.TryGetValue(__instance, out Character forced) && forced != null) __result = forced;
        }
    }

    private static void Dupe(TumbleWeed original)
    {
        GameObject? dupeObj = Utils.TrySpawnObject("TumbleWeed", original.transform.position, original.transform.rotation);
        if (dupeObj == null) return;

        TumbleWeed dupe = dupeObj.GetComponent<TumbleWeed>();
        if (dupe == null) return;

        Duplicates.Add(dupe);
        dupeObj.AddComponent<AbsurdityTumble>().Apply(dupe, true);

        dupe.rollForce = original.rollForce * .8f;

        Character? ogTarget = JustGetTarget(original);
        if (ogTarget == null) return;

        List<Character> players = Utils.GetActivePlayers();
        Character? forcedTarget = players.Count switch {
            0 => null,
            1 => players[0],
            _ => players.Find(p => p != ogTarget) ?? players[0]
        };
        if (forcedTarget == null) return;

        ForcedTargets[dupe] = forcedTarget;

        if (dupeObj.GetComponent<Rigidbody>() is {} rig) // direction change
        {
            Vector3 sideways = Vector3.Cross(Vector3.up, (forcedTarget.Center - dupeObj.transform.position).normalized);
            float side = UnityEngine.Random.value < .5f ? 1f : -1f;

            rig.AddForce(sideways * (3f * side), ForceMode.VelocityChange);
        }
    }

    private static Character? JustGetTarget(TumbleWeed tumble)
    {
        MethodInfo thisIsSoAnnoying = typeof(TumbleWeed).GetMethod("GetTarget", BindingFlags.Instance | BindingFlags.NonPublic);

        return (Character?) thisIsSoAnnoying?.Invoke(tumble, null);
    }

    private sealed class AbsurdityTumble : MonoBehaviour
    {
        private TumbleWeed tumble = null!;
        private bool duplicate;

        public void Apply(TumbleWeed tumble, bool duplicate)
        {
            this.tumble = tumble;
            this.duplicate = duplicate;
        }

        private void OnDestroy()
        {
            if (duplicate) {
                Duplicates.Remove(tumble);
                ForcedTargets.Remove(tumble);
            } else Originals.Remove(tumble);
        }
    }
}