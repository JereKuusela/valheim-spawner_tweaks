using BepInEx;
using HarmonyLib;
using Service;

namespace SpawnerTweaks;

[BepInPlugin(GUID, NAME, VERSION)]
public class SpawnerTweaksPlugin : BaseUnityPlugin
{
  public const string GUID = "spawner_tweaks";
  public const string NAME = "Spawner Tweaks";
  public const string VERSION = "1.31";
  public static ServerSync.ConfigSync ConfigSync = new(GUID)
  {
    DisplayName = NAME,
    CurrentVersion = VERSION,
    IsLocked = true,
    ModRequired = true
  };
  public void Awake()
  {
    ConfigWrapper wrapper = new("spawner_config", Config, ConfigSync);
    Configuration.Init(wrapper);
    new Harmony(GUID).PatchAll();
    Watcher.Setup(Config, Logger);
  }
}
