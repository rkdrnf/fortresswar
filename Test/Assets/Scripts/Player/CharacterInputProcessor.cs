using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const;
using FocusManager;

namespace Character
{
    public class CharacterInputProcessor
    {
        float m_horMov;
        float m_verMov;

        Vector2 m_lookingDirection;

        Server.ServerPlayer m_player;

        Server.ServerGame game;

        KeyCode[] weaponCodes = new KeyCode[4] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };
        KeyCode[] skillCodes = new KeyCode[] { KeyCode.R };

        public CharacterInputProcessor(Server.ServerPlayer player)
        {
            m_player = player;
            m_horMov = 0;
            m_verMov = 0;
            game = Server.ServerGame.Inst;
        }

        public float GetInputX()
        {
            return m_horMov;
        }
        public float GetInputY()
        {
            return m_verMov;
        }
        public Vector2 GetLookingDirection()
        {
            return m_lookingDirection;
        }

        KeyFocusManager keyFocusManager
        {
            get { return Server.ServerGame.Inst.keyFocusManager; }
        }
        MouseFocusManager mouseFocusManager
        {
            get { return Server.ServerGame.Inst.mouseFocusManager; }
        }

        public void SyncInput(ref BitStream stream)
        {
            if (stream.isWriting)
            {
                //Debug.Log (string.Format("[WRITING] player: {0} sender: {1} time: {2}", owner, mnInfo.sender, mnInfo.timestamp));
                if (m_player.IsMine())
                {
                    float horMoveVal = m_horMov;
                    float verMoveVal = m_verMov;

                    Vector3 lookingDirectionVal = m_lookingDirection;

                    stream.Serialize(ref horMoveVal);
                    stream.Serialize(ref verMoveVal);
                    stream.Serialize(ref lookingDirectionVal);
                }
            }
            else if (stream.isReading)
            {
                //Debug.Log(string.Format("[READING] player: {0}  sender: {1} time: {2}", owner, mnInfo.sender, mnInfo.timestamp));
                float horMoveVal = 0;
                float verMoveVal = 0;
                Vector3 lookingDirectionVal = Vector2.right;

                stream.Serialize(ref horMoveVal);
                stream.Serialize(ref verMoveVal);
                stream.Serialize(ref lookingDirectionVal);

                m_horMov = horMoveVal;
                m_verMov = verMoveVal;
                m_lookingDirection = lookingDirectionVal;
            }
        }

        public void ProcessInput()
        {
            do
            {
                if (!m_player.IsMine()) break;
                if (m_player.IsDead()) break;

                if (keyFocusManager.IsFocused(InputKeyFocus.PLAYER))
                {
                    m_horMov = Input.GetAxisRaw("Horizontal");
                    m_verMov = Input.GetAxisRaw("Vertical");

                    if (Input.GetKey(KeyCode.Space))
                    {
                        m_player.TryJump();
                    }

                    //TeamSelector
                    if (Input.GetKey(KeyCode.M))
                    {
                        game.OpenInGameMenu(InGameMenuType.TEAM_SELECTOR);
                    }

                    //JobSelector
                    if (Input.GetKey(KeyCode.N))
                    {
                        game.OpenInGameMenu(InGameMenuType.JOB_SELECTOR);
                    }

                    if (Input.GetKey(KeyCode.F))
                    {
                        game.OpenInGameMenu(InGameMenuType.BUILD_MENU);
                    }

                    if (Input.GetKeyDown(KeyCode.Tab))
                    {
                        game.OpenInGameMenu(InGameMenuType.SCORE_BOARD);
                    }

                    foreach (KeyCode code in weaponCodes)
                    {
                        if (Input.GetKeyDown(code))
                        {
                            m_player.ChangeWeapon(code);
                            break;
                        }
                    }

                    foreach (KeyCode code in skillCodes)
                    {
                        if (Input.GetKeyDown(code))
                        {
                            m_player.TryFire(WeaponType.ROPE);
                        }
                        /*
                        if (Input.GetKeyDown(code))
                        {
                            skillManager.Cast(code);
                            break;
                        }
                         * */
                    }
                }
                else
                {
                    Input.ResetInputAxes();
                    m_horMov = 0f;
                    m_verMov = 0f;
                }

                if (mouseFocusManager.IsFocused(InputMouseFocus.PLAYER))
                {
                    do
                    {
                        if (m_player.ProcessInputForBuild())
                        {
                            break;
                        }

                        if (Input.GetButton("Fire1"))
                        {
                            m_player.TryFire();
                        }

                        if (Input.GetButtonUp("Fire1"))
                        {
                            m_player.TryFireCharged();
                        }
                    }
                    while (false);
                }


                Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
                m_lookingDirection = (worldMousePosition - m_player.transform.position);

            } while (false);
        }

        
    }
}
