using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const;

namespace Server
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

        void Awake()
        {
            instance = this;

            buildingMap = new Dictionary<GridCoord, Building>();

            
        }

        Dictionary<GridCoord, Building> buildingMap;

        public void Add(Building building)
        {
            buildingMap.Add(building.coord, building);
        }

        public Building Get(GridCoord coord)
        {
            if (buildingMap.ContainsKey(coord))
                return buildingMap[coord];
            else
                return null;
        }

        public void Build(BuildingData bData, Vector2 position)
        {
            if (!CanBuild(bData, position)) return;

            Building building = (Building)Network.Instantiate(bData.building, position, Quaternion.identity, 3);
            building.Init(GridCoord.ToCoord(position));
        }

        public bool CanBuild(BuildingData bData, Vector2 position)
        {
            Collider2D[] colliders = Physics2D.OverlapAreaAll(position - (bData.building.size / 2 * 0.9f), position + (bData.building.size / 2 * 0.9f), bData.invalidLocations);
            if (colliders.Length > 0) return false;

            GridCoord coord = GridCoord.ToCoord(position);
            if (FindSuspension(coord).Count == 0) return false;

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
            Map map = Game.Inst.map;

            suspension.center = map.GetTile(coord);
            suspension.down = suspension.down == null ? map.GetTile(coord.Down()) : suspension.down;
            suspension.left = suspension.left == null ? map.GetTile(coord.Left()) : suspension.left;
            suspension.right = suspension.right == null ? map.GetTile(coord.Right()) : suspension.right;

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

        

        void Update()
        {

        }
    }

    public struct Suspension
    {
        public Tile center;
        public MonoBehaviour left;
        public MonoBehaviour down;
        public MonoBehaviour right;

        public bool isPermanent
        {
            get
            {
                return (center != null ||
                    left is Tile ||
                    right is Tile ||
                    down is Tile
                    );
            }
        }

        public int Count
        {
            get { 
                return (center == null ? 0 : 1) + 
                (left == null ? 0 : 1) +
                (right == null ? 0 : 1) +
                (down == null ? 0 : 1);
            }
        }

        public bool HasSuspension(GridDirection direction)
        {
            switch (direction)
            {
                case GridDirection.LEFT:
                    return left != null;

                case GridDirection.RIGHT:
                    return right != null;

                case GridDirection.DOWN:
                    return down != null;
            }

            return false;
        }

        public void DestroySuspension(GridDirection direction)
        {
            switch(direction)
            {
                case GridDirection.LEFT:
                    left = null;
                    break;

                case GridDirection.RIGHT:
                    right = null;
                    break;

                case GridDirection.DOWN:
                    down = null;
                    break;
            }
        }

        public void Add(GridDirection direction, Building building)
        {
            switch (direction)
            {
                case GridDirection.LEFT:
                    left = left == null ? building : left;
                    break;

                case GridDirection.RIGHT:
                    right = right == null ? building : right;
                    break;

                case GridDirection.DOWN:
                    down = down == null ? building : down;
                    break;
            }
        }

        public void DoForAll(Action<GridDirection, MonoBehaviour> action)
        {
            if (left != null)
                action(GridDirection.RIGHT, left);

            if (down != null)
                action(GridDirection.UP, down);

            if (right != null)
                action(GridDirection.LEFT, right);
        }

        public override string ToString()
        {
            return string.Format("Center: {0} Left: {1}, Right: {2}, Down: {3}", center != null, left != null, right != null, down != null);
        }
    }

    public struct Neighbors
    {
        public Building left;
        public Building up;
        public Building right;

        public int Count
        {
            get
            {
                return (left == null ? 0 : 1) +
                (right == null ? 0 : 1) +
                (up == null ? 0 : 1);
            }
        }

        public void Destroy(GridDirection direction)
        {
            switch (direction)
            {
                case GridDirection.LEFT:
                    left = null;
                    break;

                case GridDirection.RIGHT:
                    right = null;
                    break;

                case GridDirection.UP:
                    up = null;
                    break;
            }
        }

        public void Add(GridDirection direction, Building building)
        {
            switch (direction)
            {
                case GridDirection.LEFT:
                    left = building;
                    break;

                case GridDirection.RIGHT:
                    right = building;
                    break;

                case GridDirection.UP:
                    up = building;
                    break;
            }
        }
        
        public void DoForAll(Action<GridDirection, Building> action)
        {
            if (left != null)
                action(GridDirection.RIGHT, left);

            if (up != null)
                action(GridDirection.DOWN, up);

            if (right != null)
                action(GridDirection.LEFT, right);
        }

        public void DoForSide(Action<GridDirection, Building> action)
        {
            if (left != null)
                action(GridDirection.RIGHT, left);

            if (up != null)
                action(GridDirection.DOWN, up);

            if (right != null)
                action(GridDirection.LEFT, right);
        }

        public override string ToString()
        {
            return string.Format("Left: {0}, Right: {1}, Up: {2}", left != null, right != null, up != null);
        }
    }
}
