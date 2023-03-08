using HarmonyLib;

namespace SpawnerTweaks;

[HarmonyPatch(typeof(Smelter))]
public class SmelterPatches
{
  static int Conversion = "override_conversion".GetStableHashCode();
  // from,to|from,to|...
  static int MaxAmount = "override_maximum_amount".GetStableHashCode();
  // int
  static int MaxFuel = "override_maximum_fuel".GetStableHashCode();
  // int
  static int Fuel = "override_fuel".GetStableHashCode();
  // int (hash)
  static int FuelUsage = "override_fuel_usage".GetStableHashCode();
  // int
  static int Speed = "override_speed".GetStableHashCode();
  // float
  static int InputEffect = "override_input_effect".GetStableHashCode();
  // prefab,flags,variant,childTransform|prefab,flags,variant,childTransform|...
  static int FuelEffect = "override_fuel_effect".GetStableHashCode();
  // prefab,flags,variant,childTransform|prefab,flags,variant,childTransform|...
  static int OutputEffect = "override_output_effect".GetStableHashCode();
  // prefab,flags,variant,childTransform|prefab,flags,variant,childTransform|...

  [HarmonyPatch(nameof(Smelter.Awake)), HarmonyPostfix]
  static void Setup(Smelter __instance)
  {
    if (!Configuration.configCreatureSmelter.Value) return;
    var obj = __instance;
    var view = obj.m_nview;
    if (!view || !view.IsValid()) return;
    Helper.Int(view, FuelUsage, value => obj.m_fuelPerProduct = value);
    Helper.Int(view, MaxAmount, value => obj.m_maxOre = value);
    Helper.Int(view, MaxFuel, value => obj.m_maxFuel = value);
    Helper.Int(view, Fuel, value => obj.m_fuelItem = Helper.GetItem(value));
    Helper.Float(view, Speed, value => obj.m_secPerProduct = value);
    Helper.String(view, InputEffect, value => obj.m_oreAddedEffects = Helper.ParseEffects(value));
    Helper.String(view, FuelEffect, value => obj.m_fuelAddedEffects = Helper.ParseEffects(value));
    Helper.String(view, OutputEffect, value => obj.m_produceEffects = Helper.ParseEffects(value));
    Helper.String(view, Conversion, value => obj.m_conversion = Helper.ParseConversions(value));
  }
}
