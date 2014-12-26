//------------------------------------------------------------------------------
// <auto-generated>
//     μ½λκ΅¬λ₯¬μ©μ¬ μ±μ΅λ
//     °νλ²μ :4.0.30319.18444
//
//     μΌ ΄μ©λ³κ²½νλ©λͺ»μλ°μμΌλ© μ½λλ₯€μ μ±λ©΄
//     ΄λ¬λ³κ²΄μ©μ€©λ
// </auto-generated>
//------------------------------------------------------------------------------
using System;
namespace Const
{
    public enum GameState
    {
        IN_MENU,
        SETTING_PLAYER,
        PLAYING
    }

    public enum InputKeyFocus
    {
        CHAT_WINDOW,
        PLAYER,
        TEAM_SELECTOR,
        JOB_SELECTOR,
        NAME_SELECTOR
    }

    public enum InputMouseFocus
    {
        CHAT_WINDOW,
        PLAYER,
        TEAM_SELECTOR,
        JOB_SELECTOR,
        NAME_SELECTOR
    }

    public enum ChatState
    {
        NONE,
        WRITING,
        NEW_MESSAGE
    }

    public enum NameSelectorState
    {
        ON,
        OFF
    }

    public enum TeamSelectorState
    {
        ON,
        OFF
    }

    public enum JobSelectorState
    {
        ON,
        OFF
    }

    public enum MenuState
    {
        ON,
        OFF
    }

    public enum Team
    {
        NONE,
        BLUE,
        RED
    }

    public enum Job
    {
        SCOUT,
        HEAVY_GUNNER
    }

    public enum PlayerStatus
    {
        NONE,
        OBSERVING,
        PLAYING
    }

    public enum PlayerSettingError
    {
        NONE,
        ID,
        NAME,
        TEAM,
    }

    public enum PlayerSettingColumn
    {
        ID,
        NAME,
        TEAM,
        STATUS
    }

    
    //Unique State.
	public enum CharacterState 
	{
        DEAD = -1,
		GROUNDED = 0,
		JUMPING_UP,
		FALLING,
		WALL_JUMPING,
		WALL_WALKING,
        ROPING
	};

    //Can have multiple state at the same time.
    public enum CharacterEnv
    {
        WALLED_FRONT,
        WALLED_BACK,
        WALL_WALKED_LEFT,
        WALL_WALKED_RIGHT,
    };

	public enum Direction
	{
		RIGHT = 1,
		LEFT = -1
	};

	public enum Facing
	{
		FRONT = 0,
		BACK = 1
	};

    public enum TileType
    {
        DIRT,
        STONE,

        MAX
    }

    public enum WeaponType
    {
        GUN,
        MISSLE,
        ROPE,

        MAX
    }

    public enum AmmoType
    {
        NORMAL,
        INFINITE
    }
}

