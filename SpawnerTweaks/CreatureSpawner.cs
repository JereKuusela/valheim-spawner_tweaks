using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Service;
using UnityEngine;

namespace SpawnerTweaks;

[HarmonyPatch(typeof(CreatureSpawner))]
public class CreatureSpawnerPatches
{
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
  static int LevelChance = "override_level_chance".GetStableHashCode();
  // float (percent)
  static int Health = "override_health".GetStableHashCode();
  // float
  static int Faction = "override_faction".GetStableHashCode();
  // string
  static int Data = "override_data".GetStableHashCode();
  // string

  static void HandleSpawn(CreatureSpawner obj)
  {
    var hash = obj.m_nview.GetZDO().GetInt(Spawn, 0);
    if (hash == 0) return;
    var prefab = ZNetScene.instance.GetPrefab(hash);
    if (!prefab) return;
    obj.m_creaturePrefab = prefab;
  }

  [HarmonyPatch(nameof(CreatureSpawner.Awake)), HarmonyPostfix]
  static void Setup(CreatureSpawner __instance)
  {
    if (!Configuration.configCreatureSpawner.Value) return;
    var obj = __instance;
    var view = obj.m_nview;
    if (!view || !view.IsValid()) return;
    Helper.Float(view, Respawn, value => obj.m_respawnTimeMinuts = value);
    HandleSpawn(__instance);
    Helper.Int(view, MaxLevel, value => obj.m_maxLevel = value);
    Helper.Int(view, MinLevel, value => obj.m_minLevel = value);
    Helper.Int(view, SpawnCondition, value =>
    {
      obj.m_spawnAtNight = true;
      obj.m_spawnAtDay = true;
      if (value == 1)
        obj.m_spawnAtNight = false;
      if (value == 2)
        obj.m_spawnAtDay = false;
    });
    Helper.Float(view, TriggerDistance, value => obj.m_triggerDistance = value);
    Helper.Float(view, TriggerNoise, value => obj.m_triggerNoise = value);
    Helper.Float(view, LevelChance, value => obj.m_levelupChance = value);
    Helper.String(view, SpawnEffect, value => obj.m_spawnEffects = Helper.ParseEffects(value));
  }

  private static ZDO? SpawnData = null;


  [HarmonyPatch(nameof(CreatureSpawner.Spawn)), HarmonyPrefix]
  static void GetValues(CreatureSpawner __instance)
  {
    SpawnData = null;
    Helper.String(__instance.m_nview, Data, value => SpawnData = DataHelper.Load(value));
  }

  [HarmonyPatch(nameof(CreatureSpawner.Spawn)), HarmonyPostfix]
  static void SetupSpawn(CreatureSpawner __instance, ZNetView __result)
  {
    if (!Configuration.configCreatureSpawner.Value) return;
    if (!__result) return;
    var obj = __result.GetComponent<Character>();
    if (!obj) return;
    Helper.Float(__instance.m_nview, Health, obj.SetMaxHealth);
    Helper.String(__instance.m_nview, Faction, value =>
    {
      obj.m_nview.GetZDO().Set(Faction, value);
      if (Enum.TryParse<Character.Faction>(value, true, out var faction))
        obj.m_faction = faction;
    });
  }

  static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
  {
    if (SpawnData != null)
      DataHelper.InitZDO(prefab, position, rotation, SpawnData);
    var obj = UnityEngine.Object.Instantiate<GameObject>(prefab, position, rotation);
    return obj;
  }

  [HarmonyPatch(nameof(CreatureSpawner.Spawn)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> SetupData(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
      .MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(CreatureSpawner), nameof(CreatureSpawner.m_creaturePrefab))))
      .Advance(3)
      .Set(OpCodes.Call, Transpilers.EmitDelegate(Instantiate).operand)
      .InstructionEnumeration();
  }
}

[HarmonyPatch(typeof(CreatureSpawner), nameof(CreatureSpawner.UpdateSpawner))]
public class CreatureSpawnerUpdateSpawner
{

  static KeyValuePair<int, int> SpawnId = ZDO.GetHashZDOID("spawn_id");

  static void Prefix(CreatureSpawner __instance, ref float __state)
  {
    __state = 0.0f;
    if (__instance.m_respawnTimeMinuts != 0f && __instance.m_nview.GetZDO().GetZDOID(SpawnId).IsNone())
    {
      __state = __instance.m_respawnTimeMinuts;
      __instance.m_respawnTimeMinuts = 0;
    }
  }
  static void Postfix(CreatureSpawner __instance, float __state)
  {
    if (__state != 0f) __instance.m_respawnTimeMinuts = __state;
  }
}
