using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.IO.Compression;


namespace Communication
{
    [ProtoContract]
    public abstract class Packet<T> : MessageBase where T : Packet<T>
    {
        int chunkSize = 512;

        protected bool m_compress = false;

        public override void Serialize(NetworkWriter writer)
        {
            Debug.Log("Serialized");
            byte[] source = SerializeToBytes();
            int index = 0;
            byte[] buffer = new byte[chunkSize];

            if (chunkSize >= source.Length)
            {
                writer.WriteBytesFull(source);
                return;
            }

            while (index + chunkSize < source.Length)
            {
                Array.Copy(source, index, buffer, 0, chunkSize);
                writer.WriteBytesAndSize(buffer, chunkSize);
                index += chunkSize;
            }

            Array.Copy(source, index, buffer, 0, source.Length - index);
            writer.WriteBytesAndSize(buffer, source.Length - index);
        }

        public override void Deserialize(NetworkReader reader)
        {
            Debug.Log("Deserialized");

            IEnumerable<byte> result = new byte[0];

            do
            {
                byte[] bytes = reader.ReadBytesAndSize();
                result = result.Concat(bytes);

                if (bytes.Length < chunkSize)
                    break;
            }
            while (true);

            DeserializeFromBytes(result.ToArray());
        }

        public byte[] SerializeToBytes()
        {
            MemoryStream ms = new MemoryStream();
            Serializer.Serialize<T>(ms, (T)this);

            byte[] padding = new byte[1] { 0 };
            byte[] pck = ms.ToArray();

            pck = pck.Concat(padding).ToArray<byte>();

            if (m_compress)
            {
                pck = LZMAtools.CompressByteArrayToLZMAByteArray(pck);
            }

            return pck;
        }

        public void DeserializeFromBytes(byte[] arrBytes)
        {
            byte[] pck;

            if (m_compress)
            {
                pck = LZMAtools.DecompressLZMAByteArrayToByteArray(arrBytes);
            }

            pck = arrBytes.Take(arrBytes.Length - 1).ToArray<byte>();
            MemoryStream ms = new MemoryStream(pck);

            FillPacket(Serializer.Deserialize<T>(ms));
        }

        public virtual void FillPacket(T packet)
        {
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
