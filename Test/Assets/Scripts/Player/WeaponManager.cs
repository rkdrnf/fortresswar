using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using C2S = Packet.C2S;
using Const;
using Server;
using UnityEngine;

namespace Server
{
public class WeaponManager : MonoBehaviour
{
    ServerPlayer player;
    Dictionary<WeaponType, WeaponInfo> weapons;

    WeaponInfo currentWeapon;

    object weaponLock = new object();

    Client.C_WeaponManager c_weaponManager;

    public void Init(ServerPlayer owner)
    {
        c_weaponManager = GetComponent<Client.C_WeaponManager>();
        player = owner;
        weapons = new Dictionary<WeaponType, WeaponInfo>();    
    }

    public void LoadWeapons(IEnumerable<WeaponType> weaponSet)
    {
        weapons.Clear();

        int i = 0;
        foreach (WeaponType weaponType in weaponSet)
        {
            WeaponInfo weapon = new WeaponInfo(Game.Inst.weaponSet.weapons[(int)weaponType]);

            weapons.Add(weapon.weaponType, weapon);
            i++;
        }

        currentWeapon = weapons.First().Value;

        ReloadAll();
    }

    void FixedUpdate()
    {
        RefreshFireRate(Time.fixedDeltaTime);

        if (currentWeapon != null && currentWeapon.fireType == FireType.CHARGE && currentWeapon.isCharging)
        {
            currentWeapon.chargeTimer += Time.fixedDeltaTime;
        }
    }


    public void Charge(C2S.ChargeWeapon charge)
    {
        if (!Network.isServer) return;
        if (!CanFire(charge.weaponType)) return;

        lock (weaponLock)
        {
            ChangeWeapon(charge.weaponType);

            WeaponInfo weapon = weapons[charge.weaponType];

            if (weapon.isCharging) return;

            weapon.isCharging = true;

            weapon.chargeTimer = 0;
        }
    }

    [RPC]
    public void ServerFire(byte[] fireData, NetworkMessageInfo info)
    {
        if (!Network.isServer) return;
        if (!PlayerManager.Inst.IsValidPlayer(player.GetOwner(), info.sender)) return;

        C2S.Fire fire = C2S.Fire.DeserializeFromBytes(fireData);

        Fire(fire);
    }

    [RPC]
    public void ServerCharge(byte[] pckData, NetworkMessageInfo info)
    {
        if (!Network.isServer) return;
        if (!PlayerManager.Inst.IsValidPlayer(player.GetOwner(), info.sender)) return;

        C2S.ChargeWeapon charge = C2S.ChargeWeapon.DeserializeFromBytes(pckData);

        Charge(charge);
    }

    public void Fire(C2S.Fire fire)
    {
        if (!Network.isServer) return;
        if (!CanFire(fire.weaponType)) return;

        lock(weaponLock)
        {
            ChangeWeapon(fire.weaponType);

            WeaponInfo weapon = weapons[fire.weaponType];

            FireInfo info = new FireInfo(player.GetOwner(), player.transform.position, fire.direction);

            weapon.Fire(info);
        }
    }

    public WeaponInfo ChangeWeapon(WeaponType type)
    {
        currentWeapon.isCharging = false;

        currentWeapon = weapons[type];

        return currentWeapon;
    }

    void RefreshFireRate(double deltaTime)
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
            weapon.ammo = weapon.maxAmmo;
        }
    }

    public WeaponType GetCurrentWeapon()
    {
        return currentWeapon.weaponType;
    }

    bool CanFire(WeaponType weaponType)
    {
        if (player.IsDead()) return false;
        if (weapons.ContainsKey(weaponType) == false) return false;

        WeaponInfo weapon = weapons[weaponType];

        return weapon.fireTimer < 0 && (weapon.ammo > 0 || weapon.ammoType == AmmoType.INFINITE);
    }
}
    }


