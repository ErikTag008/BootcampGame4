using Gilzoide.UpdateManager;
using Reflex.Attributes;
using System;
using UnityEngine;

namespace Project.Assets._Project._Scripts.Systems
{
    public class TimerManager : AManagedBehaviour, IUpdatable
    {
        [SerializeField] private int _timerStartingValueInSeconds = 60;
        [Inject] private readonly UIManager _uiManager;
        private float _timerCurrentValueInSeconds;
        private bool _canUpdateTimer = false;
        public event Action OnTimeEnded;
        
        
        private void Start()
        {
            _timerCurrentValueInSeconds = _timerStartingValueInSeconds;
            string time = FormatCurrentTime();
            _uiManager.ChangeTimerValue(time);
        }

        public void StopTimer()
        {
            _canUpdateTimer = false;
        }

        public void StartTimer()
        {
            _canUpdateTimer = true;
        }
        public void ManagedUpdate()
        {
            if (!_canUpdateTimer) return;
            _timerCurrentValueInSeconds -= Time.deltaTime;
            string time = FormatCurrentTime();
            _uiManager.ChangeTimerValue(time);
        }
        public void AddTime(int seconds)
        {
            _timerCurrentValueInSeconds += seconds;
            string time = FormatCurrentTime();
            _uiManager.ChangeTimerValue(time);
        }

        private string FormatCurrentTime()
        {
            int minutes = (int)(_timerCurrentValueInSeconds / 60);
            int seconds = (int)(_timerCurrentValueInSeconds % 60);
            if(minutes <= 0 && seconds <= 0)
            {
                OnTimeEnded?.Invoke();
                _canUpdateTimer = false;
            }
            string time = $"{minutes:D2}:{seconds:D2}";
            return time;
        }
    }
}
