using HarmonyLib;
using UnityEngine;

namespace Absurdity;

[HarmonyPatch(typeof(CharacterMovement), "JetpackRPC")]
internal static class JetpackBoost
{
    private static void Postfix(CharacterMovement __instance, float fuel)
    {
        if (fuel <= 0f) return;

        Character character = __instance.GetComponent<Character>();
        if (character == null) return;

        character.AddForceAtPosition(Vector3.up * 2f, character.Center, 2f);
    }
}
