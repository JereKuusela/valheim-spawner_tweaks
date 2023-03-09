using HarmonyLib;

namespace SpawnerTweaks;

[HarmonyPatch(typeof(Fermenter))]
public class FermenterPatches
{
  static int Conversion = "override_conversion".GetStableHashCode();
  // from,to,amount|from,to,amount|...
  static int Speed = "override_speed".GetStableHashCode();
  // float
  static int InputEffect = "override_input_effect".GetStableHashCode();
  // prefab,flags,variant,childTransform|prefab,flags,variant,childTransform|...
  static int UseEffect = "override_use_effect".GetStableHashCode();
  // prefab,flags,variant,childTransform|prefab,flags,variant,childTransform|...
  static int OutputEffect = "override_output_effect".GetStableHashCode();
  // prefab,flags,variant,childTransform|prefab,flags,variant,childTransform|...

  [HarmonyPatch(nameof(Fermenter.Awake)), HarmonyPostfix]
  static void Setup(Fermenter __instance)
  {
    if (!Configuration.configFermenter.Value) return;
    var obj = __instance;
    var view = obj.m_nview;
    if (!view || !view.IsValid()) return;
    Helper.Float(view, Speed, value => obj.m_fermentationDuration = value);
    Helper.String(view, InputEffect, value => obj.m_addedEffects = Helper.ParseEffects(value));
    Helper.String(view, UseEffect, value => obj.m_tapEffects = Helper.ParseEffects(value));
    Helper.String(view, OutputEffect, value => obj.m_spawnEffects = Helper.ParseEffects(value));
    Helper.String(view, Conversion, value => obj.m_conversion = Helper.ParseFermenterConversions(value));
  }
}
