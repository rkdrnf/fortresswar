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

[Serializable]
public class Building : Structure<Building, BuildingData>
{
    private BuildingNetwork network
    {
        get { return BuildingNetwork.Inst; }
    }

    public bool m_isFalling;

    public Building()
    { }

    public Building(Building building)
    {
        m_ID = building.m_ID;
        m_coord = building.m_coord;
        m_data = building.m_data;
        m_direction = building.m_direction;
        m_health = building.m_health;
        m_isFalling = building.m_isFalling;
    }

    public Building(S2C.BuildingStatus status)
    {
        m_ID = status.m_ID;
        m_coord = status.m_coord;
        m_isFalling = status.m_falling;
        m_direction = status.m_direction;
        SetHealth(status.m_health, DestroyReason.MANUAL);
        if (m_isFalling)
        {
            Fall();
        }
    }

    public void Init(BuildingData bData, GridCoord coord)
    {
        if (!Network.isServer) return;

        m_coord = coord;
        m_health = m_data.maxHealth;
        
        FillSuspension();
        FillNeighbors();

        BuildingManager.Inst.Add(this);

        //rendering
        if (ServerGame.Inst.isDedicatedServer) return;
        GetSprite(m_health);
    }

    protected override void BroadcastHealth(int health, DestroyReason reason)
    {
        network.BroadcastHealth(m_ID, health, reason);
    }

    public void Fall()
    {
        if (Network.isServer) network.BroadcastFall(m_ID);

        BuildingManager.Inst.RemoveFromChunk(this);

        //떨어지는 오브젝트로 새로 생성

        /*
        gameObject.layer = BuildingDataLoader.Inst.fallingBuildingLayer;
        rigidbody2D.isKinematic = false;

        collider2D.isTrigger = true;
        (collider2D as BoxCollider2D).size = (collider2D as BoxCollider2D).size * 0.9f;

        if (!Network.isServer) return;
        BroadcastFall();
         * */
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

        //Network.RemoveRPCs(networkView.viewID);
        //Network.Destroy(gameObject);
    }

    protected override void OnRecvBreak() { }

    public void PropagateDestruction()
    {
        m_neighbors.DoForAll((GridDirection direction, Building building) => { building.DestroyNeighbor(direction); }); 
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

        if (m_suspension.Count == 0)
        { 
            PropagateDestruction();
            Fall();
            return;
        }

        PropagateAsNoSuspension();
    }

    public void PropagateAsNoSuspension()
    {
        if (m_suspension.isPermanent || m_suspension.down != null)
        {
            return; 
        }

        if (m_suspension.down == null)
        {
            if (m_suspension.left == null && m_neighbors.right != null)
            {
                m_neighbors.right.DestroySuspension(GridDirection.LEFT);
            }

            if (m_suspension.right == null && m_neighbors.left != null)
            {
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
        /*
        m_suspension.DoForAll((GridDirection direction, MonoBehaviour behaviour) =>
        {
            if (behaviour is Building)
                (behaviour as Building).AddNeighbor(direction, this);
        });
         * */
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


