using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using S2C = Communication.S2C;
using C2S = Communication.C2S;


namespace Architecture
{ 
[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(Collider2D))]
public class StoneWall : Building
{
}
}