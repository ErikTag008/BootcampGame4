using System;
using UnityEngine;

namespace Project.Assets._Project._Scripts.Interactables
{
    public interface IDragable
    {
        void StartDrag(Vector3 offset);
        void EndDrag();
        void UpdateDrag(Vector3 target);
        event Action OnExit;
    }
}
