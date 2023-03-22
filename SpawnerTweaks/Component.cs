using HarmonyLib;
using UnityEngine;

namespace SpawnerTweaks;

[HarmonyPatch(typeof(ZNetView))]
public class ComponentPatches
{
  static int HashComponent = "override_component".GetStableHashCode();
  static void AddComponent<T>(ZNetView view) where T : MonoBehaviour
  {
    Object.Destroy(view.GetComponent<Interactable>() as MonoBehaviour);
    view.gameObject.AddComponent<T>();
  }
  static void HandleComponent(ZNetView view)
  {
    var str = view.GetZDO().GetString(HashComponent, "").ToLower(); ;
    if (str == "") return;
    var values = str.Split(',');
    foreach (var value in values)
    {
      if (value == "altar" && !view.gameObject.GetComponent<OfferingBowl>()) AddComponent<OfferingBowl>(view);
      if (value == "pickable" && !view.gameObject.GetComponent<Pickable>()) AddComponent<Pickable>(view);
      if (value == "spawnpoint" && !view.gameObject.GetComponent<CreatureSpawner>()) view.gameObject.AddComponent<CreatureSpawner>();
      if (value == "spawner" && !view.gameObject.GetComponent<SpawnArea>()) view.gameObject.AddComponent<SpawnArea>();
      if (value == "chest" && !view.gameObject.GetComponent<Container>()) AddComponent<Container>(view);
      if (value == "itemstand" && !view.gameObject.GetComponent<ItemStand>()) AddComponent<ItemStand>(view);
      if (value == "smelter" && !view.gameObject.GetComponent<Smelter>()) AddComponent<Smelter>(view);
      if (value == "fermenter" && !view.gameObject.GetComponent<Fermenter>()) AddComponent<Fermenter>(view);
      if (value == "beehive" && !view.gameObject.GetComponent<Beehive>()) AddComponent<Beehive>(view);
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
