using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const;

namespace Character
{ 
    public class CharacterMoveController
    {
        float m_horMov
        {
            get { return m_player.GetInputX(); }
        }
        float m_verMov
        {
            get { return m_player.GetInputY(); }
        }

        float m_wallWalkTimer;
        bool m_facingRight;

        ServerPlayer m_player;

        JobStat jobStat
        {
            get
            {
                return m_player.GetJobStat();
            }
        }

        public float GetMoveX()
        {
            return m_horMov;
        }

        public float GetMoveY()
        {
            return m_verMov;
        }

        public bool IsFacingRight()
        {
            return m_facingRight;
        }

        public CharacterMoveController(ServerPlayer player)
        {
            m_facingRight = true;
            m_wallWalkTimer = 0;

            m_player = player;
        }

        public void ProcessCharacter()
        {
            if (Network.isServer)
            {
                if (m_player.IsDead())
                {
                    return;
                }

                CharacterState state = m_player.GetState();

                switch(state)
                {
                    case CharacterState.GROUNDED:
                        WhenGrounded();
                        break;

                    case CharacterState.FALLING:
                        WhenFalling();
                        break;

                    case CharacterState.JUMPING_UP:
                        WhenJumping();
                        break;

                    case CharacterState.WALL_JUMPING:
                        WhenWallJumping();
                        break;
                        
                    case CharacterState.WALL_WALKING:
                        WhenWallWalking();
                        break;

                    case CharacterState.ROPING:
                        WhenRoping();
                        break;
                }


                if (!m_player.IsLookingRight() && m_facingRight)
                {
                    Flip();
                }
                if (m_player.IsLookingRight() && !m_facingRight)
                {
                    Flip();
                }
            }
        }

        void WhenGrounded()
        {
            //Set Other States
            EndWallWalk();
            m_player.SetEnv(CharacterEnv.WALL_WALKED_LEFT, false);
            m_player.SetEnv(CharacterEnv.WALL_WALKED_RIGHT, false);

            MoveInGround();

            //
            if (m_verMov > 0)
            {
                Debug.Log("jump!!!");
                Jump();

            }
        }

        void MoveInGround()
        {
            const int multiplier = 300;
            if (m_horMov != 0)
            {
                if (m_horMov * m_player.rigidbody2D.velocity.x < 0) // 다른방향
                {
                    m_player.rigidbody2D.AddForce(new Vector2(m_horMov * jobStat.MovingSpeed * multiplier, 0));
                }
                else // 같은 방향
                {
                    m_player.rigidbody2D.AddForce(new Vector2(m_horMov * jobStat.MovingSpeed * multiplier, 0));
                    if (m_player.rigidbody2D.velocity.x > jobStat.MovingSpeed || m_player.rigidbody2D.velocity.x < -jobStat.MovingSpeed) //초과시 최대스피드로
                    {
                        m_player.rigidbody2D.velocity = new Vector2(m_horMov * jobStat.MovingSpeed, m_player.rigidbody2D.velocity.y);
                    }
                }
            }
            else
            {
                m_player.rigidbody2D.velocity = Vector2.Lerp(m_player.rigidbody2D.velocity, new Vector2(0, m_player.rigidbody2D.velocity.y), Time.deltaTime * 4);
            }
        }

        void MoveInAir()
        {
            const int multiplier = 200;
            if (m_horMov != 0)
            {
                if (m_horMov * m_player.rigidbody2D.velocity.x < 0) // 다른방향
                {
                    m_player.rigidbody2D.AddForce(new Vector2(m_horMov * jobStat.MovingSpeed * multiplier, 0));
                }
                else // 같은 방향
                {
                    m_player.rigidbody2D.AddForce(new Vector2(m_horMov * jobStat.MovingSpeed * multiplier, 0));
                    if (m_player.rigidbody2D.velocity.x > jobStat.MovingSpeed || m_player.rigidbody2D.velocity.x < -jobStat.MovingSpeed) //초과시 최대스피드로
                    {
                        m_player.rigidbody2D.velocity = new Vector2(m_horMov * jobStat.MovingSpeed, m_player.rigidbody2D.velocity.y);
                    }
                }
            }
            else
            {
                m_player.rigidbody2D.velocity = Vector2.Lerp(m_player.rigidbody2D.velocity, new Vector2(0, m_player.rigidbody2D.velocity.y), Time.deltaTime * 4);
            }
        }

