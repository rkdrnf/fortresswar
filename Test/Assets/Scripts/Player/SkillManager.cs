using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using S2C = Communication.S2C;
using C2S = Communication.C2S;
using Const;
using Data;

namespace Server
{
    public class SkillManager : MonoBehaviour
    {
        ServerPlayer player;
        Dictionary<SkillName, SkillInfo> skills;
        Dictionary<KeyCode, SkillName> keyNameDic;

        public void Init(ServerPlayer owner)
        {
            player = owner;
            skills = new Dictionary<SkillName, SkillInfo>();
            keyNameDic = new Dictionary<KeyCode, SkillName>();
        }

        public void LoadSkills(IEnumerable<SkillData> datas)
        {
            skills.Clear();

            foreach(SkillData data in datas)
            {
                skills.Add(data.skillName, new SkillInfo(data));
                keyNameDic.Add(data.code, data.skillName);
            }
        }

        public void TryCast(KeyCode code)
        {
            TryCast(skills[keyNameDic[code]]);
        }

        void TryCast(SkillInfo skill)
        {
            if (player.IsDead()) return;

            Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
            Vector2 direction = (worldMousePosition - player.transform.position);
            direction.Normalize();

            C2S.CastSkill cast = new C2S.CastSkill(player.GetOwner(), skill.skillData.skillName, direction);

            if (Network.isServer)
            {
                Cast(cast.SerializeToBytes(), new NetworkMessageInfo());
            }
            else
            {
                GetComponent<NetworkView>().RPC("Cast", RPCMode.Server, cast.SerializeToBytes());
            }
        }

        public void Cast(byte[] pckData, NetworkMessageInfo info)
        {
            //ServerCheck

            C2S.CastSkill pck = new C2S.CastSkill();
            pck.DeserializeFromBytes(pckData);

            if (CanCast(pck.skillName))
                return;

            return;
        }

        public bool CanCast(SkillName skill)
        {
            return true;
        }

        void BroadcastCast(SkillInfo info)
        {
            if (!Network.isServer) return;

            S2C.SkillCastInfo castInfo = new S2C.SkillCastInfo(info.skillData.skillName, player.GetOwner());
            if (!ServerGame.Inst.isDedicatedServer)
            {
                GetComponent<NetworkView>().RPC("RecvCast", RPCMode.All, castInfo.SerializeToBytes());
            }
            else
            {
                GetComponent<NetworkView>().RPC("RecvCast", RPCMode.Others, castInfo.SerializeToBytes());
            }
        }

        [RPC]
        void RecvCast(byte[] pckData, NetworkMessageInfo info)
        {
            //ServerCheck

            S2C.SkillCastInfo castInfo = new S2C.SkillCastInfo();
            castInfo.DeserializeFromBytes(pckData);

            return;
        }

        void FixedUpdate()
        {
            RefreshCooldown(Time.fixedDeltaTime);
        }

        void RefreshCooldown(float time)
        {
            foreach (SkillInfo info in skills.Values)
            {
                info.coolDownTimer -= time;
            }
        }

    }
}
