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
    //Unique State.
	public enum CharacterState 
	{
		GROUNDED = 0,
		JUMPING_UP,
		FALLING,
		WALL_JUMPING,
		WALL_WALKING
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
}

