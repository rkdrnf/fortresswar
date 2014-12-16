using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PlayerSetting : Packet.Packet<PlayerSetting>
{
    public PlayerSetting (NetworkPlayer player, string name)
    {
        this.player = player;
        this.name = name;
    }

    public NetworkPlayer player;
    public string name = "name";
}