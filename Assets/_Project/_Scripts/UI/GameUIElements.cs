using DG.Tweening;
using KBCore.Refs;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Assets._Project._Scripts.UI
{
    public class GameUIElements : MonoBehaviour
    {
        [field: Header("Button References")]
        [field: SerializeField] public Button PauseButton { get; private set; }
        [field: SerializeField] public TMP_Text LevelNumber { get; private set; }
        [field: Header("Timer")]
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private TMP_Text _timerAddedBonusText;
        [Header("Tutorial")]
        [SerializeField] private List<TutorialPointer> _tutorialPointers = new();


        private Tween _timerBonusTween;

        public void ToggleTutorialPointers(bool isEnabled)
        {
            foreach (var pointer in _tutorialPointers)
            {
                pointer?.gameObject.SetActive(isEnabled);
            }
        }
        public void SetTimerText(string text)
        {
            _timerText.text = text;
        }

        public void DisplayTimerBonus(int seconds)
        {
            _timerBonusTween?.Complete();
            _timerBonusTween?.Kill();
            _timerAddedBonusText.gameObject.SetActive(true);

            _timerAddedBonusText.text = $"+{seconds}s";
            _timerAddedBonusText.alpha = 1f;
            _timerBonusTween = DOTween.Sequence()
                .Append(_timerAddedBonusText.rectTransform.DOScale(Vector3.one, 1f).From(Vector3.zero).SetEase(Ease.OutBack))
                .Append(DOVirtual.Float(1f, 0f, 1f, value => _timerAddedBonusText.alpha = value))
                .OnComplete(() => _timerAddedBonusText.gameObject.SetActive(false));
        }


        public void ToggleLevelNumberImage(bool isActive)
        {
            LevelNumber.gameObject.SetActive(isActive);
        }

        public void TogglePauseButton(bool isActive)
        {
            PauseButton.gameObject.SetActive(isActive);
        }
    }
}

