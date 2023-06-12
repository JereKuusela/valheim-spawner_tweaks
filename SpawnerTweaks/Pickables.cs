using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;

namespace SpawnerTweaks;

[HarmonyPatch(typeof(Pickable))]
public class PickablePatches
{

  static readonly int SpawnLegacy = "override_spawn".GetStableHashCode();
  // prefab
  static readonly int Spawn = "override_pickable_spawn".GetStableHashCode();
  // prefab
  static readonly int RespawnLegacy = "override_respawn".GetStableHashCode();
  // int (minutes)
  static readonly int Respawn = "override_pickable_respawn".GetStableHashCode();
  // int (minutes)
  static readonly int Amount = "override_amount".GetStableHashCode();
  // int
  static readonly int Name = "override_name".GetStableHashCode();
  // string
  static readonly int SpawnOffset = "override_spawn_offset".GetStableHashCode();
  // float (meters)
  static readonly int UseEffect = "override_use_effect".GetStableHashCode();
  // prefab,flags,variant,childTransform|prefab,flags,variant,childTransform|...
  static readonly int SpawnCondition = "override_spawn_condition".GetStableHashCode();
  // flag (1 = day only, 2 = night only)
  static readonly int RequiredGlobalKey = "override_required_globalkey".GetStableHashCode();
  // hash1,hash2,hash3,...
  static readonly int ForbiddenGlobalKey = "override_forbidden_globalkey".GetStableHashCode();
  // hash1,hash2,hash3,...
  static void SetSpawn(Pickable obj, ZNetView view) =>
    Helper.Prefab(view, Spawn, SpawnLegacy, value => obj.m_itemPrefab = value);
  static void SetRespawn(Pickable obj, ZNetView view) =>
    Helper.Float(view, Respawn, RespawnLegacy, value => obj.m_respawnTimeMinutes = (int)value);
  static void SetAmount(Pickable obj, ZNetView view) =>
    Helper.Int(view, Amount, value => obj.m_amount = value);
  static void SetName(Pickable obj, ZNetView view) =>
    Helper.String(view, Name, value => obj.m_overrideName = value);
  static void SetSpawnOffset(Pickable obj, ZNetView view) =>
    Helper.Float(view, SpawnOffset, value => obj.m_spawnOffset = value);
  static void SetUseEffect(Pickable obj, ZNetView view) =>
    Helper.String(view, UseEffect, value => obj.m_pickEffector = Helper.ParseEffects(value));
  // Must be prefix because Awake starts respawn loop only if respawning.
  [HarmonyPatch(nameof(Pickable.Awake)), HarmonyPrefix]
  static void Setup(Pickable __instance)
  {
    if (!Configuration.configPickable.Value) return;
    var view = __instance.GetComponent<ZNetView>();
    if (!view || !view.IsValid()) return;
    SetRespawn(__instance, view);
    SetSpawn(__instance, view);
    SetAmount(__instance, view);
    SetName(__instance, view);
    SetSpawnOffset(__instance, view);
    SetUseEffect(__instance, view);
  }
  [HarmonyPatch(nameof(Pickable.Awake)), HarmonyPostfix]
  static void StartRespawnLoop(Pickable __instance)
  {
    if (!Configuration.configPickable.Value) return;
    var view = __instance.GetComponent<ZNetView>();
    if (!view || !view.IsValid()) return;
    if (__instance.m_respawnTimeMinutes == 0) return;
    var checkPickable = false;
    Helper.Int(view, SpawnCondition, value => checkPickable = true);
    Helper.String(view, RequiredGlobalKey, value => checkPickable = true);
    Helper.String(view, ForbiddenGlobalKey, value => checkPickable = true);
    if (checkPickable)
      __instance.InvokeRepeating("CheckCondition", UnityEngine.Random.Range(1f, 5f), 30f);
  }

  private static void SetStack(ItemDrop obj, int amount) => obj?.SetStack(amount);
  [HarmonyPatch(nameof(Pickable.Drop)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> SetStackAmount(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(ItemDrop), nameof(ItemDrop.SetStack))))
      .Set(OpCodes.Call, Transpilers.EmitDelegate(SetStack).operand).InstructionEnumeration();
  }

  [HarmonyPatch(nameof(Pickable.GetHoverName)), HarmonyPrefix]
  static bool GetHoverName(Pickable __instance, ref string __result)
  {
    if (string.IsNullOrEmpty(__instance.m_overrideName) && !__instance.m_itemPrefab)
    {
      __result = Utils.GetPrefabName(__instance.gameObject);
      return false;
    }
    return true;
  }
}

public static class PickableExtensions
{
  static readonly int SpawnCondition = "override_spawn_condition".GetStableHashCode();
  // flag (1 = day only, 2 = night only)
  static readonly int RequiredGlobalKey = "override_required_globalkey".GetStableHashCode();
  // hash1,hash2,hash3,...
  static readonly int ForbiddenGlobalKey = "override_forbidden_globalkey".GetStableHashCode();
  // hash1,hash2,hash3,...
  private static void SetPicked(Pickable obj, bool value)
  {
    obj.m_picked = value;
    if (obj.m_hideWhenPicked) obj.m_hideWhenPicked.SetActive(!value);
  }
  public static void CheckCondition(this Pickable obj)
  {
    var view = obj.m_nview;
    if (!view || !view.IsValid()) return;
    var picked = view.GetZDO().GetBool(ZDOVars.s_picked, false);
    if (picked) return;
    Helper.Int(view, SpawnCondition, value =>
    {
      if (value == 1 && EnvMan.instance.IsNight())
        picked = true;
      if (value == 2 && EnvMan.instance.IsDay())
        picked = true;
    });
    if (!picked)
    {
      Helper.HashList(view, RequiredGlobalKey, value =>
      {
        var globalKeys = ZoneSystem.instance.GetGlobalKeys().Select(s => s.GetStableHashCode()).ToHashSet();
        foreach (var key in value)
          if (!globalKeys.Contains(key))
            picked = true;
      });
    }
    if (!picked)
    {
      Helper.HashList(view, ForbiddenGlobalKey, value =>
      {
        var globalKeys = ZoneSystem.instance.GetGlobalKeys().Select(s => s.GetStableHashCode()).ToHashSet();
        foreach (var key in value)
          if (globalKeys.Contains(key))
            picked = true;
      });
    }
    SetPicked(obj, picked);
  }
}