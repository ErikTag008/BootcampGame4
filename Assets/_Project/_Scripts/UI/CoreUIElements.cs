using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Assets._Project._Scripts.UI
{
    public class CoreUIElements : MonoBehaviour
    {
        [Header("Lose Screen Elements")]
        [SerializeField] private Image _loseScreen;
        [SerializeField] private float _loseScreenAppearDuration = 1f;
        [SerializeField] private Ease _loseScreenAppearEase = Ease.InOutCubic;
        [field: SerializeField] public Button LoseReloadButton { get; private set; }
        [field: SerializeField] public Button LoseMenuButton { get; private set; }

        private Vector3 _loseScreenTargetScale;

        [Header("Win Screen Elements")]
        [SerializeField] private Image _winScreen;
        [SerializeField] private float _winScreenAppearDuration = 1f;
        [SerializeField] private Ease _winScreenAppearEase = Ease.InOutCubic;
        [field: SerializeField] public Button WinReloadButton { get; private set; }
        [field: SerializeField] public Button WinContinueButton { get; private set; }
        [field: SerializeField] public Button WinMenuButton { get; private set; }
        private Vector3 _winScreenTargetScale;

        [Header("Pause Screen Elements")]
        [SerializeField] private Image _pauseScreen;
        [SerializeField] private float _pauseScreenAnimationDuration = 1f;
        [SerializeField] private Ease _pauseScreenAnimationEase = Ease.InOutCubic;
        [field: SerializeField] public Button PauseReloadButton { get; private set; }
        [field: SerializeField] public Button PauseReturnButton { get; private set; }
        [field: SerializeField] public Button PauseMenuButton { get; private set; }
        private Vector3 _pauseScreenTargetScale;

        [Header("Loading")]
        [field: SerializeField] public Image LoadingCover { get; private set; }


        private void Start()
        {
            if(_loseScreen != null)
            {
                _loseScreen.gameObject.SetActive(false);
                _loseScreenTargetScale = _loseScreen.rectTransform.localScale;
            }
            if(_winScreen != null)
            {
                _winScreen.gameObject.SetActive(false);
                _winScreenTargetScale = _winScreen.rectTransform.localScale;
            }
            if (_pauseScreen != null)
            {
                _pauseScreen.gameObject.SetActive(false);
                _pauseScreenTargetScale = _pauseScreen.rectTransform.localScale;
            }

        }

        [ContextMenu("Show lose screen")]
        public Tween ShowLoseScreen()
        {
            _loseScreen.gameObject.SetActive(true);
            return _loseScreen.rectTransform.DOScale(_loseScreenTargetScale, _loseScreenAppearDuration)
                        .From(Vector3.zero)
                        .SetEase(_loseScreenAppearEase);
        }



        [ContextMenu("Show win screen")]
        public Tween ShowWinScreen()
        {
            _winScreen.gameObject.SetActive(true);
            return _winScreen.rectTransform.DOScale(_winScreenTargetScale, _winScreenAppearDuration)
                .From(Vector3.zero)
                .SetEase(_winScreenAppearEase);
        }

        public Tween ShowPauseScreen()
        {
            _pauseScreen.gameObject.SetActive(true);
            return _pauseScreen.rectTransform.DOScale(_pauseScreenTargetScale, _pauseScreenAnimationDuration)
                .From(Vector3.zero)
                .SetEase(_pauseScreenAnimationEase)
                .SetUpdate(true);
        }

        public void HidePauseScreen()
        {
            _pauseScreen.gameObject.SetActive(false);
            //return _pauseScreen.rectTransform.DOScale(Vector3.zero, _pauseScreenAnimationDuration)
            //    .SetEase(_pauseScreenAnimationEase)
            //    .SetUpdate(true)
            //    .OnComplete(() => _pauseScreen.gameObject.SetActive(false));
        }

    }
}
