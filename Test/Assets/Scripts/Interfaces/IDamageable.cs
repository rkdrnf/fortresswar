using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public interface IDamageable
{
    bool Damage(int damage, Vector2 point);

}
