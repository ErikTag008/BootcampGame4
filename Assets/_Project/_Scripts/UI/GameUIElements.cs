using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Assets._Project._Scripts.UI
{
    public class GameUIElements : MonoBehaviour
    {
        [field: Header("Button References")]
        [field: SerializeField] public Button PauseButton { get; private set; }
        [field: SerializeField] public Button HintButton { get; private set; }
        [Header("Hint Number")]
        [SerializeField] private TMP_Text _numberOfHintsLeft;
        
        [Header("Hint Arrow")]
        [SerializeField] private RectTransform _hintArrowParent;
        [SerializeField] private Image _hintArrowIcon;
        [SerializeField] private Vector2 _hintArrowOffset = new Vector2(150, -150);
        [SerializeField] private Vector2 _hintArrowAnimationTargetOffset = new Vector2(-50, -50);
        [SerializeField] private float _hintArrowAnimationDuration = 1f;
        [SerializeField] private Ease _hintArrowAnimationEase = Ease.InOutSine;
        private Tween _arrowTween;
        [Header("Tries Number")]
        [SerializeField] private TMP_Text _numberOfTriesText;
        [SerializeField] private float _numberOfTriesAnimationDuration = 0.3f;
        [SerializeField] private Vector2 _numberOfTriesAnimationPositionOffset = new Vector2(0f, 30f);
        [SerializeField] private Ease _numberOfTriesAnimationEase;
        private Tween _numberOfTriesMovementTween;
        private Tween _numberOfTriesColorTween;

        public void SetNumberOfTries(int numberOfTries)
        {
            _numberOfTriesText.text = numberOfTries.ToString();
            _numberOfTriesMovementTween?.Complete();
            _numberOfTriesMovementTween?.Kill();
            _numberOfTriesColorTween?.Complete();
            _numberOfTriesColorTween?.Kill();
            var position = _numberOfTriesText.rectTransform.position;
            var color = _numberOfTriesText.color;
            color.a = 0;
            _numberOfTriesColorTween = DOVirtual.Color(color, _numberOfTriesText.color, _numberOfTriesAnimationDuration, c => _numberOfTriesText.color = c)
                .SetEase(_numberOfTriesAnimationEase);
            _numberOfTriesMovementTween = _numberOfTriesText.rectTransform.DOMove(position, _numberOfTriesAnimationDuration)
                .From(position + (Vector3)_numberOfTriesAnimationPositionOffset)
                .SetEase(_numberOfTriesAnimationEase);
        }

        public void SetNumberOfHints(int numberOfHints) 
        {

            _numberOfHintsLeft.text = numberOfHints.ToString();
            if(numberOfHints <= 0)
            {
                HintButton.interactable = false;
            }
        }

        public void ToggleHintButton(bool isActive)
        {
            HintButton.gameObject.SetActive(isActive);
        }

        public void TogglePauseButton(bool isActive)
        {
            PauseButton.gameObject.SetActive(isActive);
        }

        public void ToggleHintArrow(bool isActive)
        {
            _hintArrowParent.gameObject.SetActive(isActive);
            if (isActive)
            {
                Color startingColor = _hintArrowIcon.color;
                Color noAlpha = startingColor;
                noAlpha.a = 0;
                _arrowTween = DOVirtual.Color(startingColor, noAlpha, _hintArrowAnimationDuration, c => _hintArrowIcon.color = c)
                    .SetEase(_hintArrowAnimationEase)
                    .SetLoops(-1, LoopType.Yoyo)
                    .OnComplete(() => _hintArrowIcon.color = startingColor)
                    .OnKill(() =>  _hintArrowIcon.color = startingColor);
                
            }
            else
            {
                _arrowTween?.Complete();
                _arrowTween?.Kill();
                _hintArrowIcon.rectTransform.localPosition = Vector2.zero;
            }


        }

        public void UpdateHintArrowPosition(Camera cam, Vector3 position)
        {
            var screenPos = cam.WorldToScreenPoint(position);
            _hintArrowParent.position = screenPos + (Vector3)_hintArrowOffset;
        }
    }
}

