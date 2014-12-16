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
    [ProtoContract]
    public class PacketNetworkPlayer
    {
        public PacketNetworkPlayer()
        { }

        public PacketNetworkPlayer(NetworkPlayer player)
        {
            networkPlayer = player;
        }

        public NetworkPlayer networkPlayer
        {
            get
            {
                NetworkPlayer player = new NetworkPlayer(ip, port);
                return player;
            }

            set
            {
                ip = value.ipAddress;
                port = value.port;
            }
        }

        [ProtoMember(1)]
        public string ip;
        [ProtoMember(2)]
        public int port;
    }

    public abstract class Packet<T> where T : Packet<T>
    {
        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, typeof(T), Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

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

        public static T Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
    namespace S2C
    {
        public class DamageTile : Packet<DamageTile>
        {
            public DamageTile(int tileID, int damage)
            {
                this.tileID = tileID;
                this.damage = damage;
            }

            public int tileID;
            public int damage;
        }

        public class DestroyTile : Packet<DestroyTile>
        {
            public DestroyTile(Vector3 position)
            {
                this.position = position;
            }

            public Vector3 position;
        }

        public class DestroyProjectile : Packet<DestroyProjectile>
        {
            public DestroyProjectile(long projectileID)
            {
                this.projectileID = projectileID;
            }

            public long projectileID;
        }
    }

    namespace C2S
    {
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

            public int playerID;
            public long projectileID;
            public BulletType bulletType;
            public Vector3 origin;
            public Vector3 direction;
        }

    }
}