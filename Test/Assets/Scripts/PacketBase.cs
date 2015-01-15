using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using System.IO;
using UnityEngine;
using Packet.S2C;
using Packet.C2S;
namespace Packet
{
    [ProtoContract]
    [ProtoInclude(2,    typeof(DamageTile))]
    [ProtoInclude(3,    typeof(DestroyTile))]
    [ProtoInclude(4,    typeof(DestroyProjectile))]
    [ProtoInclude(5,    typeof(MapInfo))]
    [ProtoInclude(6,    typeof(PlayerNotReady))]
    [ProtoInclude(7,    typeof(CharacterChangeState))]
    [ProtoInclude(8,    typeof(Packet.S2C.CharacterInfo))]
    [ProtoInclude(9,    typeof(CharacterStatus))]
    [ProtoInclude(10,   typeof(ProjectileStatus))]
    [ProtoInclude(11,   typeof(RopeStatus))]
    [ProtoInclude(12,   typeof(RopeStickInfo))]
    [ProtoInclude(13,   typeof(SkillCastInfo))]
    [ProtoInclude(14,   typeof(SetStructureHealth))]
    [ProtoInclude(15,   typeof(TileStatus))]
    [ProtoInclude(16,   typeof(BuildingStatus))]
    [ProtoInclude(17,   typeof(Fire))]
    [ProtoInclude(18,   typeof(ChargeWeapon))]
    [ProtoInclude(19,   typeof(UpdatePlayerName))]
    [ProtoInclude(20,   typeof(UpdatePlayerTeam))]
    [ProtoInclude(21,   typeof(UpdatePlayerStatus))]
    [ProtoInclude(22,   typeof(ChangeJob))]
    [ProtoInclude(23,   typeof(CastSkill))]
    [ProtoInclude(24,   typeof(Build))]
    public abstract class Packet<T> where T : Packet<T>
    {
        public byte[] SerializeToBytes()
        {
            MemoryStream ms = new MemoryStream();
            Serializer.Serialize<T>(ms, (T)this);
            return ms.ToArray();
        }

        public static T DeserializeFromBytes(byte[] arrBytes)
        {
            MemoryStream ms = new MemoryStream(arrBytes);
            return Serializer.Deserialize<T>(ms);
        }

        [ProtoMember(1)]
        public int m_packetID = -1;
    }

    [ProtoContract]
    public class PacketVector2
    {
        [ProtoMember(1)]
        public float x { get; set; }

        [ProtoMember(2)]
        public float y { get; set; }


        public PacketVector2()
        {
            this.x = 0.0f;
            this.y = 0.0f;
        }

        public PacketVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static implicit operator Vector3(PacketVector2 v)
        {
            return new Vector3(v.x, v.y, 0f);
        }

        public static implicit operator Vector2(PacketVector2 v)
        {
            return new Vector2(v.x, v.y);
        }

        public static implicit operator PacketVector2(Vector3 v)
        {
            return new PacketVector2(v.x, v.y);
        }

        public static implicit operator PacketVector2(Vector2 v)
        {
            return new PacketVector2(v.x, v.y);
        }
    }
}
