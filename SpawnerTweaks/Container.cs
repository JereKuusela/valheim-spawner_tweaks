using System;
using HarmonyLib;

namespace SpawnerTweaks;

[HarmonyPatch(typeof(Container), nameof(Container.Awake))]
public class ContainerAwake {
  private static int Name = "override_name".GetStableHashCode();
  // string

  static void Postfix(Container __instance) {
    if (!Configuration.configContainer.Value) return;
    Helper.String(__instance.m_nview, Name, value => __instance.m_name = value);
  }
}

[HarmonyPatch(typeof(Container), nameof(Container.AddDefaultItems))]
public class ContainerAddDefaultItems {
  static int MinAmount = "override_minamount".GetStableHashCode();
  // int
  static int MaxAmount = "override_maxamount".GetStableHashCode();
  // int
  static int Items = "override_items".GetStableHashCode();
  // id,weight,min,max|...
  static void Prefix(Container __instance) {
    if (!Configuration.configContainer.Value) return;
    var obj = __instance;
    Helper.Int(obj.m_nview, MinAmount, value => obj.m_defaultItems.m_dropMin = value);
    Helper.Int(obj.m_nview, MaxAmount, value => obj.m_defaultItems.m_dropMin = value);
    Helper.String(obj.m_nview, Items, value => {
      obj.m_defaultItems.m_drops = Helper.ParseDropsData(value);
      obj.m_defaultItems.m_oneOfEach = true;
    });
  }
}

[HarmonyPatch(typeof(Container), nameof(Container.CheckForChanges))]
public class ContainerCheckForChanges {
  static int Respawn = "override_respawn".GetStableHashCode();
  // float (minutes)
  static int Changed = "override_changed".GetStableHashCode();
  // long (timestamp)
  static int AddedItems = "addedDefaultItems".GetStableHashCode();
  // bool
  static void Postfix(Container __instance) {
    if (!Configuration.configContainer.Value) return;
    var obj = __instance;
    if (!obj.m_nview.IsOwner()) return;
    Helper.Float(obj.m_nview, Respawn, respawn => {
      Helper.Long(obj.m_nview, Changed, changed => {
        var d = new DateTime(changed);
        if ((ZNet.instance.GetTime() - d).TotalMinutes > respawn) {
          obj.m_nview.GetZDO().Set(Changed, DateTime.MaxValue.Ticks / 2);
          obj.m_nview.GetZDO().Set(AddedItems, false);
          obj.m_inventory.RemoveAll();
          obj.AddDefaultItems();
          obj.m_nview.GetZDO().Set(AddedItems, true);
        }
      });
    });
  }

}
[HarmonyPatch(typeof(Container), nameof(Container.OnContainerChanged))]
public class ContainerOnContainerChanged {
  static int Respawn = "override_respawn".GetStableHashCode();
  // float (minutes)
  static int Changed = "override_changed".GetStableHashCode();
  // long (timestamp)
  static int AddedItems = "addedDefaultItems".GetStableHashCode();
  // bool
  static void Postfix(Container __instance) {
    if (!Configuration.configContainer.Value) return;
    Helper.Bool(__instance.m_nview, AddedItems, value => {
      if (!value) return;
      Helper.Float(__instance.m_nview, Respawn, value => {
        __instance.m_nview.GetZDO().Set(Changed, ZNet.instance.GetTime().Ticks);
      });
    });
  }
}
