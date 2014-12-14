using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Const;
using Util;

public class ChatManager : MonoBehaviour {

    const int CHAT_WINDOW_TIME = 5;

    double chatWindowTimer;
    
    List<Chat> chatList;

    string writingMessage;

    ChatState state = ChatState.NONE;

    bool IsInState(ChatState state, params ChatState[] stateList)
    {
        return StateUtil.IsInState<ChatState>(this.state, state, stateList);
    }

    bool IsNotInState(ChatState state, params ChatState[] stateList)
    {
        return StateUtil.IsNotInState<ChatState>(this.state, state, stateList);
    }

    void SetState(ChatState state)
    {
        switch (state)
        {
            case ChatState.NONE:
                StateUtil.SetState<ChatState>(out this.state, state);
                break;

            case ChatState.NEW_MESSAGE:
                if (IsNotInState(ChatState.WRITING))
                {
                    StateUtil.SetState<ChatState>(out this.state, state);
                }
                break;

            case ChatState.WRITING:
                StateUtil.SetState<ChatState>(out this.state, state);
                break;
        }

        if (state == ChatState.NEW_MESSAGE)
        {
            chatWindowTimer = CHAT_WINDOW_TIME;
        }
    }

    void Awake()
    {
        chatList = new List<Chat>();
        writingMessage = "";
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        // Open for CHAT_WINDOW_TIME seconds.
        if (IsInState(ChatState.NEW_MESSAGE))
        {
            if (chatWindowTimer > 0f)
            {
                chatWindowTimer -= Time.deltaTime;

                // Change state to NONE when timer expired.
                if (chatWindowTimer <= 0f)
                {
                    SetState(ChatState.NONE);
                }
            }
        }

        
	}

    void Chat(string text)
    {
        if (text.Trim().Length > 0)
        {
            networkView.RPC("BroadCastChat", RPCMode.All, text);
        }
        writingMessage = "";
    }


    [RPC]
    void BroadCastChat(string text, NetworkMessageInfo info)
    {
        NewChat(info.sender, text);
    }

    void NewChat(NetworkPlayer player, string text)
    {
        chatList.Add(new Chat(player, text));
        SetState(ChatState.NEW_MESSAGE);
    }

    void ShowChat()
    {
        chatWindowTimer = CHAT_WINDOW_TIME;
    }

    void OnGUI()
    {
        Event e = Event.current;

        if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
        {
            if (IsInState(ChatState.NONE, ChatState.NEW_MESSAGE))
            {
                SetState(ChatState.WRITING);
                return;
            }

            if (IsInState(ChatState.WRITING))
            {
                SetState(ChatState.NONE);
                Chat(writingMessage);
                return;
            }

            e.Use();
        }

        // don't draw chat window if state is none.
        if (IsInState(ChatState.NONE))
            return;

        // show in other states (NEW_MESSAGE, WRITING)
        GUI.BeginScrollView(new Rect(20, Screen.height - 300, 300, 200), Vector2.zero, new Rect(0, 0, 220, 200));
        GUIStyle areaStyle = new GUIStyle();
        areaStyle.wordWrap = true;
        areaStyle.stretchHeight = true;
        string chatLog = String.Join("\n", chatList.Select(c => c.text).ToArray<string>());
        GUI.TextArea(new Rect(0, 0, 300, 200), chatLog, areaStyle);
        GUI.EndScrollView();

        if (IsInState(ChatState.WRITING))
        {
            GUI.SetNextControlName("ChatField");
            writingMessage = GUI.TextField(new Rect(20, Screen.height - 100, 300, 20), writingMessage);

            // Focus input field when WRITING.
            GUI.FocusControl("ChatField");
        }
        else
        {
            GUI.FocusControl("INVALID");
        }
    }
}
