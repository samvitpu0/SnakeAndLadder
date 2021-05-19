using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using PsypherLibrary.SupportLibrary.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace PsypherLibrary.SupportLibrary.UIHelperScripts
{
    [RequireComponent(typeof(ScrollRect))]
    public class UIScrollable : MonoBehaviour
    {
        public bool IgnoreLayoutGroup;

        private List<Vector2> _sizeBackup = new List<Vector2>();
        private List<Vector3> _positionBackup = new List<Vector3>();
        protected RectTransform _prefab;
        protected object _data;
        protected Action<object, int, GameObject> _functionToApply;
        protected int _totalGameobjects;
        protected List<RectTransform> _objList = new List<RectTransform>();
        protected int _dir;

        protected ScrollRect _scrollRect;
        protected Vector2 _spacing = Vector2.zero;
        public int _gameObjectBuffer = 5;

        protected int _numberOfRows;
        protected int _numberOfColumns;
        protected int _lastScrollIndex;
        protected float _elementWidth;
        protected float _elementHeight;

        public ScrollRect GetScrollRect
        {
            get
            {
                if (_scrollRect == null)
                {
                    _scrollRect = GetComponent<ScrollRect>();
                    if (_scrollRect == null)
                        _scrollRect = gameObject.AddComponent<ScrollRect>();
                }

                return _scrollRect;
            }
        }

        protected bool IsVisible(Component obj)
        {
            var screenBounds = new Bounds(GetScrollRect.viewport.GetComponent<RectTransform>().rect.center, GetScrollRect.viewport.GetComponent<RectTransform>().rect.size);
            Bounds myBounds =
                RectTransformUtility.CalculateRelativeRectTransformBounds(GetScrollRect.viewport.GetComponent<RectTransform>().transform,
                    obj.transform);
            bool isOutOfBounds = myBounds.max.x < screenBounds.min.x || myBounds.min.x > screenBounds.max.x ||
                                 myBounds.min.y > screenBounds.max.y || myBounds.max.y < screenBounds.min.y;
            return !isOutOfBounds;
        }

        void OnEnable()
        {
            if (_objList.Count == _positionBackup.Count && _objList.Count == _sizeBackup.Count)
            {
                _objList.Each((x, i) => x.anchoredPosition3D = _positionBackup[i]);
                _objList.Each((x, i) => x.sizeDelta = _sizeBackup[i]);
            }
        }

        void OnDisable()
        {
            _positionBackup.Clear();
            _sizeBackup.Clear();
            _positionBackup = _objList.Select(x => x.anchoredPosition3D).ToList();
            _sizeBackup = _objList.Select(x => x.sizeDelta).ToList();
        }

        void LateUpdate()
        {
            if (GetScrollRect.velocity.sqrMagnitude > 0)
            {
                OnUpdate();
            }
        }

        public UIScrollable SetPrefab(GameObject prefab)
        {
            _prefab = prefab.GetComponent<RectTransform>();
            return this;
        }

        public UIScrollable SetFunction(Action<object, int, GameObject> function)
        {
            _functionToApply = function;
            return this;
        }

        public UIScrollable UpdateData(object data)
        {
            SetData(data);
            Initialize();
            return this;
        }

        public UIScrollable SetData(object data)
        {
            _data = data;
            _totalGameobjects = ((ICollection) data).Count;

            return this;
        }

        public void ClearContent()
        {
            GetScrollRect.content.gameObject.DestroyChildren();
            _objList.Clear();
            GetScrollRect.StopMovement();
        }

        public void Refresh()
        {
            GetScrollRect.content.gameObject.GetFirstLevelChildren().Each((child, index) =>
            {
                var go = child;
                _functionToApply.SafeInvoke(_data, go.name.ToInt(), go);
            });
        }

        public List<RectTransform> GetContents()
        {
            return _objList;
        }

        public void ScrollToIndex(int index, float maxDuration = 3)
        {
            if (index < 0) return; //if index is less than 0, early exit

            GetScrollRect.StopMovement(); //stoping movement while scrolling

            if (DOTween.IsTweening(GetScrollRect.content))
                DOTween.Kill(GetScrollRect.content); //kill tween if already tweening
            float scrollPos = 0;
            float normalizedPos = 0;

            if (GetScrollRect.vertical)
            {
                scrollPos = index * _elementHeight - _elementHeight;
                normalizedPos = scrollPos / GetScrollRect.content.sizeDelta.y;
                _dir = normalizedPos > GetScrollRect.verticalNormalizedPosition ? -1 : 1;
                scrollPos = -1 * Mathf.Clamp(scrollPos, 0, GetScrollRect.content.sizeDelta.y);
                GetScrollRect.content.DOLocalMoveY(scrollPos, maxDuration).SetEase(Ease.InOutSine)
                    .SetAutoKill(true).OnUpdate(OnUpdate);
            }
            else
            {
                scrollPos = index * _elementWidth - _elementWidth;
                normalizedPos = scrollPos / GetScrollRect.content.sizeDelta.x;
                _dir = normalizedPos > GetScrollRect.horizontalNormalizedPosition ? -1 : 1;
                scrollPos = -1 * Mathf.Clamp(scrollPos, 0, GetScrollRect.content.sizeDelta.x);
                GetScrollRect.content.DOLocalMoveX(scrollPos, maxDuration).SetEase(Ease.InOutSine)
                    .SetAutoKill(true).OnUpdate(OnUpdate);
            }
        }

        protected virtual float SetViewPortSize()
        {
            float numberOfGameobjects = 0;

            if (!IgnoreLayoutGroup)
            {
                if (GetScrollRect.vertical)
                {
                    _spacing = new Vector2(0, GetScrollRect.content.GetComponent<HorizontalOrVerticalLayoutGroup>().spacing);
                    numberOfGameobjects = _numberOfRows + _gameObjectBuffer;
                    GetScrollRect.content.SetAnchor(AnchorPresets.HorStretchTop);
                    GetScrollRect.content.sizeDelta = new Vector2(GetScrollRect.viewport.sizeDelta.x, _elementHeight * _totalGameobjects + _totalGameobjects * _spacing.y / 2);
                }

                else
                {
                    _spacing = new Vector2(GetScrollRect.content.GetComponent<HorizontalOrVerticalLayoutGroup>().spacing, 0);
                    numberOfGameobjects = _numberOfColumns + _gameObjectBuffer;
                    GetScrollRect.content.SetAnchor(AnchorPresets.VertStretchLeft);
                    GetScrollRect.content.sizeDelta = new Vector2(_elementWidth * _totalGameobjects + _totalGameobjects * _spacing.x / 2, GetScrollRect.viewport.sizeDelta.y);
                }
            }
            else
            {
                _spacing = Vector2.zero;
                numberOfGameobjects = _gameObjectBuffer;
            }

            return numberOfGameobjects;
        }

        protected virtual void DoNullChecks()
        {
            if (_prefab == null)
            {
                Debug.Log("Prefab not set");
                return;
            }

            if (GetScrollRect.content == null)
            {
                Debug.Log("container not set");
                return;
            }

            var layoutGrp = GetScrollRect.content.GetComponent<HorizontalOrVerticalLayoutGroup>();
            if (layoutGrp == null && !IgnoreLayoutGroup)
            {
                Debug.Log("Layout Group Not Set");
                return;
            }

            if (!IgnoreLayoutGroup)
                Destroy(GetScrollRect.content.GetComponent<ContentSizeFitter>());
        }

        public bool Initialize()
        {
            DoNullChecks();
            ClearContent();
            CalculateContentHolder();

            //StartCoroutine(PopulateContent((int)SetViewPortSize()));
            PopulateContent((int) SetViewPortSize());

            return true;
        }

        protected virtual void OnUpdate()
        {
            if (!_objList.Any() || IgnoreLayoutGroup)
                return;

            if (GetScrollRect.velocity.sqrMagnitude > 0)
            {
                if (GetScrollRect.velocity.y > 0 | GetScrollRect.velocity.x < 0)
                    _dir = -1;
                if (GetScrollRect.velocity.y < 0 | GetScrollRect.velocity.x > 0)
                    _dir = 1;
            }

            if (GetScrollRect.content.GetComponent<HorizontalOrVerticalLayoutGroup>().enabled)
            {
                var pos = _objList.Select(x => x.anchoredPosition3D).ToList();
                var size = _objList.Select(x => x.sizeDelta).ToList();
                GetScrollRect.content.GetComponent<HorizontalOrVerticalLayoutGroup>().enabled = false;
                _objList.Each((x, i) => x.anchoredPosition3D = pos[i]);
                _objList.Each((x, i) => x.sizeDelta = size[i]);
            }

            var count = _objList.Count;
            do
            {
                if (_dir == -1)
                {
                    var lastgo = _objList[_objList.Count - 1];
                    var firstGo = _objList[0];
                    var newIndex = lastgo.gameObject.name.ToInt() + 1;


                    if (!IsVisible(firstGo) && newIndex < _totalGameobjects)
                    {
                        var gap = new Vector2(lastgo.rect.width + (GetScrollRect.vertical ? _spacing.y : _spacing.x), lastgo.rect.height + (GetScrollRect.vertical ? _spacing.y : _spacing.x));
                        firstGo.gameObject.name = newIndex.ToString();
                        firstGo.anchoredPosition = GetScrollRect.vertical ? new Vector2(firstGo.anchoredPosition.x, lastgo.anchoredPosition.y - gap.y) : new Vector2(lastgo.anchoredPosition.x + gap.x, firstGo.anchoredPosition.y);
                        _objList.MoveItem(0, _objList.Count - 1);
                        _functionToApply.SafeInvoke(_data,
                            newIndex, firstGo.gameObject);
                    }
                }
                else
                {
                    var lastgo = _objList[_objList.Count - 1];
                    var firstGo = _objList[0];
                    var newIndex = firstGo.name.ToInt() - 1;
                    if (!IsVisible(lastgo) && newIndex >= 0)
                    {
                        var gap = new Vector2(lastgo.rect.width + (GetScrollRect.vertical ? _spacing.y : _spacing.x), lastgo.rect.height + (GetScrollRect.vertical ? _spacing.y : _spacing.x));
                        lastgo.gameObject.name = newIndex.ToString();
                        lastgo.anchoredPosition = GetScrollRect.vertical ? new Vector2(lastgo.anchoredPosition.x, firstGo.anchoredPosition.y + gap.y) : new Vector2(firstGo.anchoredPosition.x - gap.x, lastgo.anchoredPosition.y);
                        _objList.MoveItem(_objList.Count - 1, 0);
                        _functionToApply.SafeInvoke(_data, newIndex
                            , lastgo.gameObject);
                    }
                }
            } while (count-- <= 0);
        }

        protected virtual void CalculateContentHolder()
        {
            if (!IgnoreLayoutGroup)
            {
                if (GetScrollRect.vertical)
                {
                    _spacing = new Vector2(0, GetScrollRect.content.GetComponent<HorizontalOrVerticalLayoutGroup>().spacing);
                }

                else
                {
                    _spacing = new Vector2(GetScrollRect.content.GetComponent<HorizontalOrVerticalLayoutGroup>().spacing, 0);
                }

                //width of the each item of the scrollrect 
                _elementWidth = _prefab.rect.width + (GetScrollRect.vertical ? _spacing.y : _spacing.x) + (GetScrollRect.content.GetComponent<LayoutGroup>().padding.left + GetScrollRect.content.GetComponent<LayoutGroup>().padding.right);

                //height of the each item of the scrollrect 
                _elementHeight = _prefab.rect.height + (GetScrollRect.vertical ? _spacing.y : _spacing.x) + (GetScrollRect.content.GetComponent<LayoutGroup>().padding.top + GetScrollRect.content.GetComponent<LayoutGroup>().padding.bottom);

                var fWidth = GetScrollRect.content.rect.width;
                var fheight = GetScrollRect.content.rect.height;

                _numberOfColumns = Mathf.FloorToInt(fWidth / _elementWidth);
                _numberOfRows = Mathf.FloorToInt(fheight / _elementHeight);
            }
            else
            {
                _spacing = Vector2.zero;
                _elementHeight = _prefab.rect.height;
                _elementWidth = _prefab.rect.width;
                _numberOfColumns = 1;
                _numberOfRows = 1;
            }
        }

        void PopulateContent(int numberOfGameobjects)
        {
            for (int i = 0; i < (numberOfGameobjects < _totalGameobjects ? numberOfGameobjects : _totalGameobjects); i++)
            {
                var go = GetScrollRect.content.gameObject.AddChild(_prefab.gameObject);
                _functionToApply.SafeInvoke(_data, i, go);
                go.name = i.ToString();
                _objList.Add(go.GetComponent<RectTransform>());
            }
        }
    }
}