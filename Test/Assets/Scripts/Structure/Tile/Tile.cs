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
using Const;

[Serializable]
public class Tile : Structure<Tile, TileData>, ISuspension
{
    private TileNetwork network
    {
        get { return TileNetwork.Inst; }
    }
    
    public Tile(int ID)
    {
        SetID(ID);
    }

    public Tile(int ID, Tile tile)
    {
        m_data = tile.m_data;
        SetID(ID);
        m_coord = tile.m_coord;
        
        m_direction = tile.m_direction;
        m_health = tile.m_health;
        m_spriteIndex = tile.m_spriteIndex;
        m_collidable = tile.m_collidable;
    }

    public Tile(S2C.TileStatus status)
    {
        m_data = TileManager.Inst.GetTileData(status.m_type);
        SetID(status.m_ID);
        m_coord = status.m_coord;
        m_direction = Const.GridDirection.UP;

        SetHealth(status.m_health, DestroyReason.MANUAL);
    }

    public void InitForMaker(TileData tData, GridCoord coord)
    {
        m_data = tData;
        m_coord = coord;
        m_health = tData.maxHealth;
        m_collidable = tData.collidable;

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

