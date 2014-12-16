using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using ProtoBuf;
using ProtoBuf.Meta;
namespace Packet
{
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
    }
    namespace S2C
    {
        [ProtoContract]
        public class DamageTile : Packet<DamageTile>
        {
            public DamageTile(int tileID, int damage)
            {
                this.tileID = tileID;
                this.damage = damage;
            }
            [ProtoMember(1)]
            public int tileID;
            [ProtoMember(2)]
            public int damage;
        }

        [ProtoContract]
        public class DestroyTile : Packet<DestroyTile>
        {
            public DestroyTile(Vector3 position)
            {
                this.position = position;
            }

            [ProtoMember(1)]
            public Vector3 position;
        }

        [ProtoContract]
        public class DestroyProjectile : Packet<DestroyProjectile>
        {
            public DestroyProjectile(long projectileID)
            {
                this.projectileID = projectileID;
            }

            [ProtoMember(1)]
            public long projectileID;
        }
    }

    namespace C2S
    {
        [ProtoContract]
        public class Fire : Packet<Fire>
        {
            public Fire(int playerID, long projectileID, BulletType bulletType, Vector3 origin, Vector3 direction)
            {
                this.playerID = playerID;
                this.bulletType = bulletType;
                this.origin = origin;
                this.direction = direction;
                this.projectileID = projectileID;
            }

            [ProtoMember(1)]
            public int playerID;
            [ProtoMember(2)]
            public long projectileID;
            [ProtoMember(3)]
            public BulletType bulletType;
            [ProtoMember(4)]
            public Vector3 origin;
            [ProtoMember(5)]
            public Vector3 direction;
        }

    }
}