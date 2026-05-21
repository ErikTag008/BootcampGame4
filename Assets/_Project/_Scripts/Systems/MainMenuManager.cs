using DG.Tweening;
using Project.Assets._Project._Scripts.UI;
using Reflex.Attributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Project.Assets._Project._Scripts.Systems
{
    public class MainMenuManager : MonoBehaviour
    {
        [Inject] private readonly List<Button> _allButtons;
        [Inject] private readonly AudioData _audioData;
        [Inject] private readonly PlayerPrefsData _playerPrefsData;
        [Inject] private readonly IAudioService _audioService;
        [Inject] private readonly MainMenuUIElements _mainMenuUIElements;
        [SerializeField] private Sprite _musicTurnedOnSprite, _musicTurnedOffSprite;
        [SerializeField] private Sprite _soundTurnedOnSprite, _soundTurnedOffSprite;
        [SerializeField] private float _loadingCoverFullAppearDuration = 1f;
        [SerializeField] private Ease _loadingCoverAppearEase = Ease.OutCubic;
        [SerializeField] private Ease _loadingCoverDisappearEase = Ease.InCubic;
        [SerializeField] private Sprite _lockedLevelSprite;

        private Image _loadingCover;
        private bool _musicTurnedOn = true;
        private bool _soundTurnedOn = true;
        private AudioSource _musicSource;
        private void Start()
        {
            DOVirtual.DelayedCall(0.5f, SetDisplayTo60Hz);
            _loadingCover = _mainMenuUIElements.LoadingCover;
            MakeCoverDisappear();
            int lastUnlockedLevel = PlayerPrefs.GetInt(_playerPrefsData.LastUnlockedLevelName, 0);
            ActivateUnlockedLevelButtons(lastUnlockedLevel);
            _allButtons.ForEach(button => button.onClick.AddListener(() => _audioService.Play(_audioData.AnyButtonClip, _audioData.AnyButtonVolume)));
            _mainMenuUIElements.MusicButton.onClick.AddListener(ToggleMusicAndSave);
            _mainMenuUIElements.SoundButton.onClick.AddListener(ToggleSoundAndSave);
            var persistent = PersistentParent.Instance;
            if (persistent != null)
            {
                _musicSource = persistent.GetComponentInChildren<AudioSource>();
                if (_musicSource == null) Debug.LogError("Music Source was not found!");
            }
            CheckIfMusicNeedsToBeActive();
            CheckIfSoundNeedsToBeActive();
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

        private void CheckIfMusicNeedsToBeActive()
        {
            int musicTurnedOnPlPref = PlayerPrefs.GetInt(_playerPrefsData.MusicTurnedOn, 1);
            bool shouldMusicBeOn = musicTurnedOnPlPref > 0;
            if (shouldMusicBeOn && !_musicTurnedOn || !shouldMusicBeOn && _musicTurnedOn)
            {
                ToggleMusic();
            }
        }

        private void ToggleMusic()
        {
            if (_musicSource != null)
            {
                _musicTurnedOn = !_musicTurnedOn;
                if (_musicTurnedOn)
                {
                    _mainMenuUIElements.MusicButton.image.sprite = _musicTurnedOnSprite;
                    _musicSource.Play();
                }
                else
                {
                    _mainMenuUIElements.MusicButton.image.sprite = _musicTurnedOffSprite;
                    _musicSource.Stop();
                }
            }

        }
        private void ToggleMusicAndSave()
        {
            ToggleMusic();
            if (_musicTurnedOn)
            {
                PlayerPrefs.SetInt(_playerPrefsData.MusicTurnedOn, 1);
            }
            else
            {
                PlayerPrefs.SetInt(_playerPrefsData.MusicTurnedOn, 0);
            }
        }

        private void ToggleSoundAndSave()
        {
            ToggleSound();
            if (_soundTurnedOn)
            {
                PlayerPrefs.SetInt(_playerPrefsData.SoundTurnedOn, 1);
            }
            else
            {
                PlayerPrefs.SetInt(_playerPrefsData.SoundTurnedOn, 0);
            }
        }

        private void ToggleSound()
        {
            _soundTurnedOn = !_soundTurnedOn;
            if (!_soundTurnedOn)
            {
                _mainMenuUIElements.SoundButton.image.sprite = _soundTurnedOffSprite;
                _audioService.ToggleMute(true);
            }
            else
            {
                _mainMenuUIElements.SoundButton.image.sprite = _soundTurnedOnSprite;
                _audioService.ToggleMute(false);
                _audioService.Play(_audioData.AnyButtonClip, _audioData.AnyButtonVolume);
            }

        }

        private void OnApplicationFocus(bool hasFocus)
        {
            SetDisplayTo60Hz();
        }

        private void SetDisplayTo60Hz()
        {
            Application.targetFrameRate = 60;

        }
        public void StartButton()
        {
            LoadLevel(1);
        }

        public void LoadLevel(int levelIndex)
        {
            var color = _loadingCover.color;
            color.a = 1;
            _loadingCover.gameObject.SetActive(true);
            DOVirtual.Color(_loadingCover.color, color, _loadingCoverFullAppearDuration, c => _loadingCover.color = c)
                .SetEase(_loadingCoverAppearEase)
                .OnComplete(() => SceneManager.LoadScene(levelIndex));
            //EUtils.Scene.LoadSceneWithLoadingScreen(levelIndex, _loadingScene.Name, _loadingDuration).Forget();
        }

        public void MakeCoverDisappear()
        {
            var color = _loadingCover.color;
            color.a = 0;
            _loadingCover.gameObject.SetActive(true);
            DOVirtual.Color(_loadingCover.color, color, _loadingCoverFullAppearDuration, c => _loadingCover.color = c)
                .SetEase(_loadingCoverDisappearEase)
                .OnComplete(() => _loadingCover.gameObject.SetActive(false));
        }
        private void ActivateUnlockedLevelButtons(int lastUnlockedLevel)
        {
            foreach (var button in _allButtons)
            {
                if (button == null)
                    continue;

                var name = button.name ?? string.Empty;

                if (name.Length >= 2)
                {
                    string key = name.Substring(name.Length - 2);
                    bool parsed = int.TryParse(key, out int valueInt);
                    if (parsed)
                    {
                        if (valueInt == 1)
                        {
                            button.interactable = true;
                            continue;
                        }
                        bool isInteractable = valueInt <= lastUnlockedLevel;
                        if (isInteractable)
                        {
                            button.interactable = true;
                        }
                        else
                        {
                            button.image.sprite = _lockedLevelSprite;
                            button.interactable = false;
                        }
                    }
                    else
                    {
                        button.interactable = true;

                    }

                }
                else
                {
                    button.image.sprite = _lockedLevelSprite;
                    button.interactable = false;
                }
            }
        }
    }
}
