using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const;
using Util;


namespace Client
{
    public class JobSelector : MonoBehaviour
    {
        private JobSelectorSM stateManager;
    
        // Use this for initialization
        void Start()
        {
            stateManager = new JobSelectorSM(JobSelectorState.OFF);
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

            foreach(Job job in Enum.GetValues(typeof(Job)))
            { 
    
                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.fixedWidth = 100f;
                buttonStyle.fixedHeight = 90f;

                if (GUILayout.Button(job.ToString(), buttonStyle))
                {
                    Close();
                    PlayerBehaviour player = C_PlayerManager.Inst.Get(ClientGame.Inst.GetID());
                    if (player != null)
                        player.ChangeJob(job);
                }
            }
    
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
    
    class JobSelectorSM : StateManager<JobSelectorState>
    {
        public JobSelectorSM(JobSelectorState initial)
        {
            state = initial;
        }
    
        public override void SetState(JobSelectorState newState)
        {
            ClientGame client = ClientGame.Inst;
    
         	 switch(newState)
             {
                 case JobSelectorState.ON:
                     client.keyFocusManager.FocusTo(InputKeyFocus.JOB_SELECTOR);
                     client.mouseFocusManager.FocusTo(InputMouseFocus.JOB_SELECTOR);
                     StateUtil.SetState<JobSelectorState>(ref state, newState);
                     break;
    
                 case JobSelectorState.OFF:
                    client.keyFocusManager.FreeFocus(InputKeyFocus.JOB_SELECTOR);
                    client.mouseFocusManager.FreeFocus(InputMouseFocus.JOB_SELECTOR);
                    StateUtil.SetState<JobSelectorState>(ref state, newState);
                    break;
             }
        }
    }
}
