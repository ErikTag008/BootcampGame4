using DG.Tweening;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Assets._Project._Scripts.UI
{
    public class AppearAnimationOnEnable : ValidatedMonoBehaviour
    {
        [SerializeField, Self] private RectTransform _rectTransform;
        [SerializeField] private float _duration = 0.5f;
        [SerializeField] private Vector3 _offsetFromOriginalPos = Vector3.zero;
        [SerializeField] private Ease _animationEase = Ease.OutCubic;
        private Vector3 _startingPos;
        private Tween _tween;
        private void OnEnable()
        {
            _tween?.Complete();
            _tween?.Kill();
            _startingPos = _rectTransform.localPosition;
            _rectTransform.localPosition = _startingPos + _offsetFromOriginalPos;
            _tween = _rectTransform.DOLocalMove(_startingPos, _duration)
                .SetEase(_animationEase);
        }
    }
}
