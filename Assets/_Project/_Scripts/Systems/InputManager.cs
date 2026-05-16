using DG.Tweening;
using Gilzoide.UpdateManager;
using Project.Assets._Project._Scripts.Interactables;
using Reflex.Attributes;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Project.Assets._Project._Scripts.Systems
{
    public class InputManager : AManagedBehaviour, IUpdatable
    {
        [Inject] private readonly InputReader _inputReader;
        [Inject] private readonly Camera _camera; 
        [Inject] private readonly CinemachineCamera _virtualCamera;
        [Inject] private readonly CinemachineBasicMultiChannelPerlin _virtualCameraShake;
        [Header("Interaction")]
        [SerializeField] private LayerMask _interactableLayer;
        private bool _canInteract = true;
        [Header("Camera Target")]
        [SerializeField] private Transform _cameraTarget;
        [SerializeField] private float _cameraMovementSensitivity = 2f;
        [SerializeField] private Vector2 _cameraMovementMaxPosition = new Vector2(50f, 50f);
        [Header("Camera Zoom")]
        [SerializeField] private float _zoomSpeed = 0.01f;
        [SerializeField] private float _minZoom = 5f;
        [SerializeField] private float _maxZoom = 20f;
        [SerializeField] private Ease _startingCameraZoomEase = Ease.OutCubic;
        private bool _isDragging = false;
        private bool _canMoveCamera = true;

        private Tween _cameraShakeTween;
        private void Start()
        {
            _inputReader.OnTouch += HandleTouch;
            _inputReader.OnDragStart += HandleDragStart;
            _inputReader.OnDragEnd += HandleDragEnd;
            _inputReader.OnZoom += HandleZoom;
            _virtualCamera.Target.TrackingTarget = _cameraTarget;
        }

        public Tween StartingCameraZoomEffect(float duration)
        {
            _virtualCamera.Lens.OrthographicSize = _maxZoom;
            return DOVirtual.Float(_maxZoom, EUtils.Math.GetAverage(_maxZoom, _minZoom), duration,o => _virtualCamera.Lens.OrthographicSize = o)
                .SetEase(_startingCameraZoomEase);
        }

        public void ShakeCamera(float duration, float amplitude = 3f, float frequency = 1f)
        {
            _cameraShakeTween?.Complete();
            _cameraShakeTween?.Kill();
            _virtualCameraShake.AmplitudeGain = amplitude;
            _virtualCameraShake.FrequencyGain = frequency;
            _virtualCameraShake.enabled = true;
            _cameraShakeTween = DOVirtual.DelayedCall(duration, () => _virtualCameraShake.enabled = false);
        }
        private void OnDestroy()
        {
            _inputReader.OnTouch -= HandleTouch;
            _inputReader.OnDragStart -= HandleDragStart;
            _inputReader.OnDragEnd -= HandleDragEnd;
            _inputReader.OnZoom -= HandleZoom;
        }

        private void HandleZoom(float pinchDelta)
        {
            float newSize = _virtualCamera.Lens.OrthographicSize - pinchDelta * _zoomSpeed;
            var size = Mathf.Clamp(newSize, _minZoom, _maxZoom);

            _virtualCamera.Lens.OrthographicSize = size;
        }

        public void SetCameraTargetPositionTo(Vector3 targetPos, float canMoveCameraAgainDelay = 1f)
        {
            _canMoveCamera = false;
            _cameraTarget.position = new Vector3(Mathf.Clamp(targetPos.x, -_cameraMovementMaxPosition.x, _cameraMovementMaxPosition.x), 0f,
                                                        Mathf.Clamp(targetPos.z, -_cameraMovementMaxPosition.y, _cameraMovementMaxPosition.y));
            DOVirtual.DelayedCall(canMoveCameraAgainDelay,() => _canMoveCamera = true);

        }

        public void ToggleCanInteract(bool canInteract)
        {
            _canInteract = canInteract;
        }

        public void ManagedUpdate()
        {
            if (!_canInteract)
            {
                return;
            }
            if (_isDragging && _canMoveCamera)
            {
                var drag = _inputReader.SetDragDelta();
                var targetPos = _cameraTarget.position - _cameraMovementSensitivity * Time.deltaTime * new Vector3(drag.x, 0f, drag.y);
                _cameraTarget.position = new Vector3(Mathf.Clamp(targetPos.x, -_cameraMovementMaxPosition.x, _cameraMovementMaxPosition.x), 0f,
                                                        Mathf.Clamp(targetPos.z, -_cameraMovementMaxPosition.y, _cameraMovementMaxPosition.y));
            }
        }
        private void HandleDragStart(Vector2 screenPos)
        {
            if (!_canInteract) return;
            _isDragging = true;
        }
        private void HandleDragEnd(Vector2 screenPos)
        {
            if (!_canInteract) return;
            _isDragging = false;
        }

        private bool IsPointerOverUI(Vector2 position)
        {
            var eventData = new PointerEventData(EventSystem.current)
            {
                position = position
            };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            return results.Count > 0;
        }

        private void HandleTouch()
        {
            if (IsPointerOverUI(_inputReader.TouchPosition) || !_canInteract) return;
            var touchRay = _camera.ScreenPointToRay(_inputReader.TouchPosition);
            if (Physics.Raycast(touchRay, out var hit, _interactableLayer) && hit.collider != null && hit.collider.TryGetComponent<IInteractable>(out var interactable))
            {
                interactable.Interact();
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawCube(Vector3.zero + Vector3.down, 2 * new Vector3(_cameraMovementMaxPosition.x, 0.1f, _cameraMovementMaxPosition.y));
        }
#endif
    }
}
