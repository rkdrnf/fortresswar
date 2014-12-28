using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class FireInfo
{
    public FireInfo(int owner, Vector2 origin, Vector2 direction)
    {
        this.owner = owner;
        this.origin = origin;
        this.direction = direction;
    }

    public Vector2 origin;
    public Vector2 direction;
    public int owner;
}
