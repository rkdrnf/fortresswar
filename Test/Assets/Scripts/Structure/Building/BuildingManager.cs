using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const;
using Const.Structure;
using System.Collections;

using S2C = Communication.S2C;


namespace Architecture
{
    public class BuildingManager : StructureManager<Building, BuildingType, BuildingData>
    {
        private static BuildingManager instance;

        public static BuildingManager Inst
        {
            get
            {
                if (instance == null)
                {
                    instance = new BuildingManager();
                }

                return instance;
            }
        }

        Dictionary<GridCoord, Building> m_buildingMap;
        Dictionary<int, Building> m_buildingIDMap;

        ushort m_buildingIndex;

        public BuildingDataSet m_buildingSet = null; // scene init
        Dictionary<BuildingType, BuildingData> m_buildingDataDic;
        public LayerMask m_buildingLayer;

        public FallingBuildingPool m_fallingBuildingPool;
        
        public BuildingChunkManager m_buildingChunkManager = null; // scene init

        void Awake()
        {
            instance = this;
            m_buildingIndex = 0;
            m_buildingMap = new Dictionary<GridCoord, Building>();
            m_buildingIDMap = new Dictionary<int, Building>();
            m_buildingDataDic = new Dictionary<BuildingType, BuildingData>();

            foreach(BuildingData bData in m_buildingSet.buildings)
            {
                m_buildingDataDic.Add(bData.type, bData);
            }

            int fallingBuildingLayer = LayerMask.NameToLayer("FallingBuilding");
            Physics2D.IgnoreLayerCollision(fallingBuildingLayer, fallingBuildingLayer);
            Physics2D.IgnoreLayerCollision(fallingBuildingLayer, LayerMask.NameToLayer("Particle"));
        }

        public override void Clear()
        {
            m_buildingIndex = 0;
            m_buildingMap.Clear();
            m_buildingIDMap.Clear();
            m_buildingChunkManager.Clear();
        }

        public override void New(Building building)
        {
            Building newBuilding = new Building(m_buildingIndex, building, this);

            Add(newBuilding);

            m_buildingIndex++;
        }

        public override void Add(Building building)
        {
            m_buildingIDMap.Add(building.GetID(), building);

            if (building.m_isFalling == false)
            {
                m_buildingMap.Add(building.m_coord, building);
                m_buildingChunkManager.AddBlock(building);

                Lights.ShadowPane.Inst.UpdateLight(building.m_coord, 1f);
            }
            else
            {

            }
        }

        public override void Remove(Building building)
        {
            if (m_buildingMap.ContainsKey(building.m_coord) && m_buildingMap[building.m_coord] == building)
                m_buildingMap.Remove(building.m_coord);

            m_buildingIDMap.Remove(building.GetID());

            m_buildingChunkManager.RemoveBlock(building);
        }

        public void RemoveFromChunk(Building building) //When Fall
        {
            if (m_buildingMap.ContainsKey(building.m_coord) && m_buildingMap[building.m_coord] == building)
                m_buildingMap.Remove(building.m_coord);

            m_buildingChunkManager.RemoveBlock(building);
        }

        public void RemoveID(Building building)
        {
            m_buildingIDMap.Remove(building.GetID());
        }

        List<Building> m_fallingList = new List<Building>();

        public void Fall(Building building)
        {
            StartCoroutine(FallInternal(building));
        }

        void Update()
        {
            if (m_fallingList.Count > 0)
            {
                //StartCoroutine(FallInternal(m_fallingList.ToArray()));
                m_fallingList.Clear();
            }
        }


        IEnumerator FallInternal(Building building)
        {
            yield return new WaitForFixedUpdate();

            //Debug.Log("FallFrame: " + Time.frameCount);
            //for(int i = 0; i < fallings.Length; i++)
            //{
              //  Building building = fallings[i];
                RemoveFromChunk(building);

                FallingBuilding falling = m_fallingBuildingPool.Borrow();

                if (falling != null)
                    falling.Init(building);
            //}
        }

        public void Build(BuildingData bData, GridCoord coord)
        {
            if (Network.isServer)
            {
                if (!CanBuild(bData, coord)) return;
            }

            Building building = new Building(m_buildingIndex, bData, coord, this);

            Add(building);
            m_buildingIndex++;

            BuildingNetwork.Inst.BroadcastBuild(building);
        }

        public void Build(S2C.NewBuilding pck)
        {
            Building building = new Building(pck, this);
            BuildingManager.Inst.Add(building);
        }

        public bool CanBuild(BuildingData bData, GridCoord coord)
        {
            if (BuildingManager.instance.Get(coord) != null || TileManager.Inst.Get(coord) != null) return false;
            if (FindSuspension(coord).Count == 0) return false;
            Vector2 coordVector = coord.ToVector2();
            Collider2D[] colliders = Physics2D.OverlapAreaAll(coordVector - (bData.size / 2 * 0.9f), coordVector + (bData.size / 2 * 0.9f), bData.invalidLocations);
            if (colliders.Length > 0) return false;

            
            return true;
        }

        public Suspension FindSuspension(GridCoord coord)
        {
            Suspension suspension = new Suspension();

            FindBuildingSuspension(coord, ref suspension);
            FindTileSuspension(coord, ref suspension);

            return suspension;
        }

        public int FindTileSuspension(GridCoord coord, ref Suspension suspension)
        {
            suspension.center = TileManager.Inst.Get(coord);
            suspension.down = suspension.down == null ? TileManager.Inst.Get(coord.Down()) : suspension.down;
            suspension.left = suspension.left == null ? TileManager.Inst.Get(coord.Left()) : suspension.left;
            suspension.right = suspension.right == null ? TileManager.Inst.Get(coord.Right()) : suspension.right;

            return suspension.Count;
        }

        public int FindBuildingSuspension(GridCoord coord, ref Suspension suspension)
        {
            suspension.down     =   Get(coord.Down());
            suspension.left     =   Get(coord.Left());
            suspension.right    =   Get(coord.Right());

            return suspension.Count;
        }

        public Neighbors FindNeighbors(GridCoord coord)
        {
            Neighbors neighbors = new Neighbors();

            neighbors.up = Get(coord.Up());
            neighbors.left = Get(coord.Left());
            neighbors.right = Get(coord.Right());

            return neighbors;
        }
    }

    
}    

    
