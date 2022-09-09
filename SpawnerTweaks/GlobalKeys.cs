using System;
using HarmonyLib;

namespace SpawnerTweaks;

public static class KeyUtils {

  public static bool MissingAllUniques(string value) {
    var keys = value.Split(',');
    foreach (var key in keys) {
      if (key.StartsWith("-") && Player.m_localPlayer?.HaveUniqueKey(key.Substring(1)) == false)
        return false;
      else if (Player.m_localPlayer?.HaveUniqueKey(key) == true)
        return false;
    }
    return true;
  }
  public static bool MissingAllGlobalKeys(string value) {
    var keys = value.Split(',');
    foreach (var key in keys) {
      if (key.StartsWith("-") && !ZoneSystem.instance.GetGlobalKey(key.Substring(1)))
        return false;
      else if (ZoneSystem.instance.GetGlobalKey(key))
        return false;
    }
    return true;
  }
}


[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SetGlobalKey))]
public class GlobalKeyTweaks {
  static bool Prefix(ZoneSystem __instance, string name) {
    var keys = name.Split(',');
    if (name.Length > 1) {
      foreach (var key in keys) __instance.SetGlobalKey(key);
      return false;
    }
    if (name.StartsWith("-", StringComparison.Ordinal)) {
      __instance.RemoveGlobalKey(name.Substring(1));
      return false;
    }
    return true;
  }
}

[HarmonyPatch(typeof(Player), nameof(Player.AddUniqueKey))]
public class UniqueKeyTweaks {
  static bool Prefix(Player __instance, string name) {
    var keys = name.Split(',');
    if (name.Length > 1) {
      foreach (var key in keys) __instance.AddUniqueKey(key);
      return false;
    }
    if (name.StartsWith("-", StringComparison.Ordinal)) {
      __instance.m_uniques.Remove(name.Substring(1));
      return false;
    }
    return true;
  }
}
