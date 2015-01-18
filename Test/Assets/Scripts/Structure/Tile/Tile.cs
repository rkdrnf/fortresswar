using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using S2C = Packet.S2C;
using C2S = Packet.C2S;
using System;
using Const.Structure;
using Architecture;
using Data;

public class Tile : Structure<Tile, TileData> 
{

    public void InitForMaker(TileData tData, GridCoord coord)
    {
        m_data = tData;
        m_coord = coord;
        m_health = tData.maxHealth;

        RefreshSprite();
    }

    public void RemoveForMaker()
    {
        m_chunk.RemoveBlock(this);
    }

    public void Init(TileData tData)
    {
        /*
        if (!Network.isServer) return;

        m_ID = tData.ID;
        m_health = tData.maxHealth;
        m_coord = tData.coord;

        //rendering
        if (ServerGame.Inst.isDedicatedServer) return;
        GetSprite(m_health);

        TileManager.Inst.Add(this);
         * */
    }

    protected override void AfterAwake() { }

    /*
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

        TileManager.Inst.Add(this);
    }
     * */
    
    public override string ToString()
    {

        return string.Format("coord: {0}, type:{2}, health: {3}", m_coord, m_data.type, m_health);
    }

    protected override void OnBreak(DestroyReason reason)
    {
        /*
        Destroy(m_collider);
        Destroy(rigidbody2D);

        //rendering
        if (Network.isServer && ServerGame.Inst.isDedicatedServer) return;

        m_spriteRenderer.sprite = m_tileBack;
         * */
    }

    protected override void OnRecvBreak()
    {
        //m_spriteRenderer.sprite = m_tileBack;
    }
}

