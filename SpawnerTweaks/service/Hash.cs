namespace Service;

public static class Hash
{
  public static readonly int MaxAmount = "override_maximum_amount".GetStableHashCode();
  // int
  public static readonly int Spawn = "override_spawn".GetStableHashCode();
  // int
  public static readonly int Command = "override_command".GetStableHashCode();
  // string
  public static readonly int Biome = "override_biome".GetStableHashCode();
  // int (biome)
  public static readonly int Speed = "override_speed".GetStableHashCode();
  // float
  public static readonly int MaxCover = "override_maximum_cover".GetStableHashCode();
  // float
  public static readonly int SpawnCondition = "override_spawn_condition".GetStableHashCode();
  // flag (1 = day only, 2 = night only)
  public static readonly int CoverOffset = "override_cover_offset".GetStableHashCode();
  // float,float,float (x,z,y)
  public static readonly int SpawnOffset = "override_spawn_offset".GetStableHashCode();
  // float,float,float (x,z,y)
  // meters
  public static readonly int SpawnEffect = "override_spawn_effect".GetStableHashCode();
  // prefab,flags,variant,childTransform|prefab,flags,variant,childTransform|...
  public static readonly int TextBiome = "override_text_biome".GetStableHashCode();
  // string
  public static readonly int TextSpace = "override_text_space".GetStableHashCode();
  // string
  public static readonly int TextSleep = "override_text_sleep".GetStableHashCode();
  // string
  public static readonly int TextHappy = "override_text_happy".GetStableHashCode();
  // string
  public static readonly int TextCheck = "override_text_check".GetStableHashCode();
  // string
  public static readonly int TextExtract = "override_text_extract".GetStableHashCode();
  // string
  public static readonly int Name = "override_name".GetStableHashCode();
  // string
  public static readonly int Faction = "override_faction".GetStableHashCode();
  // string
  public static readonly int Boss = "override_boss".GetStableHashCode();
  // int (-1 false, 0 default, 1 true)
  public static readonly int Resistances = "override_resistances".GetStableHashCode();
  // type,modifier|...
  public static readonly int Items = "override_items".GetStableHashCode();
  // id,weight,min,max|...
  public static readonly int Attacks = "override_attacks".GetStableHashCode();
  // id|id|id|...
  public static readonly int Component = "override_component".GetStableHashCode();
  public static readonly int MinAmount = "override_minimum_amount".GetStableHashCode();
  // int
  public static readonly int Respawn = "override_respawn".GetStableHashCode();
  // float (minutes)
  public static readonly int Changed = "override_changed".GetStableHashCode();
  // long (timestamp)
  public static readonly int MinLevel = "override_minimum_level".GetStableHashCode();
  // int
  public static readonly int MaxLevel = "override_maximum_level".GetStableHashCode();
  // int
  public static readonly int TriggerDistance = "override_trigger_distance".GetStableHashCode();
  // float (meters)
  public static readonly int TriggerNoise = "override_trigger_noise".GetStableHashCode();
  // float (meters)
  public static readonly int LevelChance = "override_level_chance".GetStableHashCode();
  // float (percent)
  public static readonly int Health = "override_health".GetStableHashCode();
  // float
  public static readonly int Data = "override_data".GetStableHashCode();
  // string
  public static readonly int Conversion = "override_conversion".GetStableHashCode();
  // from,to,amount|from,to,amount|...
  public static readonly int InputEffect = "override_input_effect".GetStableHashCode();
  // prefab,flags,variant,childTransform|prefab,flags,variant,childTransform|...
  public static readonly int UseEffect = "override_use_effect".GetStableHashCode();
  // prefab,flags,variant,childTransform|prefab,flags,variant,childTransform|...
  public static readonly int OutputEffect = "override_output_effect".GetStableHashCode();
  // prefab,flags,variant,childTransform|prefab,flags,variant,childTransform|...
  public static readonly int Item = "override_item".GetStableHashCode();
  // string,int
  public static readonly int SpawnItem = "override_spawn_item".GetStableHashCode();
  // prefab
  public static readonly int Amount = "override_amount".GetStableHashCode();
  // int
  public static readonly int StartEffect = "override_start_effect".GetStableHashCode();
  // prefab,flags,variant,childTransform|prefab,flags,variant,childTransform|...
  public static readonly int Text = "override_text".GetStableHashCode();
  // string
  public static readonly int Delay = "override_delay".GetStableHashCode();
  // float (seconds)
  public static readonly int ItemOffset = "override_item_offset".GetStableHashCode();
  // float,float,float (x,z,y)
  public static readonly int SpawnMaxY = "override_spawn_max_y".GetStableHashCode();
  // float (meters)
  public static readonly int SpawnRadius = "override_spawn_radius".GetStableHashCode();
  // float (meters)
  public static readonly int ItemStandPrefix = "override_item_stand_prefix".GetStableHashCode();
  // string
  public static readonly int ItemStandRange = "override_item_stand_range".GetStableHashCode();
  // float (meters)
  public static readonly int SpawnTime = "spawn_time".GetStableHashCode();
  public static readonly int PickableSpawn = "override_pickable_spawn".GetStableHashCode();
  // prefab
  public static readonly int PickableRespawn = "override_pickable_respawn".GetStableHashCode();
  // int (minutes)
  public static readonly int RequiredGlobalKey = "override_required_globalkey".GetStableHashCode();
  // hash1,hash2,hash3,...
  public static readonly int ForbiddenGlobalKey = "override_forbidden_globalkey".GetStableHashCode();
  // hash1,hash2,hash3,...
  public static readonly int MaxFuel = "override_maximum_fuel".GetStableHashCode();
  // int
  public static readonly int Fuel = "override_fuel".GetStableHashCode();
  // int (hash)
  public static readonly int FuelUsage = "override_fuel_usage".GetStableHashCode();
  // int
  public static readonly int FuelEffect = "override_fuel_effect".GetStableHashCode();
  // prefab,flags,variant,childTransform|prefab,flags,variant,childTransform|...
  public static readonly int SpawnAreaSpawn = "override_spawnarea_spawn".GetStableHashCode();
  // prefab,weight,minLevel,maxLevel|prefab,weight,minLevel,maxLevel|...
  public static readonly int SpawnAreaRespawn = "override_spawnarea_respawn".GetStableHashCode();
  // float (seconds)
  public static readonly int MaxNear = "override_max_near".GetStableHashCode();
  // int
  public static readonly int MaxTotal = "override_max_total".GetStableHashCode();
  // int
  public static readonly int NearRadius = "override_near_radius".GetStableHashCode();
  // float (meters)
  public static readonly int FarRadius = "override_far_radius".GetStableHashCode();
  // float (meters
}