using System;
using HarmonyLib;
using UnityEngine;
namespace SpawnerTweaks;
public class MiniTameable : MonoBehaviour, Interactable, TextReceiver
{
  int petHash = "fx_wolf_pet".GetStableHashCode();
  public void Awake()
  {
    view = base.GetComponent<ZNetView>();
    baseAI = base.GetComponent<BaseAI>();
    character = base.GetComponent<Character>();
    animalAI = base.GetComponent<AnimalAI>();
    monsterAI = base.GetComponent<MonsterAI>();
    if (view.IsValid())
    {
      view.Register<ZDOID, bool>("Command", new Action<long, ZDOID, bool>(RPC_Command));
      view.Register<string>("SetName", new Action<long, string>(RPC_SetName));
    }
    var prefab = ZNetScene.instance.GetPrefab(petHash);
    if (prefab)
    {
      petEffect.m_effectPrefabs = new EffectList.EffectData[]{
        new () { m_enabled = true, m_prefab = prefab, m_variant = -1 }
      };
    }
  }
  public void Update()
  {
    this.UpdateSavedFollowTarget();
  }
  public float updateTimer = 0f;
  public void FixedUpdate()
  {
    if (!view.IsValid()) return;
    updateTimer += Time.deltaTime;
    if (updateTimer >= 0.05f)
    {
      if (follow != null)
        Follow(follow, 0.05f);
      updateTimer -= 0.05f;
    }
  }

  static int Creator = "creator".GetStableHashCode();
  public bool IsOwner()
  {
    if (!Configuration.configTamedCommandControl.Value) return true;
    var isOwner = Plugin.ConfigSync.IsAdmin || ZNet.instance.IsServer();
    Helper.Long(view, Creator, value =>
    {
      var isCreator = value == Game.instance.GetPlayerProfile().GetPlayerID();
      isOwner |= isCreator;
    });
    return isOwner;
  }

  public void Follow(GameObject go, float dt)
  {
    if (animalAI != null)
    {
      baseAI.SetAlerted(false);
      animalAI.m_updateTargetTimer += dt;
      animalAI.m_target = null;
    }
    var distance = Vector3.Distance(go.transform.position, transform.position);
    if (distance < 3f)
      baseAI.StopMoving();
    else
      baseAI.MoveTo(dt, go.transform.position, 0f, distance > 10f);
  }

  public string GetHoverText()
  {
    if (!view.IsValid()) return "";
    string text = Localization.instance.Localize(character.m_name);
    text += Localization.instance.Localize(" ( $hud_tame, $hud_tamehappy )");
    text += Localization.instance.Localize("\n[<color=yellow><b>$KEY_Use</b></color>] $hud_pet");
    if (IsOwner())
      text += Localization.instance.Localize("\n[<color=yellow><b>$KEY_AltPlace + $KEY_Use</b></color>] $hud_rename");
    return text;
  }

  public bool Interact(Humanoid user, bool hold, bool alt)
  {
    if (!view.IsValid()) return false;
    if (hold) return false;
    if (alt)
    {
      if (!IsOwner()) return false;
      SetName();
      return true;
    }
    if (Time.time - m_lastPetTime <= 1f) return false;
    m_lastPetTime = Time.time;
    petEffect.Create(base.transform.position, base.transform.rotation, null, 1f, -1);
    if (IsOwner()) Command(user, true);
    else user.Message(MessageHud.MessageType.Center, GetHoverName() + " $hud_tamelove", 0, null);
    return true;
  }

  public string GetHoverName()
  {
    var text = GetText();
    if (text.Length > 0) return text;
    return Localization.instance.Localize(character.m_name);
  }

  public void SetName() => TextInput.instance.RequestText(this, "$hud_rename", 10);

  int NameHash = "TamedName".GetStableHashCode();
  public string GetText()
  {
    if (!view.IsValid()) return "";
    return view.GetZDO().GetString(NameHash, "");
  }

  public void SetText(string text)
  {
    if (!view.IsValid()) return;
    view.InvokeRPC("SetName", text);
  }

  private void RPC_SetName(long sender, string name)
  {
    if (!view.IsValid() || !view.IsOwner()) return;
    view.GetZDO().Set(NameHash, name);
  }

  public bool UseItem(Humanoid user, ItemDrop.ItemData item) => false;

  public void Command(Humanoid user, bool message) => view.InvokeRPC("Command", user.GetZDOID(), message);

