using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using S2C = Packet.S2C;
using C2S = Packet.C2S;
using System;
using Const.Structure;
using Architecture;
using Data;
using Maps;

[Serializable]
public class Tile : Structure<Tile, TileData> 
{
    private TileNetwork network
    {
        get { return TileNetwork.Inst; }
    }
    
    public Tile()
    {
    }

    public Tile(Tile tile)
    {
        m_ID = tile.m_ID;
        m_coord = tile.m_coord;
        m_data = tile.m_data;
        m_direction = tile.m_direction;
        m_health = tile.m_health;
        m_spriteIndex = tile.m_spriteIndex;

        if (m_health > 0)
            m_collidable = true;
    }

    public Tile(S2C.TileStatus status)
    {
        m_ID = status.m_ID;
        m_coord = status.m_coord;
        m_data = TileManager.Inst.GetTileData(status.m_type);
        m_direction = Const.GridDirection.UP;

        SetHealth(status.m_health, DestroyReason.MANUAL);
        if (m_health > 0)
        {
            m_collidable = true;
        }
    }

    public void InitForMaker(TileData tData, GridCoord coord)
    {
        m_data = tData;
        m_coord = coord;
        m_health = tData.maxHealth;

        if (m_health > 0)
            m_collidable = true;

        RefreshSprite();
    }

    public void RemoveForMaker()
    {
        m_chunk.RemoveBlock(this);
    }

    protected override void BroadcastHealth(int health, DestroyReason reason)
    {
        network.BroadcastHealth(m_ID, health, reason);
    }
    
    public override string ToString()
    {

        return string.Format("coord: {0}, type:{2}, health: {3}", m_coord, m_data.type, m_health);
    }

    protected override void OnBreak(DestroyReason reason)
    {
        m_collidable = false;
        //rendering
        if (Network.isServer && ServerGame.Inst.isDedicatedServer) return;
    }

    protected override void OnRecvBreak()
    {
        //m_spriteRenderer.sprite = m_tileBack;
    }
}

