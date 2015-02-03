using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const.Effect;

namespace Data
{
    public class LightEffectData : ScriptableObject
    {
        public LightEffectType m_type;
        public float m_maxRange;
        public float m_minRange;
        public float m_intensity;
        public float m_duration;
    }
}
