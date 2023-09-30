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

  // CLLC patches the same thing. Lower priority to override it.
  [HarmonyPatch(nameof(CreatureSpawner.Awake)), HarmonyPostfix, HarmonyPriority(Priority.LowerThanNormal)]
  static void Setup(CreatureSpawner __instance)
  {
    if (!Configuration.configCreatureSpawner.Value) return;
    var obj = __instance;
    var view = obj.m_nview;
    if (!view || !view.IsValid()) return;
    Helper.Float(view, Hash.Respawn, value => obj.m_respawnTimeMinuts = value);
    Helper.Prefab(view, Hash.Spawn, value => obj.m_creaturePrefab = value);
    Helper.Int(view, Hash.MaxLevel, value => obj.m_maxLevel = value);
    Helper.Int(view, Hash.MinLevel, value => obj.m_minLevel = value);
    Helper.Int(view, Hash.SpawnCondition, value =>
    {
      obj.m_spawnAtNight = true;
      obj.m_spawnAtDay = true;
      if (value == 1)
        obj.m_spawnAtNight = false;
      if (value == 2)
        obj.m_spawnAtDay = false;
    });
    Helper.Float(view, Hash.TriggerDistance, value => obj.m_triggerDistance = value);
    Helper.Float(view, Hash.TriggerNoise, value => obj.m_triggerNoise = value);
    Helper.Float(view, Hash.LevelChance, value => obj.m_levelupChance = value);
    Helper.String(view, Hash.SpawnEffect, value => obj.m_spawnEffects = Helper.ParseEffects(value));
  }

  private static ZPackage? SpawnData = null;


  [HarmonyPatch(nameof(CreatureSpawner.Spawn)), HarmonyPrefix]
  static void GetValues(CreatureSpawner __instance)
  {
    SpawnData = null;
    Helper.String(__instance.m_nview, Hash.Data, value => SpawnData = DataHelper.Deserialize(value));
  }

  [HarmonyPatch(nameof(CreatureSpawner.Spawn)), HarmonyPostfix]
  static void SetupSpawn(CreatureSpawner __instance, ZNetView __result)
  {
    if (!Configuration.configCreatureSpawner.Value) return;
    if (!__result) return;
    var obj = __result.GetComponent<Character>();
    if (!obj) return;
    // Level must be done here to override CLLC changes.
    OverrideLevel(__instance, obj);
    var view = __instance.m_nview;
    Helper.Float(view, Hash.Health, obj.SetMaxHealth);
    Helper.String(view, Hash.Faction, value =>
    {
      obj.m_nview.GetZDO().Set(Hash.Faction, value);
      if (Enum.TryParse<Character.Faction>(value, true, out var faction))
        obj.m_faction = faction;
    });
  }

  private static void OverrideLevel(CreatureSpawner spawner, Character obj)
  {
    var view = spawner.m_nview;
    var setupLevel = false;
    Helper.Int(view, Hash.MaxLevel, value => setupLevel = true);
    if (!setupLevel)
      Helper.Int(view, Hash.MinLevel, value => setupLevel = true);
    if (!setupLevel)
      Helper.Float(view, Hash.LevelChance, value => setupLevel = true);
    if (setupLevel)
      obj.SetLevel(Helper.RollLevel(spawner.m_minLevel, spawner.m_maxLevel, spawner.m_levelupChance));
  }

  static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
  {
    if (SpawnData != null)
      DataHelper.InitZDO(prefab, position, rotation, SpawnData);
    var obj = UnityEngine.Object.Instantiate(prefab, position, rotation);
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
  static void Prefix(CreatureSpawner __instance, ref float __state)
  {
    __state = 0f;
    if (__instance.m_respawnTimeMinuts == 0f) return;
    var firstSpawn = __instance.m_nview.GetZDO().GetConnection() == null;
    if (firstSpawn)
    {
      __state = __instance.m_respawnTimeMinuts;
      __instance.m_respawnTimeMinuts = 0f;
    }
  }
  static void Postfix(CreatureSpawner __instance, float __state)
  {
    if (__state != 0f) __instance.m_respawnTimeMinuts = __state;
  }
}
