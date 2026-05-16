using DG.Tweening;
using Gilzoide.UpdateManager;
using Project.Assets._Project._Scripts.Interactables;
using Reflex.Attributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Project.Assets._Project._Scripts.ShowIf;

namespace Project.Assets._Project._Scripts.Systems
{
    public class GameManager : AManagedBehaviour, IUpdatable
    {
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly InputManager _inputManager; 
        [Inject] private readonly LevelManager _levelManager;
        [Inject] private readonly HintManager _hintManager;
        [Inject] private readonly IAudioService _audioService;
        [Inject] private readonly PlayerPrefsData _playerPrefsData;
        //[Inject] private readonly List<MovableBlock> _blocks;
        [Inject] private readonly List<Button> _allButtons;
        [Inject] private readonly Camera _camera;
        [Inject] private readonly AudioData _audioData;
        [SerializeField] private int _startingNumberOfTries = 10;

        [Header("Camera Effects")]
        [SerializeField] private float _cameraShakeDuration = 0.3f;
        [SerializeField] private float _cameraShakeAmplitude = 3f;
        [SerializeField] private float _cameraShakeFrequency = 1f;
        [SerializeField] private float _startingCameraEffectsDuration = 1f;
        [SerializeField] private float _cameraBetweenTutorialDelay = 1f;
        [Header("Tutorial Config")]
        [SerializeField] private bool _isTutorial = false;
        [SerializeField, ShowIf("_isTutorial", true)] private int _tutorialHintAmount = 2;


        private int _remainingTutorialHintAmount;
        private int _currentNumberOfTries;
        private bool _hasLost = false;
        private bool _hasWon = false;
        private bool _showingHintArrow = false;
        private Vector3 _lastHintPosition = Vector3.zero;
        private bool _soundTurnedOn = true;

        
        private void Awake()
        {
            
        }
        private void Start()
        {
            DOVirtual.DelayedCall(0.5f, () => Application.targetFrameRate = 60);
            _inputManager.ToggleCanInteract(false);
            _uiManager.OnLevelReloadRequested += _levelManager.ReloadLevel;
            _uiManager.OnContinueToNextLevelRequested += _levelManager.ContinueToNextLevel;
            _uiManager.OnReturnToMenuRequested += _levelManager.ReturnToMenu;
            _uiManager.OnHintRequested += TryGetHint;
            _uiManager.OnPauseRequested += Pause;
            _hintManager.OnCurrentHintAmountChanged += _uiManager.SetNumberOfHints;
            //_blocks.ForEach(block => block.OnInteracted += OnInteracted);
            _allButtons.ForEach(button => button.onClick.AddListener(() => _audioService.Play(_audioData.AnyButtonClip, _audioData.AnyButtonVolume)));
            _remainingTutorialHintAmount = _tutorialHintAmount;
            _currentNumberOfTries = _startingNumberOfTries;
            _uiManager.SetNumberOfTries(_currentNumberOfTries);
            CheckIfSoundNeedsToBeActive();
            _uiManager.ToggleHintButton(false);
            _uiManager.TogglePauseButton(false);
            DOTween.Sequence()
                .Append(_levelManager.MakeCoverDisappear())
                .Append(_inputManager.StartingCameraZoomEffect(_startingCameraEffectsDuration))
                .AppendCallback(() =>
                {
                    if (_isTutorial)
                    {
                        _uiManager.ToggleHintButton(false);
                        _uiManager.TogglePauseButton(true);
                        DOVirtual.DelayedCall(0.1f, () => { TryGetHint(); _inputManager.ToggleCanInteract(true); });
                    }
                    else
                    {
                        _inputManager.ToggleCanInteract(true);
                        _uiManager.TogglePauseButton(true);
                        _uiManager.ToggleHintButton(true);
                    }
                });
        }

        private void Pause(bool isPaused)
        {
            Time.timeScale = 1;
            _inputManager.ToggleCanInteract(!isPaused);
            _uiManager.TogglePauseButton(!isPaused);

            Time.timeScale = isPaused ? 0 : 1;

        }

