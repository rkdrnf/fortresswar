using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using S2C = Packet.S2C;
using C2S = Packet.C2S;
using Server;
using Const;
using Const.Structure;
using Architecture;
using Maps;
using System.Collections;

[Serializable]
public class Building : Structure<Building, BuildingData>, ISuspension
{
    private BuildingNetwork network
    {
        get { return BuildingNetwork.Inst; }
    }

    public bool m_isFalling;

    public Building()
    { }

    public Building(int ID, BuildingData bData, GridCoord coord)
    {
        Init(bData, coord);
        SetID(ID);
    }

    public Building(int ID, Building building)
    {
        m_data = building.m_data;
        SetID(ID);
        m_coord = building.m_coord;
        
        m_direction = building.m_direction;
        m_health = building.m_health;
        m_isFalling = building.m_isFalling;
        m_collidable = building.m_collidable;
    }

    public Building(S2C.BuildingStatus status)
    {
        m_data = BuildingManager.Inst.GetBuildingData(status.m_type);
        SetID(status.m_ID);
        m_coord = status.m_coord;
        m_direction = status.m_direction;
        m_collidable = true;
        m_isFalling = status.m_falling;
        SetHealth(status.m_health, DestroyReason.MANUAL);

        if (m_isFalling)
        {
            Fall();
        }
    }

    public void Init(BuildingData bData, GridCoord coord)
    {
        if (!Network.isServer) return;

        m_data = bData;
        m_coord = coord;
        m_health = m_data.maxHealth;
        m_collidable = bData.collidable;
        
        FillSuspension();
        FillNeighbors();

        //rendering
        if (ServerGame.Inst.isDedicatedServer) return;
        m_spriteIndex = GetSprite(m_health);
        
    }

    protected override void BroadcastHealth(int health, DestroyReason reason)
    {
        network.BroadcastHealth(m_ID, health, reason);
    }

    public void Fall()
    {
        if (Network.isServer) network.BroadcastFall(m_ID);

        BuildingManager.Inst.Fall(this);

        Lights.ShadowPane.Inst.UpdateLight(m_coord);
        
    }

    void OnTriggerEnter2D(Collider2D targetCollider)
    {
        if (!Network.isServer) return;
        //if (rigidbody2D.isKinematic) return;

        if (targetCollider.tag == "Building" || targetCollider.tag == "Tile")
            SetHealth(0, DestroyReason.COLLIDE);
        
        //damage target collider
    }

    protected override void OnBreak(DestroyReason reason)
    {
        BuildingManager.Inst.Remove(this);

        
        if (!Network.isServer) return;

        switch (reason)
        {
            case DestroyReason.DAMAGE:
                PropagateDestruction();
                break;

            case DestroyReason.MANUAL:
                break;

            case DestroyReason.COLLIDE:
                break;
        }

        Lights.ShadowPane.Inst.UpdateLight(m_coord);
    }

    protected override void OnRecvBreak() { }

    public void PropagateDestruction()
    {
        System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
        watch.Start();

        //int[] m_checkList = new int[]{ m_ID };
        List<int> fallList = new List<int>();
        List<int> noSuspensionList = new List<int>();
        List<int> checkList = new List<int>();

        PropagateDestructionInternal(ref checkList);

        while(true)
        {
            if (checkList.Count == 0)
                break;

            fallList.Clear();
            noSuspensionList.Clear();
            for (int j = 0; j < checkList.Count; j++)
            {
                BuildingManager.Inst.Get(checkList[j]).Check(ref fallList, ref noSuspensionList);
            }

            if (fallList.Count == 0 && noSuspensionList.Count == 0)
                break;

            checkList.Clear();
            for (int i = 0; i < fallList.Count; i++)
            {
                Building building = BuildingManager.Inst.Get(fallList[i]);
                building.PropagateDestructionInternal(ref checkList);
                building.Fall();
            }

            for (int i = 0; i < noSuspensionList.Count; i++)
            {
                Building building = BuildingManager.Inst.Get(noSuspensionList[i]);
                building.PropagateAsNoSuspension(ref checkList);
            }
        }

        watch.Stop();
        Debug.Log("TotalDestructionTime : " + watch.Elapsed + "Frame : " + Time.frameCount);
    }
    public void PropagateDestructionInternal(ref List<int> checkList)
    {
        System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
        watch.Start();

        if (m_neighbors.left != null)
        {
            m_neighbors.left.DestroyNeighbor(GridDirection.RIGHT);
            if (!checkList.Contains(m_neighbors.left.GetID()))
                checkList.Add(m_neighbors.left.GetID());
        }
        if (m_neighbors.right != null)
        { 
            m_neighbors.right.DestroyNeighbor(GridDirection.LEFT);
            if (!checkList.Contains(m_neighbors.right.GetID()))
                checkList.Add(m_neighbors.right.GetID());
        }
        if (m_neighbors.up != null)
        {
            m_neighbors.up.DestroyNeighbor(GridDirection.DOWN);
            if (!checkList.Contains(m_neighbors.up.GetID()))
                checkList.Add(m_neighbors.up.GetID());
        }

        if (m_suspension.down is Building)
        {
            (m_suspension.down as Building).DestroyNeighbor(GridDirection.UP);
        }

        watch.Stop();
        //Debug.Log("DestructionTime : " + watch.Elapsed + "Frame : " + Time.frameCount);
    }

