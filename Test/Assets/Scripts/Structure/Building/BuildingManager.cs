using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const;

namespace Architecture
{ 
    public class BuildingManager : MonoBehaviour
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

        int m_buildingIndex;

        public BuildingChunkManager m_buildingChunkManager;

        void Awake()
        {
            instance = this;
            m_buildingIndex = 0;
            m_buildingIDMap = new Dictionary<int, Building>();
            m_buildingMap = new Dictionary<GridCoord, Building>();
        }

        public void Clear()
        {
            m_buildingMap.Clear();
            m_buildingIDMap.Clear();
            m_buildingChunkManager.Clear();
        }

        public void Add(Building building)
        {
            m_buildingMap.Add(building.m_coord, building);
            m_buildingIDMap.Add(building.GetID(), building);

            m_buildingChunkManager.AddBlock(building);

            m_buildingIndex++;
        }

        public void Remove(Building building)
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


        public Building Get(GridCoord coord)
        {
            if (m_buildingMap.ContainsKey(coord))
                return m_buildingMap[coord];
            else
                return null;
        }

        public Building Get(int ID)
        {
            if (m_buildingIDMap.ContainsKey(ID))
                return m_buildingIDMap[ID];
            else
                return null;
        }

        public List<Building> GetBuildings()
        {
            return m_buildingMap.Values.ToList();
        }

        public void Build(BuildingData bData, GridCoord coord)
        {
            if (!CanBuild(bData, coord)) return;

            //Building building = (Building)Network.Instantiate(bData.building, position, Quaternion.identity, 3);
            //building.Init(bData, GridCoord.ToCoord(position));
        }

        public bool CanBuild(BuildingData bData, GridCoord coord)
        {
            /*
            Collider2D[] colliders = Physics2D.OverlapAreaAll(position - (bData.building.m_size / 2 * 0.9f), position + (bData.building.m_size / 2 * 0.9f), bData.invalidLocations);
            if (colliders.Length > 0) return false;

            GridCoord coord = GridCoord.ToCoord(position);
            if (FindSuspension(coord).Count == 0) return false;
            */
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
            /*
            Map map = Game.Inst.map;

            suspension.center = TileManager.Inst.Get(coord);
            suspension.down = suspension.down == null ? TileManager.Inst.Get(coord.Down()) : suspension.down;
            suspension.left = suspension.left == null ? TileManager.Inst.Get(coord.Left()) : suspension.left;
            suspension.right = suspension.right == null ? TileManager.Inst.Get(coord.Right()) : suspension.right;
            */
            return suspension.Count;
        }

        public int FindBuildingSuspension(GridCoord coord, ref Suspension suspension)
        {
            /*
            suspension.down     =   Get(coord.Down());
            suspension.left     =   Get(coord.Left());
            suspension.right    =   Get(coord.Right());
            */
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

    
