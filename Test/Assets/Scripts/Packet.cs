using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const;
using Newtonsoft.Json;

namespace Packet
{
    public abstract class Packet<T>
    {
        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, typeof(T), Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
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
            public DamageTile(Vector3 position, int damage)
            {
                this.position = position;
                this.damage = damage;
            }

            public Vector3 position;
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
            public Fire(long projectileID, BulletType bulletType, Vector3 origin, Vector3 direction)
            {
                this.bulletType = bulletType;
                this.origin = origin;
                this.direction = direction;
                this.ID = projectileID;
            }

            public long ID;
            public BulletType bulletType;
            public Vector3 origin;
            public Vector3 direction;
        }
    }
}