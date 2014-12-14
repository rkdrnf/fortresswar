using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Chat
{
	public Chat(NetworkPlayer player, string chat)
    {
        this.player = player;
        this.text = chat;
    }
    public string text;
    public NetworkPlayer player;
}
