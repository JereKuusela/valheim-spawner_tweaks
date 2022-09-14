using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace SpawnerTweaks;
public class Helper {
  public static float Float(string arg, float defaultValue = 0f) {
    if (!float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
      return defaultValue;
    return result;
  }
  public static int Int(string arg, int defaultValue = 0) {
    if (!int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
      return defaultValue;
    return result;
  }

  public static GameObject? GetPrefab(string hashStr) {
    if (int.TryParse(hashStr, out var hash)) return GetPrefab(hash);
    return null;
  }
  public static GameObject? GetPrefab(int hash) {
    if (hash == 0) return null;
    var prefab = ZNetScene.instance.GetPrefab(hash);
    if (!prefab) return null;
    return prefab;
  }
  public static EffectList.EffectData? ParseEffect(string data) {
    var split = data.Split(',');
    EffectList.EffectData effect = new();
    effect.m_prefab = GetPrefab(split[0]);
    if (effect.m_prefab == null) return null;
    if (split.Length > 1 && int.TryParse(split[1], out var flag)) {
      effect.m_randomRotation = (flag & 1) > 0;
      effect.m_inheritParentRotation = (flag & 2) > 0;
      effect.m_scale = (flag & 4) > 0;
      effect.m_inheritParentScale = (flag & 8) > 0;
      effect.m_attach = (flag & 16) > 0;
    }
    if (split.Length > 2 && int.TryParse(split[2], out var variant))
      effect.m_variant = variant;
    if (split.Length > 3)
      effect.m_childTransform = split[3];
    return effect;
  }
  public static EffectList ParseEffects(string data) {
    var effects = data.Split('|').Select(effect => ParseEffect(effect)!).Where(effect => effect != null);
    EffectList list = new();
    list.m_effectPrefabs = effects.ToArray();
    return list;
  }
  public static SpawnArea.SpawnData? ParseSpawnData(string data) {
    var split = data.Split(',');
    SpawnArea.SpawnData spawn = new();
    spawn.m_prefab = GetPrefab(split[0]);
    if (spawn.m_prefab == null) return null;
    spawn.m_weight = 1f;
    spawn.m_minLevel = 1;
    spawn.m_maxLevel = 1;
    if (split.Length > 1)
      spawn.m_weight = Float(split[1], 1f);
    if (split.Length > 2) {
      spawn.m_minLevel = Int(split[2], 1);
      spawn.m_maxLevel = Int(split[2], 1);
    }
    if (split.Length > 3)
      spawn.m_maxLevel = Int(split[3], 1);
    return spawn;
  }
  public static List<SpawnArea.SpawnData> ParseSpawnsData(string data) {
    var spawns = data.Split('|').Select(spawn => ParseSpawnData(spawn)!).Where(spawn => spawn != null);
    return spawns.ToList();
  }

  public static void Float(ZNetView view, int hash, Action<float> action) {
    var value = view.GetZDO().GetFloat(hash, -1f);
    if (value < 0f) return;
    action(value);
  }
  public static void Long(ZNetView view, int hash, Action<long> action) {
    var value = view.GetZDO().GetLong(hash, -1L);
    if (value < 0L) return;
    action(value);
  }
  public static void Int(ZNetView view, int hash, Action<int> action) {
    var value = view.GetZDO().GetInt(hash, -1);
    if (value < 0) return;
    action(value);
  }
  public static void String(ZNetView view, int hash, Action<string> action) {
    var value = view.GetZDO().GetString(hash, "");
    if (value == "") return;
    action(value);
  }
  public static void Prefab(ZNetView view, int hash, Action<GameObject> action) {
    var value = view.GetZDO().GetInt(hash, 0);
    var prefab = Helper.GetPrefab(value);
    if (prefab == null) return;
    action(prefab);
  }

  public static int RollLevel(int min, int max, float chance) {
    var level = min;
    while (level < max && UnityEngine.Random.Range(0f, 100f) <= chance)
      level++;
    return level;
  }
}
