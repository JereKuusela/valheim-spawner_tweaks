using HarmonyLib;
using UnityEngine;

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
      if (value == "container") view.gameObject.AddComponent<Container>();
      if (value == "itemstand") view.gameObject.AddComponent<ItemStand>();
      if (value == "-fireplace") Object.Destroy(view.GetComponent<Fireplace>());
    }
  }
  static void Postfix(ZNetView __instance) {
    if (!Configuration.configComponent.Value) return;
    if (!__instance || !__instance.IsValid()) return;
    HandleComponent(__instance);
  }
}
