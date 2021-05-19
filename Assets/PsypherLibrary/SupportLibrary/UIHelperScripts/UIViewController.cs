using System;
using System.Collections.Generic;
using System.Linq;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace PsypherLibrary.SupportLibrary.UIHelperScripts
{
    public enum UIType
    {
        StackedView,
        NormalView,
        BaseView
    }

    public class UIViewController : MonoBehaviour
    {
        [Serializable]
        public class ViewInformation
        {
            public UIType mType;
            public UIPanel mPanel;

            public ViewInformation()
            {
            }

            public ViewInformation(UIPanel panel, UIType type)
            {
                this.mType = type;
                this.mPanel = panel;
            }

            public override bool Equals(object otherObj)
            {
                var other = (ViewInformation) otherObj;
                if (other.mPanel == this.mPanel)
                    return true;
                else
                {
                    return false;
                }
            }

            public override int GetHashCode()
            {
                return mPanel.GetHashCode();
            }
        }

        public bool UseHardwareBack;

        public LinkedList<ViewInformation> mPanelStack = new LinkedList<ViewInformation>();

        public Action OnLandedOnLastPanel;
        public UnityEvent LandedOnLastPanel;

        public Action OnPressBackOnLastPanel;
        public UnityEvent PressedBackOnLastPanel;

        public Action OnPressBack;
        public UnityEvent PressBack;

        public Action<Vector2, GameObject> OnMouseDownOnObject;

        [SerializeField] protected ViewInformation _currentUI = new ViewInformation();

        public ViewInformation CurrentUI
        {
            get { return _currentUI; }
        }

        private ViewInformation _lastClosedPanel = new ViewInformation();

        public ViewInformation LastClosedPanel
        {
            get { return _lastClosedPanel; }
        }

        void OnEnable()
        {
            if (UseHardwareBack)
            {
                HardwareInputManager.OnBack += Back;
            }

            HardwareInputManager.OnDown += OnMouseDown;
        }

        void OnDisable()
        {
            if (UseHardwareBack)
            {
                HardwareInputManager.OnBack -= Back;
            }

            HardwareInputManager.OnDown -= OnMouseDown;
        }

        void OnMouseDown(Vector2 mousePosition)
        {
            var current = EventSystem.current;
            var currentSelectedObject = current ? current.currentSelectedGameObject : null;

            OnMouseDownOnObject.SafeInvoke(mousePosition, currentSelectedObject);
        }

        public void Flush()
        {
            while (mPanelStack.Count != 0)
            {
                mPanelStack.Last().mPanel.DeactivatePanel();
                _lastClosedPanel = mPanelStack.Last();

                mPanelStack.RemoveLast();
            }

            //debug purpose
            _currentUI.mPanel = null;
            _currentUI.mType = 0;
        }

        public void Activate(UIPanel panel, UIType type = UIType.NormalView, Action callBack = null)
        {
            //if (mPanelStack.Count > 0 && panel.GetInstanceID() == mPanelStack.Last().mPanel.GetInstanceID())
            //   return; //early exit if trying to activate the same panel again
            //commented since the user should be able to activate the same panel again

            switch (type)
            {
                case UIType.NormalView:
                {
                    if (mPanelStack.Count > 0 && mPanelStack.Last().mType != UIType.BaseView) //excluding the base view
                    {
                        mPanelStack.Last().mPanel.DeactivatePanel();
                    }

                    break;
                }
                case UIType.StackedView:
                {
                    break;
                }
                case UIType.BaseView:
                {
                    OnLandedOnLastPanel.SafeInvoke();
                    LandedOnLastPanel.SafeInvoke();
                    break;
                }
            }

            Debug.Log("Activating a " + type + " Panel: " + panel.name);

            //debug purpose
            _currentUI.mPanel = panel;
            _currentUI.mType = type;

            if (mPanelStack.Contains(new ViewInformation(panel, type)))
            {
                mPanelStack.Remove(new ViewInformation(panel, type));
            }

            mPanelStack.AddLast(new ViewInformation(panel, type));

            panel.ActivatePanel();
            callBack.SafeInvoke();
        }

        #region activate helper functions

        public void ActivateWithStackView(UIPanel panel)
        {
            Activate(panel, UIType.StackedView);
        }

        public void ActivateWithNormalView(UIPanel panel)
        {
            Activate(panel);
        }

        public void ActivateBaseView(UIPanel panel)
        {
            Activate(panel, UIType.BaseView);
        }

        #endregion

        public void ShowBaseView()
        {
            while (mPanelStack.Count > 0 && mPanelStack.Last().mType != UIType.BaseView)
            {
                mPanelStack.Last().mPanel.DeactivatePanel();
                _lastClosedPanel = mPanelStack.Last();
                mPanelStack.RemoveLast();
            }

            //debug purpose
            _currentUI.mPanel = mPanelStack.Last().mPanel;
            _currentUI.mType = mPanelStack.Last().mType;

            //action to fire on landed on base view
            OnLandedOnLastPanel.SafeInvoke();
            LandedOnLastPanel.SafeInvoke();
        }

        public void Back()
        {
            _lastClosedPanel = null;

            if (mPanelStack.Count <= 0)
            {
                PressedBackOnLastPanel.SafeInvoke();
                OnPressBackOnLastPanel.SafeInvoke();
                return;
            }

            var panelInfo = mPanelStack.Last();

            //only process back if the gameObject is active
            if (!panelInfo.mPanel.isActiveAndEnabled) return;

            switch (panelInfo.mType)
            {
                case UIType.NormalView:
                {
                    Debug.Log("Deactivated A NormalView Panel: " + mPanelStack.Last().mPanel.name);
                    mPanelStack.Last().mPanel.DeactivatePanel();
                    _lastClosedPanel = mPanelStack.Last();
                    mPanelStack.RemoveLast();
                    if (mPanelStack.Count > 0)
                    {
                        var peekPanel = mPanelStack.Last(); //.mPanel.ActivatePanel();
                        peekPanel.mPanel.ActivatePanel();

                        if (peekPanel.mType == UIType.BaseView && mPanelStack.Count == 1)
                        {
                            OnLandedOnLastPanel.SafeInvoke();
                            LandedOnLastPanel.SafeInvoke();
                        }

                        //debug purpose
                        _currentUI.mPanel = peekPanel.mPanel;
                        _currentUI.mType = peekPanel.mType;
                    }
                    else
                    {
                        //debug purpose
                        _currentUI.mPanel = null;
                        _currentUI.mType = 0;
                    }


                    break;
                }
                case UIType.StackedView:
                {
                    Debug.Log("Deactivated a StackedView Panel: " + mPanelStack.Last().mPanel.name);
                    mPanelStack.Last().mPanel.DeactivatePanel();
                    _lastClosedPanel = mPanelStack.Last();
                    mPanelStack.RemoveLast();

                    if (mPanelStack.Count == 1)
                    {
                        OnLandedOnLastPanel.SafeInvoke();
                        LandedOnLastPanel.SafeInvoke();
                    }

                    //debug purpose
                    var peekPanel = mPanelStack.Last();

                    _currentUI.mPanel = peekPanel.mPanel;
                    _currentUI.mType = peekPanel.mType;

                    break;
                }
                case UIType.BaseView:
                {
                    Debug.Log("Landed Base View");
                    ShowBaseView();

                    PressedBackOnLastPanel.SafeInvoke();
                    OnPressBackOnLastPanel.SafeInvoke();
                    break;
                }
            }

            OnPressBack.SafeInvoke();
            PressBack.SafeInvoke();
        }
    }
}