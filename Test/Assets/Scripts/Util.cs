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

            public static void SetState<T>(out T srcState, T trgState)
            {
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
}

