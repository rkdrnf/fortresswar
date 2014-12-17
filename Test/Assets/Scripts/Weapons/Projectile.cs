﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    public long ID;
    public int owner;

    public int damage;
    public int range;

    protected Vector3 startPosition;
    public GameObject explosionAnimation;

    void Awake()
    {
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Draw in Client, Collide in Server
        //Client

        Quaternion rot = Quaternion.FromToRotation(Vector3.right, new Vector3(rigidbody2D.velocity.x, rigidbody2D.velocity.y));
        transform.rotation = rot;

        if ((transform.position - startPosition).sqrMagnitude > range * range)
        {
            DestroyFromNetwork();
        }
    }

    void OnTriggerEnter2D(Collider2D targetCollider)
    {
        CollisionFunc(targetCollider);
    }

    protected abstract void CollisionFunc(Collider2D targetCollider);

    void OnApplicationQuit()
    {
        DestroyImmediate(gameObject);
    }

    public void DestroyFromNetwork()
    {
        if (Network.isServer)
        {
            ProjectileManager.Inst.DestroyProjectile(ID);
        }
    }
    void OnDestroy()
    {
        GameObject explosion = (GameObject)Instantiate(explosionAnimation, transform.position, transform.rotation);
        Destroy(explosion, 0.4f);
    }
}
