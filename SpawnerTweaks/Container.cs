using System;
using HarmonyLib;

namespace SpawnerTweaks;

[HarmonyPatch(typeof(Container))]
public class ContainerPatches
{
  private static int Name = "override_name".GetStableHashCode();
  // string
  static int MinAmount = "override_minimum_amount".GetStableHashCode();
  // int
  static int MaxAmount = "override_maximum_amount".GetStableHashCode();
  // int
  static int Items = "override_items".GetStableHashCode();
  // id,weight,min,max|...
  static int Respawn = "override_respawn".GetStableHashCode();
  // float (minutes)
  static int Changed = "override_changed".GetStableHashCode();
  // long (timestamp)
  static int AddedItems = "addedDefaultItems".GetStableHashCode();
  // bool

  [HarmonyPatch(nameof(Container.Awake)), HarmonyPostfix]
  static void Setup(Container __instance)
  {
    if (!Configuration.configContainer.Value) return;
    Helper.String(__instance.m_nview, Name, value => __instance.m_name = value);
  }

  [HarmonyPatch(nameof(Container.AddDefaultItems)), HarmonyPrefix]
  static void ReplaceDefaultItems(Container __instance)
  {
    if (!Configuration.configContainer.Value) return;
    var obj = __instance;
    Helper.Int(obj.m_nview, MinAmount, value => obj.m_defaultItems.m_dropMin = value);
    Helper.Int(obj.m_nview, MaxAmount, value => obj.m_defaultItems.m_dropMax = value);
    Helper.String(obj.m_nview, Items, value =>
    {
      obj.m_defaultItems.m_drops = Helper.ParseDropsData(value);
      obj.m_defaultItems.m_oneOfEach = true;
    });
  }

  [HarmonyPatch(nameof(Container.CheckForChanges)), HarmonyPostfix]
  static void RespawnItems(Container __instance)
  {
    if (!Configuration.configContainer.Value) return;
    var obj = __instance;
    if (!Helper.Owner(__instance.m_nview)) return;
    var respawnContents = false;
    Helper.Float(obj.m_nview, Respawn, respawn =>
    {
      respawnContents = true;
      Helper.Long(obj.m_nview, Changed, changed =>
      {
        var d = new DateTime(changed);
        respawnContents = (ZNet.instance.GetTime() - d).TotalMinutes >= respawn;
      });
    });
    if (respawnContents)
    {
      obj.m_nview.GetZDO().Set(Changed, DateTime.MaxValue.Ticks / 2);
      obj.m_nview.GetZDO().Set(AddedItems, false);
      obj.m_inventory.RemoveAll();
      obj.AddDefaultItems();
      obj.m_nview.GetZDO().Set(AddedItems, true);
    }
  }

  [HarmonyPatch(nameof(Container.OnContainerChanged)), HarmonyPostfix]
  static void TriggerRespawn(Container __instance)
  {
    if (!Configuration.configContainer.Value) return;
    Helper.Bool(__instance.m_nview, AddedItems, value =>
    {
      if (!value) return;
      Helper.Float(__instance.m_nview, Respawn, value =>
      {
        __instance.m_nview.GetZDO().Set(Changed, ZNet.instance.GetTime().Ticks);
      });
    });
  }
}
