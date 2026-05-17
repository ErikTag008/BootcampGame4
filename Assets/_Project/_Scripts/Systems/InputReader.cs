using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Assets._Project._Scripts.Systems
{
    public class InputReader : MonoBehaviour
    {
        public event Action<Vector2> OnDragStart;
        public event Action<Vector2> OnDragEnd;
        public Vector2 DragPosition
        {
            get
            {
                if (Touchscreen.current != null) return Touchscreen.current.primaryTouch.position.ReadValue();
                if (Mouse.current != null) return Mouse.current.position.ReadValue();
                if (Pointer.current != null) return Pointer.current.position.ReadValue();
                return Vector2.zero;
            }
        }


        //private Vector2 SetDragPosition()
        //{
        //    Vector2 dragPos;
        //    if(Touchscreen.current != null)
        //    {
        //        dragPos = Touchscreen.current.primaryTouch.position.ReadValue();
        //    }
        //    else if(Mouse.current != null && Mouse.current.press.isPressed)
        //    {
        //        dragPos = Mouse.current.position.ReadValue();
        //    }
        //    else
        //    {
        //        dragPos = Pointer.current.position.ReadValue();
        //    }
        //    DragPosition = dragPos;
        //    return dragPos;
        //}

        //public void DragPositionContext(InputAction.CallbackContext context)
        //{
        //    DragPosition = context.ReadValue<Vector2>();
        //}


        public void Drag(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnDragStart?.Invoke(DragPosition);
                print("Drag Start!");
            }
            else if (context.canceled)
            {
                OnDragEnd?.Invoke(DragPosition);
                print("Drag End!");

            }
        }
    }
}
