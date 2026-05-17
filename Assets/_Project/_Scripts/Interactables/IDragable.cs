using UnityEngine;

namespace Project.Assets._Project._Scripts.Interactables
{
    public interface IDragable
    {
        void StartDrag(Vector3 offset);
        void EndDrag(Vector3 finalPos);
        void UpdateDrag(Vector3 target);
    }
}
