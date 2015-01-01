using UnityEngine;
using System.Collections;

public class ParticleSystem2D : MonoBehaviour {

    public Sprite sprite;
    public Bounds bounds;
    public Material material;
    public Color[] colors;
    

    public float size;
    
    public bool collide;
    public LayerMask collidingMask;
    public PhysicsMaterial2D collidingMaterial;

    public double duration;
    public float rate;
    public bool loop;

    public float lifeTime;
    public int maxCount;

    public float minVelocity;
    public float maxVelocity;

    public int gravityScale;
    public Vector2 gravityModifier;

    public bool playAutomatically;


    double durationTimer;
    double spawnTimer;
    int spawningCount;
    int totalSpawningCount;

    

    bool isPlaying;

	// Use this for initialization
	void Start () {
        spawnTimer = 0;
        spawningCount = 0;
        totalSpawningCount = 0;
        isPlaying = false;
        

        if (playAutomatically)
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
        durationTimer = duration;

        //InvokeRepeating("CreateParticle", 0f, 2);
    }

    public void Stop()
    {
        isPlaying = false;
        CancelInvoke("CreateParticle");
    }

    void CreateParticle()
    {
        if (!isPlaying)
        {
            Stop();
            return;
        }

        if (durationTimer < 0)
        {
            if (loop)
            {
                durationTimer = duration;
                totalSpawningCount = 0;
            }
            else
            {
                Stop();
                return;
            }
        }

        if (totalSpawningCount > maxCount) { Stop(); return; }

        durationTimer -= rate;

        totalSpawningCount += 1;

        Particle2D particleObj = Client.ClientGame.Inst.particlePool.Borrow();
        particleObj.Init(minVelocity, maxVelocity, gravityModifier, gravityScale,
            collide, sprite, collidingMaterial, material, colors,
            size, bounds, lifeTime, transform.position);
    }


    // Update is called once per frame
    void Update () {
        if (!isPlaying) return;

        if (durationTimer < 0)
        {
            if (loop)
            { 
                durationTimer = duration;
                totalSpawningCount = 0;
            }
            else
            {
                isPlaying = false;
                return;
            }
        }

        if (totalSpawningCount > maxCount) return;

        durationTimer -= Time.deltaTime;
            
        spawnTimer += Time.deltaTime;
        spawningCount = (int)(spawnTimer / rate);

        totalSpawningCount += spawningCount;

        if (spawnTimer > rate)
        {
            spawnTimer -= spawningCount * rate;
        }

        for(int i = 0; i < spawningCount; i++)
        {
            Particle2D particleObj = Client.ClientGame.Inst.particlePool.Borrow();
        particleObj.Init(minVelocity, maxVelocity, gravityModifier, gravityScale,
            collide, sprite, collidingMaterial, material, colors,
            size, bounds, lifeTime, transform.position);
        }
    }


}
