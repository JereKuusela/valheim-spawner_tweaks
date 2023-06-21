using HarmonyLib;
using Service;

namespace SpawnerTweaks;

[HarmonyPatch(typeof(Fermenter))]
public class FermenterPatches
{
  [HarmonyPatch(nameof(Fermenter.Awake)), HarmonyPostfix]
  static void Setup(Fermenter __instance)
  {
    if (!Configuration.configFermenter.Value) return;
    var obj = __instance;
    var view = obj.m_nview;
    if (!view || !view.IsValid()) return;
    Helper.Float(view, Hash.Speed, value => obj.m_fermentationDuration = value);
    Helper.String(view, Hash.InputEffect, value => obj.m_addedEffects = Helper.ParseEffects(value));
    Helper.String(view, Hash.UseEffect, value => obj.m_tapEffects = Helper.ParseEffects(value));
    Helper.String(view, Hash.OutputEffect, value => obj.m_spawnEffects = Helper.ParseEffects(value));
    Helper.String(view, Hash.Conversion, value => obj.m_conversion = Helper.ParseFermenterConversions(value));
  }
}
