using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class RopableController
{
    IRopable m_owner;
    RopableID m_ropableID;

    List<Rope> m_InfectingRopes;

    public RopableController(IRopable owner, RopableID ID)
    {
        m_owner = owner;
        m_ropableID = ID;

        m_InfectingRopes = new List<Rope>();
    }

    public RopableID GetRopableID()
    {
        return m_ropableID;
    }

    public void Roped(Rope newRope)
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
    }
}
