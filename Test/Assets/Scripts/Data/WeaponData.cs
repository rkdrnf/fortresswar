using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Const;
using UnityEngine;

public class WeaponData : ScriptableObject
{
    public float power;
    public AmmoType ammoType;
    public int maxAmmo;
    public FireType fireType;
    public double fireRate;
    public bool isCharging;
    public double maxChargeTime;

    public WeaponType weaponType;
    public GameObject weaponPrefab;

    
}