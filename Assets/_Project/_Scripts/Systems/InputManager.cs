using DG.Tweening;
using Gilzoide.UpdateManager;
using Project.Assets._Project._Scripts.GridComponents;
using Project.Assets._Project._Scripts.Interactables;
using Reflex.Attributes;
using System;
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
        [Inject] private readonly GridGenerator _gridGen;
        [Header("Interaction")]
        [SerializeField] private LayerMask _interactableLayer;
        [SerializeField] private LayerMask _tileLayer;
        private bool _canInteract = true;
        [Header("Camera Target")]
        [SerializeField] private Transform _cameraTarget;
        [Header("Camera Zoom")]
        [SerializeField] private float _startingCameraZoomFovOffset = 10f;
        [SerializeField] private Ease _startingCameraZoomEase = Ease.OutCubic;
        private bool _isDragging = false;
        private Tween _cameraShakeTween;
        private IDragable _currentDraggedBlock;
        public event Action OnTouch;
        private void Start()
        {
            _inputReader.OnDragStart += HandleDragStart;
            _inputReader.OnDragEnd += HandleDragEnd;
            _virtualCamera.Target.TrackingTarget = _cameraTarget;
        }

        public Tween StartingCameraZoomEffect(float duration)
        {
            float fov = _virtualCamera.Lens.FieldOfView;
            _virtualCamera.Lens.FieldOfView += _startingCameraZoomFovOffset;
            return DOVirtual.Float(_virtualCamera.Lens.FieldOfView, fov, duration,o => _virtualCamera.Lens.FieldOfView = o)
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
            _inputReader.OnDragStart -= HandleDragStart;
            _inputReader.OnDragEnd -= HandleDragEnd;
        }


        public void ToggleCanInteract(bool canInteract)
        {
            _canInteract = canInteract;
        }

        public void ManagedUpdate()
        {
            if (!_canInteract) return;
            if (_isDragging && _currentDraggedBlock != null)
            {
                Ray ray = _camera.ScreenPointToRay(_inputReader.DragPosition);
                Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

                if (groundPlane.Raycast(ray, out float distance))
                {
                    Vector3 position = ray.GetPoint(distance);
                    _currentDraggedBlock?.UpdateDrag(position);
                }
            }
        }
        private void HandleDragStart(Vector2 screenPos)
        {
            if (IsPointerOverUI(_inputReader.DragPosition) || !_canInteract) return;
            OnTouch?.Invoke();
            var touchRay = _camera.ScreenPointToRay(_inputReader.DragPosition);
            if (Physics.Raycast(touchRay, out var hit, _interactableLayer) && hit.collider != null && hit.collider.TryGetComponent<IDragable>(out var dragable))
            {

                _currentDraggedBlock = dragable;
                if (_currentDraggedBlock == null) return;
                print($"Current Dragable: {_currentDraggedBlock}");
                var point = hit.point;
                var blockTransform = (_currentDraggedBlock as Component).transform;
                var offset = blockTransform.position - point;

                _currentDraggedBlock.StartDrag(offset);

                _isDragging = true;
                _currentDraggedBlock.OnExit += HandleDragEnd;
            }
        }
        private void HandleDragEnd()
        {
            if (!_canInteract && !_isDragging) return;
            if(_currentDraggedBlock == null) return;
            _currentDraggedBlock.EndDrag();
            _currentDraggedBlock.OnExit -= HandleDragEnd;
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

        
    }
}
