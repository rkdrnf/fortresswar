using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Data;

namespace Effect
{
    public class AnimationEffect : MonoBehaviour
    {
        public float m_animTimer;
        public Vector2 offset;
        public bool m_isPlaying;

        Animator m_animator;
        AnimationClip m_clip;

        void Awake()
        {
            m_animator = GetComponent<Animator>();
        }

        public void Init(AnimationEffectData aData)
        {
            m_animator.runtimeAnimatorController = aData.m_animController;
            m_animTimer = aData.m_duration;
            m_isPlaying = false;

            transform.localPosition += (Vector3)aData.m_offset;
            transform.rotation = Quaternion.identity;
        }


        public void Play()
        {
            m_isPlaying = true;
        }

        void Update()
        {
            if (!m_isPlaying) return;

            if (m_animTimer < 0)
                Stop();

            m_animTimer -= Time.deltaTime;
        }

        public void Stop()
        {
            m_isPlaying = false;
            m_animator.StopPlayback();
            Disable();
        }

        void Disable()
        {
            gameObject.SetActive(false);
        }
    }
}
