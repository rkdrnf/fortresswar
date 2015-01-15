using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const.Effect;

namespace Data
{
    public class AnimationEffectData : ScriptableObject
    {
        public AnimationEffectType m_type;
        public AnimationClip m_clip;
        public float m_duration;
        public Quaternion m_quaternion;
        public Vector2 m_offset;
    }
}
