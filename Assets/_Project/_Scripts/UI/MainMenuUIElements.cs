using UnityEngine;
using UnityEngine.UI;

namespace Project.Assets._Project._Scripts.UI
{
    public class MainMenuUIElements : MonoBehaviour
    {
        [SerializeField] private RectTransform _mainButtonsLayout;
        [SerializeField] private RectTransform _levelButtonsLayout;
        [SerializeField] private Button _backButton;

        [field: SerializeField] public Button MusicButton { get; private set; }
        [field: SerializeField] public Button SoundButton { get; private set; }
        [field: SerializeField] public Image LoadingCover { get; private set; }


        private void Start()
        {
            _mainButtonsLayout?.gameObject.SetActive(true);
            _levelButtonsLayout?.gameObject.SetActive(false);
            _backButton?.gameObject.SetActive(false);
        }


    }
}
