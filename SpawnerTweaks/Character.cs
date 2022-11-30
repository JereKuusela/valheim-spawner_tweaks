using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace SpawnerTweaks;

[HarmonyPatch(typeof(Character))]
public class CharacterPatches
{
  static int Faction = "override_faction".GetStableHashCode();
  // string
  static int Name = "override_name".GetStableHashCode();
  // string
  static int Boss = "override_boss".GetStableHashCode();
  // bool (-1 default, 0 false, 1 true)
  static int Resistances = "override_resistances".GetStableHashCode();
  // type,modifier|...
  static int Items = "override_items".GetStableHashCode();
  // id,weight,min,max|...

  [HarmonyPatch(nameof(Character.Awake)), HarmonyPostfix]
  static void Setup(Character __instance)
  {
    if (!Configuration.configCharacter.Value) return;
    Helper.String(__instance.m_nview, Faction, value =>
    {
      if (Enum.TryParse<Character.Faction>(value, true, out var faction))
        __instance.m_faction = faction;
    });
    Helper.Int(__instance.m_nview, Boss, value => __instance.m_boss = value > 0);
    Helper.String(__instance.m_nview, Name, value => __instance.m_name = value);
    Helper.String(__instance.m_nview, Resistances, value => __instance.m_damageModifiers = Helper.ParseDamageModifiers(value));
    if (__instance.TryGetComponent<CharacterDrop>(out var drop))
      Helper.String(__instance.m_nview, Items, value => drop.m_drops = Helper.ParseCharacterDropsData(value));
  }

  [HarmonyPatch(nameof(Character.Awake)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> DisableMaxHealthSetup(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Character), nameof(Character.SetupMaxHealth))))
      .Set(OpCodes.Call, Transpilers.EmitDelegate(((Character _) => { })).operand).InstructionEnumeration();
  }
}
