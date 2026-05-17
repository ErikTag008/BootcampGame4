using Gilzoide.UpdateManager;
using KBCore.Refs;
using UnityEngine;

namespace Project.Assets._Project._Scripts.Interactables
{
    [RequireComponent(typeof(Rigidbody))]
    public class Block : ValidatedManagedBehaviour, IDragable, IFixedUpdatable
    {
        [SerializeField, Self] private Rigidbody _rb;
        [SerializeField, Self] private Collider[] _colliders;
        [SerializeField, Child] private MeshRenderer[] _renderers;
        [SerializeField] private RenderingLayerMask _noOutlineLayer;
        [SerializeField] private RenderingLayerMask _outlinedLayer;
        [SerializeField] private float _moveSpeed = 10f;
        private Bounds _bounds;
        private Vector3? _offset;
        private Vector3? _targetPosition;
        private bool _isGettingDragged = false;

        private void Awake()
        {
            UpdateBounds();
            ToggleOutLine(false);
        }
        public void ManagedFixedUpdate()
        {
            print($"_currentTagetPos: {_targetPosition}");
            print($"_offset: {_offset}");
            GoTowardsTargetPosition();
        }

        private void ToggleOutLine(bool isOn)
        {
            foreach(var rend in _renderers)
            {
                rend.renderingLayerMask = isOn ? _outlinedLayer : _noOutlineLayer;
            }
        }

        private void GoTowardsTargetPosition()
        {
            if (_targetPosition == null || !_isGettingDragged) return;
            print($"Going towards {_targetPosition}");
            var vector = _targetPosition - transform.position + _offset;
            if (vector == null) return;
            _rb.linearVelocity = (Vector3)(vector * _moveSpeed);
            //transform.position = (Vector3)(_targetPosition + _offset);
            UpdateBounds();
        }

        private void UpdateBounds()
        {
            if (_colliders == null || _colliders.Length == 0) return;
            _bounds = _colliders[0].bounds;
            for (int i = 1; i < _colliders.Length; i++)
            {
                _bounds.Encapsulate(_colliders[i].bounds);
            }
        }

        public void StartDrag(Vector3 offset)
        {
            offset.y = 0f;
            _offset = offset;
            _isGettingDragged = true;
            ToggleOutLine(true);
            UpdateBounds();
        }

        public void EndDrag(Vector3 finalPos)
        {
            UpdateBounds();
            ToggleOutLine(false);

            _rb.linearVelocity = Vector3.zero;
            finalPos.y = 0f;
            transform.position = finalPos;
            _isGettingDragged = false;
            _targetPosition = null;
            _offset = null;
        }

        

        public void UpdateDrag(Vector3 target)
        {
            target.y = transform.position.y;
            _targetPosition = target;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            UpdateBounds();
            Gizmos.DrawWireCube(_bounds.center, _bounds.size);
        }

        
#endif
    }
}
