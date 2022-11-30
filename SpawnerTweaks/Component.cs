using HarmonyLib;
using UnityEngine;

namespace SpawnerTweaks;

[HarmonyPatch(typeof(ZNetView))]
public class ComponentPatches
{
  static int HashComponent = "override_component".GetStableHashCode();
  static void HandleComponent(ZNetView view)
  {
    var str = view.GetZDO().GetString(HashComponent, "").ToLower(); ;
    if (str == "") return;
    var values = str.Split(',');
    foreach (var value in values)
    {
      if (value == "altar" && !view.gameObject.GetComponent<OfferingBowl>()) view.gameObject.AddComponent<OfferingBowl>();
      if (value == "pickable" && !view.gameObject.GetComponent<Pickable>()) view.gameObject.AddComponent<Pickable>();
      if (value == "spawnpoint" && !view.gameObject.GetComponent<CreatureSpawner>()) view.gameObject.AddComponent<CreatureSpawner>();
      if (value == "spawner" && !view.gameObject.GetComponent<SpawnArea>()) view.gameObject.AddComponent<SpawnArea>();
      if (value == "chest" && !view.gameObject.GetComponent<Container>()) view.gameObject.AddComponent<Container>();
      if (value == "itemstand" && !view.gameObject.GetComponent<ItemStand>()) view.gameObject.AddComponent<ItemStand>();
      if (value == "-fireplace" && view.gameObject.GetComponent<Fireplace>()) Object.Destroy(view.GetComponent<Fireplace>());
    }
  }
  [HarmonyPatch(nameof(ZNetView.Awake)), HarmonyPostfix]
  static void Setup(ZNetView __instance)
  {
    if (!Configuration.configComponent.Value) return;
    if (!__instance || !__instance.IsValid()) return;
    HandleComponent(__instance);
  }
}
