using UnityEngine;
using System.Collections;
using Util;

namespace Particle
{ 
    public class ParticleSystem2D : MonoBehaviour
    {

        public LayerMask m_collidingMask;
        ParticleSystem2DData m_pSystemData;

        double m_durationTimer;
        double m_spawnTimer;
        int m_spawningCount;
        int m_totalSpawningCount;
        int m_amountMultiplier = 1;

        float m_inclination;

        bool m_isPlaying;

        // Use this for initialization
        void Awake()
        {

        }


        public void Init(ParticleSystem2DData setting)
        {
            m_spawnTimer = 0;
            m_spawningCount = 0;
            m_totalSpawningCount = 0;
            m_isPlaying = false;

            m_pSystemData = setting;

            if (m_pSystemData.bounds.size.x != 0)
            {
                m_inclination = m_pSystemData.bounds.size.y / m_pSystemData.bounds.size.x;
            }

            if (m_pSystemData.playAutomatically)
            {
                Play();
            }
        }

        public void ChangeAmount(int amount)
        {
            m_amountMultiplier = amount;
        }

        public void Play()
        {
            m_isPlaying = true;
            m_spawnTimer = 0;
            m_spawningCount = 0;
            m_totalSpawningCount = 0;
            m_durationTimer = m_pSystemData.duration;
        }

        public void Stop()
        {
            m_isPlaying = false;
            Disable();
        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_isPlaying) return;

            if (m_durationTimer < 0)
            {
                if (m_pSystemData.loop)
                {
                    m_durationTimer = m_pSystemData.duration;
                    m_totalSpawningCount = 0;
                }
                else
                {
                    Stop();
                    return;
                }
            }

            if (m_totalSpawningCount > m_pSystemData.maxCount) { Stop(); return; }

            m_durationTimer -= Time.deltaTime;

            m_spawnTimer += Time.deltaTime * m_amountMultiplier;
            m_spawningCount = (int)(m_spawnTimer / m_pSystemData.rate);

            m_totalSpawningCount += m_spawningCount;

            if (m_spawnTimer > m_pSystemData.rate)
            {
                m_spawnTimer -= m_spawningCount * m_pSystemData.rate;
            }

            for (int i = 0; i < m_spawningCount; i++)
            {
                Particle2DData randomParticle = MakeRandomParticle();
                if (randomParticle == null)
                    continue;

                Particle2D particleObj = ParticleManager.Inst.particlePool.Borrow();
                if (particleObj == null) return;

                particleObj.Init(randomParticle);
            }
        }

        Particle2DData MakeRandomParticle()
        {
            Particle2DData p = new Particle2DData();
            p.lifeTime = m_pSystemData.lifeTime;

            float randomX = Random.value - 0.5f;
            float randomY = Random.value - 0.5f;

            Vector3 randomDirection = new Vector3(randomX, randomY, 0);

            if (m_pSystemData.bounds.size.x == 0)
            {
                p.position = transform.position;
            }
            else
            {
                p.position = transform.position + (Vector3)FindBorder(randomX, randomY);
            }

            if (LayerUtil.IsLayerExists(p.position, ParticleManager.Inst.particleCollidingLayer))
                return null;
            /*
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, randomDirection, 1.2f, collidingMask);

            if (hits.Length > 1)
            {
                return null;
            }
            */
            p.velocity = (randomDirection * Random.Range(m_pSystemData.minVelocity, m_pSystemData.maxVelocity)) + (Vector3)(m_pSystemData.gravityModifier);
            p.gravityScale = m_pSystemData.gravityScale;

            p.collide = m_pSystemData.collide;
            p.sprite = m_pSystemData.sprite;
            p.collidingMaterial = m_pSystemData.collidingMaterial;

            p.material = m_pSystemData.material;
            p.color = m_pSystemData.colors[(int)(Random.value * m_pSystemData.colors.Length)];

            p.scale = Vector3.one * (m_pSystemData.size / m_pSystemData.sprite.bounds.size.x);

            return p;
        }

        Vector2 FindBorder(float x, float y)
        {
            Bounds bounds = m_pSystemData.bounds;
            float particleInc = y / x;
            if (Mathf.Abs(m_inclination) >= Mathf.Abs(particleInc))
            {
                x = (x >= 0) ? (bounds.size.x + m_pSystemData.size) / 2.0f : -(bounds.size.x + m_pSystemData.size) / 2.0f;
                return new Vector2(x, x * particleInc);
            }
            else
            {
                y = (y >= 0) ? (bounds.size.y + m_pSystemData.size) / 2.0f : -(bounds.size.y + m_pSystemData.size) / 2.0f;
                return new Vector2(y / particleInc, y);
            }
        }
    }

}
