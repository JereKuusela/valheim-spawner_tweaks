using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Service;
using UnityEngine;

namespace SpawnerTweaks;

[HarmonyPatch(typeof(SpawnArea))]
public class SpawnAreaPatches
{
  static int SpawnEffect = "override_spawn_effect".GetStableHashCode();
  // prefab,flags,variant,childTransform|prefab,flags,variant,childTransform|...
  static int Spawn = "override_spawn".GetStableHashCode();
  // prefab,weight,minLevel,maxLevel|prefab,weight,minLevel,maxLevel|...
  static int Respawn = "override_respawn".GetStableHashCode();
  // float (seconds)
  static int MaxNear = "override_max_near".GetStableHashCode();
  // int
  static int MaxTotal = "override_max_total".GetStableHashCode();
  // int
  static int LevelChance = "override_level_chance".GetStableHashCode();
  // float (percent)
  static int TriggerDistance = "override_trigger_distance".GetStableHashCode();
  // float (meters)
  static int SpawnRadius = "override_spawn_radius".GetStableHashCode();
  // float (meters)
  static int NearRadius = "override_near_radius".GetStableHashCode();
  // float (meters)
  static int FarRadius = "override_far_radius".GetStableHashCode();
  // float (meters)
  static int SpawnCondition = "override_spawn_condition".GetStableHashCode();
  // flag (1 = day only, 2 = night only, 4 = ground only)
  static int MinLevel = "override_minimum_level".GetStableHashCode();
  // int
  static int MaxLevel = "override_maximum_level".GetStableHashCode();
  // float (percent)
  static int Health = "override_health".GetStableHashCode();
  // float
  static int Faction = "override_faction".GetStableHashCode();
  // string

  [HarmonyPatch(nameof(SpawnArea.Awake)), HarmonyPostfix]
  static void Setup(SpawnArea __instance)
  {
    if (!Configuration.configSpawnArea.Value) return;
    var obj = __instance;
    var view = obj.m_nview;
    if (!view || !view.IsValid()) return;
    Helper.Float(view, LevelChance, value => obj.m_levelupChance = value);
    Helper.Float(view, SpawnRadius, value => obj.m_spawnRadius = value);
    Helper.Float(view, NearRadius, value => obj.m_nearRadius = value);
    Helper.Float(view, FarRadius, value => obj.m_farRadius = value);
    Helper.Float(view, TriggerDistance, value => obj.m_triggerDistance = value);
    Helper.Int(view, MaxNear, value => obj.m_maxNear = value);
    Helper.Int(view, MaxTotal, value => obj.m_maxTotal = value);
    Helper.Float(view, Respawn, value => obj.m_spawnIntervalSec = value);
    Helper.String(view, SpawnEffect, value => obj.m_spawnEffects = Helper.ParseEffects(value));
    Helper.String(view, Spawn, value => obj.m_prefabs = Helper.ParseSpawnsData(value));
    Helper.Int(view, SpawnCondition, value => obj.m_onGroundOnly = (value & 4) > 0);
  }


  private static float? SpawnHealth = null;
  private static string? SpawnFaction = null;
  private static int? SpawnLevel = null;
  private static ZDO? SpawnData = null;
  [HarmonyPatch(nameof(SpawnArea.SelectWeightedPrefab)), HarmonyPostfix]
  static void GetSpawnedData(SpawnArea __instance, SpawnArea.SpawnData __result)
  {
    Spawned = null;
    SpawnHealth = null;
    SpawnFaction = null;
    SpawnLevel = null;
    SpawnData = null;
    int? minLevel = null;
    int? maxLevel = null;
    Helper.Float(__instance.m_nview, Health, value => SpawnHealth = value);
    Helper.String(__instance.m_nview, Faction, value => SpawnFaction = value);
    Helper.Int(__instance.m_nview, MinLevel, value => minLevel = value);
    Helper.Int(__instance.m_nview, MaxLevel, value => maxLevel = value);

    Helper.String(__instance.m_nview, Spawn, value =>
    {
      var index = __instance.m_prefabs.IndexOf(__result);
      var split = value.Split('|')[index].Split(',');
      if (split.Length > 2)
        minLevel = Helper.Int(split[2]);
      if (split.Length > 3)
        maxLevel = Helper.Int(split[3]);
      if (split.Length > 4)
      {
        var arg = Helper.Float(split[4]);
        if (arg.HasValue)
        {
          SpawnHealth = arg;
          if (split.Length > 5)
            SpawnData = DataHelper.Load(split[5]);
        }
        else
        {
          if (Enum.TryParse<Character.Faction>(split[4], true, out var faction))
            SpawnFaction = split[4];
          else
            SpawnData = DataHelper.Load(split[4]);
          if (split.Length > 5)
            SpawnHealth = Helper.Float(split[5]);
        }
      }
    });
    if (minLevel.HasValue && maxLevel.HasValue)
    {
      SpawnLevel = Helper.RollLevel(minLevel.Value, maxLevel.Value, __instance.m_levelupChance);
    }
  }

  [HarmonyPatch(nameof(SpawnArea.SpawnOne)), HarmonyPrefix]
  static bool CheckTime(SpawnArea __instance)
  {
    if (!Configuration.configSpawnArea.Value) return true;
    var value = __instance.m_nview.GetZDO().GetInt(SpawnCondition, -1);
    if (value < 0) return true;
    if ((value & 1) > 0 && EnvMan.instance.IsNight()) return false;
    if ((value & 2) > 0 && EnvMan.instance.IsDay()) return false;
    return true;
  }

  private static Character? Spawned = null;
  private static Vector3 GetCenterPoint(Character character, GameObject obj)
  {
    Spawned = character;
    return character?.GetCenterPoint() ?? obj.transform.position;
  }

  [HarmonyPatch(nameof(SpawnArea.SpawnOne)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> FixCenterPoint(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Character), nameof(Character.GetCenterPoint))))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 4))
      .Set(OpCodes.Call, Transpilers.EmitDelegate(GetCenterPoint).operand).InstructionEnumeration();
  }

  static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
  {
    if (SpawnData != null)
      DataHelper.InitZDO(prefab, position, rotation, SpawnData);
    var obj = UnityEngine.Object.Instantiate<GameObject>(prefab, position, rotation);
    return obj;
  }

  [HarmonyPatch(nameof(SpawnArea.SpawnOne)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> SetData(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
      .MatchForward(false, new CodeMatch(OpCodes.Ldc_I4, 360))
      .Advance(5)
      .Set(OpCodes.Call, Transpilers.EmitDelegate(Instantiate).operand)
      .InstructionEnumeration();
  }

  // Must be done here to override CLLC changes.
  [HarmonyPatch(nameof(SpawnArea.SpawnOne)), HarmonyPostfix, HarmonyPriority(Priority.VeryLow)]
  static void ApplyChanges()
  {
    if (Spawned == null) return;
    if (Enum.TryParse<Character.Faction>(SpawnFaction, true, out var faction))
    {
      Spawned.m_faction = faction;
      Spawned.m_nview.GetZDO().Set(Faction, SpawnFaction);
    }
    if (SpawnLevel.HasValue)
      Spawned.SetLevel(SpawnLevel.Value);
    if (SpawnHealth.HasValue)
      Spawned.SetMaxHealth(SpawnHealth.Value);
  }
}
