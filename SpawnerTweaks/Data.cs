using UnityEngine;

namespace Service;

public class DataHelper
{
  public static ZPackage? Deserialize(string data)
  {
    if (data == "") return null;
    ZPackage pkg = new(data);
    return pkg;
  }
  public static void InitZDO(GameObject obj, Vector3 position, Quaternion rotation, ZPackage data)
  {
    if (!obj.TryGetComponent<ZNetView>(out var view)) return;
    var prefab = view.GetPrefabName().GetStableHashCode();
    ZNetView.m_initZDO = ZDOMan.instance.CreateNewZDO(position, prefab);
    Load(data, ZNetView.m_initZDO);
    ZNetView.m_initZDO.m_rotation = rotation.eulerAngles;
    ZNetView.m_initZDO.Type = view.m_type;
    ZNetView.m_initZDO.Distant = view.m_distant;
    ZNetView.m_initZDO.Persistent = view.m_persistent;
    ZNetView.m_initZDO.m_prefab = prefab;
    ZNetView.m_initZDO.DataRevision = 1;
  }
  private static void Load(ZPackage pkg, ZDO zdo)
  {
    pkg.SetPos(0);
    var id = zdo.m_uid;
    var num = pkg.ReadInt();
    if ((num & 1) != 0)
    {
      var count = pkg.ReadByte();
      if (!ZDOExtraData.s_floats.ContainsKey(id))
        ZDOExtraData.s_floats[id] = new();
      for (var i = 0; i < count; ++i)
        ZDOExtraData.s_floats[id][pkg.ReadInt()] = pkg.ReadSingle();
    }
    if ((num & 2) != 0)
    {
      var count = pkg.ReadByte();
      if (!ZDOExtraData.s_vec3.ContainsKey(id))
        ZDOExtraData.s_vec3[id] = new();
      for (var i = 0; i < count; ++i)
        ZDOExtraData.s_vec3[id][pkg.ReadInt()] = pkg.ReadVector3();
    }
    if ((num & 4) != 0)
    {
      var count = pkg.ReadByte();
      if (!ZDOExtraData.s_quats.ContainsKey(id))
        ZDOExtraData.s_quats[id] = new();
      for (var i = 0; i < count; ++i)
        ZDOExtraData.s_quats[id][pkg.ReadInt()] = pkg.ReadQuaternion();
    }
    if ((num & 8) != 0)
    {
      var count = pkg.ReadByte();
      if (!ZDOExtraData.s_ints.ContainsKey(id))
        ZDOExtraData.s_ints[id] = new();
      for (var i = 0; i < count; ++i)
        ZDOExtraData.s_ints[id][pkg.ReadInt()] = pkg.ReadInt();
    }
    // Intended to come before strings.
    if ((num & 64) != 0)
    {
      var count = pkg.ReadByte();
      if (!ZDOExtraData.s_longs.ContainsKey(id))
        ZDOExtraData.s_longs[id] = new();
      for (var i = 0; i < count; ++i)
        ZDOExtraData.s_longs[id][pkg.ReadInt()] = pkg.ReadLong();
    }
    if ((num & 16) != 0)
    {
      var count = pkg.ReadByte();
      if (!ZDOExtraData.s_strings.ContainsKey(id))
        ZDOExtraData.s_strings[id] = new();
      for (var i = 0; i < count; ++i)
        ZDOExtraData.s_strings[id][pkg.ReadInt()] = pkg.ReadString();
    }
    if ((num & 128) != 0)
    {
      var count = pkg.ReadByte();
      if (!ZDOExtraData.s_byteArrays.ContainsKey(id))
        ZDOExtraData.s_byteArrays[id] = new();
      for (var i = 0; i < count; ++i)
        ZDOExtraData.s_byteArrays[id][pkg.ReadInt()] = pkg.ReadByteArray();
    }
  }
}