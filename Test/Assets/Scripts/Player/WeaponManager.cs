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
        Dictionary<int, WeaponType> indexTypeDic;

        WeaponInfo currentWeapon;

        bool isCharging = false;

        object weaponLock = new object();

        public void Init(ServerPlayer owner)
        {
            player = owner;
            weapons = new Dictionary<WeaponType, WeaponInfo>();
            indexTypeDic = new Dictionary<int, WeaponType>();
        }

        public void LoadWeapons(IEnumerable<WeaponType> weaponSet)
        {
            weapons.Clear();
            indexTypeDic.Clear();

            int i = 0;
            foreach (WeaponType weaponType in weaponSet)
            {
                WeaponInfo weapon = new WeaponInfo(Game.Inst.weaponSet.weapons[(int)weaponType]);

                weapons.Add(weapon.weaponType, weapon);
                indexTypeDic.Add(i, weaponType);
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

        public WeaponInfo[] GetWeapons()
        {
            return weapons.Values.ToArray();
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

            if (indexTypeDic.ContainsKey(index))
                currentWeapon = weapons[indexTypeDic[index]];

            return currentWeapon;
        }

        public WeaponInfo GetCurrentWeapon()
        {
            return currentWeapon;
        }

        public void EndCharge()
        {
            lock (weaponLock)
            {
                isCharging = false;
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

        public void TryFireCharged()
        {
            lock (weaponLock)
            {
                if (isCharging && currentWeapon.fireType == FireType.CHARGE)
                {
                    isCharging = false;
                    TryFire(currentWeapon.weaponType);
                }
            }
        }

        public void TryFire()
        {
            //if (!CanFire(currentWeapon)) return;

            switch (currentWeapon.fireType)
            {
                case FireType.INSTANT:
                    TryFire(currentWeapon.weaponType);
                    break;

                case FireType.CHARGE:
                    TryCharge(currentWeapon.weaponType);
                    break;
            }

        }

        public void TryUseSkill(KeyCode code)
        {
            TryFire(WeaponType.ROPE);
        }

        public void TryFire(WeaponType weaponType)
        {
            if (player.IsDead()) return;

            Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
            Vector2 direction = (worldMousePosition - player.transform.position);
            direction.Normalize();

            C2S.Fire fire = new C2S.Fire(player.GetOwner(), -1, weaponType, direction);

            if (Network.isServer)
            {
                Fire(fire.SerializeToBytes(), new NetworkMessageInfo());
            }
            else
            {
                networkView.RPC("Fire", RPCMode.Server, fire.SerializeToBytes());
            }
        }

        public void TryCharge(WeaponType weaponType)
        {
            if (player.IsDead()) return;

            lock (weaponLock)
            {
                if (isCharging) return;

                isCharging = true;

                C2S.ChargeWeapon charge = new C2S.ChargeWeapon(player.GetOwner(), weaponType);

                if (Network.isServer)
                {
                    ServerCharge(charge.SerializeToBytes(), new NetworkMessageInfo());
                }
                else
                {
                    player.networkView.RPC("ServerCharge", RPCMode.Server, charge.SerializeToBytes());
                }
            }
        }

        [RPC]
        public void Fire(byte[] fireData, NetworkMessageInfo msgInfo)
        {
            if (!Network.isServer) return;
            if (!PlayerManager.Inst.IsValidPlayer(player.GetOwner(), msgInfo.sender)) return;

            C2S.Fire fire = C2S.Fire.DeserializeFromBytes(fireData);

            if (!CanFire(fire.weaponType)) return;

            lock (weaponLock)
            {
                ChangeWeapon(fire.weaponType);

                WeaponInfo weapon = weapons[fire.weaponType];

                FireInfo info = new FireInfo(player.GetOwner(), player.transform.position, fire.direction);

                weapon.Fire(info);
            }
        }

        [RPC]
        public void ServerCharge(byte[] pckData, NetworkMessageInfo info)
        {
            if (!Network.isServer) return;
            if (!PlayerManager.Inst.IsValidPlayer(player.GetOwner(), info.sender)) return;

            C2S.ChargeWeapon charge = C2S.ChargeWeapon.DeserializeFromBytes(pckData);

            Charge(charge);
        }

        public WeaponInfo TryChangeWeapon(WeaponType type, double time)
        {
            foreach (WeaponInfo weapon in weapons.Values)
            {
                if (weapon.weaponType == type)
                {
                    currentWeapon = weapon;
                    break;
                }
            }

            return currentWeapon;
        }

        public WeaponInfo ChangeWeapon(WeaponType type)
        {
            currentWeapon.isCharging = false;

            currentWeapon = weapons[type];

            return currentWeapon;
        }

        void RefreshFireRate(double deltaTime)
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

        public WeaponType GetCurrentWeaponType()
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


