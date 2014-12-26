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

    [ProtoContract]
    public class TileStatus
    {
        public TileStatus() { }
        public TileStatus(int ID, int health)
        {
            this.ID = ID;
            this.health = health;
        }
        [ProtoMember(1)]
        public int packetID = -1;
        [ProtoMember(2)]
        public int ID;
        [ProtoMember(3)]
        public int health;
    }

    namespace S2C
    {
        [ProtoContract]
        public class DamageTile : Packet<DamageTile>
        {
            public DamageTile() { }
            public DamageTile(int tileID, int damage)
            {
                this.tileID = tileID;
                this.damage = damage;
            }
            [ProtoMember(1)]
            public int packetID = -1;
            [ProtoMember(2)]
            public int tileID;
            [ProtoMember(3)]
            public int damage;
        }

        [ProtoContract]
        public class DestroyTile : Packet<DestroyTile>
        {
            public DestroyTile() { }
            public DestroyTile(Vector3 position)
            {
                this.position = position;
            }

            [ProtoMember(1)]
            public int packetID = -1;
            [ProtoMember(2)]
            public Vector3 position;
        }

        [ProtoContract]
        public class DestroyProjectile : Packet<DestroyProjectile>
        {
            public DestroyProjectile() { }
            public DestroyProjectile(long projectileID)
            {
                this.projectileID = projectileID;
            }

            [ProtoMember(1)]
            public int packetID = -1;
            [ProtoMember(2)]
            public long projectileID;
        }

        [ProtoContract]
        public class PlayerList : Packet<PlayerList>
        {
            public PlayerList() { }
            public PlayerList(List<PlayerSetting> settings)
            {
                this.settings = settings;
            }

            [ProtoMember(1)]
            public int packetID = -1;
            [ProtoMember(2)]
            public List<PlayerSetting> settings;
        }

        [ProtoContract]
        public class MapInfo : Packet<MapInfo>
        {
            public MapInfo() { }
            public MapInfo(Dictionary<int, Tile> tileList)
            {
                tileStatusList = new List<TileStatus>();

                foreach(var tile in tileList)
                {
                    tileStatusList.Add(new TileStatus(tile.Key, tile.Value.health));
                }
            }
            [ProtoMember(1)]
            public int packetID = -1;
            [ProtoMember(2)]
            public List<TileStatus> tileStatusList;
        }

        [ProtoContract]
        public class PlayerNotReady : Packet<PlayerNotReady>
        {
            public PlayerNotReady() { }
            public PlayerNotReady(PlayerSettingError error)
            {
                this.error = error;
            }

            [ProtoMember(1)]
            public int packetID = -1;
            [ProtoMember(2)]
            public PlayerSettingError error;
        }

        [ProtoContract]
        public class CharacterChangeState : Packet<CharacterChangeState>
        {
            public CharacterChangeState() { }
            public CharacterChangeState(CharacterState state)
            {
                this.state = state;
            }

            [ProtoMember(1)]
            public int packetID = -1;

            [ProtoMember(2)]
            public CharacterState state;
        }

        [ProtoContract]
        public class CharacterStatus : Packet<CharacterStatus>
        {
            public CharacterStatus() { }
            public CharacterStatus(Job job, WeaponType weapon, int health, CharacterState state)
            {
                this.job = job;
                this.weapon = weapon;
                this.health = health;
            }

            [ProtoMember(1)]
            public int packetID = -1;

            [ProtoMember(2)]
            public Job job;

            [ProtoMember(3)]
            public WeaponType weapon;

            [ProtoMember(4)]
            public int health;

            [ProtoMember(5)]
            public CharacterState state;
        }

        [ProtoContract]
        public class RopeStuck : Packet<RopeStuck>
        {
            public RopeStuck() { }
            public RopeStuck(long targetID, ObjectType objType, Vector2 ropePos, Vector2 ropeAnchor, Vector2 targetAnchor)
            {
                this.targetID = targetID;
                this.objType = objType;
                this.ropePos = ropePos;
                this.ropeAnchor = ropeAnchor;
                this.targetAnchor = targetAnchor;
            }

            [ProtoMember(1)]
            public int packetID = -1;

            [ProtoMember(2)]
            public long targetID;

            [ProtoMember(3)]
            public ObjectType objType;

            [ProtoMember(4)]
            public Vector2 ropePos;

            [ProtoMember(5)]
            public Vector2 ropeAnchor;

            [ProtoMember(6)]
            public Vector2 targetAnchor;
        }


    }

    namespace C2S
    {
        [ProtoContract]
        public class Fire : Packet<Fire>
        {
            public Fire() { }
            public Fire(int playerID, long projectileID, WeaponType weapon, Vector3 origin, Vector3 direction)
            {
                this.playerID = playerID;
                this.weaponType = weapon;
                this.origin = origin;
                this.direction = direction;
                this.projectileID = projectileID;
            }

            [ProtoMember(1)]
            public int packetID = -1;
            [ProtoMember(2)]
            public int playerID;
            [ProtoMember(3)]
            public long projectileID;
            [ProtoMember(4)]
            public WeaponType weaponType;
            [ProtoMember(5)]
            public PacketVector2 origin;
            [ProtoMember(6)]
            public PacketVector2 direction;
        }

        [ProtoContract]
        public class UpdatePlayerName : Packet<UpdatePlayerName>
        {
            public UpdatePlayerName() { }
            public UpdatePlayerName(int playerID, string name)
            {
                this.playerID = playerID;
                this.name = name;
            }


            [ProtoMember(1)]
            public int packetID = -1;
            [ProtoMember(2)]
            public int playerID;
            [ProtoMember(3)]
            public string name;
        }

        [ProtoContract]
        public class UpdatePlayerTeam : Packet<UpdatePlayerTeam>
        {
            public UpdatePlayerTeam() { }
            public UpdatePlayerTeam(int playerID, Team team)
            {
                this.playerID = playerID;
                this.team = team;
            }

            [ProtoMember(1)]
            public int packetID = -1;
            [ProtoMember(2)]
            public int playerID;
            [ProtoMember(3)]
            public Team team;
        }

        [ProtoContract]
        public class UpdatePlayerStatus : Packet<UpdatePlayerStatus>
        {
            public UpdatePlayerStatus() { }
            public UpdatePlayerStatus(int playerID, PlayerStatus status)
            {
                this.playerID = playerID;
                this.status = status;
            }

            [ProtoMember(1)]
            public int packetID = -1;
            [ProtoMember(2)]
            public int playerID;
            [ProtoMember(3)]
            public PlayerStatus status;
        }

        [ProtoContract]
        public class ChangeJob : Packet<ChangeJob>
        {
            public ChangeJob() { }
            public ChangeJob(Job newJob)
            {
                this.job = newJob;
            }

            [ProtoMember(1)]
            public int packetID = -1;

            [ProtoMember(2)]
            public Job job;
        }
    }
}