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
    public class C_WeaponManager
    {
        PlayerBehaviour player;
        Dictionary<WeaponType, WeaponInfo> weapons;

        WeaponInfo currentWeapon;

        double weaponLastSet = 0f;

        public C_WeaponManager(PlayerBehaviour owner)
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

            currentWeapon = weapons.First().Value;
        }

        public void Fire()
        {
            Fire(currentWeapon.weaponType);
        }

        public void Fire(WeaponType weaponType)
        {
            if (player.IsDead())
                return;

            Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
            Vector2 direction = (worldMousePosition - player.transform.position);
            direction.Normalize();

            C2S.Fire fire = new C2S.Fire(player.GetOwner(), -1, weaponType, Vector3.zero, direction);

            Debug.Log(string.Format("Player {0} pressed Fire", Network.player));

            if (Network.isServer)
            {
                player.serverPlayer.ServerFire(fire.SerializeToBytes(), new NetworkMessageInfo());
            }
            else
            {
                player.networkView.RPC("ServerFire", RPCMode.Server, fire.SerializeToBytes());
            }
        }

        public WeaponInfo ChangeWeapon(KeyCode code)
        {
            WeaponType type;

            switch (code)
            {
                case KeyCode.Alpha1:
                    type = (WeaponType)Enum.Parse(typeof(WeaponType), "0");
                    break;

                case KeyCode.Alpha2:
                    type = (WeaponType)Enum.Parse(typeof(WeaponType), "1");
                    break;

                case KeyCode.Alpha3:
                    type = (WeaponType)Enum.Parse(typeof(WeaponType), "2");
                    break;

                case KeyCode.Alpha4:
                    type = (WeaponType)Enum.Parse(typeof(WeaponType), "3");
                    break;

                default:
                    type = WeaponType.MAX;
                    break;
            }

            if (weapons.ContainsKey(type))
                currentWeapon = weapons[type];

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
}


