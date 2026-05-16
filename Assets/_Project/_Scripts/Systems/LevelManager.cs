using DG.Tweening;
using Eflatun.SceneReference;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Assets._Project._Scripts.Systems
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private SceneReference _mainMenuScene;
        [Inject] private readonly UIManager _uiManager;
        [Inject] private readonly PlayerPrefsData _playerPrefsData;
        [SerializeField] private float _loadingCoverFullAppearDuration = 1f;
        [SerializeField] private Ease _loadingCoverAppearEase = Ease.OutCubic;
        [SerializeField] private Ease _loadingCoverDisappearEase = Ease.InCubic;

        public Tween MakeCoverDisappear()
        {
            var cover = _uiManager.GetLoadingCover();
            var color = cover.color;
            color.a = 0;
            cover.gameObject.SetActive(true);
            return DOVirtual.Color(cover.color, color, _loadingCoverFullAppearDuration, c => cover.color = c)
                .SetEase(_loadingCoverDisappearEase)
                .OnStepComplete(() => cover.gameObject.SetActive(false));
        }

        public void UnlockNextLevel()
        {
            int nextScene = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextScene >= SceneManager.sceneCountInBuildSettings) return;
            var lastUnlocked = PlayerPrefs.GetInt(_playerPrefsData.LastUnlockedLevelName, 0);
            if (nextScene < lastUnlocked) return;
            PlayerPrefs.SetInt(_playerPrefsData.LastUnlockedLevelName, nextScene);
        }
        public void ReturnToMenu()
        {
            LoadLevel(_mainMenuScene.BuildIndex);
        }

        public void ContinueToNextLevel()
        {
            int nextScene = SceneManager.GetActiveScene().buildIndex + 1;
            if(nextScene >= SceneManager.sceneCountInBuildSettings) 
            {
                Debug.LogWarning("Next Scene Is Not in Build Settings or This is the last level");
                return; 
            }
            LoadLevel(nextScene);
        }

        public void ReloadLevel()
        {
            LoadLevel(SceneManager.GetActiveScene().buildIndex);
        }

        public void LoadLevel(int levelIndex)
        {
            Time.timeScale = 1;
            var cover = _uiManager.GetLoadingCover();
            var color = cover.color;
            color.a = 1;
            cover.gameObject.SetActive(true);
            DOVirtual.Color(cover.color, color, _loadingCoverFullAppearDuration, c => cover.color = c)
                .SetEase(_loadingCoverAppearEase)
                .OnComplete(() => SceneManager.LoadScene(levelIndex));
            //EUtils.Scene.LoadSceneWithLoadingScreen(levelIndex, _loadingScene.Name, _loadingDuration).Forget();
        }
    }
}
