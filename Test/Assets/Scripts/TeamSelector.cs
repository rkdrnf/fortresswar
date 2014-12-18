using UnityEngine;
using System.Collections;
using Util;
using Const;

public class TeamSelector : MonoBehaviour {

    private TeamSelectorSM stateManager;

	// Use this for initialization
	void Start () {
        stateManager = new TeamSelectorSM();
        stateManager.SetState(TeamSelectorState.OFF);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Open()
    {
        if (stateManager.IsInState(TeamSelectorState.ON))
            return;

        stateManager.SetState(TeamSelectorState.ON);
    }

    public void Close()
    {
        if (stateManager.IsInState(TeamSelectorState.OFF))
            return;

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
            if (Network.isServer)
                Game.Inst.ServerSpecificSelectTeam(Team.BLUE);
            else
                Game.Inst.SelectTeam(Team.BLUE);

            Close();
        }

        GUI.color = Color.red;
        GUIStyle redStyle = new GUIStyle(GUI.skin.button);
        redStyle.fixedWidth = 145f;
        redStyle.fixedHeight = 90f;
        if (GUILayout.Button("RED", redStyle))
        {
            if (Network.isServer)
                Game.Inst.ServerSpecificSelectTeam(Team.RED);
            else
                Game.Inst.SelectTeam(Team.RED);
            Close();
        }

        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}

class TeamSelectorSM : StateManager<TeamSelectorState>
{
    override public void SetState(TeamSelectorState newState)
    {
        switch(newState)
        {
            case TeamSelectorState.ON:
                Game.Inst.keyFocusManager.FocusTo(InputKeyFocus.TEAM_SELECTOR);
                Game.Inst.mouseFocusManager.FocusTo(InputMouseFocus.TEAM_SELECTOR);
                StateUtil.SetState<TeamSelectorState>(out state, newState);
                break;

            case TeamSelectorState.OFF:
                Game.Inst.keyFocusManager.FreeFocus();
                Game.Inst.mouseFocusManager.FreeFocus();
                StateUtil.SetState<TeamSelectorState>(out state, newState);
                break;
        }
    }
}
