using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Assets._Project._Scripts.Systems
{
    public class HintManager : MonoBehaviour
    {
        [SerializeField] private int _startingHintAmount = 3;
        private int _currentHintAmount = 0;
        //private List<MovableBlock> _blocksAsideFromHinted;
        private bool _hasTurnedOffInteractionForNonHintedBlocks = false;
        public event Action<int> OnCurrentHintAmountChanged;
        private bool _isInHint = false;
        private void Start()
        {
            _currentHintAmount = _startingHintAmount;
            OnCurrentHintAmountChanged?.Invoke(_currentHintAmount < 0 ? 0 : _currentHintAmount);
        }
        public bool TryGetHint<T>(List<T> blocks, out Vector3 pos, bool isFree = false)
        {
            pos = Vector3.zero;
            if (_currentHintAmount <= 0 && !isFree || _isInHint) return false;
            _isInHint = true;
            if (!isFree) 
            {
                _currentHintAmount--;
                OnCurrentHintAmountChanged?.Invoke(_currentHintAmount < 0 ? 0 : _currentHintAmount);
            }
            //var hintedBlock = blocks.FirstOrDefault(block => !block.IsInBlockedMovement && !block.HasDisappeared && block.IsFreeToMoveForward());
            //pos = hintedBlock.transform.position;
            //_blocksAsideFromHinted = blocks.Where(block => block != hintedBlock).ToList();
            //_blocksAsideFromHinted.ForEach(block => block.ToggleInteractable(false));
            _hasTurnedOffInteractionForNonHintedBlocks = true;

            return true;

        }

        public void OnAnyBlockInteracted()
        {
            if (_hasTurnedOffInteractionForNonHintedBlocks)
            {
                //_blocksAsideFromHinted?.ForEach(block => block.ToggleInteractable(true));
                _hasTurnedOffInteractionForNonHintedBlocks = false;
                _isInHint = false;
            }

        }
    }
}
