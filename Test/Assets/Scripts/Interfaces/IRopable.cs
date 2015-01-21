using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Const;
using ProtoBuf;
using UnityEngine;

public interface IRopable
{
    RopableID GetRopableID();
    void Roped(Rope rope, Vector2 position);
    void CutInfectingRope(Rope rope);
}

[ProtoContract]
public struct RopableID
{
    public RopableID(ObjectType type, long id)
    {
        m_type = type;
        m_ID = id;
    }

    [ProtoMember(1)]
    public ObjectType m_type;
    [ProtoMember(2)]
    public long m_ID;
}