    public void DestroyNeighbor(GridDirection direction)
    {
        m_neighbors.Destroy(direction);
        DestroySuspension(direction);
    }

    public void DestroySuspension(GridDirection direction)
    {
        if (!m_suspension.HasSuspension(direction)) return;

        m_suspension.DestroySuspension(direction);
    }

    public void Check(ref List<int> fallList, ref List<int> noSuspensionList)
    {
        if (m_suspension.Count == 0)
        {
            if (fallList.Contains(m_ID)) Debug.Log("FallList Already Contains ID");
            fallList.Add(m_ID);
            return;
        }
        else
        {
            if (noSuspensionList.Contains(m_ID)) Debug.Log("NoSusList Already Contains ID");
            noSuspensionList.Add(m_ID);
            return;
        }
    }

    public void PropagateAsNoSuspension(ref List<int> checkList)
    {
        if (m_suspension.isPermanent || m_suspension.down != null)
        {
            return; 
        }

        if (m_suspension.down == null)
        {
            if (m_suspension.left == null && m_neighbors.right != null)
            {
                if (!checkList.Contains(m_neighbors.right.GetID())) checkList.Add(m_neighbors.right.GetID());
                m_neighbors.right.DestroySuspension(GridDirection.LEFT);
            }

            if (m_suspension.right == null && m_neighbors.left != null)
            {
                if (!checkList.Contains(m_neighbors.left.GetID())) checkList.Add(m_neighbors.left.GetID());
                m_neighbors.left.DestroySuspension(GridDirection.RIGHT);
            }
        }
    }

    public void AddSuspension(GridDirection direction, Building building)
    {
        if (m_suspension.HasSuspension(direction)) return;

        m_suspension.Add(direction, building);

        PropagateAsSuspension();
    }

    public void AddNeighbor(GridDirection direction, Building building)
    {
        m_neighbors.Add(direction, building);
    }

    public void PropagateAsSuspension()
    {
        if (m_neighbors.up != null)
        {
            m_neighbors.up.AddSuspension(GridDirection.DOWN, this);
        }

        if (m_suspension.isPermanent || m_suspension.down != null) // 영구 지지대거나 아래쪽이 있을 경우 모두에게 지지대로 등록
        {
            m_neighbors.DoForSide((GridDirection direction, Building building) =>
            {
                //if (building.suspension.down != null) return; // 이미 propagate 했어야함.

                building.AddSuspension(direction, this);
            });

            return;
        }

        if (m_suspension.left != null && m_suspension.right != null) //양쪽이 모두 있을 경우 양쪽에게 지지대로 등록
        {
            m_neighbors.DoForSide((GridDirection direction, Building building) =>
                {
                    //if (building.suspension.down != null) return; // 이미 propagate 했어야함.

                    building.AddSuspension(direction, this);
                });
            return;
        }
    }

    public void PropagateAsNeighbor()
    {
        m_suspension.DoForAll((GridDirection direction, ISuspension suspension) =>
        {
            if (suspension is Building)
                (suspension as Building).AddNeighbor(direction, this);
        });
    }

    public void LogForDebug()
    {
        Debug.Log("Current Building Coord : " + m_coord);
        Debug.Log("Suspension: " + m_suspension);
        Debug.Log("Neighbors: " + m_neighbors);

    }

    private Neighbors m_neighbors;
    private Suspension m_suspension;

    public void FillNeighbors()
    {
        m_neighbors = BuildingManager.Inst.FindNeighbors(m_coord);
        PropagateAsSuspension();
    }

    public void FillSuspension()
    {
        m_suspension = BuildingManager.Inst.FindSuspension(m_coord);
        PropagateAsNeighbor();
    }
}


