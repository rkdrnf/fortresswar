using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const.Effect;
using Const.Structure;
using Const;

namespace Data
{
    [Serializable]
    public class StructureData : ScriptableObject
    {
        public ObjectType objectType;
        public int maxHealth;
        public bool destroyable;
        public bool collidable;
        public Vector2 size;
        public int spriteRowIndex;
        public SpriteInfo[] sprites;
        public ParticleType particleType;
        public AnimationEffectType destructionAnimation;
    }
}
