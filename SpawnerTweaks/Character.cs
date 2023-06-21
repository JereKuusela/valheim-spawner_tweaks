using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Service;
using UnityEngine;

namespace SpawnerTweaks;

[HarmonyPatch(typeof(Character))]
public class CharacterPatches
{
  [HarmonyPatch(nameof(Character.Awake)), HarmonyPostfix]
  static void Setup(Character __instance)
  {
    if (!Configuration.configCharacter.Value) return;
    Helper.String(__instance.m_nview, Hash.Faction, value =>
    {
      if (Enum.TryParse<Character.Faction>(value, true, out var faction))
        __instance.m_faction = faction;
    });
    Helper.Int(__instance.m_nview, Hash.Boss, value => __instance.m_boss = value > 0);
    Helper.String(__instance.m_nview, Hash.Name, value => __instance.m_name = value);
    Helper.String(__instance.m_nview, Hash.Resistances, value => __instance.m_damageModifiers = Helper.ParseDamageModifiers(value));
    if (__instance.TryGetComponent<CharacterDrop>(out var drop))
      Helper.String(__instance.m_nview, Hash.Items, value => drop.m_drops = Helper.ParseCharacterDropsData(value));
    if (__instance.TryGetComponent<Humanoid>(out var humanoid))
    {
      Helper.String(__instance.m_nview, Hash.Attacks, value =>
      {
        var sets = Helper.ParseCharacterItemSets(value);
        if (sets.Length == 0) return;
        humanoid.m_randomWeapon = new GameObject[0];
        humanoid.m_randomShield = new GameObject[0];
        Console.instance.AddString($"SpawnerTweaks: {__instance.m_name} has {sets.Length} item sets");
        if (sets.Length == 1)
        {
          humanoid.m_randomSets = new Humanoid.ItemSet[0];
          humanoid.m_defaultItems = sets[0].m_items;
        }
        else
        {
          humanoid.m_randomSets = sets;
          humanoid.m_defaultItems = new GameObject[0];
        }
      });
    }
  }

  [HarmonyPatch(nameof(Character.Awake)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> DisableMaxHealthSetup(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Character), nameof(Character.SetupMaxHealth))))
      .Set(OpCodes.Call, Transpilers.EmitDelegate((Character _) => { }).operand).InstructionEnumeration();
  }
}
