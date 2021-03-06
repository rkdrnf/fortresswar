﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using S2C = Communication.S2C;
using C2S = Communication.C2S;
using System;
using Const.Structure;
using Architecture;
using Data;
using Maps;
using Const;

[Serializable]
public class Tile : Structure<Tile, TileType, TileData>, ISuspension
{
    private TileNetwork network
    {
        get { return TileNetwork.Inst; }
    }
    
    public Tile(ushort ID, TileData data, GridCoord coord, TileManager manager) // MapMaker Scene
    {
        m_manager = manager;
        m_ID = ID;
        m_type = data.type;
        m_coord = coord;
        m_health = (short)data.maxHealth;
        m_collidable = data.collidable;

        RefreshSprite();
    }

    public Tile(ushort ID, Tile tile, TileManager manager) //Game Scene
    {
        m_manager = manager;
        m_type = tile.m_type;
        SetID(ID);
        m_coord = tile.m_coord;
        
        m_direction = tile.m_direction;
        m_health = tile.m_health;
        m_spriteIndex = tile.m_spriteIndex;
        m_collidable = tile.m_collidable;
    }

    public override TileData GetData()
    {
        return GDataManager.tile.GetData(m_type);
    }

    public void SetDirtyStatus(S2C.TileStatus status)
    {
        SetHealth(status.m_health, DestroyReason.MANUAL);
    }

    public void InitForMaker(TileData tData, GridCoord coord)
    {
        
    }

    public void RemoveForMaker()
    {
        m_chunk.RemoveBlock(this);
    }

    protected override void BroadcastHealth(short health, DestroyReason reason)
    {
        network.BroadcastHealth(m_ID, health, reason);
    }
    
    public override string ToString()
    {

        return string.Format("coord: {0}, type:{2}, health: {3}", m_coord, m_type, m_health);
    }

    protected override void OnBreak(DestroyReason reason)
    {
        Lights.ShadowPane.Inst.UpdateLight(m_coord);

        m_collidable = false;
        //rendering
        if (Network.isServer && ServerGame.Inst.isDedicatedServer) return;
    }

    protected override void OnRecvBreak()
    {
        //m_spriteRenderer.sprite = m_tileBack;
    }
}

