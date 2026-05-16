using UnityEngine;

namespace Project.Assets._Project._Scripts.Systems
{
    [CreateAssetMenu(fileName = "PlayerPrefsData", menuName = "ScriptableObjects/PlayerPrefsData", order = 1)]
    public class PlayerPrefsData : ScriptableObject
    {
        [field: SerializeField] public string LastUnlockedLevelName { get; private set; } = "LastUnlockedLevel";
        [field: SerializeField] public string MusicTurnedOn { get; private set; } = "MusicTurnedOn";
        [field: SerializeField] public string SoundTurnedOn { get; private set; } = "SoundTurnedOn";
    }
}
