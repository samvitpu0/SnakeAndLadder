using System;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.Utils.Generics;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PsypherLibrary.SupportLibrary.Managers
{
    public class HardwareInputManager : GenericManager<HardwareInputManager>
    {
        public enum SwipeDirection
        {
            NONE,
            UP,
            DOWN,
            LEFT,
            RIGHT
        };


        public static Action<SwipeDirection> OnSwipe; // Called every frame passing in the swipe, including if there is no swipe.
        public static Action<Vector2> OnClick; // Called when Fire1 is released and it's not a double click.
        public static Action<Vector2> OnDown; // Called when Fire1 is pressed.
        public static Action<Vector2> OnUp; // Called when Fire1 is released.
        public static Action<Vector2> OnDoubleClick; // Called when a double click is detected.
        public static Action<Vector2> OnDrag; //called when drag is detected
        public static Action OnCancel; // Called when Cancel is pressed.
        public static Action OnBack; // Called when Back is pressed.

        [SerializeField] private float m_DoubleClickTime = 0.3f; //The max time allowed between double clicks

        [SerializeField] private float m_SwipeWidth = 0.3f; //The width of a swipe
        [SerializeField] private float m_DragThreshold = 0.3f; //The threshold after which the drag is recognised


        private Vector2 m_MouseDownPosition; // The screen position of the mouse when Fire1 is pressed.
        private Vector2 m_MouseUpPosition; // The screen position of the mouse when Fire1 is released.
        private Vector2 m_MouseDragPoition; // The screen position of the mouse during drag.
        private float m_LastMouseUpTime; // The time when Fire1 was last released.

        private float
            m_LastHorizontalValue; // The previous value of the horizontal axis used to detect keyboard swipes.

        private float m_LastVerticalValue; // The previous value of the vertical axis used to detect keyboard swipes.

        public float DoubleClickTime
        {
            get { return m_DoubleClickTime; }
        }

        private bool _isPointerDown;
        private bool _isDraggable;

        /// <summary>
        /// Gets the value, if the touch/mouse pointer is on any UI elements
        /// </summary>
        public bool IsPointerOverUIElements { get; private set; }

        /// <summary>
        /// GameObject under the Pointer
        /// </summary>
        public GameObject SelectableUnderPointer;

        //current event system
        private EventSystem _eventSystem;

        private void Update()
        {
            GetEventSystem(); //called here as event system might initialize after awake or start

            GetSystemStatus();
            CheckInput();
        }


        private void GetEventSystem()
        {
            if (_eventSystem != null) return;

            _eventSystem = EventSystem.current;
        }

        private void CheckInput()
        {
            // Set the default swipe to be none.
            SwipeDirection swipe = SwipeDirection.NONE;

            if (Input.GetMouseButtonDown(0))
            {
                // When Fire1 is pressed record the position of the mouse.
                m_MouseDownPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

                _isPointerDown = true;
                SelectableUnderPointer = _eventSystem.currentSelectedGameObject;

                OnDown.SafeInvoke(m_MouseDownPosition);
            }

            // This if statement is to gather information about the mouse when the button is up.
            if (Input.GetMouseButtonUp(0))
            {
                // When Fire1 is released record the position of the mouse.
                m_MouseUpPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

                // Detect the direction between the mouse positions when Fire1 is pressed and released.
                swipe = DetectSwipe();

                _isPointerDown = false;
                _isDraggable = false;
            }

            // If there was no swipe this frame from the mouse, check for a keyboard swipe.
            if (swipe == SwipeDirection.NONE)
            {
                swipe = DetectKeyboardEmulatedSwipe();
            }

            // If there are any subscribers to OnSwipe call it passing in the detected swipe.
            OnSwipe.SafeInvoke(swipe);

            // This if statement is to trigger events based on the information gathered before.
            if (Input.GetMouseButtonUp(0))
            {
                OnUp.SafeInvoke(m_MouseUpPosition);

                // If the time between the last release of Fire1 and now is less
                // than the allowed double click time then it's a double click.
                if (Time.time - m_LastMouseUpTime < m_DoubleClickTime)
                {
                    OnDoubleClick.SafeInvoke(m_MouseDownPosition);
                }
                else
                {
                    OnClick.SafeInvoke(m_MouseDownPosition);
                }

                // Record the time when Fire1 is released.
                m_LastMouseUpTime = Time.time;

                //setting the current selectable to null once the pointer operations are done
                SelectableUnderPointer = null;
            }

            // If the Cancel button is pressed and there are subscribers to OnCancel call it.
            if (Input.GetButtonDown("Cancel"))
            {
                if (OnCancel != null)
                    OnCancel();
            }

            // If the Back button is pressed and there are subscribers to OnBack call it.
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (OnBack != null)
                    OnBack();
            }

            if (_isPointerDown)
            {
                if (Mathf.Abs(m_MouseDownPosition.magnitude - Input.mousePosition.magnitude) > m_DragThreshold)
                {
                    _isDraggable = true;
                }
                else
                {
                    _isDraggable = false;
                }
            }

            if (_isDraggable)
            {
                m_MouseDragPoition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                OnDrag.SafeInvoke(m_MouseDragPoition);
            }
        }

        void GetSystemStatus()
        {
            if (_eventSystem)
            {
                IsPointerOverUIElements = _eventSystem.IsPointerOverGameObject();
            }
        }


        private SwipeDirection DetectSwipe()
        {
            // Get the direction from the mouse position when Fire1 is pressed to when it is released.
            Vector2 swipeData = (m_MouseUpPosition - m_MouseDownPosition).normalized;

            // If the direction of the swipe has a small width it is vertical.
            bool swipeIsVertical = Mathf.Abs(swipeData.x) < m_SwipeWidth;

            // If the direction of the swipe has a small height it is horizontal.
            bool swipeIsHorizontal = Mathf.Abs(swipeData.y) < m_SwipeWidth;

            // If the swipe has a positive y component and is vertical the swipe is up.
            if (swipeData.y > 0f && swipeIsVertical)
                return SwipeDirection.UP;

            // If the swipe has a negative y component and is vertical the swipe is down.
            if (swipeData.y < 0f && swipeIsVertical)
                return SwipeDirection.DOWN;

            // If the swipe has a positive x component and is horizontal the swipe is right.
            if (swipeData.x > 0f && swipeIsHorizontal)
                return SwipeDirection.RIGHT;

            // If the swipe has a negative x component and is vertical the swipe is left.
            if (swipeData.x < 0f && swipeIsHorizontal)
                return SwipeDirection.LEFT;

            // If the swipe meets none of these requirements there is no swipe.
            return SwipeDirection.NONE;
        }


        private SwipeDirection DetectKeyboardEmulatedSwipe()
        {
            // Store the values for Horizontal and Vertical axes.
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            // Store whether there was horizontal or vertical input before.
            bool noHorizontalInputPreviously = Mathf.Abs(m_LastHorizontalValue) < float.Epsilon;
            bool noVerticalInputPreviously = Mathf.Abs(m_LastVerticalValue) < float.Epsilon;

            // The last horizontal values are now the current ones.
            m_LastHorizontalValue = horizontal;
            m_LastVerticalValue = vertical;

            // If there is positive vertical input now and previously there wasn't the swipe is up.
            if (vertical > 0f && noVerticalInputPreviously)
                return SwipeDirection.UP;

            // If there is negative vertical input now and previously there wasn't the swipe is down.
            if (vertical < 0f && noVerticalInputPreviously)
                return SwipeDirection.DOWN;

            // If there is positive horizontal input now and previously there wasn't the swipe is right.
            if (horizontal > 0f && noHorizontalInputPreviously)
                return SwipeDirection.RIGHT;

            // If there is negative horizontal input now and previously there wasn't the swipe is left.
            if (horizontal < 0f && noHorizontalInputPreviously)
                return SwipeDirection.LEFT;

            // If the swipe meets none of these requirements there is no swipe.
            return SwipeDirection.NONE;
        }


        protected override void RefreshStaticOnDestroy()
        {
            base.RefreshStaticOnDestroy();
            // Ensure that all events are unsubscribed when this is destroyed.
            OnSwipe = null;
            OnClick = null;
            OnDoubleClick = null;
            OnDown = null;
            OnUp = null;
        }
    }
}