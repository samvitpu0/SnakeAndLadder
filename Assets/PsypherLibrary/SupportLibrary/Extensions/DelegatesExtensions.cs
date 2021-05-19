using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace PsypherLibrary.SupportLibrary.Extensions
{
    public static class DelegatesExtensions
    {
        public static void SafeInvoke(this Action callBack)
        {
            if (callBack != null)
            {
                callBack();
            }
        }

        public static void SafeInvoke<X>(this Action<X> callBack, X data)
        {
            if (callBack != null)
            {
                callBack(data);
            }
        }

        public static void SafeInvoke<X, Y>(this Action<X, Y> callBack, X data1, Y data2)
        {
            if (callBack != null)
            {
                callBack(data1, data2);
            }
        }

        public static void SafeInvoke<X, Y, Z>(this Action<X, Y, Z> callBack, X data1, Y data2, Z data3)
        {
            if (callBack != null)
            {
                callBack(data1, data2, data3);
            }
        }

        public static void SafeInvoke<X, Y, Z, W>(this Action<X, Y, Z, W> callBack, X data1, Y data2, Z data3, W data4)
        {
            if (callBack != null)
            {
                callBack(data1, data2, data3, data4);
            }
        }

        public static void SafeInvoke(this UnityEvent callBack)
        {
            if (callBack != null)
            {
                callBack.Invoke();
            }
        }

        public static void SafeInvoke<X>(this UnityEvent<X> callBack, X data)
        {
            if (callBack != null)
            {
                callBack.Invoke(data);
            }
        }

        public static void SafeInvoke<X, Y>(this UnityEvent<X, Y> callBack, X data1, Y data2)
        {
            if (callBack != null)
            {
                callBack.Invoke(data1, data2);
            }
        }

        public static void SafeInvoke<X, Y, Z>(this UnityEvent<X, Y, Z> callBack, X data1, Y data2, Z data3)
        {
            if (callBack != null)
            {
                callBack.Invoke(data1, data2, data3);
            }
        }

        public static void SafeInvoke<X, Y, Z, W>(this UnityEvent<X, Y, Z, W> callBack, X data1, Y data2, Z data3, W data4)
        {
            if (callBack != null)
            {
                callBack.Invoke(data1, data2, data3, data4);
            }
        }

        #region Delayed Invoke

        /// <summary>
        ///     delayed invoke for general purposes, unity specific properties/methods cannot be called by this method.
        ///     Use it's overloaded method for unity specific properties/methods
        /// </summary>
        public static void InvokeAfter(this Action action, float delay)
        {
            Timer timer = null;
            timer = new Timer(obj =>
                {
                    action.SafeInvoke();
                    timer.Dispose();
                },
                null, (int) (delay * 1000), Timeout.Infinite);
        }

        /// <summary>
        ///     delayed invoke for Monobehaviour, unity specific properties/methods can be safely call by this method
        /// </summary>
        public static Coroutine InvokeAfter(this MonoBehaviour mono, Action onComplete, float delay, Action<float> onUpdate = null, float? deltaDelay = null, float delayBeforeComplete = 0.00f, Predicate<bool> extraWaitCheck = null, bool ignoreUnityTimeScale = false)
        {
            var dDelay = deltaDelay ?? delay % 1; // making smallest fragment to be the delta delay, when deltaDelay is not supplied
            if (dDelay <= 0) dDelay = 1;

            return mono.StartCoroutine(InvokeMethod(onComplete, delay, onUpdate, dDelay, delayBeforeComplete, extraWaitCheck, ignoreUnityTimeScale));
        }


        private static IEnumerator InvokeMethod(Action onComplete, float delay, Action<float> onUpdate, float deltaDelay, float delayBeforeComplete, Predicate<bool> extraWaitCheck, bool ignoreUnityTimeScale)
        {
            float elapsedTime = 0;
            while (true)
            {
                if (extraWaitCheck != null)
                    yield return new WaitWhile(() => extraWaitCheck.Invoke(true));

                if (elapsedTime >= delay)
                    break;

                elapsedTime += deltaDelay;
                onUpdate.SafeInvoke(elapsedTime);

                if (ignoreUnityTimeScale)
                {
                    yield return new WaitForSecondsRealtime(deltaDelay);
                }
                else
                {
                    yield return new WaitForSeconds(deltaDelay);
                }
            }

            if (ignoreUnityTimeScale)
            {
                yield return new WaitForSecondsRealtime(delayBeforeComplete);
            }
            else
            {
                yield return new WaitForSeconds(delayBeforeComplete);
            }

            onComplete.SafeInvoke();
        }

        #endregion
    }
}