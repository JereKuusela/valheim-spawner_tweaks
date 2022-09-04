using HarmonyLib;

namespace SpawnerTweaks;

[HarmonyPatch(typeof(ZNetView), nameof(ZNetView.Awake))]
public class ZNetViewAwake {
  static int HashComponent = "override_component".GetStableHashCode();
  static void HandleComponent(ZNetView view) {
    var str = view.GetZDO().GetString(HashComponent, "").ToLower(); ;
    if (str == "") return;
    var values = str.Split(',');
    foreach (var value in values) {
      if (value == "altar") view.gameObject.AddComponent<OfferingBowl>();
      if (value == "pickable") view.gameObject.AddComponent<Pickable>();
      if (value == "spawnpoint") view.gameObject.AddComponent<CreatureSpawner>();
      if (value == "spawner") view.gameObject.AddComponent<SpawnArea>();
    }
  }
  static void Postfix(ZNetView __instance) {
    if (!Configuration.configComponent.Value) return;
    if (!__instance || !__instance.IsValid()) return;
    HandleComponent(__instance);
  }
}
