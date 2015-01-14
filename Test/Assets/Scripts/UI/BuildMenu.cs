using UnityEngine;
using System.Collections;
using Const;
using Util;
using Server;

namespace InGameMenu
{
    public class BuildMenu : MonoBehaviour
    {

        public BuildingDataSet buildingSet;

        private BuildMenuSM stateManager;

        int rowCount = 4;
        // Use this for initialization
        void Start()
        {
            stateManager = new BuildMenuSM(BuildMenuState.OFF);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Open()
        {
            if (stateManager.IsInState(BuildMenuState.ON))
                return;

            stateManager.SetState(BuildMenuState.ON);
        }

        public void Close()
        {
            if (stateManager.IsInState(BuildMenuState.OFF))
                return;

            stateManager.SetState(BuildMenuState.OFF);
        }

        void OnGUI()
        {
            if (stateManager.IsInState(BuildMenuState.OFF))
                return;

            int totalCount = 0;
            GUIStyle areaStyle = new GUIStyle(GUI.skin.box);
            GUILayout.BeginArea(new Rect(Screen.width / 2 + 50, Screen.height / 2 - 30, 100, 100), areaStyle);
            for (int i = 0; i <= buildingSet.buildings.Length / rowCount; i++)
            {
                GUILayout.BeginVertical();

                for (int j = 0; j < rowCount; j++)
                {
                    if (totalCount == buildingSet.buildings.Length)
                        break;

                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button(buildingSet.buildings[i * rowCount + j].image, GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        ServerPlayer player = PlayerManager.Inst.Get(ServerGame.Inst.GetID());
                        if (player != null)
                            player.SelectBuildTool(buildingSet.buildings[i * rowCount + j]);

                        Close();
                    }

                    totalCount++;

                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndArea();
        }
    }

    class BuildMenuSM : StateManager<BuildMenuState>
    {
        public BuildMenuSM(BuildMenuState initial)
        {
            state = initial;
        }

        public override void SetState(BuildMenuState newState)
        {
            ServerGame game = ServerGame.Inst;

            switch (newState)
            {
                case BuildMenuState.ON:
                    game.mouseFocusManager.FocusTo(InputMouseFocus.BUILD_MENU);
                    StateUtil.SetState<BuildMenuState>(ref state, newState);
                    break;

                case BuildMenuState.OFF:
                    game.mouseFocusManager.FreeFocus(InputMouseFocus.BUILD_MENU);
                    StateUtil.SetState<BuildMenuState>(ref state, newState);
                    break;
            }
        }
    }
}