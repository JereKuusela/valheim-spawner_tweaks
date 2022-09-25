using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace Plugin;

[HarmonyPatch(typeof(CreatureSpawner), nameof(CreatureSpawner.Awake))]
public class CreatureSpawnerAwake {
  static int Spawn = "override_spawn".GetStableHashCode();
  // prefab
  static int Respawn = "override_respawn".GetStableHashCode();
  // float (minutes)
  static int MinLevel = "override_minimum_level".GetStableHashCode();
  // int
  static int MaxLevel = "override_maximum_level".GetStableHashCode();
  // int
  static int SpawnCondition = "override_spawn_condition".GetStableHashCode();
  // flag (1 = day only, 2 = night only)
  static int TriggerDistance = "override_trigger_distance".GetStableHashCode();
  // float (meters)
  static int TriggerNoise = "override_trigger_noise".GetStableHashCode();
  // float (meters)
  static int SpawnEffect = "override_spawn_effect".GetStableHashCode();
  // prefab,flags,variant,childTransform|prefab,flags,variant,childTransform|...
  static void HandleSpawn(CreatureSpawner obj) {
    var hash = obj.m_nview.GetZDO().GetInt(Spawn, 0);
    if (hash == 0) return;
    var prefab = ZNetScene.instance.GetPrefab(hash);
    if (!prefab) return;
    obj.m_creaturePrefab = prefab;
  }
  static void SetRespawn(CreatureSpawner obj) =>
    Helper.Float(obj.m_nview, Respawn, value => obj.m_respawnTimeMinuts = value);
  static void SetTriggerDistance(CreatureSpawner obj) =>
    Helper.Float(obj.m_nview, TriggerDistance, value => obj.m_triggerDistance = value);
  static void SetTriggerNoise(CreatureSpawner obj) =>
    Helper.Float(obj.m_nview, TriggerNoise, value => obj.m_triggerNoise = value);
  static void SetSpawnCondition(CreatureSpawner obj) =>
    Helper.Int(obj.m_nview, SpawnCondition, value => {
      obj.m_spawnAtNight = true;
      obj.m_spawnAtDay = true;
      if (value == 1)
        obj.m_spawnAtNight = false;
      if (value == 2)
        obj.m_spawnAtDay = false;
    });
  static void SetMinLevel(CreatureSpawner obj) =>
    Helper.Int(obj.m_nview, MinLevel, value => obj.m_minLevel = value);
  static void SetMaxLevel(CreatureSpawner obj) =>
    Helper.Int(obj.m_nview, MaxLevel, value => obj.m_maxLevel = value);
  static void SetSpawnEffect(CreatureSpawner obj) =>
    Helper.String(obj.m_nview, SpawnEffect, value => obj.m_spawnEffects = Helper.ParseEffects(value));
  static void Postfix(CreatureSpawner __instance) {
    if (!Configuration.configCreatureSpawner.Value) return;
    if (!__instance.m_nview || !__instance.m_nview.IsValid()) return;
    SetRespawn(__instance);
    HandleSpawn(__instance);
    SetMaxLevel(__instance);
    SetMinLevel(__instance);
    SetSpawnCondition(__instance);
    SetTriggerDistance(__instance);
    SetTriggerNoise(__instance);
    SetSpawnEffect(__instance);
  }
}

[HarmonyPatch(typeof(CreatureSpawner), nameof(CreatureSpawner.Spawn))]
public class CreatureSpawnerSpawn {
  static int Health = "override_health".GetStableHashCode();
  // float
  static int LevelChance = "override_level_chance".GetStableHashCode();
  // float (percent)

  static void Postfix(CreatureSpawner __instance, ZNetView __result) {
    if (!Configuration.configCreatureSpawner.Value) return;
    if (!__result) return;
    var obj = __result.GetComponent<Character>();
    if (!obj) return;
    Helper.Float(__instance.m_nview, LevelChance, levelChance => {
      var level = Helper.RollLevel(__instance.m_minLevel, __instance.m_maxLevel, levelChance);
      obj.SetLevel(level);
    });
    Helper.Float(__instance.m_nview, Health, obj.SetMaxHealth);
  }
}

[HarmonyPatch(typeof(Character), nameof(Character.Awake))]
public class DisableMaxHealthSetup {
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
    return new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Character), nameof(Character.SetupMaxHealth))))
      .Set(OpCodes.Call, Transpilers.EmitDelegate(((Character _) => { })).operand).InstructionEnumeration();
  }
}
