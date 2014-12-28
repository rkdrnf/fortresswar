using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Const;
using Util;

namespace Client
{
    public class ChatManager : MonoBehaviour
    {

        const int CHAT_WINDOW_TIME = 5;

        double chatWindowTimer;

        List<Chat> chatList;

        string writingMessage;

        ChatSM stateManager;

        Vector2 scrollPos;

        public Texture2D ChatBackground;

        // Use this for initialization
        void Start()
        {
            scrollPos = Vector2.zero;
            chatList = new List<Chat>();
            writingMessage = "";
            stateManager = new ChatSM(ChatState.NONE);
        }

        // Update is called once per frame
        void Update()
        {
            // Open for CHAT_WINDOW_TIME seconds.
            if (stateManager.IsInState(ChatState.NEW_MESSAGE))
            {
                if (chatWindowTimer > 0f)
                {
                    chatWindowTimer -= Time.deltaTime;

                    // Change state to NONE when timer expired.
                    if (chatWindowTimer <= 0f)
                    {
                        stateManager.SetState(ChatState.NONE);
                    }
                }
            }
        }

        void Chat(string text)
        {
            if (text.Trim().Length > 0)
            {
                networkView.RPC("BroadCastChat", RPCMode.All, ClientGame.Inst.GetID(), text);
            }
            writingMessage = "";
        }


        [RPC]
        void BroadCastChat(int playerID, string text, NetworkMessageInfo info)
        {
            NewChat(playerID, text);
        }

        void NewChat(int playerID, string text)
        {
            chatList.Add(new Chat(playerID, text));
            stateManager.SetState(ChatState.NEW_MESSAGE);
            chatWindowTimer = CHAT_WINDOW_TIME;
            scrollPos = new Vector2(0f, 99999999f);
        }

        void ShowChat()
        {
            chatWindowTimer = CHAT_WINDOW_TIME;
        }

        void OnGUI()
        {
            Event e = Event.current;

            if (e.type == EventType.keyDown)
            {
                Debug.Log(e.keyCode);
                Debug.Log(e.character);
            }

            if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
            {
                if (stateManager.IsInState(ChatState.NONE, ChatState.NEW_MESSAGE))
                {
                    stateManager.SetState(ChatState.WRITING);
                    return;
                }

                if (stateManager.IsInState(ChatState.WRITING))
                {
                    stateManager.SetState(ChatState.NONE);
                    Chat(writingMessage);
                    return;
                }

                e.Use();
            }

            // don't draw chat window if state is none.
            if (stateManager.IsInState(ChatState.NONE))
                return;

            // show in other states (NEW_MESSAGE, WRITING)

            GUIStyle areaStyle = new GUIStyle(GUI.skin.box);
            areaStyle.padding = new RectOffset(0, 0, 10, 0);
            areaStyle.margin = new RectOffset();
            areaStyle.alignment = TextAnchor.UpperLeft;
            GUILayout.BeginArea(new Rect(20, Screen.height - 300, 300, 200), areaStyle);

            GUIStyle scrollStyle = new GUIStyle(GUI.skin.verticalScrollbar);
            scrollStyle.padding = new RectOffset(0, 0, 0, 0);
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUIStyle.none, (stateManager.IsInState(ChatState.WRITING) ? scrollStyle : GUIStyle.none), GUILayout.Width(300f), GUILayout.Height(190f));

            GUIStyle textAreaStyle = new GUIStyle();
            textAreaStyle.normal.textColor = new Color(255f, 255f, 255f, 255f);
            textAreaStyle.wordWrap = true;
            textAreaStyle.stretchHeight = true;
            textAreaStyle.padding = new RectOffset(5, 5, 5, 5);

            string chatLog = String.Join("\n", chatList.Select(c => string.Format("{0}: {1}", C_PlayerManager.Inst.GetSetting(c.playerID).name, c.text)).ToArray<string>());
            GUILayout.TextArea(chatLog, textAreaStyle);
            GUI.color = Color.gray;
            GUILayout.EndScrollView();
            GUILayout.EndArea();



            if (stateManager.IsInState(ChatState.WRITING))
            {
                GUI.SetNextControlName("ChatField");
                writingMessage = GUI.TextField(new Rect(20, Screen.height - 100, 300, 20), writingMessage);

                // Focus input field when WRITING.
                GUI.FocusControl("ChatField");
            }
        }
    }

    class ChatSM : StateManager<ChatState>
    {

        public ChatSM(ChatState initial)
        {
            state = initial;
        }

        public override void SetState(ChatState newState)
        {
            switch (newState)
            {
                case ChatState.NONE:
                    ClientGame.Inst.keyFocusManager.FreeFocus(InputKeyFocus.CHAT_WINDOW);
                    ClientGame.Inst.mouseFocusManager.FreeFocus(InputMouseFocus.CHAT_WINDOW);
                    StateUtil.SetState<ChatState>(ref this.state, newState);
                    break;

                case ChatState.NEW_MESSAGE:

                    if (IsNotInState(ChatState.WRITING))
                    {
                        ClientGame.Inst.keyFocusManager.FreeFocus(InputKeyFocus.CHAT_WINDOW);
                        ClientGame.Inst.mouseFocusManager.FreeFocus(InputMouseFocus.CHAT_WINDOW);
                        StateUtil.SetState<ChatState>(ref this.state, newState);
                    }
                    break;

                case ChatState.WRITING:
                    ClientGame.Inst.keyFocusManager.FocusTo(InputKeyFocus.CHAT_WINDOW);
                    ClientGame.Inst.mouseFocusManager.FocusTo(InputMouseFocus.CHAT_WINDOW);
                    StateUtil.SetState<ChatState>(ref this.state, newState);
                    break;
            }
        }
    }
}
