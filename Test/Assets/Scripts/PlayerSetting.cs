using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ProtoBuf;
using Communication;
using Const;

[ProtoContract]
public class PlayerSetting : Communication.Packet<PlayerSetting>
{
    public PlayerSetting()
    { }

    public PlayerSetting(int playerID)
    {
        this.playerID = playerID;
        this.name = "";
        this.team = Team.NONE;
        this.status = PlayerStatus.NONE;
    }

    public PlayerSetting (int playerID, string name)
    {
        this.playerID = playerID;
        this.name = name;
        this.team = Team.NONE;
        this.status = PlayerStatus.NONE;
    }

    [ProtoMember(1)]
    public int playerID;

    [ProtoMember(2)]
    public string name = "name";

    [ProtoMember(3)]
    public Team team;

    [ProtoMember(4)]
    public PlayerStatus status;

    public PlayerSettingError IsSettingCompleted()
    {
        if (playerID == -1) return PlayerSettingError.ID;

        if (name == "") return PlayerSettingError.NAME;

        if (team == Team.NONE) return PlayerSettingError.TEAM;

        return PlayerSettingError.NONE;
    }

    public override void FillPacket(PlayerSetting packet)
    {
        this.playerID = packet.playerID;
        this.name = packet.name;
        this.team = packet.team;
        this.status = packet.status;
    }
}

