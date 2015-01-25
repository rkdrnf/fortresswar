using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const;

namespace Particle
{ 
    public class ParticleManager : MonoBehaviour
    {
        private static ParticleManager instance;

        public static ParticleManager Inst
        {
            get
            {
                if (instance == null) throw new System.Exception("Particle Manager not instantiated");
                return instance;
            }
        }
        
        public ParticlePool particlePool;
        public ParticleSystem2DPool particleSystemPool;

        public ParticleSystem2DSet particleSet;

        public LayerMask particleCollidingLayer;

    
        void Awake()
        {
            instance = this;

            int particleLayer = LayerMask.NameToLayer("Particle");
            Physics2D.IgnoreLayerCollision(particleLayer, LayerMask.NameToLayer("Default"));
            Physics2D.IgnoreLayerCollision(particleLayer, particleLayer);
        }
    }

    

    
}