  public Player? GetPlayer(ZDOID characterID)
  {
    var obj = ZNetScene.instance.FindInstance(characterID);
    return obj?.GetComponent<Player>();
  }

  public bool CommandAnimalAI(GameObject obj)
  {
    if (follow)
    {
      follow = null;
      baseAI.SetPatrolPoint();
      return false;
    }
    baseAI.ResetPatrolPoint();
    follow = obj;
    return true;
  }
  public bool CommandMonsterAI(GameObject obj)
  {
    if (monsterAI == null) return false;
    if (monsterAI.GetFollowTarget())
    {
      monsterAI.SetFollowTarget(null);
      monsterAI.SetPatrolPoint();
      return false;
    }
    monsterAI.ResetPatrolPoint();
    monsterAI.SetFollowTarget(obj);
    return true;
  }
  static int FollowHash = "follow".GetStableHashCode();
  public void RPC_Command(long sender, ZDOID characterID, bool message)
  {
    var player = GetPlayer(characterID);
    if (player == null) return;
    var ret = monsterAI == null ? CommandAnimalAI(player.gameObject) : CommandMonsterAI(player.gameObject);
    if (ret)
    {
      if (view.IsOwner()) view.GetZDO().Set(FollowHash, "");
      if (message)
        player.Message(MessageHud.MessageType.Center, GetHoverName() + " $hud_tamefollow", 0, null);
    }
    else
    {
      if (view.IsOwner()) view.GetZDO().Set(FollowHash, player.GetPlayerName());
      if (message)
        player.Message(MessageHud.MessageType.Center, GetHoverName() + " $hud_tamestay", 0, null);
    }
  }

  private void UpdateSavedFollowTarget()
  {
    if ((monsterAI != null && monsterAI.GetFollowTarget() != null) || !view.IsOwner() || !follow) return;
    var following = view.GetZDO().GetString(FollowHash, "");
    if (string.IsNullOrEmpty(following)) return;
    foreach (var player in Player.GetAllPlayers())
    {
      if (player.GetPlayerName() == following)
      {
        Command(player, false);
        return;
      }
    }
  }

  public EffectList petEffect = new();
#nullable disable
  public Character character;
  public BaseAI baseAI;
  public ZNetView view;
#nullable enable
  public MonsterAI? monsterAI;
  public AnimalAI? animalAI;
  public GameObject? follow;
  public float m_lastPetTime;

}

[HarmonyPatch]
public class TamingTweak
{
  static int Tamed = "tamed".GetStableHashCode();

  // Start makes World Edit Commands tamed spawning work (Awake triggers too early).
  [HarmonyPatch(typeof(Character), nameof(Character.Start)), HarmonyPostfix]
  static void AddTameable(Character __instance)
  {
    if (!Configuration.configCharacterTamed.Value) return;
    if (__instance.GetComponent<Tameable>()) return;
    Helper.Bool(__instance.m_nview, Tamed, value =>
    {
      if (!value) return;
      __instance.gameObject.AddComponent<MiniTameable>();
    });
  }


  [HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage)), HarmonyPrefix]
  static bool MakeImmune(Character __instance, HitData hit)
  {
    if (!Configuration.configCharacterTamed.Value) return true;
    if (!__instance.m_nview.IsOwner()) return true;
    var attacker = hit.GetAttacker();
    if (!attacker || !attacker.IsPlayer()) return true;
    if (!__instance.TryGetComponent<MiniTameable>(out var tame)) return true;
    return tame.IsOwner();
  }

  [HarmonyPatch(typeof(Character), nameof(Character.GetHoverText)), HarmonyPostfix]
  static string GetHoverText(string result, Character __instance)
  {
    if (__instance.TryGetComponent<MiniTameable>(out var obj))
      return obj.GetHoverText();
    return result;
  }
  [HarmonyPatch(typeof(Character), nameof(Character.GetHoverName)), HarmonyPostfix]
  static string GetHoverName(string result, Character __instance)
  {
    if (__instance.TryGetComponent<MiniTameable>(out var obj))
      return obj.GetHoverName();
    return result;
  }

  [HarmonyPatch(typeof(BaseAI), nameof(BaseAI.IdleMovement)), HarmonyPrefix]
  static bool DisableIdleMovement(BaseAI __instance)
  {
    if (__instance.TryGetComponent<MiniTameable>(out var tamed) && tamed.follow) return false;
    return true;
  }
}