        void MoveInAirDirection(float maxSpeed)
        {
            const int multiplier = 200;
            if (m_horMov != 0)
            {
                if (m_horMov * m_player.rigidbody2D.velocity.x < 0) // 다른방향
                {
                    m_player.rigidbody2D.AddForce(new Vector2(m_horMov * jobStat.MovingSpeed * multiplier, 0));
                }
                else // 같은 방향
                {
                    m_player.rigidbody2D.AddForce(new Vector2(m_horMov * jobStat.MovingSpeed * multiplier, 0));
                    if (m_player.rigidbody2D.velocity.x > maxSpeed || m_player.rigidbody2D.velocity.x < -maxSpeed) //초과시 최대스피드로
                    {
                        m_player.rigidbody2D.velocity = new Vector2(m_horMov * maxSpeed, m_player.rigidbody2D.velocity.y);
                        Debug.Log("MaxSpeed: " + m_player.rigidbody2D.velocity);
                    }
                }
            }
        }

        void WhenFalling()
        {
            MoveInAir();

            if (m_horMov != 0)
            {
                //WallJump Or Climb more.
                if (m_verMov > 0)
                {
                    //Debug.Log("CheckWallJump");
                    CheckWallJump();

                    return;
                }
            }
        }

        void StopJumping()
        {
            if (m_player.rigidbody2D.velocity.y > 0)
            {
                m_player.rigidbody2D.velocity = Vector2.Lerp(m_player.rigidbody2D.velocity, new Vector2(m_player.rigidbody2D.velocity.x, 0), Time.deltaTime * 4);
            }
        }

        void WhenJumping()
        {
            MoveInAir();

            if (m_horMov != 0 && m_verMov > 0)
            {
                CheckWallJump();
            }

            if (m_verMov <= 0)
            {
                StopJumping();
            }

            if (m_player.rigidbody2D.velocity.y <= 0f)
            {
                m_player.SetState(CharacterState.FALLING);
            }
        }

        void WhenWallJumping()
        {
            MoveInAirDirection(jobStat.WallJumpingSpeed.x);

            if (m_horMov != 0 && m_verMov > 0)
            {
                CheckWallJump();
            }

            if (m_verMov <= 0)
            {
                StopJumping();
            }
        }

        void WhenWallWalking()
        {
            //Fall to ground.
            if (m_horMov == 0)
            {
                m_player.SetState(CharacterState.FALLING);
                return;
            }

            if (m_horMov != 0)
            {
                //WallJump Or Climb more.
                if (m_verMov > 0)
                {
                    //월 워킹 중에 위 방향키를 누르고 있는데도 월 관련 함수가 실패하면 벽이 사라진거. 가던 방향으로 점프함.
                    if (CheckWallJump())
                    {
                        return;
                    }
                    else
                    {
                        Jump();
                    }
                }

                //Fall to ground.
                if (m_verMov <= 0f)
                {
                    m_player.rigidbody2D.velocity = new Vector2(m_horMov * jobStat.MovingSpeed, m_player.rigidbody2D.velocity.y);


                    m_player.SetState(CharacterState.FALLING);
                    return;
                }

                m_player.rigidbody2D.velocity = new Vector2(m_horMov * jobStat.MovingSpeed, m_player.rigidbody2D.velocity.y);
            }
        }


        bool IsMoving(Direction direction)
        {
            return direction == Direction.RIGHT ? m_horMov > 0 : m_horMov < 0;
        }

        bool IsWalled(Direction direction)
        {
            if (direction == Direction.LEFT)
            {
                return m_facingRight ? m_player.IsInEnv(CharacterEnv.WALLED_BACK) : m_player.IsInEnv(CharacterEnv.WALLED_FRONT);
            }
            else
            {
                return m_facingRight ? m_player.IsInEnv(CharacterEnv.WALLED_FRONT) : m_player.IsInEnv(CharacterEnv.WALLED_BACK);
            }
        }

