using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong clientId;
    public int ColorID;

    public bool Equals(PlayerData other)
    {
        return clientId == other.clientId && ColorID == other.ColorID;
    }

    void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref ColorID);
    }
}
