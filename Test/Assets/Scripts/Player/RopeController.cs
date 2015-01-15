using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const;    

namespace Character
{
    public class RopeController
    {
        Rope m_rope;
        object m_ropeLock = new object();

        List<Rope> m_InfectingRopes;

        ServerPlayer m_player;
        
        public RopeController(ServerPlayer player)
        {
            m_player = player;
            m_rope = null;
            m_InfectingRopes = new List<Rope>();
            
        }

        public void ModifyRopeLength(float modifier)
        {
            m_rope.ModifyLength(modifier);
        }

        public Vector2 GetRopeDirection()
        {
            return m_rope.GetNormalVector();
        }

        public void Roped(Rope newRope)
        {
            lock (m_ropeLock)
            {
                if (newRope != null)
                {
                    m_player.SetState(CharacterState.ROPING);
                    m_player.ToAirMaterial();
                }
            }
        }

        public void RopedToMe(Rope newRope)
        {
            m_InfectingRopes.Add(newRope);
        }

        public void CutInfectingRope(Rope rope)
        {
            m_InfectingRopes.Remove(rope);
        }

        public void CutRopeAll()
        {
            foreach (Rope rope in m_InfectingRopes)
            {
                rope.Cut();
            }

            CutRope();
        }

        public void OnFireRope(Rope newRope)
        {
            lock (m_ropeLock)
            {
                if (m_rope != null)
                    CutRope();

                m_rope = newRope;
            }
        }

        public void CutRope()
        {
            lock (m_ropeLock)
            {
                if (m_rope != null)
                {
                    m_rope.Cut();
                    m_rope = null;
                }
            }
        }

        public void StopRoping()
        {
            m_rope = null;
            m_player.SetState(CharacterState.FALLING);
            m_player.ToGroundMaterial();
        }
    }
}
