using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const;
using Const.Structure;

namespace Data
{
    [ExecuteInEditMode]
    public class GDataManager : MonoBehaviour
    {
        private static GDataManager instance;

        public static GDataManager Inst
        {
            get { return instance; }
        }

        public void OnEnable()
        {
            if (Application.isPlaying == false && Application.isEditor)
            {
                Awake();
            }
        }

        void Awake()
        {
            instance = this;

            tile = new DataManager<TileType, TileData, TileSet>(m_tileSet);
            building = new DataManager<BuildingType, BuildingData, BuildingDataSet>(m_buildingSet);
            skill = new DataManager<SkillName, SkillData, SkillDataSet>(m_skillSet);
        }

        public TileSet m_tileSet;
        public BuildingDataSet m_buildingSet;
        public SkillDataSet m_skillSet;

        public static DataManager<TileType, TileData, TileSet> tile;
        public static DataManager<BuildingType, BuildingData, BuildingDataSet> building;
        public static DataManager<SkillName, SkillData, SkillDataSet> skill;
    }


    public class DataManager<KEY_TYPE, DATA_TYPE, SET_TYPE> 
        where DATA_TYPE : KeyValueData<KEY_TYPE>
        where SET_TYPE : KeyValueDataSet<DATA_TYPE>
    {
        SET_TYPE m_dataSet;
        Dictionary<KEY_TYPE, DATA_TYPE> m_typeDataDic;

        public DataManager(SET_TYPE dataSet)
        {
            m_dataSet = dataSet;
            m_typeDataDic = new Dictionary<KEY_TYPE, DATA_TYPE>();

            SetData(dataSet);
        }

        public virtual void SetData(SET_TYPE dataSet)
        {
            foreach(DATA_TYPE data in dataSet.m_datas)
            {
                m_typeDataDic.Add((KEY_TYPE)data.GetKey(), data);
            }
        }


        public virtual DATA_TYPE GetData(KEY_TYPE key)
        {
            if (m_typeDataDic.ContainsKey(key))
            {
                return m_typeDataDic[key];
            }

            return null;
        }

        public DATA_TYPE[] GetDatas()
        {
            return m_dataSet.GetDatas();
        }

        public int Count()
        {
            return m_dataSet.GetDatas().Count();
        }
    }
}
