using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const;
using Const.Effect;
using Data;

namespace Character
{
    public class CharacterRenderer
    {
        Server.ServerPlayer m_player;

        SpriteRenderer m_characterRenderer;
        Animator m_animator;

        public double m_highlightTime = 0.3f;
        double m_highlightTimer;

        public MaterialSet effectMaterials;
        public Material normalMaterial;

        public CharacterRenderer(Server.ServerPlayer player)
        {
            m_player = player;
            m_characterRenderer = m_player.GetComponent<SpriteRenderer>();
            m_animator = m_player.GetComponent<Animator>();

            normalMaterial = (Material)Resources.Load("Materials/Player/Normal", typeof(Material));
            effectMaterials = (MaterialSet)Resources.Load("Materials/Player/effects", typeof(MaterialSet));
        }

        public void Render()
        {
            if (m_highlightTimer > 0)
                m_highlightTimer -= Time.deltaTime;
            if (m_highlightTimer <= 0)
            {
                ChangeMaterial(normalMaterial);
            }

            m_animator.SetBool("HorMoving", m_player.GetInputX() != 0);
            m_animator.SetBool("Dead", m_player.IsInState(CharacterState.DEAD));
        }

        void ChangeMaterial(Material material)
        {
            m_characterRenderer.material = material;
        }

        public void LoadAnimation(Team team)
        {
            JobStat jobStat = m_player.GetJobStat();

            if (jobStat == null) return;

            m_animator.runtimeAnimatorController = team == Team.BLUE ?
                jobStat.BlueTeamAnimations :
                jobStat.RedTeamAnimations;
        }

        public void Highlight(CharacterHighlight highlight)
        {
            if (highlight == Const.Effect.CharacterHighlight.DAMAGE)
            { 
                ChangeMaterial(effectMaterials.materials[(int)highlight]);
                m_highlightTimer = m_highlightTime;
            }
        }
    }
}
