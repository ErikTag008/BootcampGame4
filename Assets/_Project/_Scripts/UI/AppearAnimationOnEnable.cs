using DG.Tweening;
using DG.Tweening.Core;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening.CustomPlugins;

namespace Project.Assets._Project._Scripts.UI
{
    public class AppearAnimationOnEnable : ValidatedMonoBehaviour
    {
        [SerializeField, Self] private RectTransform _rectTransform;
        [SerializeField] private float _duration = 0.5f;
        [SerializeField] private Vector3 _offsetFromOriginalPos = Vector3.zero;
        [SerializeField] private Ease _animationEase = Ease.OutCubic;
        private Tween _tween;
        private Vector2 _startingPos;

        private void Awake()
        {
            _startingPos = _rectTransform.anchoredPosition;
        }

        private void OnEnable()
        {
            _tween?.Kill();

            _rectTransform.anchoredPosition =
                _startingPos + (Vector2)_offsetFromOriginalPos;

            _tween = _rectTransform.DOAnchorPos(_startingPos, _duration)
                .SetEase(_animationEase);
        }
    }
}
