using System;
using System.Collections.Generic;
using DG.Tweening;
using PsypherLibrary.SupportLibrary.Extensions;
using UnityEngine;

namespace PsypherLibrary.SupportLibrary.Utils.UI
{
    public class ParallaxUGUI : MonoBehaviour
    {
        public float Speed = 1;
        public bool StartParallax = false;
        public List<ParallaxElement> Layers;

        [HideInInspector]
        public Bounds ScreenBounds;


        void Start()
        {
            Rect parentRect = GetComponent<RectTransform>().rect;
            ScreenBounds = new Bounds(parentRect.center, parentRect.size);

            foreach (var layer in Layers)
            {
                layer.Init(this);
            }
        }

        void Update()
        {
            if (StartParallax)
            {
                float t = Time.deltaTime * Speed;
                foreach (var layer in Layers)
                {
                    layer.Update(t);
                }
            }
        }
    }

    [Serializable]
    public class ParallaxElement
    {
        public enum RepositionModes
        {
            InfiniteScroll,
            RandomScroll
        }

        public GameObject Layer;
        public RepositionModes RepositionMode;
        public float SpeedRatio = 1;

        float MinEdge, MaxEdge;
        public bool isHorizontal = true;
        private RectTransform ScreenRect;
        private List<GameObject> ObjectsInLayer;


        public Vector3 NewPosition(RectTransform parallaxObjectRect, Bounds parallaxObjectBounds)
        {
            if (isHorizontal)
                return SpeedRatio > 0 ? new Vector3((MinEdge - (parallaxObjectBounds.size.x / 2)), parallaxObjectRect.anchoredPosition3D.y, parallaxObjectRect.anchoredPosition3D.z) : new Vector3((MaxEdge + (parallaxObjectBounds.size.x / 2)), parallaxObjectRect.anchoredPosition3D.y, parallaxObjectRect.anchoredPosition3D.z);
            else
                return SpeedRatio > 0 ? new Vector3(parallaxObjectRect.anchoredPosition3D.x, MinEdge - (parallaxObjectBounds.size.y / 2), parallaxObjectRect.anchoredPosition3D.z) : new Vector3(parallaxObjectRect.anchoredPosition3D.x, MaxEdge + (parallaxObjectBounds.size.y / 2), parallaxObjectRect.anchoredPosition3D.z);
        }

        public Vector3 NewRandomPosition(RectTransform parallaxObjectRect, Bounds parallaxObjectBounds)
        {
            if (isHorizontal)
            {
                var randY = UnityEngine.Random.Range(ScreenRect.rect.yMin + (parallaxObjectBounds.size.y / 2),
                    ScreenRect.rect.yMax - (parallaxObjectBounds.size.y / 2));
                return SpeedRatio > 0
                    ? new Vector3(
                        UnityEngine.Random.Range((MinEdge - ScreenRect.rect.width) + (parallaxObjectBounds.size.x / 2),
                            (MinEdge - (parallaxObjectBounds.size.x / 2))), randY, parallaxObjectRect.anchoredPosition3D.z)
                    : new Vector3(
                        UnityEngine.Random.Range((MaxEdge + (parallaxObjectBounds.size.x / 2)),
                            (MaxEdge + ScreenRect.rect.width) - (parallaxObjectBounds.size.x / 2)), randY,
                        parallaxObjectRect.anchoredPosition3D.z);
            }
            else
            {
                var randX = UnityEngine.Random.Range(ScreenRect.rect.xMin + (parallaxObjectBounds.size.x / 2),
                    ScreenRect.rect.xMax - (parallaxObjectBounds.size.x / 2));
                return SpeedRatio > 0
                    ? new Vector3(randX, UnityEngine.Random.Range((MinEdge - (parallaxObjectBounds.size.y / 2)),
                        (MinEdge - ScreenRect.rect.height) - (parallaxObjectBounds.size.y / 2)), parallaxObjectRect.anchoredPosition3D.z)
                    : new Vector3(randX, UnityEngine.Random.Range(ScreenRect.rect.yMax + (parallaxObjectBounds.size.y / 2), (ScreenRect.rect.yMax + ScreenRect.rect.height) - parallaxObjectBounds.size.y), parallaxObjectRect.anchoredPosition3D.z);
            }
        }

