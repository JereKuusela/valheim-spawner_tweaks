using System;
using HarmonyLib;

namespace SpawnerTweaks;

[HarmonyPatch(typeof(ItemStand))]
public class ItemStandPatches
{
  private static readonly int Name = "override_name".GetStableHashCode();
  // string
  static readonly int Respawn = "override_respawn".GetStableHashCode();
  // float (minutes)
  static readonly int Changed = "override_changed".GetStableHashCode();
  // long (timestamp)
  static readonly int Item = "item".GetStableHashCode();
  // string
  static readonly int SpawnItem = "override_item".GetStableHashCode();
  // string,int
  static readonly int Variant = "variant".GetStableHashCode();
  // string

  [HarmonyPatch(nameof(ItemStand.Awake)), HarmonyPostfix]
  static void Setup(ItemStand __instance)
  {
    if (!Configuration.configItemStand.Value) return;
    Helper.String(__instance.m_nview, Name, value => __instance.m_name = value);
  }

  [HarmonyPatch(nameof(ItemStand.UpdateVisual)), HarmonyPrefix]
  static void RespawnItem(ItemStand __instance)
  {
    if (!Configuration.configItemStand.Value) return;
    var obj = __instance;
    var view = obj.m_nview;
    if (!Helper.Owner(view)) return;
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
  }
  [HarmonyPatch(nameof(ItemStand.DropItem)), HarmonyPrefix]
  static void TriggerRespawn(ItemStand __instance)
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
