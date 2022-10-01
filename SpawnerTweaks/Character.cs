using System;
using HarmonyLib;

namespace SpawnerTweaks;

[HarmonyPatch(typeof(Character), nameof(Character.Awake))]
public class CharacterAwake {
  static int Faction = "override_faction".GetStableHashCode();
  // string
  static void Postfix(Character __instance) {
    Helper.String(__instance.m_nview, Faction, value => {
      if (Enum.TryParse<Character.Faction>(value, true, out var faction))
        __instance.m_faction = faction;
    });
  }
}
