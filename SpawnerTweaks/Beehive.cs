using HarmonyLib;
using Service;

namespace SpawnerTweaks;

[HarmonyPatch(typeof(Beehive))]
public class BeehivePatches
{
  [HarmonyPatch(nameof(Beehive.Awake)), HarmonyPostfix]
  static void Setup(Beehive __instance)
  {
    if (!Configuration.configBeehive.Value) return;
    var obj = __instance;
    var view = obj.m_nview;
    if (!view || !view.IsValid()) return;
    if (!__instance.m_beeEffect) __instance.m_beeEffect = new();
    Helper.Int(view, Hash.MaxAmount, value => obj.m_maxHoney = value);
    Helper.Int(view, Hash.Biome, value => obj.m_biome = (Heightmap.Biome)value);
    Helper.Int(view, Hash.SpawnCondition, value => obj.m_effectOnlyInDaylight = value != 1);
    Helper.Item(view, Hash.Spawn, value => obj.m_honeyItem = value);
    Helper.Float(view, Hash.Speed, value => obj.m_secPerUnit = value);
    Helper.Float(view, Hash.MaxCover, value => obj.m_maxCover = value);
    Helper.Offset(view, Hash.CoverOffset, obj.m_coverPoint, value => obj.m_coverPoint = value);
    Helper.Offset(view, Hash.SpawnOffset, obj.m_spawnPoint, value => obj.m_spawnPoint = value);
    Helper.String(view, Hash.TextBiome, value => obj.m_areaText = value);
    Helper.String(view, Hash.TextSpace, value => obj.m_freespaceText = value);
    Helper.String(view, Hash.TextSleep, value => obj.m_sleepText = value);
    Helper.String(view, Hash.TextHappy, value => obj.m_happyText = value);
    Helper.String(view, Hash.TextCheck, value => obj.m_checkText = value);
    Helper.String(view, Hash.TextExtract, value => obj.m_extractText = value);
    Helper.String(view, Hash.Name, value => obj.m_name = value);
  }
}
