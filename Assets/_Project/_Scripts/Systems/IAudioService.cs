using UnityEngine;

namespace Project.Assets._Project._Scripts.Systems
{
    public interface IAudioService
    {
        void Play(AudioClip clip, float volume = 1f, float pitch = 1f, float spatialBlend = 0f);
        void Stop();
        void ToggleMute(bool isMuted);
    }
}