        void Jump()
        {
            m_player.SetState(CharacterState.JUMPING_UP);
            m_player.rigidbody2D.velocity = new Vector2(m_player.rigidbody2D.velocity.x, jobStat.JumpingSpeed);
        }

        void WallWalk(Direction direction)
        {
            //Already wall walked same wall
            if ((m_player.IsNotInState(CharacterState.WALL_WALKING)) && WallWalked(direction))
                return;

            m_player.SetState(CharacterState.WALL_WALKING);

            m_player.SetEnv(GetWallWalkStateByDirection(direction), true);
            m_player.SetEnv(direction == Direction.RIGHT ? CharacterEnv.WALL_WALKED_LEFT : CharacterEnv.WALL_WALKED_RIGHT, false);
            m_wallWalkTimer += Time.deltaTime;

            if (m_wallWalkTimer > jobStat.WallWalkingTime)
            {
                EndWallWalk();
                m_player.SetState(CharacterState.FALLING);
                return;
            }

            m_player.rigidbody2D.velocity = new Vector2(m_player.rigidbody2D.velocity.x, jobStat.WallWalkingSpeed);
        }

        void EndWallWalk()
        {
            m_wallWalkTimer = 0f;
        }

        CharacterEnv GetWallWalkStateByDirection(Direction direction)
        {
            return direction == Direction.RIGHT ? CharacterEnv.WALL_WALKED_RIGHT : CharacterEnv.WALL_WALKED_LEFT;
        }

        bool WallWalked(Direction direction)
        {
            return m_player.IsInEnv(GetWallWalkStateByDirection(direction));
        }

        bool CheckWallJump()
        {
            do
            {
                if (m_player.rigidbody2D.velocity.y < -6f)
                    return false;
                //Check Wall and moving direction

                //Climb more;
                if (IsWalled(Direction.LEFT) && IsMoving(Direction.LEFT))
                {
                    WallWalk(Direction.LEFT);
                    return true;
                }

                if (IsWalled(Direction.RIGHT) && IsMoving(Direction.RIGHT))
                {
                    WallWalk(Direction.RIGHT);
                    return true;
                }

                //Wall Jump;
                if (IsWalled(Direction.LEFT) && IsMoving(Direction.RIGHT))
                {
                    WallJump(Direction.RIGHT);
                    return true;
                }

                if (IsWalled(Direction.RIGHT) && IsMoving(Direction.LEFT))
                {
                    WallJump(Direction.LEFT);
                    return true;
                }

            } while (false);

            return false;
        }

        void WallJump(Direction direction)
        {
            Debug.Log("Wall Jump!!");
            EndWallWalk();

            m_player.SetState(CharacterState.WALL_JUMPING);
            m_player.rigidbody2D.velocity = new Vector2(direction == Direction.RIGHT ? jobStat.WallJumpingSpeed.x : -jobStat.WallJumpingSpeed.x, jobStat.WallJumpingSpeed.y);
        }

        void WhenRoping()
        {
            if (m_verMov != 0)
            {
                m_player.ModifyRopeLength(m_verMov * jobStat.RopingSpeed * Time.deltaTime);
            }

            if (m_horMov != 0)
            {
                Vector2 normalVector = m_player.GetRopeDirection();
                Vector2 perpendicular = (m_horMov == 1) ?
                    new Vector2(normalVector.y, -normalVector.x)
                    : new Vector2(-normalVector.y, normalVector.x);

                perpendicular.Normalize();

                m_player.rigidbody2D.AddForce(perpendicular * jobStat.RopeMovingSpeed * 70);
            }
        }

        void Flip()
        {
            if (m_player.IsDead())
                return;

            m_facingRight = !m_facingRight;
            Vector3 scale = m_player.transform.localScale;
            scale.x = -scale.x;
            m_player.transform.localScale = scale;
        }
    }
}
