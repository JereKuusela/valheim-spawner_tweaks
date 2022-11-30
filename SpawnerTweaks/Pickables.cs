using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace SpawnerTweaks;

[HarmonyPatch(typeof(Pickable))]
public class PickablePatches
{
  static int Spawn = "override_spawn".GetStableHashCode();
  // prefab
  static int Respawn = "override_respawn".GetStableHashCode();
  // float (minutes)
  static int Amount = "override_amount".GetStableHashCode();
  // int
  static int Name = "override_name".GetStableHashCode();
  // string
  static int SpawnOffset = "override_spawn_offset".GetStableHashCode();
  // float (meters)
  static int UseEffect = "override_use_effect".GetStableHashCode();
  // prefab,flags,variant,childTransform|prefab,flags,variant,childTransform|...
  static void SetSpawn(Pickable obj, ZNetView view) =>
    Helper.Prefab(view, Spawn, value => obj.m_itemPrefab = value);
  static void SetRespawn(Pickable obj, ZNetView view) =>
    Helper.Float(view, Respawn, value => obj.m_respawnTimeMinutes = (int)value);
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
