using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Const;
using UnityEngine;

namespace Architecture
{
    [RequireComponent(typeof(Collider2D), typeof(SpriteRenderer), typeof(Rigidbody2D))]
    public class FallingBuilding : MonoBehaviour
    {
        Building m_building;
        public Texture2D m_buildingTexture = null; // scene init
        
        public void Init (Building building)
        {
            //Debug.Log("Init Frame : " + Time.frameCount);
            m_building = building;
            transform.position = building.m_coord.ToVector2();
            int tTop = m_building.m_data.spriteRowIndex * 8;
            int tLeft = m_building.m_spriteIndex * 8;
            float tWidth = m_building.m_data.size.x * 8;
            float tHeight = m_building.m_data.size.y * 8;

            GetComponent<SpriteRenderer>().sprite = Sprite.Create(m_buildingTexture, new Rect(tLeft, tTop, tWidth, tHeight), new Vector2(0.5f, 0.5f), 8);
            GetComponent<BoxCollider2D>().size = m_building.m_data.size;
        }

        void OnCollisionEnter2D(Collision2D coll)
        {
            //Debug.Log("falling collision Frame: " + Time.frameCount);
            DestroyBuilding();
        }

        void DestroyBuilding()
        {
            BuildingManager.Inst.RemoveID(m_building);

            m_building.PlayDestructionAnimation(transform.position);
            int particleAmount = 3;
            m_building.PlaySplash(particleAmount, transform.position);

            Disable();
        }

        void Disable()
        {
            gameObject.SetActive(false);
        }
    }
}
