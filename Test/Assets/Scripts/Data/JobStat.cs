using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const;

public class JobStat : ScriptableObject
{
    public int MaxHealth;
    public float MovingSpeed;
    public float JumpingSpeed;
    public bool CanWallWalk;
    public bool CanWallJump;
    public float WallWalkingSpeed;
    public Vector2 WallJumpingSpeed;
    public float WallWalkingTime;

    public WeaponType[] Weapons;
    public RuntimeAnimatorController BlueTeamAnimations;
    public RuntimeAnimatorController RedTeamAnimations;
}
