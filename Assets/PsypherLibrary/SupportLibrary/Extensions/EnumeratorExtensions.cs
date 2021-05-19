using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PsypherLibrary.SupportLibrary.Extensions
{
    public class Iterator
    {
        int currentValue = 0;

        public int Next()
        {
            return currentValue++;
        }

        public int GetIndex()
        {
            return currentValue;
        }

        public int Skip(int skipBy)
        {
            currentValue += skipBy;
            return currentValue++;
        }
    }

    public static class EnumeratorExtensions
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static bool Has<T>(this System.Enum type, T value)
        {
            try
            {
                return (((int) (object) type & (int) (object) value) == (int) (object) value);
            }
            catch
            {
                return false;
            }
        }

        public static bool Is<T>(this System.Enum type, T value)
        {
            try
            {
                return (int) (object) type == (int) (object) value;
            }
            catch
            {
                return false;
            }
        }


        public static T Add<T>(this System.Enum type, T value)
        {
            try
            {
                return (T) (object) (((int) (object) type | (int) (object) value));
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    string.Format(
                        "Could not append value from enumerated type '{0}'.",
                        typeof(T).Name
                    ), ex);
            }
        }


        public static T Remove<T>(this System.Enum type, T value)
        {
            try
            {
                return (T) (object) (((int) (object) type & ~(int) (object) value));
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    string.Format(
                        "Could not remove value from enumerated type '{0}'.",
                        typeof(T).Name
                    ), ex);
            }
        }

        /// <summary>
        /// Convert a string value to an Enum value.
        /// </summary>
        public static T AsEnum<T>(this string source, bool ignoreCase = true)
            where T : System.Enum =>
            (T) Enum.Parse(typeof(T), source, ignoreCase);

        public static string GetDescription<T>(this T enumerationValue)
            where T : struct
        {
            Type type = enumerationValue.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException("EnumerationValue must be of Enum type", "enumerationValue");
            }

            //Tries to find a DescriptionAttribute for a potential friendly name
            //for the enum
            MemberInfo[] memberInfo = type.GetMember(enumerationValue.ToString());
            if (memberInfo != null && memberInfo.Length > 0)
            {
                object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs != null && attrs.Length > 0)
                {
                    //Pull out the description value
                    return ((DescriptionAttribute) attrs[0]).Description;
                }
            }

            //If we have no description attribute, just return the ToString of the enum
            return enumerationValue.ToString();
        }

        public static T GetDescription<T>(this Enum enumerationValue)
            where T : DescriptionInfo
        {
            var type = enumerationValue.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException("EnumerationValue must be of Enum type", "enumerationValue");
            }

            //Tries to find a DescriptionAttribute for a potential friendly name
            //for the enum
            MemberInfo[] memberInfo = type.GetMember(enumerationValue.ToString());
            if (memberInfo != null && memberInfo.Length > 0)
            {
                var attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionInfo), false);

                if (attrs != null && attrs.Length > 0)
                {
                    //Pull out the description value
                    return ((T) attrs[0]);
                }
            }

            //If we have no description attribute, just return default value of T
            return default(T);
        }


        public static void MoveItem<T>(this List<T> list, int currentIndex, int destIndex)
        {
            T item = list[currentIndex];
            list.RemoveAt(currentIndex);
            list.Insert(destIndex, item);
        }

        /// <summary>
        /// Gets the random enum from the type input
        /// </summary>
        /// <returns>The random enum.</returns>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T GetRandomEnum<T>()
        {
            System.Array A = System.Enum.GetValues(typeof(T));
            T v = (T) A.GetValue(UnityEngine.Random.Range(0, A.Length));

            return v;
        }

        /// <summary>
        /// Sorts the given list
        /// </summary>
        /// <param name="list">List.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void SortByName<T>(this List<T> list)
        {
            list.Sort((x, y) => string.Compare(x.ToString(), y.ToString()));
        }

        /// <summary>
        /// Determines if given collectables is equal to the compared
        /// </summary>
        public static bool IsEqualTo<T>(this IList<T> list, IList<T> other)
        {
            if (list.Count != other.Count)
                return false;
            for (int i = 0, count = list.Count; i < count; i++)
            {
                if (!list[i].Equals(other[i]))
                {
                    return false;
                }
            }

            return true;
        }


        public static void Each<T>(this IEnumerable<T> ie, Action<T, int> action)
        {
            var enumerable = ie.ToList();
            for (var i = 0; i < enumerable.Count; i++)
            {
                action(enumerable[i], i);
            }
        }


        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            return source.PickRandom(1).Single();
        }

        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }

        public static T ToEnum<T>(this string str)
        {
            return (T) Enum.Parse(typeof(T), str);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T element in source)
            {
                action(element);
            }

            return source;
        }

        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int N)
        {
            return source.Skip(Math.Max(0, source.Count() - N));
        }

        public static void AssignRandomElementsFromArray<T>(T[] arrayToAssign, T[] baseArray)
        {
            List<T> baseList = baseArray.ToList();

            for (int i = 0; i < arrayToAssign.Length; ++i)
            {
                int randomIndex = UnityEngine.Random.Range(0, baseList.Count() - 1); // (min, max) both inclusive

                arrayToAssign[i] = baseList[randomIndex];

                baseList.RemoveAt(randomIndex);

                Debug.Log("randomly picked element from index " + randomIndex + " from array of length " + baseArray.Length);
            }
        }


        public static bool FindAndRemove<T>(this List<T> list, Predicate<T> condition)
        {
            var itemToRemove = list.Find(condition);
            if (itemToRemove != null)
            {
                list.Remove(itemToRemove);
                return true;
            }

            return false;
        }

        public static List<T> RepeatedDefault<T>(int count)
        {
            return Repeated(default(T), count);
        }

        public static List<T> Repeated<T>(T value, int count)
        {
            List<T> ret = new List<T>(count);
            ret.AddRange(Enumerable.Repeat(value, count));
            return ret;
        }


        public static void SafeAdd<T, Y>(this IDictionary<T, Y> dic, T key, Y val)
        {
            if (dic.ContainsKey(key))
                dic[key] = val;
            else
            {
                dic.Add(key, val);
            }
        }

        public static Y SafeRetrieve<T, Y>(this IDictionary<T, Y> dic, T key)
        {
            return dic.ContainsKey(key) ? dic[key] : default(Y);
        }

        public static void SetActiveInteractables<T>(this IEnumerable<T> mEnumerable, bool isActive = true) where T : Selectable
        {
            foreach (var item in mEnumerable)
            {
                item.interactable = isActive;
            }
        }

        public static void SetActiveToggles(this IEnumerable<Toggle> mEnumerable, bool isActive = true, bool invokeCallBack = false)
        {
            foreach (var item in mEnumerable)
            {
                item.isOn = isActive;

                if (invokeCallBack) //if true, invoke the callback
                    item.onValueChanged.Invoke(isActive);
            }
        }

        public static WWWForm GetWWWForm(this IDictionary<string, string> queries)
        {
            WWWForm form = new WWWForm();
            foreach (KeyValuePair<string, string> arg in queries)
            {
                form.AddField(arg.Key, arg.Value);
            }

            return form;
        }

        /// <summary>
        /// checks if the object is among the list provided, list items as params
        /// </summary>
        public static bool IsEither<T>(this T obj, params T[] collection)
        {
            return collection.Contains(obj);
        }

        /// <summary>
        /// checks if the object is among the list provided, input the list
        /// </summary>
        public static bool IsEither<T>(this T obj, ICollection<T> collection)
        {
            return collection.Contains(obj);
        }

        /// <summary>
        /// give a random element among the given set
        /// </summary>
        public static T RandomAmong<T>(params T[] elements)
        {
            var v = (T) elements.GetValue(UnityEngine.Random.Range(0, elements.Length));

            return v;
        }

        /// <summary>
        /// Choose a element from the given list based on the given values. Chance value should be 0.0 to 1.0
        /// </summary>
        /// <param name="chanceBasedElements"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ChooseByChance<T>(params KeyValuePair<T, double>[] chanceBasedElements)
        {
            var elements = chanceBasedElements.ToList();

            //sorting list ascending of chance value
            elements.Sort((x, y) => x.Value.CompareTo(y.Value));

            var masterRandomList = new List<T>();

            foreach (var element in elements)
                for (var count = 0; count < element.Value * 100; count++)
                    masterRandomList.Add(element.Key);

            return masterRandomList.PickRandom();
        }

        public static void Shuffle<T>(this T[,] array)
        {
            var random = new System.Random();
            var lengthRow = array.GetLength(1);

            for (var i = array.Length - 1; i > 0; i--)
            {
                var i0 = i / lengthRow;
                var i1 = i % lengthRow;

                var j = random.Next(i + 1);
                var j0 = j / lengthRow;
                var j1 = j % lengthRow;

                var temp = array[i0, i1];
                array[i0, i1] = array[j0, j1];
                array[j0, j1] = temp;
            }
        }

        /// <summary>
        /// return the next or previous item from a list with respect to steps
        /// </summary>
        public static T JumpBy<T>(this List<T> collection, int startIndex, int step, out int currentIndex, bool loop = true)
        {
            var calcStep = startIndex + step;

            if (calcStep < 0)
            {
                if (loop)
                {
                    calcStep = collection.Count - Mathf.Abs(calcStep);
                }
                else
                {
                    calcStep = 0;
                }
            }
            else if (calcStep >= collection.Count)
            {
                if (loop)
                {
                    calcStep = calcStep - collection.Count;
                }
                else
                {
                    calcStep = collection.Count - 1;
                }
            }

            currentIndex = calcStep;

            return collection[calcStep];
        }

        /// <summary>
        /// Jump the current index of a collection and return the new element, safe mode checks and caps the extremes to first and last elements
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="startIndex"></param>
        /// <param name="step"></param>
        /// <param name="loop"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T JumpBy<T>(this List<T> collection, int startIndex, int step, bool loop = true)
        {
            var calcStep = startIndex + step;

            if (calcStep < 0)
            {
                if (loop)
                {
                    calcStep = collection.Count - Mathf.Abs(calcStep);
                }
                else
                {
                    calcStep = 0;
                }
            }
            else if (calcStep >= collection.Count)
            {
                if (loop)
                {
                    calcStep = calcStep - collection.Count;
                }
                else
                {
                    calcStep = collection.Count - 1;
                }
            }

            return collection[calcStep];
        }

        //--Json related

        public static List<T> TryAndFindList<T>(this IEnumerable<JToken> jList, Predicate<JToken> filter = null)
        {
            var itemList = new List<T>();
            jList.ForEach(x =>
            {
                //trying to add categories from overall category data
                try
                {
                    if (filter != null)
                    {
                        if (filter(x))
                        {
                            var cat = x.ToObject<T>();
                            itemList.Add(cat);
                        }
                    }
                    else
                    {
                        var cat = x.ToObject<T>();
                        itemList.Add(cat);
                    }
                }
                catch (Exception)
                {
                    //Debug.Log(e);
                }
            });

            return itemList;
        }

        public static T TryAndFind<T>(this JToken jToken, Predicate<JToken> filter = null)
        {
            T obj = default(T);
            try
            {
                if (filter != null)
                {
                    if (filter(jToken))
                    {
                        obj = jToken.ToObject<T>();
                    }
                }
                else
                {
                    obj = jToken.ToObject<T>();
                }

                return obj;
            }
            catch (Exception)
            {
                //Debug.Log(e);
                return default(T);
            }
        }

        public static void AddUnique<T>(this ICollection<T> collection, T element, Predicate<T> condition = null)
        {
            var collectionInstance = new List<T>(collection);
            if (condition != null)
            {
                bool shouldAdd = true;
                foreach (var item in collectionInstance)
                {
                    if (!condition(item))
                    {
                        shouldAdd = false;
                        break;
                    }
                }

                if (shouldAdd)
                {
                    collection.Add(element);
                }
            }
            else
            {
                if (!collection.Contains(element))
                {
                    collection.Add(element);
                }
            }
        }

        public static bool Contains<T>(this IEnumerable<T> collection, Predicate<T> condition)
        {
            var collectionInstance = new List<T>(collection);
            if (condition != null)
            {
                foreach (var item in collectionInstance)
                {
                    if (condition(item))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static void AddUniqueElseModify<T>(this IList<T> collection, T element, Predicate<T> condition = null)
        {
            var collectionInstance = new List<T>(collection);
            if (condition != null)
            {
                var index = 0;
                bool shouldAdd = true;
                for (var i = 0; i < collectionInstance.Count; i++)
                {
                    var item = collectionInstance[i];
                    if (!condition(item))
                    {
                        shouldAdd = false;
                        index = i;
                        break;
                    }
                }

                if (shouldAdd)
                {
                    collection.Add(element);
                }
                else
                {
                    collection[index] = element;
                }
            }
            else
            {
                if (!collectionInstance.Contains(element))
                {
                    collection.Add(element);
                }
                else
                {
                    try
                    {
                        var index = collectionInstance.FindIndex(item => item.Equals(element));
                        collection[index] = element;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }

        #region 2D Flips

        /// <summary>
        /// Flips the 2D array horizontally 
        /// </summary>
        /// <param name="inputMatrix"></param>
        /// <typeparam name="T"></typeparam>
        public static void HorizontalFlip<T>(this T[,] inputMatrix)
        {
            Console.WriteLine("Flip Matrix Horizontally..");
            var rows = inputMatrix.GetLength(0);
            var cols = inputMatrix.GetLength(1);

            for (var i = 0; i <= rows - 1; i++)
            {
                var j = 0;
                var k = cols - 1;
                while (j < k)
                {
                    var temp = inputMatrix[i, j];
                    inputMatrix[i, j] = inputMatrix[i, k];
                    inputMatrix[i, k] = temp;
                    j++;
                    k--;
                }
            }
        }

        /// <summary>
        /// Flips the 2D array vertically
        /// </summary>
        /// <param name="inputMatrix"></param>
        /// <typeparam name="T"></typeparam>
        public static void VerticalFlip<T>(this T[,] inputMatrix)
        {
            Console.WriteLine("Flip Matrix Vertically..");
            var rows = inputMatrix.GetLength(0);
            var cols = inputMatrix.GetLength(1);

            for (var i = 0; i <= cols - 1; i++)
            {
                var j = 0;
                var k = rows - 1;
                while (j < k)
                {
                    var temp = inputMatrix[j, i];
                    inputMatrix[j, i] = inputMatrix[k, i];
                    inputMatrix[k, i] = temp;
                    j++;
                    k--;
                }
            }
        }

        #endregion

        /// <summary>
        /// Is array null or empty
        /// </summary>
        public static bool IsNullOrEmpty<T>(this T[] collection)
        {
            if (collection == null) return true;

            return collection.Length == 0;
        }

        /// <summary>
        /// Is list null or empty
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IList<T> collection)
        {
            if (collection == null) return true;

            return collection.Count == 0;
        }

        /// <summary>
        /// Is collection null or empty. IEnumerable is relatively slow. Use Array or List implementation if possible
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            if (collection == null) return true;

            return !collection.Any();
        }
    }
}