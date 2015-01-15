using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const;
using Util;
using Server;

namespace InGameMenu
{
    public class JobSelector : MonoBehaviour
    {
        private JobSelectorSM m_stateManager;

        // Use this for initialization
        void Start()
        {
            m_stateManager = new JobSelectorSM(JobSelectorState.OFF);
        }


        public void Open()
        {
            if (m_stateManager.IsInState(JobSelectorState.ON))
                return;

            m_stateManager.SetState(JobSelectorState.ON);
        }

        public void Close()
        {
            if (m_stateManager.IsInState(JobSelectorState.OFF))
                return;

            m_stateManager.SetState(JobSelectorState.OFF);
        }


        void OnGUI()
        {
            if (m_stateManager.IsInState(JobSelectorState.OFF))
                return;

            GUIStyle areaStyle = new GUIStyle(GUI.skin.box);
            GUILayout.BeginArea(new Rect(Screen.width / 2 - 150f, Screen.height / 2 - 50f, 300, 100), areaStyle);

            GUILayout.BeginHorizontal();

            foreach (Job job in Enum.GetValues(typeof(Job)))
            {

                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.fixedWidth = 100f;
                buttonStyle.fixedHeight = 90f;

                if (GUILayout.Button(job.ToString(), buttonStyle))
                {
                    Close();
                    ServerPlayer player = PlayerManager.Inst.Get(ServerGame.Inst.GetID());
                    if (player != null)
                        player.TryChangeJob(job);
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
            ServerGame game = ServerGame.Inst;

            switch (newState)
            {
                case JobSelectorState.ON:
                    game.keyFocusManager.FocusTo(InputKeyFocus.JOB_SELECTOR);
                    game.mouseFocusManager.FocusTo(InputMouseFocus.JOB_SELECTOR);
                    StateUtil.SetState<JobSelectorState>(ref state, newState);
                    break;

                case JobSelectorState.OFF:
                    game.keyFocusManager.FreeFocus(InputKeyFocus.JOB_SELECTOR);
                    game.mouseFocusManager.FreeFocus(InputMouseFocus.JOB_SELECTOR);
                    StateUtil.SetState<JobSelectorState>(ref state, newState);
                    break;
            }
        }
    }
}
