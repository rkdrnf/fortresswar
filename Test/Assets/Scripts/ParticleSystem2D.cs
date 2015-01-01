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

        InvokeRepeating("CreateParticle", 0f, 2);
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

	
    /*
	// Update is called once per frame
	void Update () {
        return;

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
            GameObject particleObj = new GameObject();
            Rigidbody2D pBody = particleObj.AddComponent<Rigidbody2D>();
            float randomX = Random.value - 0.5f;
            float randomY = Random.value - 0.5f;
            Vector3 randomDirection = new Vector3(randomX, randomY, 0);
            
            pBody.velocity = (randomDirection * Random.Range(minVelocity, maxVelocity)) + (Vector3)gravityModifier;
            pBody.gravityScale = gravityScale;
            pBody.fixedAngle = true;

            if (collide)
            {
                BoxCollider2D pColl = particleObj.AddComponent<BoxCollider2D>();
                pColl.size = sprite.bounds.size;
                pColl.sharedMaterial = collidingMaterial;
            }

            SpriteRenderer pRenderer = particleObj.AddComponent<SpriteRenderer>();

            pRenderer.sprite = sprite;
            pRenderer.material = material;
            pRenderer.color = colors[(int)(Random.value * colors.Length)];

            particleObj.transform.localScale = Vector3.one * (size / sprite.bounds.size.x);
            particleObj.layer = particleLayer;
            //particleObj.transform.parent = this.transform;

            particleObj.transform.position = this.transform.position + (Vector3)FindBorder(randomX, randomY);

            Destroy(particleObj, lifeTime);
        }
	}
     * */

    
}
