using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const;

namespace Data
{
    public class SkillData : KeyValueData<SkillName>
    {
        public SkillName skillName;
        public KeyCode code;
        public Job[] jobs;
        public float coolTime;
        public bool channeling;
        public GameObject skill;

        protected override SkillName GetDataKey()
        {
            return skillName;
        }
    }
}