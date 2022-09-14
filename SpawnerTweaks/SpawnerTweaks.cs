using BepInEx;
using HarmonyLib;
using Service;

namespace SpawnerTweaks;
[BepInPlugin(GUID, NAME, VERSION)]
public class SpawnerTweaks : BaseUnityPlugin {
  const string GUID = "spawner_tweaks";
  const string NAME = "Spawner Tweaks";
  const string VERSION = "1.1";
  public static ServerSync.ConfigSync ConfigSync = new(GUID)
  {
    DisplayName = NAME,
    CurrentVersion = VERSION,
    ModRequired = true,
    IsLocked = true
  };
  public void Awake() {
    ConfigWrapper wrapper = new("spawner_config", Config, ConfigSync);
    Configuration.Init(wrapper);
    new Harmony(GUID).PatchAll();
  }
}

