using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const;
using Data;
    
public class SkillDataLoader : MonoBehaviour
{
    private static SkillDataLoader instance;

    public static SkillDataLoader Inst
    {
        get
        {
            if (instance == null)
            {
                instance = new SkillDataLoader();
            }
            return instance;
        }
    }

    void Awake()
    {
        instance = this;

        FillJobSkillSet();
    }


    public SkillDataSet skillSet;
    private Dictionary<Job, List<SkillData>> jobSkillSet;

    void FillJobSkillSet()
    {
        jobSkillSet = new Dictionary<Job, List<SkillData>>();
        foreach (SkillData data in skillSet.GetDatas())
        {
            foreach (Job job in data.jobs)
            {
                if (jobSkillSet.ContainsKey(job))
                {
                    jobSkillSet[job].Add(data);
                }
                else
                {
                    jobSkillSet.Add(job, new List<SkillData>());
                    jobSkillSet[job].Add(data);
                }
            }
        }
    }

    public List<SkillData> GetJobSkills(Job job)
    {
        return jobSkillSet[job];
    }
}
