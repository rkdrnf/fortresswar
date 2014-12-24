using UnityEngine;
using System.Collections;
using Util;
using Const;

namespace Client
{ 
    public class TeamSelector : MonoBehaviour {
    
        private TeamSelectorSM stateManager;
    
        ClientGame client
        {
            get { return ClientGame.Inst; }
        }
    
    	// Use this for initialization
    	void Start () {
            stateManager = new TeamSelectorSM(TeamSelectorState.OFF);
            stateManager.SetState(TeamSelectorState.OFF);
    	}
    	
        public void Open()
        {
            stateManager.SetState(TeamSelectorState.ON);
        }
    
        public void Close()
        {
            stateManager.SetState(TeamSelectorState.OFF);
        }
    
    
        void OnGUI ()
        {
            if (stateManager.IsInState(TeamSelectorState.OFF))
                return;
    
            GUIStyle areaStyle = new GUIStyle(GUI.skin.box);
            GUILayout.BeginArea(new Rect(Screen.width / 2 - 150f, Screen.height / 2 - 50f, 300, 100), areaStyle);
    
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
    
            GUI.color = Color.blue;
            GUIStyle blueStyle = new GUIStyle(GUI.skin.button);
            blueStyle.fixedWidth = 145f;
            blueStyle.fixedHeight = 90f;
    
            if (GUILayout.Button("BLUE", blueStyle))
            {
                Close();
                ClientGame.Inst.SelectTeam(Team.BLUE);
            }
    
            GUI.color = Color.red;
            GUIStyle redStyle = new GUIStyle(GUI.skin.button);
            redStyle.fixedWidth = 145f;
            redStyle.fixedHeight = 90f;
            if (GUILayout.Button("RED", redStyle))
            {
                Close();
                ClientGame.Inst.SelectTeam(Team.RED);
            }
    
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
    
    class TeamSelectorSM : StateManager<TeamSelectorState>
    {
        public TeamSelectorSM(TeamSelectorState initial)
        {
            state = initial;
        }
    
        override public void SetState(TeamSelectorState newState)
        {
            ClientGame client = ClientGame.Inst;
    
            switch(newState)
            {
                case TeamSelectorState.ON:
                    client.keyFocusManager.FocusTo(InputKeyFocus.TEAM_SELECTOR);
                    client.mouseFocusManager.FocusTo(InputMouseFocus.TEAM_SELECTOR);
                    StateUtil.SetState<TeamSelectorState>(ref state, newState);
                    break;
    
                case TeamSelectorState.OFF:
                    client.keyFocusManager.FreeFocus(InputKeyFocus.TEAM_SELECTOR);
                    client.mouseFocusManager.FreeFocus(InputMouseFocus.TEAM_SELECTOR);
                    StateUtil.SetState<TeamSelectorState>(ref state, newState);
                    break;
            }
        }
    }
}