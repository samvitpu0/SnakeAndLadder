using System.Linq;
using PsypherLibrary.SupportLibrary.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace PsypherLibrary.SupportLibrary.UIHelperScripts
{
    [RequireComponent(typeof(ScrollRect))]
    public class UIScrollableGrid : UIScrollable
    {
        GridLayoutGroup gridLayoutgroup;

        GridLayoutGroup GetGridLayoutGroup
        {
            get
            {
                if (gridLayoutgroup == null)
                {
                    gridLayoutgroup = GetScrollRect.content.GetComponent<GridLayoutGroup>();
                    if (gridLayoutgroup == null)
                        gridLayoutgroup = GetScrollRect.content.gameObject.AddComponent<GridLayoutGroup>();
                }

                return gridLayoutgroup;
            }
        }

        public void SetDir(int moveDir)
        {
            _dir = moveDir;
        }

        protected override void CalculateContentHolder()
        {
            _spacing = GetScrollRect.content.GetComponent<GridLayoutGroup>().spacing;

            _elementWidth = GetGridLayoutGroup.cellSize.x + _spacing.x;

            _elementHeight = GetGridLayoutGroup.cellSize.y + _spacing.y;

            var fWidth = GetScrollRect.content.rect.width;
            var fheight = GetScrollRect.content.rect.height;

            if (GetScrollRect.vertical)
            {
                _numberOfRows = Mathf.FloorToInt(fheight / _elementHeight);
                if (GetGridLayoutGroup.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
                    _numberOfColumns = GetGridLayoutGroup.constraintCount;
                else
                    _numberOfColumns = Mathf.FloorToInt(fWidth / _elementWidth);
            }

            else
            {
                _numberOfColumns = Mathf.FloorToInt(fWidth / _elementWidth);
                if (GetGridLayoutGroup.constraint == GridLayoutGroup.Constraint.FixedRowCount)
                    _numberOfRows = GetGridLayoutGroup.constraintCount;
                else
                    _numberOfRows = Mathf.FloorToInt(fheight / _elementHeight);
            }
        }

        protected override float SetViewPortSize()
        {
            float numberOfGameobjects = 0;
            //width of the each item of the scrollrect 


            if (GetScrollRect.vertical)
            {
                GetScrollRect.content.SetAnchor(AnchorPresets.HorStretchTop);
                GetScrollRect.content.sizeDelta = new Vector2(GetScrollRect.content.sizeDelta.x, _elementHeight * Mathf.CeilToInt(((float) _totalGameobjects / _numberOfColumns)));
                GetScrollRect.content.GetComponent<GridLayoutGroup>().startAxis = GridLayoutGroup.Axis.Horizontal;

                numberOfGameobjects = _numberOfColumns *
                    _numberOfRows + (_numberOfColumns * _gameObjectBuffer);
            }

            else
            {
                GetScrollRect.content.SetAnchor(AnchorPresets.VertStretchLeft);
                GetScrollRect.content.sizeDelta = new Vector2(_elementWidth * Mathf.CeilToInt(((float) _totalGameobjects / _numberOfRows)), GetScrollRect.content.sizeDelta.y);
                GetScrollRect.content.GetComponent<GridLayoutGroup>().startAxis = GridLayoutGroup.Axis.Vertical;

                numberOfGameobjects = _numberOfColumns *
                    _numberOfRows + (_numberOfRows * _gameObjectBuffer);
            }

            return numberOfGameobjects;
        }

        protected override void DoNullChecks()
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

            if (GetScrollRect.content == null)
            {
                Debug.Log("container not set");
                return;
            }

            var layoutGrp = GetScrollRect.content.GetComponent<GridLayoutGroup>();
            if (layoutGrp == null)
            {
                Debug.Log("Grid Layout Group Not Set");
                return;
            }

            Destroy(GetScrollRect.content.GetComponent<ContentSizeFitter>());
        }

        protected override void OnUpdate()
        {
            if (!_objList.Any())
                return;
            if (GetScrollRect.velocity.sqrMagnitude > 0)
            {
                if (GetScrollRect.velocity.y > 0 | GetScrollRect.velocity.x < 0)
                    _dir = -1;
                if (GetScrollRect.velocity.y < 0 | GetScrollRect.velocity.x > 0)
                    _dir = 1;
            }

            if (GetGridLayoutGroup.enabled)
            {
                var pos = _objList.Select(x => x.anchoredPosition3D).ToList();
                var size = _objList.Select(x => x.sizeDelta).ToList();
                GetScrollRect.content.GetComponent<GridLayoutGroup>().enabled = false;
                _objList.Each((x, i) => x.anchoredPosition3D = pos[i]);
                _objList.Each((x, i) => x.sizeDelta = size[i]);
            }

            var goToTake = GetScrollRect.vertical ? _numberOfColumns : _numberOfRows;
            if (_dir == -1)
            {
                var lastRow = _objList.TakeLast(goToTake).ToList();
                var firstRow = _objList.Take(goToTake).Where(x => !IsVisible(x)).ToList();
                int newIndex = 0;
                if (lastRow.Any() && lastRow.FindLastIndex(x => x.gameObject.activeInHierarchy) >= 0)
                    newIndex = lastRow.FindLast(x => x.gameObject.activeInHierarchy).name.ToInt() + 1;
                if (firstRow.Count == goToTake && newIndex < _totalGameobjects)
                {
                    for (int i = 0; i < lastRow.Count; i++)
                    {
                        firstRow[i].name = newIndex.ToString();
                        firstRow[i].anchoredPosition = GetScrollRect.vertical ? new Vector2(firstRow[i].anchoredPosition.x, lastRow[i].anchoredPosition.y - _elementHeight) : new Vector2(lastRow[i].anchoredPosition.x + _elementWidth, firstRow[i].anchoredPosition.y);
                        if (newIndex < _totalGameobjects)
                        {
                            firstRow[i].Activate();
                            _functionToApply.SafeInvoke(_data,
                                newIndex, firstRow[i].gameObject);
                        }
                        else
                            firstRow[i].Deactivate();

                        _objList.MoveItem(0, _objList.Count - 1);
                        newIndex++;
                    }
                }
            }
            else
            {
                var lastRow = _objList.TakeLast(goToTake).Where(x => !IsVisible(x)).ToList();
                var firstRow = _objList.Take(goToTake).ToList();
                int newIndex = 0;
                if (firstRow.Any() && firstRow.FindIndex(x => x.gameObject.activeInHierarchy) >= 0)
                    newIndex = firstRow.Find(x => x.gameObject.activeInHierarchy).name.ToInt() - 1;

                if (lastRow.Count == goToTake && newIndex >= 0)
                {
                    for (int i = firstRow.Count - 1; i >= 0; i--)
                    {
                        lastRow[i].name = newIndex.ToString();
                        lastRow[i].anchoredPosition = GetScrollRect.vertical ? new Vector2(lastRow[i].anchoredPosition.x, firstRow[i].anchoredPosition.y + _elementHeight) : new Vector2(firstRow[i].anchoredPosition.x - _elementWidth, lastRow[i].anchoredPosition.y);
                        if (newIndex >= 0)
                        {
                            lastRow[i].Activate();
                            _functionToApply.SafeInvoke(_data,
                                newIndex, lastRow[i].gameObject);
                        }
                        else
                            lastRow[i].Deactivate();

                        _objList.MoveItem(_objList.Count - 1, 0);
                        newIndex--;
                    }
                }
            }
        }
    }
}