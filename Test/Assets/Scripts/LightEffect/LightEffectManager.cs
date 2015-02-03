using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Data;
using Const.Effect;

namespace Effect
{
    class LightEffectManager : MonoBehaviour
    {
        private static LightEffectManager instance;

        public static LightEffectManager Inst
        {
            get
            {
                if (instance == null) throw new System.Exception("LightEffectManager not instantiated");
                return instance;
            }
        }

        public LightEffectPool m_lightEffectPool = null; // scene initialized.
        public LightEffectDataSet m_lightEffectSet = null; // scene initialized.

        Dictionary<LightEffectType, LightEffectData> m_lightEffectDic;

        public LightEffectData GetLightEffectData(LightEffectType type)
        {
            if (type == LightEffectType.NONE)
                return null;

            return m_lightEffectDic[type];
        }

        void Awake()
        {
            instance = this;

            m_lightEffectDic = new Dictionary<LightEffectType, LightEffectData>();
            foreach(LightEffectData lData in m_lightEffectSet.effects)
            {
                m_lightEffectDic.Add(lData.m_type, lData);
            }
        }

        public void PlayLightEffect(LightEffectType type, Vector2 position)
        {
            if (type == LightEffectType.NONE) return;

            LightEffect lEffect = m_lightEffectPool.Borrow();
            if (lEffect == null) return;

            LightEffectData lData = GetLightEffectData(type);
            if (lData == null) { lEffect.Stop(); return; }

            lEffect.transform.position = position;
            lEffect.Init(lData);
            lEffect.Play();
        }
    }
}
