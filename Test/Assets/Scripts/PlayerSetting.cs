using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ProtoBuf;
using Packet;

[ProtoContract]
public class PlayerSetting : Packet.Packet<PlayerSetting>
{
    public PlayerSetting()
    { }

    public PlayerSetting (int playerID, string name)
    {
        this.playerID = playerID;
        this.name = name;
    }

    [ProtoMember(1)]
    public int playerID;

    [ProtoMember(2)]
    public string name = "name";
}