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
using Const.Effect;
using Effect;
using Data;

[System.Serializable]
public struct SpriteInfo
{
    public int HealthValue;
}

[System.Serializable]
[RequireComponent(typeof(SpriteRenderer), typeof(NetworkView))]
public abstract class Structure<T, DT>
    where T : Structure<T, DT>
    where DT : StructureData
{
    public int m_ID;

    public DT m_data;

    public GridCoord m_coord;
    public GridDirection m_direction;
    public PolygonGenerator<T, DT> m_chunk;

    [HideInInspector]
    public int m_health;

    public int m_spriteIndex;
    public AnimationEffectType m_destructionAnim;

    public Structure()
    {
        m_spriteIndex = -1;
        
        AfterAwake();

        /*
        if (Network.isClient)
        {
            RequestCurrentStatus();
            networkView.RPC("RequestCurrentStatus", RPCMode.Server);
        }
         * */
    }

    public void SetChunk(PolygonGenerator<T, DT> chunk)
    {
        m_chunk = chunk;
    }

    //protected abstract void RequestCurrentStatus(NetworkMessageInfo info);

    //[RPC]
    //protected abstract void RecvCurrentStatus(byte[] pckData, NetworkMessageInfo info);

    public void SetHealth(int health, DestroyReason reason)
    {
        m_health = health;

        if (m_health < 0)
            m_health = 0;

        if (Network.isServer) BroadcastHealth(m_health, reason);
            

        AfterSetHealth(m_health, reason);
    }

    void BroadcastHealth(int health, DestroyReason reason)
    {
        /*
        if (!Network.isServer) return;

        S2C.SetStructureHealth pck = new S2C.SetStructureHealth(health, reason);

        networkView.RPC("RecvHealth", RPCMode.Others, pck.SerializeToBytes());
         * */
    }

    [RPC]
    void RecvHealth(byte[] pckData, NetworkMessageInfo info)
    {
        //ServerCheck

        S2C.SetStructureHealth pck = S2C.SetStructureHealth.DeserializeFromBytes(pckData);

        m_health = pck.m_health;

        AfterSetHealth(m_health, pck.m_reason);
    }

    void AfterSetHealth(int health, DestroyReason reason)
    {
        //m_spriteRenderer.sprite = GetSprite(m_health);

        if (m_health <= 0)
        {
            OnBreak(reason);
        }

        //rendering
        if (Network.isServer && ServerGame.Inst.isDedicatedServer) return;

        if (reason == DestroyReason.COLLIDE || reason == DestroyReason.DAMAGE)
        {
            int particleAmount = 1;
            if (m_health == 0)
            {
                PlayDestructionAnimation();
                particleAmount = 3;
            }

            PlaySplash(particleAmount);
        }
    }

    public void Damage(int damage, Vector2 point)
    {
        if (!Network.isServer) return;

        if (m_data.destroyable)
        {
            SetHealth(m_health - damage, DestroyReason.DAMAGE);
        }
    }

    protected abstract void AfterAwake();

    protected abstract void OnBreak(DestroyReason reason);

    protected abstract void OnRecvBreak();

    protected void Destroy(DestroyReason reason)
    {
        //Network.RemoveRPCs(networkView.viewID);
        //Network.Destroy(gameObject);

        m_chunk.RemoveBlock((T)this);
    }

    protected bool RefreshSprite()
    {
        int newIndex = GetSprite(m_health);

        if (newIndex != m_spriteIndex)
        {
            m_spriteIndex = newIndex;
            return true;
        }

        return false;
    }

    protected int GetSprite(int health)
    {
        int index = 0;

        while (index + 1 < m_data.sprites.Length && health < m_data.sprites[index].HealthValue)
        {
            index++;
        }

        return index;
    }

    protected void PlaySplash(int amount)
    {
        ParticleSystem2D pSystem = ParticleManager.Inst.particleSystemPool.Borrow();
        if (pSystem == null) return;

        ParticleSystem2DData pSysData = ParticleManager.Inst.particleSet.particles[(int)m_data.particleType];
        pSystem.Init(pSysData);
        pSystem.transform.position = m_coord.ToVector2();
        pSystem.ChangeAmount(amount);
        pSystem.Play();
    }

    void PlayDestructionAnimation()
    {
        AnimationEffectManager.Inst.PlayAnimationEffect(m_destructionAnim, m_coord.ToVector2());
    }
}
