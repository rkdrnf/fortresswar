using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ParticleSystem2DData : ScriptableObject
{
    public Sprite sprite;
    public Bounds bounds;
    public Material material;
    public Color[] colors;

    public float size;
    public bool collide;
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
}
