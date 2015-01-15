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

[System.Serializable]
public struct spriteInfo
{
    public Sprite sprite;
    public int HealthValue;
}

[RequireComponent(typeof(SpriteRenderer), typeof(NetworkView))]
public abstract class Structure : MonoBehaviour
{
    public int m_ID;
    public GridCoord m_coord;
    public Vector2 m_size;
    public GridDirection m_direction;
    public bool m_destroyable;
    public int m_maxHealth;
    public spriteInfo[] m_sprites;
    public ParticleType m_particleType;

    [HideInInspector]
    public int m_health;
    [HideInInspector]
    public Map m_map;

    protected SpriteRenderer m_spriteRenderer;
    protected BoxCollider2D m_collider;

    public AnimationEffectType m_destructionAnim;

    void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        
        m_collider = GetComponent<BoxCollider2D>();

        AfterAwake();

        if (Network.isClient)
        {
            networkView.RPC("RequestCurrentStatus", RPCMode.Server);
        }
    }

    [RPC]
    protected abstract void RequestCurrentStatus(NetworkMessageInfo info);

    [RPC]
    protected abstract void RecvCurrentStatus(byte[] pckData, NetworkMessageInfo info);

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
        if (!Network.isServer) return;

        S2C.SetStructureHealth pck = new S2C.SetStructureHealth(health, reason);

        networkView.RPC("RecvHealth", RPCMode.Others, pck.SerializeToBytes());
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
        m_spriteRenderer.sprite = GetSprite(m_health);

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

        if (m_destroyable)
        {
            SetHealth(m_health - damage, DestroyReason.DAMAGE);
        }
    }

    protected abstract void AfterAwake();

    protected abstract void OnBreak(DestroyReason reason);

    protected abstract void OnRecvBreak();

    protected void Destroy(DestroyReason reason)
    {
        Network.RemoveRPCs(networkView.viewID);
        Network.Destroy(gameObject);
    }

    protected Sprite GetSprite(int health)
    {
        int index = 0;

        while (index + 1 < m_sprites.Length && health < m_sprites[index].HealthValue)
        {
            index++;
        }

        return m_sprites[index].sprite;
    }

    protected void PlaySplash(int amount)
    {
        ParticleSystem2D pSystem = ParticleManager.Inst.particleSystemPool.Borrow();
        if (pSystem == null) return;

        ParticleSystem2DData pSysData = ParticleManager.Inst.particleSet.particles[(int)m_particleType];
        pSystem.Init(pSysData);
        pSystem.transform.position = transform.position;
        pSystem.ChangeAmount(amount);
        pSystem.Play();
    }

    void PlayDestructionAnimation()
    {
        AnimationEffectManager.Inst.PlayAnimationEffect(m_destructionAnim, transform.position);
    }
}
