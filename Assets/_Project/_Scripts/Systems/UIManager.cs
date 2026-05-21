using Project.Assets._Project._Scripts.UI;
using Reflex.Attributes;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Project.Assets._Project._Scripts.Systems
{
    public class UIManager : MonoBehaviour
    {
        [Inject] private readonly CoreUIElements _coreUIElements;
        [Inject] private readonly GameUIElements _gameUIElements;
        public event Action OnLevelReloadRequested;
        public event Action OnContinueToNextLevelRequested;
        public event Action OnReturnToMenuRequested;
        public event Action OnHintRequested;
        public event Action<bool> OnPauseRequested;

        private void Start()
        {
            _coreUIElements?.WinReloadButton?.onClick.AddListener(() => OnLevelReloadRequested?.Invoke());
            _coreUIElements?.WinContinueButton?.onClick.AddListener(() => OnContinueToNextLevelRequested?.Invoke());
            _coreUIElements?.WinMenuButton?.onClick.AddListener(() => OnReturnToMenuRequested?.Invoke());
            _coreUIElements?.LoseReloadButton?.onClick.AddListener(() => OnLevelReloadRequested?.Invoke());
            _coreUIElements?.LoseMenuButton?.onClick.AddListener(() => OnReturnToMenuRequested?.Invoke());
            _coreUIElements?.PauseMenuButton?.onClick.AddListener(() => OnReturnToMenuRequested?.Invoke());
            _coreUIElements?.PauseReloadButton?.onClick.AddListener(() => OnLevelReloadRequested?.Invoke());
            _coreUIElements?.PauseReturnButton?.onClick.AddListener(PauseReturnButton);
            _gameUIElements?.PauseButton?.onClick.AddListener(PauseButton);
            _gameUIElements?.HintButton?.onClick.AddListener(() => OnHintRequested?.Invoke());
        }
        public Image GetLoadingCover()
        {
            return _coreUIElements.LoadingCover;
        }

        private void PauseReturnButton()
        {
            _coreUIElements.HidePauseScreen();
            ToggleHintButton(true);
            OnPauseRequested?.Invoke(false);
        }

        public void ChangeTimerValue(string time)
        {
            _gameUIElements.SetTimerText(time);
        }
        private void PauseButton()
        {
            _coreUIElements.ShowPauseScreen();
            ToggleHintButton(false);
            OnPauseRequested?.Invoke(true);
        }

        public void ToggleHintArrow(bool isActive)
        {
            _gameUIElements.ToggleHintArrow(isActive);
        }

        public void ToggleLevelNumberImage(bool isActive)
        {
            _gameUIElements.ToggleLevelNumberImage(isActive);
        }   

        public void ToggleHintButton(bool isActive)
        {
            _gameUIElements.ToggleHintButton(isActive);
        }

        public void TogglePauseButton(bool isActive)
        {
            _gameUIElements.TogglePauseButton(isActive);
        }

        public void SetHintArrowPos(Camera cam, Vector3 pos)
        {
            _gameUIElements.UpdateHintArrowPosition(cam, pos);
        }

        public void SetNumberOfHints(int num)
        {
            if (num < 0) num = 0;
            _gameUIElements.SetNumberOfHints(num);
        }


        [ContextMenu("Lose")]
        public void OnLose()
        {
            ToggleHintButton(false);
            TogglePauseButton(false);
            _coreUIElements.ShowLoseScreen();
        }

        [ContextMenu("Win")]
        public void OnWin()
        {
            ToggleHintButton(false);
            TogglePauseButton(false);
            _coreUIElements.ShowWinScreen();
        }

    }
}
