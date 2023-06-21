using System;
using HarmonyLib;
using Service;

namespace SpawnerTweaks;

[HarmonyPatch(typeof(Container))]
public class ContainerPatches
{


  [HarmonyPatch(nameof(Container.Awake)), HarmonyPostfix]
  static void Setup(Container __instance)
  {
    if (!Configuration.configContainer.Value) return;
    Helper.String(__instance.m_nview, Hash.Name, value => __instance.m_name = value);
  }

  [HarmonyPatch(nameof(Container.AddDefaultItems)), HarmonyPrefix]
  static void ReplaceDefaultItems(Container __instance)
  {
    if (!Configuration.configContainer.Value) return;
    var obj = __instance;
    Helper.Int(obj.m_nview, Hash.MinAmount, value => obj.m_defaultItems.m_dropMin = value);
    Helper.Int(obj.m_nview, Hash.MaxAmount, value => obj.m_defaultItems.m_dropMax = value);
    Helper.String(obj.m_nview, Hash.Items, value =>
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
      obj.m_nview.GetZDO().Set(Hash.Changed, DateTime.MaxValue.Ticks / 2);
      obj.m_nview.GetZDO().Set(ZDOVars.s_addedDefaultItems, false);
      obj.m_inventory.RemoveAll();
      obj.AddDefaultItems();
      obj.m_nview.GetZDO().Set(ZDOVars.s_addedDefaultItems, true);
    }
  }

  [HarmonyPatch(nameof(Container.OnContainerChanged)), HarmonyPostfix]
  static void TriggerRespawn(Container __instance)
  {
    if (__instance.m_loading) return;
    if (!Configuration.configContainer.Value) return;
    Helper.Bool(__instance.m_nview, ZDOVars.s_addedDefaultItems, () =>
    {
      Helper.Float(__instance.m_nview, Hash.Respawn, value =>
      {
        __instance.m_nview.GetZDO().Set(Hash.Changed, ZNet.instance.GetTime().Ticks);
      });
    });
  }
}
