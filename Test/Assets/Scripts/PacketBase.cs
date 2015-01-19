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
    public abstract class Packet<T> where T : Packet<T>
    {
        public byte[] SerializeToBytes()
        {
            MemoryStream ms = new MemoryStream();
            Serializer.Serialize<T>(ms, (T)this);

            byte[] padding = new byte[1] { 0 };
            byte[] pck = ms.ToArray();

            return pck.Concat(padding).ToArray<byte>();
        }

        public static T DeserializeFromBytes(byte[] arrBytes)
        {
            byte[] pck = arrBytes.Take(arrBytes.Length - 1).ToArray<byte>();
            MemoryStream ms = new MemoryStream(pck);
            return Serializer.Deserialize<T>(ms);
        }
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
