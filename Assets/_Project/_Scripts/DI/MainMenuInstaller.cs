using KBCore.Refs;
using Project.Assets._Project._Scripts.Systems;
using Project.Assets._Project._Scripts.UI;
using Reflex.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Assets._Project._Scripts.DI
{
    public class MainMenuInstaller : ValidatedMonoBehaviour, IInstaller
    {
        [SerializeField, Scene] private AudioManager _audioManager;
        [SerializeField, Scene] private List<Button> _allButtons;
        [SerializeField, Scene] private MainMenuUIElements _menuUIElements;
 
        public void InstallBindings(ContainerBuilder builder)
        {
            builder.AddSingleton(_audioManager, typeof(IAudioService));
            builder.AddSingleton(_allButtons);
            builder.AddSingleton(_menuUIElements);
        }
    }
}
