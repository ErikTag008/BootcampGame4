using DG.Tweening;
using KBCore.Refs;
using UnityEngine;

namespace Project.Assets._Project._Scripts.UI
{
    public class TutorialPointer : ValidatedMonoBehaviour
    {
        [SerializeField, Self] private RectTransform _pointerTransform;
        [SerializeField] private Vector3 _moveOffset;
        [SerializeField] private float _animationDuration = 1f;
        [SerializeField] private float _animationRestartDelay = 0.5f;
        [SerializeField] private Ease _animationEase = Ease.OutCubic;
        [SerializeField, Child] private TrailRenderer _trailRenderer;

        private Tween _animationTween;

        private void OnEnable()
        {
            StartAnimation();
        }

        private void OnDisable()
        {
            _animationTween?.Complete();
            _animationTween?.Kill();
            _trailRenderer.Clear();
            _trailRenderer.emitting = false;
        }

        private void StartAnimation()
        {
            _animationTween?.Complete();
            _animationTween?.Kill();
            _trailRenderer.emitting = true;
            _trailRenderer.Clear();
            _animationTween = DOTween.Sequence()
                .Append(_pointerTransform.DOMove(
                    _pointerTransform.position + _moveOffset,
                    _animationDuration
                ).SetEase(_animationEase))
                .AppendInterval(_animationRestartDelay)
                .AppendCallback(() =>
                {
                    _pointerTransform.position -= _moveOffset; // reset position if needed
                    _trailRenderer.Clear();
                })
                .SetLoops(-1);
        }
    }
}
