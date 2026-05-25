using DG.Tweening;
using Gilzoide.UpdateManager;
using KBCore.Refs;
using Project.Assets._Project._Scripts.GridComponents;
using Project.Assets._Project._Scripts.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
        [SerializeField] private LayerMask _obstacleForMergeLayer;
        [SerializeField] private float _moveSpeed = 10f;
        [SerializeField] private float _maxSpeed = 20f;
        [SerializeField] private float _posLockInDuration = 0.1f;
        [SerializeField] private Ease _posLockInEase = Ease.OutCubic;
        [SerializeField] private float _exitDuration = 0.3f;
        [SerializeField] private float _exitDistance = 2.0f;
        [SerializeField] private Ease _exitEase = Ease.InOutCubic;
        
        private bool _canExit = true;
        private bool _hasExited = false;
        private bool _isMerged = false;
        private bool _isMerging = false;
        public event Action OnExit;
        public event Action<UnitColor, BlockType, Vector3, Quaternion, int> OnMerge; 
        public bool HasExited => _hasExited;
        [field: SerializeField] public bool HasTimeBonus { get; private set; } = false;
        [field: SerializeField, ShowIf.ShowIf(nameof(HasTimeBonus), true)] public int TimeBonusInSeconds { get; private set; } = 5;
        public event Action<int> OnTimeBonusAcquired;
        [SerializeField, ShowIf.ShowIf(nameof(HasTimeBonus), true)] private TMP_Text _timeBonusText;
        [field: SerializeField] public bool HasMoveDelay { get; private set; } = false;
        [field: SerializeField, ShowIf.ShowIf(nameof(HasMoveDelay), true)] public int MoveDelay { get; private set; } = 5;
        [SerializeField, ShowIf.ShowIf(nameof(HasMoveDelay), true)] private int _currentMoveDelay;
        [SerializeField, ShowIf.ShowIf(nameof(HasMoveDelay), true)] private TMP_Text _moveDelayText;
        private bool _canMove = true;
        [field: SerializeField] public bool IsMergable { get; private set; } = false;
        [field: SerializeField, ShowIf.ShowIf(nameof(IsMergable), true)] public BlockType Type { get; private set; } = BlockType.OneByOne;
        [SerializeField, ShowIf.ShowIf(nameof(IsMergable), true)] private Ease _mergeEase = Ease.InOutCubic;
        [SerializeField, ShowIf.ShowIf(nameof(IsMergable), true)] private float _mergeDuration = 1f;
        private Bounds _bounds;
        private Vector3? _offset;
        private Vector3? _targetPosition;
        private bool _isGettingDragged = false;
        private Tween _posLockInTween;
        private float _startingYPosition;
        public event Action<Block, UnitColor> OnColorChanged;
        private UnitColor _lastColor;
        private void Awake()
        {
            UpdateBounds();
            ToggleOutLine(false);
            _rb.isKinematic = true;
            _currentMoveDelay = MoveDelay;
            _startingYPosition = transform.position.y;
            _timeBonusText.text = HasTimeBonus ? $"+{TimeBonusInSeconds}s" : string.Empty;
            _moveDelayText.text = HasMoveDelay ? _currentMoveDelay.ToString() : string.Empty;
            _canMove = !HasMoveDelay || _currentMoveDelay <= 0;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (_lastColor != _color)
            {
                _lastColor = _color;
#if UNITY_EDITOR
                BlockColorChanger colorChanger = FindAnyObjectByType<BlockColorChanger>();
                colorChanger.ChangeBlockColor(this, _color);
#endif
            }
        }


        public void ChangeColor(UnitColor newColor)
        {
            if (_color == newColor) return;
            _color = newColor;
            OnColorChanged?.Invoke(this, _color);
        }
        public void ChangeMaterial(Material material)
        {
            _renderers ??= GetComponentsInChildren<MeshRenderer>();
            foreach(var rend in _renderers)
            {
                rend.material = material;
            }
        }

        public void SetFeatures(bool isMergable, bool hasTimeBonus, bool hasMoveDelay)
        {
            IsMergable = isMergable;
            HasTimeBonus = hasTimeBonus;
            HasMoveDelay = hasMoveDelay;
            _timeBonusText.gameObject.SetActive(hasTimeBonus);
            _moveDelayText.gameObject.SetActive(hasMoveDelay);
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
            if (!IsMergable) return;
            if (collision.collider.TryGetComponent(out Block otherBlock) && otherBlock.IsMergable)
            {
                if (!_isMerged && otherBlock.CanMerge(_bounds, _color, Type, out var mergedResultColor))
                {
                    Vector3 meetPoint = (_bounds.center + otherBlock._bounds.center) / 2f;
                    Vector3 thisTarget = meetPoint - (_bounds.center - transform.position);
                    Vector3 otherTarget = meetPoint - (otherBlock._bounds.center - otherBlock.transform.position);

                    // Snap targets to tiles (normalized)
                    Vector3 snappedThisTarget = SnapToTilePosition(thisTarget);
                    Vector3 snappedOtherTarget = SnapToTilePosition(otherTarget);

                    // Check path from this block center to snappedThisTarget and from other to snappedOtherTarget.
                    Vector3 dirThis = snappedThisTarget - _bounds.center;
                    float distThis = Mathf.Max(0f, dirThis.magnitude / 2 - 0.05f);
                    Vector3 dirOther = snappedOtherTarget - otherBlock._bounds.center;
                    float distOther = Mathf.Max(0f, dirOther.magnitude / 2 - 0.05f);

                    if (!IsPathEmpty(dirThis, distThis, _bounds, this, otherBlock))
                    {
                        print("Path Is Not Empty");
                        return;
                    }

                    var timeBonus = GetTimeBonus(otherBlock);

                    DOTween.Sequence()
                        .Join(otherBlock.GetMerged(snappedOtherTarget, _mergeDuration))
                        .Join(GetMerged(snappedThisTarget + Vector3.one * 0.01f, _mergeDuration))
                        .AppendCallback(() =>
                        {
                            // Notify spawn position as snapped (tile normalized)
                            OnMerge?.Invoke(mergedResultColor, otherBlock.Type, snappedThisTarget, transform.rotation, timeBonus);
                        });
                }
            }
        }

        private int GetTimeBonus(Block otherBlock)
        {
            int timeBonus;
            if (HasTimeBonus && otherBlock.HasTimeBonus)
            {
                timeBonus = TimeBonusInSeconds + otherBlock.TimeBonusInSeconds;
            }
            else if (HasTimeBonus)
            {
                timeBonus = TimeBonusInSeconds;
            }
            else if (otherBlock.HasTimeBonus)
            {
                timeBonus = otherBlock.TimeBonusInSeconds;
            }
            else
            {
                timeBonus = 0;
            }
            return timeBonus;
        }

        private bool IsPathEmpty(Vector3 direction, float distance, Bounds bounds, Block thisBlock, Block otherBlock)
        {
            if (direction.sqrMagnitude <= 0.0001f || distance <= 0f) return true;

            Vector3 dir = direction.normalized;
            
            // Start a tiny bit ahead to avoid hitting self-colliders at origin
            Vector3 origin = bounds.center + dir * 0.1f;
            var rays = new RaycastHit[5];
            Vector3 halfExtents = bounds.extents;
            halfExtents *= 0.8f;
            int rayCount = Physics.BoxCastNonAlloc(origin, halfExtents, dir, rays, Quaternion.identity, distance, _obstacleForMergeLayer);
            int checkCount = Mathf.Min(rayCount, rays.Length);
            foreach(RaycastHit ray in rays)
            {
                if (ray.collider == null) continue;
                print(ray.collider.name);
            }
            for (int i = 0; i < checkCount; i++)
            {
                var hit = rays[i];
                if (hit.collider == null) continue;
                // ignore very close hits caused by starting overlap
                //if (hit.distance <= 0.05f) continue;
                var hitGo = hit.collider.gameObject;
                if (hitGo == thisBlock.gameObject || hitGo == otherBlock.gameObject) continue;
                print($"Hit {hit.collider.name} while checking path");
                return false;
            }
            return true;
        }

        public void SetTimeBonusOnMerge(int timeBonus)
        {
            HasTimeBonus = true;
            TimeBonusInSeconds = timeBonus;
            if (TimeBonusInSeconds <= 0)
            {
                _timeBonusText.text = string.Empty;
            }
            else 
            { 
                _timeBonusText.text = $"+{TimeBonusInSeconds}s";
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

        public void DepleteMoveDelay()
        {
            if (!HasMoveDelay) return;
            _currentMoveDelay--;
            _canMove = _currentMoveDelay <= 0;
            _moveDelayText.text = _currentMoveDelay <= 0 ? string.Empty : _currentMoveDelay.ToString();
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
                //print("wrong color");
                return false;
            }

            Bounds thisBounds = _bounds;
            if (blockBounds.min.x >= thisBounds.min.x - 0.1f && blockBounds.max.x <= thisBounds.max.x + 0.1f ||
                blockBounds.min.z >= thisBounds.min.z - 0.1f && blockBounds.max.z <= thisBounds.max.z + 0.1f)
            {
                return true;
            }

            //print("Wrong Bounds");
            return false;
        }

        public Tween GetMerged(Vector3 pos, float duration)
        {
            if(!IsMergable || _isMerging) return null;
            _isMerged = true;
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
            if(HasTimeBonus) OnTimeBonusAcquired?.Invoke(TimeBonusInSeconds);
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
            if (_hasExited || !_canMove) return;
            offset.y = _startingYPosition;
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
            finalPos.y = _startingYPosition;
            return finalPos;
        }

        private Vector3 SnapToTilePosition(Vector3 pos)
        {
            Vector3 origin = pos + Vector3.up * 5f;
            if (Physics.Raycast(origin, Vector3.down, out var hit, 10f, _tileLayer))
            {
                Vector3 tilePos = hit.transform.position;
                tilePos.y = _startingYPosition;
                return tilePos;
            }
            // fallback: snap to nearest integer grid
            Vector3 fallback = pos;
            fallback.x = Mathf.Round(fallback.x);
            fallback.z = Mathf.Round(fallback.z);
            fallback.y = _startingYPosition;
            return fallback;
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