        public void Init(ParallaxUGUI p)
        {
            if (isHorizontal)
            {
                MinEdge = p.ScreenBounds.min.x;
                MaxEdge = p.ScreenBounds.max.x;
            }
            else
            {
                MinEdge = p.ScreenBounds.min.y;
                MaxEdge = p.ScreenBounds.max.y;
            }

            ScreenRect = p.GetComponent<RectTransform>();
            ObjectsInLayer = Layer.GetFirstLevelChildren();
        }

        public void Update(float t)
        {
            if (isHorizontal)
            {
                foreach (GameObject obj in ObjectsInLayer)
                {
                    obj.transform.Translate(t * SpeedRatio, 0.0f, 0.0f);
                }
            }
            else
            {
                foreach (GameObject obj in ObjectsInLayer)
                {
                    obj.transform.Translate(0.0f, t * SpeedRatio, 0.0f);
                }
            }

            Reposition();
        }

        void Reposition()
        {
            for (int i = ObjectsInLayer.Count - 1; i >= 0; i--)
            {
                RectTransform parallaxObjectRect = ObjectsInLayer[i].GetComponent<RectTransform>();
                Bounds parallaxObjectBounds =
                    RectTransformUtility.CalculateRelativeRectTransformBounds(ObjectsInLayer[i].transform.parent.transform,
                        ObjectsInLayer[i].transform);


                switch (RepositionMode)
                {
                    case RepositionModes.InfiniteScroll:
                    {
                        if (isHorizontal)
                        {
                            if (SpeedRatio > 0 && parallaxObjectBounds.min.x >= MaxEdge)
                            {
                                parallaxObjectRect.anchoredPosition3D = NewPosition(parallaxObjectRect,
                                    parallaxObjectBounds);
                            }
                            else if (SpeedRatio < 0 && parallaxObjectBounds.max.x <= MinEdge)
                            {
                                parallaxObjectRect.anchoredPosition3D = NewPosition(parallaxObjectRect,
                                    parallaxObjectBounds);
                            }
                        }
                        else
                        {
                            if (SpeedRatio > 0 && parallaxObjectBounds.min.y >= MaxEdge)
                            {
                                parallaxObjectRect.anchoredPosition3D = NewPosition(parallaxObjectRect,
                                    parallaxObjectBounds);
                            }
                            else if (SpeedRatio < 0 && parallaxObjectBounds.max.y <= MinEdge)
                            {
                                parallaxObjectRect.anchoredPosition3D = NewPosition(parallaxObjectRect,
                                    parallaxObjectBounds);
                            }
                        }

                        break;
                    }
                    case RepositionModes.RandomScroll:
                    {
                        if (isHorizontal)
                        {
                            if (SpeedRatio > 0 && parallaxObjectBounds.min.x >= MaxEdge)
                            {
                                parallaxObjectRect.anchoredPosition3D = NewRandomPosition(parallaxObjectRect,
                                    parallaxObjectBounds);
                            }
                            else if (SpeedRatio < 0 && parallaxObjectBounds.max.x <= MinEdge)
                            {
                                parallaxObjectRect.anchoredPosition3D = NewRandomPosition(parallaxObjectRect,
                                    parallaxObjectBounds);
                            }
                        }
                        else
                        {
                            if (SpeedRatio > 0 && parallaxObjectBounds.min.y >= MaxEdge)
                            {
                                parallaxObjectRect.anchoredPosition3D = NewRandomPosition(parallaxObjectRect,
                                    parallaxObjectBounds);
                            }
                            else if (SpeedRatio < 0 && parallaxObjectBounds.max.y <= MinEdge)
                            {
                                parallaxObjectRect.anchoredPosition3D = NewRandomPosition(parallaxObjectRect,
                                    parallaxObjectBounds);
                            }
                        }

                        break;
                    }
                }
            }
        }
    }
}