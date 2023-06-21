using System;
using HarmonyLib;
using Service;

namespace SpawnerTweaks;

[HarmonyPatch(typeof(ItemStand))]
public class ItemStandPatches
{
  [HarmonyPatch(nameof(ItemStand.Awake)), HarmonyPostfix]
  static void Setup(ItemStand __instance)
  {
    if (!Configuration.configItemStand.Value) return;
    Helper.String(__instance.m_nview, Hash.Name, value => __instance.m_name = value);
  }

  [HarmonyPatch(nameof(ItemStand.UpdateVisual)), HarmonyPrefix]
  static void RespawnItem(ItemStand __instance)
  {
    if (!Configuration.configItemStand.Value) return;
    var obj = __instance;
    var view = obj.m_nview;
    if (!Helper.Owner(view)) return;
    var respawnContents = false;
    Helper.Float(obj.m_nview, Hash.Respawn, respawn =>
    {
      respawnContents = true;
      Helper.Long(obj.m_nview, Hash.Changed, changed =>
      {
        var d = new DateTime(changed);
        respawnContents = (ZNet.instance.GetTime() - d).TotalMinutes >= respawn;
      });
    });
    if (respawnContents)
    {
      if (obj.HaveAttachment()) obj.DropItem();
      view.GetZDO().Set(Hash.Changed, DateTime.MaxValue.Ticks / 2);
      Helper.String(view, Hash.Item, value =>
      {
        var split = value.Split(',');
        view.GetZDO().Set(ZDOVars.s_item, split[0]);
        if (split.Length > 1 && int.TryParse(split[1], out var variant))
          view.GetZDO().Set(ZDOVars.s_variant, variant);
        else
          view.GetZDO().Set(ZDOVars.s_variant, 0);
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
    Helper.Float(view, Hash.Respawn, value =>
    {
      view.GetZDO().Set(Hash.Changed, ZNet.instance.GetTime().Ticks);
    });
  }
}
