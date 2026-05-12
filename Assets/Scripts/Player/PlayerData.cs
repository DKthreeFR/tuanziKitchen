using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct PlayerData:IEquatable<PlayerData>,INetworkSerializable
{
    public ulong clientId;
    public int colorId;
    public int characterId;
    //不使用字符串是因为字符串可能为空导致报错
    public FixedString64Bytes playerName;
    public FixedString64Bytes playerId;
    public bool Equals(PlayerData other)
    {
       return clientId  == other.clientId&&colorId==other.colorId&&playerName == other.playerName&&playerId == other.playerId&&characterId==other.characterId;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref colorId);
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref characterId);
    }
}
 