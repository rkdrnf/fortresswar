using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const;
using Util;

public class JobSelector : MonoBehaviour
{
    private JobSelectorSM stateManager;

    // Use this for initialization
    void Start()
    {
        stateManager = new JobSelectorSM();
        stateManager.SetState(JobSelectorState.OFF);
    }


    public void Open()
    {
        if (stateManager.IsInState(JobSelectorState.ON))
            return;

        stateManager.SetState(JobSelectorState.ON);
    }

    public void Close()
    {
        if (stateManager.IsInState(JobSelectorState.OFF))
            return;

        stateManager.SetState(JobSelectorState.OFF);
    }


    void OnGUI()
    {
        if (stateManager.IsInState(JobSelectorState.OFF))
            return;

        GUIStyle areaStyle = new GUIStyle(GUI.skin.box);
        GUILayout.BeginArea(new Rect(Screen.width / 2 - 150f, Screen.height / 2 - 50f, 300, 100), areaStyle);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUIStyle blueStyle = new GUIStyle(GUI.skin.button);
        blueStyle.fixedWidth = 145f;
        blueStyle.fixedHeight = 90f;

        if (GUILayout.Button("Scout", blueStyle))
        {
            PlayerBehaviour player = PlayerManager.Inst.Get(Game.Inst.GetID());
            if (player != null)
                player.ChangeJob(Job.SCOUT);

            Close();
        }

        GUIStyle redStyle = new GUIStyle(GUI.skin.button);
        redStyle.fixedWidth = 145f;
        redStyle.fixedHeight = 90f;
        if (GUILayout.Button("Heavy Gunner", redStyle))
        {
            PlayerBehaviour player = PlayerManager.Inst.Get(Game.Inst.GetID());
            if (player != null)
                player.ChangeJob(Job.HEAVY_GUNNER);

            Close();
        }

        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}

class JobSelectorSM : StateManager<JobSelectorState>
{
    public override void SetState(JobSelectorState newState)
    {
     	 switch(newState)
         {
             case JobSelectorState.ON:
                 Game.Inst.keyFocusManager.FocusTo(InputKeyFocus.JOB_SELECTOR);
                 Game.Inst.mouseFocusManager.FocusTo(InputMouseFocus.JOB_SELECTOR);
                 StateUtil.SetState<JobSelectorState>(out state, newState);
                 break;

             case JobSelectorState.OFF:
             Game.Inst.keyFocusManager.FreeFocus();
             Game.Inst.mouseFocusManager.FreeFocus();
             StateUtil.SetState<JobSelectorState>(out state, newState);
             break;
         }
    }
}
