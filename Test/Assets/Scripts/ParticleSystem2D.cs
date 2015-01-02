using UnityEngine;
using System.Collections;

public class ParticleSystem2D : MonoBehaviour {

    public LayerMask collidingMask;
    ParticleSystem2DData pSystemData;

    double durationTimer;
    double spawnTimer;
    int spawningCount;
    int totalSpawningCount;

    float inclination;

    bool isPlaying;

    static readonly float PARTICLE_SIZE = 0.15f;

    // Use this for initialization
    void Awake()
    {
        
    }

	
	public void Init (ParticleSystem2DData setting) {
        spawnTimer = 0;
        spawningCount = 0;
        totalSpawningCount = 0;
        isPlaying = false;

        pSystemData = setting;

        if (pSystemData.bounds.size.x != 0)
        {
            inclination = pSystemData.bounds.size.y / pSystemData.bounds.size.x;
        }

        if (pSystemData.playAutomatically)
        {
            Play();
        }
	}

    public void Play()
    {
        isPlaying = true;
        spawnTimer = 0;
        spawningCount = 0;
        totalSpawningCount = 0;
        durationTimer = pSystemData.duration;
    }

    public void Stop()
    {
        isPlaying = false;
        Disable();
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update () {
        if (!isPlaying) return;

        if (durationTimer < 0)
        {
            if (pSystemData.loop)
            {
                durationTimer = pSystemData.duration;
                totalSpawningCount = 0;
            }
            else
            {
                Stop();
                return;
            }
        }

        if (totalSpawningCount > pSystemData.maxCount) { Stop(); return; }

        durationTimer -= Time.deltaTime;
            
        spawnTimer += Time.deltaTime;
        spawningCount = (int)(spawnTimer / pSystemData.rate);

        totalSpawningCount += spawningCount;

        if (spawnTimer > pSystemData.rate)
        {
            spawnTimer -= spawningCount * pSystemData.rate;
        }

        for(int i = 0; i < spawningCount; i++)
        {
            Particle2DData randomParticle = MakeRandomParticle();
            if (randomParticle == null)
                continue;

            Particle2D particleObj = Client.ParticleManager.Inst.particlePool.Borrow();
            particleObj.Init(randomParticle);
        }
    }

    Particle2DData MakeRandomParticle()
    {
        Particle2DData p = new Particle2DData();
        p.lifeTime = pSystemData.lifeTime;

        float randomX = Random.value - 0.5f;
        float randomY = Random.value - 0.5f;

        Vector3 randomDirection = new Vector3(randomX, randomY, 0);
        
        if (pSystemData.bounds.size.x == 0)
        {
            p.position = transform.position;
        }
        else
        {
            p.position = transform.position + (Vector3)FindBorder(randomX, randomY);
        }

        if (Map.GetTile(p.position.x, p.position.y))
            return null;
        /*
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, randomDirection, 1.2f, collidingMask);

        if (hits.Length > 1)
        {
            return null;
        }
        */
        p.velocity = (randomDirection * Random.Range(pSystemData.minVelocity, pSystemData.maxVelocity)) + (Vector3)(pSystemData.gravityModifier);
        p.gravityScale = pSystemData.gravityScale;

        p.collide = pSystemData.collide;
        p.sprite = pSystemData.sprite;
        p.collidingMaterial = pSystemData.collidingMaterial;

        p.material = pSystemData.material;
        p.color = pSystemData.colors[(int)(Random.value * pSystemData.colors.Length)];

        p.scale = Vector3.one * (pSystemData.size / pSystemData.sprite.bounds.size.x);

        return p;
    }

    Vector2 FindBorder(float x, float y)
    {
        Bounds bounds = pSystemData.bounds;
        float particleInc = y / x;
        if (Mathf.Abs(inclination) >= Mathf.Abs(particleInc))
        {
            x = (x >= 0) ? (bounds.size.x + PARTICLE_SIZE) / 2.0f : -(bounds.size.x + PARTICLE_SIZE) / 2.0f;
            return new Vector2(x, x * particleInc);
        }
        else
        {
            y = (y >= 0) ? (bounds.size.y + PARTICLE_SIZE) / 2.0f : -(bounds.size.y + PARTICLE_SIZE) / 2.0f;
            return new Vector2(y / particleInc, y);
        }
    }
}
