using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using S2C = Packet.S2C;
using C2S = Packet.C2S;
using System;
using Const.Structure;



public class Tile : Structure {

    public Const.TileType m_tileType;
    public Sprite m_tileBack;


    public void Init(TileData tData, Map map)
    {
        if (!Network.isServer) return;

        m_ID = tData.ID;
        m_health = tData.health;
        m_coord = tData.coord;

        //rendering
        if (ServerGame.Inst.isDedicatedServer) return;
        GetSprite(m_health);
    }

    protected override void AfterAwake() { }

    [RPC]
    protected override void RequestCurrentStatus(NetworkMessageInfo info)
    {
        if (!Network.isServer) return;

        S2C.TileStatus pck = new S2C.TileStatus(m_coord, m_health);

        networkView.RPC("RecvCurrentStatus", info.sender, pck.SerializeToBytes());
    }

    [RPC]
    protected override void RecvCurrentStatus(byte[] pckData, NetworkMessageInfo info)
    {
        S2C.TileStatus pck = S2C.TileStatus.DeserializeFromBytes(pckData);

        SetHealth(pck.m_health, DestroyReason.MANUAL);
        m_coord = pck.m_coord;

        Map.Inst.AddTile(this);
        
    }
    
    public override string ToString()
    {
        
        return transform.position.x.ToString() + "\t" + transform.position.y.ToString() + "\t" + ((int)m_tileType).ToString() + "\t" + m_health.ToString();
    }

    protected override void OnBreak(DestroyReason reason)
    {
        Destroy(m_collider);
        Destroy(rigidbody2D);

        //rendering
        if (Network.isServer && ServerGame.Inst.isDedicatedServer) return;

        m_spriteRenderer.sprite = m_tileBack;
    }

    protected override void OnRecvBreak()
    {
        m_spriteRenderer.sprite = m_tileBack;
    }
}

