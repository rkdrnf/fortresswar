using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using Client;

public class Chat
{
	public Chat(int playerID, string chat)
    {
        this.playerID = playerID;
        this.text = chat;

        if (C_PlayerManager.Inst.GetSetting(playerID) != null)
            this.writer = C_PlayerManager.Inst.GetSetting(playerID).name;
    }
    public string text;
    public string writer;
    public int playerID;
}
