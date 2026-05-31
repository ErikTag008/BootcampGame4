using DG.Tweening;
using Gilzoide.UpdateManager;
using Project.Assets._Project._Scripts.Interactables;
using Reflex.Attributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Project.Assets._Project._Scripts.ShowIf;
using System;

namespace Project.Assets._Project._Scripts.Systems
{
    public class GameManager : AManagedBehaviour, IUpdatable
    {
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly InputManager _inputManager; 
        [Inject] private readonly LevelManager _levelManager;
        [Inject] private readonly IAudioService _audioService;
        [Inject] private readonly TimerManager _timerManager;
        [Inject] private readonly MergeManager _mergeManager;
        [Inject] private readonly PlayerPrefsData _playerPrefsData;
        [Inject] private readonly List<Button> _allButtons;
        [Inject] private readonly AudioData _audioData;
        [Inject] private readonly List<Block> _blocks;

        [Header("Camera Effects")]
        [SerializeField] private float _cameraShakeDuration = 0.3f;
        [SerializeField] private float _cameraShakeAmplitude = 3f;
        [SerializeField] private float _cameraShakeFrequency = 1f;
        [SerializeField] private float _startingCameraEffectsDuration = 1f;
        [SerializeField] private float _cameraBetweenTutorialDelay = 1f;
        [Header("Tutorial Config")]
        [SerializeField] private bool _hasTutorial;
        [SerializeField] private float _tutorialInputDisabledDuration = 1f;


        private bool _hasLost = false;
        private bool _hasWon = false;
        private bool _soundTurnedOn = true;

        private void Start()
        {
            DOVirtual.DelayedCall(0.5f, () => Application.targetFrameRate = 60);
            DOVirtual.DelayedCall(0.5f, () => QualitySettings.vSyncCount = 0);

            SubscribeToEvents();
            CheckIfSoundNeedsToBeActive();
            _inputManager.ToggleCanInteract(false);
            _uiManager.TogglePauseButton(false);
            _uiManager.ToggleLevelNumberImage(false);
            DoStartingSequence();
        }


        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            _audioService.Stop();
            Time.timeScale = 1;

            DOTween.KillAll();
        }
        private void StartTutorialIfNeeded()
        {
            if (_hasTutorial)
            {
                DoTutorialActions();
            }
            _inputManager.OnTouch += StopTutorial;
        }

        private void DoTutorialActions()
        {
            _inputManager.ToggleCanInteract(false);
            _uiManager.ToggleTutorial(true);
            DOVirtual.DelayedCall(_tutorialInputDisabledDuration, () => _inputManager.ToggleCanInteract(true));
        }

        private void StopTutorial()
        {
            _inputManager.OnTouch -= StopTutorial;
            _uiManager.ToggleTutorial(false);
        }



        private void TogglePause(bool isPaused)
        {
            Time.timeScale = 1;
            _inputManager.ToggleCanInteract(!isPaused);
            _uiManager.TogglePauseButton(!isPaused);

            Time.timeScale = isPaused ? 0 : 1;

        }

        
        

        public void ManagedUpdate()
        {
            Application.targetFrameRate = 60;
        }

        private void CheckIfSoundNeedsToBeActive()
        {
            int soundShoudBeOnPlPref = PlayerPrefs.GetInt(_playerPrefsData.SoundTurnedOn, 1);
            bool shouldSoundBeOn = soundShoudBeOnPlPref > 0;
            if (shouldSoundBeOn && !_soundTurnedOn || !shouldSoundBeOn && _soundTurnedOn)
            {
                ToggleSound();
            }
        }

        private void ToggleSound()
        {
            _soundTurnedOn = !_soundTurnedOn;
            if (!_soundTurnedOn) _audioService.ToggleMute(true);
            else _audioService.ToggleMute(false);

        }
        private void SubscribeToEvents()
        {
            _uiManager.OnLevelReloadRequested += _levelManager.ReloadLevel;
            _uiManager.OnContinueToNextLevelRequested += _levelManager.ContinueToNextLevel;
            _uiManager.OnReturnToMenuRequested += _levelManager.ReturnToMenu;
            _uiManager.OnPauseRequested += TogglePause;
            _timerManager.OnTimeEnded += LoseLevel;
            _mergeManager.OnBlockCreated += RegisterBlock;
            _blocks.ForEach(block => 
                {
                    block.OnExit += OnBlockExited;
                    block.OnTimeBonusAcquired += _timerManager.AddTime;
                    block.OnTimeBonusAcquired += _uiManager.AddTimeBonus;
                    block.OnWaterHit += OnBlockWaterHit; 
                    block.OnDragToggle += OnBlockDragToggle;
                    block.OnMergeStart += OnMergeStart;

                });
            
            _allButtons.ForEach(button => button.onClick.AddListener(() => _audioService.Play(_audioData.AnyButtonClip, _audioData.AnyButtonVolume)));
        }

