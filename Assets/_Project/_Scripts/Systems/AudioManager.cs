using KBCore.Refs;
using UnityEngine;

namespace Project.Assets._Project._Scripts.Systems
{
    public class AudioManager : ValidatedMonoBehaviour, IAudioService
    {
        [SerializeField, Self] private AudioSource _source;
        private bool _isMuted = false;
        public void Play(AudioClip clip, float volume = 1, float pitch = 1, float spatialBlend = 0)
        {
            if (clip == null || _isMuted) return;
            _source.pitch = pitch;
            _source.spatialBlend = spatialBlend;
            _source.PlayOneShot(clip, volume);
        }

        public void Stop()
        {
            _source.Stop();
        }

        public void ToggleMute(bool isMuted)
        {
            _isMuted = isMuted;
            if (_isMuted) Stop();
        }
    }
}
