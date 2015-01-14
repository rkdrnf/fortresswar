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

[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(Collider2D))]
public class Building : Structure
{
    bool m_isFalling;

    protected override void AfterAwake()
    {
        rigidbody2D.isKinematic = true;
    }

    [RPC]
    protected override void RequestCurrentStatus(NetworkMessageInfo info)
    {
        S2C.BuildingStatus pck = new S2C.BuildingStatus(m_coord, m_health, m_isFalling, m_direction);

        networkView.RPC("RecvCurrentStatus", info.sender, pck.SerializeToBytes());
    }

    [RPC]
    protected override void RecvCurrentStatus(byte[] pckData, NetworkMessageInfo info)
    {
        S2C.BuildingStatus pck = S2C.BuildingStatus.DeserializeFromBytes(pckData);

        m_coord = pck.m_coord;
        m_health = pck.m_health;
        
        m_direction = pck.m_direction;
        if (pck.m_falling)
        {
            Fall();
        }

    }

    public void Init(GridCoord coord)
    {
        m_coord = coord;
 
        if (Network.isServer)
        {
            BuildingManager.Inst.Add(this);
            FillSuspension();
            FillNeighbors();
        }
    }

    public void Fall()
    {
        //if (!Network.isServer) return;

        gameObject.layer = BuildingDataLoader.Inst.fallingBuildingLayer;
        rigidbody2D.isKinematic = false;

        collider2D.isTrigger = true;
        (collider2D as BoxCollider2D).size = (collider2D as BoxCollider2D).size * 0.9f;
    }

    void BroadcastFall()
    {
        //서버는 이미 처리 다 해서 또 보낼 필요 없음
        networkView.RPC("RecvFall", RPCMode.Others);
    }

    [RPC]
    void RecvFall()
    {
        gameObject.layer = BuildingDataLoader.Inst.fallingBuildingLayer;
        rigidbody.isKinematic = false;
        collider2D.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D targetCollider)
    {
        if (!Network.isServer) return;
        if (rigidbody2D.isKinematic) return;

        if (targetCollider.tag == "Building" || targetCollider.tag == "Tile")
            OnBreak(DestroyReason.COLLIDE);
        
        //damage target collider
    }

    protected override void OnBreak(DestroyReason reason)
    {
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

        Network.RemoveRPCs(networkView.viewID);
        Network.Destroy(gameObject);
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
        m_suspension.DoForAll((GridDirection direction, MonoBehaviour behaviour) =>
        {
            if (behaviour is Building)
                (behaviour as Building).AddNeighbor(direction, this);
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


