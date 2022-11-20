using System;
using HarmonyLib;

namespace SpawnerTweaks;

[HarmonyPatch(typeof(ItemStand), nameof(ItemStand.Awake))]
public class ItemStandAwake
{
  private static int Name = "override_name".GetStableHashCode();
  // string

  static void Postfix(ItemStand __instance)
  {
    if (!Configuration.configItemStand.Value) return;
    Helper.String(__instance.m_nview, Name, value => __instance.m_name = value);
  }
}

[HarmonyPatch(typeof(ItemStand), nameof(ItemStand.UpdateVisual))]
public class ItemStandUpdateVisual
{
  static int Respawn = "override_respawn".GetStableHashCode();
  // float (minutes)
  static int Changed = "override_changed".GetStableHashCode();
  // long (timestamp)
  static int Item = "item".GetStableHashCode();
  // string
  static int SpawnItem = "override_item".GetStableHashCode();
  // string,int
  static int Variant = "variant".GetStableHashCode();
  // string
  static void Prefix(ItemStand __instance)
  {
    if (!Configuration.configItemStand.Value) return;
    var obj = __instance;
    var view = obj.m_nview;
    if (!Helper.Owner(view)) return;
    Helper.Float(view, Respawn, respawn =>
    {
      Helper.Long(view, Changed, changed =>
      {
        var d = new DateTime(changed);
        if ((ZNet.instance.GetTime() - d).TotalMinutes > respawn)
        {
          if (obj.HaveAttachment()) obj.DropItem();
          view.GetZDO().Set(Changed, DateTime.MaxValue.Ticks / 2);
          Helper.String(view, SpawnItem, value =>
          {
            var split = value.Split(',');
            view.GetZDO().Set(Item, split[0]);
            if (split.Length > 1 && int.TryParse(split[1], out var variant))
              view.GetZDO().Set(Variant, variant);
            else
              view.GetZDO().Set(Variant, 0);
          });
        }
      });
    });
  }

}
[HarmonyPatch(typeof(ItemStand), nameof(ItemStand.DropItem))]
public class ItemStandDropItem
{
  static int Respawn = "override_respawn".GetStableHashCode();
  // float (minutes)
  static int Changed = "override_changed".GetStableHashCode();
  // long (timestamp)
  static void Prefix(ItemStand __instance)
  {
    if (!Configuration.configItemStand.Value) return;
    var obj = __instance;
    var view = obj.m_nview;
    if (!Helper.Owner(view)) return;
    if (!obj.HaveAttachment()) return;
    Helper.Float(view, Respawn, value =>
    {
      view.GetZDO().Set(Changed, ZNet.instance.GetTime().Ticks);
    });
  }
}
