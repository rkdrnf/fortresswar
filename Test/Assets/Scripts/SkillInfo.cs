using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SkillInfo
{
    public SkillInfo(SkillData data)
    {
        this.skillData = data;
        coolDownTimer = 0f;
        isCasting = false;
    }

    public SkillData skillData;
    public float coolDownTimer;
    public bool isCasting;
    public ServerPlayer caster;

    public void Cast()
    {
        GameObject skillObj = (GameObject)MonoBehaviour.Instantiate(skillData.skill, caster.transform.position, Quaternion.identity);


    }
}
