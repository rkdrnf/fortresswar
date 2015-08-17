//------------------------------------------------------------------------------
// <auto-generated>
//     이 코드는 도구를 사용하여 생성되었습니다.
//     런타임 버전:4.0.30319.18444
//
//     파일 내용을 변경하면 잘못된 동작이 발생할 수 있으며, 코드를 다시 생성하면
//     이러한 변경 내용이 손실됩니다.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using UnityEngine;
using Const;
using System.Globalization;
using System.Collections.Generic;

namespace Util
{
	public abstract class LayerUtil
	{
		public static bool HasLayer(int layer, LayerMask layers)
		{
			return HasLayer (layer, layers.value);
		}

		public static bool HasLayer(int layer, params string[] layers)
		{
			return HasLayer(layer, LayerMask.GetMask(layers));
		}


		public static bool HasLayer(int layer, int layerArray)
		{
			return ((1 << layer) & layerArray) != 0;
		}

        public static bool IsLayerExists(Vector2 pos, LayerMask mask)
        {
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero, float.MaxValue, mask);

            if (hit)
            {
                return true;
            }
            else return false;
        }

        public static GameObject GetLayerObjectAt(Vector2 pos, LayerMask mask)
        {
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero, float.MaxValue, mask);

            if (hit)
            {
                return hit.collider.gameObject;
            }
            else return null;
        }
	}

    public abstract class StateUtil
    {
        public static bool IsInState<T>(T srcState, T state, params T[] stateList) where T : IComparable
        {
            bool result = srcState.CompareTo(state) == 0;

            foreach (T stateVal in stateList)
            {
                if (result == true)
                    break;

                result = srcState.CompareTo(stateVal) == 0;
            }

            return result;
        }

        public static void SetState<T>(ref T srcState, T trgState)
        {
            if (srcState.Equals(trgState))
                return;

            srcState = trgState;
        }

        public static bool IsNotInState<T>(T srcState, T state, params T[] stateList) where T : IComparable
        {
            bool result = srcState.CompareTo(state) != 0;

            foreach (T stateVal in stateList)
            {
                if (result == false)
                    break;

                result = srcState.CompareTo(stateVal) != 0;
            }

            return result;
        }
    }

    public abstract class StateManager<T> where T : IComparable
    {
        protected T state;

        public bool IsInState(T state, params T[] states)
        {
            return StateUtil.IsInState<T>(this.state, state, states);
        }

        public T GetState()
        {
            return state;
        }

        public virtual void SetState(T newState)
        {
            StateUtil.SetState<T>(ref this.state, newState);
        }

        public bool IsNotInState(T state, params T[] states)
        {
            return StateUtil.IsNotInState<T>(this.state, state, states);
        }

    }

    public abstract class EnvManager<T> where T : IComparable, IConvertible
    {
        int envFlag;

        int ShiftEnv(T env)
        {
            return 1 << env.ToInt32(CultureInfo.InvariantCulture.NumberFormat);
        }

        public bool IsInEnv(T env, params T[] envList)
        {
            bool result = (envFlag & ShiftEnv(env)) != 0;


            foreach (T envVal in envList)
            {
                if (result == false)
                    break;

                result = result && ((envFlag & ShiftEnv(envVal)) != 0);
            }

            return result;
        }

        public void SetEnv(T env, bool value)
        {
            if (IsInEnv(env) != value)
            {
                //Debug.Log("EnvSet : " + env + "   " + value);
            }


            if (value)
            {
                envFlag = envFlag | ShiftEnv(env);
            }
            else
            {
                envFlag = envFlag & (~ShiftEnv(env));
            }
        }
    }

    public abstract class Calc
    {
        public static int mod(int a, int n)
        {
            int result = a % n;
            if ((a < 0 && n > 0) || (a > 0 && n < 0))
                result += n;
            return result % n;
        }
    }

    public abstract class USelection
    {
        public static List<GridCoordDist> GetCoordsInRange(Vector2 center, float radius)
        {
            List<GridCoordDist> points = new List<GridCoordDist>();

            int yBottom = Mathf.CeilToInt(center.y - radius);
            int yTop = Mathf.FloorToInt(center.y + radius);

            for (int y = yBottom; y <= yTop; y++)
            {
                float yLength = center.y - y;
                float xLengthSqr = (radius * radius) - (yLength * yLength);
                float xLength = xLengthSqr < 0 ? 0 : Mathf.Sqrt(xLengthSqr);

                int xLeft = Mathf.CeilToInt(center.x - xLength);
                int xRight = Mathf.FloorToInt(center.x + xLength);

                for (int x = xLeft; x <= xRight; x++)
                {
                    float length = Mathf.Sqrt((center.x - x) * (center.x - x) + (yLength * yLength));
                    points.Add(new GridCoordDist(new GridCoord((short)x, (short)y), length));
                }
            }

            return points;
        }
    }
}

