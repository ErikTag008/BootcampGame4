using Project.Assets._Project._Scripts.Interactables;
using Reflex.Attributes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Assets._Project._Scripts.Systems
{
    public class MergeManager : MonoBehaviour
    {
        [Inject] private readonly List<Block> _blocks;
        [SerializeField] private List<Block> _mergedBlockPrefabs;

        private void Start()
        {
            _blocks.ForEach(block => block.OnMerge += HandleMerge);
        }
        private void OnDestroy()
        {
            _blocks.ForEach(block => block.OnMerge -= HandleMerge);
        }
        private void HandleMerge(UnitColor color, BlockType type, Vector3 pos, Quaternion rotation)
        {
            foreach(var  blockPrefab in _mergedBlockPrefabs)
            {
                if(blockPrefab.Type == type && blockPrefab.Color == color)
                {
                    var mergedBlock = Instantiate(blockPrefab, pos, rotation);
                    break;
                }
            }
        }
    }
}
