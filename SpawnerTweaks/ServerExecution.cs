using System;
using System.Globalization;
using System.Linq;
using HarmonyLib;
using UnityEngine;
namespace SpawnerTweaks;

[HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_PeerInfo))]
public class ServerExecution
{

  ///<summary>Sends command to the server so that it can be executed there.</summary>
  public static void Send(string command, Vector3 center, Vector3 rot)
  {
    var cmd = Parse(command, center, rot);
    if (!ZNet.instance || ZNet.instance.IsServer())
    {
      Console.instance.TryRunCommand(cmd);
      return;
    }
    var server = ZNet.instance.GetServerRPC();
    if (server == null) return;
    server.Invoke(RPC_Command, new[] { cmd });
  }

  public static string RPC_Command = "SpawnerTweaks_Command";

  private static void RPC_Do_Command(ZRpc rpc, string command)
  {
    Console.instance.TryRunCommand(command);
  }
  static void Postfix(ZNet __instance, ZRpc rpc)
  {
    if (__instance.IsDedicated())
      rpc.Register<string>(RPC_Command, new(RPC_Do_Command));
  }
  private static string Parse(string command, Vector3 center, Vector3 rot)
  {
    var zone = ZoneSystem.instance.GetZone(center);
    var cmd = command
        .Replace("$$x", center.x.ToString(NumberFormatInfo.InvariantInfo))
        .Replace("$$y", center.y.ToString(NumberFormatInfo.InvariantInfo))
        .Replace("$$z", center.z.ToString(NumberFormatInfo.InvariantInfo))
        .Replace("$$i", zone.x.ToString(NumberFormatInfo.InvariantInfo))
        .Replace("$$j", zone.y.ToString(NumberFormatInfo.InvariantInfo))
        .Replace("$$a", rot.y.ToString(NumberFormatInfo.InvariantInfo))
        .Replace("<x>", center.x.ToString(NumberFormatInfo.InvariantInfo))
        .Replace("<y>", center.y.ToString(NumberFormatInfo.InvariantInfo))
        .Replace("<z>", center.z.ToString(NumberFormatInfo.InvariantInfo))
        .Replace("<i>", zone.x.ToString(NumberFormatInfo.InvariantInfo))
        .Replace("<j>", zone.y.ToString(NumberFormatInfo.InvariantInfo))
        .Replace("<a>", rot.y.ToString(NumberFormatInfo.InvariantInfo));

    var expressions = cmd.Split(' ').Select(s => s.Split('=')).Select(a => a[a.Length - 1].Trim()).SelectMany(s => s.Split(',')).ToArray();
    foreach (var expression in expressions)
    {
      if (!expression.Contains('*') && !expression.Contains('/') && !expression.Contains('+') && !expression.Contains('-')) continue;
      var value = Evaluate(expression);
      cmd = cmd.Replace(expression, value.ToString(NumberFormatInfo.InvariantInfo));
    }
    return cmd;
  }
  private static float Evaluate(string expression)
  {
    var mult = expression.Split('*');
    if (mult.Length > 1)
    {
      var sum = 1f;
      foreach (var m in mult) sum *= Evaluate(m);
      return sum;
    }
    var div = expression.Split('/');
    if (div.Length > 1)
    {
      var sum = Evaluate(div[0]);
      for (var i = 1; i < div.Length; ++i) sum /= Evaluate(div[i]);
      return sum;
    }
    var plus = expression.Split('+');
    if (plus.Length > 1)
    {
      var sum = 0f;
      foreach (var p in plus) sum += Evaluate(p);
      return sum;
    }
    var minus = expression.Split('-');
    // Negative numbers get split as well, so check for actual parts.
    if (minus.Where(s => s != "").Count() > 1)
    {
      var sum = Evaluate(minus[0]);
      for (var i = 1; i < minus.Length; ++i)
      {
        if (minus[i] == "" && i + 1 < minus.Length)
        {
          minus[i + 1] = "-" + minus[i + 1];
          continue;
        }
        sum -= Evaluate(minus[i]);
      }
      return sum;
    }
    try
    {
      return float.Parse(expression.Trim(), NumberFormatInfo.InvariantInfo);
    }
    catch
    {
      throw new Exception($"Failed to parse expression: {expression}");
    }
  }
}
