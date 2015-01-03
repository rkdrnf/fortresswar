using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Server;
using C2S = Packet.C2S;
using S2C = Packet.S2C;

namespace Client
{
    public class C_SkillManager : MonoBehaviour
    {
        PlayerBehaviour player;
        Dictionary<KeyCode, SkillInfo> skills;

        SkillManager s_skillManager;

        public void Init(PlayerBehaviour owner)
        {
            s_skillManager = GetComponent<SkillManager>();

            player = owner;
            skills = new Dictionary<KeyCode, SkillInfo>();
        }

        public void LoadSkills(IEnumerable<SkillData> datas)
        {
            skills.Clear();

            foreach (SkillData data in datas)
            {
                skills.Add(data.code, new SkillInfo(data));
            }
        }

        public void Cast(KeyCode code)
        {
            Cast(skills[code]);
        }

        void Cast(SkillInfo skill)
        {
            if (player.IsDead()) return;

            Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
            Vector2 direction = (worldMousePosition - player.transform.position);
            direction.Normalize();

            C2S.CastSkill cast = new C2S.CastSkill(player.GetOwner(), skill.skillData.skillName, direction);

            if (Network.isServer)
            {
                s_skillManager.Cast(cast.SerializeToBytes(), new NetworkMessageInfo());
            }
            else
            {
                networkView.RPC("Cast", RPCMode.Server, cast.SerializeToBytes());
            }
        }

        [RPC]
        void CastSkill(byte[] pckData, NetworkMessageInfo info)
        {
            //ServerCheck

            S2C.SkillCastInfo castInfo = S2C.SkillCastInfo.DeserializeFromBytes(pckData);

            return;
        }

    }
}
