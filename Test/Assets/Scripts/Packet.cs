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
    namespace S2C
    {
        [ProtoContract]
        public class DamageTile : Packet<DamageTile>
        {
            public DamageTile() { }
            public DamageTile(GridCoord tileCoord, int damage, Vector2 point)
            {
                this.tileCoord = tileCoord;
                this.damage = damage;
                this.point = point;
            }
            [ProtoMember(1)]
            public GridCoord tileCoord;
            [ProtoMember(2)]
            public int damage;
            [ProtoMember(3)]
            public PacketVector2 point;
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
            public long projectileID;
        }

        [ProtoContract]
        public class MapInfo : Packet<MapInfo>
        {
            public MapInfo() { }
            public MapInfo(Dictionary<GridCoord, Tile> tileList)
            {
                tileStatusList = new List<TileStatus>();

                foreach(var tile in tileList)
                {
                    tileStatusList.Add(new TileStatus(tile.Key, tile.Value.m_health));
                }
            }
            [ProtoMember(1)]
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
            public CharacterState state;
        }

        [ProtoContract]
        public class CharacterInfo: Packet<CharacterInfo>
        {
            public CharacterInfo() { }
            public CharacterInfo(Job job, WeaponType weapon, int health, CharacterState state)
            {
                this.job = job;
                this.weapon = weapon;
                this.health = health;
                this.state = state;
            }

            [ProtoMember(1)]
            public Job job;

            [ProtoMember(2)]
            public WeaponType weapon;

            [ProtoMember(3)]
            public int health;

            [ProtoMember(4)]
            public CharacterState state;
        }

        [ProtoContract]
        public class CharacterStatus : Packet<CharacterStatus>
        {
            public CharacterStatus() { }
            public CharacterStatus(int playerID, PlayerSetting setting, CharacterInfo info)
            {
                this.playerID = playerID;
                this.setting = setting;
                this.info = info;
            }
            
            [ProtoMember(1)]
            public int playerID;

            [ProtoMember(2)]
            public CharacterInfo info;

            [ProtoMember(3)]
            public PlayerSetting setting;
        }

        
        [ProtoContract]
        public class ProjectileStatus : Packet<ProjectileStatus>
        {
            public ProjectileStatus() { }
            public ProjectileStatus(int owner, Vector2 position, Vector2 velocity)
            {
                this.position = position;
                this.velocity = velocity;
            }
            [ProtoMember(1)]
            public int owner;

            [ProtoMember(2)]
            public PacketVector2 position;

            [ProtoMember(3)]
            public PacketVector2 velocity;
        }

        [ProtoContract]
        public class RopeStatus : Packet<RopeStatus>
        {
            public RopeStatus() { }
            public RopeStatus(int owner, Vector2 position, Vector2 velocity, RopeStickInfo info)
            {
                this.owner = owner;
                this.position = position;
                this.velocity = velocity;
                this.stickInfo = info;
            }
            [ProtoMember(1)]
            public int owner;

            [ProtoMember(2)]
            public PacketVector2 position;

            [ProtoMember(3)]
            public PacketVector2 velocity;

            [ProtoMember(4)]
            public RopeStickInfo stickInfo;
        }

        [ProtoContract]
        public class RopeStickInfo: Packet<RopeStickInfo>
        {
            public RopeStickInfo() { }
            public RopeStickInfo(bool sticked, long targetID, ObjectType objType, Vector2 pos, Vector2 anchor, Vector2 targetAnchor)
            {
                this.isSticked = sticked;
                this.targetID = targetID;
                this.objType = objType;
                this.position = pos;
                this.anchor = anchor;
                this.targetAnchor = targetAnchor;
            }

            [ProtoMember(1)]
            public long targetID;

            [ProtoMember(2)]
            public ObjectType objType;

            [ProtoMember(3)]
            public PacketVector2 position;

            [ProtoMember(4)]
            public PacketVector2 anchor;

            [ProtoMember(5)]
            public PacketVector2 targetAnchor;

            [ProtoMember(6)]
            public bool isSticked;
        }

        [ProtoContract]
        public class SkillCastInfo: Packet<SkillCastInfo>
        {
            public SkillCastInfo() { }
            public SkillCastInfo(SkillName name, int casterID)
            {
                skillName = name;
                this.casterID = casterID;
            }

            [ProtoMember(1)]
            public SkillName skillName;

            [ProtoMember(2)]
            public int casterID;
        }

        [ProtoContract]
        public class SetStructureHealth: Packet<SetStructureHealth>
        {
            public SetStructureHealth() { }
            public SetStructureHealth(int health, Const.Structure.DestroyReason reason)
            {
                m_health = health;
                m_reason = reason;
            }

            [ProtoMember(1)]
            public int m_health;
            [ProtoMember(2)]
            public Const.Structure.DestroyReason m_reason;
        }

        [ProtoContract]
        public class TileStatus : Packet<TileStatus>
        {
            public TileStatus() { }
            public TileStatus(GridCoord coord, int health )
            {
                m_coord = coord;
                m_health = health;
            }

            [ProtoMember(1)]
            public GridCoord m_coord;
            [ProtoMember(2)]
            public int m_health;

        }

        [ProtoContract]
        public class BuildingStatus : Packet<BuildingStatus>
        {
            public BuildingStatus() { }
            public BuildingStatus(GridCoord coord, int health, bool falling, GridDirection direction)
            {
                m_coord = coord;
                m_health = health;
                m_falling = falling;
                m_direction = direction;
            }

            [ProtoMember(1)]
            public GridCoord m_coord;
            [ProtoMember(2)]
            public int m_health;
            [ProtoMember(3)]
            public bool m_falling;
            [ProtoMember(4)]
            public GridDirection m_direction;
        }

    }

    namespace C2S
    {
        [ProtoContract]
        public class Fire : Packet<Fire>
        {
            public Fire() { }
            public Fire(int playerID, long projectileID, WeaponType weapon, Vector3 direction)
            {
                this.playerID = playerID;
                this.weaponType = weapon;
                this.direction = direction;
                this.projectileID = projectileID;
            }

            [ProtoMember(1)]
            public int playerID;
            [ProtoMember(2)]
            public long projectileID;
            [ProtoMember(3)]
            public WeaponType weaponType;
            [ProtoMember(4)]
            public PacketVector2 direction;
        }

        [ProtoContract]
        public class ChargeWeapon : Packet<ChargeWeapon>
        {
            public ChargeWeapon() { }
            public ChargeWeapon(int playerID, WeaponType weapon)
            {
                this.playerID = playerID;
                this.weaponType = weapon;
            }

            [ProtoMember(1)]
            public int playerID;
            [ProtoMember(2)]
            public WeaponType weaponType;
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
            public int playerID;
            [ProtoMember(2)]
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
            public int playerID;
            [ProtoMember(2)]
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
            public int playerID;
            [ProtoMember(2)]
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
            public Job job;
        }

        [ProtoContract]
        public class CastSkill : Packet<CastSkill>
        {
            public CastSkill() { }
            public CastSkill(int casterID, SkillName skillName, Vector2 direction )
            {
                this.casterID = casterID;
                this.skillName = skillName;
                this.direction = direction;
            }

            [ProtoMember(1)]
            public int casterID;
            [ProtoMember(2)]
            public SkillName skillName;
            [ProtoMember(3)]
            public PacketVector2 direction;
        }

        [ProtoContract]
        public class Build : Packet<Build>
        {
            public Build() { }
            public Build(string buildingName, Vector2 position)
            {
                this.buildingName = buildingName;
                this.position = position;
            }

            [ProtoMember(1)]
            public string buildingName;
            [ProtoMember(2)]
            public PacketVector2 position;
        }
    }
}