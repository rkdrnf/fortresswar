using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using C2S = Packet.C2S;
using Const;
using Server;
using UnityEngine;

public class WeaponManager
{
    ServerPlayer player;
    Dictionary<WeaponType, WeaponInfo> weapons;

    WeaponInfo currentWeapon;

    public WeaponManager(ServerPlayer owner)
    {
        player = owner;
        weapons = new Dictionary<WeaponType, WeaponInfo>();    
    }

    public void LoadWeapons(IEnumerable<WeaponType> weaponSet)
    {
        weapons.Clear();

        int i = 0;
        foreach (WeaponType weaponType in weaponSet)
        {
            WeaponInfo weapon = Game.Inst.weaponSet.weapons[(int)weaponType];

            weapons.Add(weapon.weaponType, weapon);
            i++;
        }

        ReloadAll();
    }


    public void Fire(C2S.Fire fire)
    {
        if (!Network.isServer) return;

        if (weapons.ContainsKey(fire.weaponType) == false) return;

        WeaponInfo weapon = weapons[fire.weaponType];

        if (CanFire(weapon) == false)
        {
            return;
        }

        weapon.fireTimer = weapon.FireRate;
        weapon.ammo--;

        fire.origin = player.transform.position;


        GameObject projObj = (GameObject)Network.Instantiate(Game.Inst.weaponSet.weapons[(int)fire.weaponType].weaponPrefab, fire.origin, Quaternion.identity, 2);
        projObj.GetComponent<Projectile>().Init(fire);

        //player.networkView.RPC("BroadcastFire", RPCMode.All, fire.SerializeToBytes());
    }

    public WeaponInfo ChangeWeapon(WeaponType type)
    {
        currentWeapon = weapons[type];

        return currentWeapon;
    }

    public void RefreshFireRate(double deltaTime)
    {
        foreach(WeaponInfo weapon in weapons.Values)
        {
            weapon.fireTimer -= deltaTime;
        }
    }

    public void ReloadAll()
    {
        foreach(WeaponInfo weapon in weapons.Values)
        {
            weapon.ammo = weapon.MaxAmmo;
        }
    }

    bool CanFire(WeaponInfo weapon)
    {
        if (player.IsDead())
            return false;

        return weapon.fireTimer < 0 && (weapon.ammo > 0 || weapon.ammoType == AmmoType.INFINITE);
    }
}


