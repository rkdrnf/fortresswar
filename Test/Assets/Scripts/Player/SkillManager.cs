using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using S2C = Packet.S2C;
using C2S = Packet.C2S;
using Const;

namespace Server
{
    public class SkillManager : MonoBehaviour
    {
        ServerPlayer player;
        Dictionary<SkillName, SkillInfo> skills;

        public void Init(ServerPlayer owner)
        {
            player = owner;
            skills = new Dictionary<SkillName, SkillInfo>();
        }

        public void LoadSkills(IEnumerable<SkillData> datas)
        {
            skills.Clear();

            foreach(SkillData data in datas)
            {
                skills.Add(data.skillName, new SkillInfo(data));
            }
        }

        public void Cast(byte[] pckData, NetworkMessageInfo info)
        {
            //ServerCheck

            C2S.CastSkill pck = C2S.CastSkill.DeserializeFromBytes(pckData);

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
                networkView.RPC("CastSkill", RPCMode.All, castInfo.SerializeToBytes());
            }
            else
            {
                networkView.RPC("CastSkill", RPCMode.Others, castInfo.SerializeToBytes());
            }
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
