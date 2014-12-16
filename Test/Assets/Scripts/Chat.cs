using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Chat
{
	public Chat(int playerID, string chat)
    {
        this.playerID = playerID;
        this.text = chat;
    }
    public string text;
    public int playerID;
}
