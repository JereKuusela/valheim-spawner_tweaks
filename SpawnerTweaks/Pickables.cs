using HarmonyLib;

namespace SpawnerTweaks;

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
  // prefab|flags|variant|childTransform,prefab|flags|variant|childTransform,...
  static void SetSpawn(Pickable obj) =>
    Helper.Prefab(obj.m_nview, Spawn, value => obj.m_itemPrefab = value);
  static void SetRespawn(Pickable obj) =>
    Helper.Float(obj.m_nview, Spawn, value => obj.m_respawnTimeMinutes = (int)value);
  static void SetAmount(Pickable obj) =>
    Helper.Int(obj.m_nview, Spawn, value => obj.m_amount = value);
  static void SetName(Pickable obj) =>
    Helper.String(obj.m_nview, Spawn, value => obj.m_overrideName = value);
  static void SetSpawnOffset(Pickable obj) =>
    Helper.Float(obj.m_nview, SpawnOffset, value => obj.m_spawnOffset = value);
  static void SetUseEffect(Pickable obj) =>
    Helper.String(obj.m_nview, UseEffect, value => obj.m_pickEffector = Helper.ParseEffects(value));
  static void Postfix(Pickable __instance) {
    if (!Configuration.configPickable.Value) return;
    if (!__instance.m_nview || !__instance.m_nview.IsValid()) return;
    SetRespawn(__instance);
    SetSpawn(__instance);
    SetAmount(__instance);
    SetName(__instance);
    SetSpawnOffset(__instance);
  }
}
