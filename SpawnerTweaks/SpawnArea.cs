using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Service;
using UnityEngine;

namespace SpawnerTweaks;

[HarmonyPatch(typeof(SpawnArea))]
public class SpawnAreaPatches
{


  [HarmonyPatch(nameof(SpawnArea.Awake)), HarmonyPostfix]
  static void Setup(SpawnArea __instance)
  {
    if (!Configuration.configSpawnArea.Value) return;
    var obj = __instance;
    var view = obj.m_nview;
    if (!view || !view.IsValid()) return;
    Helper.Float(view, Hash.LevelChance, value => obj.m_levelupChance = value);
    Helper.Float(view, Hash.SpawnRadius, value => obj.m_spawnRadius = value);
    Helper.Float(view, Hash.NearRadius, value => obj.m_nearRadius = value);
    Helper.Float(view, Hash.FarRadius, value => obj.m_farRadius = value);
    Helper.Float(view, Hash.TriggerDistance, value => obj.m_triggerDistance = value);
    Helper.Int(view, Hash.MaxNear, value => obj.m_maxNear = value);
    Helper.Int(view, Hash.MaxTotal, value => obj.m_maxTotal = value);
    Helper.Float(view, Hash.SpawnAreaRespawn, Hash.Respawn, value => obj.m_spawnIntervalSec = value);
    Helper.String(view, Hash.SpawnEffect, value => obj.m_spawnEffects = Helper.ParseEffects(value));
    Helper.String(view, Hash.SpawnAreaSpawn, Hash.Spawn, value => obj.m_prefabs = Helper.ParseSpawnsData(value));
    Helper.Int(view, Hash.SpawnCondition, value => obj.m_onGroundOnly = (value & 4) > 0);
  }

  [HarmonyPatch(nameof(SpawnArea.UpdateSpawn)), HarmonyPrefix]
  static bool PickableCheck(SpawnArea __instance)
  {
    return __instance.GetComponent<Pickable>()?.m_picked != true;
  }

  private static float? SpawnHealth;
  private static string? SpawnFaction;
  private static int? SpawnLevel;
  private static ZPackage? SpawnData;
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
    Helper.Float(__instance.m_nview, Hash.Health, value => SpawnHealth = value);
    Helper.String(__instance.m_nview, Hash.Faction, value => SpawnFaction = value);
    Helper.Int(__instance.m_nview, Hash.MinLevel, value => minLevel = value);
    Helper.Int(__instance.m_nview, Hash.MaxLevel, value => maxLevel = value);

    Helper.String(__instance.m_nview, Hash.SpawnAreaSpawn, Hash.Spawn, value =>
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
            SpawnData = DataHelper.Deserialize(split[5]);
        }
        else
        {
          if (Enum.TryParse<Character.Faction>(split[4], true, out var faction))
            SpawnFaction = split[4];
          else
            SpawnData = DataHelper.Deserialize(split[4]);
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
    var value = __instance.m_nview.GetZDO().GetInt(Hash.SpawnCondition);
    if (value <= 0) return true;
    if ((value & 1) > 0 && EnvMan.IsNight()) return false;
    if ((value & 2) > 0 && EnvMan.IsDay()) return false;
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
    var obj = UnityEngine.Object.Instantiate(prefab, position, rotation);
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
    if (SpawnFaction != null && Enum.TryParse<Character.Faction>(SpawnFaction, true, out var faction))
    {
      Spawned.m_faction = faction;
      Spawned.m_nview.GetZDO().Set(Hash.Faction, SpawnFaction);
    }
    if (SpawnLevel.HasValue)
      Spawned.SetLevel(SpawnLevel.Value);
    if (SpawnHealth.HasValue)
      Spawned.SetMaxHealth(SpawnHealth.Value);
  }
  static readonly int Tamed = "Tamed".GetStableHashCode();
  static readonly int Stack = "stack".GetStableHashCode();
  [HarmonyPatch(nameof(SpawnArea.GetInstances)), HarmonyPrefix]
  static bool GetInstances(SpawnArea __instance, out int near, out int total)
  {
    near = 0;
    total = 0;
    var pos = __instance.transform.position;
    var prefabs = __instance.m_prefabs.Select(x => x.m_prefab.name.GetStableHashCode()).ToHashSet();
    foreach (var zdo in ZNetScene.instance.m_instances.Keys)
    {
      if (!prefabs.Contains(zdo.GetPrefab())) continue;
      if (zdo.GetBool(Tamed)) continue;
      var distance = Utils.DistanceXZ(pos, zdo.GetPosition());
      var amount = zdo.GetInt(Stack, 1);
      if (distance < __instance.m_nearRadius) near += amount;
      if (distance < __instance.m_farRadius) total += amount;
    }
    return false;
  }
}
