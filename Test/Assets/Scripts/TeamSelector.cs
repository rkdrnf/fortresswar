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
        GUILayout.BeginArea(new Rect(150, 150, 400, 300), areaStyle);
        
        GUIStyle blueStyle = new GUIStyle();
        blueStyle.fixedWidth = 150f;
        blueStyle.fixedHeight = 100f;

        if (GUILayout.Button("BLUE", blueStyle))
        {
            if (Network.isServer)
                Game.Inst.ServerSpecificSelectTeam(Team.BLUE);
            else
                Game.Inst.SelectTeam(Team.BLUE);

            Close();
        }

        GUIStyle redStyle = new GUIStyle();
        redStyle.fixedWidth = 150f;
        redStyle.fixedHeight = 100f;
        if (GUILayout.Button("RED", redStyle))
        {
            if (Network.isServer)
                Game.Inst.ServerSpecificSelectTeam(Team.RED);
            else
                Game.Inst.SelectTeam(Team.RED);
            Close();
        }

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