        private void OnDestroy()
        {
            _uiManager.OnLevelReloadRequested -= _levelManager.ReloadLevel;
            _uiManager.OnContinueToNextLevelRequested -= _levelManager.ContinueToNextLevel;
            _uiManager.OnReturnToMenuRequested -= _levelManager.ReturnToMenu;
            _uiManager.OnHintRequested -= TryGetHint;
            _uiManager.OnPauseRequested -= Pause;
            _hintManager.OnCurrentHintAmountChanged -= _uiManager.SetNumberOfHints;
            //_blocks.ForEach(block => block.OnInteracted -= OnInteracted);
            _audioService.Stop();
            Time.timeScale = 1;

            DOTween.KillAll();
        }

        public void ManagedUpdate()
        {
            Application.targetFrameRate = 60;
            if (_showingHintArrow)
            {
                _uiManager.SetHintArrowPos(_camera, _lastHintPosition);
            }
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

        private void TryGetHint()
        {
            if(_isTutorial && _remainingTutorialHintAmount > 0)
            {
                _remainingTutorialHintAmount--;
                //if (_hintManager.TryGetHint(_blocks, out var pos, true))
                //{
                //    _lastHintPosition = pos;
                //    _inputManager.ToggleCanInteract(false);
                //    DOVirtual.DelayedCall(_cameraBetweenTutorialDelay,
                //        () =>
                //        {
                //            _inputManager.SetCameraTargetPositionTo(_lastHintPosition);
                //            _showingHintArrow = true;
                //            _uiManager.SetHintArrowPos(_camera, _lastHintPosition);
                //            _uiManager.ToggleHintArrow(true);
                //            _inputManager.ToggleCanInteract(true);
                //        });
                    
                //}
            }
            else
            {
                //if (_hintManager.TryGetHint(_blocks, out var pos))
                //{
                //    _lastHintPosition = pos;
                //    _inputManager.SetCameraTargetPositionTo(_lastHintPosition);
                //    _showingHintArrow = true;
                //    _uiManager.SetHintArrowPos(_camera, _lastHintPosition);
                //    _uiManager.ToggleHintArrow(true);
                //}
            }
            
        }


        private void OnInteracted(bool isCorrect)
        {
            if (_hasWon || _hasLost) return;
            _hintManager.OnAnyBlockInteracted();
            _uiManager.ToggleHintArrow(false);
            _showingHintArrow = false;

            _currentNumberOfTries--;
            _uiManager.SetNumberOfTries(_currentNumberOfTries);

            if (!isCorrect)
            {
                _inputManager.ShakeCamera(_cameraShakeDuration, _cameraShakeAmplitude, _cameraShakeFrequency);
                _audioService.Play(_audioData.WrongInteractionClip, _audioData.WrongInteractionVolume, UnityEngine.Random.Range(0.95f, 1.05f));
            }
            else
            {
                _audioService.Play(_audioData.CorrectInteractionClip, _audioData.CorrectInteractionVolume, UnityEngine.Random.Range(0.95f, 1.05f));
            }
            if (_isTutorial && _remainingTutorialHintAmount > 0)
            {
                TryGetHint();
            }
            else
            {
                _uiManager.ToggleHintButton(true);
            }
            if (_currentNumberOfTries <= 0)
            {
                CheckForWinOrLose();
            }
            else
            {
                CheckForWin();
            }
            

        }

        private void CheckForWin()
        {
            bool hasWon = true;
            //foreach(var block in _blocks)
            //{
            //    if (!block.HasDisappeared)
            //    {
            //        hasWon = false;
            //        break;
            //    }
            //}
            if (hasWon) WinLevel();
        }

        private void CheckForWinOrLose()
        {
            CheckForLose();
            if (!_hasLost) WinLevel();

        }

        private void CheckForLose()
        {
            //foreach (var block in _blocks)
            //{
            //    if (!block.HasDisappeared)
            //    {

            //        LoseLevel();
            //        break;
            //    }
            //}
        }

        private void WinLevel()
        {
            _hasWon = true;
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
