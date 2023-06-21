using HarmonyLib;
using Service;

namespace SpawnerTweaks;

[HarmonyPatch(typeof(Smelter))]
public class SmelterPatches
{
  [HarmonyPatch(nameof(Smelter.Awake)), HarmonyPostfix]
  static void Setup(Smelter __instance)
  {
    if (!Configuration.configSmelter.Value) return;
    var obj = __instance;
    var view = obj.m_nview;
    if (!view || !view.IsValid()) return;
    Helper.Int(view, Hash.FuelUsage, value => obj.m_fuelPerProduct = value);
    Helper.Int(view, Hash.MaxAmount, value => obj.m_maxOre = value);
    Helper.Int(view, Hash.MaxFuel, value => obj.m_maxFuel = value);
    Helper.Item(view, Hash.Fuel, value => obj.m_fuelItem = value);
    Helper.Float(view, Hash.Speed, value => obj.m_secPerProduct = value);
    Helper.String(view, Hash.InputEffect, value => obj.m_oreAddedEffects = Helper.ParseEffects(value));
    Helper.String(view, Hash.FuelEffect, value => obj.m_fuelAddedEffects = Helper.ParseEffects(value));
    Helper.String(view, Hash.OutputEffect, value => obj.m_produceEffects = Helper.ParseEffects(value));
    Helper.String(view, Hash.Conversion, value => obj.m_conversion = Helper.ParseSmelterConversions(value));
  }
}
