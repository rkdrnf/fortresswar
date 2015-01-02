using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const;

namespace Client
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
        
        [HideInInspector]
        public ParticlePool particlePool;
        [HideInInspector]
        public ParticleSystem2DPool particleSystemPool;

        public GameObject particlePrefab;
        public GameObject particleSystemPrefab;

        public ParticleSystem2DSet particleSet;

    
        void Awake()
        {
            instance = this;

            particlePool = new ParticlePool(particlePrefab, 800);
            particleSystemPool = new ParticleSystem2DPool(particleSystemPrefab, 200);
    
            int particleLayer = LayerMask.NameToLayer("Particle");
            Physics2D.IgnoreLayerCollision(particleLayer, LayerMask.NameToLayer("Default"));
            Physics2D.IgnoreLayerCollision(particleLayer, particleLayer);
        }
    }
}