        private void RegisterBlock(Block block)
        {
            _blocks.Add(block);
            block.OnExit += OnBlockExited;
            block.OnTimeBonusAcquired += _timerManager.AddTime;
            block.OnTimeBonusAcquired += _uiManager.AddTimeBonus;

            block.OnWaterHit += OnBlockWaterHit;
            block.OnDragToggle += OnBlockDragToggle;
            block.OnMergeStart += OnMergeStart;

        }

        private void UnsubscribeFromEvents()
        {
            _uiManager.OnLevelReloadRequested -= _levelManager.ReloadLevel;
            _uiManager.OnContinueToNextLevelRequested -= _levelManager.ContinueToNextLevel;
            _uiManager.OnReturnToMenuRequested -= _levelManager.ReturnToMenu;
            _uiManager.OnPauseRequested -= TogglePause;
            _timerManager.OnTimeEnded -= LoseLevel;
            _mergeManager.OnBlockCreated -= RegisterBlock;
            _blocks.ForEach(block => 
                {
                    block.OnExit -= OnBlockExited; 
                    block.OnTimeBonusAcquired -= _timerManager.AddTime; 
                    block.OnTimeBonusAcquired -= _uiManager.AddTimeBonus;
                    block.OnWaterHit -= OnBlockWaterHit;
                    block.OnDragToggle -= OnBlockDragToggle;
                    block.OnMergeStart -= OnMergeStart;
                });
        }

        private void OnMergeStart()
        {
            _audioService.Play(_audioData.BlockInteractionClip, _audioData.BlockInteractionVolume, 1f);
        }

        private void OnBlockDragToggle(bool isDragging)
        {
            if (isDragging)
            {
                _audioService.Play(_audioData.BlockInteractionClip, _audioData.BlockInteractionVolume, 1.1f);
            }
            else
            {
                _audioService.Play(_audioData.BlockInteractionClip, _audioData.BlockInteractionVolume, 0.9f);

            }
        }
        


        private void OnBlockExited()
        {
            if (_hasWon || _hasLost) return;
            foreach(var block in _blocks)
            {
                if(block != null && block.gameObject.activeInHierarchy && !block.HasExited && block.HasMoveDelay)
                {
                    block.DepleteMoveDelay();
                }
            }
            if (_hasWon) return;
            CheckForWin();
        }

        private void OnBlockWaterHit()
        {
            _audioService.Play(_audioData.CorrectInteractionClip, _audioData.CorrectInteractionVolume);
        }

        private void CheckForWin()
        {
            print("Checking For Win");
            bool hasWon = true;
            foreach (var block in _blocks)
            {
                if (block != null && block.gameObject.activeInHierarchy && !block.HasExited)
                {
                    hasWon = false;
                    break;
                }
            }

            if (hasWon) WinLevel();
        }

        private void DoStartingSequence()
        {
            DOTween.Sequence()
                            .Append(_levelManager.MakeCoverDisappear())
                            .Append(_inputManager.StartingCameraZoomEffect(_startingCameraEffectsDuration))
                            .AppendCallback(() =>
                            {
                                _inputManager.ToggleCanInteract(true);
                                _uiManager.TogglePauseButton(true);
                                _uiManager.ToggleLevelNumberImage(true);
                                _inputManager.OnTouch += StartTimerOnTouch;
                                StartTutorialIfNeeded();
                            });
        }

        private void StartTimerOnTouch()
        {
            _inputManager.OnTouch -= StartTimerOnTouch;
            _timerManager.StartTimer();
        }

        private void WinLevel()
        {
            _hasWon = true;
            _timerManager.StopTimer();
            _inputManager.ToggleCanInteract(false);
            _levelManager.UnlockNextLevel();
            _audioService.Play(_audioData.LevelWinClip, _audioData.LevelWinVolume);
            _uiManager.OnWin();
        }

        private void LoseLevel()
        {
            _hasLost = true;
            _inputManager.ToggleCanInteract(false);
            _audioService.Play(_audioData.LevelLoseClip, _audioData.LevelLoseVolume);
            _uiManager.OnLose();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            SetDisplayTo60Hz();
        }

        private void SetDisplayTo60Hz()
        {
            Application.targetFrameRate = 60;
            
        }
        
    }
}
