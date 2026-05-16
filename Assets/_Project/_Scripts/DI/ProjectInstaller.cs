using Project.Assets._Project._Scripts.Systems;
using Reflex.Core;
using UnityEngine;

namespace Project.Assets._Project._Scripts.DI
{
    public class ProjectInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private PlayerPrefsData _playerPrefsData;
        [SerializeField] private AudioData _audioData;
        public void InstallBindings(ContainerBuilder builder)
        {
            builder.AddSingleton(_playerPrefsData);
            builder.AddSingleton(_audioData);
        }
    }
}
