using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const;
using C2S = Packet.C2S;
using Architecture;

namespace Character
{
    public class BuildController
    {
        BuildingData m_building;
        ServerPlayer m_player;

        public BuildController(ServerPlayer player)
        {
            m_player = player;
        }

        public bool ProcessInput()
        {
            if (m_building == null) return false;

            if (Input.GetButton("Fire1"))
                TryBuild();

            if (Input.GetButton("Fire2"))
                ReleaseBuildTool();

            return true;
        }

        bool CanBuild(BuildingData bData)
        {
            return true;
        }

        public void SelectBuildTool(BuildingData building)
        {
            if (m_player.GetJob() != Job.ENGINEER) return;

            m_building = building;
        }

        public void ReleaseBuildTool()
        {
            m_building = null;
        }

        public void TryBuild()
        {
            if (m_building == null) return;
            if (m_player.GetJob() != Job.ENGINEER) { ReleaseBuildTool(); return; }

            Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

            m_player.TryBuild(m_building, worldMousePosition);
        }

        public void Build(C2S.Build pck)
        {
            BuildingData building = BuildingDataLoader.Inst.GetBuilding(pck.buildingName);

            if (!CanBuild(building)) return;

            BuildingManager.Inst.Build(building, Map.GetGridPos(pck.position));
        }
    }
}
