using UnityEngine;
using UnityEngine.UI;

namespace Project.Assets._Project._Scripts.UI
{
    public class MainMenuUIElements : MonoBehaviour
    {
        [SerializeField] private RectTransform _mainButtonsLayout;
        [SerializeField] private RectTransform _levelButtonsLayout;
        [SerializeField] private Button _backButton;
        [SerializeField] private Image _levelsImage;
        [SerializeField] private Image _settingsImage;
        [SerializeField] private Image _settingsButtonsLayout;
        [SerializeField] private Button _settingsBackButton;

        [field: SerializeField] public Button MusicButton { get; private set; }
        [field: SerializeField] public Button SoundButton { get; private set; }
        [field: SerializeField] public Image LoadingCover { get; private set; }


        private void Start()
        {
            _mainButtonsLayout?.gameObject.SetActive(true);
            _levelButtonsLayout?.gameObject.SetActive(false);
            _backButton?.gameObject.SetActive(false);
            _levelsImage?.gameObject.SetActive(false);
            _settingsImage?.gameObject.SetActive(false);
            _settingsButtonsLayout?.gameObject.SetActive(false);
            _settingsBackButton?.gameObject.SetActive(false);
        }


    }
}
