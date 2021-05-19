using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace PsypherLibrary.SupportLibrary.Extensions
{
//common includes
    public struct ValueNamePair
    {
        public string name;
        public object obj;

        public ValueNamePair(string name, object obj)
        {
            this.name = name;
            this.obj = obj;
        }
    }

    public enum AnchorPresets
    {
        TopLeft,
        TopCenter,
        TopRight,

        MiddleLeft,
        MiddleCenter,
        MiddleRight,

        BottomLeft,
        BottonCenter,
        BottomRight,
        BottomStretch,

        VertStretchLeft,
        VertStretchRight,
        VertStretchCenter,

        HorStretchTop,
        HorStretchMiddle,
        HorStretchBottom,

        StretchAll
    }

    public enum PivotPresets
    {
        TopLeft,
        TopCenter,
        TopRight,

        MiddleLeft,
        MiddleCenter,
        MiddleRight,

        BottomLeft,
        BottomCenter,
        BottomRight,
    }

    public static class ComponentExtensions
    {
        public static Bounds GetTotalBounds(this Transform transform)
        {
            //getting the initial bound, also check if the parent has bounds
            Bounds combinedBounds = transform.GetComponent<Renderer>()
                ? (transform.GetComponent<Renderer>().bounds.extents != Vector3.zero
                    ? (transform.GetComponent<Renderer>()).bounds
                    : new Bounds(transform.position, Vector3.zero))
                : new Bounds(transform.position, Vector3.zero);

            //iterating over all children in the hierarchy
            foreach (Transform grandChild in transform)
            {
                var render = grandChild.GetComponent<Renderer>();
                if (render && render.bounds.extents != Vector3.zero
                ) //if there is a valid renderer and the bounds extents are greater than zero
                    combinedBounds.Encapsulate(render.bounds);

                combinedBounds.Encapsulate(GetTotalBounds(grandChild));
            }


            return combinedBounds;
        }

        public static Bounds GetTotalBounds(this RectTransform rectTransform, float canvasScale = 1)
        {
            //getting the initial bound, also check if the parent has bounds
            Bounds combinedBounds = rectTransform.GetComponent<Renderer>()
                ? rectTransform.GetComponent<Renderer>().bounds.extents != Vector3.zero
                    ? rectTransform.GetComponent<Renderer>().bounds
                    : new Bounds(rectTransform.position,
                        new Vector3(rectTransform.rect.width, rectTransform.rect.height, 0.0f) * canvasScale)
                : new Bounds(rectTransform.position,
                    new Vector3(rectTransform.rect.width, rectTransform.rect.height, 0.0f) * canvasScale);

            //iterating over all children in the hierarchy
            foreach (Transform grandChild in rectTransform)
            {
                var render = grandChild.GetComponent<Renderer>();
                if (render && render.bounds.extents != Vector3.zero
                ) //if there is a valid renderer and the bounds extents are greater than zero
                    combinedBounds.Encapsulate(render.bounds);

                combinedBounds.Encapsulate(GetTotalBounds(grandChild));
            }


            return combinedBounds;
        }

        public static Rect GetRectFromBounds(this Bounds bound)
        {
            var rect = new Rect(bound.min.x, bound.min.y, bound.size.x, bound.size.y);
            return rect;
        }

        public static void Deactivate(this UnityEngine.Object obj)
        {
            if (obj == null)
                return;

            switch (obj)
            {
                case GameObject gameObject:
                    gameObject.SetActive(false);
                    break;
                case Component component:
                {
                    if (component.gameObject)
                    {
                        component.gameObject.SetActive(false);
                    }

                    break;
                }
                default:
                    throw new Exception("This object has to be a GameObject or a Component!");
            }
        }

        public static void Deactivate(this Graphic obj)
        {
            if (obj == null)
                return;

            if (obj.gameObject)
            {
                obj.gameObject.SetActive(false);
            }
        }

        public static void Activate(this UnityEngine.Object obj)
        {
            if (obj == null)
                return;

            switch (obj)
            {
                case GameObject gameObject:
                    gameObject.SetActive(true);
                    break;
                case Component component:
                {
                    if (component.gameObject)
                    {
                        component.gameObject.SetActive(true);
                    }

                    break;
                }
                default:
                    throw new Exception("This object has to be a GameObject or a Component!");
            }
        }

        public static void Activate(this Graphic obj)
        {
            if (obj == null)
                return;

            if (obj.gameObject)
            {
                obj.gameObject.SetActive(true);
            }
        }

        public static List<Transform> GetActiveChildren(this Transform trans)
        {
            var childs = new List<Transform>();

            foreach (Transform child in trans)
            {
                if (child.gameObject.activeSelf)
                {
                    childs.Add(child);
                }
            }

            return childs;
        }

        public static List<Transform> GetInactiveChildren(this Transform trans)
        {
            var childs = new List<Transform>();

            foreach (Transform child in trans)
            {
                if (!child.gameObject.activeSelf)
                {
                    childs.Add(child);
                }
            }

            return childs;
        }


        public static void ScaleAround(this Transform trans, float scaleBy, Vector3 aroundPoint)
        {
            var A = trans.position;
            var B = aroundPoint;

            var startScale = trans.localScale.magnitude;

            trans.localScale += new Vector3(scaleBy, scaleBy);

            var endScale = trans.localScale.magnitude;

            var relativeScale = endScale / startScale;

            var C = A - B; // diff from object pivot to desired pivot/origin

            // calc final position post-scale
            var FP = (C * relativeScale) + B;
            var finalPos = new Vector3(FP.x, FP.y, 0);

            // finally, translation
            trans.position = finalPos;
        }

        public static Toggle GetActiveToggle(this ToggleGroup aGroup)
        {
            return aGroup.ActiveToggles().FirstOrDefault();
        }

        public static Vector3 getRandomVector3Around(this Vector3 inVect3, float upPerAxis, float downPerAxis)
        {
            float randomX = inVect3.x.GetRandomNumberAround(upPerAxis, downPerAxis);
            float randomY = inVect3.y.GetRandomNumberAround(upPerAxis, downPerAxis);
            float randomZ = inVect3.z.GetRandomNumberAround(upPerAxis, downPerAxis);

            return new Vector3(randomX, randomY, randomZ);
        }

        public static Vector3 PointBetweenTwoVectors(Vector3 vectorOne, Vector3 vectorTwo, float DistanceFromOne)
        {
            return vectorOne + Vector3.Normalize(vectorTwo - vectorOne) * DistanceFromOne;
        }

        public static bool RemoveAndDestroyObject(this List<UnityEngine.Object> list, UnityEngine.Object obj)
        {
            if (list.FindAndRemove(x => x.Equals(obj)))
            {
                UnityEngine.Object.Destroy(obj);
                return true;
            }

            return false;
        }

        public static void SetText(this Text text, object value = null)
        {
            var textToDisplay = value == null ? "" : value.ToString();
            if (text != null)
            {
                var oldText = text.text;
                if (oldText != textToDisplay)
                    text.text = textToDisplay;
            }
        }

        public static void SetImage(this Image image, Sprite imageToSet)
        {
            if (image != null)
                image.sprite = imageToSet;
        }

        public static void SetImage(this Image image, string location)
        {
            if (image != null)
                try
                {
                    image.sprite = Resources.Load<Sprite>(location);
                }
                catch (Exception)
                {
                    Debug.Log("Image Not Found at " + location);
                }
        }

        public static GameObject AddChild(this GameObject parent, GameObject prefab, bool worldPositionStays = false,
            bool addAtStart = false)
        {
            var go = GameObject.Instantiate(prefab);
#if UNITY_EDITOR
            UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create Object");
#endif
            if (go != null && parent != null)
            {
                Transform t = go.transform;
                t.SetParent(parent.transform, worldPositionStays);
                if (addAtStart) t.SetAsFirstSibling(); //if addAtStart add the child as first in the hierarchy
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;
                go.layer = parent.layer;
                go.SetActive(true);
            }

            return go;
        }

        public static GameObject AddSibling(this GameObject sibling, GameObject prefab)
        {
            // Analysis disable once AccessToStaticMemberViaDerivedType
            GameObject go = GameObject.Instantiate(prefab);
#if UNITY_EDITOR
            UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create Object");
#endif
            if (go != null && sibling != null)
            {
                Transform t = go.transform;

                t.SetParent(sibling.transform.parent.transform,
                    false); //Note: no need for parent.transform, parent itself represent a transform
                t.SetSiblingIndex(sibling.transform.GetSiblingIndex() + 1);
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;
                go.layer = sibling.layer;
                go.SetActive(true);
            }

            return go;
        }

        public static List<GameObject> AddChildren(this GameObject Parent, GameObject Child, bool worldPositionStays,
            bool isDisabled, string name, int NumberOfChildren = 0)
        {
            List<GameObject> gameobjList = new List<GameObject>();
            for (int i = 0; i < NumberOfChildren; i++)
            {
                GameObject ChildElement = UnityEngine.Object.Instantiate(Child) as GameObject;
                ChildElement.transform.SetParent(Parent.transform, worldPositionStays);
                ChildElement.SetActive(isDisabled);
                gameobjList.Add(ChildElement);
                ChildElement.name = name + "_" + i.ToString();
            }

            return gameobjList;
        }

        public static void SetActiveWithChildren(this GameObject parent, bool state)
        {
            parent.SetActive(state);
            foreach (var item in parent.GetAllChildren())
            {
                item.SetActive(state);
            }
        }

        public static void DestroyChildren(this GameObject Parent)
        {
            var children = Parent.GetFirstLevelChildren();
            foreach (var item in children)
            {
                UnityEngine.Object.Destroy(item);
            }
        }


        public static void ChangeLayersRecursively(this GameObject obj, string name)
        {
            obj.layer = LayerMask.NameToLayer(name);
            foreach (GameObject child in obj.GetAllChildren())
            {
                child.layer = LayerMask.NameToLayer(name);
            }
        }

        public static List<GameObject> GetAllChildren(this GameObject obj)
        {
            List<GameObject> children = new List<GameObject>();

            foreach (Transform child in obj.transform)
            {
                children.Add(child.gameObject);
                children.AddRange(GetAllChildren(child.gameObject));
            }

            return children;
        }

        public static T FindComponentInAllChildren<T>(this GameObject obj) where T : Component
        {
            var allChilds = obj.GetAllChildren();

            foreach (var child in allChilds)
            {
                T comp = child.GetComponent<T>();

                if (comp != null)
                    return comp;
            }

            return null;
        }

        public static List<GameObject> GetFirstLevelChildren(this GameObject obj)
        {
            List<GameObject> children = new List<GameObject>();
            foreach (Transform child in obj.transform)
            {
                children.Add(child.gameObject);
            }

            return children;
        }

        public static List<GameObject> FindGameObjectsInChildren(this GameObject obj,
            [CanBeNull] params string[] childName)
        {
            List<GameObject> children = GetAllChildren(obj);
            List<GameObject> results = new List<GameObject>();

            if (childName != null)
            {
                foreach (var item in childName)
                {
                    var child = children.Find(x => string.Equals(x.name, item));
                    if (child != null)
                        results.Add(child);
                }
            }
            else
            {
                results = children;
            }

            return results;
        }

        /// <summary>
        /// Find Components, event the inactive ones
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> FindObjectsOfTypeIncludingInactive<T>() where T : Component
        {
            List<T> foundObjects = new List<T>();

            foreach (T component in Resources.FindObjectsOfTypeAll<T>().ToList())
            {
                if (component.hideFlags == HideFlags.NotEditable || component.hideFlags == HideFlags.HideAndDontSave)
                    continue;

                if (component.gameObject.scene.rootCount == 0)
                    continue;

                foundObjects.Add(component);
            }

            return foundObjects;
        }

        public static float GetLeftEdge(this Transform t)
        {
            return (t.GetComponent<Collider>().bounds.center - t.GetComponent<Collider>().bounds.extents).x;
        }

        public static float GetRightEdge(this Transform t)
        {
            return (t.GetComponent<Collider>().bounds.center + t.GetComponent<Collider>().bounds.extents).x;
        }

        public static float GetWorldLeftEdge(this Camera t)
        {
            return t.ViewportToWorldPoint(Vector3.zero).x;
        }

        public static float GetWorldRightEdge(this Camera t)
        {
            return t.ViewportToWorldPoint(Vector3.one).x;
        }

        public static float GetWorldTopEdge(this Camera t)
        {
            return t.ViewportToWorldPoint(Vector3.one).y;
        }

        public static float GetWorldBottomEdge(this Camera t)
        {
            return t.ViewportToWorldPoint(Vector3.zero).y;
        }

        public static void ChangeX(this Transform t, float x)
        {
            t.position = new Vector3(x, t.position.y, t.position.z);
        }

        public static void ChangeY(this Transform t, float y)
        {
            t.position = new Vector3(t.position.x, y, t.position.z);
        }

        public static void ChangeZ(this Transform t, float z)
        {
            t.position = new Vector3(t.position.x, t.position.y, z);
        }

        public static void LocalChangeX(this Transform t, float x)
        {
            t.localPosition = new Vector3(x, t.localPosition.y, t.localPosition.z);
        }

        public static void LocalChangeY(this Transform t, float y)
        {
            t.localPosition = new Vector3(t.localPosition.x, y, t.localPosition.z);
        }

        public static void LocalChangeZ(this Transform t, float z)
        {
            t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y, z);
        }

        public static float GetXPos(this Transform t)
        {
            return t.position.x;
        }

        public static float GetYPos(this Transform t)
        {
            return t.position.y;
        }

        public static float GetZPos(this Transform t)
        {
            return t.position.z;
        }

        public static float GetXLocalPos(this Transform t)
        {
            return t.localPosition.x;
        }

        public static float GetYLocalPos(this Transform t)
        {
            return t.localPosition.y;
        }

        public static float GetZLocalPos(this Transform t)
        {
            return t.localPosition.z;
        }

        public static void AddToX(this Transform t, float x)
        {
            t.position = new Vector3(t.position.x + x, t.position.y, t.position.z);
        }

        public static void AddToY(this Transform t, float y)
        {
            t.position = new Vector3(t.position.x, t.position.y + y, t.position.z);
        }

        public static void AddToZ(this Transform t, float z)
        {
            t.position = new Vector3(t.position.x, t.position.y, t.position.z + z);
        }

        public static float GetWorldWidth(this Camera t)
        {
            return t.ViewportToWorldPoint(Vector3.one).x;
        }

        public static float GetWorldWidth(this Transform obj)
        {
            return obj.GetComponent<Renderer>().bounds.size.x;
        }

        public static float GetWorldHeight(this Transform obj)
        {
            return obj.GetComponent<Renderer>().bounds.size.y;
        }

        public static float GetHeight(this RectTransform obj)
        {
            return obj.rect.size.y;
        }

        public static float GetWidth(this RectTransform obj)
        {
            return obj.rect.size.x;
        }

        public static Vector3 ReflectAlongXAxis(this Vector3 obj, Vector3 origin)
        {
            var distance = Mathf.Abs(origin.x - obj.x);
            var newX = Mathf.Abs((origin.x + distance) - obj.x) < 1
                ? origin.x - distance
                : origin.x + distance;
            return new Vector3(newX, obj.y, obj.z);
        }

        public static Vector3 ReflectAlongYAxis(this Vector3 obj, Vector3 origin)
        {
            var distance = Mathf.Abs(origin.y - obj.y);
            var newY = Mathf.Abs((origin.y + distance) - obj.y) < 1
                ? origin.y - distance
                : origin.y + distance;
            return new Vector3(obj.x, newY, obj.z);
        }

        public static Vector3 ReflectAlongZAxis(this Vector3 obj, Vector3 origin)
        {
            var distance = Mathf.Abs(origin.z - obj.z);
            var newZ = Mathf.Abs((origin.z + distance) - obj.z) < 1
                ? origin.z - distance
                : origin.z + distance;
            return new Vector3(obj.x, obj.y, newZ);
        }

        /// <summary>
        /// Get relative position of a rectTransform compared with another
        /// </summary>
        /// <param name="thisRT"></param>
        /// <param name="relativeTo"></param>
        /// <returns></returns>
        public static Vector2 RelativeRectTransformPosition(this RectTransform thisRT, RectTransform relativeTo)
        {
            Vector2 localPoint;
            Vector2 screenP = RectTransformUtility.WorldToScreenPoint(null, thisRT.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(relativeTo, screenP, null, out localPoint);

            return localPoint;
        }

        public static List<ValueNamePair> GetValidFields(object obj)
        {
            var valueNamePair = new List<ValueNamePair>();

            var value = obj.GetType()
                .GetFields()
                .Select(field =>
                {
                    var _value = field.GetValue(obj);

                    return !_value.Equals(GetDefault(_value.GetType())) ? _value : null;
                })
                .Where(x => x != null)
                .ToList();

            var name = obj.GetType()
                .GetFields()
                .Select(field =>
                {
                    var _value = field.GetValue(obj);
                    var fieldName = field.Name;

                    return !_value.Equals(GetDefault(_value.GetType())) ? fieldName : null;
                })
                .Where(x => x != null)
                .ToList();

            for (int i = 0; i < value.Count; i++)
            {
                valueNamePair.Add(new ValueNamePair(name[i], value[i]));
            }

            return valueNamePair;
        }

        public static object GetDefault(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        public static void SetAnchor(this RectTransform source, AnchorPresets allign, int offsetX = 0, int offsetY = 0)
        {
            source.anchoredPosition = new Vector3(offsetX, offsetY, 0);

            switch (allign)
            {
                case (AnchorPresets.TopLeft):
                {
                    source.anchorMin = new Vector2(0, 1);
                    source.anchorMax = new Vector2(0, 1);
                    break;
                }
                case (AnchorPresets.TopCenter):
                {
                    source.anchorMin = new Vector2(0.5f, 1);
                    source.anchorMax = new Vector2(0.5f, 1);
                    break;
                }
                case (AnchorPresets.TopRight):
                {
                    source.anchorMin = new Vector2(1, 1);
                    source.anchorMax = new Vector2(1, 1);
                    break;
                }

                case (AnchorPresets.MiddleLeft):
                {
                    source.anchorMin = new Vector2(0, 0.5f);
                    source.anchorMax = new Vector2(0, 0.5f);
                    break;
                }
                case (AnchorPresets.MiddleCenter):
                {
                    source.anchorMin = new Vector2(0.5f, 0.5f);
                    source.anchorMax = new Vector2(0.5f, 0.5f);
                    break;
                }
                case (AnchorPresets.MiddleRight):
                {
                    source.anchorMin = new Vector2(1, 0.5f);
                    source.anchorMax = new Vector2(1, 0.5f);
                    break;
                }

                case (AnchorPresets.BottomLeft):
                {
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(0, 0);
                    break;
                }
                case (AnchorPresets.BottonCenter):
                {
                    source.anchorMin = new Vector2(0.5f, 0);
                    source.anchorMax = new Vector2(0.5f, 0);
                    break;
                }
                case (AnchorPresets.BottomRight):
                {
                    source.anchorMin = new Vector2(1, 0);
                    source.anchorMax = new Vector2(1, 0);
                    break;
                }

                case (AnchorPresets.HorStretchTop):
                {
                    source.anchorMin = new Vector2(0, 1);
                    source.anchorMax = new Vector2(1, 1);
                    break;
                }
                case (AnchorPresets.HorStretchMiddle):
                {
                    source.anchorMin = new Vector2(0, 0.5f);
                    source.anchorMax = new Vector2(1, 0.5f);
                    break;
                }
                case (AnchorPresets.HorStretchBottom):
                {
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(1, 0);
                    break;
                }

                case (AnchorPresets.VertStretchLeft):
                {
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(0, 1);
                    break;
                }
                case (AnchorPresets.VertStretchCenter):
                {
                    source.anchorMin = new Vector2(0.5f, 0);
                    source.anchorMax = new Vector2(0.5f, 1);
                    break;
                }
                case (AnchorPresets.VertStretchRight):
                {
                    source.anchorMin = new Vector2(1, 0);
                    source.anchorMax = new Vector2(1, 1);
                    break;
                }

                case (AnchorPresets.StretchAll):
                {
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(1, 1);
                    break;
                }
            }
        }

        public static Sprite GetSprite(this Texture2D tex)
        {
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.Tight);
            sprite.name = tex.name;
            sprite.texture.name = tex.name;
            return sprite;
        }

        public static void SetPivot(this RectTransform source, PivotPresets preset)
        {
            switch (preset)
            {
                case (PivotPresets.TopLeft):
                {
                    source.pivot = new Vector2(0, 1);
                    break;
                }
                case (PivotPresets.TopCenter):
                {
                    source.pivot = new Vector2(0.5f, 1);
                    break;
                }
                case (PivotPresets.TopRight):
                {
                    source.pivot = new Vector2(1, 1);
                    break;
                }

                case (PivotPresets.MiddleLeft):
                {
                    source.pivot = new Vector2(0, 0.5f);
                    break;
                }
                case (PivotPresets.MiddleCenter):
                {
                    source.pivot = new Vector2(0.5f, 0.5f);
                    break;
                }
                case (PivotPresets.MiddleRight):
                {
                    source.pivot = new Vector2(1, 0.5f);
                    break;
                }

                case (PivotPresets.BottomLeft):
                {
                    source.pivot = new Vector2(0, 0);
                    break;
                }
                case (PivotPresets.BottomCenter):
                {
                    source.pivot = new Vector2(0.5f, 0);
                    break;
                }
                case (PivotPresets.BottomRight):
                {
                    source.pivot = new Vector2(1, 0);
                    break;
                }
            }
        }
    }
}