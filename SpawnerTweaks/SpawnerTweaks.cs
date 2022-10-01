using BepInEx;
using HarmonyLib;
using Service;

namespace SpawnerTweaks;
[BepInPlugin(GUID, NAME, VERSION)]
public class Plugin : BaseUnityPlugin {
  const string GUID = "spawner_tweaks";
  const string NAME = "Spawner Tweaks";
  const string VERSION = "1.3";
  public static ServerSync.ConfigSync ConfigSync = new(GUID, NAME, VERSION);
  public void Awake() {
    ConfigWrapper wrapper = new("spawner_config", Config, ConfigSync);
    Configuration.Init(wrapper);
    new Harmony(GUID).PatchAll();
  }
}

