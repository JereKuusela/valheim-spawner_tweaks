using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Service;
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
  public static GameObject? GetAttack(string hashStr)
  {
    if (int.TryParse(hashStr, out var hash)) return GetAttack(hash);
    return null;
  }
  public static GameObject GetAttack(int hash)
  {
    return ObjectDB.instance.GetItemPrefab(hash);
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
    EffectList.EffectData effect = new()
    {
      m_prefab = GetPrefab(split[0])
    };
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
    EffectList list = new()
    {
      m_effectPrefabs = effects.ToArray()
    };
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
      m_producedItems = split.Length > 2 ? Int(split[2], 1) : 1
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
    SpawnArea.SpawnData spawn = new()
    {
      m_prefab = GetPrefab(split[0])
    };
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
    DropTable.DropData drop = new()
    {
      m_item = GetPrefab(split[0]),
      m_weight = 1f,
      m_stackMin = 1,
      m_stackMax = 1
    };
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
    CharacterDrop.Drop drop = new()
    {
      m_prefab = GetPrefab(split[0]),
      m_chance = 1f,
      m_amountMin = 1,
      m_amountMax = 1
    };
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
    var drops = data.Split('|').Select(ParseDropData).Where(drop => drop.m_item != null);
    return drops.ToList();
  }
  public static List<CharacterDrop.Drop> ParseCharacterDropsData(string data)
  {
    var drops = data.Split('|').Select(ParseCharacterDropData).Where(drop => drop.m_prefab != null);
    return drops.ToList();
  }

  public static Humanoid.ItemSet ParseItemSet(string data)
  {
    Humanoid.ItemSet set = new()
    {
      m_name = "",
      m_items = data.Split(',').Select(GetAttack).Where(item => item).ToArray()
    };
    return set;
  }
  public static Humanoid.ItemSet[] ParseCharacterItemSets(string data)
  {
    var sets = data.Split('|').Select(ParseItemSet).Where(set => set.m_items.Length > 0);
    return sets.ToArray();
  }
  public static bool Float(ZNetView view, int hash, Action<float> action)
  {
    if (view == null || !view.IsValid()) return false;
    var value = view.GetZDO().GetFloat(hash, 0f);
    if (value == 0f) return false;
    action(value);
    return true;
  }
  public static void Float(ZNetView view, int hash, int legagyHash, Action<float> action)
  {
    if (!Float(view, hash, action))
      Float(view, legagyHash, action);
  }

  public static void Long(ZNetView view, int hash, Action<long> action)
  {
    if (view == null || !view.IsValid()) return;
    var value = view.GetZDO().GetLong(hash);
    if (value == 0L) return;
    action(value);
  }
  public static void Int(ZNetView view, int hash, Action<int> action)
  {
    if (view == null || !view.IsValid()) return;
    var value = view.GetZDO().GetInt(hash);
    if (value == 0) return;
    action(value);
  }
  public static void Bool(ZNetView view, int hash, Action action)
  {
    if (view == null || !view.IsValid()) return;
    var value = view.GetZDO().GetBool(hash);
    if (!value) return;
    action();
  }
  public static bool String(ZNetView view, int hash, Action<string> action)
  {
    if (view == null || !view.IsValid()) return false;
    var value = view.GetZDO().GetString(hash);
    if (value == "") return false;
    action(value);
    return true;
  }
  public static bool HashList(ZNetView view, int hash, Action<int[]> action)
  {
    if (view == null || !view.IsValid()) return false;
    var value = view.GetZDO().GetString(hash);
    if (value == "") return false;
    var list = value.Split(',').Select(s => s.GetStableHashCode()).ToArray();
    action(list);
    return true;
  }
  public static void String(ZNetView view, int hash, int legacyHash, Action<string> action)
  {
    if (!String(view, hash, action))
      String(view, legacyHash, action);
  }
  public static bool Prefab(ZNetView view, int hash, Action<GameObject> action)
  {
    if (view == null || !view.IsValid()) return false;
    var value = view.GetZDO().GetInt(hash);
    var prefab = GetPrefab(value);
    if (prefab == null) return false;
    action(prefab);
    return true;
  }
  public static void Prefab(ZNetView view, int hash, int legacyHash, Action<GameObject> action)
  {
    if (!Prefab(view, hash, action))
      Prefab(view, legacyHash, action);
  }

  public static void Item(ZNetView view, int hash, Action<ItemDrop> action)
  {
    if (view == null || !view.IsValid()) return;
    var value = view.GetZDO().GetInt(hash);
    var item = GetItem(value);
    if (item == null) return;
    action(item);
  }
  public static void Offset(ZNetView view, int hash, Transform initial, Action<Transform> action)
  {
    if (!initial)
    {
      GameObject go = new();
      go.transform.parent = view.transform;
      go.transform.localPosition = Vector3.zero;
      go.transform.localRotation = Quaternion.identity;
      initial = go.transform;
    }
    String(view, hash, value =>
    {
      var split = value.Split(',');
      var pos = initial.localPosition;
      pos.x = Float(split[0], pos.x);
      if (split.Length > 1)
        pos.z = Float(split[1], pos.z);
      if (split.Length > 2)
        pos.y = Float(split[2], pos.y);
      initial.localPosition = pos;
    });
    action(initial);
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
