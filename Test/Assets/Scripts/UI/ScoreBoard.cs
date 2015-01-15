using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Util;
using Const;
using Server;

namespace InGameMenu
{
    public class ScoreBoard : MonoBehaviour
    {
        private ScoreBoardSM m_stateManager;
         
        void Start()
        {
            m_stateManager = new ScoreBoardSM(ScoreBoardState.OFF);
        }

        public void Open()
        {
            if (m_stateManager.IsInState(ScoreBoardState.ON))
                return;

            m_stateManager.SetState(ScoreBoardState.ON);
        }

        public void Close()
        {
            if (m_stateManager.IsInState(ScoreBoardState.OFF)) return;

            m_stateManager.SetState(ScoreBoardState.OFF);
        }

        void OnGUI()
        {
            if (m_stateManager.IsInState(ScoreBoardState.OFF))
                return;

            Event e = Event.current;
            if (e.type == EventType.keyUp && (e.keyCode == KeyCode.Tab))
                Close();

            GUIStyle areaStyle = new GUIStyle(GUI.skin.box);
            GUILayout.BeginArea(new Rect(Screen.width / 2 - 200f, Screen.height / 2 - 200f, 400, 500), areaStyle);
            GUILayout.BeginHorizontal();

            GUI.color = Color.blue;
            GUILayout.BeginVertical();
            foreach (var player in ServerGame.Inst.GetTeam(Team.BLUE).GetPlayers())
            {
                GUILayout.BeginHorizontal();
                PlayerSetting setting = PlayerManager.Inst.GetSetting(player.GetOwner());
                GUILayout.Box(setting.name, GUILayout.Width(190f));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUI.color = Color.red;
            GUILayout.BeginVertical();
            foreach (var player in ServerGame.Inst.GetTeam(Team.RED).GetPlayers())
            {
                GUILayout.BeginHorizontal();
                PlayerSetting setting = PlayerManager.Inst.GetSetting(player.GetOwner());
                GUILayout.Box(setting.name, GUILayout.Width(190f));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }

    class ScoreBoardSM : StateManager<ScoreBoardState>
    {
        public ScoreBoardSM(ScoreBoardState initial)
        {
            state = initial;
        }

        public override void SetState(ScoreBoardState newState)
        {
            ServerGame game = ServerGame.Inst;

         	switch(newState)
            {
                case ScoreBoardState.ON:
                    game.mouseFocusManager.FocusTo(InputMouseFocus.SCORE_BOARD);
                    StateUtil.SetState<ScoreBoardState>(ref state, newState);
                    break;

                case ScoreBoardState.OFF:
                    game.mouseFocusManager.FreeFocus(InputMouseFocus.SCORE_BOARD);
                    StateUtil.SetState<ScoreBoardState>(ref state, newState);
                    break;
            }
        }

    }
}
