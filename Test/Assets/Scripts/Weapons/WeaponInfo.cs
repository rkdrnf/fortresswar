using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Const;
using UnityEngine;

public class WeaponInfo
{

    public WeaponInfo(WeaponData data)
    {
        this.power = data.power;
        this.ammoType = data.ammoType;
        this.ammo = data.maxAmmo;
        this.maxAmmo = data.maxAmmo;
        this.fireType = data.fireType;
        this.fireRate = data.fireRate;
        this.fireTimer = 0f;
        this.isCharging = false;
        this.maxChargeTime = data.maxChargeTime;
        this.chargeTimer = 0f;
        this.weaponType = data.weaponType;
        this.weaponPrefab = data.weaponPrefab;
    }

    public float power;
    public AmmoType ammoType;
    public int ammo;
    public int maxAmmo;
    public FireType fireType;
    public double fireRate;
    public double fireTimer;
    public bool isCharging;
    public double maxChargeTime;
    public double chargeTimer;

    public WeaponType weaponType;
    public GameObject weaponPrefab;

    public void Fire(FireInfo info)
    {
        GameObject projObj = (GameObject)Network.Instantiate(weaponPrefab, info.origin, Quaternion.identity, 2);
        projObj.GetComponent<Projectile>().Init(this, info);

        fireTimer = fireRate;
        ammo--;
    }
}
