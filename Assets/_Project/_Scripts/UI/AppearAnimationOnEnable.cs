using DG.Tweening;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Assets._Project._Scripts.UI
{
    public class AppearAnimationOnEnable : ValidatedMonoBehaviour
    {
        [SerializeField, Self] private Image _image;
        [SerializeField] private float _duration = 0.5f;
        [SerializeField] private Vector3 _offsetFromOriginalPos = Vector3.zero;
        [SerializeField] private Ease _animationEase = Ease.OutCubic;
        private Vector3 _startingPos;
        private Tween _tween;
        private void OnEnable()
        {
            _tween?.Complete();
            _tween?.Kill();
            _startingPos = _image.rectTransform.localPosition;
            _image.rectTransform.localPosition = _startingPos + _offsetFromOriginalPos;
            _tween = _image.rectTransform.DOLocalMove(_startingPos, _duration)
                .SetEase(_animationEase);
        }
    }
}
