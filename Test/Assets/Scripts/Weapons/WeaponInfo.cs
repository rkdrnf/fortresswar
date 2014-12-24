using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Const;
using UnityEngine;

public class WeaponInfo : ScriptableObject
{
    public AmmoType ammoType;
    public int ammo;
    public int MaxAmmo;
    public double FireRate;
    public double fireTimer;
    public WeaponType weaponType;
    public GameObject weaponPrefab;
}