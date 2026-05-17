using KBCore.Refs;
using Project.Assets._Project._Scripts.GridComponents;
using Project.Assets._Project._Scripts.Interactables;
using Project.Assets._Project._Scripts.Systems;
using Project.Assets._Project._Scripts.UI;
using Reflex.Core;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Assets._Project._Scripts.DI
{
    public class SceneInstaller : ValidatedMonoBehaviour, IInstaller
    {
        [Header("Managers")]
        [SerializeField, Scene] private GameManager _gameManager;
        [SerializeField, Scene] private UIManager _uiManager;
        [SerializeField, Scene] private InputReader _inputReader;
        [SerializeField, Scene] private InputManager _inputManager;
        [SerializeField, Scene] private LevelManager _levelManager;
        [SerializeField, Scene] private HintManager _hintManager;
        [SerializeField, Scene] private AudioManager _audioManager;
        [SerializeField, Scene] private GridGenerator _gridGenerator;
        [Header("Camera")]
        [SerializeField, Scene] private Camera _camera;
        [SerializeField, Scene] private CinemachineCamera _cinemachineCam;
        [SerializeField, Scene] private CinemachineBasicMultiChannelPerlin _cinemachineShakeComponent;
        [Header("UI Elements")]
        [SerializeField, Scene] private CoreUIElements _coreUIElements;
        [SerializeField, Scene] private GameUIElements _gameUIElements;
        [SerializeField, Scene] private List<Button> _allButtons;
        [Header("Game Objects")]
        [SerializeField, Scene] private List<Block> _blocks;
        
        
        

        public void InstallBindings(ContainerBuilder builder)
        {
            builder.AddSingleton(_gameManager);
            builder.AddSingleton(_camera);
            builder.AddSingleton(_cinemachineCam);
            builder.AddSingleton(_cinemachineShakeComponent);
            builder.AddSingleton(_inputReader);
            builder.AddSingleton(_inputManager);
            builder.AddSingleton(_levelManager);
            builder.AddSingleton(_hintManager);
            builder.AddSingleton(_uiManager);
            builder.AddSingleton(_gridGenerator);
            builder.AddSingleton(_audioManager, typeof(IAudioService));
            builder.AddSingleton(_coreUIElements);
            builder.AddSingleton(_gameUIElements);
            builder.AddSingleton(_allButtons);
            builder.AddSingleton(_levelManager);
            builder.AddSingleton(_blocks);
            
            
        }

    }
}
