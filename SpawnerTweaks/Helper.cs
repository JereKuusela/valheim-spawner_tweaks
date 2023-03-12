using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace SpawnerTweaks;
public class Helper
{
  public static float Float(string arg, float defaultValue)
  {
    if (!float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
      return defaultValue;
    return result;
  }
  public static float? Float(string arg)
  {
    if (!float.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
      return null;
    return result;
  }
  public static int Int(string arg, int defaultValue)
  {

    if (!int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
      return defaultValue;
    return result;
  }
  public static int? Int(string arg)
  {
    if (!int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
      return null;
    return result;
  }

  public static GameObject? GetPrefab(string hashStr)
  {
    if (int.TryParse(hashStr, out var hash)) return GetPrefab(hash);
    return null;
  }
  public static ItemDrop? GetItem(string hashStr)
  {
    if (int.TryParse(hashStr, out var hash)) return GetItem(hash);
    return null;
  }
  public static ItemDrop? GetItem(int hash)
  {
    return GetPrefab(hash)?.GetComponent<ItemDrop>();
  }
  public static GameObject? GetPrefab(int hash)
  {
    if (hash == 0) return null;
    var prefab = ZNetScene.instance.GetPrefab(hash);
    if (!prefab) return null;
    return prefab;
  }
  public static EffectList.EffectData? ParseEffect(string data)
  {
    var split = data.Split(',');
    EffectList.EffectData effect = new();
    effect.m_prefab = GetPrefab(split[0]);
    if (effect.m_prefab == null) return null;
    if (split.Length > 1 && int.TryParse(split[1], out var flag))
    {
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
  public static HitData.DamageModifiers ParseDamageModifiers(string data)
  {
    HitData.DamageModifiers modifiers = new();
    var values = data.Split('|');
    foreach (var value in values)
    {
      var split = value.Split(',');
      if (split.Length < 2) continue;
      var type = (HitData.DamageType)Int(split[0], 0);
      var modifier = (HitData.DamageModifier)Int(split[1], 0);
      if (type == HitData.DamageType.Blunt)
        modifiers.m_blunt = modifier;
      if (type == HitData.DamageType.Chop)
        modifiers.m_chop = modifier;
      if (type == HitData.DamageType.Fire)
        modifiers.m_fire = modifier;
      if (type == HitData.DamageType.Frost)
        modifiers.m_frost = modifier;
      if (type == HitData.DamageType.Lightning)
        modifiers.m_lightning = modifier;
      if (type == HitData.DamageType.Pickaxe)
        modifiers.m_pickaxe = modifier;
      if (type == HitData.DamageType.Pierce)
        modifiers.m_pierce = modifier;
      if (type == HitData.DamageType.Poison)
        modifiers.m_poison = modifier;
      if (type == HitData.DamageType.Slash)
        modifiers.m_slash = modifier;
      if (type == HitData.DamageType.Spirit)
        modifiers.m_spirit = modifier;
    }
    return modifiers;
  }
  public static EffectList ParseEffects(string data)
  {
    var effects = data.Split('|').Select(effect => ParseEffect(effect)!).Where(effect => effect != null);
    EffectList list = new();
    list.m_effectPrefabs = effects.ToArray();
    return list;
  }
  public static Smelter.ItemConversion? ParseSmelterConversion(string data)
  {
    var split = data.Split(',');
    if (split.Length != 2) return null;
    return new Smelter.ItemConversion()
    {
      m_from = GetItem(split[0]),
      m_to = GetItem(split[1])
    };
  }
  public static Fermenter.ItemConversion? ParseFermenterConversion(string data)
  {
    var split = data.Split(',');
    if (split.Length < 2) return null;
    return new Fermenter.ItemConversion()
    {
      m_from = GetItem(split[0]),
      m_to = GetItem(split[1]),
      m_producedItems = split.Length > 2 ? Helper.Int(split[2], 1) : 1
    };
  }
  public static List<Smelter.ItemConversion> ParseSmelterConversions(string data)
  {
    return data.Split('|').Select(conversion => ParseSmelterConversion(conversion)!).Where(conversion => conversion != null).ToList();
  }
  public static List<Fermenter.ItemConversion> ParseFermenterConversions(string data)
  {
    return data.Split('|').Select(conversion => ParseFermenterConversion(conversion)!).Where(conversion => conversion != null).ToList();
  }
  public static SpawnArea.SpawnData? ParseSpawnData(string data)
  {
    var split = data.Split(',');
    SpawnArea.SpawnData spawn = new();
    spawn.m_prefab = GetPrefab(split[0]);
    if (spawn.m_prefab == null) return null;
    spawn.m_weight = 1f;
    spawn.m_minLevel = 1;
    spawn.m_maxLevel = 1;
    if (split.Length > 1)
      spawn.m_weight = Float(split[1], 1f);
    if (split.Length > 2)
    {
      spawn.m_minLevel = Int(split[2], 1);
      spawn.m_maxLevel = Int(split[2], 1);
    }
    if (split.Length > 3)
      spawn.m_maxLevel = Int(split[3], 1);
    return spawn;
  }
  public static List<SpawnArea.SpawnData> ParseSpawnsData(string data)
  {
    var spawns = data.Split('|').Select(spawn => ParseSpawnData(spawn)!).Where(spawn => spawn != null);
    return spawns.ToList();
  }


  public static DropTable.DropData ParseDropData(string data)
  {
    var split = data.Split(',');
    DropTable.DropData drop = new();
    drop.m_item = GetPrefab(split[0]);
    drop.m_weight = 1f;
    drop.m_stackMin = 1;
    drop.m_stackMax = 1;
    if (split.Length > 1)
      drop.m_weight = Float(split[1], 1f);
    if (split.Length > 2)
    {
      drop.m_stackMin = Int(split[2], 1);
      drop.m_stackMax = Int(split[2], 1);
    }
    if (split.Length > 3)
      drop.m_stackMax = Int(split[3], 1);
    return drop;
  }
  public static CharacterDrop.Drop ParseCharacterDropData(string data)
  {
    var split = data.Split(',');
    CharacterDrop.Drop drop = new();
    drop.m_prefab = GetPrefab(split[0]);
    drop.m_chance = 1f;
    drop.m_amountMin = 1;
    drop.m_amountMax = 1;
    if (split.Length > 1)
      drop.m_chance = Float(split[1], 1f);
    if (split.Length > 2)
    {
      drop.m_amountMin = Int(split[2], 1);
      drop.m_amountMax = Int(split[2], 1) + 1; // Valheim bug.
    }
    if (split.Length > 3)
      drop.m_amountMax = Int(split[3], 1) + 1; // Valheim bug.
    if (split.Length > 4)
    {
      drop.m_levelMultiplier = (Int(split[4], 0) & 1) > 0;
      drop.m_onePerPlayer = (Int(split[4], 0) & 2) > 0;
    }
    return drop;
  }
  public static List<DropTable.DropData> ParseDropsData(string data)
  {
    var drops = data.Split('|').Select(drop => ParseDropData(drop)).Where(drop => drop.m_item != null);
    return drops.ToList();
  }
  public static List<CharacterDrop.Drop> ParseCharacterDropsData(string data)
  {
    var drops = data.Split('|').Select(drop => ParseCharacterDropData(drop)).Where(drop => drop.m_prefab != null);
    return drops.ToList();
  }

  public static void Float(ZNetView view, int hash, Action<float> action)
  {
    if (view == null || !view.IsValid()) return;
    var value = view.GetZDO().GetFloat(hash, -1f);
    if (value < 0f) return;
    action(value);
  }
  public static void Long(ZNetView view, int hash, Action<long> action)
  {
    if (view == null || !view.IsValid()) return;
    var value = view.GetZDO().GetLong(hash, -1L);
    if (value < 0L) return;
    action(value);
  }
  public static void Int(ZNetView view, int hash, Action<int> action)
  {
    if (view == null || !view.IsValid()) return;
    var value = view.GetZDO().GetInt(hash, -1);
    if (value < 0) return;
    action(value);
  }
  public static void Bool(ZNetView view, int hash, Action<bool> action)
  {
    if (view == null || !view.IsValid()) return;
    var value = view.GetZDO().GetBool(hash, false);
    action(value);
  }
  public static void String(ZNetView view, int hash, Action<string> action)
  {
    if (view == null || !view.IsValid()) return;
    var value = view.GetZDO().GetString(hash, "");
    if (value == "") return;
    action(value);
  }
  public static void Prefab(ZNetView view, int hash, Action<GameObject> action)
  {
    if (view == null || !view.IsValid()) return;
    var value = view.GetZDO().GetInt(hash, 0);
    var prefab = Helper.GetPrefab(value);
    if (prefab == null) return;
    action(prefab);
  }
  public static void Item(ZNetView view, int hash, Action<ItemDrop> action)
  {
    if (view == null || !view.IsValid()) return;
    var value = view.GetZDO().GetInt(hash, 0);
    var item = Helper.GetItem(value);
    if (item == null) return;
    action(item);
  }

  public static int RollLevel(int min, int max, float chance)
  {
    var level = min;
    while (level < max && UnityEngine.Random.Range(0f, 100f) <= chance)
      level++;
    return level;
  }

  public static bool Owner(ZNetView view) => view && view.IsValid() && view.IsOwner();
}
