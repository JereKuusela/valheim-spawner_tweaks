using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace Plugin;

[HarmonyPatch(typeof(Pickable), nameof(Pickable.Awake))]
public class PickableAwake {
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
  static void Prefix(Pickable __instance) {
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
}

[HarmonyPatch(typeof(Pickable), nameof(Pickable.Drop))]
public class PickableDrop {
  private static void SetStack(ItemDrop obj, int amount) => obj?.SetStack(amount);
  static IEnumerable<CodeInstruction> Transpilera(IEnumerable<CodeInstruction> instructions) {
    return new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(ItemDrop), nameof(ItemDrop.SetStack))))
      .Set(OpCodes.Call, Transpilers.EmitDelegate(SetStack).operand).InstructionEnumeration();
  }
}
