using DG.Tweening;
using Gilzoide.UpdateManager;
using KBCore.Refs;
using Project.Assets._Project._Scripts.GridComponents;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Assets._Project._Scripts.Interactables
{
    [RequireComponent(typeof(Rigidbody))]
    public class Block : ValidatedManagedBehaviour, IDragable, IFixedUpdatable
    {
        [SerializeField] private UnitColor _color;
        public UnitColor Color => _color;
        [SerializeField, Self] private Rigidbody _rb;
        [SerializeField, Self] private Collider[] _colliders;
        [SerializeField, Child] private MeshRenderer[] _renderers;
        [SerializeField] private RenderingLayerMask _noOutlineLayer;
        [SerializeField] private RenderingLayerMask _outlinedLayer;
        [SerializeField] private LayerMask _tileLayer;
        [SerializeField] private float _moveSpeed = 10f;
        [SerializeField] private float _maxSpeed = 20f;
        [SerializeField] private float _posLockInDuration = 0.1f;
        [SerializeField] private Ease _posLockInEase = Ease.OutCubic;
        [SerializeField] private float _exitDuration = 0.3f;
        [SerializeField] private float _exitDistance = 2.0f;
        [SerializeField] private Ease _exitEase = Ease.InOutCubic;
        [SerializeField] private Ease _mergeEase = Ease.InOutCubic;
        [SerializeField] private float _mergeDuration = 1f;
        private bool _canExit = true;
        private bool _hasExited = false;
        private bool _isMerged = false;
        private bool _isMerging = false;
        public event Action OnExit;
        public event Action<UnitColor, BlockType, Vector3, Quaternion> OnMerge; 
        public bool HasExited => _hasExited;
        [field: SerializeField] public bool HasTimeBonus { get; private set; } = false;
        [field: SerializeField, ShowIf.ShowIf("HasTimeBonus", true)] public int TimeBonusInSeconds { get; private set; } = 5;

        [field: SerializeField] public bool HasMoveDelay { get; private set; } = false;
        [field: SerializeField, ShowIf.ShowIf(nameof(HasMoveDelay), true)] public int MoveDelay { get; private set; } = 5;

        [field: SerializeField] public bool IsMergable { get; private set; } = false;
        [field: SerializeField, ShowIf.ShowIf(nameof(IsMergable), true)] public BlockType Type { get; private set; } = BlockType.OneByOne;
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
        }
        public void ManagedFixedUpdate()
        {
            GoTowardsTargetPosition();
        }

        private void OnCollisionEnter(Collision collision)
        {
            CheckMergeCollision(collision);
        }

        private void CheckMergeCollision(Collision collision)
        {
            if (collision.collider.TryGetComponent(out Block otherBlock) && otherBlock.IsMergable)
            {
                if (!_isMerged && otherBlock.CanMerge(_bounds, _color, Type, out var mergedResultColor))
                {
                    _isMerged = true;

                    var pos = transform.position + (otherBlock.transform.position - transform.position) / 2;
                    DOTween.Sequence()
                        .Join(otherBlock.GetMerged(pos, _mergeDuration))
                        .Join(GetMerged(pos + Vector3.one * 0.01f, _mergeDuration))
                        .AppendCallback(() =>
                        {
                            OnMerge?.Invoke(mergedResultColor, otherBlock.Type, pos, transform.rotation);
                        });

                }
                else
                {
                    print($"Cannot Merge, otherBlock = {otherBlock}, otherBlock.IsMergable = {otherBlock.IsMergable}, _isMerged = {_isMerged}");
                }
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            CheckMergeCollision(collision);
        }

        private void OnTriggerEnter(Collider other)
        {
            CheckExitCollision(other);
        }

        private void OnTriggerStay(Collider other)
        {
            CheckExitCollision(other);

        }

        

        private void CheckExitCollision(Collider collider)
        {
            if (collider.CompareTag("Exit") && collider.TryGetComponent(out ExitPoint exitPoint))
            {
                if (_canExit && exitPoint.CanExit(_bounds, _color))
                {
                    _hasExited = true;
                    OnExit?.Invoke();
                    ExitThrough(exitPoint.GetExitVector());
                    ToggleOutLine(false);
                    _canExit = false;
                }
            }
        }

        public bool CanMerge(Bounds blockBounds, UnitColor blockColor, BlockType blockType, out UnitColor mergedResultColor)
        {
            
            mergedResultColor = UnitColor.Orange;
            if (!IsMergable || blockType != Type || _isMerged)
            {
                return false;
            }
            
            if(UnitColorMappings.MergeResults.TryGetValue((_color, blockColor), out var result))
            {
                mergedResultColor = result;
            }
            else
            {
                print("wrong color");
                return false;
            }

            Bounds thisBounds = _bounds;
            if (blockBounds.min.x >= thisBounds.min.x - 0.1f && blockBounds.max.x <= thisBounds.max.x + 0.1f ||
                blockBounds.min.z >= thisBounds.min.z - 0.1f && blockBounds.max.z <= thisBounds.max.z + 0.1f)
            {
                _isMerged = true;
                return true;
            }

            print("Wrong Bounds");
            return false;
        }

        public Tween GetMerged(Vector3 pos, float duration)
        {
            if(!IsMergable || _isMerging) return null;
            _isMerging = true;
            _rb.isKinematic = true;
            return transform.DOMove(pos, duration).SetEase(_mergeEase).OnComplete(() =>
            {
                gameObject.SetActive(false);
                _hasExited = true;
            });
        }

        private void ExitThrough(Vector3 direction)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.isKinematic = true;
            var tilePos = GetCurrentTilePos();
            transform.position = tilePos;
            transform.DOMove(transform.position + direction * _exitDistance, _exitDuration)
                .SetEase(_exitEase);
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
            if (_targetPosition == null || _offset == null || !_isGettingDragged || _hasExited || _isMerged) return;

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
            if (_hasExited) return;
            offset.y = 0f;
            _offset = offset;
            _isGettingDragged = true;
            _posLockInTween?.Complete();
            _posLockInTween?.Kill();
            ToggleOutLine(true);
            _rb.isKinematic = false;
            UpdateBounds();
        }

        public void EndDrag()
        {
            if (_hasExited) return;

            UpdateBounds();
            ToggleOutLine(false);
            if (!_rb.isKinematic)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.isKinematic = true;
            }
            _posLockInTween?.Complete();
            _posLockInTween?.Kill();
            Vector3 finalPos = GetCurrentTilePos();
            _posLockInTween = transform.DOMove(finalPos, _posLockInDuration)
                .SetEase(_posLockInEase);
            _isGettingDragged = false;
            _targetPosition = null;
            _offset = null;
        }

        private Vector3 GetCurrentTilePos()
        {
            Vector3 finalPos = transform.position;
            if (Physics.Raycast(transform.position, Vector3.down, out var hitInfo, 3f, _tileLayer) && hitInfo.transform != null)
            {
                finalPos = hitInfo.transform.position;
            }
            finalPos.y = 0f;
            return finalPos;
        }

        public void UpdateDrag(Vector3 target)
        {
            if (_hasExited) return;

            target.y = transform.position.y;
            _targetPosition = target;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = UnityEngine.Color.yellow;
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

public static class UnitColorMappings
{
    public static Dictionary<(UnitColor, UnitColor), UnitColor> MergeResults = new Dictionary<(UnitColor, UnitColor), UnitColor>
    {
        { (UnitColor.Red, UnitColor.Blue), UnitColor.Purple },
        { (UnitColor.Blue, UnitColor.Red), UnitColor.Purple },
        { (UnitColor.Red, UnitColor.Yellow), UnitColor.Orange },
        { (UnitColor.Yellow, UnitColor.Red), UnitColor.Orange },
        { (UnitColor.Yellow, UnitColor.Blue), UnitColor.Green },
        { (UnitColor.Blue, UnitColor.Yellow), UnitColor.Green }
    };
}
