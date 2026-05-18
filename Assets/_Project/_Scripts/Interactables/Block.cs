using DG.Tweening;
using Gilzoide.UpdateManager;
using KBCore.Refs;
using UnityEngine;

namespace Project.Assets._Project._Scripts.Interactables
{
    [RequireComponent(typeof(Rigidbody))]
    public class Block : ValidatedManagedBehaviour, IDragable, IFixedUpdatable
    {
        [SerializeField] private UnitColor _color;
        [SerializeField, Self] private Rigidbody _rb;
        [SerializeField, Self] private Collider[] _colliders;
        [SerializeField, Child] private MeshRenderer[] _renderers;
        [SerializeField] private RenderingLayerMask _noOutlineLayer;
        [SerializeField] private RenderingLayerMask _outlinedLayer;
        [SerializeField] private float _moveSpeed = 10f;
        [SerializeField] private float _maxSpeed = 20f;
        [SerializeField] private float _posLockInDuration = 0.1f;
        [SerializeField] private Ease _posLockInEase = Ease.OutCubic;


        private Bounds _bounds;
        private Vector3? _offset;
        private Vector3? _targetPosition;
        private bool _isGettingDragged = false;
        private Tween _posLockInTween;


        private void Awake()
        {
            UpdateBounds();
            ToggleOutLine(false);
            _rb.isKinematic = true;
            //_rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            //_rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
        public void ManagedFixedUpdate()
        {
            print($"_currentTagetPos: {_targetPosition}");
            print($"_offset: {_offset}");
            GoTowardsTargetPosition();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Exit"))
            {
                
            }
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
            if (_targetPosition == null || _offset == null || !_isGettingDragged) return;

            Vector3 desiredPosition = _targetPosition.Value + _offset.Value;
            Vector3 currentPosition = _rb.position;
            Vector3 difference = desiredPosition - currentPosition;

            float distance = difference.magnitude;

            if (distance < 0.02f)
            {
                _rb.linearVelocity = Vector3.zero;
                return;
            }
            Vector3 targetVelocity = difference * _moveSpeed;

            if (targetVelocity.magnitude > _maxSpeed)
            {
                targetVelocity = targetVelocity.normalized * _maxSpeed;
            }

            _rb.linearVelocity = targetVelocity;
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
            _posLockInTween?.Complete();
            _posLockInTween?.Kill();
            ToggleOutLine(true);
            _rb.isKinematic = false;
            UpdateBounds();
        }

        public void EndDrag(Vector3 finalPos)
        {
            UpdateBounds();
            ToggleOutLine(false);
            _rb.isKinematic = true;
            _rb.linearVelocity = Vector3.zero;
            finalPos.y = 0f;
            _posLockInTween?.Complete();
            _posLockInTween?.Kill();
            _posLockInTween = transform.DOMove(finalPos, _posLockInDuration)
                .SetEase(_posLockInEase);
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

public enum UnitColor
{
    Red,
    Yellow,
    Blue,
    Orange,
    Purple,
    Green
}