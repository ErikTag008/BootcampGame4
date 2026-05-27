using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Assets._Project._Scripts.Systems
{
    public class HintManager : MonoBehaviour
    {
        private bool _hasTurnedOffInteractionForNonHintedBlocks = false;
        private bool _isInHint = false;
        private void Start()
        {
        }
        public bool TryGetHint<T>(List<T> blocks, out Vector3 pos, bool isFree = false)
        {
            pos = Vector3.zero;
            _isInHint = true;
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
