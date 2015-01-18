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

        void Awake()
        {
            instance = this;
            m_buildingMap = new Dictionary<GridCoord, Building>();
        }

        public void Clear()
        {
            m_buildingMap.Clear();
        }

        public void Add(Building building)
        {
            m_buildingMap.Add(building.m_coord, building);
        }

        public void Remove(Building building)
        {
            if (m_buildingMap.ContainsKey(building.m_coord) && m_buildingMap[building.m_coord] == building)
                m_buildingMap.Remove(building.m_coord);
        }

        public Building Get(GridCoord coord)
        {
            if (m_buildingMap.ContainsKey(coord))
                return m_buildingMap[coord];
            else
                return null;
        }

        public void Build(BuildingData bData, Vector2 position)
        {
            if (!CanBuild(bData, position)) return;

            Building building = (Building)Network.Instantiate(bData.building, position, Quaternion.identity, 3);
            building.Init(bData, GridCoord.ToCoord(position));
        }

        public bool CanBuild(BuildingData bData, Vector2 position)
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
            Map map = Game.Inst.map;

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

    
