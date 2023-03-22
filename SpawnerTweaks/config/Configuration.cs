using BepInEx.Configuration;
using Service;
namespace SpawnerTweaks;
public class Configuration
{
#nullable disable
  public static ConfigEntry<bool> configOfferingBowl;
  public static ConfigEntry<bool> configComponent;
  public static ConfigEntry<bool> configPickable;
  public static ConfigEntry<bool> configContainer;
  public static ConfigEntry<bool> configItemStand;
  public static ConfigEntry<bool> configSpawnArea;
  public static ConfigEntry<bool> configCharacter;
  public static ConfigEntry<bool> configCreatureSpawner;
  public static ConfigEntry<bool> configSmelter;
  public static ConfigEntry<bool> configBeehive;
  public static ConfigEntry<bool> configFermenter;
  public static ConfigEntry<bool> configNoCreatureSpawnerSuppression;
  public static ConfigEntry<bool> configNoCreatureRespawnerSuppression;
#nullable enable
  public static void Init(ConfigWrapper wrapper)
  {
    var section = "1. General";
    configNoCreatureSpawnerSuppression = wrapper.Bind(section, "No spawn point suppression (one time)", true, "One time spawn points can't be suppressed with player base (even when configured to respawn).");
    configNoCreatureSpawnerSuppression.SettingChanged += (s, e) => NoSuppression.Update();
    configNoCreatureRespawnerSuppression = wrapper.Bind(section, "No spawn point suppression (respawning)", false, "Respawning spawn points can't be suppressed with player base (even when configured to one time).");
    configNoCreatureRespawnerSuppression.SettingChanged += (s, e) => NoSuppression.Update();
    configCreatureSpawner = wrapper.Bind(section, "Spawn points", true, "Spawn point properties can be overridden.");
    configSmelter = wrapper.Bind(section, "Smelters", true, "Smelter properties can be overridden.");
    configBeehive = wrapper.Bind(section, "Beehives", true, "Beehive properties can be overridden.");
    configFermenter = wrapper.Bind(section, "Fermenters", true, "Fermenter properties can be overridden.");
    configComponent = wrapper.Bind(section, "Components", true, "Altars, pickables, spawners, etc. can be attached to any object.");
    configPickable = wrapper.Bind(section, "Pickables", true, "Pickable properties can be overridden.");
    configCharacter = wrapper.Bind(section, "Creatures", true, "Creature properties can be overridden.");
    configContainer = wrapper.Bind(section, "Chests", true, "Chest properties can be overridden.");
    configItemStand = wrapper.Bind(section, "Item stands", true, "Item stand properties can be overridden.");
    configSpawnArea = wrapper.Bind(section, "Spawners", true, "Spawner properties can be overridden.");
    configOfferingBowl = wrapper.Bind(section, "Boss altars", true, "Boss altar properties can be overridden.");
  }
}
