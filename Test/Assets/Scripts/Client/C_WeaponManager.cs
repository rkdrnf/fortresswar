using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using C2S = Packet.C2S;
using Const;
using Server;
using UnityEngine;

namespace Client
{
    public class C_WeaponManager : MonoBehaviour
    {
        PlayerBehaviour player;
        Dictionary<WeaponType, WeaponInfo> weapons;

        WeaponInfo currentWeapon;

        double weaponLastSet = 0f;

        bool isCharging = false;

        object weaponLock = new object();

        WeaponManager s_weaponManager;

        public void Init(PlayerBehaviour owner)
        {
            s_weaponManager = GetComponent<WeaponManager>();

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

            ReloadAll();

            currentWeapon = weapons.First().Value;
        }

        public void Fire()
        {
            //if (!CanFire(currentWeapon)) return;

            switch(currentWeapon.fireType)
            {
                case FireType.INSTANT:
                    Fire(currentWeapon.weaponType);
                    break;

                case FireType.CHARGE:
                    Charge(currentWeapon.weaponType);
                    break;
            }
            
        }

        public void FireCharged()
        {
            lock(weaponLock)
            { 
                if (isCharging && currentWeapon.fireType == FireType.CHARGE)
                {
                    isCharging = false;
                    Fire(currentWeapon.weaponType);
                }
            }
        }

        public void Charge(WeaponType weaponType)
        {
            if (player.IsDead()) return;

            lock (weaponLock)
            { 
                if (isCharging) return;

                isCharging = true;

                C2S.ChargeWeapon charge = new C2S.ChargeWeapon(player.GetOwner(), weaponType);

                if (Network.isServer)
                {
                    s_weaponManager.ServerCharge(charge.SerializeToBytes(), new NetworkMessageInfo());
                }
                else
                {
                    player.networkView.RPC("ServerCharge", RPCMode.Server, charge.SerializeToBytes());
                }
            }
        }

        public void Fire(WeaponType weaponType)
        {
            if (player.IsDead()) return;

            Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
            Vector2 direction = (worldMousePosition - player.transform.position);
            direction.Normalize();

            C2S.Fire fire = new C2S.Fire(player.GetOwner(), -1, weaponType, direction);

            Debug.Log(string.Format("Player {0} pressed Fire", Network.player));

            if (Network.isServer)
            {
                s_weaponManager.ServerFire(fire.SerializeToBytes(), new NetworkMessageInfo());
            }
            else
            {
                networkView.RPC("ServerFire", RPCMode.Server, fire.SerializeToBytes());
            }
        }

        public void EndCharge()
        {
            lock (weaponLock)
            {
                isCharging = false;
            }
        }

        public WeaponInfo ChangeWeapon(KeyCode code)
        {
            EndCharge();

            int index;

            switch (code)
            {
                case KeyCode.Alpha1:
                    index = 0;
                    break;

                case KeyCode.Alpha2:
                    index = 1;
                    break;

                case KeyCode.Alpha3:
                    index = 2;
                    break;

                case KeyCode.Alpha4:
                    index = 3;
                    break;

                default:
                    index = -1;
                    break;
            }

            try
            {
                currentWeapon = weapons[player.jobStat.Weapons.ElementAt(index)];
            }
            catch(ArgumentOutOfRangeException e) // No weapon at index
            {
                return currentWeapon;
            }

            return currentWeapon;
        }

        public void UseSkill(KeyCode code)
        {
            Fire(WeaponType.ROPE);
        }

        public WeaponInfo ChangeWeapon(WeaponType type, double time)
        {
            if (weaponLastSet > time)
                return currentWeapon;

            weaponLastSet = time;
            currentWeapon = weapons[type];
            
            return currentWeapon;
        }

        public void RefreshFireRate(double deltaTime)
        {
            foreach (WeaponInfo weapon in weapons.Values)
            {
                weapon.fireTimer -= deltaTime;
            }
        }

        public void ReloadAll()
        {
            foreach (WeaponInfo weapon in weapons.Values)
            {
                weapon.ammo = weapon.maxAmmo;
            }
        }

        bool CanFire(WeaponInfo weapon)
        {
            if (player.IsDead())
                return false;

            return weapon.fireTimer < 0 && (weapon.ammo > 0 || weapon.ammoType == AmmoType.INFINITE);
        }
    }
}


