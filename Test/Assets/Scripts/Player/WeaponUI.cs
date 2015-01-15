using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Server;
namespace UI
{ 
    public class WeaponUI : MonoBehaviour
    {
        WeaponManager weaponManager;

        const float weaponWidth = 100f;

        void Start()
        {
            weaponManager = transform.parent.gameObject.GetComponent<WeaponManager>();
        }

        void OnGUI()
        {
            WeaponInfo current = weaponManager.GetCurrentWeapon();

            if (current != null)
                DrawCurrentWeapon(current);


            DrawWeaponList(weaponManager.GetWeapons());
        }

        void DrawCurrentWeapon(WeaponInfo current)
        {

        }

        void DrawWeaponList(WeaponInfo[] weapons)
        {
            float totalWidth = (weapons.Count() * weaponWidth);
            GUILayout.BeginArea(new Rect(Screen.width / 2 - (totalWidth / 2), Screen.height - 100f, totalWidth, 100f));
            GUILayout.BeginHorizontal();
                foreach(WeaponInfo weapon in weapons)
                {
                    GUILayout.BeginVertical();

                    GUILayout.Box(Game.Inst.weaponSet.weapons[(int)weapon.weaponType].UIImage, GUILayout.Width(80f), GUILayout.Height(30f));
                    GUIStyle ammoStyle = new GUIStyle();
                    ammoStyle.normal.textColor = Color.white;
                    GUILayout.Box(weapon.ammo + " / " + weapon.maxAmmo, ammoStyle, GUILayout.Width(100f), GUILayout.Height(30f));
                    GUILayout.Box(weapon.fireRate.ToString(), ammoStyle, GUILayout.Width(100f), GUILayout.Height(30f));
                    var con = new GUIContent();



                    GUILayout.EndVertical();
                }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

        }


    }
}
