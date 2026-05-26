using UnityEngine;

namespace Project.Assets._Project._Scripts.Systems
{
    [CreateAssetMenu(fileName = "AudioData", menuName = "ScriptableObjects/AudioData",order = 2)]
    public class AudioData : ScriptableObject
    {
        [field: SerializeField] public AudioClip WrongInteractionClip { get; private set; }
        [field: SerializeField] public float WrongInteractionVolume { get; private set; }
        [field: Space(2)]
        [field: SerializeField] public AudioClip CorrectInteractionClip { get; private set; }
        [field: SerializeField] public float CorrectInteractionVolume { get; private set; }

        [field: Space(2)]
        [field: SerializeField] public AudioClip BlockInteractionClip { get; private set; }
        [field: SerializeField] public float BlockInteractionVolume { get; private set; }
        [field: Space(2)]
        [field: SerializeField] public AudioClip LevelWinClip { get; private set; }
        [field: SerializeField] public float LevelWinVolume { get; private set; }

        [field: Space(2)]
        [field: SerializeField] public AudioClip LevelLoseClip { get; private set; }
        [field: SerializeField] public float LevelLoseVolume { get; private set; }
        [field: Space(2)]
        [field: SerializeField] public AudioClip AnyButtonClip { get; private set; }
        [field: SerializeField] public float AnyButtonVolume { get; private set; }

    }
}
