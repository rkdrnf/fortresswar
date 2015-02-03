using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Data;

namespace Effect
{
    public class LightEffect : MonoBehaviour
    {
        public float m_duration;
        public bool m_isPlaying;

        Light m_light;

        LightEffectData m_data;

        float m_intensity;
        float m_range;

        void Awake()
        {
            m_light = GetComponent<Light>();
        }

        public void Init(LightEffectData lData)
        {
            m_data = lData;
        }

        public void Play()
        {
            m_isPlaying = true;
            m_intensity = m_data.m_intensity;
            m_duration = m_data.m_duration;
            m_range = m_data.m_maxRange;
        }

        void Update()
        {
            if (!m_isPlaying) return;

            if (m_duration < 0)
                Stop();

            m_light.intensity = m_intensity;
            m_light.range = m_range;
            
            m_duration -= Time.deltaTime;
            m_range -= (m_data.m_maxRange - m_data.m_minRange) * (Time.deltaTime / m_data.m_duration);
            m_intensity = m_duration / m_data.m_duration * m_data.m_intensity;

        }

        public void Stop()
        {
            m_isPlaying = false;
            Disable();
        }

        void Disable()
        {
            gameObject.SetActive(false);
        }

    }
}
