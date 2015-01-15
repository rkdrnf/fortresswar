using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Data;

namespace Effect
{
    [RequireComponent(typeof(Animation))]
    public class AnimationEffect : MonoBehaviour
    {
        public float m_animTimer;
        public Vector2 offset;
        public bool m_isPlaying;

        Animation m_animation;
        AnimationClip m_clip;

        void Awake()
        {
            gameObject.SetActive(false);
            m_animation = GetComponent<Animation>();
        }

        public void Init(AnimationEffectData aData)
        {
            m_clip = aData.m_clip;
            m_animation.clip = m_clip;
            m_animation.AddClip(m_clip, "Default");
            m_animTimer = aData.m_duration;
            m_isPlaying = false;

            transform.localPosition += (Vector3)aData.m_offset;
            transform.rotation = Quaternion.identity;
            
        }


        public void Play()
        {
            m_isPlaying = true;
            m_animation.Play();
        }

        void Update()
        {
            if (!m_isPlaying) return;

            if (m_animTimer < 0)
                Stop();

            m_animTimer -= Time.deltaTime;
        }

        void Stop()
        {
            m_isPlaying = false;
            m_animation.RemoveClip(m_clip);
            Disable();
        }

        void Disable()
        {
            gameObject.SetActive(false);
        }
    }
}
