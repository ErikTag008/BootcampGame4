using Gilzoide.UpdateManager;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Assets._Project._Scripts.Systems
{
    public class InputReader : AManagedBehaviour, IUpdatable
    {
        [Header("Input Config")]
        [SerializeField] private float _mouseScrollMultiplier = 100f;
        [SerializeField] private float _pinchDeadzone = 3f;
        public event Action OnTouch;
        public Vector2 TouchPosition { get; private set; } = Vector2.zero;
        public event Action<Vector2> OnDragStart;
        public event Action<Vector2> OnDragEnd;
        public event Action<float> OnZoom;
        public float PinchDelta { get; private set; }
        private float _previousPinchDistance = 0f;
        public Vector2 DragDelta { get; private set; } = Vector2.zero;
        private bool _isZooming = false;

        public void ManagedUpdate()
        {
            if (Touchscreen.current == null || Touchscreen.current.touches.Count < 2)
            {
                _previousPinchDistance = 0f;
                PinchDelta = Mouse.current?.scroll.ReadValue().y * _mouseScrollMultiplier ?? 0f;
                if (Mathf.Abs(PinchDelta) > 0.01f)
                {
                    OnZoom?.Invoke(PinchDelta);
                }
                return;
            }

            if (_isZooming)
                CheckPinch();
        }

        public void SecondaryTouchContact(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                _previousPinchDistance = GetCurrentDistanceBetween2Touches();
                _isZooming = true;
            }
            else if (context.canceled) 
            { 
                _isZooming = false; 
            }
        }

        public void CheckPinch()
        {
            float currentDistance = GetCurrentDistanceBetween2Touches();

            if (currentDistance != _previousPinchDistance)
            {
                PinchDelta = currentDistance - _previousPinchDistance;
                if (Mathf.Abs(PinchDelta) > _pinchDeadzone)
                {
                    OnZoom?.Invoke(PinchDelta);
                }
            }

            _previousPinchDistance = currentDistance;
        }

        private static float GetCurrentDistanceBetween2Touches()
        {
            var touch1 = Touchscreen.current.touches[0].position.ReadValue();
            var touch2 = Touchscreen.current.touches[1].position.ReadValue();

            float currentDistance = Vector2.Distance(touch1, touch2);
            return currentDistance;
        }

        public Vector2 SetDragDelta()
        {
            if (_isZooming) return Vector2.zero;
            if (Touchscreen.current != null)
            {
                DragDelta = Touchscreen.current.primaryTouch.delta.ReadValue();
            }
            else
            {
                DragDelta = Mouse.current.delta.ReadValue();
            }
            return DragDelta;
        }

        public void Drag(InputAction.CallbackContext context)
        {
            if (_isZooming) return;
            if (context.performed)
            {
                OnDragStart?.Invoke(SetDragDelta());
                print("Drag Start!");
            }
            else if (context.canceled)
            {
                OnDragEnd?.Invoke(SetDragDelta());
                print("Drag End!");

            }
        }

        public void Touch(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                if (context.control.device is Touchscreen)
                {
                    TouchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                }
                else if(Mouse.current.leftButton.isPressed)
                {
                    TouchPosition = Mouse.current.position.ReadValue();
                }
                else
                {
                    TouchPosition = Pointer.current.position.ReadValue();
                }
            }
            else if (context.performed)
            {
                print($"On Touch Performed");
                OnTouch?.Invoke();
            }
            
        }

        
    }
}
