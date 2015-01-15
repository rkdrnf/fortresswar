using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const;

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

        public Particle2D particlePrefab;
        public ParticleSystem2D particleSystemPrefab;

        public ParticleSystem2DSet particleSet;

        public LayerMask particleCollidingLayer;

    
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
