using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const.Effect;
using Const.Structure;

namespace Data
{
    [Serializable]
    public class StructureData : ScriptableObject
    {
        public int maxHealth;
        public bool destroyable;
        public Vector2 size;
        public int spriteRowIndex;
        public SpriteInfo[] sprites;
        public ParticleType particleType;
        public AnimationEffectType destructionAnimation;
    }
}
