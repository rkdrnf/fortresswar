using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const;
using Data;
using Const.Effect;
namespace Effect
{
    class AnimationEffectManager : MonoBehaviour
    {
        private static AnimationEffectManager instance;

        public static AnimationEffectManager Inst
        {
            get
            {
                if (instance == null) throw new System.Exception("AnimationEffectManager not instantiated");
                return instance;
            }
        }

        [HideInInspector]
        public AnimationEffectPool m_animationEffectPool;

        public AnimationEffect m_animationEffectPrefab;

        public AnimationEffectDataSet m_animationEffectSet;

        [HideInInspector]
        public Dictionary<AnimationEffectType, AnimationEffectData> m_animationEffectDic;

        public AnimationEffectData GetAnimationEffectData(AnimationEffectType type)
        {
            if (type == AnimationEffectType.NONE)
                return null;

            return m_animationEffectDic[type];
        }

        void Awake()
        {
            instance = this;

            m_animationEffectDic = new Dictionary<AnimationEffectType, AnimationEffectData>();
            foreach(AnimationEffectData aData in m_animationEffectSet.effects)
            {
                m_animationEffectDic.Add(aData.m_type, aData);
            }

            m_animationEffectPool = new AnimationEffectPool(m_animationEffectPrefab, 300);
        }

        public void PlayAnimationEffect(AnimationEffectType type, Vector2 position)
        {
            if (type == AnimationEffectType.NONE) return;

            AnimationEffect aEffect = AnimationEffectManager.Inst.m_animationEffectPool.Borrow();
            if (aEffect == null) return;

            AnimationEffectData aData = AnimationEffectManager.Inst.GetAnimationEffectData(type);
            aEffect.Init(aData);
            aEffect.transform.position = position;
            aEffect.Play();
        }
    }
}
